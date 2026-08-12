using System.Data.Common;

namespace System.Data;

/// <summary>
/// Provides extensions for opening database-context connections.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Retrieves a System.Data.Common.DbConnection instance and opens it
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <returns>An open System.Data.Common.DbConnection instance</returns>
    public static DbConnection Open( this IDbContext context )
    {
        var connection = context.GetDbConnection();

        try
        {
            connection.Open();

            return connection;
        }
        catch
        {
            connection.Dispose();

            throw;
        }
    }

    /// <summary>
    /// Retrieves a System.Data.Common.DbConnection instance and opens it
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>An open System.Data.Common.DbConnection instance</returns>
    public static async Task<DbConnection> OpenAsync( this IDbContext context, CancellationToken cancellationToken = default )
    {
        var connection = context.GetDbConnection();

        try
        {
            await connection.OpenAsync( cancellationToken );

            return connection;
        }
        catch
        {
            connection.Dispose();

            throw;
        }
    }
}
