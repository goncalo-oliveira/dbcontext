using System.Data.Mapper;

#pragma warning disable IDE0130
namespace System.Data;
#pragma warning restore IDE0130

internal static class DbCommandBuilderExtensions
{
    /// <summary>
    /// Adds a parameter to the command.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    public static IDbCommandBuilder AddParameterValue( this IDbCommandBuilder builder, string name, DbParameterValue value )
    {
        builder.AddParameter( p =>
        {
            p.ParameterName = name;

            // set the DbType if it is specified
            if ( value.DbType != null )
            {
                p.DbType = value.DbType.Value;
            }

            if ( value.Converter != null )
            {
                // write the value through the specified converter
                EntityCache.GetDbTypeConverter( value.Converter )
                    .Write( p, value.Value );
            }
            else
            {
                // write the value directly
                p.Value = value.Value ?? DBNull.Value;
            }
        } );

        return builder;
    }
}
