using System;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using Faactory.RestClient;
using Faactory.RestSql;

namespace System.Data;

public class RestSqlDbContext : IDbContext
{
    private readonly IHttpClientFactory clientFactory;
    private readonly DbContextOptions options;

    public RestSqlDbContext( IHttpClientFactory httpClientFactory, DbContextOptions contextOptions )
    {
        clientFactory = httpClientFactory;
        options = contextOptions;
    }

    public string Name => options.Name;

    public DbContextProviderTypes Provider => DbContextProviderTypes.SqlServer;

    public IDbConnection GetDbConnection()
    {
        var httpClient = clientFactory.CreateClient();

        var connectionString = new Uri( options.ConnectionString );

        // connection is secure by default
        // restsql://localhost:5001?database=restsql&user=restsql&password=restsql
        // is the same as
        // restsql+https://localhost:5001?database=restsql&user=restsql&password=restsql

        // unsafe http connection by setting the scheme to restsql+http
        // restsql+http://localhost:5000?database=restsql&user=restsql&password=restsql

        if ( !new string[]
        {
            "restsql",
            "restsql+http",
            "restsql+https",
        }
        .Contains( connectionString.Scheme ) )
        {
            throw new ArgumentException( "Invalid connection string. Invalid scheme." );
        }

        var scheme = ( connectionString.Scheme == "restsql+http" )
            ? "http"
            : "https";

        var query = HttpUtility.ParseQueryString( connectionString.Query );

        var database = query["database"];

        if ( string.IsNullOrWhiteSpace( database ) )
        {
            throw new ArgumentException( "Invalid connection string. Missing database name." );
        }

        var port = connectionString.Port > 0
            ? $":{connectionString.Port}"
            : string.Empty;

        httpClient.BaseAddress = new Uri(
            $"{scheme}://{connectionString.Host}{port}"
        );

        httpClient.Timeout = TimeSpan.FromSeconds( 120 );
        //httpClient.DefaultRequestHeaders.Add( "Accept", "application/json" );
        //httpClient.DefaultRequestVersion = HttpVersion.Version20;
        //httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

        var user = query["user"];
        var password = query["password"];

        if ( !string.IsNullOrWhiteSpace( password ) )
        {
            // set password as Basic Authentication header
            if ( !string.IsNullOrWhiteSpace( user ) )
            {
                httpClient.DefaultRequestHeaders.Authorization = new Net.Http.Headers.AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String( System.Text.Encoding.ASCII.GetBytes( $"{user}:{password}" ) )
                );
            }

            // hide password from connection string
            query["password"] = new string( '*', 8 );
        }

        return new RestSqlConnection(
            new RestClient( httpClient, null ),
            $"{connectionString.Scheme}://{connectionString.Host}{port}?{query}"
        );
    }
}
