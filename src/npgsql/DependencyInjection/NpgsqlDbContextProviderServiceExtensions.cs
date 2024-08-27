using Npgsql;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

public static class NpgsqlDbContextProviderServiceExtensions
{
    /// <summary>
    /// Adds a PostgreSql context provider and provides a context builder
    /// </summary>
    public static IDbContextBuilder AddNpgsqlDbContextProvider( this IServiceCollection services )
        => services.AddDbContextProvider<NpgsqlDbContext>();
}
