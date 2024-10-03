using Application.Common;
using Newtonsoft.Json;

namespace WebAPI.Extensions;

public class ExceptionMiddleware(RequestDelegate next, Serilog.ILogger logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            logger.Error("unhandled exception occured. {error}", ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var errors = new Dictionary<string, List<string>>{{"Server", ["Cannot handle your request right now. Please try again later."]}};
        var result = new BaseResult(ResultStatusTypes.ServerError, errors);
        await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(result));
    }
}