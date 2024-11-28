using System.Data.Common;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Data;
#pragma warning restore IDE0130


public static class DbConnectionQueryExtensions
{
    /// <summary>
    /// Executes a query command over an open connection
    /// </summary>
    /// <typeparam name="T">The type of the object to map to each row</typeparam>
    /// <param name="connection">The connection to execute the command on</param>
    /// <param name="cmd">The command to execute</param>
    /// <param name="map">A function to map a row to an object</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An array of mapped objects</returns>
    /// <exception cref="InvalidOperationException">Thrown when the command's connection does not match the provided connection</exception>
    private static async Task<T[]> ExecuteQueryAsync<T>( this DbConnection connection, DbCommand cmd, Func<DbDataReader, T> map, CancellationToken cancellationToken = default )
    {
        if ( cmd.Connection != connection )
        {
            throw new InvalidOperationException( "The command's connection does not match the provided connection." );
        }

        await using var reader = await cmd.ExecuteReaderAsync( cancellationToken );

        return await reader.MapAsync( map, cancellationToken );
    }

    /// <summary>
    /// Executes a query command over an open connection
    /// </summary>
    /// <typeparam name="T">The type of the object to map to each row</typeparam>
    /// <param name="connection">The connection to execute the command on</param>
    /// <param name="sql">The SQL command to execute</param>
    /// <param name="map">A function to map a row to an object</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An array of mapped objects</returns>
    public static Task<T[]> ExecuteQueryAsync<T>( this DbConnection connection, string sql, Func<DbDataReader, T> map, CancellationToken cancellationToken = default )
    {
        var cmd = connection.BuildCommand( sql ).Build();

        return ExecuteQueryAsync( connection, cmd, map, cancellationToken );
    }

    /// <summary>
    /// Executes a query command over an open connection
    /// </summary>
    /// <typeparam name="T">The type of the object to map to each row</typeparam>
    /// <param name="connection">The connection to execute the command on</param>
    /// <param name="configure">A delegate to configure the command</param>
    /// <param name="map">A function to map a row to an object</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An array of mapped objects</returns>
    public static Task<T[]> ExecuteQueryAsync<T>( this DbConnection connection, Action<IDbCommandBuilder> configure, Func<DbDataReader, T> map, CancellationToken cancellationToken = default )
    {
        var builder = connection.BuildCommand();

        configure( builder );

        return ExecuteQueryAsync( connection, builder.Build(), map, cancellationToken );
    }

    /// <summary>
    /// Executes a query command over an open connection
    /// </summary>
    /// <typeparam name="T">The type of the object to map to each row</typeparam>
    /// <param name="connection">The connection to execute the command on</param>
    /// <param name="sql">The SQL command to execute</param>
    /// <param name="configure">A delegate to configure the command</param>
    /// <param name="map">A function to map a row to an object</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An array of mapped objects</returns>
    public static Task<T[]> ExecuteQueryAsync<T>( this DbConnection connection, string sql, Action<IDbCommandBuilder> configure, Func<DbDataReader, T> map, CancellationToken cancellationToken = default )
    {
        var builder = connection.BuildCommand( sql );

        configure( builder );

        return ExecuteQueryAsync( connection, builder.Build(), map, cancellationToken );
    }
}
