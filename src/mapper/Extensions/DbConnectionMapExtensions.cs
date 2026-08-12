using System.Data.Common;
using System.Data.Mapper;

#pragma warning disable IDE0130
namespace System.Data;
#pragma warning restore IDE0130

/// <summary>
/// Extension methods for <see cref="DbConnection"/> to map query results.
/// </summary>
public static class DbConnectionMapExtensions
{
    /// <summary>
    /// Executes a query and maps the results to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to map the results to</typeparam>
    /// <param name="connection">The database connection.</param>
    /// <param name="sql">The SQL query to execute</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An array of objects of type <typeparamref name="T"/></returns>
    /// <exception cref="InvalidCastException">Thrown when a database value cannot be converted to the corresponding property type.</exception>
    public static async Task<T[]> ExecuteQueryAsync<T>( this DbConnection connection, string sql, CancellationToken cancellationToken = default ) where T : notnull, new()
        => await connection.ExecuteQueryAsync( sql, System.Data.Mapper.DbDataReaderExtensions.CreateMapper<T>(), cancellationToken );

    /// <summary>
    /// Executes a query and maps the results to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to map the results to</typeparam>
    /// <param name="connection">The database connection.</param>
    /// <param name="configure">A delegate to configure the command</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An array of objects of type <typeparamref name="T"/></returns>
    /// <exception cref="InvalidCastException">Thrown when a database value cannot be converted to the corresponding property type.</exception>
    public static async Task<T[]> ExecuteQueryAsync<T>( this DbConnection connection, Action<IDbCommandBuilder> configure, CancellationToken cancellationToken = default ) where T : notnull, new()
        => await connection.ExecuteQueryAsync( configure, System.Data.Mapper.DbDataReaderExtensions.CreateMapper<T>(), cancellationToken );

    /// <summary>
    /// Executes a query and maps the results to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to map the results to</typeparam>
    /// <param name="connection">The database connection.</param>
    /// <param name="sql">The SQL query to execute</param>
    /// <param name="configure">A delegate to configure the command</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An array of objects of type <typeparamref name="T"/></returns>
    /// <exception cref="InvalidCastException">Thrown when a database value cannot be converted to the corresponding property type.</exception>
    public static async Task<T[]> ExecuteQueryAsync<T>( this DbConnection connection, string sql, Action<IDbCommandBuilder> configure, CancellationToken cancellationToken = default ) where T : notnull, new()
        => await connection.ExecuteQueryAsync( sql, configure, System.Data.Mapper.DbDataReaderExtensions.CreateMapper<T>(), cancellationToken );
}
