using Application.Features.UserFeatures;
using Application.Features.UserFeatures.CreateUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
public class UserController(
   Serilog.ILogger logger,
   CreateUserHandler createUserHandler
   ) : ControllerBase
{
   [HttpPost]
   public async Task<ActionResult<UserResult>> Post([FromBody] CreateUserRequest createUserRequest)
   {
      logger.Debug("new request to create user");
      var result = await createUserHandler.Handle(createUserRequest);
      return ControllerExtensions<UserResult>.HttpResponseFromResult(result);
   }

   [HttpGet]
   [Authorize]
   public async Task<ActionResult<UserResult>> Get()
   {
      await Task.Delay(0);
      throw new NotImplementedException();
   }
}