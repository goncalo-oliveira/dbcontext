# Mapper Benchmarks

These benchmarks compare four ways of materializing the same query result:

1. Raw `DbDataReader` mapping through the core `MapAsync` path.
2. Mapper 0.9 behavior, before query-local column bindings.
3. Mapper 0.10 behavior, with a cached expression-compiled materializer.
4. Dapper 2.1.66.

## Results

The benchmark reads 50,000 rows with 10 columns from an in-memory SQLite database. Lower values are better.

| Implementation | Median | Relative to raw | Allocated | Bytes per row |
|---|---:|---:|---:|---:|
| Raw reader | 59.80 ms | 1.00x | 20.83 MiB | 436.8 B |
| Mapper 0.9, pre-bindings | 133.42 ms | 2.23x | 25.41 MiB | 532.9 B |
| Mapper 0.10, compiled materializer | 78.58 ms | 1.31x | 20.84 MiB | 437.0 B |
| Dapper 2.1.66 | 88.25 ms | 1.48x | 25.03 MiB | 524.8 B |

The compiled materializer reduced the mapper's end-to-end time by 41.1% compared with the pre-binding path. It completed in 11.0% less time than Dapper and allocated essentially the same amount as raw mapping. The raw reader remained fastest because its mapping code is handwritten and requires no schema lookup or materializer dispatch.

The compiled mapper emits direct construction, typed reads and direct property assignments in one delegate. Materializers are cached by entity type, concrete reader type, ordered column names and provider field types. The cache is strictly bounded to 1,024 entries.

### Reader strategy comparison

The production materializer uses a hybrid reader strategy. Known exact types use dedicated typed getters; exact types without a dedicated getter use `GetFieldValue<T>`; provider/property mismatches use a precompiled conversion, custom `DbTypeConverter`, or the generic conversion fallback.

| Compiled read strategy | Median | Allocated |
|---|---:|---:|
| Dedicated typed getters | 78.74 ms | 20.83 MiB |
| `GetFieldValue<T>` | 103.70 ms | 20.83 MiB |

On this SQLite workload, dedicated typed getters were 24.1% faster than `GetFieldValue<T>`.

## PostgreSQL results

The same benchmark was also run against PostgreSQL 16 in a local Docker container, using Npgsql 10.0.3. The table was unlogged and populated with the same 50,000 rows and 10 logical columns. PostgreSQL-native column types were used: `bigint`, `text`, `boolean`, and `timestamp with time zone`.

| Implementation | Median | Relative to raw | Allocated | Bytes per row |
|---|---:|---:|---:|---:|
| Raw reader | 40.29 ms | 1.00x | 17.03 MiB | 357.1 B |
| Mapper 0.9, pre-bindings | 55.99 ms | 1.39x | 22.74 MiB | 476.9 B |
| Mapper 0.10, compiled materializer | 40.31 ms | 1.00x | 17.03 MiB | 357.1 B |
| Dapper 2.1.66 | 47.51 ms | 1.18x | 22.37 MiB | 469.1 B |

On this Npgsql workload, the compiled mapper was effectively tied with handwritten raw mapping, completed in 15.2% less time than Dapper, and matched raw mapping's allocation rate. A complete second run confirmed the ordering: 40.99 ms raw, 42.11 ms compiled mapper, and 45.95 ms Dapper.

### PostgreSQL reader strategy comparison

| Compiled read strategy | Median | Allocated |
|---|---:|---:|
| Dedicated typed getters | 42.19 ms | 17.02 MiB |
| `GetFieldValue<T>` | 43.39 ms | 17.02 MiB |

Unlike SQLite, Npgsql showed only a small difference between the two compiled reader strategies on this schema. The hybrid strategy still preserves the direct typed path where appropriate while supporting provider types without dedicated `DbDataReader` getters.

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

### Mapper 0.10, compiled materializer

The typed 0.10 extension resolves the reader schema, retrieves or compiles a complete materializer, and uses it for every row:

```csharp
Person[] people = await connection.ExecuteQueryAsync<Person>( sql );
```

### Dapper

```csharp
IEnumerable<Person> people = await connection.QueryAsync<Person>( sql );
```

`DefaultTypeMap.MatchNamesWithUnderscores` was enabled so Dapper maps names such as `created_at` to `CreatedAt`, matching the mapper's snake-case policy.

## Methodology

- Date: 2026-08-12
- Machine: MacBook Pro, Apple M1 Pro, 10 cores, 16 GB RAM
- Operating system: macOS 26.5.2, arm64
- Runtime: .NET SDK 10.0.302, Release configuration
- Database provider: Microsoft.Data.Sqlite 10.0.11
- Additional provider run: PostgreSQL 16 (`postgres:16-alpine`) with Npgsql 10.0.3
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

`Microsoft.Data.Sqlite` exposes SQLite `INTEGER` values from `DbDataReader.GetValue()` as `Int64`. Mapper 0.10 performs a checked, invariant conversion when the provider field type differs from the property type. That conversion is compiled into the materializer, so matching types stay on the direct-assignment path.

The benchmark model uses provider-native `long` and `string` property types so the pre-materializer and compiled implementations materialize exactly the same values without conversion work. A separate integration check using the same SQLite database verifies `Int64` to `int`, `Int64` to `bool`, and ISO text to `DateTime` mapping.
