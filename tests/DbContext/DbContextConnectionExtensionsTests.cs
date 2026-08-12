using System.Data;

namespace tests;

public class DbContextConnectionExtensionsTests
{
    [Fact]
    public void UseConnectionRethrowsWithoutErrorHandler()
    {
        var context = new FakeDbContext();
        var expected = new InvalidOperationException( "failed" );

        var actual = Assert.Throws<InvalidOperationException>( () =>
            context.UseConnection( _ => throw expected )
        );

        Assert.Same( expected, actual );
    }

    [Fact]
    public void UseConnectionInvokesErrorHandler()
    {
        var context = new FakeDbContext();
        var expected = new InvalidOperationException( "failed" );
        Exception? handled = null;

        context.UseConnection( _ => throw expected, ex => handled = ex );

        Assert.Same( expected, handled );
    }

    [Fact]
    public async Task UseConnectionAsyncPropagatesCancellation()
    {
        var context = new FakeDbContext();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>( () =>
            context.UseConnectionAsync(
                ( _, cancellationToken ) => Task.FromCanceled( cancellationToken ),
                _ => throw new InvalidOperationException( "Cancellation must not be handled as an error." ),
                cts.Token
            )
        );
    }
}
