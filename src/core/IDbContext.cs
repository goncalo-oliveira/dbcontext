using System;
using System.Data.Common;

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
        /// Constructs and retrieves a System.Data.Common.DbConnection instance
        /// </summary>
        /// <returns>A System.Data.Common.DbConnection instance</returns>
        DbConnection GetDbConnection();
    }
}
