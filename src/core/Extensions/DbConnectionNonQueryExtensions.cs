using System.Data.Common;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Data;
#pragma warning restore IDE0130


public static class DbConnectionNonQueryExtensions
{
    /// <summary>
    /// Executes a non-query command over an open connection
    /// </summary>
    /// <param name="connection">The connection to execute the command on</param>
    /// <param name="sql">The SQL command to execute</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    public static async Task ExecuteNonQueryAsync( this DbConnection connection, string sql, CancellationToken cancellationToken = default )
    {
        var cmd = connection.BuildCommand( sql ).Build();

        await cmd.ExecuteNonQueryAsync( cancellationToken );
    }

    /// <summary>
    /// Executes a non-query command over an open connection
    /// </summary>
    /// <param name="connection">The connection to execute the command on</param>
    /// <param name="configure">A delegate to configure the command</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    public static async Task ExecuteNonQueryAsync( this DbConnection connection, Action<IDbCommandBuilder> configure, CancellationToken cancellationToken = default )
    {
        var builder = connection.BuildCommand();

        configure( builder );

        await builder.Build().ExecuteNonQueryAsync( cancellationToken );
    }

    /// <summary>
    /// Executes a non-query command over an open connection
    /// </summary>
    /// <param name="connection">The connection to execute the command on</param>
    /// <param name="sql">The SQL command to execute</param>
    /// <param name="configure">A delegate to configure the command</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    public static async Task ExecuteNonQueryAsync( this DbConnection connection, string sql, Action<IDbCommandBuilder> configure, CancellationToken cancellationToken = default )
    {
        var builder = connection.BuildCommand( sql );

        configure( builder );

        await builder.Build().ExecuteNonQueryAsync( cancellationToken );
    }
}
