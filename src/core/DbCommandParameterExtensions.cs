namespace System.Data
{
    public static class DbCommandParameterExtensions
    {
        public static IDbDataParameter AddParameter( this IDbCommand command )
        {
            var p = command.CreateParameter();

            command.Parameters.Add( p );

            return ( p );
        }

        public static IDbDataParameter AddParameterWithValue( this IDbCommand command, string name, object value )
        {
            var p = command.CreateParameter();

            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;

            command.Parameters.Add( p );

            return ( p );
        }

        public static IDbDataParameter AddParameter( this IDbCommand command, string name, DbType dbType )
        {
            var p = command.CreateParameter();

            p.ParameterName = name;
            p.DbType = dbType;

            command.Parameters.Add( p );

            return ( p );
        }

        public static IDbDataParameter AddParameter( this IDbCommand command, string name, DbType dbType, int size )
        {
            var p = command.CreateParameter();

            p.ParameterName = name;
            p.DbType = dbType;
            p.Size = size;

            command.Parameters.Add( p );

            return ( p );
        }

        public static IDbDataParameter AddParameter( this IDbCommand command, string name, DbType dbType, int size, string sourceColumn )
        {
            var p = command.CreateParameter();

            p.ParameterName = name;
            p.DbType = dbType;
            p.Size = size;
            p.SourceColumn = sourceColumn;

            command.Parameters.Add( p );

            return ( p );
        }
    }
}
