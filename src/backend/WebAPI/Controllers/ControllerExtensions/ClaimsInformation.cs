using System.Diagnostics;

namespace WebAPI.Controllers.ControllerExtensions;

public class ClaimsInformation(IHttpContextAccessor httpContextAccessor)
{
    public Guid UserId()
    {
        Debug.Assert(httpContextAccessor.HttpContext != null);
        var result = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId")?.Value;
        Debug.Assert(result != null);

        return Guid.Parse(result);
    }
}