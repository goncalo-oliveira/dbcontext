using System.Data;
using System.Data.Mapper;
using System.Data.Common;
using System.Data.Mapper.Materialization;

namespace tests;

public class DbDataReaderMappingTests
{
    [Fact]
    public void QueryLocalMapperBindsColumnsOnlyOnce()
    {
        using var reader = new FakeDbDataReader(
            [ "id", "display_name", "ignored" ],
            [ 1, "one", "unused" ],
            [ 2, "two", "unused" ]
        );
        var map = System.Data.Mapper.DbDataReaderExtensions.CreateMapper<Row>();

        Assert.True( reader.Read() );
        var first = map( reader );
        Assert.True( reader.Read() );
        var second = map( reader );

        Assert.Equal( 1, first.Id );
        Assert.Equal( "one", first.DisplayName );
        Assert.Equal( 2, second.Id );
        Assert.Equal( "two", second.DisplayName );
        Assert.Equal( reader.FieldCount, reader.GetNameCallCount );
    }

    [Fact]
    public void QueryLocalDictionaryMapperCachesColumnNames()
    {
        using var reader = new FakeDbDataReader(
            [ "id", "name" ],
            [ 1, "one" ],
            [ 2, "two" ]
        );
        var map = System.Data.Mapper.DbDataReaderExtensions.CreateMapper<Dictionary<string, object?>>();

        Assert.True( reader.Read() );
        var first = map( reader );
        Assert.True( reader.Read() );
        var second = map( reader );

        Assert.Equal( 1, first["id"] );
        Assert.Equal( "two", second["name"] );
        Assert.Equal( reader.FieldCount, reader.GetNameCallCount );
    }

    [Fact]
    public void QueryLocalMapperUsesConfiguredConverters()
    {
        using var reader = new FakeDbDataReader(
            [ "converted" ],
            [ "one" ],
            [ "two" ]
        );
        var map = System.Data.Mapper.DbDataReaderExtensions.CreateMapper<ConvertedRow>();

        Assert.True( reader.Read() );
        var first = map( reader );
        Assert.True( reader.Read() );
        var second = map( reader );

        Assert.Equal( "mapped:one", first.Converted );
        Assert.Equal( "mapped:two", second.Converted );
    }

    [Fact]
    public void QueryLocalMapperConvertsProviderNativeValues()
    {
        using var reader = new FakeDbDataReader(
            [ "id", "active", "created_at" ],
            [ 42L, 1L, "2026-08-12T10:30:00.0000000Z" ]
        );
        var map = System.Data.Mapper.DbDataReaderExtensions.CreateMapper<NativeValuesRow>();

        Assert.True( reader.Read() );
        var result = map( reader );

        Assert.Equal( 42, result.Id );
        Assert.True( result.Active );
        Assert.Equal(
            new DateTime( 2026, 8, 12, 10, 30, 0, DateTimeKind.Utc ),
            result.CreatedAt
        );
    }

    [Fact]
    public void StandaloneMapperConvertsProviderNativeValues()
    {
        using var reader = new FakeDbDataReader(
            [ "id", "active" ],
            [ 42L, 1L ]
        );

        Assert.True( reader.Read() );
        var result = reader.MapObject<NativeValuesRow>();

        Assert.Equal( 42, result.Id );
        Assert.True( result.Active );
    }

    [Fact]
    public void CompiledNumericConversionIsChecked()
    {
        using var reader = new FakeDbDataReader(
            [ "id" ],
            [ long.MaxValue ]
        );
        var map = System.Data.Mapper.DbDataReaderExtensions.CreateMapper<Row>();

        Assert.True( reader.Read() );
        Assert.Throws<OverflowException>( () => map( reader ) );
    }

    [Fact]
    public void CompiledMaterializerLeavesDatabaseNullsAtDefaults()
    {
        using var reader = new FakeDbDataReader(
            [ "id", "display_name" ],
            [ DBNull.Value, DBNull.Value ]
        );
        var map = System.Data.Mapper.DbDataReaderExtensions.CreateMapper<Row>();

        Assert.True( reader.Read() );
        var result = map( reader );

        Assert.Equal( 0, result.Id );
        Assert.Null( result.DisplayName );
    }

    [Theory]
    [InlineData( (int)MaterializerReadStrategy.TypedGetters )]
    [InlineData( (int)MaterializerReadStrategy.GetFieldValue )]
    public void MaterializerReadStrategiesProduceEquivalentResults( int strategyValue )
    {
        var strategy = (MaterializerReadStrategy)strategyValue;
        using var reader = new FakeDbDataReader(
            [ "id", "display_name" ],
            [ 7, "seven" ]
        );
        var map = System.Data.Mapper.DbDataReaderExtensions.CreateMapper<Row>( strategy );

        Assert.True( reader.Read() );
        var result = map( reader );

        Assert.Equal( 7, result.Id );
        Assert.Equal( "seven", result.DisplayName );
    }

    [Fact]
    public void MaterializerCacheReusesEntityAndSchemaCombination()
    {
        MaterializerCache.Clear();

        using var firstReader = new FakeDbDataReader( [ "id" ], [ 1 ] );
        using var secondReader = new FakeDbDataReader( [ "id" ], [ 2 ] );

        var first = MaterializerCache.GetOrAdd<Row>( firstReader );
        var second = MaterializerCache.GetOrAdd<Row>( secondReader );

        Assert.Same( first, second );
        Assert.Equal( 1, MaterializerCache.Count );
    }

    [Fact]
    public void MaterializerCacheIsStrictlyBounded()
    {
        MaterializerCache.Clear();

        for ( var index = 0; index <= MaterializerCache.Capacity; index++ )
        {
            using var reader = new FakeDbDataReader( [ $"column_{index}" ], [ index ] );
            MaterializerCache.GetOrAdd<Row>( reader );
        }

        Assert.Equal( MaterializerCache.Capacity, MaterializerCache.Count );
        MaterializerCache.Clear();
    }

    [Fact]
    public void MaterializerCacheReturnsOneDelegateUnderConcurrency()
    {
        MaterializerCache.Clear();

        var materializers = Enumerable.Range( 0, 32 )
            .AsParallel()
            .Select( index =>
            {
                using var reader = new FakeDbDataReader( [ "id" ], [ index ] );
                return MaterializerCache.GetOrAdd<Row>( reader );
            } )
            .ToArray();

        Assert.Single( materializers.Distinct() );
        MaterializerCache.Clear();
    }

    private sealed class Row
    {
        public int Id { get; set; }
        public string? DisplayName { get; set; }
    }

    private sealed class ConvertedRow
    {
        [DbTypeConverter( typeof( PrefixConverter ) )]
        public string? Converted { get; set; }
    }

    private sealed class NativeValuesRow
    {
        public int Id { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class PrefixConverter : DbTypeConverter
    {
        public override object? Read( DbDataReader reader, int ordinal, Type propertyType )
            => $"mapped:{reader.GetValue( ordinal )}";

        public override void Write( IDbDataParameter parameter, object? value )
            => parameter.Value = value ?? DBNull.Value;
    }
}
