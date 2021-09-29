using System;
using Microsoft.Extensions.DependencyInjection;

namespace System.Data
{
    internal class DbContextBuilder : IDbContextBuilder
    {
        public DbContextBuilder( Type contextType, IServiceCollection services )
        {
            ContextType = contextType;
            Services = services;
        }

        public Type ContextType { get; }

        public IServiceCollection Services { get; }
    }
}
