using System.Data.Common;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Data;
#pragma warning restore IDE0130

public static class DbDataReaderMapperExtensions
{
    /// <summary>
    /// Reads all rows from the reader and maps them to an array of objects
    /// </summary>
    /// <typeparam name="T">The type of object to map to each row</typeparam>
    /// <param name="reader">The reader to read from</param>
    /// <param name="map">A function to map a row to an object</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An array of mapped objects</returns>
    public static async Task<T[]> MapAsync<T>( this DbDataReader reader, Func<DbDataReader, T> map, CancellationToken cancellationToken = default )
    {
        var list = new List<T>();

        while ( await reader.ReadAsync( cancellationToken) )
        {
            list.Add( map( reader ) );
        }

        return list.ToArray();
    }
}
