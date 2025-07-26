using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.ControllerExtensions;

public static class HttpResponseFromResult<T> where T : BaseResult
{
    public static ActionResult<T> Map(T result)
    {
        var objectResult = new ObjectResult(result)
        {
            StatusCode = result.ResultStatus switch
            {
                ResultStatusTypes.Created => StatusCodes.Status201Created,
                ResultStatusTypes.Ok => StatusCodes.Status200OK,
                ResultStatusTypes.FeatureDisabled => StatusCodes.Status403Forbidden,
                ResultStatusTypes.InvalidCredentials => StatusCodes.Status401Unauthorized,
                ResultStatusTypes.NotFound => StatusCodes.Status404NotFound,
                ResultStatusTypes.ValidationError => StatusCodes.Status400BadRequest,
                ResultStatusTypes.ServerError => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError,
            }
        };
        return objectResult;
    }
}