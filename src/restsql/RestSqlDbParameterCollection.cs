using System.Collections;
using System.Data.Common;

#pragma warning disable IDE0130
namespace Faactory.RestSql;
#pragma warning restore IDE0130

internal sealed class RestSqlDbParameterCollection : DbParameterCollection
{
    private List<RestSqlParameter> items = new();

    internal bool IsDirty { get; private set; }

    public override int Count => items.Count;

    public override object SyncRoot => ((ICollection)InnerList).SyncRoot;

    public void Add( RestSqlParameter value )
        => Add( (object)value );

    public override int Add( object value )
    {
        IsDirty = true;

        if ( value is not RestSqlParameter )
        {
            throw new ArgumentException( "Value must be of type RestSqlParameter", nameof( value ) );
        }

        InnerList.Add((RestSqlParameter)value);

        return ( Count - 1 );
    }

    public override void AddRange( Array values )
    {
        IsDirty = true;

        foreach ( object value in values )
        {
            if ( value is not RestSqlParameter )
            {
                throw new ArgumentException( "Values must be of type RestSqlParameter", nameof( values ) );
            }

            InnerList.Add( (RestSqlParameter)value );
        }
    }

    public override void Clear()
    {
        IsDirty = true;

        var inner = InnerList;

        // foreach ( RestSqlParameter parameter in inner )
        // {
        //     parameter.ResetParent(); ??
        // }

        inner.Clear();
    }

    public override bool Contains( object value ) => IndexOf( value ) != -1;

    public override bool Contains( string value )
    {
        var inner = InnerList;

        return inner.Any( p => p.ParameterName == value );
    }

    public override void CopyTo( Array array, int index )
        => ((ICollection)InnerList).CopyTo( array, index );

    public override IEnumerator GetEnumerator()
        => ((ICollection)InnerList).GetEnumerator();

    public override int IndexOf( object value )
    {
        if ( value is not RestSqlParameter )
        {
            throw new ArgumentException( "Value must be of type RestSqlParameter", nameof( value ) );
        }

        var inner = InnerList;

        return inner.IndexOf( (RestSqlParameter)value );
    }

    public override int IndexOf( string parameterName )
    {
        var inner = InnerList;

        return inner.FindIndex( p => p.ParameterName == parameterName );
    }

    public override void Insert( int index, object value )
    {
        IsDirty = true;

        if ( value is not RestSqlParameter )
        {
            throw new ArgumentException( "Value must be of type RestSqlParameter", nameof( value ) );
        }

        InnerList.Insert( index, (RestSqlParameter)value );
    }

    public override void Remove( object value )
    {
        IsDirty = true;

        if ( value is not RestSqlParameter )
        {
            throw new ArgumentException( "Value must be of type RestSqlParameter", nameof( value ) );
        }

        var index = IndexOf( value );

        if ( index != -1 )
        {
            RemoveAt( index );
        }
    }

    public override void RemoveAt( int index )
    {
        IsDirty = true;

        var inner = InnerList;

        // inner[index].ResetParent(); ??

        inner.RemoveAt( index );
    }

    public override void RemoveAt( string parameterName )
    {
        var index = IndexOf( parameterName );

        if ( index != -1 )
        {
            RemoveAt( index );
        }
    }

    protected override DbParameter GetParameter( int index ) => InnerList[index];

    protected override DbParameter GetParameter( string parameterName )
    {
        var index = IndexOf( parameterName );

        if ( index == -1 )
        {
            throw new ArgumentException( "Parameter not found", nameof( parameterName ) );
        }

        return InnerList[index];
    }

    protected override void SetParameter( int index, DbParameter value )
    {
        IsDirty = true;

        if ( value is not RestSqlParameter )
        {
            throw new ArgumentException( "Value must be of type RestSqlParameter", nameof( value ) );
        }

        var items = InnerList;

        //var old = items[index];

        items[index] = (RestSqlParameter)value;

        // old.ResetParent(); ??
    }

    protected override void SetParameter( string parameterName, DbParameter value )
    {
        var index = IndexOf( parameterName );

        if ( index == -1 )
        {
            throw new ArgumentException( "Parameter not found", nameof( parameterName ) );
        }

        SetParameter( index, value );
    }

    private List<RestSqlParameter> InnerList
    {
        get
        {
            List<RestSqlParameter> inner = items;

            if (inner == null)
            {
                inner = new List<RestSqlParameter>();
                items = inner;
            }
            return inner;
        }
    }

}
