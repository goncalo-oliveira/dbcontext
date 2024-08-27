using System.Net;
using System.Web;
using Faactory.RestClient;
using Faactory.RestSql;

namespace System.Data;

public sealed class RestSqlDbContext( IHttpClientFactory httpClientFactory, DbContextOptions contextOptions ) : IDbContext
{
    private readonly IHttpClientFactory clientFactory = httpClientFactory;
    private readonly DbContextOptions options = contextOptions;

    public string Name => options.Name;

    public DbContextProviderType Provider => DbContextProviderType.RestSql;

    private static readonly string[] sourceArray =
        [
            "restsql",
            "restsql+http",
            "restsql+https",
        ];

    public Common.DbConnection GetDbConnection()
    {
        var httpClient = clientFactory.CreateClient();

        var connectionString = new Uri( options.ConnectionString );

        // connection is secure by default
        // restsql://localhost:5001?database=restsql&user=restsql&password=restsql
        // is the same as
        // restsql+https://localhost:5001?database=restsql&user=restsql&password=restsql

        // unsafe http connection by setting the scheme to restsql+http
        // restsql+http://localhost:5000?database=restsql&user=restsql&password=restsql

        if ( !sourceArray.Contains( connectionString.Scheme ) )
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
        httpClient.DefaultRequestHeaders.Add( "Accept", "application/json" );
        httpClient.DefaultRequestVersion = HttpVersion.Version20;
        httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

        var user = query["user"];
        var password = query["password"];

        if ( !string.IsNullOrWhiteSpace( password ) )
        {
            // set password as Basic Authentication header
            if ( !string.IsNullOrWhiteSpace( user ) )
            {
                httpClient.DefaultRequestHeaders.Authorization = new Net.Http.Headers.AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(Text.Encoding.ASCII.GetBytes( $"{user}:{password}" ) )
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
