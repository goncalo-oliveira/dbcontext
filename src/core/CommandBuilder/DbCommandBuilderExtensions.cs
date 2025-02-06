using System.Data.Common;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Data;
#pragma warning restore IDE0130

public static class DbCommandBuilderExtensions
{
    /// <summary>
    /// Creates a command builder
    /// </summary>
    public static IDbCommandBuilder BuildCommand( this DbConnection connection )
        => new DbCommandBuilder( connection );

    /// <summary>
    /// Creates a command builder
    /// </summary>
    /// <param name="commandText">The command text to initialize the builder with</param>
    public static IDbCommandBuilder BuildCommand( this DbConnection connection, string commandText )
        => new DbCommandBuilder( connection, commandText );

    /// <summary>
    /// Adds a parameter to the command
    /// </summary>
    /// <param name="name">The parameter name</param>
    /// <param name="value">The parameter value</param>
    /// <param name="dbType">The parameter type</param>
    public static IDbCommandBuilder AddParameter( this IDbCommandBuilder builder, string name, object? value, DbType dbType )
        => builder.AddParameter( p =>
        {
            p.DbType = dbType;
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
        } );
}
