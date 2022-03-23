namespace System.Data
{
    public static class DbCommandParameterExtensions
    {
        /// <summary>
        /// Adds a parameter to the command
        /// </summary>
        public static IDbDataParameter AddParameter( this IDbCommand command )
        {
            var p = command.CreateParameter();

            command.Parameters.Add( p );

            return ( p );
        }

        /// <summary>
        /// Adds a parameter with the given name and value to the command
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="value">The parameter value</param>
        public static IDbDataParameter AddParameterWithValue( this IDbCommand command, string name, object value )
        {
            var p = command.CreateParameter();

            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;

            command.Parameters.Add( p );

            return ( p );
        }

        /// <summary>
        /// Adds a parameter with the given name and type to the command
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="dbType">The parameter type</param>
        public static IDbDataParameter AddParameter( this IDbCommand command, string name, DbType dbType )
        {
            var p = command.CreateParameter();

            p.ParameterName = name;
            p.DbType = dbType;

            command.Parameters.Add( p );

            return ( p );
        }

        /// <summary>
        /// Adds a parameter with the given name and type to the command
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="dbType">The parameter type</param>
        /// <param name="size">The parameter size</param>
        public static IDbDataParameter AddParameter( this IDbCommand command, string name, DbType dbType, int size )
        {
            var p = command.CreateParameter();

            p.ParameterName = name;
            p.DbType = dbType;
            p.Size = size;

            command.Parameters.Add( p );

            return ( p );
        }

        /// <summary>
        /// Adds a parameter with the given name and type to the command
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="dbType">The parameter type</param>
        /// <param name="size">The parameter size</param>
        /// <param name="sourceColumn">The parameter source column</param>
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
