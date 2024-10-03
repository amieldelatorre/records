using Application.Common;

namespace Application.Features.UserFeatures;

public class UserResult : BaseResult
{
    public bool ShouldSerializeUser() => Errors.Count == 0;
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
}