using System.Data;
using System.Data.Common;

#pragma warning disable IDE0130
namespace tests;
#pragma warning restore IDE0130

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).

public sealed class FakeNpgsqlDbConnection( DbContextOutput output ) : DbConnection
{
    public override string ConnectionString { get; set; } = string.Empty;

    public override string Database => string.Empty;

    public override string DataSource => string.Empty;

    public override string ServerVersion => string.Empty;

    public override ConnectionState State => ConnectionState.Open;

    public override void ChangeDatabase( string databaseName )
    {
    }

    public override void Close()
    {
    }

    public override void Open()
    {
    }

    protected override DbTransaction BeginDbTransaction( IsolationLevel isolationLevel )
    {
        throw new NotImplementedException();
    }

    protected override DbCommand CreateDbCommand()
    {
        return new FakeDbCommand( this, output );
    }
}

#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
