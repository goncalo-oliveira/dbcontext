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

        Assert.Equal( "id = @p_id", whereClause.ToString() );
        Assert.Single( parameters );
        Assert.Equal( "p_id", parameters.Keys.First() );
        Assert.True( parameters.Values.First() is Guid );

        var entityId = Guid.NewGuid();

        whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => x.Id == entityId );
        parameters = whereClause.Parameters;

        Assert.Equal( "id = @p_id", whereClause.ToString() );
        Assert.Single( parameters );
        Assert.Equal( "p_id", parameters.Keys.First() );
        Assert.Equal( entityId, parameters.Values.First() );
    }

    [Fact]
    public void TestComplexExpressions()
    {
        var whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => x.Id == Guid.NewGuid() && x.LicenseNumber == "12345" );
        var parameters = whereClause.Parameters;

        Assert.Equal( "id = @p_id AND license_number = @p_license_number", whereClause.ToString() );
        Assert.Equal( 2, parameters.Count );
        Assert.Equal( "12345", parameters["p_license_number"] );

        whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => x.Id == Guid.NewGuid() && ( x.LicenseNumber == "12345" || x.LicenseNumber == "54321" ) );
        parameters = whereClause.Parameters;

        Assert.Equal( "id = @p_id AND (license_number = @p_license_number OR license_number = @p_license_number_1)", whereClause.ToString() );

        Assert.Equal( 3, parameters.Count );
        Assert.Equal( "12345", parameters["p_license_number"] );
        Assert.Equal( "54321", parameters["p_license_number_1"] );

        whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => ( x.Id == Guid.NewGuid() && x.LicenseNumber == "12345" ) || ( x.LicenseNumber == "54321" ) );
        parameters = whereClause.Parameters;

        Assert.Equal( "(id = @p_id AND license_number = @p_license_number) OR license_number = @p_license_number_1", whereClause.ToString() );
        Assert.Equal( 3, parameters.Count );
        Assert.Equal( "12345", parameters["p_license_number"] );
        Assert.Equal( "54321", parameters["p_license_number_1"] );

        whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => x.LicenseNumber == null || x.LicenseNumber == "12345" );
        parameters = whereClause.Parameters;

        Assert.Equal( "license_number IS NULL OR license_number = @p_license_number", whereClause.ToString() );
        Assert.Single( parameters );

        string? licenseNumber = "12345";
        whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => licenseNumber == null || x.LicenseNumber == licenseNumber );
        parameters = whereClause.Parameters;

        Assert.Equal( "@p_license_number IS NULL OR license_number = @p_license_number", whereClause.ToString() );
        Assert.Single( parameters );
        Assert.Equal( licenseNumber, parameters["p_license_number"] );

        Guid id = Guid.NewGuid();
        licenseNumber = null;
        whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => x.Id == id && ( licenseNumber == null || x.LicenseNumber == licenseNumber ) );
        parameters = whereClause.Parameters;

        Assert.Equal( "id = @p_id AND (@p_license_number IS NULL OR license_number = @p_license_number)", whereClause.ToString() );
        Assert.Equal( 2, parameters.Count );
        Assert.Equal( id, parameters["p_id"] );
        Assert.Equal( DBNull.Value, parameters["p_license_number"] );
    }

    private static readonly Guid accountId = new();

    [Fact]
    public void TestField( )
    {
        var whereClause = DbExpressionVisitor.GetWhereClause<Driver>( x => x.Id == accountId );
        var parameters = whereClause.Parameters;

        Assert.Equal( "id = @p_account_id", whereClause.ToString() );
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

        Assert.Equal( "id = @p_id", whereClause.ToString() );
        Assert.Single( parameters );
        Assert.Equal( "p_id", parameters.Keys.First() );
        Assert.Equal( id, parameters.Values.First() );
    }

    private class Driver
    {
        [EntityId]
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? LicenseNumber { get; set; }
    }

    private class DriverInherit : Driver
    {
        [EntityId]
        public new string? Id { get; set; } // Hides the base class property
    }
}
