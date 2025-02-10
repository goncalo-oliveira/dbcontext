using System.Data.Mapper;
using BTelematics.Data;

namespace tests;

public class ObjectUpdateTests
{
    [Fact]
    public async Task Update()
    {
        var context = new FakeDbContext();
        var driver = new Driver { Id = Guid.NewGuid(), Name = "John Doe", LicenseNumber = "123456" };

        await context.UpdateAsync(
            entity: driver
        );

        Assert.Equal(
            """
            UPDATE drivers SET
                name = @name,
                license_number = @license_number
            WHERE id = @p_id
            """.Replace( "\r", string.Empty ),
            context.Output.ToString()
        );
    }

    [Fact]
    public async Task UpdateWithSelector()
    {
        var context = new FakeDbContext();
        var driver = new Driver { Id = Guid.NewGuid(), Name = "John Doe", LicenseNumber = "123456" };

        await context.UpdateAsync(
            entity: driver,
            selector: x => new { x.LicenseNumber }
        );

        Assert.Equal(
            """
            UPDATE drivers SET
                license_number = @license_number
            WHERE id = @p_id
            """.Replace( "\r", string.Empty ),
            context.Output.ToString()
        );
    }

    [Fact]
    public async Task UpdateWithWhereClause()
    {
        var context = new FakeDbContext();
        var driver = new Driver { Id = Guid.NewGuid(), Name = "John Doe", LicenseNumber = "123456" };

        await context.UpdateAsync(
            entity: driver,
            selector: x => new { x.LicenseNumber },
            where: x => x.LicenseNumber == "3312"
        );

        Assert.Equal(
            """
            UPDATE drivers SET
                license_number = @license_number
            WHERE
                license_number = @p_license_number
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
