using Application.Features.UserFeatures.CreateUser;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/v1/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ApiController]
public class UserController(
   Serilog.ILogger logger,
   CreateUserHandler createUserHandler
   ) : ControllerBase
{
   [HttpPost]
   public async Task<ActionResult<CreateUserResponse>> Post([FromBody] CreateUserRequest createUserRequest)
   {
      // TODO: Actual processing
      await Task.Delay(0);
      var result = await createUserHandler.Handle(createUserRequest);
      return Ok();
   }
}