using System.Data;
using System.Data.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

public static class DbContextHealthCheckBuilderExtensions
{
    /// <summary>
    /// Adds a health check registration for a DbContext
    /// </summary>
    /// <param name="contextName">The name of the DbContext</param>
    /// <param name="name">The name of the health check</param>
    /// <param name="failureStatus">The status to return when the health check fails</param>
    /// <param name="tags">The tags to associate with the health check</param>
    public static IHealthChecksBuilder AddDbContext( this IHealthChecksBuilder builder
        , string contextName
        , string? name = null
        , HealthStatus? failureStatus = null
        , IEnumerable<string>? tags = null )
    {
        builder.Add( new HealthCheckRegistration( 
                name ?? $"IDbContext({contextName})"
            , provider => new DbContextHealthCheck( 
                    contextName
                , provider.GetRequiredService<IDbContextFactory>()
            )
            , failureStatus
            , tags )
            );

        return builder;
    }
}
