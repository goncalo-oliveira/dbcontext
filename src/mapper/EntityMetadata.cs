namespace System.Data.Mapper;

/// <summary>
/// Metadata for an entity.
/// </summary>
internal sealed class EntityMetadata
{
    /// <summary>
    /// The name of the table in the database.
    /// </summary>
    public required string TableName { get; init; }

    public DbNamingPolicy NamingPolicy { get; init; } = DbNamingPolicy.SnakeCasePolicy;
    public required PropertyMetadataCollection Properties { get; init; }
    public required Func<object> Factory { get; init; }

    public PropertyMetadata? IdProperty => Properties.IdProperty;
}
