using Application.Features.AuthFeatures.Jwt.JwtCreate;
using Microsoft.AspNetCore.Mvc;
using Application.Features.AuthFeatures.Jwt.JwtCreate;
using Application.Features.AuthFeatures.Login;

namespace WebAPI.Controllers;

[Route("api/v1/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ApiController]
public class AuthController(
    Serilog.ILogger logger,
    JwtCreateHandler jwtCreateHandler
    ) : ControllerBase
{
    [HttpPost("jwt")]
    public async Task<ActionResult<JwtCreateResult>> JwtCreate(LoginRequest loginRequest)
    {
        logger.Debug("new request to create JWT token");
        var result = await jwtCreateHandler.Handle(loginRequest);
        return ControllerExtensions<JwtCreateResult>.HttpResponseFromResult(result);
    }
}