using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data
{
    /// <summary>
    /// Provides an async adapter between IDbCommand and DbCommand
    /// </summary>
    internal class CommandAdapter
    {
        internal readonly Func<IDbCommand, CancellationToken, ValueTask<IDataReader>> ExecuteReaderAsync;
        internal readonly Func<IDbCommand, CommandBehavior, CancellationToken, ValueTask<IDataReader>> ExecuteReaderBehaviorAsync;

        internal CommandAdapter( Type type )
        {
            if ( type.GetRuntimeMethod( "ExecuteReaderAsync", new[] { typeof( CancellationToken ) } ) != null )
            {
                ExecuteReaderAsync = async ( command, token ) =>
                {
                    dynamic cmd = command;

                    return await cmd.ExecuteReaderAsync( token );
                };
            }
            else
            {
                ExecuteReaderAsync = async ( command, token ) => await Task.Run( () =>
                {
                    return command.ExecuteReader();
                } );
            }

            if ( type.GetRuntimeMethod( "ExecuteReaderAsync", new[] { typeof( CommandBehavior ), typeof( CancellationToken ) } ) != null )
            {
                ExecuteReaderBehaviorAsync = async ( command, behavior, token ) =>
                {
                    dynamic cmd = command;

                    return await cmd.ExecuteReaderAsync( behavior, token );
                };
            }
            else
            {
                ExecuteReaderBehaviorAsync = async ( command, behavior, token ) => await Task.Run( () =>
                {
                    return command.ExecuteReader( behavior );
                } );
            }
        }
    }
}
