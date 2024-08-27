using Microsoft.Extensions.DependencyInjection;

#pragma warning disable IDE0130
namespace System.Data;
#pragma warning restore IDE0130

internal class DbContextBuilder( Type contextType, IServiceCollection services ) : IDbContextBuilder
{
    public Type ContextType { get; } = contextType;

    public IServiceCollection Services { get; } = services;
}
