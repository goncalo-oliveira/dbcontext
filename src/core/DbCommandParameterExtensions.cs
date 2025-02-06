using System.Data.Common;

namespace System.Data;

public static class DbCommandParameterExtensions
{
    /// <summary>
    /// Adds a parameter to the command
    /// </summary>
    public static DbParameter AddParameter( this DbCommand command )
    {
        var p = command.CreateParameter();

        command.Parameters.Add( p );

        return p;
    }

    /// <summary>
    /// Adds a parameter with the given name and value to the command
    /// </summary>
    /// <param name="name">The parameter name</param>
    /// <param name="value">The parameter value</param>
    public static DbParameter AddParameterWithValue( this DbCommand command, string name, object? value )
    {
        /*
        In SQL Server, NULL is an INT by default in all of the scenarios I can think of.
        So, if the value is null, we can't infer the type from the value.
        In that case, we can just use the default type of INT.
        */
        var valueType = value?.GetType() ?? typeof( int );
        var p = command.CreateParameter();

        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        p.DbType = valueType switch
        {
            Type t when t == typeof(DBNull) => DbType.Int32,
            Type t when t == typeof(string) => DbType.String,
            Type t when t == typeof(bool) => DbType.Boolean,
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
            Type t when t == typeof(byte[]) => DbType.Binary,
            _ => throw new NotSupportedException($"Unsupported parameter type: {value?.GetType() ?? null}"),
        };

        command.Parameters.Add( p );

        return p;
    }

    /// <summary>
    /// Adds a parameter with the given name and type to the command
    /// </summary>
    /// <param name="name">The parameter name</param>
    /// <param name="dbType">The parameter type</param>
    public static DbParameter AddParameter( this DbCommand command, string name, DbType dbType )
    {
        var p = command.CreateParameter();

        p.ParameterName = name;
        p.DbType = dbType;

        command.Parameters.Add( p );

        return p;
    }

    /// <summary>
    /// Adds a parameter with the given name and type to the command
    /// </summary>
    /// <param name="name">The parameter name</param>
    /// <param name="dbType">The parameter type</param>
    /// <param name="size">The parameter size</param>
    public static DbParameter AddParameter( this DbCommand command, string name, DbType dbType, int size )
    {
        var p = command.CreateParameter();

        p.ParameterName = name;
        p.DbType = dbType;
        p.Size = size;

        command.Parameters.Add( p );

        return p;
    }

    /// <summary>
    /// Adds a parameter with the given name and type to the command
    /// </summary>
    /// <param name="name">The parameter name</param>
    /// <param name="dbType">The parameter type</param>
    /// <param name="size">The parameter size</param>
    /// <param name="sourceColumn">The parameter source column</param>
    public static DbParameter AddParameter( this DbCommand command, string name, DbType dbType, int size, string sourceColumn )
    {
        var p = command.CreateParameter();

        p.ParameterName = name;
        p.DbType = dbType;
        p.Size = size;
        p.SourceColumn = sourceColumn;

        command.Parameters.Add( p );

        return p;
    }
}
