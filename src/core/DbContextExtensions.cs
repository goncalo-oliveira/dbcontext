using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// Retrieves a System.Data.Common.DbConnection instance and opens it
        /// </summary>
        /// <returns>An open System.Data.Common.DbConnection instance</returns>
        public static DbConnection Open( this IDbContext context )
        {
            var connection = context.GetDbConnection();

            connection.Open();

            return ( connection );
        }

        /// <summary>
        /// Retrieves a System.Data.Common.DbConnection instance and opens it
        /// </summary>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>An open System.Data.Common.DbConnection instance</returns>
        public static async Task<DbConnection> OpenAsync( this IDbContext context, CancellationToken cancellationToken = default )
        {
            var connection = context.GetDbConnection();

            await connection.OpenAsync( cancellationToken );

            return ( connection );
        }
    }
}
