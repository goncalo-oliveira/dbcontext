using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Mapper;

internal sealed class PropertyMetadata
{
    public PropertyMetadata( PropertyInfo propertyInfo, DbNamingPolicy namingPolicy )
    {
        PropertyType = propertyInfo.PropertyType;
        PropertyName = propertyInfo.Name;
        CanWrite = propertyInfo.CanWrite;        
        CustomAttributes = propertyInfo.GetCustomAttributes().ToArray();

        /*
        If the property has a ColumnAttribute, use the name specified in the attribute.
        Otherwise, use the property name and apply the naming convention.
        */
        ColumnName = GetCustomAttribute<ColumnAttribute>()?.Name
            ?? namingPolicy.ConvertName( propertyInfo.Name );

        IsEntityId = GetCustomAttribute<EntityIdAttribute>() is {} attr && attr.Ref is null;

        /*
        If the property has a DbTypeConverterAttribute, use the converter type specified in the attribute.
        */
        DbTypeConverter = GetCustomAttribute<DbTypeConverterAttribute>()?.ConverterType;

        /*
        Otherwise, if the property type has a DbTypeConverterAttribute, use the converter type specified in the attribute.
        */
        if ( DbTypeConverter is null )
        {
            // if propertyType is nullable, get the underlying type
            var propertyType = ( propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof( Nullable<> ) )
                ? propertyInfo.PropertyType.GetGenericArguments()[0]
                : propertyInfo.PropertyType;

            DbTypeConverter = propertyType.GetCustomAttribute<DbTypeConverterAttribute>()?.ConverterType;
        }

        /*
        Compile the setter and getter expressions.
        */
        var instance = Expression.Parameter( typeof(object), "instance" );
        var value = Expression.Parameter( typeof(object), "value" );

        var castInstance = Expression.Convert(instance, propertyInfo.DeclaringType!);
        var castValue = Expression.Convert(value, propertyInfo.PropertyType);

        var propertyAccess = Expression.Property( castInstance, propertyInfo );
        
        SetValue = Expression.Lambda<Action<object, object?>>(
            Expression.Assign( propertyAccess, castValue ),
            instance,
            value
        )
        .Compile();
        
        GetValue = Expression.Lambda<Func<object, object?>>(
            Expression.Convert(propertyAccess, typeof( object ) ), // Convert return type to object
            instance
        )
        .Compile();
    }

    public Type PropertyType { get; }
    public string PropertyName  { get; }
    public Attribute[] CustomAttributes { get; }
    public string ColumnName { get; }
    public bool IsEntityId { get; }
    public bool CanWrite { get; }
    public Type? DbTypeConverter { get; }

    public T? GetCustomAttribute<T>() where T : Attribute
        => CustomAttributes.OfType<T>().SingleOrDefault();

    public Func<object, object?> GetValue { get; }

    public Action<object, object?> SetValue { get; }

    public override string ToString()
        => $"{PropertyType} {PropertyName}";
}
