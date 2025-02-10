namespace System.Data.Mapper;

/// <summary>
/// Provides the entity type name based on the class name.
/// <para>Uses a pluralization rule to convert the class name to the entity type name.</para>
/// </summary>
internal static class EntityTypeName
{
    public static string GetValue( Type type, DbNamingPolicy namingPolicy )
    {
        var typeName = namingPolicy.ConvertName( type.Name );

        /*
        The entity type is usually a pluralized version of the class name.
        The entity type is always lower case.
        e.g. "Example" -> "examples"
             "Driver" -> "drivers"
             "Bus" -> "buses"
             "Butterfly" -> "butterflies"
             "Wolf" -> "wolves"

        Some cases might not follow the rule above, in which case the EntityAttribute should be used.
        e.g. "Person" -> "people"
             "Child" -> "children"

        */

        // if the type name ends with "y", replace it with "ies"; e.g. "city" -> "cities"
        // the letter before the "y" must be a consonant
        if ( typeName.EndsWith( 'y' ) )
        {
            var lastChar = typeName[^2];
            if ( lastChar != 'a' && lastChar != 'e' && lastChar != 'i' && lastChar != 'o' && lastChar != 'u' )
            {
                typeName = $"{typeName[..^1]}ies";
            }
        }
        // if the type name ends with "s" or "x", add "es"; e.g. "bus" -> "buses", "fox" -> "foxes"
        // this only applies if the letter before the "s" or "x" is a vowel
        else if ( typeName.EndsWith( 's' ) || typeName.EndsWith( 'x' ) )
        {
            var lastChar = typeName[^2];
            if ( lastChar == 'a' || lastChar == 'e' || lastChar == 'i' || lastChar == 'o' || lastChar == 'u' )
            {
                typeName = $"{typeName}es";
            }
        }
        // if the type name ends with "ch", "sh", "ss", or "zz", add "es"; e.g. "church" -> "churches", "glass" -> "glasses"
        else if ( typeName.EndsWith( "ch" ) || typeName.EndsWith( "sh" ) || typeName.EndsWith( "ss" ) || typeName.EndsWith( "zz" ) )
        {
            typeName = $"{typeName}es";
        }

        // if the type name ends with "f" or "fe", replace it with "ves"; e.g. "wolf" -> "wolves", "knife" -> "knives"
        else if ( typeName.EndsWith( 'f' ) || typeName.EndsWith( "fe" ) )
        {
            typeName = $"{typeName[..^1]}ves";
        }
        else
        {
            typeName = $"{typeName}s";
        }

        return typeName;
    }
}
