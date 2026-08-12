using System.Globalization;

namespace System.Data.Mapper;

internal static class DbValueConverter
{
    public static Type? GetConversionType( Type fieldType, Type propertyType )
    {
        var targetType = Nullable.GetUnderlyingType( propertyType ) ?? propertyType;
        return targetType.IsAssignableFrom( fieldType ) ? null : targetType;
    }

    public static object Convert( object value, Type targetType )
    {
        if ( targetType.IsInstanceOfType( value ) )
        {
            return value;
        }

        try
        {
            if ( targetType.IsEnum )
            {
                return value is string text
                    ? Enum.Parse( targetType, text, ignoreCase: true )
                    : Enum.ToObject( targetType, System.Convert.ChangeType(
                        value,
                        Enum.GetUnderlyingType( targetType ),
                        CultureInfo.InvariantCulture
                    )! );
            }

            if ( targetType == typeof( Guid ) )
            {
                return value switch
                {
                    string text => Guid.Parse( text ),
                    byte[] bytes => new Guid( bytes ),
                    _ => throw new InvalidCastException()
                };
            }

            if ( targetType == typeof( DateTime ) && value is string dateTime )
            {
                return DateTime.Parse( dateTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind );
            }

            if ( targetType == typeof( DateTimeOffset ) )
            {
                return value switch
                {
                    string text => DateTimeOffset.Parse( text, CultureInfo.InvariantCulture, DateTimeStyles.None ),
                    DateTime date => new DateTimeOffset( date ),
                    _ => throw new InvalidCastException()
                };
            }

            if ( targetType == typeof( DateOnly ) )
            {
                return value switch
                {
                    string text => DateOnly.Parse( text, CultureInfo.InvariantCulture ),
                    DateTime date => DateOnly.FromDateTime( date ),
                    _ => throw new InvalidCastException()
                };
            }

            if ( targetType == typeof( TimeOnly ) )
            {
                return value switch
                {
                    string text => TimeOnly.Parse( text, CultureInfo.InvariantCulture ),
                    DateTime date => TimeOnly.FromDateTime( date ),
                    TimeSpan time => TimeOnly.FromTimeSpan( time ),
                    _ => throw new InvalidCastException()
                };
            }

            if ( targetType == typeof( TimeSpan ) && value is string timeSpan )
            {
                return TimeSpan.Parse( timeSpan, CultureInfo.InvariantCulture );
            }

            return System.Convert.ChangeType( value, targetType, CultureInfo.InvariantCulture )
                ?? throw new InvalidCastException();
        }
        catch ( Exception exception ) when ( exception is InvalidCastException or FormatException or OverflowException or ArgumentException )
        {
            throw new InvalidCastException(
                $"Cannot map a database value of type '{value.GetType()}' to '{targetType}'. "
                + "Configure a DbTypeConverter for custom conversions.",
                exception
            );
        }
    }
}
