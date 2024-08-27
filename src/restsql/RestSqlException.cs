#pragma warning disable IDE0130
namespace Faactory.RestSql;
#pragma warning restore IDE0130

public sealed class RestSqlException : Exception
{
    public RestSqlException()
    : base( "An error occurred while executing the request." )
    { }

    public RestSqlException( int statusCode, string? content ) : this()
    {
        StatusCode = statusCode;
        Content = content;
    }

    public RestSqlException( int statusCode, string? content, string message ) : base( message )
    {
        StatusCode = statusCode;
        Content = content;
    }

    public RestSqlException( string message ) : base( message )
    { }

    public RestSqlException( string message, Exception innerException ) : base( message, innerException )
    { }

    public int? StatusCode { get; }
    public string? Content { get; }
}
