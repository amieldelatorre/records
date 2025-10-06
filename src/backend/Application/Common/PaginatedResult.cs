namespace Application.Common;

public class PaginatedResult<T> : BaseResult
{
    public bool ShouldSerializeItems() => Errors.Count == 0;
    public List<T> Items { get; set; } = [];

    // This is the total number of results the match the query, not the number of results in the items list
    public bool ShouldSerializeTotal() => Errors.Count == 0;
    public int Total { get; set; } 

    public bool ShouldSerializeLimit() => Errors.Count == 0;
    public int Limit { get; set; } 

    public bool ShouldSerializeOffset() => Errors.Count == 0;
    public int Offset { get; set; }

    public bool ShouldSerializeNext() => Errors.Count == 0 && !string.IsNullOrEmpty(Next);
    public string? Next { get; set; } = null;

    public PaginatedResult(ResultStatusTypes resultStatus, Dictionary<string, List<string>> errors) : base(resultStatus, errors)
    {
        ResultStatus = resultStatus;
        Errors = errors;
    }

    public PaginatedResult(ResultStatusTypes resultStatus, IDictionary<string, string[]> errors) : base(resultStatus, errors)
    {
        ResultStatus = resultStatus;
    }

    public PaginatedResult(ResultStatusTypes resultStatus, List<T> items, int total, int limit, int offset, string requestPath) : base(resultStatus)
    {
        ResultStatus = resultStatus;
        Items = items;
        Total = total;
        Limit = limit;
        Offset = offset;
        
        if (limit + offset < total)
            Next = $"{requestPath}?limit={limit}&offset={limit + offset}";
    }
    
}