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
        return Convert.ToBoolean( GetValue( ordinal ) );
    }

    public override byte GetByte(int ordinal)
    {
        return Convert.ToByte( GetValue( ordinal ) );
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override char GetChar(int ordinal)
    {
        return Convert.ToChar( GetValue( ordinal ) );
    }

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override string GetDataTypeName(int ordinal)
    {
        return GetFieldType( ordinal ).Name;
    }

    public override DateTime GetDateTime(int ordinal)
    {
        return Convert.ToDateTime( GetValue( ordinal ) );
    }

    public override decimal GetDecimal(int ordinal)
    {
        return Convert.ToDecimal( GetValue( ordinal ) );
    }

    public override double GetDouble(int ordinal)
    {
        return Convert.ToDouble( GetValue( ordinal ) );
    }

    public override IEnumerator GetEnumerator()
    {
        return rows.GetEnumerator();
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
        return Convert.ToSingle( GetValue( ordinal ) );
    }

    public override Guid GetGuid(int ordinal)
    {
        return GetValue( ordinal ) is Guid guid ? guid : Guid.Parse( GetString( ordinal ) );
    }

    public override short GetInt16(int ordinal)
    {
        return Convert.ToInt16( GetValue( ordinal ) );
    }

    public override int GetInt32(int ordinal)
    {
        return Convert.ToInt32( GetValue( ordinal ) );
    }

    public override long GetInt64(int ordinal)
    {
        return Convert.ToInt64( GetValue( ordinal ) );
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
        return Convert.ToString( GetValue( ordinal ) )!;
    }

    public override T GetFieldValue<T>( int ordinal )
        => (T)GetValue( ordinal );

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
