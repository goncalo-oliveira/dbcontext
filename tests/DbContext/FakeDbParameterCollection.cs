using System.Collections;
using System.Data.Common;

#pragma warning disable IDE0130
namespace tests;
#pragma warning restore IDE0130

public sealed class FakeDbParameterCollection : DbParameterCollection
{
    private readonly Dictionary<string, DbParameter> parameters = [];

    public override int Count => parameters.Count;

    public override object SyncRoot => parameters;

    public override int Add( object value )
    {
        if( value is not DbParameter parameter )
        {
            throw new ArgumentException( "The value is not a DbParameter.", nameof( value ) );
        }

        var idx = parameters.Count + 1;

        parameters.Add( $"@p{idx}", parameter );

        //parameters.Add( parameter.ParameterName, parameter );

        return parameters.Count;
    }

    public override void AddRange( Array values )
    {
        foreach( var value in values )
        {
            Add( value );
        }
    }

    public override void Clear()
    {
        parameters.Clear();
    }

    public override bool Contains( object value )
    {
        if( value is not DbParameter parameter )
        {
            throw new ArgumentException( "The value is not a DbParameter.", nameof( value ) );
        }

        return parameters.ContainsKey( parameter.ParameterName );
    }

    public override bool Contains( string value )
    {
        return parameters.ContainsKey( value );
    }

    public override void CopyTo( Array array, int index )
    {
        throw new NotImplementedException();
    }

    public override IEnumerator GetEnumerator()
    {
        return parameters.Values.GetEnumerator();
    }

    public override int IndexOf( object value )
    {
        if( value is not DbParameter parameter )
        {
            throw new ArgumentException( "The value is not a DbParameter.", nameof( value ) );
        }

        return parameters.Values.ToList().IndexOf( parameter );
    }

    public override int IndexOf( string parameterName )
    {
        return parameters.Values.ToList().IndexOf( parameters[parameterName] );
    }

    public override void Insert( int index, object value )
    {
        throw new NotImplementedException();
    }

    public override void Remove( object value )
    {
        if( value is not DbParameter parameter )
        {
            throw new ArgumentException( "The value is not a DbParameter.", nameof( value ) );
        }

        parameters.Remove( parameter.ParameterName );
    }

    public override void RemoveAt( int index )
    {
        throw new NotImplementedException();
    }

    public override void RemoveAt( string parameterName )
    {
        parameters.Remove( parameterName );
    }

    protected override DbParameter GetParameter( int index )
    {
        return parameters.Values.ToList()[index];
    }

    protected override DbParameter GetParameter( string parameterName )
    {
        return parameters[parameterName];
    }

    protected override void SetParameter( int index, DbParameter value )
    {
        throw new NotImplementedException();
    }

    protected override void SetParameter( string parameterName, DbParameter value )
    {
        parameters[parameterName] = value;
    }
}
