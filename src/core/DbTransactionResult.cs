namespace System.Data;

/// <summary>
/// Represents the result of a <see cref="DbTransaction"/> operation.
/// </summary>
public readonly struct DbTransactionResult
{

    /// <summary>
    /// Gets whether the transaction operation succeeded.
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Gets the exception that occurred during the transaction operation, if not succeeded.
    /// </summary>
    public Exception? Exception { get; }

    internal DbTransactionResult( bool succeeded, Exception? exception = null )
    {
        Succeeded = succeeded;
        Exception = exception;
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if the transaction operation did not succeed.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void ThrowIfFailed()
    {
        if ( !Succeeded )
        {
            throw new InvalidOperationException( "The transaction operation failed.", Exception );
        }
    }

    internal static DbTransactionResult Success { get; } = new( true );
}
