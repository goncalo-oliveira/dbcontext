using Microsoft.Data.Sqlite;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

public static class SqliteDbContextProviderServiceExtensions
{
    /// <summary>
    /// Adds a Sqlite context provider and provides a context builder
    /// </summary>
    public static IDbContextBuilder AddSqliteDbContextProvider( this IServiceCollection services )
        => services.AddDbContextProvider<SqliteDbContext>();
}
