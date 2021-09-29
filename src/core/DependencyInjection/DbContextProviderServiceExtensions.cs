using System;
using System.Data;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DbContextProviderServiceExtensions
    {
        /// <summary>
        /// Adds the specified context provider and provides a context builder
        /// </summary>
        /// <typeparam name="TDbContext">The DbContext provider type</typeparam>
        public static IDbContextBuilder AddDbContextProvider<TDbContext>( this IServiceCollection services ) where TDbContext : class, IDbContext
        {
            services.TryAddSingleton<IDbContextFactory, DbContextFactory>();

            return ( new DbContextBuilder( typeof( TDbContext ), services ) );
        }
    }
}
