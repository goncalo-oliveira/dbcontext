using System;

namespace System.Data
{
    public interface IDbCommandBuilder
    {
        IDbCommandBuilder SetType( CommandType commandType );
        IDbCommandBuilder SetText( string commandText );
        IDbCommandBuilder SetTimeout( int commandTimeout );
        IDbCommandBuilder AddParameter( Action<IDbDataParameter> configure );
        IDbCommandBuilder AddParameter( string name, object value );
        IDbCommand Build();
    }
}
