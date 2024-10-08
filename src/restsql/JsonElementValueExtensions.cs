using System.Text.Json;

#pragma warning disable IDE0130
namespace Faactory.RestSql;
#pragma warning restore IDE0130

internal static class JsonElementValueExtensions
{
    /// <summary>
    /// Converts a <see cref="JsonElement"/> value to a CLR value.
    /// </summary>
    public static object? GetValue( this JsonElement jsonElement )
    {
        if ( jsonElement.IsOfKind( JsonValueKind.Undefined ) || jsonElement.IsOfKind( JsonValueKind.Null ) )
        {
            return null;
        }

        // numbers: double, int32, int64
        if ( jsonElement.IsOfKind( JsonValueKind.Number ) )
        {
            if ( jsonElement.TryGetDouble( out var doubleValue ) )
            {
                return doubleValue;
            }

            long longValue = jsonElement.GetInt64();

            // if it fits inside an int32, return an int32
            if ( longValue >= int.MinValue && longValue <= int.MaxValue )
            {
                return (int)longValue;
            }

            return longValue;
        }

        // datetime
        if ( jsonElement.IsOfKind( JsonValueKind.String ) && jsonElement.TryGetDateTime( out var dateTime ) )
        {
            return dateTime;
        }

        switch ( jsonElement.ValueKind )
        {
            case JsonValueKind.String:
                return jsonElement.GetString();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            default:
                return jsonElement;
        }
    }

    public static bool IsOfKind( this JsonElement jsonElement, JsonValueKind kind )
        => jsonElement.ValueKind == kind;
}
