using Application.Common;
using Application.Features.WeightEntryFeatures;
using Application.Features.WeightEntryFeatures.CreateWeightEntry;
using Application.Features.WeightEntryFeatures.GetWeightEntry;
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
    private readonly GetWeightEntryHandler _getWeightEntryHandler;

    public WeightEntryController(
        Serilog.ILogger logger,
        ClaimsInformation claimsInformation,
        CreateWeightEntryHandler createWeightEntryHandler,
        GetWeightEntryHandler getWeightEntryHandler
    )
    {
        _logger = logger;
        _claimsInformation = claimsInformation;
        _createWeightEntryHandler = createWeightEntryHandler;
        _getWeightEntryHandler = getWeightEntryHandler;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<WeightEntryResult>> Post(
        [FromBody] CreateWeightEntryRequest createWeightEntryRequest) {
        _logger.Debug("new request to create weight entry");
        var userId = _claimsInformation.UserId();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(Defaults.RequestTimeout);
        var result = await _createWeightEntryHandler.Handle(userId, createWeightEntryRequest, 
            cancellationTokenSource.Token);
        return HttpResponseFromResult<WeightEntryResult>.Map(result);
    }

    [HttpGet("{weightEntryId}")]
    [Authorize]
    public async Task<ActionResult<WeightEntryResult>> Get(string weightEntryId)
    {
        _logger.Debug("new request to get weight entry");
        var userId = _claimsInformation.UserId();

        if (!ValidGuid.IsValidGuid(weightEntryId, out var weightEntryIdGuid)) 
            return HttpResponseFromResult<WeightEntryResult>.Map(
                new WeightEntryResult(
                    ResultStatusTypes.ValidationError, ValidGuid.CreateErrorMessage(nameof(weightEntryId))));
        
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(Defaults.RequestTimeout);
        var result = await _getWeightEntryHandler.Handle(userId, weightEntryIdGuid, cancellationTokenSource.Token);
        return HttpResponseFromResult<WeightEntryResult>.Map(result);
    }
}