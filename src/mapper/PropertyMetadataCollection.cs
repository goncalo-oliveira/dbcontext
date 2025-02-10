using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace System.Data.Mapper;

internal sealed class PropertyMetadataCollection : IEnumerable<KeyValuePair<string, PropertyMetadata>>
{
    private readonly Dictionary<string, PropertyMetadata> properties;
    private readonly Dictionary<string, PropertyMetadata> columns;

    public PropertyMetadataCollection( IEnumerable<PropertyMetadata> metadata )
    {
        properties = new( StringComparer.OrdinalIgnoreCase );
        columns = new( StringComparer.OrdinalIgnoreCase );

        foreach ( var property in metadata )
        {
            properties.Add( property.PropertyName, property );
            columns.Add( property.ColumnName, property );

            if ( property.IsEntityId )
            {
                if ( IdProperty is not null )
                {
                    throw new InvalidOperationException( "Only one property can be marked with the EntityIdAttribute." );
                }

                IdProperty = property;
            }
        }

        Values = properties.Values.ToArray();
    }

    public PropertyMetadata? IdProperty { get; init; }

    public PropertyMetadata[] Values { get; init; }

    public bool TryGetProperty( string propertyName, [NotNullWhen( true )] out PropertyMetadata? property )
        => properties.TryGetValue( propertyName, out property );

    public bool TryGetColumn( string columnName, [NotNullWhen( true )] out PropertyMetadata? property )
        => columns.TryGetValue( columnName, out property );

    public PropertyMetadata[] FromExpression<T>( Expression<Func<T, object>> selector )
    {
        if ( selector.Body is not NewExpression newExpression || newExpression.Members == null || newExpression.Members.Count == 0 )
        {
            throw new ArgumentException(
                "Selector cannot be empty. Specify at least one property, e.g., x => new { x.Property1 }.",
                nameof( selector )
            );
        }

        return newExpression.Members
            .Select( m => TryGetProperty( m.Name, out var property ) ? property : null )
            .Where(p => p != null)
            .Select( p => p! )
            .ToArray();
    }

    public IEnumerator<KeyValuePair<string, PropertyMetadata>> GetEnumerator()
        => properties.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
