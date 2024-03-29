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

        public string Name => options.Name;

        public DbContextProviderTypes Provider => DbContextProviderTypes.PostgreSql;

        public System.Data.Common.DbConnection GetDbConnection()
            => new NpgsqlConnection( options.ConnectionString );
    }
}
