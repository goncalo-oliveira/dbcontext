using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// The context builder interface
    /// </summary>
    public interface IDbContextBuilder
    {
        /// <summary>
        /// The context type
        /// </summary>
        Type ContextType { get; }

        /// <summary>
        /// The collection of service descriptors
        /// </summary>
        IServiceCollection Services { get; }
    }
}
