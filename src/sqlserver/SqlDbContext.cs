using System;
using System.Data;
using Microsoft.Extensions.Options;

namespace Microsoft.Data.SqlClient
{
    public class SqlDbContext : IDbContext
    {
        private readonly DbContextOptions options;

        public SqlDbContext( DbContextOptions contextOptions )
        {
            options = contextOptions;
        }

        public string Name => options.Name;

        public DbContextProviderTypes Provider => DbContextProviderTypes.SqlServer;

        public IDbConnection GetDbConnection()
            => new SqlConnection( options.ConnectionString );
    }
}
