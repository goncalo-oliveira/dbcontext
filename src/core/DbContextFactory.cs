using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace System.Data;

internal class DbContextFactory( IServiceProvider serviceProvider, IOptionsMonitor<DbContextFactoryOptions> optionsMonitor ) : IDbContextFactory
{
    private readonly IOptionsMonitor<DbContextFactoryOptions> optionsAccessor = optionsMonitor;
    private readonly IServiceProvider provider = serviceProvider;

    public IDbContext GetDbContext( string name )
    {
        var factoryOptions = optionsAccessor.Get( name );

        if ( factoryOptions.ContextType is null )
        {
            throw new InvalidOperationException( $"The '{name}' context type is null." );
        }

        return (IDbContext)ActivatorUtilities.CreateInstance(
            provider,
            factoryOptions.ContextType,
            factoryOptions.ContextOptions
        );
    }
}
