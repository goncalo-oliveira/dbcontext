using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Faactory.RestSql;

public sealed class RestSqlDataReader : DbDataReader
{
    private readonly IEnumerable<JsonElement> records;
    private int currentRecordIndex;

    internal RestSqlDataReader( JsonDocument jsonDocument )
    {
        JsonDocument = jsonDocument;

        // kind is expected to be an array
        if ( JsonDocument.RootElement.ValueKind != JsonValueKind.Array )
        {
            throw new ArgumentException( "Invalid JSON document. Expected an array." );
        }

        records = JsonDocument.RootElement.EnumerateArray();
        currentRecordIndex = -1;
    }

    public JsonDocument JsonDocument { get; set; }

    public JsonElement CurrentRecord => records.ElementAt( currentRecordIndex );

    public override object this[int ordinal] => GetValue( ordinal );

    public override object this[string name]
    {
        get
        {
            var index = GetOrdinal( name );

            if ( index < 0 )
            {
                throw new ArgumentException( $"Column '{name}' not found." );
            }

            return GetValue( index );
        }
    }

    public override int Depth => 0;

    public override int FieldCount
    {
        get
        {
            if ( currentRecordIndex < 0 )
            {
                return 0;
            }

            return CurrentRecord.EnumerateObject().Count();
        }
    }

    public override bool HasRows => records.Any();

    public override bool IsClosed => true;

    public override int RecordsAffected => -1;

    public override bool GetBoolean( int ordinal )
        => CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.GetBoolean();

    public override byte GetByte( int ordinal )
        => CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.GetByte();

    public override long GetBytes( int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length )
    {
        var bytes = CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.GetBytesFromBase64();

        if ( buffer != null )
        {
            Array.Copy( bytes, dataOffset, buffer, bufferOffset, length );
        }

        return Math.Min( bytes.Length, length );
    }

    public override char GetChar( int ordinal )
        => (char)CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.GetByte();

    public override long GetChars( int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length )
    {
        var chars = CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.GetString();

        if ( chars == null )
        {
            return ( 0 );
        }

        if ( buffer != null )
        {
            Array.Copy( chars.ToCharArray(), dataOffset, buffer, bufferOffset, length );
        }

        return Math.Min( chars.Length, length );
    }

    public override string GetDataTypeName( int ordinal )
        => CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.ValueKind.ToString();

    public override DateTime GetDateTime( int ordinal )
        => CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.GetDateTime();

    public override decimal GetDecimal( int ordinal )
        => CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.GetDecimal();

    public override double GetDouble( int ordinal )
        => CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.GetDouble();

    public override IEnumerator GetEnumerator()
        => records.GetEnumerator();

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
    public override Type GetFieldType( int ordinal )
    {
        var kind = CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.ValueKind;

        // special type: datetime
        if
        (
            ( kind == JsonValueKind.String )
            &&
            DateTime.TryParse( CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.GetString(), out _ )
        )
        {
            return typeof( DateTime );
        }

        return kind switch
        {
            JsonValueKind.Undefined => typeof( object ),
            JsonValueKind.Object => typeof( object ),
            JsonValueKind.Array => typeof( object ),
            JsonValueKind.String => typeof( string ),
            JsonValueKind.Number => typeof( int ),
            JsonValueKind.True => typeof( bool ),
            JsonValueKind.False => typeof( bool ),
            JsonValueKind.Null => typeof( object ),
            _ => throw new FormatException( $"Unknown JSON value kind: {kind}" ),
        };
    }

    public override float GetFloat( int ordinal )
        => CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.GetSingle();

    public override Guid GetGuid( int ordinal )
        => CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.GetGuid();

    public override short GetInt16( int ordinal )
        => CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.GetInt16();

    public override int GetInt32( int ordinal )
        => CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.GetInt32();

    public override long GetInt64( int ordinal )
        => CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.GetInt64();

    public override string GetName( int ordinal )
        => CurrentRecord.EnumerateObject().ElementAt( ordinal ).Name;

    public override int GetOrdinal( string name )
        => CurrentRecord.EnumerateObject()
            .Select( ( x, i ) => new { x.Name, i } )
            .First( x => x.Name == name )
            .i;

    public override string GetString( int ordinal )
        => CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.GetString()
            ?? throw new InvalidCastException( "Cannot cast null to string." );

    public override object GetValue( int ordinal )
        => CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.GetValue()
            ?? DBNull.Value;

    public override int GetValues( object[] values )
    {
        var array = CurrentRecord.EnumerateObject()
            .Select( x => x.Value.GetValue() ?? DBNull.Value )
            .ToArray();

        array.CopyTo( values, 0 );

        return array.Length;
    }

    public override bool IsDBNull( int ordinal )
        => CurrentRecord.EnumerateObject().ElementAt( ordinal ).Value.ValueKind == JsonValueKind.Null;

    public override bool NextResult()
    {
        return ( false );
    }

    public override bool Read()
    {
        if ( currentRecordIndex < records.Count() - 1 )
        {
            currentRecordIndex++;

            return ( true );
        }

        return ( false );
    }
}
