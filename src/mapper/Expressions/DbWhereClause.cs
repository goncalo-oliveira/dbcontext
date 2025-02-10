namespace System.Data.Mapper.Expressions;

internal sealed class DbWhereClause
{
    public required string WhereClause { get; init; }
    public required IReadOnlyDictionary<string, object> Parameters { get; init; }

    public override string ToString() => WhereClause;
}
