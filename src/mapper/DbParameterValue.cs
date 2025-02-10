namespace System.Data.Mapper;

/// <summary>
/// Represents a parameter value for a database query or command.
/// </summary>
internal sealed class DbParameterValue
{
    /// <summary>
    /// The converter to use when writing the value to a DbParameter.
    /// </summary>
    public Type? Converter { get; init; }

    /// <summary>
    /// An optional DbType to use when writing the value to a DbParameter.
    /// <para>This value might be overwritten by the converter, if one is specified.</para>
    /// </summary>
    public DbType? DbType { get; init; }

    /// <summary>
    /// The value of the parameter.
    /// </summary>
    public object? Value { get; init; }
}
