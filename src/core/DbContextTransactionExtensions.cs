using System;
using System.Transactions;

namespace System.Data
{
    public static class DbContextTransactionExtensions
    {
        /// <summary>
        /// Creates a transaction scope with the given options
        /// </summary>
        /// <param name="options">The transaction options to use if a new transaction is created</param>
        /// <returns>A System.Transactions.TransactionScope instance</returns>
        public static TransactionScope CreateTransactionScope( this IDbContext context, TransactionOptions options )
            => new TransactionScope( TransactionScopeOption.Required, options );

        /// <summary>
        /// Creates an async transaction scope with the given options
        /// </summary>
        /// <param name="options">The transaction options to use if a new transaction is created</param>
        /// <returns>A System.Transactions.TransactionScope instance</returns>
        public static TransactionScope CreateAsyncTransactionScope( this IDbContext context, TransactionOptions options )
            => new TransactionScope( TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled );

        /// <summary>
        /// Creates a transaction scope with the default options (IsolationLevel = ReadCommitted)
        /// </summary>
        /// <returns>A System.Transactions.TransactionScope instance</returns>
        public static TransactionScope CreateTransactionScope( this IDbContext context )
        {
            return CreateTransactionScope( context, new TransactionOptions()
            {
                IsolationLevel = Transactions.IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.MaximumTimeout
            } );
        }

        /// <summary>
        /// Creates a transaction scope with the default options (IsolationLevel = ReadCommitted)
        /// </summary>
        /// <returns>A System.Transactions.TransactionScope instance</returns>
        public static TransactionScope CreateAsyncTransactionScope( this IDbContext context )
        {
            return CreateAsyncTransactionScope( context, new TransactionOptions()
            {
                IsolationLevel = Transactions.IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.MaximumTimeout
            } );
        }
    }
}
