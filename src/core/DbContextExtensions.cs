using System;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// Retrieves a System.Data.IDbConnection instance and opens it
        /// </summary>
        /// <returns>An open System.Data.IDbConnection instance</returns>
        public static IDbConnection Open( this IDbContext context )
        {
            var connection = context.GetDbConnection();

            connection.Open();

            return ( connection );
        }

        /// <summary>
        /// Retrieves a System.Data.IDbConnection instance and opens it
        /// </summary>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>An open System.Data.IDbConnection instance</returns>
        public static async Task<IDbConnection> OpenAsync( this IDbContext context, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            var connection = context.GetDbConnection();

            await connection.OpenAsync( cancellationToken );

            return ( connection );
        }
    }
}
