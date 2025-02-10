using System.Text;

#pragma warning disable IDE0130
namespace System.Data.Mapper;
#pragma warning restore IDE0130

/// <summary>
/// Lower case naming policy for database objects.
/// </summary>
internal sealed class DbLowerCaseNamingPolicy : DbNamingPolicy
{
    public override string ConvertName( string name )
        => name.ToLower();
}
