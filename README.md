# ADO.NET Extensions

This project contains a set of extensions to help with managing multiple data sources, more specifically, inside a DI scenario.

## Getting started

Before we can use the extensions, we need to register the context provider with our DI container. We'll have to install the appropriate package; the following providers are currently supported

|Provider|Package|
|---|---|
|SqlServer|Faactory.DbContext.SqlClient|
|PostgreSql|Faactory.DbContext.Npgsql|

We'll use SqlServer as an example

```bash
$ dotnet add package Faactory.DbContext.SqlClient
```

On our `Startup.cs` class, we'll register the provider and configure our databases; we can add multiple contexts.

```csharp
IServiceCollection services = ...

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

    ...
}
```

When we require a new connection, we'll use the `IDbContext` instance to construct a new connection.

```csharp
public class Example
{
    private readonly IDbContext mydb;

    ...

    public async Task DoSomethingAsync()
    {
        using ( var connection = mydb.GetDbConnection() )
        {
            await connection.OpenAsync();

            ...
        }
    }
}
```

We can also construct the connection and open it directly

```csharp
public async Task DoSomethingAsync()
{
    using ( var connection = await mydb.OpenAsync() )
    {
        ...
    }
}
```

From this point forward, we'll have an `IDbConnection` instance ready to use.

> Note: All `IDbConnection` instances should be properly disposed after use. Most of the ADO implementations will pool connections and not properly disposing the connections can lead to exceeding the number of open connections.

## Transactions

As an alternative to the `IDbConnection.BeginTransaction` method, there are extensions available to shorten the syntax. The `UseTransaction[Async]` methods take care of opening/reusing a connection, creating a transaction and gracefully disposing of it all when finished.

```csharp
public async Task DoSomethingAsync()
{
    await mydb.UseTransactionAsync( async t =>
    {
        var sqlCommand = t.Connection.CreateCommand();

        ...

        await t.CommitAsync();
    } );
}
```

If an error occurs, the transaction is automatically rolled back. We can also provide additional behaviour to when this happens.

```csharp
public async Task DoSomethingAsync()
{
    await mydb.UseTransactionAsync( async t =>
    {
        var sqlCommand = t.Connection.CreateCommand();

        ...

        await t.CommitAsync();
    }, ex =>
    {
        // handle exception
    } );
}
```
