using System;
using System.Data;
using System.Data.Common;
using Faactory.RestClient;

namespace Faactory.RestSql;

internal sealed class RestSqlConnection : DbConnection
{
    public RestSqlConnection( IRestClient restClient, string connectionString )
    {
        Client = restClient;
        ConnectionString = connectionString;

        Database = System.Web.HttpUtility.ParseQueryString( new Uri( connectionString )
            .Query )["database"]
            ?? throw new ArgumentException( "Invalid connection string. Missing database name." );

        var signingKey = System.Web.HttpUtility.ParseQueryString( new Uri( connectionString )
            .Query )["signingKey"];

        if ( !string.IsNullOrEmpty( signingKey ) )
        {
            SigningKey = Convert.FromBase64String( signingKey );

            // hide the signing key from the connection string
            ConnectionString = ConnectionString.Replace( $"signingKey={signingKey}", "signingKey=********" );
            ConnectionString = ConnectionString.Replace( $"signingKey={System.Web.HttpUtility.UrlEncode( signingKey )}", "signingKey=********" );
        }
    }

    internal IRestClient Client { get; }

    internal byte[]? SigningKey { get; }

    #pragma warning disable CS8765 // disable nullability of properties

    public override string ConnectionString { get; set; }

    #pragma warning restore CS8765

    public override string Database { get; } = string.Empty;

    public override string DataSource { get; } = string.Empty;

    public override string ServerVersion { get; } = string.Empty;

    public override ConnectionState State => ConnectionState.Open;

    public override void ChangeDatabase( string databaseName )
    {
        // does nothing
    }

    public override void Close()
    {
        // does nothing
    }

    public override void Open()
    {
        // does nothing
    }

    protected override DbTransaction BeginDbTransaction( IsolationLevel isolationLevel )
    {
        throw new NotSupportedException( "Transactions are not supported" );
    }

    protected override DbCommand CreateDbCommand()
    {
        return new RestSqlCommand( this );
    }
}
