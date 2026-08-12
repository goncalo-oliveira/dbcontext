using System.Data;

#pragma warning disable IDE0130
namespace Microsoft.Data.Sqlite;
#pragma warning restore IDE0130

/// <summary>
/// Represents a SQLite database context.
/// </summary>
/// <param name="contextOptions">The context configuration.</param>
public class SqliteDbContext( DbContextOptions contextOptions ) : IDbContext
{
    private readonly DbContextOptions options = contextOptions;

    /// <inheritdoc />
    public string Name => options.Name;

    /// <inheritdoc />
    public DbContextProviderType Provider => DbContextProviderType.Sqlite;

    /// <inheritdoc />
    public System.Data.Common.DbConnection GetDbConnection()
        => new SqliteConnection( options.ConnectionString );
}
