using System.Data.Common;

namespace System.Data;

/// <summary>
/// Provides extensions for executing operations with context-owned connections.
/// </summary>
public static class DbContextConnectionExtensions
{
    /// <summary>
    /// Opens a connection and executes an action
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="action">The action to execute with the connection</param>
    /// <param name="errorAction">An optional action that handles non-cancellation errors.</param>
    public static void UseConnection( this IDbContext context, Action<DbConnection> action, Action<Exception>? errorAction = null )
    {
        using var connection = context.GetDbConnection();
        try
        {
            connection.Open();

            action( connection );
        }
        catch ( OperationCanceledException )
        {
            throw;
        }
        catch ( Exception ex )
        {
            if ( errorAction is null )
            {
                throw;
            }

            errorAction( ex );
        }
    }

    /// <summary>
    /// Opens a connection and executes an action
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="action">The asynchronous action to execute with the connection</param>
    /// <param name="errorAction">An optional action that handles non-cancellation errors.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    public static async Task UseConnectionAsync( this IDbContext context, Func<DbConnection, Task> action, Action<Exception>? errorAction = null, CancellationToken cancellationToken = default )
    {
        using var connection = context.GetDbConnection();
        try
        {
            await connection.OpenAsync( cancellationToken );

            await action( connection );
        }
        catch ( OperationCanceledException )
        {
            throw;
        }
        catch ( Exception ex )
        {
            if ( errorAction is null )
            {
                throw;
            }

            errorAction( ex );
        }
    }

    /// <summary>
    /// Opens a connection and executes an action
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="action">The asynchronous action to execute with the connection</param>
    /// <param name="errorAction">An optional action that handles non-cancellation errors.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    public static async Task UseConnectionAsync( this IDbContext context, Func<DbConnection, CancellationToken, Task> action, Action<Exception>? errorAction = null, CancellationToken cancellationToken = default )
    {
        using var connection = context.GetDbConnection();
        try
        {
            await connection.OpenAsync( cancellationToken );

            await action( connection, cancellationToken );
        }
        catch ( OperationCanceledException )
        {
            throw;
        }
        catch ( Exception ex )
        {
            if ( errorAction is null )
            {
                throw;
            }

            errorAction( ex );
        }
    }
}
