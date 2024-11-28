using System.Data.Common;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Data;
#pragma warning restore IDE0130

public static class DbCommandBuilderExtensions
{
    /// <summary>
    /// Creates a command builder
    /// </summary>
    [Obsolete( "Use BuildCommand from DbConnection instead. This method will be removed in future versions." )]
    public static IDbCommandBuilder BuildCommand( this IDbContext context )
        => new DbCommandBuilder( context.GetDbConnection() );

    /// <summary>
    /// Creates a command builder
    /// </summary>
    /// <param name="commandText">The command text to initialize the builder with</param>
    [Obsolete( "Use BuildCommand from DbConnection instead. This method will be removed in future versions." )]
    public static IDbCommandBuilder BuildCommand( this IDbContext context, string commandText )
        => new DbCommandBuilder( context.GetDbConnection(), commandText );

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
}
