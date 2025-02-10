using System.Text;

#pragma warning disable IDE0130
namespace System.Data.Mapper;
#pragma warning restore IDE0130

/// <summary>
/// Snake case naming policy for database objects.
/// </summary>
internal sealed class DbSnakeCaseNamingPolicy : DbNamingPolicy
{
    public override string ConvertName( string name )
    {
        var sb = new StringBuilder();
        var first = true;

        foreach ( var c in name )
        {
            if ( char.IsUpper( c ) )
            {
                if ( !first )
                {
                    sb.Append( '_' );
                }

                sb.Append( char.ToLower( c ) );
            }
            else
            {
                sb.Append( c );
            }

            first = false;
        }

        return sb.ToString();
    }
}
