using System.Data.Common;
using System.Text;

namespace System.Data.Mapper.Sql;

internal abstract class DbSqlDialect
{
    public static DbSqlDialect ForConnection( DbConnection connection )
        => ForProviderName( connection.GetType().FullName ?? connection.GetType().Name );

    internal static DbSqlDialect ForProviderName( string providerName )
    {
        if ( providerName.Contains( "Npgsql", StringComparison.Ordinal ) )
        {
            return PostgreSql;
        }

        if ( providerName.Contains( "Sqlite", StringComparison.Ordinal ) )
        {
            return Sqlite;
        }

        if ( providerName.Contains( "SqlClient", StringComparison.Ordinal ) )
        {
            return SqlServer;
        }

        throw new NotSupportedException(
            $"Generated SQL is not supported for connection type '{providerName}'. "
            + "Use the mapping extensions with explicitly written SQL instead."
        );
    }

    public static DbSqlDialect PostgreSql { get; } = new DoubleQuoteDialect( "PostgreSQL" );
    public static DbSqlDialect Sqlite { get; } = new DoubleQuoteDialect( "SQLite" );
    public static DbSqlDialect SqlServer { get; } = new SqlServerDialect();

    public abstract string Name { get; }

    public string QuoteIdentifier( string identifier )
        => string.Join( ".", identifier.Split( '.' ).Select( QuoteIdentifierPart ) );

    protected abstract string QuoteIdentifierPart( string identifier );

    public virtual void AppendSelectLimit( StringBuilder sql, int? limit )
    {
    }

    public virtual void AppendTrailingLimit( StringBuilder sql, int? limit )
    {
        if ( limit > 0 )
        {
            sql.AppendLine( $"LIMIT {limit.Value}" );
        }
    }

    private sealed class DoubleQuoteDialect( string name ) : DbSqlDialect
    {
        public override string Name { get; } = name;

        protected override string QuoteIdentifierPart( string identifier )
            => $"\"{identifier.Replace( "\"", "\"\"" )}\"";
    }

    private sealed class SqlServerDialect : DbSqlDialect
    {
        public override string Name => "SQL Server";

        protected override string QuoteIdentifierPart( string identifier )
            => $"[{identifier.Replace( "]", "]]" )}]";

        public override void AppendSelectLimit( StringBuilder sql, int? limit )
        {
            if ( limit > 0 )
            {
                sql.AppendLine( $"TOP {limit.Value}" );
            }
        }

        public override void AppendTrailingLimit( StringBuilder sql, int? limit )
        {
        }
    }
}
