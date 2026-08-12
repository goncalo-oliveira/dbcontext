using System.Data;

#pragma warning disable IDE0130
namespace Npgsql;
#pragma warning restore IDE0130

/// <summary>
/// Represents a PostgreSQL database context.
/// </summary>
/// <param name="contextOptions">The context configuration.</param>
public class NpgsqlDbContext( DbContextOptions contextOptions ) : IDbContext
{
    private readonly DbContextOptions options = contextOptions;

    /// <inheritdoc />
    public string Name => options.Name;

    /// <inheritdoc />
    public DbContextProviderType Provider => DbContextProviderType.PostgreSql;

    /// <inheritdoc />
    public System.Data.Common.DbConnection GetDbConnection()
        => new NpgsqlConnection( options.ConnectionString );
}
