using Npgsql;
using NpgsqlTypes;

namespace System.Data;

public static class NpgsqlDbCommandBuilderExtensions
{
    /// <summary>
    /// Adds a parameter to the command
    /// </summary>
    /// <param name="name">The parameter name</param>
    /// <param name="value">The parameter value</param>
    /// <param name="npgsqlDbType">The parameter type</param>
    public static IDbCommandBuilder AddParameter( this IDbCommandBuilder builder, string name, object? value, NpgsqlDbType npgsqlDbType )
        => builder.AddParameter( p =>
        {
            if ( p is not NpgsqlParameter np )
            {
                throw new ArgumentException( "The parameter must be an NpgsqlParameter", nameof( p ) );
            }

            np.NpgsqlDbType = npgsqlDbType;
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
        } );
}
