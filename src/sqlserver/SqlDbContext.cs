using System.Data;

#pragma warning disable IDE0130
namespace Microsoft.Data.SqlClient;
#pragma warning restore IDE0130

public class SqlDbContext( DbContextOptions contextOptions ) : IDbContext
{
    private readonly DbContextOptions options = contextOptions;

    public string Name => options.Name;

    public DbContextProviderType Provider => DbContextProviderType.SqlServer;

    public System.Data.Common.DbConnection GetDbConnection()
        => new SqlConnection( options.ConnectionString );
}
