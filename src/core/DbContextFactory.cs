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

        // find the constructor with the most parameters
        if ( factoryOptions.ContextType is null )
        {
            throw new InvalidOperationException( $"The '{name}' context type is null." );
        }

        var constructor = factoryOptions.ContextType.GetConstructors()
            .OrderByDescending( c => c.GetParameters().Length )
            .First();

        // get the constructor parameters
        var parameters = constructor.GetParameters();

        // populate the constructor arguments
        var arguments = new List<object>();

        foreach ( var parameter in parameters )
        {
            if ( parameter.ParameterType == typeof( DbContextOptions ) )
            {
                arguments.Add( factoryOptions.ContextOptions );
            }
            else if ( parameter.ParameterType == typeof( IServiceProvider ) )
            {
                arguments.Add( provider );
            }
            else
            {
                /*
                All other types are resolved from the service provider.
                */

                arguments.Add( provider.GetRequiredService( parameter.ParameterType ) );
            }
        }

        // create the DbContext
        var context = (IDbContext)constructor.Invoke( arguments.ToArray() );

        return context;
    }
}
