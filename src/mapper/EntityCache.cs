using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Mapper;

/// <summary>
/// A cache for entity properties. It is used to avoid reflection overhead.
/// <para>Although it was thought for entities, it can be used for any class type.</para>
/// </summary>
internal static class EntityCache
{
    private static readonly ConcurrentDictionary<Type, EntityMetadata> cachedEntities = new();
    private static readonly ConcurrentDictionary<Type, DbTypeConverter> cachedConverters = new();

    public static DbTypeConverter GetDbTypeConverter( Type converterType )
        => cachedConverters.GetOrAdd( converterType, static type =>
        {
            var converter = (DbTypeConverter?)Activator.CreateInstance( type )
                ?? throw new InvalidOperationException( $"The converter type '{type}' must have a parameterless constructor." );

            return converter;
        } );

    public static T CreateInstance<T>() where T : notnull, new()
        => (T)GetEntityInfo<T>().Factory();

    public static EntityMetadata GetEntityInfo<T>()
        => GetEntityInfo( typeof( T ) );

    public static EntityMetadata GetEntityInfo( Type type )
        => cachedEntities.GetOrAdd( type, static t =>
        {
            var entityAttribute = t.GetCustomAttribute<EntityAttribute>();
            var namingPolicy = entityAttribute?.GetNamingPolicy() ?? DbNamingPolicy.SnakeCasePolicy;
            var tableName = entityAttribute?.TableName ?? EntityTypeName.GetValue( t, namingPolicy );

            // Get all properties only once
            var allProperties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var declaredProperties = t.GetProperties( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly );
            var declaredKeys = declaredProperties.Select( p => p.Name ).ToHashSet();

            var properties = allProperties
                .Where( p => !declaredKeys.Contains( p.Name ) || declaredProperties.Contains( p ) )
                .Select( p => new PropertyMetadata( p, namingPolicy ) )
                .ToArray();

            return new EntityMetadata
            {
                TableName = tableName,
                NamingPolicy = namingPolicy,
                Properties = new( properties ),
                Factory = CreateFactory( t )
            };
        } );

    public static PropertyMetadata? GetProperty( Type type, string propertyName )
        => GetEntityInfo( type ).Properties.TryGetProperty( propertyName, out var property )
            ? property
            : null;

    /// <summary>
    /// Gets the table name of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object</typeparam>
    /// <returns>The entity type name</returns>
    public static string GetEntityTypeName<T>()
        => GetEntityTypeName( typeof( T ) );

    /// <summary>
    /// Gets the entity type name of the specified type.
    /// </summary>
    /// <param name="type">The type of the object</param>
    /// <returns>The entity type name</returns>
    public static string GetEntityTypeName( Type type )
        => GetEntityInfo( type ).TableName;

    private static Func<object> CreateFactory( Type type )
    {
        if ( type.IsValueType || type.GetConstructor( Type.EmptyTypes ) is not null )
        {
            // Use "new()" for types with a default constructor
            return Expression.Lambda<Func<object>>( Expression.New( type ) ).Compile();
        }

        throw new InvalidOperationException( $"Type '{type.FullName}' must have a parameterless constructor." );
    }
}
