using System.Data;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;

namespace tests;

public class DbContextFactoryTests
{
    [Fact]
    public void FactoryResolvesContextDependenciesFromCurrentScope()
    {
        var services = new ServiceCollection();
        services.AddScoped<ScopedDependency>();
        services.AddDbContextProvider<ContextWithScopedDependency>()
            .AddDbContext( "test", "connection-string" );

        using var provider = services.BuildServiceProvider( validateScopes: true );
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        var firstContext = Assert.IsType<ContextWithScopedDependency>(
            firstScope.ServiceProvider.GetRequiredService<IDbContextFactory>().GetDbContext( "test" )
        );
        var firstDependency = firstScope.ServiceProvider.GetRequiredService<ScopedDependency>();
        var secondContext = Assert.IsType<ContextWithScopedDependency>(
            secondScope.ServiceProvider.GetRequiredService<IDbContextFactory>().GetDbContext( "test" )
        );

        Assert.Same( firstDependency, firstContext.Dependency );
        Assert.NotSame( firstContext.Dependency, secondContext.Dependency );
    }

    private sealed class ScopedDependency;

    private sealed class ContextWithScopedDependency(
        DbContextOptions options,
        ScopedDependency dependency
    ) : IDbContext
    {
        public ScopedDependency Dependency { get; } = dependency;
        public string Name => options.Name;
        public DbContextProviderType Provider => DbContextProviderType.Custom;
        public DbConnection GetDbConnection() => throw new NotSupportedException();
    }
}
