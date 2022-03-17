using System;
using System.Collections.Generic;
using System.Data;
using System.Data.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DbContextHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddDbContext( this IHealthChecksBuilder builder
            , string contextName
            , string name = default
            , HealthStatus? failureStatus = default
            , IEnumerable<string> tags = default )
        {
            builder.Add( new HealthCheckRegistration( 
                  name ?? $"dbcontext-{contextName}"
                , provider => new DbContextHealthCheck( 
                      contextName
                    , provider.GetRequiredService<IDbContextFactory>()
                )
                , failureStatus
                , tags )
                );

            return ( builder );
        }
    }
}
