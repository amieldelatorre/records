using System.Diagnostics;
using Application.Common;
using Application.Features.WeightEntryFeatures;
using Application.Features.WeightEntryFeatures.CreateWeightEntry;
using Application.Features.WeightEntryFeatures.DeleteWeightEntry;
using Application.Features.WeightEntryFeatures.GetWeightEntry;
using Application.Features.WeightEntryFeatures.ListWeightEntry;
using Application.Features.WeightEntryFeatures.UpdateWeightEntry;
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
    private readonly ILogger<WeightEntryController> _logger;
    private readonly ClaimsInformation _claimsInformation;
    private readonly CreateWeightEntryHandler _createWeightEntryHandler;
    private readonly GetWeightEntryHandler _getWeightEntryHandler;
    private readonly ListWeightEntryHandler _listWeightEntryHandler;
    private readonly UpdateWeightEntryHandler _updateWeightEntryHandler;
    private readonly DeleteWeightEntryHandler _deleteWeightEntryHandler;
    
    public WeightEntryController(
        ILogger<WeightEntryController> logger,
        ClaimsInformation claimsInformation,
        CreateWeightEntryHandler createWeightEntryHandler,
        GetWeightEntryHandler getWeightEntryHandler,
        ListWeightEntryHandler listWeightEntryHandler,
        UpdateWeightEntryHandler updateWeightEntryHandler,
        DeleteWeightEntryHandler deleteWeightEntryHandler
    )
    {
        _logger = logger;
        _claimsInformation = claimsInformation;
        _createWeightEntryHandler = createWeightEntryHandler;
        _getWeightEntryHandler = getWeightEntryHandler;
        _listWeightEntryHandler = listWeightEntryHandler;
        _updateWeightEntryHandler = updateWeightEntryHandler;
        _deleteWeightEntryHandler = deleteWeightEntryHandler;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<WeightEntryResult>> Post(
        [FromBody] CreateWeightEntryRequest createWeightEntryRequest) {
        _logger.LogDebug("new request to create weight entry");
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
        _logger.LogDebug("new request to get weight entry");
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

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PaginatedResult<WeightEntryResponse>>> List([FromQuery] 
        ListWeightEntryQueryParameters queryParameters)
    {
        _logger.LogDebug("new request to list weight entry");
        var userId = _claimsInformation.UserId();
        // Only path part of the URL, does not include query parameters or host or protocol
        var requestPath = Request.Path.Value;
        Debug.Assert(requestPath != null);
        
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(Defaults.RequestTimeout);
        var result = await _listWeightEntryHandler.Handle(userId, queryParameters, requestPath, cancellationTokenSource.Token);
        return HttpResponseFromResult<PaginatedResult<WeightEntryResponse>>.Map(result);
        
    }

    [HttpPut("{weightEntryId}")]
    [Authorize]
    public async Task<ActionResult<WeightEntryResult>> Put(string weightEntryId,
        [FromBody] UpdateWeightEntryRequest updateWeightEntryRequest)
    {
        _logger.LogDebug("new request to update weight entry");
        var userId = _claimsInformation.UserId();
        
        if (!ValidGuid.IsValidGuid(weightEntryId, out var weightEntryIdGuid)) 
            return HttpResponseFromResult<WeightEntryResult>.Map(
                new WeightEntryResult(
                    ResultStatusTypes.ValidationError, ValidGuid.CreateErrorMessage(nameof(weightEntryId))));
        
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(Defaults.RequestTimeout);
        var result = await _updateWeightEntryHandler.Handle(userId, weightEntryIdGuid, updateWeightEntryRequest, cancellationTokenSource.Token);
        return HttpResponseFromResult<WeightEntryResult>.Map(result);
    }
    
    [HttpDelete("{weightEntryId}")]
    [Authorize]
    public async Task<ActionResult<WeightEntryResult>> Delete(string weightEntryId)
    {
        _logger.LogDebug("new request to delete weight entry");
        var userId = _claimsInformation.UserId();

        if (!ValidGuid.IsValidGuid(weightEntryId, out var weightEntryIdGuid)) 
            return HttpResponseFromResult<WeightEntryResult>.Map(
                new WeightEntryResult(
                    ResultStatusTypes.ValidationError, ValidGuid.CreateErrorMessage(nameof(weightEntryId))));
        
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(Defaults.RequestTimeout);
        var result = await _deleteWeightEntryHandler.Handle(userId, weightEntryIdGuid, cancellationTokenSource.Token);
        return HttpResponseFromResult<WeightEntryResult>.Map(result);
    }
}
