#pragma warning disable IDE0130
namespace System.Data.Mapper;
#pragma warning restore IDE0130

public enum EntityNamingPolicy
{
    LowerCase,
    SnakeCase,
}

internal abstract class DbNamingPolicy
{
    public static readonly DbNamingPolicy LowerCasePolicy = new DbLowerCaseNamingPolicy();
    public static readonly DbNamingPolicy SnakeCasePolicy = new DbSnakeCaseNamingPolicy();

    public abstract string ConvertName( string name );
}
