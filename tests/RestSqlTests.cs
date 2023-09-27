using System.Data;
using System.Security;
using System.Text;
using Faactory.RestSql;
using Microsoft.Extensions.DependencyInjection;

namespace tests;

public class RestSqlTests
{
    [Fact]
    public void TestConnectionString()
    {
        var valid = "restsql://localhost:5001?database=restsql&user=restsql&password=restsql";
        var invalid = "restsql://localhost:5001?user=restsql&password=restsql"; // no database

        var services = new ServiceCollection();

        services.AddRestSqlDbContextProvider()
            .AddDbContext( "valid", valid )
            .AddDbContext( "invalid", invalid );

        var provider = services.BuildServiceProvider();

        IDbContextFactory dbContextFactory = provider.GetRequiredService<IDbContextFactory>();

        var validConnection = dbContextFactory.GetDbContext( "valid" )
            .GetDbConnection();

        Assert.Equal(
            valid.Replace( "password=restsql", "password=********" ),
            validConnection.ConnectionString
        );

        validConnection.Dispose();

        try
        {
            var invalidConnection = dbContextFactory.GetDbContext( "invalid" )
                .GetDbConnection();

            invalidConnection.Dispose();

            Assert.True( false ); // the above connection should throw an exception
        }
        catch ( ArgumentException )
        {
            Assert.True( true );
        }

    }

    [Fact]
    public void TestParameterSqlType()
    {
        var parameters = new List<RestSqlParameter>();

        var datetime = DateTime.Now;
        var datetimeoffset = DateTimeOffset.Now;
        var guid = Guid.NewGuid();

        parameters.Add( RestSqlParameter.Create( "@v_int32", 1 ) );
        parameters.Add( RestSqlParameter.Create( "@v_int64", 11111111111L ) );
        parameters.Add( RestSqlParameter.Create( "@v_string", "test" ) );
        parameters.Add( RestSqlParameter.Create( "@v_datetime", datetime ) );
        parameters.Add( RestSqlParameter.Create( "@v_datetimeoffset", datetimeoffset ) );
        parameters.Add( RestSqlParameter.Create( "@v_guid", guid ) );
        parameters.Add( RestSqlParameter.Create( "@v_NULL", null, DbType.String ) );

        var declare = new StringBuilder();
        var values = new StringBuilder();

        foreach ( var parameter in parameters )
        {
            declare.AppendLine( $"DECLARE {parameter.ParameterName} {parameter.GetSqlType()};" );
            values.AppendLine( $"SET {parameter.ParameterName} = {parameter.GetSqlValue()};" );
        }

        Assert.Equal(
            """
            DECLARE @v_int32 int;
            DECLARE @v_int64 bigint;
            DECLARE @v_string nvarchar(MAX);
            DECLARE @v_datetime datetime2;
            DECLARE @v_datetimeoffset datetimeoffset;
            DECLARE @v_guid uniqueidentifier;
            DECLARE @v_NULL nvarchar(MAX);
            """
            .Replace( "\r\n", string.Empty ),
            declare.ToString().Replace( "\n", string.Empty )
        );

        Assert.Equal(
            $$"""
            SET @v_int32 = 1;
            SET @v_int64 = 11111111111;
            SET @v_string = N'test';
            SET @v_datetime = '{{datetime:o}}';
            SET @v_datetimeoffset = '{{datetimeoffset:o}}';
            SET @v_guid = '{{guid}}';
            SET @v_NULL = NULL;
            """
            .Replace( "\r\n", string.Empty ),
            values.ToString().Replace( "\n", string.Empty )
        );
    }

    [Fact]
    public void TestParametersWithContext()
    {
        var connectionString = "restsql://localhost:5001?database=restsql&user=restsql&password=restsql";

        var services = new ServiceCollection();

        services.AddRestSqlDbContextProvider()
            .AddDbContext( "restsql", connectionString );

        var provider = services.BuildServiceProvider();

        IDbContextFactory dbContextFactory = provider.GetRequiredService<IDbContextFactory>();

        var dbContext = dbContextFactory.GetDbContext( "restsql" );

        var command = dbContext.BuildCommand()
            .SetTimeout( 5 )
            .AddParameter( "@v_int32", 1 )
            .AddParameter( "@v_int64", 11111111111L )
            .AddParameter( "@v_string", "test" )
            .AddParameter( p =>
            {
                p.ParameterName = "@v_NULL";
                p.DbType = DbType.String;
                p.Value = null;
            } )
            .Build();

        Assert.Equal( typeof( RestSqlCommand ), command.GetType() );

        var declare = new StringBuilder();
        var values = new StringBuilder();

        foreach ( RestSqlParameter parameter in command.Parameters )
        {
            declare.AppendLine( $"DECLARE {parameter.ParameterName} {parameter.GetSqlType()};" );
            values.AppendLine( $"SET {parameter.ParameterName} = {parameter.GetSqlValue()};" );
        }

        Assert.Equal(
            """
            DECLARE @v_int32 int;
            DECLARE @v_int64 bigint;
            DECLARE @v_string nvarchar(MAX);
            DECLARE @v_NULL nvarchar(MAX);
            """
            .Replace( "\r\n", string.Empty ),
            declare.ToString().Replace( "\n", string.Empty )
        );

        Assert.Equal(
            $$"""
            SET @v_int32 = 1;
            SET @v_int64 = 11111111111;
            SET @v_string = N'test';
            SET @v_NULL = NULL;
            """
            .Replace( "\r\n", string.Empty ),
            values.ToString().Replace( "\n", string.Empty )
        );
    }
}
