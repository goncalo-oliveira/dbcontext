using System.Data.Mapper;
using System.Data.Mapper.Expressions;

namespace tests;

public class DbExpressionVisitorTests
{
    [Fact]
    public void TestSimpleExpressions()
    {
        var whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => x.Id == Guid.NewGuid() );
        var parameters = whereClause.Parameters;

        Assert.Equal( "\"id\" = @p_id", whereClause.ToString() );
        Assert.Single( parameters );
        Assert.Equal( "p_id", parameters.Keys.First() );
        Assert.True( parameters.Values.First() is Guid );

        var entityId = Guid.NewGuid();

        whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => x.Id == entityId );
        parameters = whereClause.Parameters;

        Assert.Equal( "\"id\" = @p_id", whereClause.ToString() );
        Assert.Single( parameters );
        Assert.Equal( "p_id", parameters.Keys.First() );
        Assert.Equal( entityId, parameters.Values.First() );
    }

    [Fact]
    public void TestComplexExpressions()
    {
        var whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => x.Id == Guid.NewGuid() && x.LicenseNumber == "12345" );
        var parameters = whereClause.Parameters;

        Assert.Equal( "\"id\" = @p_id AND \"license_number\" = @p_license_number", whereClause.ToString() );
        Assert.Equal( 2, parameters.Count );
        Assert.Equal( "12345", parameters["p_license_number"] );

        whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => x.Id == Guid.NewGuid() && ( x.LicenseNumber == "12345" || x.LicenseNumber == "54321" ) );
        parameters = whereClause.Parameters;

        Assert.Equal( "\"id\" = @p_id AND (\"license_number\" = @p_license_number OR \"license_number\" = @p_license_number_1)", whereClause.ToString() );

        Assert.Equal( 3, parameters.Count );
        Assert.Equal( "12345", parameters["p_license_number"] );
        Assert.Equal( "54321", parameters["p_license_number_1"] );

        whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => ( x.Id == Guid.NewGuid() && x.LicenseNumber == "12345" ) || ( x.LicenseNumber == "54321" ) );
        parameters = whereClause.Parameters;

        Assert.Equal( "(\"id\" = @p_id AND \"license_number\" = @p_license_number) OR \"license_number\" = @p_license_number_1", whereClause.ToString() );
        Assert.Equal( 3, parameters.Count );
        Assert.Equal( "12345", parameters["p_license_number"] );
        Assert.Equal( "54321", parameters["p_license_number_1"] );

        whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => x.LicenseNumber == null || x.LicenseNumber == "12345" );
        parameters = whereClause.Parameters;

        Assert.Equal( "\"license_number\" IS NULL OR \"license_number\" = @p_license_number", whereClause.ToString() );
        Assert.Single( parameters );

        string? licenseNumber = "12345";
        whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => licenseNumber == null || x.LicenseNumber == licenseNumber );
        parameters = whereClause.Parameters;

        Assert.Equal( "@p_license_number IS NULL OR \"license_number\" = @p_license_number", whereClause.ToString() );
        Assert.Single( parameters );
        Assert.Equal( licenseNumber, parameters["p_license_number"] );

        Guid id = Guid.NewGuid();
        licenseNumber = null;
        whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => x.Id == id && ( licenseNumber == null || x.LicenseNumber == licenseNumber ) );
        parameters = whereClause.Parameters;

        Assert.Equal( "\"id\" = @p_id AND (@p_license_number IS NULL OR \"license_number\" = @p_license_number)", whereClause.ToString() );
        Assert.Equal( 2, parameters.Count );
        Assert.Equal( id, parameters["p_id"] );
        Assert.Equal( DBNull.Value, parameters["p_license_number"] );

        whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => x.LicenseNumber != null );

        Assert.Equal( "\"license_number\" IS NOT NULL", whereClause.ToString() );
        Assert.Empty( whereClause.Parameters );
    }

    private static readonly Guid accountId = new();

    [Fact]
    public void TestField( )
    {
        var whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => x.Id == accountId );
        var parameters = whereClause.Parameters;

        Assert.Equal( "\"id\" = @p_account_id", whereClause.ToString() );
        Assert.Single( parameters );
        Assert.Equal( "p_account_id", parameters.Keys.First() );
        Assert.Equal( accountId, (Guid)parameters.Values.First() );

    }

    [Fact]
    public void TestHiddenProperties()
    {
        var id = Guid.NewGuid().ToString( "N" );
        var whereClause = DbExpressionVisitor.GetWhereClause<DriverInherit>( x => x.Id == id );
        var parameters = whereClause.Parameters;

        Assert.Equal( "\"id\" = @p_id", whereClause.ToString() );
        Assert.Single( parameters );
        Assert.Equal( "p_id", parameters.Keys.First() );
        Assert.Equal( id, parameters.Values.First() );
    }

    [Fact]
    public void EntityMethodCallsAreRejected()
    {
        var exception = Assert.Throws<NotSupportedException>(
            () => DbExpressionVisitor.GetWhereClause<Driver>( x => x.Name!.StartsWith( "A" ) )
        );

        Assert.Contains( "method calls that reference the entity", exception.Message );
    }

    [Fact]
    public void ComputedEntityValuesAreRejected()
    {
        var exception = Assert.Throws<NotSupportedException>(
            () => DbExpressionVisitor.GetWhereClause<Driver>( x => x.Age + 1 > 18 )
        );

        Assert.Contains( "computed entity values", exception.Message );
    }

    [Fact]
    public void PropertyToPropertyComparisonsAreRejected()
    {
        var exception = Assert.Throws<NotSupportedException>(
            () => DbExpressionVisitor.GetWhereClause<Driver>( x => x.Age == x.MinimumAge )
        );

        Assert.Contains( "property-to-property comparisons", exception.Message );
    }

    [Fact]
    public void UnaryPredicatesAreRejected()
    {
        var exception = Assert.Throws<NotSupportedException>(
            () => DbExpressionVisitor.GetWhereClause<Driver>( x => !x.Active )
        );

        Assert.Contains( "predicates other than comparisons", exception.Message );
    }

    [Fact]
    public void EntityFieldsAreRejected()
    {
        var exception = Assert.Throws<NotSupportedException>(
            () => DbExpressionVisitor.GetWhereClause<Driver>( x => x.LegacyAge > 18 )
        );

        Assert.Contains( "computed entity values", exception.Message );
    }

    private class Driver
    {
        [EntityId]
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? LicenseNumber { get; set; }
        public int Age { get; set; }
        public int MinimumAge { get; set; }
        public bool Active { get; set; }
        public int LegacyAge = 0;
    }

    private class DriverInherit : Driver
    {
        [EntityId]
        public new string? Id { get; set; } // Hides the base class property
    }
}
