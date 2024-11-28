#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Data;
#pragma warning restore IDE0130

public static class DbContextNonQueryExtensions
{
    /// <summary>
    /// Opens a connection and executes a non-query command
    /// </summary>
    /// <param name="sql">The SQL command to execute</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    public static async Task ExecuteNonQueryAsync( this IDbContext context, string sql, CancellationToken cancellationToken = default )
    {
        using var connection = await context.OpenAsync( cancellationToken );

        await connection.ExecuteNonQueryAsync( sql, cancellationToken );
    }

    /// <summary>
    /// Opens a connection and executes a non-query command
    /// </summary>
    /// <param name="configure">A delegate to configure the command</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    public static async Task ExecuteNonQueryAsync( this IDbContext context, Action<IDbCommandBuilder> configure, CancellationToken cancellationToken = default )
    {
        using var connection = await context.OpenAsync( cancellationToken );

        await connection.ExecuteNonQueryAsync( configure, cancellationToken );
    }

    /// <summary>
    /// Opens a connection and executes a non-query command
    /// </summary>
    /// <param name="sql">The SQL command to execute</param>
    /// <param name="configure">A delegate to configure the command</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    public static async Task ExecuteNonQueryAsync( this IDbContext context, string sql, Action<IDbCommandBuilder> configure, CancellationToken cancellationToken = default )
    {
        using var connection = await context.OpenAsync( cancellationToken );

        await connection.ExecuteNonQueryAsync( sql, configure, cancellationToken );
    }
}
