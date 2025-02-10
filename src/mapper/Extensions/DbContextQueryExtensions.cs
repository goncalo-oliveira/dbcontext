using System.Data;
using System.Data.Common;
using System.Linq.Expressions;

#pragma warning disable IDE0130
namespace BTelematics.Data;
#pragma warning restore IDE0130

/// <summary>
/// Provides extension methods for executing queries on a <see cref="IDbContext"/>.
/// </summary>
public static class DbContextQueryExtensions
{
    /// <summary>
    /// Executes a query and returns the results as an array of objects.
    /// </summary>
    /// <typeparam name="T">The type to map the results to</typeparam>
    /// <param name="selector">An expression to select the columns to return</param>
    /// <param name="where">An expression to filter the results</param>
    /// <param name="limit">The maximum number of results to return</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An array of objects of type <typeparamref name="T"/></returns>
    public static async Task<T[]> QueryAsync<T>( this IDbContext context, Expression<Func<T, object>>? selector = null, Expression<Func<T, bool>>? where = null, int? limit = null, CancellationToken cancellationToken = default ) where T : notnull, new()
    {
        using DbConnection connection = await context.OpenAsync( cancellationToken );

        return await connection.QueryAsync( selector, where, limit, cancellationToken );
    }

    /// <summary>
    /// Executes a query and returns a single result as an object.
    /// </summary>
    /// <typeparam name="T">The type to map the results to</typeparam>
    /// <param name="selector">An expression to select the columns to return</param>
    /// <param name="where">An expression to filter the results</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An object of type <typeparamref name="T"/> or null</returns>
    public static async Task<T?> QuerySingleAsync<T>( this IDbContext context, Expression<Func<T, bool>>? where, Expression<Func<T, object>>? selector = null, CancellationToken cancellationToken = default ) where T : notnull, new()
    {
        using DbConnection connection = await context.OpenAsync( cancellationToken );

        return await connection.QuerySingleAsync( where, selector, cancellationToken );
    }

    /// <summary>
    /// Executes a query and returns a single result as an object.
    /// </summary>
    /// <typeparam name="T">The type to map the results to</typeparam>
    /// <param name="entityId">The ID of the entity to query</param>
    /// <param name="selector">An expression to select the columns to return</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An object of type <typeparamref name="T"/> or null</returns>
    /// <exception cref="InvalidOperationException">Thrown when the entity does not have a property marked with the <see cref="EntityIdAttribute"/>.</exception>
    public static async Task<T?> QuerySingleAsync<T>( this IDbContext context, object entityId, Expression<Func<T, object>>? selector = null, CancellationToken cancellationToken = default ) where T : notnull, new()
    {
        using DbConnection connection = await context.OpenAsync( cancellationToken );

        return await connection.QuerySingleAsync( entityId, selector, cancellationToken );
    }
}
