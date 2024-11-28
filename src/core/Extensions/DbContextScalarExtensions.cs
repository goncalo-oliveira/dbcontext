#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Data;
#pragma warning restore IDE0130

public static class DbContextScalarExtensions
{
    /// <summary>
    /// Opens a connection and executes a scalar command
    /// </summary>
    /// <typeparam name="T">The type of the scalar value</typeparam>
    /// <param name="sql">The SQL command to execute</param>
    /// <param name="dbNullValue">The value to return when the result is DBNull</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The scalar value if it is not DBNull, otherwise the provided value or default</returns>
    public static async Task<T?> ExecuteScalarAsync<T>( this IDbContext context, string sql, T? dbNullValue = default, CancellationToken cancellationToken = default )
    {
        using var connection = await context.OpenAsync( cancellationToken );

        return await connection.ExecuteScalarAsync( sql, dbNullValue, cancellationToken );
    }

    /// <summary>
    /// Opens a connection and executes a scalar command
    /// </summary>
    /// <typeparam name="T">The type of the scalar value</typeparam>
    /// <param name="configure">A delegate to configure the command</param>
    /// <param name="dbNullValue">The value to return when the result is DBNull</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The scalar value if it is not DBNull, otherwise the provided value or default</returns>
    public static async Task<T?> ExecuteScalarAsync<T>( this IDbContext context, Action<IDbCommandBuilder> configure, T? dbNullValue = default, CancellationToken cancellationToken = default )
    {
        using var connection = await context.OpenAsync( cancellationToken );

        return await connection.ExecuteScalarAsync( configure, dbNullValue, cancellationToken );
    }

    /// <summary>
    /// Opens a connection and executes a scalar command
    /// </summary>
    /// <typeparam name="T">The type of the scalar value</typeparam>
    /// <param name="sql">The SQL command to execute</param>
    /// <param name="configure">A delegate to configure the command</param>
    /// <param name="dbNullValue">The value to return when the result is DBNull</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The scalar value if it is not DBNull, otherwise the provided value or default</returns>
    public static async Task<T?> ExecuteScalarAsync<T>( this IDbContext context, string sql, Action<IDbCommandBuilder> configure, T? dbNullValue = default, CancellationToken cancellationToken = default )
    {
        using var connection = await context.OpenAsync( cancellationToken );

        return await connection.ExecuteScalarAsync( sql, configure, dbNullValue, cancellationToken );
    }
}
