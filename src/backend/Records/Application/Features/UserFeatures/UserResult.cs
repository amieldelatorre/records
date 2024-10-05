using Application.Common;

namespace Application.Features.UserFeatures;

public class UserResult : BaseResult
{
    public bool ShouldSerializeUser() => Errors.Count == 0 && User != null;
    public UserResponse? User { get; set; }

    public UserResult(ResultStatusTypes resultStatus, Dictionary<string, List<string>> errors) : base(
        resultStatus, errors)
    {
        User = null;
    }

    public UserResult(ResultStatusTypes resultStatus, IDictionary<string, string[]> errors) : base(resultStatus, errors)
    {
        User = null;
    }

    public UserResult(ResultStatusTypes resultStatus, UserResponse user) : base(resultStatus)
    {
        User = user;
    }

    public UserResult(ResultStatusTypes resultStatus) : base(resultStatus)
    {
        User = null;
    }
}