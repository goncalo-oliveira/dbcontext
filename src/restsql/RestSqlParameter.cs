using System.Data;
using System.Data.Common;

#pragma warning disable IDE0130
namespace Faactory.RestSql;
#pragma warning restore IDE0130

internal sealed class RestSqlParameter : DbParameter
{
    public RestSqlParameter()
    {
    }

    #pragma warning disable CS8765 // disable nullability of properties

    public override DbType DbType { get; set; }
    public override ParameterDirection Direction { get; set; } = ParameterDirection.Input;
    public override bool IsNullable { get; set; }
    public override string ParameterName { get; set; } = string.Empty;
    public override int Size { get; set; }
    public override string SourceColumn { get; set; } = string.Empty;
    public override bool SourceColumnNullMapping { get; set; }
    public override object? Value { get; set; }

    #pragma warning restore CS8765

    public string GetSqlType()
    {
        var size = Size > 0 ? $"({Size})" : "(MAX)";

        return ( DbType switch
        {
            DbType.AnsiString => $"varchar{size}",
            DbType.AnsiStringFixedLength => $"char{size}",
            DbType.Boolean => "bit",
            DbType.Binary => $"varbinary{size}",
            DbType.Byte => "tinyint",
            DbType.Currency => "money",
            DbType.Date => "date",
            DbType.DateTime => "datetime",
            DbType.DateTime2 => "datetime2",
            DbType.DateTimeOffset => "datetimeoffset",
            DbType.Decimal => "decimal",
            DbType.Double => "float",
            DbType.Guid => "uniqueidentifier",
            DbType.Int16 => "smallint",
            DbType.Int32 => "int",
            DbType.Int64 => "bigint",
            DbType.SByte => "tinyint",
            DbType.Single => "real",
            DbType.String => $"nvarchar{size}",
            DbType.StringFixedLength => $"nchar{size}",
            DbType.Time => "time",
            DbType.UInt16 => "smallint",
            DbType.UInt32 => "int",
            DbType.UInt64 => "bigint",
            DbType.VarNumeric => "numeric",
            _ => throw new NotSupportedException( $"Unsupported DbType: {DbType}" )
        } );
    }

    public string GetSqlValue()
    {
        if ( ( Value == null ) || ( Value == DBNull.Value ) )
        {
            return "NULL";
        }

        return ( DbType switch
        {
            DbType.AnsiString => $"'{Value}'",
            DbType.AnsiStringFixedLength => $"'{Value}'",
            DbType.Boolean => ( (bool)Value ) ? "1" : "0",
            DbType.Byte => $"'{Value}'",
            DbType.Currency => $"'{Value}'",
            DbType.Date => $"'{Value}'",
            DbType.DateTime => $"'{(DateTime)Value:o}'",
            DbType.DateTime2 => $"'{(DateTime)Value:o}'",
            DbType.DateTimeOffset => $"'{(DateTimeOffset)Value:o}'",
            DbType.Decimal => $"{Value}",
            DbType.Double => $"{Value}",
            DbType.Guid => $"'{Value}'",
            DbType.Int16 => $"{Value}",
            DbType.Int32 => $"{Value}",
            DbType.Int64 => $"{Value}",
            DbType.SByte => $"{Value}",
            DbType.Single => $"{Value}",
            DbType.String => $"N'{Value}'",
            DbType.StringFixedLength => $"N'{Value}'",
            DbType.Time => $"'{Value}'",
            DbType.UInt16 => $"{Value}",
            DbType.UInt32 => $"{Value}",
            DbType.UInt64 => $"{Value}",
            DbType.VarNumeric => $"{Value}",
            _ => throw new NotSupportedException( $"Unsupported DbType: {DbType}" )
        } );
    }

    public override void ResetDbType()
    {
    }

    public static RestSqlParameter Create( string name, object? value, DbType? dbType = null )
    {
        var type = value?.GetType() ?? typeof( object );

        dbType ??= type switch
        {
            Type t when t == typeof(string) => DbType.String,
            Type t when t == typeof(bool) => DbType.Boolean,
            Type t when t == typeof(byte[]) => DbType.Binary,
            Type t when t == typeof(byte) => DbType.Byte,
            Type t when t == typeof(short) => DbType.Int16,
            Type t when t == typeof(int) => DbType.Int32,
            Type t when t == typeof(long) => DbType.Int64,
            Type t when t == typeof(float) => DbType.Single,
            Type t when t == typeof(double) => DbType.Double,
            Type t when t == typeof(decimal) => DbType.Decimal,
            Type t when t == typeof(DateTime) => DbType.DateTime2,
            Type t when t == typeof(DateTimeOffset) => DbType.DateTimeOffset,
            Type t when t == typeof(Guid) => DbType.Guid,
            _ => throw new NotSupportedException($"Unsupported parameter type: {type}"),
        };

        return new RestSqlParameter
        {
            ParameterName = name,
            Value = value,
            DbType = dbType.Value,
        };
    }
}
