using System;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Faactory.RestClient;

namespace Faactory.RestSql;

internal sealed class RestSqlCommand : DbCommand
{
    private static readonly JsonDocument EmptyJsonDocument = JsonDocument.Parse( "[]" );
    private readonly IRestClient client;
    private readonly byte[]? signingKey;

    public RestSqlCommand( RestSqlConnection restSqlConnection )
    {
        client = restSqlConnection.Client;
        signingKey = restSqlConnection.SigningKey;

        CommandText = string.Empty;
        DbConnection = restSqlConnection;
        DbParameterCollection = new RestSqlDbParameterCollection();
    }

    #pragma warning disable CS8765 // disable nullability of properties
    
    public override string CommandText { get; set; }

    #pragma warning restore CS8765

    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    protected override DbConnection? DbConnection { get; set; }

    protected override DbParameterCollection DbParameterCollection { get; }

    protected override DbTransaction? DbTransaction { get; set; }

    public override void Cancel()
    {
        throw new NotImplementedException();
    }

    public override int ExecuteNonQuery()
    {
        _ = Send();

        return ( 0 );
    }

    public override async Task<int> ExecuteNonQueryAsync( CancellationToken cancellationToken )
    {
        _ = await SendAsync();

        return ( 0 );
    }

    public override object ExecuteScalar()
    {
        var jsonDocument = Send();

        var jsonValue = jsonDocument.RootElement.EnumerateArray().First().EnumerateObject().First().Value;

        return jsonValue.GetValue()
            ?? DBNull.Value;
    }

    public override async Task<object?> ExecuteScalarAsync( CancellationToken cancellationToken )
    {
        var jsonDocument = await SendAsync();

        var jsonValue = jsonDocument.RootElement.EnumerateArray().First().EnumerateObject().First().Value;

        return jsonValue.GetValue()
            ?? DBNull.Value;
    }

    public override void Prepare()
    {
        throw new NotImplementedException();
    }

    protected override DbParameter CreateDbParameter() => new RestSqlParameter();

    protected override DbDataReader ExecuteDbDataReader( CommandBehavior behavior )
    {
        var jsonDocument = Send();

        return new RestSqlDataReader( jsonDocument );
    }

    protected override async Task<DbDataReader> ExecuteDbDataReaderAsync( CommandBehavior behavior, CancellationToken cancellationToken )
    {
        var jsonDocument = await SendAsync();

        return new RestSqlDataReader( jsonDocument );
    }

    private JsonDocument Send()
        => SendAsync().ConfigureAwait( false ).GetAwaiter().GetResult();

    private async Task<JsonDocument> SendAsync()
    {
        /*
        if parameters exist, we need to add them to the sql query
        */
        var commandText = GetCommandTextWithParameters();
        var url = $"/execute/{DbConnection!.Database}";

        // create request
        var request = new HttpRequestMessage( HttpMethod.Post, url )
        {
            Content = new StringContent( commandText, Encoding.UTF8, "text/plain" ),
        };

        /*
        bug on restsql side; if the content type includes the charset, the request fails
        https://github.com/goncalo-oliveira/restsql/issues/4
        */
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( "text/plain" );

        /*
        HMAC signing
        */
        if ( signingKey != null )
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            // create sha256 hash of the command text
            var sha256 = SHA256.HashData( Encoding.UTF8.GetBytes( commandText ) );

            // create signature
            var bytes = new List<byte>();

            bytes.AddRange( Encoding.UTF8.GetBytes( timestamp ) );
            bytes.AddRange( sha256 );
            bytes.AddRange( Encoding.UTF8.GetBytes( url ) );
            bytes.AddRange( Encoding.UTF8.GetBytes( "POST" ) );

            var hash = HMACSHA256.HashData( signingKey, bytes.ToArray() );
            var signature = Convert.ToBase64String( hash );

            request.Headers.Add( "X-Request-Timestamp", timestamp );
            request.Headers.Add( "X-Content-Sha256", Convert.ToBase64String( sha256 ) );
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "HMAC-SHA256",
                $"SignedHeaders=x-request-timestamp;x-content-sha256&Signature={signature}"
            );
        }

        using var cts = new CancellationTokenSource( CommandTimeout * 1000 );
        try
        {
            var response = await client.HttpClient.SendAsync( request, cts.Token ).ConfigureAwait( false );

            cts.Token.ThrowIfCancellationRequested();

            var responseContent = await response.Content.ReadAsByteArrayAsync().ConfigureAwait( false );

            if ( !response.IsSuccessStatusCode )
            {

                throw new RestSqlException( (int)response.StatusCode, Encoding.UTF8.GetString( responseContent ) );
            }

            if ( !responseContent.Any() )
            {
                return EmptyJsonDocument;
            }

            return JsonDocument.Parse( responseContent );
        }
        catch ( OperationCanceledException )
        {
            throw new TimeoutException();
        }
    }

    /// <summary>
    /// Returns the command text and parameters applied
    /// </summary>
    private string GetCommandTextWithParameters()
    {
        var declare = new StringBuilder();
        var values = new StringBuilder();

        var parameters = DbParameterCollection;

        foreach ( RestSqlParameter parameter in parameters )
        {
            declare.AppendLine( $"DECLARE {parameter.ParameterName} {parameter.GetSqlType()};" );
            values.AppendLine( $"SET {parameter.ParameterName} = {parameter.GetSqlValue()};" );
        }

        var sql = new StringBuilder();

        sql.AppendLine( declare.ToString() )
            .AppendLine()
            .AppendLine( values.ToString() )
            .AppendLine()
            .AppendLine( CommandText );

        return sql.ToString();
    }
}
