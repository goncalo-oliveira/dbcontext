using System.Data;
using System.Data.Common;
using System.Text;

#pragma warning disable IDE0130
namespace tests;
#pragma warning restore IDE0130

public sealed class FakeDbContext : IDbContext
{
    public string Name => "tests";

    public DbContextOutput Output { get; } = new();

    public DbContextProviderType Provider => DbContextProviderType.PostgreSql;

    public DbConnection GetDbConnection()
    {
        return new FakeNpgsqlDbConnection( Output );
    }
}

