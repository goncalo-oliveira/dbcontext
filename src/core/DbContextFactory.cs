using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace System.Data
{
    internal class DbContextFactory : IDbContextFactory
    {
        private readonly IOptionsMonitor<DbContextFactoryOptions> optionsAccessor;
        private readonly IServiceProvider provider;

        public DbContextFactory( IServiceProvider serviceProvider
            , IOptionsMonitor<DbContextFactoryOptions> optionsMonitor )
        {
            provider = serviceProvider;
            optionsAccessor = optionsMonitor;
        }

        public IDbContext GetDbContext( string name )
        {
            var factoryOptions = optionsAccessor.Get( name );

            var context = (IDbContext)Activator.CreateInstance( factoryOptions.ContextType
                , new object[]
                {
                    factoryOptions.ContextOptions
                } );

            return ( context );
        }
    }
}
