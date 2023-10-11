using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data
{
    public static class DbContextConnectionExtensions
    {
        /// <summary>
        /// Opens a connection and executes an action
        /// </summary>
        /// <param name="action">The action to execute with the connection</param>
        public static void UseConnection( this IDbContext context, Action<DbConnection> action, Action<Exception> errorAction = null )
        {
            using ( var connection = context.GetDbConnection() )
            {
                try
                {
                    connection.Open();

                    action( connection );
                }
                catch ( Exception ex )
                {
                    errorAction?.Invoke( ex );
                }
            }
        }

        /// <summary>
        /// Opens a connection and executes an action
        /// </summary>
        /// <param name="action">The asynchronous action to execute with the connection</param>
        public static async Task UseConnectionAsync( this IDbContext context, Func<DbConnection, Task> action, Action<Exception> errorAction = null, CancellationToken cancellationToken = default )
        {
            using ( var connection = context.GetDbConnection() )
            {
                try
                {
                    await connection.OpenAsync( cancellationToken );

                    await action( connection );
                }
                catch ( Exception ex )
                {
                    errorAction?.Invoke( ex );
                }
            }
        }

        /// <summary>
        /// Opens a connection and executes an action
        /// </summary>
        /// <param name="action">The asynchronous action to execute with the connection</param>
        public static async Task UseConnectionAsync( this IDbContext context, Func<DbConnection, CancellationToken, Task> action, Action<Exception> errorAction = null, CancellationToken cancellationToken = default )
        {
            using ( var connection = context.GetDbConnection() )
            {
                try
                {
                    await connection.OpenAsync( cancellationToken );

                    await action( connection, cancellationToken );
                }
                catch ( Exception ex )
                {
                    errorAction?.Invoke( ex );
                }
            }
        }
    }
}
