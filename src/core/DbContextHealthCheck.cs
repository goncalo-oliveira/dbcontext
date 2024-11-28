using Microsoft.Extensions.Diagnostics.HealthChecks;

#pragma warning disable IDE0130
namespace System.Data.HealthChecks;
#pragma warning restore IDE0130

internal class DbContextHealthCheck( string contextName, IDbContextFactory dbContextFactory ) : IHealthCheck
{
    private readonly IDbContext db = dbContextFactory.GetDbContext( contextName );

    public async Task<HealthCheckResult> CheckHealthAsync( HealthCheckContext context, CancellationToken cancellationToken )
    {
        try
        {
            _ = await db.ExecuteScalarAsync( "select 1", cancellationToken );

            return HealthCheckResult.Healthy();
        }
        catch ( Exception ex )
        {
            return new HealthCheckResult( context.Registration.FailureStatus, exception: ex );
        }
    }
}
