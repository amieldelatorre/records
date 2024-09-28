using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

public static class ControllerExtensions
{
    public static IActionResult HttpResponseFromResult(this ControllerBase controller)
    {
        // TODO: Get the correct result type and return that as HTTP result
        throw new NotImplementedException();
        return controller.Ok();
    }
}