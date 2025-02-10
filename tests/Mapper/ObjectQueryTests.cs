using System.Data.Mapper;
using BTelematics.Data;

namespace tests;

public class ObjectQueryTests
{
    [Fact]
    public async Task Query()
    {
        var context = new FakeDbContext();

        await context.QueryAsync<Driver>();

        await context.QueryAsync<Driver>(
            where: x => x.Id == Guid.NewGuid()
        );

        await context.QueryAsync<Driver>(
            where: x => x.Id == Guid.NewGuid() && x.Name == "John"
        );

        Assert.Equal(
            """
            SELECT
                id,
                name,
                license_number
            FROM drivers
            ---
            SELECT
                id,
                name,
                license_number
            FROM drivers
            WHERE
                id = @p_id
            ---
            SELECT
                id,
                name,
                license_number
            FROM drivers
            WHERE
                id = @p_id AND name = @p_name
            """.Replace( "\r", string.Empty ),
            context.Output.ToString()
        );
    }

    [Fact]
    public async Task QueryWithSelector()
    {
        var context = new FakeDbContext();

        await context.QueryAsync<Driver>(
            selector: x => new { x.Id, x.Name }
        );

        Assert.Equal(
            """
            SELECT
                id,
                name
            FROM drivers
            """.Replace( "\r", string.Empty ),
            context.Output.ToString()
        );
    }

    [Fact]
    public async Task QueryWithLimit()
    {
        var context = new FakeDbContext();

        await context.QueryAsync<Driver>(
            limit: 10
        );

        Assert.Equal(
            """
            SELECT
                id,
                name,
                license_number
            FROM drivers
            LIMIT 10
            """.Replace( "\r", string.Empty ),
            context.Output.ToString()
        );
    }

    [Fact]
    public async Task QuerySingle()
    {
        var context = new FakeDbContext();
        Guid vehicleId = Guid.NewGuid();

        var vehicle = await context.QuerySingleAsync<Vehicle>(
            vehicleId,
            selector: x => new { x.Id, x.AccountId, x.FleetId, x.Name, x.Annotations, x.CreatedAt, x.UpdatedAt }
        );

        Assert.Equal(
            """
            SELECT
                id,
                account_id,
                fleet_id,
                name,
                annotations,
                created_at,
                updated_at
            FROM vehicles
            WHERE
                id = @p_id
            LIMIT 1
            """.Replace( "\r", string.Empty ),
            context.Output.ToString()
        );
    }

    [Fact]
    public async Task QueryWithMultipleEntityIds()
    {
        var context = new FakeDbContext();
        Guid accountId = Guid.NewGuid();
        Guid? fleetId = null;

        var vehicles = await context.QueryAsync<Vehicle>(
            selector: x => new { x.Id, x.AccountId, x.FleetId, x.Name, x.Annotations, x.CreatedAt, x.UpdatedAt },
            where: x => x.AccountId == accountId && ( fleetId == null || x.FleetId == fleetId )
        );

        Assert.Equal(
            """
            SELECT
                id,
                account_id,
                fleet_id,
                name,
                annotations,
                created_at,
                updated_at
            FROM vehicles
            WHERE
                account_id = @p_account_id AND (@p_fleet_id IS NULL OR fleet_id = @p_fleet_id)
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

    public sealed class Vehicle
    {
        [EntityId]
        public Guid? Id { get; set; }

        [EntityId( Ref = "accounts" )]
        public Guid? AccountId { get; set; }

        [EntityId( Ref = "fleets" )]
        public Guid? FleetId { get; set; }

        public string? Name { get; set; }
        public Dictionary<string, string>? Annotations { get; set; }
        public Dictionary<string, object?>? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
