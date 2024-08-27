#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

/// <summary>
/// The context builder interface
/// </summary>
public interface IDbContextBuilder
{
    /// <summary>
    /// The context type
    /// </summary>
    Type ContextType { get; }

    /// <summary>
    /// The collection of service descriptors
    /// </summary>
    IServiceCollection Services { get; }
}
