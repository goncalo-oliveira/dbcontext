using System.Data;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

public static class RestSqlDbContextProviderServiceExtensions
{
    /// <summary>
    /// Adds a RestSql context provider and provides a context builder
    /// </summary>
    public static IDbContextBuilder AddRestSqlDbContextProvider( this IServiceCollection services )
    {
        services.AddHttpClient();

        return services.AddDbContextProvider<RestSqlDbContext>();
    }
}
