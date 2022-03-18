using System;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data
{
    internal class DbCommandBuilder : IDbCommandBuilder
    {
        private readonly IDbCommand command;

        public DbCommandBuilder( IDbConnection connection )
        {
            command = connection.CreateCommand();
        }

        public IDbCommandBuilder AddParameter( Action<IDbDataParameter> configure )
        {
            configure( command.AddParameter() );

            return ( this );
        }

        public IDbCommandBuilder AddParameter( string name, object value )
        {
            command.AddParameterWithValue( name, value );

            return ( this );
        }

        public IDbCommandBuilder SetText( string commandText )
        {
            command.CommandText = commandText;

            return ( this );
        }

        public IDbCommandBuilder SetTimeout( int commandTimeout )
        {
            command.CommandTimeout = commandTimeout;

            return ( this );
        }

        public IDbCommandBuilder SetType( CommandType commandType )
        {
            command.CommandType = commandType;

            return ( this );
        }

        public IDbCommand Build()
        {
            return ( command );
        }
    }
}
