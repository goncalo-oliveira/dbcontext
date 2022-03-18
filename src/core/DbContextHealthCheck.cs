using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace System.Data.HealthChecks
{
    internal class DbContextHealthCheck : IHealthCheck
    {
        private readonly IDbContext db;

        public DbContextHealthCheck( string contextName, IDbContextFactory dbContextFactory )
        {
            db = dbContextFactory.GetDbContext( contextName );
        }

        public async Task<HealthCheckResult> CheckHealthAsync( HealthCheckContext context, CancellationToken cancellationToken )
        {
            try
            {
                using ( var connection = await db.OpenAsync( cancellationToken ) )
                {
                    using ( var command = connection.CreateCommand() )
                    {
                        command.CommandText = "select 1";

                        _ = await command.ExecuteScalarAsync( cancellationToken );
                    }
                }

                return HealthCheckResult.Healthy();
            }
            catch ( Exception ex )
            {
                return new HealthCheckResult( context.Registration.FailureStatus, exception: ex );
            }
        }
    }
}
