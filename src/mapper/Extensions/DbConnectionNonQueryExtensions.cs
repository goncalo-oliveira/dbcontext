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
/// Provides extension methods for executing non-query operations on a <see cref="DbConnection"/>.
/// </summary>
public static class DbConnectionNonQueryExtensions
{
    /// <summary>
    /// Executes an INSERT statement.
    /// </summary>
    /// <typeparam name="T">The type to map the statement from</typeparam>
    /// <param name="entity">An object containing the values to insert</param>
    /// <param name="selector">An expression to select the columns to insert</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The number of rows affected</returns>
    public static async Task<int> InsertAsync<T>( this DbConnection connection, T entity, Expression<Func<T, object>>? selector = null, CancellationToken cancellationToken = default )  where T : notnull, new()
    {
        var sql = new StringBuilder();

        var entityInfo = EntityCache.GetEntityInfo<T>();
        var tableName = entityInfo.TableName;
        var properties = selector != null
            ? entityInfo.Properties.FromExpression( selector )
            : entityInfo.Properties.Values;

        sql.AppendLine( $"INSERT INTO {tableName} (" );

        foreach (var ( property, idx ) in properties.Select( ( property, idx ) => ( property, idx ) ) )
        {
            sql.Append( $"    {property.ColumnName}" );

            if ( idx < properties.Length - 1 )
            {
                sql.Append( ',' );
            }

            sql.AppendLine();
        }

        sql.AppendLine(
            """
            )
            VALUES (
            """ );

        foreach (var ( property, idx ) in properties.Select( ( property, idx ) => ( property, idx ) ) )
        {
            sql.Append( $"    @{property.ColumnName}" );

            if ( idx < properties.Length - 1 )
            {
                sql.Append( ',' );
            }

            sql.AppendLine();
        }

        sql.AppendLine( ")" );

        return await connection.ExecuteNonQueryAsync(
            sql.ToString(),
            b =>
            {
                foreach( var property in properties )
                {
                    var value = property.GetValue( entity );

                    if ( property.DbTypeConverter is not null && value is not DbParameterValue )
                    {
                        b.AddParameterValue( property.ColumnName, new DbParameterValue
                        {
                            Converter = property.DbTypeConverter,
                            Value = value
                        } );
                    }
                    else
                    {
                        b.AddParameter( property.ColumnName, value, GetDbType( property.PropertyType ) );
                    }
                }
            },
            cancellationToken
        );
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
    public static async Task<int> UpdateAsync<T>( this DbConnection connection, T entity, Expression<Func<T, object>>? selector = null, Expression<Func<T, bool>>? where = null, bool coalesce = false, CancellationToken cancellationToken = default ) where T : notnull, new()
    {
        var sql = new StringBuilder();
        var entityInfo = EntityCache.GetEntityInfo<T>();
        var tableName = entityInfo.TableName;
        var properties = selector != null
            ? entityInfo.Properties.FromExpression( selector )
            : entityInfo.Properties.Values;

        sql.AppendLine($"UPDATE {tableName} SET");

        // Generate SET clause
        foreach ( var ( property, idx ) in properties.Select( ( property, idx ) => ( property, idx ) ) )
        {
            if ( property.IsEntityId )
            {
                continue;
            }

            if ( coalesce )
            {
                sql.Append( $"    {property.ColumnName} = COALESCE( @{property.ColumnName}, {property.ColumnName} )" );
            }
            else
            {
                sql.Append( $"    {property.ColumnName} = @{property.ColumnName}" );
            }

            if ( idx < properties.Length - 1 )
            {
                sql.Append( ',' );
            }

            sql.AppendLine();
        }

        // Generate WHERE clause
        var whereClause = where != null
            ? DbExpressionVisitor.GetWhereClause( where )
            : null;

        if ( whereClause != null )
        {
            sql.AppendLine( "WHERE" );
            sql.AppendLine( $"    {whereClause}" );
        }
        else
        {
            var entityIdColumnName = entityInfo.IdProperty?.ColumnName
                ?? throw new InvalidOperationException( "Entity does not have an ID." );

            sql.AppendLine( $"WHERE {entityIdColumnName} = @p_{entityIdColumnName}" );
        }

        // Execute the query
        return await connection.ExecuteNonQueryAsync(
            sql.ToString(),
            b =>
            {
                // Add parameters for SET clause
                foreach ( var property in properties )
                {
                    var value = property.GetValue( entity );

                    if ( property.DbTypeConverter is not null )
                    {
                        b.AddParameterValue( property.ColumnName, new DbParameterValue
                        {
                            Converter = property.DbTypeConverter,
                            Value = value
                        } );
                    }
                    else
                    {
                        b.AddParameter( property.ColumnName, value, GetDbType( property.PropertyType ) );
                    }
                }

                // Add parameters for WHERE clause if it exists
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
                else
                {
                    var entityIdProperty = entityInfo.IdProperty
                        ?? throw new InvalidOperationException( "Entity does not have an ID." );

                    var parameterName = entityIdProperty.ColumnName;
                    var parameterValue = entityIdProperty.GetValue( entity );

                    if ( entityIdProperty.DbTypeConverter is not null )
                    {
                        b.AddParameterValue( parameterName, new DbParameterValue
                        {
                            Converter = entityIdProperty.DbTypeConverter,
                            Value = parameterValue
                        } );
                    }
                    else
                    {
                        b.AddParameter( parameterName, parameterValue );
                    }
                }
            },
            cancellationToken
        );
    }

    /// <summary>
    /// Executes a DELETE statement.
    /// </summary>
    /// <typeparam name="T">The type to map the statement from</typeparam>
    /// <param name="where">An expression to filter the rows to delete</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The number of rows affected</returns>
    public static async Task<int> DeleteAsync<T>( this DbConnection connection, Expression<Func<T, bool>> where, CancellationToken cancellationToken = default ) where T : notnull, new()
    {
        var sql = new StringBuilder();

        var entityType = EntityCache.GetEntityTypeName<T>();

        sql.AppendLine( $"DELETE FROM {entityType}" );

        var whereClause = DbExpressionVisitor.GetWhereClause( where );

        sql.AppendLine( $"WHERE {whereClause}" );

        return await connection.ExecuteNonQueryAsync(
            sql.ToString(),
            b =>
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
            },
            cancellationToken
        );
    }

    /// <summary>
    /// Executes a DELETE statement.
    /// </summary>
    /// <typeparam name="T">The type to map the statement from</typeparam>
    /// <param name="entityId">The ID of the entity to delete</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The number of rows affected</returns>
    /// <exception cref="InvalidOperationException">Thrown when the entity does not have a property marked with the <see cref="EntityIdAttribute"/>.</exception>
    public static async Task<int> DeleteAsync<T>( this DbConnection connection, object entityId, CancellationToken cancellationToken = default ) where T : notnull, new()
    {
        var sql = new StringBuilder();

        var entityInfo = EntityCache.GetEntityInfo<T>();
        var tableName = entityInfo.TableName;

        var entityIdProperty = entityInfo.IdProperty
            ?? throw new InvalidOperationException( "Entity does not have an ID." );

        var entityIdColumnName = entityIdProperty.ColumnName;

        sql.AppendLine( $"DELETE FROM {tableName}" );
        sql.AppendLine( $"WHERE {entityIdColumnName} = @p_{entityIdColumnName}" );

        return await connection.ExecuteNonQueryAsync(
            sql.ToString(),
            b =>
            {
                if ( entityIdProperty.DbTypeConverter is not null )
                {
                    b.AddParameterValue( $"p_{entityIdColumnName}", new DbParameterValue
                    {
                        Converter = entityIdProperty.DbTypeConverter,
                        Value = entityId
                    } );
                }
                else
                {
                    b.AddParameter( $"p_{entityIdColumnName}", entityId );
                }
            },
            cancellationToken
        );
    }

    /// <summary>
    /// Executes a DELETE statement.
    /// </summary>
    /// <typeparam name="T">The type to map the statement from</typeparam>
    /// <param name="entity">An object containing the ID of the entity to delete</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The number of rows affected</returns>
    /// <exception cref="InvalidOperationException">Thrown when the entity does not have a property marked with the <see cref="EntityIdAttribute"/>.</exception>
    public static Task<int> DeleteAsync<T>( this DbConnection connection, T entity, CancellationToken cancellationToken = default ) where T : notnull, new()
    {
        var entityIdProperty = EntityCache.GetEntityInfo<T>().IdProperty
            ?? throw new InvalidOperationException( "Entity does not have an ID." );

        var entityId = entityIdProperty.GetValue( entity )
            ?? throw new InvalidOperationException( "Entity ID is null." );

        return connection.DeleteAsync<T>( entityId, cancellationToken );
    }

    /// <summary>
    /// Gets the <see cref="DbType"/> for a given <see cref="Type"/>.
    /// </summary>
    private static DbType GetDbType( Type type )
    {
        return type switch {
            Type t when t == typeof( string ) => DbType.String,
            Type t when t == typeof( int ) => DbType.Int32,
            Type t when t == typeof( long ) => DbType.Int64,
            Type t when t == typeof( short ) => DbType.Int16,
            Type t when t == typeof( byte ) => DbType.Byte,
            Type t when t == typeof( bool ) => DbType.Boolean,
            Type t when t == typeof( DateTime ) => DbType.DateTime,
            Type t when t == typeof( DateTimeOffset ) => DbType.DateTimeOffset,
            Type t when t == typeof( decimal ) => DbType.Decimal,
            Type t when t == typeof( double ) => DbType.Double,
            Type t when t == typeof( float ) => DbType.Single,
            Type t when t == typeof( Guid ) => DbType.Guid,
            Type t when t == typeof( byte[] ) => DbType.Binary,
            _ => DbType.Object
        };
    }
}
