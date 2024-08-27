using System.Data.Common;

namespace System.Data;

public static class DbContextTransactionExtensions
{
    /// <summary>
    /// Opens a connection and provides a transaction to work with
    /// </summary>
    /// <param name="action">The action to execute with the transaction</param>
    /// <param name="errorAction">The action to execute if an error occurs</param>
    /// <returns>True if the transaction was successful, false otherwise</returns>
    public static bool UseTransaction( this IDbContext context, Action<DbTransaction> action, Action<Exception>? errorAction = null )
    {
        using var connection = context.GetDbConnection();
        try
        {
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                action.Invoke( transaction );

                return true;
            }
            catch ( Exception ex )
            {
                transaction.Rollback();

                errorAction?.Invoke( ex );

                return false;
            }
        }
        catch ( Exception ex )
        {
            errorAction?.Invoke( ex );

            return false;
        }
    }

    /// <summary>
    /// Opens a connection and provides a transaction to work with
    /// </summary>
    /// <param name="isolationLevel">The transaction's isolation level</param>
    /// <param name="action">The action to execute with the transaction</param>
    /// <param name="errorAction">The action to execute if an error occurs</param>
    /// <returns>True if the transaction was successful, false otherwise</returns>
    public static bool UseTransaction( this IDbContext context, IsolationLevel isolationLevel, Action<DbTransaction> action, Action<Exception>? errorAction = null )
    {
        using var connection = context.GetDbConnection();
        try
        {
            connection.Open();

            using var transaction = connection.BeginTransaction( isolationLevel );
            try
            {
                action.Invoke( transaction );

                return true;
            }
            catch ( Exception ex )
            {
                transaction.Rollback();

                errorAction?.Invoke( ex );

                return false;
            }
        }
        catch ( Exception ex )
        {
            errorAction?.Invoke( ex );

            return false;
        }
    }

    /// <summary>
    /// Opens a connection and provides a transaction to work with asynchronously
    /// </summary>
    /// <param name="action">The asynchronous action to execute with the transaction</param>
    /// <param name="errorAction">The action to execute if an error occurs</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the transaction was successful, false otherwise</returns>
    public static async Task<bool> UseTransactionAsync( this IDbContext context, Func<DbTransaction, Task> action, Action<Exception>? errorAction = null, CancellationToken cancellationToken = default )
    {
        using var connection = context.GetDbConnection();
        try
        {
            await connection.OpenAsync( cancellationToken );

            using var transaction = await connection.BeginTransactionAsync( cancellationToken );
            try
            {
                await action( transaction );

                return true;
            }
            catch ( Exception ex )
            {
                await transaction.RollbackAsync( CancellationToken.None );

                errorAction?.Invoke( ex );

                return false;
            }
        }
        catch ( Exception ex )
        {
            errorAction?.Invoke( ex );

            return false;
        }
    }

    /// <summary>
    /// Opens a connection and provides a transaction to work with asynchronously
    /// </summary>
    /// <param name="action">The asynchronous action to execute with the transaction</param>
    /// <param name="errorAction">The action to execute if an error occurs</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the transaction was successful, false otherwise</returns>
    public static async Task<bool> UseTransactionAsync( this IDbContext context, Func<DbTransaction, CancellationToken, Task> action, Action<Exception>? errorAction = null, CancellationToken cancellationToken = default )
    {
        using var connection = context.GetDbConnection();
        try
        {
            await connection.OpenAsync( cancellationToken );

            using var transaction = await connection.BeginTransactionAsync( cancellationToken );
            try
            {
                await action( transaction, cancellationToken );

                return true;
            }
            catch ( Exception ex )
            {
                await transaction.RollbackAsync( CancellationToken.None );

                errorAction?.Invoke( ex );

                return false;
            }
        }
        catch ( Exception ex )
        {
            errorAction?.Invoke( ex );

            return false;
        }
    }

    /// <summary>
    /// Opens a connection and provides a transaction to work with asynchronously
    /// </summary>
    /// <param name="isolationLevel">The transaction's isolation level</param>
    /// <param name="action">The asynchronous action to execute with the transaction</param>
    /// <param name="errorAction">The action to execute if an error occurs</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the transaction was successful, false otherwise</returns>
    public static async Task<bool> UseTransactionAsync( this IDbContext context, IsolationLevel isolationLevel, Func<DbTransaction, Task> action, Action<Exception>? errorAction = null, CancellationToken cancellationToken = default )
    {
        using var connection = context.GetDbConnection();
        try
        {
            await connection.OpenAsync( cancellationToken );

            using var transaction = await connection.BeginTransactionAsync( isolationLevel, cancellationToken );
            try
            {
                await action( transaction );

                return true;
            }
            catch ( Exception ex )
            {
                await transaction.RollbackAsync( CancellationToken.None );

                errorAction?.Invoke( ex );

                return false;
            }
        }
        catch ( Exception ex )
        {
            errorAction?.Invoke( ex );

            return false;
        }
    }

    /// <summary>
    /// Opens a connection and provides a transaction to work with asynchronously
    /// </summary>
    /// <param name="isolationLevel">The transaction's isolation level</param>
    /// <param name="action">The asynchronous action to execute with the transaction</param>
    /// <param name="errorAction">The action to execute if an error occurs</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the transaction was successful, false otherwise</returns>
    public static async Task<bool> UseTransactionAsync( this IDbContext context, IsolationLevel isolationLevel, Func<DbTransaction, CancellationToken, Task> action, Action<Exception>? errorAction = null, CancellationToken cancellationToken = default )
    {
        using var connection = context.GetDbConnection();
        try
        {
            await connection.OpenAsync( cancellationToken );

            using var transaction = await connection.BeginTransactionAsync( isolationLevel, cancellationToken );
            try
            {
                await action( transaction, cancellationToken );

                return true;
            }
            catch ( Exception ex )
            {
                await transaction.RollbackAsync( CancellationToken.None );

                errorAction?.Invoke( ex );

                return false;
            }
        }
        catch ( Exception ex )
        {
            errorAction?.Invoke( ex );

            return false;
        }
    }
}
