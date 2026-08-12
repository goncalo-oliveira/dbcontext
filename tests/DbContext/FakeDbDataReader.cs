using System.Collections;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130
namespace tests;
#pragma warning restore IDE0130

public sealed class FakeDbDataReader : DbDataReader
{
    private readonly string[] columns;
    private readonly object?[][] rows;
    private int rowIndex = -1;

    public FakeDbDataReader()
        : this( [], [] )
    {
    }

    public FakeDbDataReader( string[] columns, params object?[][] rows )
    {
        this.columns = columns;
        this.rows = rows;
    }

    public int GetNameCallCount { get; private set; }

    public override object this[int ordinal] => throw new NotImplementedException();

    public override object this[string name] => throw new NotImplementedException();

    public override int Depth => throw new NotImplementedException();

    public override int FieldCount => columns.Length;

    public override bool HasRows => rows.Length > 0;

    public override bool IsClosed => throw new NotImplementedException();

    public override int RecordsAffected => throw new NotImplementedException();

    public override bool GetBoolean(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override byte GetByte(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override char GetChar(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override string GetDataTypeName(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override DateTime GetDateTime(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override decimal GetDecimal(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override double GetDouble(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
    public override Type GetFieldType(int ordinal)
    {
        return rows
            .Select( row => row[ordinal] )
            .FirstOrDefault( value => value is not null and not DBNull )
            ?.GetType()
            ?? typeof( object );
    }

    public override float GetFloat(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override Guid GetGuid(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override short GetInt16(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override int GetInt32(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetInt64(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override string GetName(int ordinal)
    {
        GetNameCallCount++;
        return columns[ordinal];
    }

    public override int GetOrdinal(string name)
    {
        throw new NotImplementedException();
    }

    public override string GetString(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override object GetValue(int ordinal)
    {
        return rows[rowIndex][ordinal] ?? DBNull.Value;
    }

    public override int GetValues(object[] values)
    {
        throw new NotImplementedException();
    }

    public override bool IsDBNull(int ordinal)
    {
        return rows[rowIndex][ordinal] is null or DBNull;
    }

    public override bool NextResult()
    {
        return false;
    }

    public override bool Read()
    {
        rowIndex++;
        return rowIndex < rows.Length;
    }
}
