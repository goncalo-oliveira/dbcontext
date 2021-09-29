using System;
using System.Data;

namespace Npgsql
{
    public class NpgsqlDbContext : IDbContext
    {
        private readonly DbContextOptions options;

        public NpgsqlDbContext( DbContextOptions contextOptions )
        {
            options = contextOptions;
        }

        public DbContextProviderTypes Provider => DbContextProviderTypes.PostgreSql;

        public IDbConnection GetDbConnection()
            => new NpgsqlConnection( options.ConnectionString );
    }
}
