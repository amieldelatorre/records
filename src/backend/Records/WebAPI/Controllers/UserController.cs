using Application.Features.UserFeatures.CreateUser;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/v1/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ApiController]
public class UserController : ControllerBase
{
   [HttpPost]
   public async Task<ActionResult<CreateUserResponse>> Post([FromBody] CreateUserRequest createUserRequest)
   {
      // TODO: Actual processing
      await Task.Delay(0);
      return Ok();
   }
}