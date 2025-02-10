using System.Data.Mapper;
using BTelematics.Data;

namespace tests;

public class ObjectDeleteTests
{
    [Fact]
    public async Task Delete()
    {
        var context = new FakeDbContext();
        var driver = new Driver { Id = Guid.NewGuid(), Name = "John Doe", LicenseNumber = "123456" };

        await context.DeleteAsync( driver );

        Assert.Equal(
            """
            DELETE FROM drivers
            WHERE id = @p_id
            """.Replace( "\r", string.Empty ),
            context.Output.ToString()
        );
    }

    [Fact]
    public async Task DeleteWithEntityId()
    {
        var context = new FakeDbContext();
        var entityId = Guid.NewGuid();

        await context.DeleteAsync<Driver>( entityId );
        await context.DeleteAsync<Driver>( entityId.ToString() );

        Assert.Equal(
            """
            DELETE FROM drivers
            WHERE id = @p_id
            ---
            DELETE FROM drivers
            WHERE id = @p_id
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
