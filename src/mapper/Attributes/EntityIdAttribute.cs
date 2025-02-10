#pragma warning disable IDE0130
namespace System.Data.Mapper;
#pragma warning restore IDE0130

/// <summary>
/// Marks a property as the entity's identifier. The property must be either a <see cref="string"/> or an <see cref="EntityId"/>.
/// </summary>
[AttributeUsage( AttributeTargets.Property )]
public sealed class EntityIdAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the reference of the entity identifier. If null, the property is assumed to be the entity identifier.
    /// </summary>
    public string? Ref { get; set; }

    /// <summary>
    /// Gets the identifier type from the specified entity.
    /// </summary>
    /// <param name="entity">The object to get the identifier type from</param>
    /// <returns>The identifier type</returns>
    public static Type? GetType( object entity )
        => EntityCache.GetEntityInfo( entity.GetType() ).IdProperty?.PropertyType;

    /// <summary>
    /// Gets the identifier value from the specified entity.
    /// </summary>
    /// <param name="entity">The object to get the identifier value from</param>
    /// <returns>The identifier value</returns>
    public static object? GetValue( object entity )
    {
        var property = EntityCache.GetEntityInfo( entity.GetType() ).IdProperty
            ?? throw new InvalidOperationException( "The entity does not have an identifier property." );

        return property.GetValue( entity );
    }

    /// <summary>
    /// Tries to get the identifier value from the specified entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="value"></param>
    /// <returns>True if the identifier value was retrieved and not null; false otherwise</returns>
    public static bool TryGetValue( object entity, out object? value )
    {
        try
        {
            value = GetValue( entity );

            return value is not null;
        }
        catch
        {
            value = null;

            return false;
        }
    }
}
