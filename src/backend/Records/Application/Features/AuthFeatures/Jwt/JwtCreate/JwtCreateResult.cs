using Application.Common;

namespace Application.Features.AuthFeatures.Jwt.JwtCreate;

public class JwtCreateResult : BaseResult
{
    public bool ShouldSerializeCredentials() => Errors.Count == 0;
    public JwtCreateResponse? Credentials { get; set; }

    public JwtCreateResult(ResultStatusTypes status, Dictionary<string, List<string>> errors) : base(status, errors)
    {
        Credentials = null;
    }

    public JwtCreateResult(ResultStatusTypes status, JwtCreateResponse credentials) : base(status)
    {
        Credentials = credentials;
    }
}