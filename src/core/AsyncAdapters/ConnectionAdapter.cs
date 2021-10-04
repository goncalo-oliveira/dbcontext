using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data
{
    /// <summary>
    /// Provides an async adapter between IDbConnection and DbConnection
    /// </summary>
    internal class ConnectionAdapter
    {
        internal readonly Func<IDbConnection, CancellationToken, Task> OpenAsync;
        internal readonly Func<IDbConnection, CancellationToken, Task<IDbTransaction>> BeginTransactionAsync;
        internal readonly Func<IDbConnection, IsolationLevel, CancellationToken, Task<IDbTransaction>> BeginTransactionIsolationAsync;

        internal ConnectionAdapter( Type type )
        {
            if ( type.GetRuntimeMethod( "OpenAsync", new[] { typeof( CancellationToken ) } ) != null )
            {
                OpenAsync = async ( connection, token ) =>
                {
                    dynamic cmd = connection;

                    await cmd.OpenAsync( token );
                };
            }
            else
            {
                OpenAsync = async ( connection, token ) => await Task.Run( () =>
                {
                    connection.Open();
                } );
            }

            if ( type.GetRuntimeMethod( "BeginTransactionAsync", new[] { typeof( CancellationToken ) } ) != null )
            {
                BeginTransactionAsync = async ( connection, token ) =>
                {
                    dynamic cmd = connection;

                    return await cmd.BeginTransactionAsync( token );
                };
            }
            else
            {
                BeginTransactionAsync = async ( connection, token ) => await Task.Run( () =>
                {
                    return connection.BeginTransaction();
                } );
            }

            if ( type.GetRuntimeMethod( "BeginTransactionAsync", new[] { typeof( IsolationLevel ), typeof( CancellationToken ) } ) != null )
            {
                BeginTransactionIsolationAsync = async ( connection, isolation, token ) =>
                {
                    dynamic cmd = connection;
                    
                    return await cmd.BeginTransactionAsync( isolation, token );
                };
            }
            else
            {
                BeginTransactionIsolationAsync = async ( connection, isolation, token ) => await Task.Run( () =>
                {
                    return connection.BeginTransaction( isolation );
                } );
            }
        }
    }
}
