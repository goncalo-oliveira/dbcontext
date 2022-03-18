using System;
using System.Threading.Tasks;

namespace System.Data
{
    public static class DbContextConnectionExtensions
    {
        /// <summary>
        /// Opens a connection and executes an action
        /// </summary>
        /// <param name="action">The action to execute with the connection</param>
        public static void UseConnection( this IDbContext context, Action<IDbConnection> action, Action<Exception> errorAction = null )
        {
            using ( var connection = context.Open() )
            {
                try
                {
                    action.Invoke( connection );
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
        public static async Task UseConnectionAsync( this IDbContext context, Func<IDbConnection, Task> action, Action<Exception> errorAction = null )
        {
            using ( var connection = await context.OpenAsync() )
            {
                try
                {
                    await action.Invoke( connection );
                }
                catch ( Exception ex )
                {
                    errorAction?.Invoke( ex );
                }
            }
        }
    }
}
