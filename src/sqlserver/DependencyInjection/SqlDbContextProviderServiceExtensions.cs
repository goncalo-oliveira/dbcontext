using Microsoft.Data.SqlClient;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

public static class SqlDbContextProviderServiceExtensions
{
    /// <summary>
    /// Adds a SqlServer context provider and provides a context builder
    /// </summary>
    public static IDbContextBuilder AddSqlDbContextProvider( this IServiceCollection services )
        => services.AddDbContextProvider<SqlDbContext>();
}
