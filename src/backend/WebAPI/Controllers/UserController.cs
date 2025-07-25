using Application.Features.UserFeatures;
using Application.Features.UserFeatures.CreateUser;
using Application.Features.UserFeatures.GetUser;
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
public class UserController : Controller
{
    private readonly Serilog.ILogger _logger;
    private readonly ClaimsInformation _claimsInformation;
    private readonly CreateUserHandler _createUserHandler;
    private readonly GetUserHandler _getUserHandler;
    
    public UserController(
        Serilog.ILogger logger,
        ClaimsInformation claimsInformation,
        CreateUserHandler createUserHandler,
        GetUserHandler getUserHandler
        )
    {
        _logger = logger;
        _claimsInformation = claimsInformation;
        _createUserHandler = createUserHandler;
        _getUserHandler = getUserHandler;
    }
    
    [HttpPost]
    public async Task<ActionResult<UserResult>> Post([FromBody] CreateUserRequest createUserRequest)
    {
        _logger.Debug("new request to create user");
        var result = await _createUserHandler.Handle(createUserRequest);
        return HttpResponseFromResult<UserResult>.Map(result);
    }   
    
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<UserResult>> Get()
    {
        _logger.Debug("new request to get user");
        var userId = _claimsInformation.UserId();
        var result = await _getUserHandler.Handle(userId);
        return HttpResponseFromResult<UserResult>.Map(result);
    }
}