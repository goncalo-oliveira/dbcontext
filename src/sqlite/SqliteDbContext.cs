using System.Data;

#pragma warning disable IDE0130
namespace Microsoft.Data.Sqlite;
#pragma warning restore IDE0130

public class SqliteDbContext( DbContextOptions contextOptions ) : IDbContext
{
    private readonly DbContextOptions options = contextOptions;

    public string Name => options.Name;

    public DbContextProviderType Provider => DbContextProviderType.Sqlite;

    public System.Data.Common.DbConnection GetDbConnection()
        => new SqliteConnection( options.ConnectionString );
}
