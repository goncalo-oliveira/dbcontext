using System.Data;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

/// <summary>
/// Provides extensions for registering database-context providers.
/// </summary>
public static class DbContextProviderServiceExtensions
{
    /// <summary>
    /// Adds the specified context provider and provides a context builder
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext provider type</typeparam>
    public static IDbContextBuilder AddDbContextProvider<TDbContext>( this IServiceCollection services ) where TDbContext : class, IDbContext
    {
        services.TryAddScoped<IDbContextFactory, DbContextFactory>();

        return new DbContextBuilder( typeof( TDbContext ), services );
    }
}
