using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Mapper.Materialization;

internal static class MaterializerBuilder
{
    private static readonly MethodInfo IsDbNullMethod = GetReaderMethod(
        nameof( DbDataReader.IsDBNull ),
        typeof( int )
    );
    private static readonly MethodInfo GetValueMethod = GetReaderMethod(
        nameof( DbDataReader.GetValue ),
        typeof( int )
    );
    private static readonly MethodInfo GetFieldValueMethod = typeof( DbDataReader )
        .GetMethods()
        .Single( method => method.Name == nameof( DbDataReader.GetFieldValue )
            && method.IsGenericMethodDefinition
            && method.GetParameters() is [{ ParameterType: var type }] && type == typeof( int ) );
    private static readonly MethodInfo ConvertValueMethod = typeof( DbValueConverter )
        .GetMethod( nameof( DbValueConverter.Convert ) )!;
    private static readonly MethodInfo ConverterReadMethod = typeof( DbTypeConverter )
        .GetMethod( nameof( DbTypeConverter.Read ) )!;

    private static readonly IReadOnlyDictionary<Type, MethodInfo> TypedGetters = new Dictionary<Type, MethodInfo>
    {
        [typeof( bool )] = GetReaderMethod( nameof( DbDataReader.GetBoolean ), typeof( int ) ),
        [typeof( byte )] = GetReaderMethod( nameof( DbDataReader.GetByte ), typeof( int ) ),
        [typeof( char )] = GetReaderMethod( nameof( DbDataReader.GetChar ), typeof( int ) ),
        [typeof( DateTime )] = GetReaderMethod( nameof( DbDataReader.GetDateTime ), typeof( int ) ),
        [typeof( decimal )] = GetReaderMethod( nameof( DbDataReader.GetDecimal ), typeof( int ) ),
        [typeof( double )] = GetReaderMethod( nameof( DbDataReader.GetDouble ), typeof( int ) ),
        [typeof( float )] = GetReaderMethod( nameof( DbDataReader.GetFloat ), typeof( int ) ),
        [typeof( Guid )] = GetReaderMethod( nameof( DbDataReader.GetGuid ), typeof( int ) ),
        [typeof( short )] = GetReaderMethod( nameof( DbDataReader.GetInt16 ), typeof( int ) ),
        [typeof( int )] = GetReaderMethod( nameof( DbDataReader.GetInt32 ), typeof( int ) ),
        [typeof( long )] = GetReaderMethod( nameof( DbDataReader.GetInt64 ), typeof( int ) ),
        [typeof( string )] = GetReaderMethod( nameof( DbDataReader.GetString ), typeof( int ) )
    };

    public static Func<DbDataReader, T> Build<T>(
        MaterializerCache.ReaderSchema schema,
        MaterializerReadStrategy strategy
    ) where T : notnull, new()
    {
        var metadata = EntityCache.GetEntityInfo<T>();
        var readerParameter = Expression.Parameter( typeof( DbDataReader ), "reader" );
        var entityVariable = Expression.Variable( typeof( T ), "entity" );
        var expressions = new List<Expression>
        {
            Expression.Assign( entityVariable, Expression.New( typeof( T ) ) )
        };

        foreach ( var column in schema.Columns )
        {
            if ( !metadata.Properties.TryGetColumn( column.Name, out var property )
                || !property.CanWrite )
            {
                continue;
            }

            var ordinalExpression = Expression.Constant( column.Ordinal );
            var readExpression = CreateReadExpression(
                readerParameter,
                ordinalExpression,
                column.FieldType,
                property,
                strategy
            );

            expressions.Add(
                Expression.IfThen(
                    Expression.Not( Expression.Call( readerParameter, IsDbNullMethod, ordinalExpression ) ),
                    Expression.Assign(
                        Expression.Property( entityVariable, property.PropertyInfo ),
                        readExpression
                    )
                )
            );
        }

        expressions.Add( entityVariable );

        return Expression.Lambda<Func<DbDataReader, T>>(
            Expression.Block( [entityVariable], expressions ),
            readerParameter
        ).Compile();
    }

    private static Expression CreateReadExpression(
        ParameterExpression reader,
        ConstantExpression ordinal,
        Type fieldType,
        PropertyMetadata property,
        MaterializerReadStrategy strategy
    )
    {
        if ( property.DbTypeConverter is not null )
        {
            var converter = EntityCache.GetDbTypeConverter( property.DbTypeConverter );
            var converted = Expression.Call(
                Expression.Constant( converter ),
                ConverterReadMethod,
                reader,
                ordinal,
                Expression.Constant( property.PropertyType, typeof( Type ) )
            );

            return Expression.Convert( converted, property.PropertyType );
        }

        var targetType = Nullable.GetUnderlyingType( property.PropertyType ) ?? property.PropertyType;
        var providerValue = CreateProviderRead( reader, ordinal, fieldType, strategy );
        Expression value;

        if ( targetType.IsAssignableFrom( fieldType ) )
        {
            value = fieldType == targetType
                ? providerValue
                : Expression.Convert( providerValue, targetType );
        }
        else if ( IsNumeric( fieldType ) && IsNumeric( targetType ) )
        {
            value = Expression.ConvertChecked( providerValue, targetType );
        }
        else if ( targetType == typeof( bool ) && IsNumeric( fieldType ) )
        {
            value = Expression.NotEqual( providerValue, Expression.Default( fieldType ) );
        }
        else if ( targetType.IsEnum && IsNumeric( fieldType ) )
        {
            var underlyingType = Enum.GetUnderlyingType( targetType );
            value = Expression.Convert(
                Expression.ConvertChecked( providerValue, underlyingType ),
                targetType
            );
        }
        else
        {
            value = Expression.Convert(
                Expression.Call(
                    ConvertValueMethod,
                    Expression.Convert( providerValue, typeof( object ) ),
                    Expression.Constant( targetType, typeof( Type ) )
                ),
                targetType
            );
        }

        return property.PropertyType == targetType
            ? value
            : Expression.Convert( value, property.PropertyType );
    }

    private static Expression CreateProviderRead(
        ParameterExpression reader,
        ConstantExpression ordinal,
        Type fieldType,
        MaterializerReadStrategy strategy
    )
    {
        if ( strategy is MaterializerReadStrategy.Hybrid or MaterializerReadStrategy.TypedGetters
            && TypedGetters.TryGetValue( fieldType, out var getter ) )
        {
            return Expression.Call( reader, getter, ordinal );
        }

        if ( strategy is MaterializerReadStrategy.Hybrid or MaterializerReadStrategy.GetFieldValue )
        {
            return Expression.Call( reader, GetFieldValueMethod.MakeGenericMethod( fieldType ), ordinal );
        }

        return Expression.Convert( Expression.Call( reader, GetValueMethod, ordinal ), fieldType );
    }

    private static bool IsNumeric( Type type )
        => Type.GetTypeCode( type ) is TypeCode.Byte
            or TypeCode.SByte
            or TypeCode.Int16
            or TypeCode.UInt16
            or TypeCode.Int32
            or TypeCode.UInt32
            or TypeCode.Int64
            or TypeCode.UInt64
            or TypeCode.Single
            or TypeCode.Double
            or TypeCode.Decimal;

    private static MethodInfo GetReaderMethod( string name, params Type[] parameterTypes )
        => typeof( DbDataReader ).GetMethod( name, parameterTypes )
            ?? throw new MissingMethodException( typeof( DbDataReader ).FullName, name );
}
