using System;
using Microsoft.Extensions.Options;

namespace System.Data.SqlClient
{
    public class SqlDbContext : IDbContext
    {
        private readonly DbContextOptions options;

        public SqlDbContext( DbContextOptions contextOptions )
        {
            options = contextOptions;
        }

        public DbContextProviderTypes Provider => DbContextProviderTypes.SqlServer;

        public IDbConnection GetDbConnection()
            => new SqlConnection( options.ConnectionString );
    }
}
