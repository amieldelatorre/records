using Application.Features.WeightEntryFeatures;
using Application.Features.WeightEntryFeatures.CreateWeightEntry;
using Application.Repositories.Database;
using Domain.Entities;
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
public class WeightEntryController : Controller
{
    private readonly Serilog.ILogger _logger;
    private readonly ClaimsInformation _claimsInformation;
    private readonly CreateWeightEntryHandler _createWeightEntryHandler;

    public WeightEntryController(
        Serilog.ILogger logger,
        ClaimsInformation claimsInformation,
        CreateWeightEntryHandler createWeightEntryHandler
    )
    {
        _logger = logger;
        _claimsInformation = claimsInformation;
        _createWeightEntryHandler = createWeightEntryHandler;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<WeightEntryResult>> Post(
        [FromBody] CreateWeightEntryRequest createWeightEntryRequest) {
        _logger.Information("new request to create weight entry");
        var userId = _claimsInformation.UserId();
        var result = await _createWeightEntryHandler.Handle(userId, createWeightEntryRequest);
        return HttpResponseFromResult<WeightEntryResult>.Map(result);
    }
}