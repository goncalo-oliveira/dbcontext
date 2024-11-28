using System.Data.Common;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Data;
#pragma warning restore IDE0130

public static class DbConnectionScalarExtensions
{
    /// <summary>
    /// Executes a scalar command over an open connection
    /// </summary>
    /// <typeparam name="T">The type of the scalar value</typeparam>
    /// <param name="connection">The connection to execute the command on</param>
    /// <param name="cmd">The command to execute</param>
    /// <param name="dbNullValue">The value to return when the result is DBNull</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The scalar value if it is not DBNull, otherwise the provided value or default</returns>
    /// <exception cref="InvalidOperationException">Thrown when the command's connection does not match the provided connection</exception>
    private static async Task<T?> ExecuteScalarAsync<T>( this DbConnection connection, DbCommand cmd, T? dbNullValue = default, CancellationToken cancellationToken = default )
    {
        if ( cmd.Connection != connection )
        {
            throw new InvalidOperationException( "The command's connection does not match the provided connection." );
        }

        var result = await cmd.ExecuteScalarAsync( cancellationToken );

        return result == DBNull.Value ? dbNullValue : (T?)result;
    }

    /// <summary>
    /// Executes a scalar command over an open connection
    /// </summary>
    /// <typeparam name="T">The type of the scalar value</typeparam>
    /// <param name="connection">The connection to execute the command on</param>
    /// <param name="sql">The SQL command to execute</param>
    /// <param name="dbNullValue">The value to return when the result is DBNull</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The scalar value if it is not DBNull, otherwise the provided value or default</returns>
    public static Task<T?> ExecuteScalarAsync<T>( this DbConnection connection, string sql, T? dbNullValue = default, CancellationToken cancellationToken = default )
    {
        var cmd = connection.BuildCommand( sql ).Build();

        return ExecuteScalarAsync<T>( connection, cmd, dbNullValue, cancellationToken );
    }

    /// <summary>
    /// Executes a scalar command over an open connection
    /// </summary>
    /// <typeparam name="T">The type of the scalar value</typeparam>
    /// <param name="connection">The connection to execute the command on</param>
    /// <param name="configure">A delegate to configure the command</param>
    /// <param name="dbNullValue">The value to return when the result is DBNull</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The scalar value if it is not DBNull, otherwise the provided value or default</returns>
    public static Task<T?> ExecuteScalarAsync<T>( this DbConnection connection, Action<IDbCommandBuilder> configure, T? dbNullValue = default, CancellationToken cancellationToken = default )
    {
        var builder = connection.BuildCommand();

        configure( builder );

        return ExecuteScalarAsync<T>( connection, builder.Build(), dbNullValue, cancellationToken );
    }

    /// <summary>
    /// Executes a scalar command over an open connection
    /// </summary>
    /// <typeparam name="T">The type of the scalar value</typeparam>
    /// <param name="connection">The connection to execute the command on</param>
    /// <param name="sql">The SQL command to execute</param>
    /// <param name="configure">A delegate to configure the command</param>
    /// <param name="dbNullValue">The value to return when the result is DBNull</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The scalar value if it is not DBNull, otherwise the provided value or default</returns>
    public static Task<T?> ExecuteScalarAsync<T>( this DbConnection connection, string sql, Action<IDbCommandBuilder> configure, T? dbNullValue = default, CancellationToken cancellationToken = default )
    {
        var builder = connection.BuildCommand( sql );

        configure( builder );

        return ExecuteScalarAsync<T>( connection, builder.Build(), dbNullValue, cancellationToken );
    }
}
