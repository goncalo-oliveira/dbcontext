using System;
using Npgsql;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NpgsqlDbContextProviderServiceExtensions
    {
        /// <summary>
        /// Adds a SqlServer context provider and provides a context builder
        /// </summary>
        public static IDbContextBuilder AddNpgsqlDbContextProvider( this IServiceCollection services )
            => services.AddDbContextProvider<NpgsqlDbContext>();
    }
}
