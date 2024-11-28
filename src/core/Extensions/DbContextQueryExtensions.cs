using System.Data.Common;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Data;
#pragma warning restore IDE0130

public static class DbContextQueryExtensions
{
    /// <summary>
    /// Opens a connection and executes a query command
    /// </summary>
    /// <typeparam name="T">The type of the object to map to each row</typeparam>
    /// <param name="context">The database context</param>
    /// <param name="sql">The SQL command to execute</param>
    /// <param name="map">A function to map a row to an object</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An array of mapped objects</returns>
    public static async Task<T[]> ExecuteQueryAsync<T>( this IDbContext context, string sql, Func<DbDataReader, T> map, CancellationToken cancellationToken = default )
    {
        using var connection = await context.OpenAsync( cancellationToken );

        return await connection.ExecuteQueryAsync( sql, map, cancellationToken );
    }

    /// <summary>
    /// Opens a connection and executes a query command
    /// </summary>
    /// <typeparam name="T">The type of the object to map to each row</typeparam>
    /// <param name="context">The database context</param>
    /// <param name="configure">A delegate to configure the command</param>
    /// <param name="map">A function to map a row to an object</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An array of mapped objects</returns>
    public static async Task<T[]> ExecuteQueryAsync<T>( this IDbContext context, Action<IDbCommandBuilder> configure, Func<DbDataReader, T> map, CancellationToken cancellationToken = default )
    {
        using var connection = await context.OpenAsync( cancellationToken );

        return await connection.ExecuteQueryAsync( configure, map, cancellationToken );
    }

    /// <summary>
    /// Opens a connection and executes a query command
    /// </summary>
    /// <typeparam name="T">The type of the object to map to each row</typeparam>
    /// <param name="context">The database context</param>
    /// <param name="sql">The SQL command to execute</param>
    /// <param name="configure">A delegate to configure the command</param>
    /// <param name="map">A function to map a row to an object</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An array of mapped objects</returns>
    public static async Task<T[]> ExecuteQueryAsync<T>( this IDbContext context, string sql, Action<IDbCommandBuilder> configure, Func<DbDataReader, T> map, CancellationToken cancellationToken = default )
    {
        using var connection = await context.OpenAsync( cancellationToken );

        return await connection.ExecuteQueryAsync( sql, configure, map, cancellationToken );
    }
}
