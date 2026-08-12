#pragma warning disable IDE0130
namespace System.Data.Mapper;
#pragma warning restore IDE0130

/// <summary>
/// Specifies how entity and property names are converted to database identifiers.
/// </summary>
public enum EntityNamingPolicy
{
    /// <summary>Converts names to lowercase.</summary>
    LowerCase,
    /// <summary>Converts names to snake case.</summary>
    SnakeCase,
}

internal abstract class DbNamingPolicy
{
    public static readonly DbNamingPolicy LowerCasePolicy = new DbLowerCaseNamingPolicy();
    public static readonly DbNamingPolicy SnakeCasePolicy = new DbSnakeCaseNamingPolicy();

    public abstract string ConvertName( string name );
}
