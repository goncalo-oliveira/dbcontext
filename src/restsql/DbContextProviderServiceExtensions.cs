using System;
using System.Data;

namespace Microsoft.Extensions.DependencyInjection;

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
