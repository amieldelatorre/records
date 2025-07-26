using Application.Features.AuthFeatures.Jwt.JwtCreate;
using Microsoft.AspNetCore.Mvc;
using Application.Features.AuthFeatures.Login;
using WebAPI.Controllers.ControllerExtensions;

namespace WebAPI.Controllers;

[Route("api/v1/[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        return HttpResponseFromResult<JwtCreateResult>.Map(result);
    }
}