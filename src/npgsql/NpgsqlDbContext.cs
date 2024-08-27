using System.Data;

#pragma warning disable IDE0130
namespace Npgsql;
#pragma warning restore IDE0130

public class NpgsqlDbContext( DbContextOptions contextOptions ) : IDbContext
{
    private readonly DbContextOptions options = contextOptions;

    public string Name => options.Name;

    public DbContextProviderType Provider => DbContextProviderType.PostgreSql;

    public System.Data.Common.DbConnection GetDbConnection()
        => new NpgsqlConnection( options.ConnectionString );
}
