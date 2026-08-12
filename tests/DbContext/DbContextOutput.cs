using System.Data.Common;
using System.Text;

#pragma warning disable IDE0130
namespace tests;
#pragma warning restore IDE0130

public sealed class DbContextOutput
{
    private readonly StringBuilder output = new();

    public DbCommand? LastCommand { get; private set; }

    public void Log( string commandText, DbCommand? command = null )
    {
        LastCommand = command;

        if ( output.Length > 0 )
        {
            output.AppendLine( "---" );
        }

        output.AppendLine( commandText.TrimEnd( '\n' ) );
    }

    public override string ToString()
        => output.ToString().TrimEnd( '\n');
}
