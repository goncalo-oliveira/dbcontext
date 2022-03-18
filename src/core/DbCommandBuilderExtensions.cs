using System;

namespace System.Data
{
    public static class DbCommandBuilderExtensions
    {
        public static IDbCommandBuilder BuildCommand( this IDbContext context )
            => new DbCommandBuilder( context.GetDbConnection() );

        public static IDbCommandBuilder BuildCommand( this IDbConnection connection )
            => new DbCommandBuilder( connection );
    }
}
