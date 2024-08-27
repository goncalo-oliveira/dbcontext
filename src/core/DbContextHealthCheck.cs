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
            using ( var connection = await db.OpenAsync( cancellationToken ) )
            {
                using var command = connection.CreateCommand();
                
                command.CommandText = "select 1";

                _ = await command.ExecuteScalarAsync(cancellationToken);
            }

            return HealthCheckResult.Healthy();
        }
        catch ( Exception ex )
        {
            return new HealthCheckResult( context.Registration.FailureStatus, exception: ex );
        }
    }
}
