using System.Data.Mapper.Sql;
using System.Text;

namespace tests;

public class DbSqlDialectTests
{
    [Theory]
    [InlineData( "Npgsql.NpgsqlConnection", "PostgreSQL", "\"sales\".\"order\"" )]
    [InlineData( "Microsoft.Data.Sqlite.SqliteConnection", "SQLite", "\"sales\".\"order\"" )]
    [InlineData( "Microsoft.Data.SqlClient.SqlConnection", "SQL Server", "[sales].[order]" )]
    public void ResolvesProviderAndQuotesIdentifiers( string providerName, string dialectName, string quotedIdentifier )
    {
        var dialect = DbSqlDialect.ForProviderName( providerName );

        Assert.Equal( dialectName, dialect.Name );
        Assert.Equal( quotedIdentifier, dialect.QuoteIdentifier( "sales.order" ) );
    }

    [Fact]
    public void PostgreSqlAndSqliteUseTrailingLimit()
    {
        foreach ( var dialect in new[] { DbSqlDialect.PostgreSql, DbSqlDialect.Sqlite } )
        {
            var sql = new StringBuilder( "SELECT *\n" );

            dialect.AppendTrailingLimit( sql, 10 );

            Assert.Equal( "SELECT *\nLIMIT 10\n", sql.ToString() );
        }
    }

    [Fact]
    public void SqlServerUsesTopLimit()
    {
        var sql = new StringBuilder( "SELECT\n" );

        DbSqlDialect.SqlServer.AppendSelectLimit( sql, 10 );
        DbSqlDialect.SqlServer.AppendTrailingLimit( sql, 10 );

        Assert.Equal( "SELECT\nTOP 10\n", sql.ToString() );
    }

    [Fact]
    public void UnknownProvidersAreRejected()
    {
        var exception = Assert.Throws<NotSupportedException>(
            () => DbSqlDialect.ForProviderName( "Example.UnknownConnection" )
        );

        Assert.Contains( "Generated SQL is not supported", exception.Message );
        Assert.Contains( "explicitly written SQL", exception.Message );
    }
}
