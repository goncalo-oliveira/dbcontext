using System;

namespace System.Data
{
    /// <summary>
    /// A DbContext contract
    /// </summary>
    public interface IDbContext
    {
        /// <summary>
        /// The name of the context
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The type of the context provider
        /// </summary>
        DbContextProviderTypes Provider { get; }

        /// <summary>
        /// Constructs and retrieves a System.Data.IDbConnection instance
        /// </summary>
        /// <returns>A System.Data.IDbConnection instance</returns>
        IDbConnection GetDbConnection();
    }
}
