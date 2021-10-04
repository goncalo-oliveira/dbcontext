using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data
{
    internal static class DbConnectionAsyncExtensions
    {
        private static readonly ConcurrentDictionary<Type, ConnectionAdapter> ConnectionAdapters =
            new ConcurrentDictionary<Type, ConnectionAdapter>();

        public static Task OpenAsync( this IDbConnection connection, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            if ( connection == null )
            {
                throw new ArgumentNullException( nameof( connection ) );
            }

            var dbConnection = connection as DbConnection;

            if ( dbConnection != null )
            {
                return dbConnection.OpenAsync( cancellationToken );
            }

            var adapter = ConnectionAdapters.GetOrAdd(
                connection.GetType(),
                type => new ConnectionAdapter( type )
            );

            return adapter.OpenAsync( connection, cancellationToken );
        }

        public static async ValueTask<IDbTransaction> BeginTransactionAsync( this IDbConnection connection
            , CancellationToken cancellationToken = default( CancellationToken ) )
        {
            if ( connection == null )
            {
                throw new ArgumentNullException( nameof( connection ) );
            }

            var dbConnection = connection as DbConnection;

            if ( dbConnection != null )
            {
                return await dbConnection.BeginTransactionAsync( cancellationToken );
            }

            var adapter = ConnectionAdapters.GetOrAdd(
                connection.GetType(),
                type => new ConnectionAdapter( type )
            );

            return await adapter.BeginTransactionAsync( connection, cancellationToken );
        }

        public static async ValueTask<IDbTransaction> BeginTransactionAsync( this IDbConnection connection
            , IsolationLevel isolationLevel
            , CancellationToken cancellationToken = default( CancellationToken ) )
        {
            if ( connection == null )
            {
                throw new ArgumentNullException( nameof( connection ) );
            }

            var dbConnection = connection as DbConnection;

            if ( dbConnection != null )
            {
                return await dbConnection.BeginTransactionAsync( isolationLevel, cancellationToken );
            }

            var adapter = ConnectionAdapters.GetOrAdd(
                connection.GetType(),
                type => new ConnectionAdapter( type )
            );

            return await adapter.BeginTransactionIsolationAsync( connection, isolationLevel, cancellationToken );
        }
    }
}
