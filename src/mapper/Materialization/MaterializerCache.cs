using System.Data.Common;
using System.Text;

namespace System.Data.Mapper.Materialization;

internal static class MaterializerCache
{
    internal const int Capacity = 1024;

    private static readonly object SyncRoot = new();
    private static readonly Dictionary<MaterializerKey, Delegate> Materializers = [];
    private static readonly Queue<MaterializerKey> InsertionOrder = [];

    public static Func<DbDataReader, T> GetOrAdd<T>(
        DbDataReader reader,
        MaterializerReadStrategy strategy = MaterializerReadStrategy.Hybrid
    ) where T : notnull, new()
    {
        var schema = ReadSchema( reader );
        var key = new MaterializerKey( typeof( T ), strategy, schema.Signature );

        lock ( SyncRoot )
        {
            if ( Materializers.TryGetValue( key, out var cached ) )
            {
                return (Func<DbDataReader, T>)cached;
            }

            var materializer = MaterializerBuilder.Build<T>( schema, strategy );

            while ( Materializers.Count >= Capacity )
            {
                Materializers.Remove( InsertionOrder.Dequeue() );
            }

            Materializers.Add( key, materializer );
            InsertionOrder.Enqueue( key );

            return materializer;
        }
    }

    internal static int Count
    {
        get
        {
            lock ( SyncRoot )
            {
                return Materializers.Count;
            }
        }
    }

    internal static void Clear()
    {
        lock ( SyncRoot )
        {
            Materializers.Clear();
            InsertionOrder.Clear();
        }
    }

    private static ReaderSchema ReadSchema( DbDataReader reader )
    {
        var signature = new StringBuilder();
        var columns = new ReaderColumn[reader.FieldCount];

        signature.Append( reader.GetType().AssemblyQualifiedName )
            .Append( '|' )
            .Append( reader.FieldCount );

        for ( var ordinal = 0; ordinal < reader.FieldCount; ordinal++ )
        {
            var name = reader.GetName( ordinal );
            var fieldType = reader.GetFieldType( ordinal );
            columns[ordinal] = new ReaderColumn( ordinal, name, fieldType );

            signature.Append( '|' )
                .Append( name.Length )
                .Append( ':' )
                .Append( name )
                .Append( ':' )
                .Append( fieldType.AssemblyQualifiedName );
        }

        return new ReaderSchema( signature.ToString(), columns );
    }

    private readonly record struct MaterializerKey(
        Type EntityType,
        MaterializerReadStrategy Strategy,
        string Schema
    );

    internal sealed record ReaderSchema( string Signature, ReaderColumn[] Columns );
    internal readonly record struct ReaderColumn( int Ordinal, string Name, Type FieldType );
}
