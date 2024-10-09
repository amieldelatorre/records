using Application.Common;
using Application.Features.UserFeatures;
using Application.Features.UserFeatures.CreateUser;
using Application.Features.UserFeatures.DeleteUser;
using Application.Features.UserFeatures.GetUser;
using Application.Features.UserFeatures.UpdateUser;
using Application.Features.UserFeatures.UpdateUserPassword;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
public class UserController(
   Serilog.ILogger logger,
   ClaimsInformation claimsInformation,
   CreateUserHandler createUserHandler,
   GetUserHandler getUserHandler,
   UpdateUserHandler updateUserHandler,
   UpdateUserPasswordHandler updateUserPasswordHandler,
   DeleteUserHandler deleteUserHandler) : ControllerBase
{
   [HttpPost]
   public async Task<ActionResult<UserResult>> Post([FromBody] CreateUserRequest createUserRequest)
   {
      logger.Debug("new request to create user");
      var result = await createUserHandler.Handle(createUserRequest);
      return HttpResponseFromResult<UserResult>.Map(result);
   }

   [HttpGet]
   [Authorize]
   public async Task<ActionResult<UserResult>> Get()
   {
      logger.Debug("new request to get user");
      var userId = claimsInformation.UserId();
      var result = await getUserHandler.Handle(userId);
      return HttpResponseFromResult<UserResult>.Map(result);
   }

   [HttpPut]
   [Authorize]
   public async Task<ActionResult<UserResult>> Put([FromBody] UpdateUserRequest updateUserRequest)
   {
      logger.Debug("new request to update user");
      var userId = claimsInformation.UserId();
      var result = await updateUserHandler.Handle(userId, updateUserRequest);
      return HttpResponseFromResult<UserResult>.Map(result);
   }

   [HttpPut("{userId}/password")]
   [Authorize]
   public async Task<ActionResult<UserResult>> PutPassword(string userId, [FromBody] UpdateUserPasswordRequest updateUserPasswordRequest)
   {
      logger.Debug("new request to update user password");
      var claimsUserId = claimsInformation.UserId();
      var paramUserId = new Guid(userId);
      if (claimsUserId != paramUserId)
         return HttpResponseFromResult<UserResult>.Map(new UserResult(ResultStatusTypes.NotFound));

      var result = await updateUserPasswordHandler.Handle(claimsUserId, updateUserPasswordRequest);
      return HttpResponseFromResult<UserResult>.Map(result);
   }

   [HttpDelete]
   [Authorize]
   public async Task<ActionResult<UserResult>> Delete()
   {
      logger.Debug("new request to delete user");
      var userId = claimsInformation.UserId();
      var result = await deleteUserHandler.Handle(userId);
      return HttpResponseFromResult<UserResult>.Map(result);
   }
}