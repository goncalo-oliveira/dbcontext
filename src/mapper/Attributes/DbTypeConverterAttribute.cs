#pragma warning disable IDE0130
namespace System.Data.Mapper;
#pragma warning restore IDE0130

/// <summary>
/// When placed on a type or property, specifies the converter type to use
/// </summary>
/// <param name="converterType"></param>
[AttributeUsage( AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Property )]
public sealed class DbTypeConverterAttribute( Type converterType ) : Attribute
{
    /// <summary>
    /// Gets the converter type to use
    /// </summary>
    public Type ConverterType { get; } = converterType;
}
