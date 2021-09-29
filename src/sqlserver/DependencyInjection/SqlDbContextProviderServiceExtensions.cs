using System;
using System.Data;
using System.Data.SqlClient;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SqlDbContextProviderServiceExtensions
    {
        /// <summary>
        /// Adds a SqlServer context provider and provides a context builder
        /// </summary>
        public static IDbContextBuilder AddSqlDbContextProvider( this IServiceCollection services )
            => services.AddDbContextProvider<SqlDbContext>();
    }
}
