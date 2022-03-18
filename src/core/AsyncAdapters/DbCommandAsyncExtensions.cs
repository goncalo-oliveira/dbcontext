using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data
{
    public static class DbCommandAsyncExtensions
    {
        private static readonly ConcurrentDictionary<Type, CommandAdapter> CommandAdapters =
            new ConcurrentDictionary<Type, CommandAdapter>();

        public static async Task<int> ExecuteNonQueryAsync( this IDbCommand command
            , CancellationToken cancellationToken = default( CancellationToken ) )
        {
            if ( command == null )
            {
                throw new ArgumentNullException( nameof( command ) );
            }

            var dbCommand = command as DbCommand;

            if ( dbCommand != null )
            {
                return await dbCommand.ExecuteNonQueryAsync( cancellationToken );
            }

            var adapter = CommandAdapters.GetOrAdd(
                command.GetType(),
                type => new CommandAdapter( type )
            );

            return await adapter.ExecuteNonQueryAsync( command, cancellationToken );
        }

        public static async Task<object> ExecuteScalarAsync( this IDbCommand command
            , CancellationToken cancellationToken = default( CancellationToken ) )
        {
            if ( command == null )
            {
                throw new ArgumentNullException( nameof( command ) );
            }

            var dbCommand = command as DbCommand;

            if ( dbCommand != null )
            {
                return await dbCommand.ExecuteScalarAsync( cancellationToken );
            }

            var adapter = CommandAdapters.GetOrAdd(
                command.GetType(),
                type => new CommandAdapter( type )
            );

            return await adapter.ExecuteScalarAsync( command, cancellationToken );
        }

        public static async Task<IDataReader> ExecuteReaderAsync( this IDbCommand command
            , CancellationToken cancellationToken = default( CancellationToken ) )
        {
            if ( command == null )
            {
                throw new ArgumentNullException( nameof( command ) );
            }

            var dbCommand = command as DbCommand;

            if ( dbCommand != null )
            {
                return await dbCommand.ExecuteReaderAsync( cancellationToken );
            }

            var adapter = CommandAdapters.GetOrAdd(
                command.GetType(),
                type => new CommandAdapter( type )
            );

            return await adapter.ExecuteReaderAsync( command, cancellationToken );
        }

        public static async Task<IDataReader> ExecuteReaderAsync( this IDbCommand command
            , CommandBehavior commandBehavior
            , CancellationToken cancellationToken = default( CancellationToken ) )
        {
            if ( command == null )
            {
                throw new ArgumentNullException( nameof( command ) );
            }

            var dbCommand = command as DbCommand;

            if ( dbCommand != null )
            {
                return await dbCommand.ExecuteReaderAsync( commandBehavior, cancellationToken );
            }

            var adapter = CommandAdapters.GetOrAdd(
                command.GetType(),
                type => new CommandAdapter( type )
            );

            return await adapter.ExecuteReaderBehaviorAsync( command, commandBehavior, cancellationToken );
        }
    }
}
