using System.Linq.Expressions;

namespace System.Data.Mapper.Expressions;

internal sealed class DbExpressionValidator( ParameterExpression entityParameter ) : ExpressionVisitor
{
    private static readonly HashSet<ExpressionType> SupportedBinaryNodes =
    [
        ExpressionType.Equal,
        ExpressionType.NotEqual,
        ExpressionType.GreaterThan,
        ExpressionType.GreaterThanOrEqual,
        ExpressionType.LessThan,
        ExpressionType.LessThanOrEqual,
        ExpressionType.AndAlso,
        ExpressionType.OrElse
    ];

    public static void Validate<T>( Expression<Func<T, bool>> expression )
    {
        var validator = new DbExpressionValidator( expression.Parameters[0] );
        validator.ValidatePredicate( expression.Body );
        validator.Visit( expression.Body );
    }

    private void ValidatePredicate( Expression expression )
    {
        expression = StripConvert( expression );

        if ( expression is MethodCallExpression && UsesEntityParameter( expression ) )
        {
            throw Unsupported( expression, "method calls that reference the entity are not supported" );
        }

        if ( expression is not BinaryExpression binary || !SupportedBinaryNodes.Contains( binary.NodeType ) )
        {
            throw Unsupported( expression, "predicates other than comparisons joined with '&&' or '||' are not supported" );
        }

        if ( binary.NodeType is ExpressionType.AndAlso or ExpressionType.OrElse )
        {
            ValidatePredicate( binary.Left );
            ValidatePredicate( binary.Right );
        }
    }

    protected override Expression VisitBinary( BinaryExpression node )
    {
        if ( !SupportedBinaryNodes.Contains( node.NodeType ) )
        {
            throw Unsupported( node, $"binary operator '{node.NodeType}' is not supported" );
        }

        if ( node.NodeType is not ExpressionType.AndAlso and not ExpressionType.OrElse )
        {
            var leftUsesEntity = UsesEntityParameter( node.Left );
            var rightUsesEntity = UsesEntityParameter( node.Right );

            if ( leftUsesEntity && rightUsesEntity )
            {
                throw Unsupported( node, "property-to-property comparisons are not supported" );
            }

            if ( rightUsesEntity && !IsEntityProperty( node.Right ) )
            {
                throw Unsupported( node.Right, "computed entity values are not supported" );
            }

            if ( rightUsesEntity )
            {
                throw Unsupported( node, "entity properties on the right side of comparisons are not supported" );
            }

            if ( leftUsesEntity && !IsEntityProperty( node.Left ) )
            {
                throw Unsupported( node.Left, "computed entity values are not supported" );
            }

            if ( !leftUsesEntity && !IsCapturedField( node.Left ) )
            {
                throw Unsupported( node.Left, "comparison values on the left side are not supported unless they are captured variables" );
            }

            if ( !rightUsesEntity && !CanEvaluate( node.Right ) )
            {
                throw Unsupported( node.Right, "values that cannot be evaluated before executing the command are not supported" );
            }
        }

        return base.VisitBinary( node );
    }

    protected override Expression VisitMethodCall( MethodCallExpression node )
    {
        if ( UsesEntityParameter( node ) )
        {
            throw Unsupported( node, "method calls that reference the entity are not supported" );
        }

        return node;
    }

    protected override Expression VisitUnary( UnaryExpression node )
    {
        if ( node.NodeType is ExpressionType.Convert or ExpressionType.ConvertChecked )
        {
            return base.VisitUnary( node );
        }

        throw Unsupported( node, $"unary operator '{node.NodeType}' is not supported" );
    }

    protected override Expression VisitMember( MemberExpression node )
    {
        if ( UsesEntityParameter( node ) && !IsEntityProperty( node ) )
        {
            throw Unsupported( node, "nested entity member access is not supported" );
        }

        if ( IsEntityProperty( node ) )
        {
            return node;
        }

        return base.VisitMember( node );
    }

    protected override Expression VisitParameter( ParameterExpression node )
    {
        if ( node == entityParameter )
        {
            throw Unsupported( node, "a bare entity parameter is not supported" );
        }

        return base.VisitParameter( node );
    }

    private bool IsEntityProperty( Expression expression )
    {
        expression = StripConvert( expression );

        return expression is MemberExpression
            {
                Member: System.Reflection.PropertyInfo,
                Expression: ParameterExpression parameter
            }
            && parameter == entityParameter;
    }

    private static bool IsCapturedField( Expression expression )
    {
        expression = StripConvert( expression );

        return expression is MemberExpression
        {
            Member: System.Reflection.FieldInfo,
            Expression: ConstantExpression
        }
        || expression is MemberExpression
        {
            Member: System.Reflection.FieldInfo { IsStatic: true },
            Expression: null
        };
    }

    private static bool CanEvaluate( Expression expression )
    {
        expression = StripConvert( expression );

        return expression switch
        {
            ConstantExpression => true,
            MemberExpression => IsCapturedField( expression ),
            NewExpression value when value.Type.IsValueType && value.Arguments.Count == 0 => true,
            MethodCallExpression method =>
                ( method.Method.IsStatic || method.Object is not null && CanEvaluate( method.Object ) )
                && method.Arguments.All( CanEvaluate ),
            _ => false
        };
    }

    private bool UsesEntityParameter( Expression expression )
    {
        var finder = new EntityParameterFinder( entityParameter );
        finder.Visit( expression );
        return finder.Found;
    }

    private static Expression StripConvert( Expression expression )
    {
        while ( expression is UnaryExpression unary
            && unary.NodeType is ExpressionType.Convert or ExpressionType.ConvertChecked )
        {
            expression = unary.Operand;
        }

        return expression;
    }

    private static NotSupportedException Unsupported( Expression expression, string feature )
        => new( $"The predicate expression '{expression}' is not supported because {feature}." );

    private sealed class EntityParameterFinder( ParameterExpression parameter ) : ExpressionVisitor
    {
        public bool Found { get; private set; }

        protected override Expression VisitParameter( ParameterExpression node )
        {
            if ( node == parameter )
            {
                Found = true;
            }

            return node;
        }
    }
}
