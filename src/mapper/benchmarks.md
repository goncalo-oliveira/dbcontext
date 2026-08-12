# Mapper Benchmarks

These benchmarks compare four ways of materializing the same query result:

1. Raw `DbDataReader` mapping through the core `MapAsync` path.
2. Mapper 0.9 behavior, before query-local column bindings.
3. Mapper 0.10 behavior, with query-local column bindings.

## Results

The benchmark reads 50,000 rows with 10 columns from an in-memory SQLite database. Lower values are better.

| Implementation | Median | Relative to raw | Allocated | Bytes per row |
|---|---:|---:|---:|---:|
| Raw reader | 58.53 ms | 1.00x | 20.83 MiB | 436.8 B |
| Mapper 0.9, pre-bindings | 130.98 ms | 2.24x | 25.41 MiB | 532.9 B |
| Mapper 0.10, column bindings | 106.01 ms | 1.81x | 25.41 MiB | 532.9 B |

Column bindings reduced the mapper's end-to-end materialization time by 19.1% without materially changing allocations. The raw reader remained fastest because it knows the column ordinals and types in advance and performs no metadata lookup or indirect property assignment.

The mapper invokes one compiled setter delegate per mapped property and currently performs an entity-cache lookup when creating every object.

## Query

Every implementation executes the same SQL over the same open connection:

```sql
SELECT id, name, email, age, score, active, created_at, city, country, notes
FROM people
```

All implementations create the same 10-property `Person` object and buffer all 50,000 results. The validation checksum includes every mapped property, ensuring that the implementations materialized identical values rather than merely returning the same number of rows.

### Raw reader

The raw case uses the core query and `MapAsync` path with explicit ordinals, equivalent to:

```csharp
Person[] people = await connection.ExecuteQueryAsync(
    sql,
    reader => new Person
    {
        Id = reader.GetInt64( 0 ),
        Name = reader.GetString( 1 ),
        Email = reader.GetString( 2 ),
        Age = reader.GetInt64( 3 ),
        Score = reader.GetInt64( 4 ),
        Active = reader.GetInt64( 5 ),
        CreatedAt = reader.GetString( 6 ),
        City = reader.GetString( 7 ),
        Country = reader.GetString( 8 ),
        Notes = reader.GetString( 9 )
    }
);
```

### Mapper 0.9, pre-bindings

The pre-binding behavior is represented by the standalone `MapObject<T>` call. It resolves column names and property metadata for every row, matching the 0.9 mapping path:

```csharp
Person[] people = await connection.ExecuteQueryAsync(
    sql,
    reader => reader.MapObject<Person>()
);
```

### Mapper 0.10, column bindings

The typed 0.10 extension creates query-local ordinal-to-property bindings once and reuses them for every row:

```csharp
Person[] people = await connection.ExecuteQueryAsync<Person>( sql );
```

## Methodology

- Date: 2026-08-12
- Machine: MacBook Pro, Apple M1 Pro, 10 cores, 16 GB RAM
- Operating system: macOS 26.5.2, arm64
- Runtime: .NET SDK 10.0.302, Release configuration
- Database provider: Microsoft.Data.Sqlite 10.0.11
- Rows: 50,000
- Columns: 10
- Warm-up runs: 5 per implementation
- Measured runs: 15 per implementation
- Result: median of the 15 measured runs
- Tiered compilation disabled to reduce JIT-related variance
- Benchmark order rotated on each repetition
- Full garbage collection performed before each sample
- Allocations measured with `GC.GetTotalAllocatedBytes( precise: true )`
- Database creation and insertion were completed before timing

These numbers are intended for relative comparison on this workload, not as universal throughput figures. Database provider, schema, data types, row width, hardware and runtime version can materially change the results.

## SQLite type conversion

`Microsoft.Data.Sqlite` exposes SQLite `INTEGER` values from `DbDataReader.GetValue()` as `Int64`. Mapper 0.10 performs a checked, invariant conversion when the provider field type differs from the property type. The conversion requirement is recorded in the query-local column binding, so matching types stay on the direct-assignment path.

The benchmark model uses provider-native `long` and `string` property types so the pre-binding and bound implementations materialize exactly the same values without conversion work. A separate integration check using the same SQLite database verifies `Int64` to `int`, `Int64` to `bool`, and ISO text to `DateTime` mapping.
