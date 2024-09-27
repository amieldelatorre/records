namespace WebAPI.Extensions;

public class ExceptionMiddleware(RequestDelegate next)
{
    // TODO: Handle exceptions
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await Task.Delay(0);
        // TODO: Create BaseResponse object and Add to Base response errors
        // await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(baseresponseobject);
    }
}