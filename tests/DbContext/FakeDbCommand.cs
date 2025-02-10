using System.Data;
using System.Data.Common;

#pragma warning disable IDE0130
namespace tests;
#pragma warning restore IDE0130

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).

public sealed class FakeDbCommand( DbConnection connection, DbContextOutput output ) : DbCommand
{
    public override string CommandText { get; set; } = string.Empty;
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    protected override DbConnection? DbConnection { get; set; } = connection;

    protected override DbParameterCollection DbParameterCollection { get; } = new FakeDbParameterCollection();

    protected override DbTransaction? DbTransaction { get; set; }

    public override void Cancel()
    {
    }

    public override int ExecuteNonQuery()
    {
        output.Log( CommandText );

        return 0;
    }

    public override object? ExecuteScalar()
    {
        output.Log( CommandText );

        return null;
    }

    public override void Prepare()
    {
    }

    protected override DbParameter CreateDbParameter()
    {
        return new FakeDbParameter();
    }

    protected override DbDataReader ExecuteDbDataReader( CommandBehavior behavior )
    {
        output.Log( CommandText );

        return new FakeDbDataReader();
    }
}

#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
