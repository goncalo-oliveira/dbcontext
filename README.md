# ADO.NET Extensions

This project contains a set of extensions to help with managing multiple data sources, more specifically, inside a DI scenario.

## Getting started

Before we can use the extensions, we need to register the context provider with our DI container. We'll have to install the appropriate package, depending on the provider we want to use; the following are currently supported

|Provider|Package|Description|
|---|---|---|
|PostgreSql|Faactory.DbContext.Npgsql| PostgreSQL driver; uses [Npgsql](https://github.com/npgsql/npgsql) |
|SqlServer|Faactory.DbContext.SqlClient| SQL Server driver; uses [Microsoft.Data.SqlClient](https://github.com/dotnet/sqlclient) |
|Sqlite|Faactory.DbContext.Sqlite| SQLite driver; uses [Microsoft.Data.Sqlite](https://github.com/dotnet/efcore/blob/main/src/Microsoft.Data.Sqlite/PACKAGE.md) |
|SqlServer|Faactory.DbContext.RestSql| SQL Server via restSQL; still experimental |

We'll use *SqlServer* as an example

```bash
dotnet add package Faactory.DbContext.SqlClient
```

We'll then register the provider and configure our databases; we can add as many contexts as we need. We can also add different providers if we need to access different types of databases.

```csharp
IServiceCollection services = ...;

services.AddSqlDbContextProvider()
    .AddDbContext( "my-db", "connection_string" )
    .AddDbContext( "my-other-db", "connection_string" );
```

Wherever we need to get access to our database context, we'll use the injected `IDbContextFactory` instance to retrieve an `IDbContext` instance.

```csharp
public class Example
{
    private readonly IDbContext mydb;

    public Example( IDbContextFactory dbContextFactory )
    {
        mydb = dbContextFactory.GetDbContext( "my-db" );
    }

    // ...
}
```

To construct a new connection, we'll retrieve it from the `IDbContext` instance.

```csharp
public class Example
{
    private readonly IDbContext mydb;

    // ...

    public async Task DoSomethingAsync()
    {
        using ( var connection = mydb.GetDbConnection() )
        {
            await connection.OpenAsync();

            // ...
        }
    }
}
```

We can also construct the connection and open it all in one go.

```csharp
public async Task DoSomethingAsync()
{
    using ( var connection = await mydb.OpenAsync() )
    {
        // ...
    }
}
```

From this point forward, we'll have a `DbConnection` instance ready to use. Please note that all `DbConnection` instances should be properly disposed after use. Most of the ADO implementations will pool connections and not properly disposing them can lead to exceeding the number of open connections (connection leaks).

> [!NOTE]
> Starting with version 0.6, the library has switched to use the `DbConnection` class instead of the `IDbConnection` interface. This was done mostly because the interface doesn't expose the async methods. Since `DbConnection` should be the base class for most (if not all) ADO.NET providers, this transition shouldn't cause any braking changes. Nonetheless, if you're using the `IDbConnection` interface explicitly in your code, you'll have to update it to use the `DbConnection` class instead.

## Transactions

As an alternative to the `DbConnection.BeginTransaction[Async]` methods, there are extensions available to shorten the amount of code written. The `UseTransaction[Async]` methods take care of opening/reusing a connection, creating a transaction and gracefully disposing of it all when finished.

```csharp
public async Task DoSomethingAsync()
{
    await mydb.UseTransactionAsync( async t =>
    {
        var sqlCommand = t.Connection.CreateCommand();

        // ...

        await t.CommitAsync();
    } );
}
```

If an exception is thrown, the transaction is automatically rolled back. We can also provide additional behaviour to when this happens.

```csharp
public async Task DoSomethingAsync()
{
    await mydb.UseTransactionAsync( async t =>
    {
        var sqlCommand = t.Connection.CreateCommand();

        // ...

        await t.CommitAsync();
    }, ex =>
    {
        // handle exception
    } );
}
```

## Health checks

The library also provides a set of health checks that verify the status of the database contexts. These health checks can be used with the ASP.NET Core health checks middleware.

```csharp
IServiceCollection services = ...;

services.AddHealthChecks()
    .AddDbContext( "my-db" )
    .AddDbContext( "my-other-db" );
```


## Command builder

The command builder is a helper class that can be used to build SQL commands. It provides a fluent interface to build a `DbCommand` instance. The builder can be accessed by calling the `BuildCommand` extension method on `DbConnection` instances.

```csharp
private readonly IDbContext mydb;

// ...

public async Task UseBuilderFromConnectionAsync()
{
    using ( var connection = await mydb.OpenAsync() )
    {
        var command = connection.BuildCommand()
            .SetText( "SELECT * FROM table WHERE id = @id" )
            .AddParameter( "@id", 1 )
            .Build();

        using ( var reader = await command.ExecuteReaderAsync() )
        {
            // ...
        }
    }
}
```

## Query extensions

It's also possible to execute a query and map the results directly by providing a mapper function. This can be useful when we don't want to use a full-blown ORM for simple queries or we just want the benefits of a more direct mapping.

```csharp
private readonly IDbContext mydb;

// ...

public async Task QueryAsync()
{
    using ( var connection = await mydb.OpenAsync() )
    {
        var results = await connection.ExecuteQueryAsync( "SELECT id, name FROM table", reader =>
        {
            return new
            {
                Id = reader.GetInt32( 0 ),
                Name = reader.GetString( 1 )
            };
        } );
    }
}
```

The same can be achieved directly from the `IDbContext` instance, however, it's important to acknowledge that this route will open a new connection for each call. If you intend to execute multiple commands in a row, you should first obtain a connection.

```csharp
private readonly IDbContext mydb;

// ...

public async Task QueryAsync()
{
    var results = await mydb.ExecuteQueryAsync( "SELECT id, name FROM table", reader =>
    {
        return new
        {
            Id = reader.GetInt32( 0 ),
            Name = reader.GetString( 1 )
        };
    } );
}
```

## Non-query and scalar extensions

The library provides a few extensions to simplify the execution of non-query commands and scalar queries. These extensions are available on the `DbConnection` class.

```csharp
private readonly IDbContext mydb;

// ...

public async Task ExecuteNonQueryAsync()
{
    using ( var connection = await mydb.OpenAsync() )
    {
        await connection.ExecuteNonQueryAsync( "DELETE FROM table WHERE id = @id", new { id = 1 } );
    }
}

public async Task ExecuteScalarAsync()
{
    using ( var connection = await mydb.OpenAsync() )
    {
        var result = await connection.ExecuteScalarAsync<int>( "SELECT COUNT(*) FROM table" );
    }
}
```

The same can be achieved directly from the `IDbContext` instance. Similarly to the query extensions, it's important to acknowledge that this route will open a new connection for each call. If you intend to execute multiple commands in a row, you should first obtain a connection.

```csharp
private readonly IDbContext mydb;

// ...

public async Task ExecuteNonQueryAsync()
{
    await mydb.ExecuteNonQueryAsync( "DELETE FROM table WHERE id = @id", new { id = 1 } );
}

public async Task ExecuteScalarAsync()
{
    var result = await mydb.ExecuteScalarAsync<int>( "SELECT COUNT(*) FROM table" );
}
```

## Compatibility with Object Mappers

The library is fully compatible with most object mappers that use `DbConnection` or `IDbConnection` instances, such as [Dapper](https://github.com/DapperLib/Dapper), [PetaPoco](github.com/CollaboratingPlatypus/PetaPoco) or [Norm.net](https://github.com/vb-consulting/Norm.net).
