using System.Data;
using System.Data.Common;
using System.Data.Mapper;
using System.Data.Mapper.Expressions;
using System.Linq.Expressions;
using System.Text;

#pragma warning disable IDE0130
namespace System.Data;
#pragma warning restore IDE0130

/// <summary>
/// Provides extension methods for executing queries on a <see cref="DbConnection"/>.
/// </summary>
public static class DbConnectionQueryExtensions
{
    /// <summary>
    /// Determines if the connection is a PostgreSQL connection.
    /// </summary>
    internal static bool IsPostgres( this DbConnection connection )
        => connection.GetType().FullName!.Contains( "Npgsql" );

    /// <summary>
    /// Executes a query and returns the results as an array of objects.
    /// </summary>
    /// <typeparam name="T">The type to map the results to</typeparam>
    /// <param name="selector">An expression to select the columns to return</param>
    /// <param name="where">An expression to filter the results</param>
    /// <param name="limit">The maximum number of results to return</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An array of objects of type <typeparamref name="T"/></returns>
    public static async Task<T[]> QueryAsync<T>( this DbConnection connection, Expression<Func<T, object>>? selector = null, Expression<Func<T, bool>>? where = null, int? limit = null, CancellationToken cancellationToken = default ) where T : notnull, new()
    {
        var sql = new StringBuilder();

        var entityInfo = EntityCache.GetEntityInfo<T>();
        var tableName = entityInfo.TableName;
        var properties = selector != null
            ? entityInfo.Properties.FromExpression( selector )
            : entityInfo.Properties.Values;

        sql.AppendLine( $"SELECT" );

        if ( limit > 0 && !connection.IsPostgres() )
        {
            sql.AppendLine( $"TOP {limit}" );
        }

        foreach ( var ( property, idx ) in properties.Select( ( property, idx ) => ( property, idx ) ) )
        {
            sql.Append( $"    {property.ColumnName}" );

            if ( idx < properties.Length - 1 )
            {
                sql.Append( ',' );
            }

            sql.AppendLine();
        }

        sql.AppendLine( $"FROM {tableName}" );

        var whereClause = where != null
            ? DbExpressionVisitor.GetWhereClause( where )
            : null;

        if ( whereClause != null )
        {
            sql.AppendLine( "WHERE" )
                .AppendLine( $"    {whereClause}" );
        }

        if ( limit > 0 && connection.IsPostgres() )
        {
            sql.AppendLine( $"LIMIT {limit}" );
        }

        return await connection.ExecuteQueryAsync<T>(
            sql.ToString(),
            b =>
            {
                if ( whereClause != null )
                {
                    foreach ( var ( name, value ) in whereClause.Parameters )
                    {
                        if ( value is DbParameterValue pValue )
                        {
                            b.AddParameterValue( name, pValue );
                        }
                        else
                        {
                            b.AddParameter( name, value );
                        }
                    }
                }
            },
            cancellationToken
        );
    }

    /// <summary>
    /// Executes a query and returns a single result as an object.
    /// </summary>
    /// <typeparam name="T">The type to map the results to</typeparam>
    /// <param name="selector">An expression to select the columns to return</param>
    /// <param name="where">An expression to filter the results</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An object of type <typeparamref name="T"/> or null</returns>
    public static async Task<T?> QuerySingleAsync<T>( this DbConnection connection, Expression<Func<T, bool>>? where, Expression<Func<T, object>>? selector = null, CancellationToken cancellationToken = default ) where T : notnull, new()
    {
        var results = await connection.QueryAsync(
            selector: selector,
            where: where,
            limit: 1,
            cancellationToken: cancellationToken
        );

        return results.FirstOrDefault();
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
    public static async Task<T?> QuerySingleAsync<T>( this DbConnection connection, object entityId, Expression<Func<T, object>>? selector = null, CancellationToken cancellationToken = default )  where T : notnull, new()
    {
        var entityIdProperty = EntityCache.GetEntityInfo<T>().IdProperty
            ?? throw new InvalidOperationException( "Entity does not have an ID." );

        // Build the dynamic where clause
        var entityParam = Expression.Parameter( typeof(T), "e" ); // e =>
        var entityIdPropertyExpr = Expression.Property( entityParam, entityIdProperty.PropertyName ); // e.Id
        var entityIdConstantExpr = Expression.Constant( entityId, entityIdProperty.PropertyType ); // value
        var equalityExpr = Expression.Equal( entityIdPropertyExpr, entityIdConstantExpr ); // e.Id == entityId
        var whereClause = Expression.Lambda<Func<T, bool>>( equalityExpr, entityParam ); // e => e.Id == entityId

        var result = await connection.QueryAsync<T>(
            limit: 1,
            where: whereClause,
            selector: selector,
            cancellationToken: cancellationToken
        );

        return result.FirstOrDefault();
    }
}
