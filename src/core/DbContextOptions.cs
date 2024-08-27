namespace System.Data;

/// <summary>
/// Base class for DbContext options
/// </summary>
public class DbContextOptions
{
    /// <summary>
    /// The name of the context
    /// </summary>
    public string Name { get; set; } = DbContext.DefaultName;

    /// <summary>
    /// The connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}
