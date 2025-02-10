#pragma warning disable IDE0130
namespace System.Data.Mapper;
#pragma warning restore IDE0130

/// <summary>
/// Marks a property as an entity. Although not required, it can be used when the name of the entity is different from the name of the class.
/// </summary>
[AttributeUsage( AttributeTargets.Class )]
public sealed class EntityAttribute( string? tableName = null ) : Attribute
{
    /// <summary>
    /// Gets the name of the table that the entity type is mapped to.
    /// </summary>
    public string? TableName { get; } = tableName;

    /// <summary>
    /// Gets the naming policy used for the entity properties. The default is <see cref="DbNamingPolicy.SnakeCase"/>.
    /// </summary>
    public EntityNamingPolicy NamingPolicy { get; init; } = EntityNamingPolicy.SnakeCase;

    internal DbNamingPolicy GetNamingPolicy()
        => NamingPolicy switch
        {
            EntityNamingPolicy.SnakeCase => DbNamingPolicy.SnakeCasePolicy,
            EntityNamingPolicy.LowerCase => DbNamingPolicy.LowerCasePolicy,
            _ => throw new InvalidOperationException( $"The naming policy '{NamingPolicy}' is not supported." )
        };
}


[Entity( NamingPolicy = EntityNamingPolicy.LowerCase)]
public class Dummy
{
}
