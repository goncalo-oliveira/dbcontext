using System.Data;

#pragma warning disable IDE0130
namespace Microsoft.Data.SqlClient;
#pragma warning restore IDE0130

/// <summary>
/// Represents a SQL Server database context.
/// </summary>
/// <param name="contextOptions">The context configuration.</param>
public class SqlDbContext( DbContextOptions contextOptions ) : IDbContext
{
    private readonly DbContextOptions options = contextOptions;

    /// <inheritdoc />
    public string Name => options.Name;

    /// <inheritdoc />
    public DbContextProviderType Provider => DbContextProviderType.SqlServer;

    /// <inheritdoc />
    public System.Data.Common.DbConnection GetDbConnection()
        => new SqlConnection( options.ConnectionString );
}
