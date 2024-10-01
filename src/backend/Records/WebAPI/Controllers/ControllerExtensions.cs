using Application.Common;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

public static class ControllerExtensions<T> where T : BaseResult
{
    public static ActionResult HttpResponseFromResult(T result)
    {
        var objectResult = new ObjectResult(result)
        {
            StatusCode = result.ResultStatus switch
            {
                ResultStatusTypes.Created => StatusCodes.Status201Created,
                ResultStatusTypes.Ok => StatusCodes.Status200OK,
                ResultStatusTypes.ValidationError => StatusCodes.Status400BadRequest,
                ResultStatusTypes.FeatureDisabled => StatusCodes.Status403Forbidden,
                ResultStatusTypes.ServerError => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError,
            }
        };
        return objectResult;
    }
}