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

        public static Task OpenAsync( this IDbConnection connection )
        {
            if ( connection == null )
            {
                throw new ArgumentNullException( nameof( connection ) );
            }

            var dbConnection = connection as DbConnection;

            if ( dbConnection != null )
            {
                return dbConnection.OpenAsync();
            }

            var adapter = ConnectionAdapters.GetOrAdd( 
                connection.GetType(),
                type => new ConnectionAdapter( type ) );

            return adapter.OpenAsync( connection );
        }

        public static Task OpenAsync( this IDbConnection connection, CancellationToken token )
        {
            if ( connection == null )
            {
                throw new ArgumentNullException( nameof( connection ) );
            }

            var dbConnection = connection as DbConnection;

            if ( dbConnection != null )
            {
                return dbConnection.OpenAsync( token );
            }

            var adapter = ConnectionAdapters.GetOrAdd(
                connection.GetType(),
                type => new ConnectionAdapter( type ) );

            return adapter.OpenAsyncToken( connection, token );
        }
    }
}
