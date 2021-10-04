using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data
{
    /// <summary>
    /// Provides an async adapter between IDbTransaction and DbTransaction
    /// </summary>
    internal class TransactionAdapter
    {
        internal readonly Func<IDbTransaction, CancellationToken, Task> CommitAsync;
        internal readonly Func<IDbTransaction, CancellationToken, Task> RollbackAsync;

        internal TransactionAdapter( Type type )
        {
            if ( type.GetRuntimeMethod( "CommitAsync", new[] { typeof( CancellationToken ) } ) != null )
            {
                CommitAsync = async ( transaction, token ) =>
                {
                    dynamic cmd = transaction;

                    await cmd.CommitAsync( token );
                };
            }
            else
            {
                CommitAsync = async ( transaction, token ) => await Task.Run( () =>
                {
                    transaction.Commit();
                } );
            }

            if ( type.GetRuntimeMethod( "RollbackAsync", new[] { typeof( CancellationToken ) } ) != null )
            {
                RollbackAsync = async ( transaction, token ) =>
                {
                    dynamic cmd = transaction;

                    await cmd.RollbackAsync( token );
                };
            }
            else
            {
                RollbackAsync = async ( transaction, token ) => await Task.Run( () =>
                {
                    transaction.Rollback();
                } );
            }
        }
    }
}
