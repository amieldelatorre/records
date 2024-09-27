namespace Application.Common;

public enum ResponseResultStatusTypes
{
    Ok,
    ValidationError,
}

public class BaseResponse<T>
{
    public bool ShouldSerializeItem() => Errors.Count == 0;
    public T? Item { get; set; }

    public bool ShouldSerializeResponseResultStatus() => false;
    public ResponseResultStatusTypes ResponseResultStatus { get; set; }

    public bool ShouldSerializeErrors() => Errors.Count > 0;
    public Dictionary<string, List<string>> Errors = new();

    public void AddError(string key, string message)
    {
        if (Errors.ContainsKey(key))
            Errors[key].Add(message);
        else
            Errors[key] = [message];
    }

}