using System.Data.Common;

namespace System.Data.Mapper;

/// <summary>
/// A base class for converting database types to .NET types and vice versa.
/// </summary>
public abstract class DbTypeConverter
{
    /// <summary>
    /// Reads a value from the database and converts it to a .NET type.
    /// </summary>
    /// <param name="reader">The database reader.</param>
    /// <param name="ordinal">The ordinal of the column.</param>
    /// <param name="propertyType">The target type of the property.</param>
    /// <returns>The converted value.</returns>
    public abstract object? Read( DbDataReader reader, int ordinal, Type propertyType );

    /// <summary>
    /// Writes a value to a database parameter.
    /// </summary>
    /// <param name="parameter">The database parameter.</param>
    /// <param name="value">The value to write.</param>
    public abstract void Write( IDbDataParameter parameter, object? value );
}
