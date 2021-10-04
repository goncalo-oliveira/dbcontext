using System;
using System.Threading.Tasks;

namespace System.Data
{
    public static class DbContextTransactionExtensions
    {
        /// <summary>
        /// Opens a connection and provides a transaction to work with
        /// </summary>
        /// <param name="action">The action to execute with the transaction</param>
        public static void UseTransaction( this IDbContext context, Action<IDbTransaction> action, Action<Exception> errorAction = null )
        {
            using ( var connection = context.Open() )
            {
                using ( var transaction = connection.BeginTransaction() )
                {
                    try
                    {
                        action.Invoke( transaction );
                    }
                    catch ( Exception ex )
                    {
                        transaction.Rollback();

                        errorAction?.Invoke( ex );
                    }
                }
            }
        }

        /// <summary>
        /// Opens a connection and provides a transaction to work with
        /// </summary>
        /// <param name="isolationLevel">The transaction's isolation level</param>
        /// <param name="action">The action to execute with the transaction</param>
        public static void UseTransaction( this IDbContext context, IsolationLevel isolationLevel, Action<IDbTransaction> action, Action<Exception> errorAction = null )
        {
            using ( var connection = context.Open() )
            {
                using ( var transaction = connection.BeginTransaction( isolationLevel ) )
                {
                    try
                    {
                        action.Invoke( transaction );
                    }
                    catch ( Exception ex )
                    {
                        transaction.Rollback();

                        errorAction?.Invoke( ex );
                    }
                }
            }
        }

        /// <summary>
        /// Opens a connection and provides a transaction to work with asynchronously
        /// </summary>
        /// <param name="action">The asynchronous action to execute with the transaction</param>
        public static async Task UseTransactionAsync( this IDbContext context, Func<IDbTransaction, ValueTask> action, Action<Exception> errorAction = null )
        {
            using ( var connection = await context.OpenAsync() )
            {
                using ( var transaction = await connection.BeginTransactionAsync() )
                {
                    try
                    {
                        await action.Invoke( transaction );
                    }
                    catch ( Exception ex )
                    {
                        transaction.Rollback();

                        errorAction?.Invoke( ex );
                    }
                }
            }
        }

        /// <summary>
        /// Opens a connection and provides a transaction to work with asynchronously
        /// </summary>
        /// <param name="isolationLevel">The transaction's isolation level</param>
        /// <param name="action">The asynchronous action to execute with the transaction</param>
        public static async Task UseTransactionAsync( this IDbContext context, IsolationLevel isolationLevel, Func<IDbTransaction, ValueTask> action, Action<Exception> errorAction = null )
        {
            using ( var connection = await context.OpenAsync() )
            {
                using ( var transaction = await connection.BeginTransactionAsync( isolationLevel ) )
                {
                    try
                    {
                        await action.Invoke( transaction );
                    }
                    catch ( Exception ex )
                    {
                        transaction.Rollback();

                        errorAction?.Invoke( ex );
                    }
                }
            }
        }
    }
}
