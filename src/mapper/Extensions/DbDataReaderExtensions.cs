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
    /// Maps the current row of the reader to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to map</typeparam>
    /// <returns>An object of type <typeparamref name="T"/></returns>
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
                : reader.GetValue( fieldOrdinal );

            property.SetValue( obj, value );
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

        for ( int fieldOrdinal = 0; fieldOrdinal < reader.FieldCount; fieldOrdinal++ )
        {
            var columnName = reader.GetName( fieldOrdinal );

            dictionary[columnName] = !reader.IsDBNull( fieldOrdinal )
                ? reader.GetValue( fieldOrdinal )
                : null;
        }

        return dictionary;
    }
}
