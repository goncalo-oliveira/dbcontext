using System;

namespace System.Data
{
    /// <summary>
    /// A DbContextFactory contract
    /// </summary>
    public interface IDbContextFactory
    {
        /// <summary>
        /// Retrieves the context with the given name
        /// </summary>
        /// <param name="name">The name of the context</param>
        /// <returns>A System.Data.IDbContext instance</returns>
        IDbContext GetDbContext( string name );
    }
}
