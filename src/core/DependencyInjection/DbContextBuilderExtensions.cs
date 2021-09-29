using System;
using System.Data;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DbContextBuilderExtensions
    {
        /// <summary>
        /// Adds a named DbContext
        /// </summary>
        /// <param name="name">The name of the DbContext</param>
        /// <param name="connectionString">The connection string</param>
        public static IDbContextBuilder AddDbContext( this IDbContextBuilder builder
            , string name
            , string connectionString )
        {
            builder.Services.Configure<DbContextFactoryOptions>( name, options =>
            {
                options.ContextType = builder.ContextType;
                options.ContextOptions = new DbContextOptions
                {
                    ConnectionString = connectionString
                };
            } );

            return ( builder );
        }
    }
}
