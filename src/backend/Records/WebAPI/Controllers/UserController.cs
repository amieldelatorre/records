using Application.Features.UserFeatures;
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
   public async Task<ActionResult<UserResult>> Post([FromBody] CreateUserRequest createUserRequest)
   {
      var result = await createUserHandler.Handle(createUserRequest);
      return ControllerExtensions<UserResult>.HttpResponseFromResult(result);
   }

   [HttpGet]
   public async Task<ActionResult<UserResult>> Get()
   {
      await Task.Delay(0);
      throw new NotImplementedException();
   }
}