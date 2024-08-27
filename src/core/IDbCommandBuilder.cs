namespace System.Data;

/// <summary>
/// A fluent DbCommand builder
/// </summary>
public interface IDbCommandBuilder
{
    /// <summary>
    /// Sets the command type (default is CommandType.Text)
    /// </summary>
    IDbCommandBuilder SetType( CommandType commandType );

    /// <summary>
    /// Sets the command text
    /// </summary>
    IDbCommandBuilder SetText( string commandText );

    /// <summary>
    /// Sets the time (in seconds) to wait for the command to execute. The default value is 30 seconds.
    /// </summary>
    IDbCommandBuilder SetTimeout( int commandTimeout );

    /// <summary>
    /// Sets the time (in seconds) to wait for the command to execute. The default value is 30 seconds.
    /// </summary>
    IDbCommandBuilder SetTimeout( TimeSpan commandTimeout );

    /// <summary>
    /// Adds a parameter to the command
    /// </summary>
    /// <param name="configure">The action to configure the parameter instance</param>
    IDbCommandBuilder AddParameter( Action<IDbDataParameter> configure );

    /// <summary>
    /// Adds a parameter to the command
    /// </summary>
    /// <param name="name">The parameter name</param>
    /// <param name="value">The parameter value</param>
    IDbCommandBuilder AddParameter( string name, object? value );

    /// <summary>
    /// Creates the DbCommand instance
    /// </summary>
    Common.DbCommand Build();
}
