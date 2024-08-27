using System.Data.Common;

namespace System.Data;

internal class DbCommandBuilder : IDbCommandBuilder
{
    private readonly DbCommand command;

    public DbCommandBuilder( DbConnection connection )
    {
        command = connection.CreateCommand();
    }

    public DbCommandBuilder( DbConnection connection, string commandText )
    {
        command = connection.CreateCommand();

        SetText( commandText );
    }

    public IDbCommandBuilder AddParameter( Action<IDbDataParameter> configure )
    {
        configure( command.AddParameter() );

        return this;
    }

    public IDbCommandBuilder AddParameter( string name, object value )
    {
        command.AddParameterWithValue( name, value );

        return this;
    }

    public IDbCommandBuilder SetText( string commandText )
    {
        command.CommandText = commandText;

        return this;
    }

    public IDbCommandBuilder SetTimeout( int commandTimeout )
    {
        command.CommandTimeout = commandTimeout;

        return this;
    }

    public IDbCommandBuilder SetTimeout( TimeSpan commandTimeout )
    {
        command.CommandTimeout = (int)commandTimeout.TotalSeconds;

        return this;
    }

    public IDbCommandBuilder SetType( CommandType commandType )
    {
        command.CommandType = commandType;

        return this;
    }

    public DbCommand Build() => command;
}
