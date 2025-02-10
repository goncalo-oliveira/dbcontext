using System.Data.Mapper;
using BTelematics.Data;

namespace tests;

public class ObjectInsertTests
{
    [Fact]
    public async Task Insert()
    {
        var context = new FakeDbContext();

        await context.InsertAsync(
            entity: new Driver { Id = Guid.NewGuid(), Name = "John Doe", LicenseNumber = "123456" }
        );

        Assert.Equal(
            """
            INSERT INTO drivers (
                id,
                name,
                license_number
            )
            VALUES (
                @id,
                @name,
                @license_number
            )
            """.Replace( "\r", string.Empty ),
            context.Output.ToString()
        );
    }

    [Fact]
    public async Task InsertWithSelector()
    {
        var context = new FakeDbContext();

        await context.InsertAsync(
            entity: new Driver { Name = "John Doe", LicenseNumber = "123456" },
            selector: x => new { x.Name }
        );

        Assert.Equal(
            """
            INSERT INTO drivers (
                name
            )
            VALUES (
                @name
            )
            """.Replace( "\r", string.Empty ),
            context.Output.ToString()
        );
    }

    private class Driver
    {
        [EntityId]
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? LicenseNumber { get; set; }
    }
}
