# DbContext - Mapper

The `DbContext` mapper is a simple and lightweight library that allows us to map our query results to objects and from objects to commands. It's designed to be simple and efficient and is built on top of the core library.

## Concepts

To understand what the library does, it's important to understand how it does it. The core library already provided a way to map our query results to objects. It's a simple and direct approach that takes a delegate to map the results of a single row to an object.

```csharp
using var reader = await command.ExecuteReaderAsync(); // command is a DbCommand instance

var result = await reader.MapAsync( reader =>
{
    return new
    {
        Id = reader.GetInt32( 0 ),
        Name = reader.GetString( 1 )
    };
} );
```

The above code snippet shows a simple example for quickly mapping a query result to an object. It's fast and efficient, but it's also a bit verbose and error-prone. The mapper library attempts to simplify this process by providing a more direct way to map the results to objects and from objects to commands. The starting point to explain how it works is the `MapObject<T>` extension method. Instead of using an anonymous object, let's use a class to represent the result.

```csharp
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

Now, let's change the previous code snippet to use the `MapObject<T>` extension method, simplifying the mapping process.

```csharp
using var reader = await command.ExecuteReaderAsync(); // command is a DbCommand instance

Person[] people = await reader.MapAsync( reader => reader.MapObject<Person>() );
```

All we are doing is removing the manual mapping and replacing it with a call to the `MapObject<T>` extension method. This will automatically map a row to the `Person` class. The same can be done directly from the `DbConnection` or `IDbContext` instances.

```csharp
Person[] people = await connection.ExecuteQueryAsync( // connection is a DbConnection instance
    sql,
    reader => reader.MapObject<Person>()
);
/// or
Person[] people = await context.ExecuteQueryAsync( // context is an IDbContext instance
    sql,
    reader => reader.MapObject<Person>()
);
```

Naturally, this comes with a (minimal) cost. The library needs to use reflection to map the properties of the object to the columns of the result set, but since these mappings are cached, the performance impact is (usually) minimal. The getters and setters are also compiled and cached instead of using reflection, which makes the process faster.

Now, instead of using the core's `ExecuteQueryAsync` method and passing the `MapObject<T>` as a delegate, we can do the same by using the typed `ExecuteQueryAsync<T>` method, which does the same thing but is more readable.

```csharp
Person[] people = await connection.ExecuteQueryAsync<Person>( sql ); // connection is a DbConnection instance
// or
Person[] people = await context.ExecuteQueryAsync<Person>( sql ); // context is an IDbContext instance
```

## Getting Started

To use the library, you need to install the package from NuGet.

```bash
dotnet add package Faactory.DbContext.Mapper
```

The library will work with any of the implemented database providers, such as PostgreSQL, SQL Server or SQLite.

## Entities

Entities are the classes that represent the data present in the database. They are simple classes that contain properties that correspond to the columns of a query result. If we are writing the queries ourselves, there's nothing else we need to do.

```csharp
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? AccountNumber { get; set; }
}
```

As seen in previous examples, we can just use the `ExecuteQueryAsync<T>` method to map the results to the `Person` class.

```csharp
Person[] people = await connection.ExecuteQueryAsync<Person>(
    """
    SELECT id, name, account_number
    FROM people
    LIMIT 10
    """
);
```

Entities can also be used to build commands. For cases where the command text is simple enough (no joins, for example), we can use the mapper to infer the columns and parameters of the command.

```csharp
Person[] people = await connection.QueryAsync<Person>( limit: 10 );
/*
SELECT id, name, account_number
FROM persons
LIMIT 10
*/
```

The snippet above does the same as the previous one, but it generates the command text for us. However, if we were to attempt to run it, it would fail, since the table name `persons` is incorrect. The library infers the table name from the entity name by using a naming policy and pluralization rules, which works for most cases, including some irregular pluralizations such as `city` to `cities`. Unfortunately, `person` is one of the exceptions, and the library will translate it to `persons` instead of `people`. To fix this, we can use the `Entity` attribute and specify the table name.

```csharp
[Entity( "people" )]
public class Person
{
    // ...
}
```

The naming policy can also be changed by setting the `NamingPolicy` property in the same attribute. By default, `SnakeCase` is used.

```csharp
[Entity( "people", NamingPolicy = NamingPolicy.LowerCase )]
public class Person
{
    // ...
    public string? AccountNumber { get; set; } // will be mapped to accountnumber
}
```

If a property name doesn't match the column name, we can also use the `Column` attribute to specify the column name.

```csharp
public class Person
{
    // ...
    [Column( "account_num" )]
    public string? AccountNumber { get; set; }
}
```

## Selectors and Where Clauses

If we want to select specific properties from the entity, instead of all of them, we can make use of a *selector expression*. This is a lambda expression that selects the properties we want to map. The snippet below shows how to select only the `Id` and `Name` properties from the `Person` entity.

```csharp
Person[] people = await connection.QueryAsync<Person>(
    selector: p => new { p.Id, p.Name },
    limit: 10
);
/*
SELECT id, name
FROM people
LIMIT 10
*/
```

We can also add a *where clause expression* to filter the results. The snippet below shows how to select only the people with an `Id` greater than 10.

```csharp
Person[] people = await connection.QueryAsync<Person>(
    selector: p => new { p.Id, p.Name },
    where: p => p.Id > 10,
    limit: 10
);
/*
SELECT id, name
FROM people
WHERE id > @p_id
LIMIT 10
*/
```

## Insert, Update and Delete

Inserting and updating is also simple with entities. We can use the `InsertAsync` and `UpdateAsync` methods to insert and update records, respectively. The snippet below shows how to insert a new person into the database.

```csharp
var person = new Person
{
    Name = "John Doe"
}

await connection.InsertAsync(
    person
    selector: p => new { p.Name }
);
/*
INSERT INTO people (name)
VALUES (@name)
*/
```

Notice that we used a selector expression to specify which properties to use in the insert command. If we don't specify a selector, all properties will be used. In our imaginary scenario, the `id` column is an auto-increment column, so we don't want to specify it.

Updating is similar to inserting. The snippet below shows how to update a person's name.

```csharp
var person = new Person
{
    Name = "Jane Doe"
}

await connection.UpdateAsync(
    person
    selector: p => new { p.Name }
    where: p => p.Id == 1
);
/*
UPDATE people
SET name = @name
WHERE id = @p_id
*/
```

Deleting is somewhat simpler, we just need the `where` clause.

```csharp
await connection.DeleteAsync<Person>(
    where: p => p.Id == 1
);
/*
DELETE FROM people
WHERE id = @p_id
*/
```

## Entity Id

Although not required, it's possible to indicate which property is the entity's id. This might be useful for some operations, such as updating and deleting, as an alternative to using the `where` clause. To do this, we decorate the property with the `EntityId` attribute; only one property can have this attribute.

```csharp
public class Person
{
    [EntityId]
    public int Id { get; set; }
    public string Name { get; set; }
}
```

Now we can use the `UpdateAsync` method without the `where` clause.

```csharp
var person = new Person
{
    Id = 1,
    Name = "Jane Doe"
}

await connection.UpdateAsync(
    person
    selector: p => new { p.Name }
);
```

The same can be done for the `DeleteAsync` method.

```csharp
var person = new Person
{
    Id = 1
}

await connection.DeleteAsync(
    person
);

// or simply

await connection.DeleteAsync<Person>( 1 );
```

## Type Converters

In some cases, we might want to convert the data from the database to a different type other than the default. For example, let's say we have a `DateTimeOffset` property in our entity but want to store it as a `long` in the database. A *type converter* can be used to read and write the data in the desired format.

```csharp
public class UnixTimeConverter : DbTypeConverter
{
    public override object? Read( DbDataReader reader, int ordinal, Type propertyType )
    {
        if ( reader.IsDBNull( ordinal ) )
        {
            return null;
        }

        var value = reader.GetInt64( ordinal );

        return DateTimeOffset.FromUnixTimeMilliseconds( value );
    }

    public override void Write( IDbDataParameter parameter, object? value )
    {
        parameter.DbType = DbType.Int64;
        parameter.Value = value is DateTimeOffset dto
            ? dto.ToUnixTimeMilliseconds()
            : DBNull.Value;
    }
}
```

The snippet above shows a simple converter that reads a `long` value from the database and converts it to a `DateTimeOffset`. The `Read` method is called when reading the data from the database, and the `Write` method is called when writing data to the database. To use the converter, we need to decorate the property with the `DbTypeConverter` attribute.

```csharp
public class Person
{
    // ...

    [DbTypeConverter( typeof( UnixTimeConverter ) )]
    public DateTimeOffset? BirthDate { get; set; }
}
```

The converter attribute can also be used at the class level for custom types, which will apply to all properties of that type.

```csharp
[DbTypeConverter( typeof( UnixTimeConverter ) )]
public struct UnixTime
{
    // ...
}

// ...

public class Person
{
    // ...

    public UnixTime? BirthDate { get; set; }
}

```

## Limitations

There are some limitations, namely when it comes to using entities to build commands. The library is designed to be simple and lightweight, and as such, it doesn't support complex queries, such as joins or subqueries. For these cases, the alternative is to write the query manually and use the result mapping functionality only.

```csharp
var people = await connection.ExecuteQueryAsync<Person>(
    """
    SELECT p.id, p.name, a.street, a.city
    FROM people p
    JOIN addresses a ON p.address_id = a.id
    LIMIT 10
    """
);
```
