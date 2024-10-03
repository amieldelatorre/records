namespace Common.ExpectedValues;

public class ExpectedValidationResult
{
    public bool Result { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
}