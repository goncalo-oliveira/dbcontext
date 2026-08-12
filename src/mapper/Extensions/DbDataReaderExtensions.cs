using System.Data;
using System.Data.Common;

#pragma warning disable IDE0130
namespace System.Data.Mapper;
#pragma warning restore IDE0130

/// <summary>
/// Provides extension methods for mapping objects from a <see cref="DbDataReader"/>.
/// </summary>
public static class DbDataReaderExtensions
{
    /// <summary>
    /// Creates a mapper that resolves reader columns once and reuses those bindings for every row.
    /// </summary>
    internal static Func<DbDataReader, T> CreateMapper<T>() where T : notnull, new()
    {
        if ( typeof( T ) == typeof( Dictionary<string, object?> ) )
        {
            string[]? columnNames = null;

            return reader =>
            {
                columnNames ??= GetColumnNames( reader );
                return (T)(object)MapObject( reader, columnNames );
            };
        }

        ColumnBinding[]? bindings = null;

        return reader =>
        {
            bindings ??= CreateBindings<T>( reader );
            return MapObject<T>( reader, bindings );
        };
    }

    /// <summary>
    /// Maps the current row of the reader to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to map</typeparam>
    /// <returns>An object of type <typeparamref name="T"/></returns>
    /// <exception cref="InvalidCastException">Thrown when a database value cannot be converted to the corresponding property type.</exception>
    public static T MapObject<T>( this DbDataReader reader ) where T : notnull, new()
    {
        if ( typeof( T ) == typeof( Dictionary<string, object?> ) )
        {
            return (T)(object)reader.MapObject();
        }

        var properties = EntityCache.GetEntityInfo<T>().Properties;

        // create a new instance of the object
        var obj = EntityCache.CreateInstance<T>();

        for ( int fieldOrdinal = 0; fieldOrdinal < reader.FieldCount; fieldOrdinal++ )
        {
            // skip null values
            if ( reader.IsDBNull( fieldOrdinal ) )
            {
                continue;
            }

            var fieldName = reader.GetName( fieldOrdinal );
            if ( !properties.TryGetColumn( fieldName, out var property ) || property.CanWrite is false )
            {
                // no matching property found
                continue;
            }

            object? value = ( property.DbTypeConverter is not null )
                ? EntityCache.GetDbTypeConverter( property.DbTypeConverter )
                    .Read( reader, fieldOrdinal, property.PropertyType )
                : GetValue( reader, fieldOrdinal, property.PropertyType );

            property.SetValue!( obj, value );
        }

        return obj;
    }

    /// <summary>
    /// Maps the current row of the reader to a dictionary.
    /// </summary>
    /// <returns>A dictionary containing the column names and values</returns>
    public static Dictionary<string, object?> MapObject( this DbDataReader reader )
    {
        var dictionary = new Dictionary<string, object?>( reader.FieldCount );

        for ( var ordinal = 0; ordinal < reader.FieldCount; ordinal++ )
        {
            var columnName = reader.GetName( ordinal );
            dictionary[columnName] = !reader.IsDBNull( ordinal )
                ? reader.GetValue( ordinal )
                : null;
        }

        return dictionary;
    }

    private static T MapObject<T>( DbDataReader reader, ColumnBinding[] bindings ) where T : notnull, new()
    {
        var obj = EntityCache.CreateInstance<T>();

        foreach ( var binding in bindings )
        {
            if ( reader.IsDBNull( binding.Ordinal ) )
            {
                continue;
            }

            var property = binding.Property;
            object? value = property.DbTypeConverter is not null
                ? EntityCache.GetDbTypeConverter( property.DbTypeConverter )
                    .Read( reader, binding.Ordinal, property.PropertyType )
                : reader.GetValue( binding.Ordinal );

            if ( binding.ConversionType is not null )
            {
                value = DbValueConverter.Convert( value!, binding.ConversionType );
            }

            property.SetValue!( obj, value );
        }

        return obj;
    }

    private static ColumnBinding[] CreateBindings<T>( DbDataReader reader )
    {
        var properties = EntityCache.GetEntityInfo<T>().Properties;
        var bindings = new List<ColumnBinding>( reader.FieldCount );

        for ( var ordinal = 0; ordinal < reader.FieldCount; ordinal++ )
        {
            var columnName = reader.GetName( ordinal );
            if ( properties.TryGetColumn( columnName, out var property ) && property.CanWrite )
            {
                var conversionType = property.DbTypeConverter is null
                    ? DbValueConverter.GetConversionType( reader.GetFieldType( ordinal ), property.PropertyType )
                    : null;

                bindings.Add( new ColumnBinding( ordinal, property, conversionType ) );
            }
        }

        return bindings.ToArray();
    }

    private static string[] GetColumnNames( DbDataReader reader )
    {
        var columnNames = new string[reader.FieldCount];

        for ( var ordinal = 0; ordinal < reader.FieldCount; ordinal++ )
        {
            columnNames[ordinal] = reader.GetName( ordinal );
        }

        return columnNames;
    }

    private static Dictionary<string, object?> MapObject( DbDataReader reader, string[] columnNames )
    {
        var dictionary = new Dictionary<string, object?>( reader.FieldCount );

        for ( int fieldOrdinal = 0; fieldOrdinal < reader.FieldCount; fieldOrdinal++ )
        {
            dictionary[columnNames[fieldOrdinal]] = !reader.IsDBNull( fieldOrdinal )
                ? reader.GetValue( fieldOrdinal )
                : null;
        }

        return dictionary;
    }

    private static object GetValue( DbDataReader reader, int ordinal, Type propertyType )
    {
        var value = reader.GetValue( ordinal );
        var conversionType = DbValueConverter.GetConversionType( value.GetType(), propertyType );

        return conversionType is null
            ? value
            : DbValueConverter.Convert( value, conversionType );
    }

    private readonly record struct ColumnBinding(
        int Ordinal,
        PropertyMetadata Property,
        Type? ConversionType
    );
}
