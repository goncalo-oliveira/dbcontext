using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data
{
    public static class DbTransactionAsyncExtensions
    {
        private static readonly ConcurrentDictionary<Type, TransactionAdapter> TransactionAdapters =
            new ConcurrentDictionary<Type, TransactionAdapter>();

        /// <summary>
        /// Asynchronously commits the database transaction.
        /// </summary>
        public static Task CommitAsync( this IDbTransaction transaction, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            if ( transaction == null )
            {
                throw new ArgumentNullException( nameof( transaction ) );
            }

            var dbTransaction = transaction as DbTransaction;

            if ( dbTransaction != null )
            {
                return dbTransaction.CommitAsync( cancellationToken );
            }

            var adapter = TransactionAdapters.GetOrAdd(
                transaction.GetType(),
                type => new TransactionAdapter( type )
            );

            return adapter.CommitAsync( transaction, cancellationToken );
        }

        /// <summary>
        /// Asynchronously rolls back a transaction from a pending state
        /// </summary>
        public static Task RollbackAsync( this IDbTransaction transaction, CancellationToken cancellationToken = default( CancellationToken ) )
        {
            if ( transaction == null )
            {
                throw new ArgumentNullException( nameof( transaction ) );
            }

            var dbTransaction = transaction as DbTransaction;

            if ( dbTransaction != null )
            {
                return dbTransaction.RollbackAsync( cancellationToken );
            }

            var adapter = TransactionAdapters.GetOrAdd(
                transaction.GetType(),
                type => new TransactionAdapter( type )
            );

            return adapter.RollbackAsync( transaction, cancellationToken );
        }
    }
}
