using Application.Common;
using Application.Repositories.Database;

namespace Application.Features.WeightEntryFeatures.ListWeightEntry;

public class ListWeightEntryHandler(
    IWeightEntryRepository weightEntryRepository,
    Serilog.ILogger logger)
{
    public async Task<PaginatedResult<WeightEntryResponse>> Handle(
        Guid userId, 
        ListWeightEntryQueryParameters queryParameters, 
        string requestPath,
        CancellationToken cancellationToken)
    {
        var validator = new ListWeightEntryQueryParametersValidator();
        var validationResult = await validator.ValidateAsync(queryParameters, cancellationToken);
        if (!validationResult.IsValid)
            return new PaginatedResult<WeightEntryResponse>(ResultStatusTypes.ValidationError, validationResult.ToDictionary());
        
        var totalCount = await weightEntryRepository.ListTotalCount(userId, queryParameters, cancellationToken);
        var weightEntryList = await weightEntryRepository.List(userId, queryParameters, cancellationToken);
        return new PaginatedResult<WeightEntryResponse>(
            ResultStatusTypes.Ok, 
            WeightEntryResponse.MapFrom(weightEntryList), 
            totalCount,
            queryParameters.Limit,
            queryParameters.Offset,
            requestPath);
    }
}