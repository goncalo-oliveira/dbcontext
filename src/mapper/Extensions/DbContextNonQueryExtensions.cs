using System.Data;
using System.Data.Common;
using System.Data.Mapper;
using System.Linq.Expressions;

#pragma warning disable IDE0130
namespace BTelematics.Data;
#pragma warning restore IDE0130

/// <summary>
/// Extension methods for <see cref="IDbContext"/> that perform non-query operations.
/// </summary>
public static class DbContextNonQueryExtensions
{
    /// <summary>
    /// Executes an INSERT statement.
    /// </summary>
    /// <typeparam name="T">The type to map the statement from</typeparam>
    /// <param name="entity">An object containing the values to insert</param>
    /// <param name="selector">An expression to select the columns to insert</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The number of rows affected</returns>
    public static async Task<int> InsertAsync<T>( this IDbContext context, T entity, Expression<Func<T, object>>? selector = null, CancellationToken cancellationToken = default ) where T : notnull, new()
    {
        using DbConnection connection = await context.OpenAsync( cancellationToken );

        return await connection.InsertAsync( entity, selector, cancellationToken );
    }

    /// <summary>
    /// Executes an UPDATE statement.
    /// </summary>
    /// <typeparam name="T">The type to map the statement from</typeparam>
    /// <param name="entity">An object containing the values to update</param>
    /// <param name="selector">An expression to select the columns to update</param>
    /// <param name="where">An expression to filter the rows to update</param>
    /// <param name="coalesce">Whether to use COALESCE to update only non-null values</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The number of rows affected</returns>
    /// <exception cref="InvalidOperationException">Thrown when the entity does not have a property marked with the <see cref="EntityIdAttribute"/>.</exception>
    public static async Task<int> UpdateAsync<T>( this IDbContext context, T entity, Expression<Func<T, object>>? selector = null, Expression<Func<T, bool>>? where = null, bool coalesce = false, CancellationToken cancellationToken = default ) where T : notnull, new()
    {
        using DbConnection connection = await context.OpenAsync( cancellationToken );

        return await connection.UpdateAsync( entity, selector, where, coalesce, cancellationToken );
    }

    /// <summary>
    /// Executes a DELETE statement.
    /// </summary>
    /// <typeparam name="T">The type to map the statement from</typeparam>
    /// <param name="where">An expression to filter the rows to delete</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The number of rows affected</returns>
    public static async Task<int> DeleteAsync<T>( this IDbContext context, Expression<Func<T, bool>> where, CancellationToken cancellationToken = default ) where T : notnull, new()
    {
        using DbConnection connection = await context.OpenAsync( cancellationToken );

        return await connection.DeleteAsync( where, cancellationToken );
    }

    /// <summary>
    /// Executes a DELETE statement.
    /// </summary>
    /// <typeparam name="T">The type to map the statement from</typeparam>
    /// <param name="entityId">The ID of the entity to delete</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The number of rows affected</returns>
    /// <exception cref="InvalidOperationException">Thrown when the entity does not have a property marked with the <see cref="EntityIdAttribute"/>.</exception>
    public static async Task<int> DeleteAsync<T>( this IDbContext context, object entityId, CancellationToken cancellationToken = default ) where T : notnull, new()
    {
        using DbConnection connection = await context.OpenAsync( cancellationToken );

        return await connection.DeleteAsync<T>( entityId, cancellationToken );
    }

    /// <summary>
    /// Executes a DELETE statement.
    /// </summary>
    /// <typeparam name="T">The type to map the statement from</typeparam>
    /// <param name="entity">An object containing the ID of the entity to delete</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The number of rows affected</returns>
    /// <exception cref="InvalidOperationException">Thrown when the entity does not have a property marked with the <see cref="EntityIdAttribute"/>.</exception>
    public static Task<int> DeleteAsync<T>( this IDbContext context, T entity, CancellationToken cancellationToken = default ) where T : notnull, new()
    {
        var entityIdProperty = EntityCache.GetEntityInfo<T>().IdProperty
            ?? throw new InvalidOperationException( "Entity does not have an ID." );

        var entityId = entityIdProperty.GetValue( entity )
            ?? throw new InvalidOperationException( "Entity ID is null." );

        return context.DeleteAsync<T>( entityId, cancellationToken );
    }
}
