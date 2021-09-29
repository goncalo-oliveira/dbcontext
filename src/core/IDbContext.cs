using System;

namespace System.Data
{
    /// <summary>
    /// A DbContext contract
    /// </summary>
    public interface IDbContext
    {
        DbContextProviderTypes Provider { get; }

        /// <summary>
        /// Constructs and retrieves a System.Data.IDbConnection instance
        /// </summary>
        /// <returns>A System.Data.IDbConnection instance</returns>
        IDbConnection GetDbConnection();
    }
}
