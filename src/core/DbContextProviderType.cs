namespace System.Data;

/// <summary>
/// Identifies the database provider used by a context.
/// </summary>
public enum DbContextProviderType
{
    /// <summary>A custom provider.</summary>
    Custom = 0,
    /// <summary>PostgreSQL.</summary>
    PostgreSql,
    /// <summary>SQL Server through restSQL.</summary>
    RestSql,
    /// <summary>SQLite.</summary>
    Sqlite,
    /// <summary>SQL Server.</summary>
    SqlServer,
}
