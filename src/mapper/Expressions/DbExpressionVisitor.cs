using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace System.Data.Mapper.Expressions;

internal sealed class DbExpressionVisitor : ExpressionVisitor
{
    private readonly StringBuilder whereClause = new();
    private readonly Dictionary<string, object> parameters = [];
    private readonly Dictionary<string, int> parametersIndex = [];
    internal static readonly char[] separator = [' ', '=', '<', '>', '!', '(', ')'];

    private string? lastColumnName = null;
    private PropertyMetadata? lastProperty = null;

    private DbExpressionVisitor( EntityMetadata entityMetadata )
    {
        EntityMetadata = entityMetadata;
    }

    private EntityMetadata EntityMetadata { get; }

    public static DbWhereClause GetWhereClause<T>( Expression<Func<T, bool>> expression )
    {
        var visitor = new DbExpressionVisitor(
            EntityCache.GetEntityInfo<T>()
        );

        visitor.Visit( visitor.EvaluateExpression( expression.Body ) );

        var whereClause = visitor.whereClause.ToString();

        // remove the outermost parentheses if they exist
        if ( whereClause.StartsWith( '(' ) && whereClause.EndsWith( ')' ) )
        {
            whereClause = whereClause[1..^1];
        }

        return new DbWhereClause
        {
            WhereClause = whereClause,
            Parameters = new ReadOnlyDictionary<string, object>( visitor.parameters )
        };
    }

    /// <summary>
    /// Keeps track of the parameter index for each parameter name to avoid conflicts.
    /// </summary>
    /// <param name="name">The parameter name</param>
    /// <returns>The parameter name with an index appended (if necessary)</returns>
    private string GetParameterName( string name )
    {
        if ( !parametersIndex.TryGetValue( name, out int index ) )
        {
            index = 0;
        }
        else
        {
            index++;
        }

        parametersIndex[name] = index;

        return index > 0
            ? $"{name}_{index}"
            : name;
    }

    protected override Expression VisitBinary( BinaryExpression node )
    {
        bool isLogicalOperator = node.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.OrElse;
        bool needsParentheses = isLogicalOperator && ( node.Left is BinaryExpression || node.Right is BinaryExpression );
        
        if ( needsParentheses )
        {
            whereClause.Append( '(' );
        }
        
        Visit( node.Left );
        
        Expression right = EvaluateExpression( node.Right );
        if ( right is ConstantExpression constantExpr && constantExpr.Value is null )
        {
            var memberName = node.Right is MemberExpression memberExpr ? memberExpr.Member.Name : null;
            if ( memberName is not null && lastProperty?.PropertyName.Equals( memberName, StringComparison.OrdinalIgnoreCase ) == true )
            {
                var paramName = $"p_{EntityMetadata.NamingPolicy.ConvertName( memberName )}";
                whereClause.Append( GetSqlOperator( node.NodeType ) )
                    .Append( '@' )
                    .Append( paramName );
            }
            else
            {
                whereClause.Append(" IS NULL");
            }
        }
        else
        {
            whereClause.Append( GetSqlOperator( node.NodeType ) );

            if ( right is ConstantExpression valueExpr )
            {
                //string columnName = ExtractLastColumnName();
                string columnName = lastColumnName ?? throw new InvalidOperationException( "Failed to determine column name for parameter binding." );
                string paramName = GetParameterName( $"p_{columnName}" );
                var value = valueExpr.Value;

                if ( lastProperty?.DbTypeConverter is {} && value is not DbParameterValue )
                {
                    value = new DbParameterValue
                    {
                        Converter = lastProperty.DbTypeConverter,
                        Value = value
                    };
                }

                parameters[paramName] = value ?? DBNull.Value;

                whereClause.Append( '@' )
                    .Append(paramName);
            }
            else
            {
                Visit(right);
            }
        }
        
        if ( needsParentheses )
        {
            whereClause.Append( ')' );
        }

        return node;
    }

    protected override Expression VisitMember( MemberExpression node )
    {
        if ( node.Member is PropertyInfo )
        {
            lastProperty = GetPropertyMetadata( node.Member );
            lastColumnName = lastProperty.ColumnName;

            whereClause.Append( lastColumnName );
        }
        else if ( node.Member is FieldInfo fieldInfo )
        {
            // parameter index is ignored to allow later reference to the same parameter
            var paramName = $"p_{EntityMetadata.NamingPolicy.ConvertName( node.Member.Name )}";
            var value = fieldInfo.GetValue( ( node.Expression as ConstantExpression )?.Value ?? node );

            parameters[paramName] = value ?? DBNull.Value;

            whereClause.Append( '@' )
                .Append( paramName );
        }
        else
        {
            throw new NotSupportedException($"Unsupported member access: {node.Member.Name}");
        }

        return node;
    }

    protected override Expression VisitConstant( ConstantExpression node )
    {
        //string columnName = ExtractLastColumnName();
        string columnName = lastColumnName ?? throw new InvalidOperationException( "Failed to determine column name for parameter binding." );
        string paramName = GetParameterName( $"p_{columnName}" );
        
        parameters[paramName] = node.Value ?? DBNull.Value;

        whereClause.Append( '@' )
            .Append( paramName );

        return node;
    }

    private Expression EvaluateExpression( Expression expression )
    {
        if ( expression.NodeType == ExpressionType.Convert )
        {
            expression = ((UnaryExpression)expression).Operand;
        }

        if ( expression is MemberExpression memberExpression && memberExpression.Expression is ConstantExpression constantExpression )
        {
            object? container = constantExpression.Value;
            object? value = ((FieldInfo)memberExpression.Member).GetValue( container );

            if ( lastProperty?.PropertyName.Equals( memberExpression.Member.Name, StringComparison.OrdinalIgnoreCase ) == true && lastProperty.DbTypeConverter is {} )
            {
                value = new DbParameterValue
                {
                    Converter = lastProperty.DbTypeConverter,
                    Value = value
                };
            }

            return Expression.Constant( value );
        }

        if ( expression is MethodCallExpression methodCall )
        {
            object? instance = null;
            if ( methodCall.Object != null )
            {
                Expression evaluatedInstance = EvaluateExpression( methodCall.Object );
                if ( evaluatedInstance is ConstantExpression constInstance )
                {
                    instance = constInstance.Value;
                }
                else if ( methodCall.Object is NewExpression newExpr )
                {
                    instance = Activator.CreateInstance(
                        newExpr.Type,
                        newExpr.Arguments.Select( arg => ((ConstantExpression)EvaluateExpression(arg)).Value ).ToArray()
                    );
                }
            }
            
            object?[] arguments = methodCall.Arguments.Select( arg =>
            {
                var evaluatedArg = EvaluateExpression( arg );

                return ((ConstantExpression)evaluatedArg).Value;
            } )
            .ToArray();
            
            if ( instance == null && !methodCall.Method.IsStatic )
            {
                throw new TargetException( $"Cannot invoke non-static method {methodCall.Method.Name} without an instance." );
            }
            
            object? result = methodCall.Method.Invoke( instance, arguments );

            return Expression.Constant( result );
        }

        if ( expression is NewExpression newStructExpr && newStructExpr.Type.IsValueType )
        {
            object? structInstance = Activator.CreateInstance( newStructExpr.Type );

            return Expression.Constant( structInstance );
        }
        
        return expression;
    }
    
    private static PropertyMetadata GetPropertyMetadata( MemberInfo member )
    {
        if ( !EntityCache.GetEntityInfo( member.DeclaringType! ).Properties.TryGetProperty( member.Name, out var property ) )
        {
            throw new InvalidOperationException( $"Failed to determine property info for member: {member.Name}" );
        }

        return property;
    }

    private static string GetSqlOperator( ExpressionType nodeType ) => nodeType switch
    {
        ExpressionType.Equal => " = ",
        ExpressionType.NotEqual => " <> ",
        ExpressionType.GreaterThan => " > ",
        ExpressionType.GreaterThanOrEqual => " >= ",
        ExpressionType.LessThan => " < ",
        ExpressionType.LessThanOrEqual => " <= ",
        ExpressionType.AndAlso => " AND ",
        ExpressionType.OrElse => " OR ",
        _ => throw new NotSupportedException( $"Unsupported operation: {nodeType}" )
    };
}
