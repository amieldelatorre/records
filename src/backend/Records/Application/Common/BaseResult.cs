namespace Application.Common;

public enum ResultStatusTypes
{
    Created,
    Ok,
    FeatureDisabled,
    InvalidCredentials,
    NotFound,
    ValidationError,
    ServerError,
}

public class BaseResult
{
    public bool ShouldSerializeResultStatus() => false;
    public ResultStatusTypes ResultStatus { get; set; }

    public bool ShouldSerializeErrors() => Errors.Count > 0;
    public Dictionary<string, List<string>> Errors = new();

    public BaseResult(ResultStatusTypes resultStatus, Dictionary<string, List<string>> errors)
    {
        ResultStatus = resultStatus;
        Errors = errors;
    }

    public BaseResult(ResultStatusTypes resultStatus, IDictionary<string, string[]> errors)
    {
        ResultStatus = resultStatus;
        foreach (var field in errors)
        {
            foreach (var errorMessage in field.Value)
                AddError(field.Key, errorMessage);
        }
    }

    public BaseResult(ResultStatusTypes resultStatus)
    {
        ResultStatus = resultStatus;
    }

    public void AddError(string key, string message)
    {
        if (Errors.ContainsKey(key))
            Errors[key].Add(message);
        else
            Errors[key] = [message];
    }
}