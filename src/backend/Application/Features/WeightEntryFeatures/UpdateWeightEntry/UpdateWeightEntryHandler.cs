using Application.Common;
using Application.Repositories.Database;

namespace Application.Features.WeightEntryFeatures.UpdateWeightEntry;

public class UpdateWeightEntryHandler(
    IWeightEntryRepository weightEntryRepository,
    Serilog.ILogger logger)
{
    public async Task<WeightEntryResult> Handle(Guid userId, Guid weightEntryId, 
        UpdateWeightEntryRequest request, CancellationToken cancellationToken)
    {
        var weightEntry = await weightEntryRepository.Get(weightEntryId, userId, cancellationToken);
        if (weightEntry == null)
            return new WeightEntryResult(ResultStatusTypes.NotFound);

        var validator = new UpdateWeightEntryValidator(weightEntryRepository, userId, weightEntry);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return new WeightEntryResult(ResultStatusTypes.ValidationError, validationResult.ToDictionary()); 
        
        UpdateWeightEntryMapper.Map(request, weightEntry);
        await weightEntryRepository.Update(weightEntry, cancellationToken);
        return new WeightEntryResult(ResultStatusTypes.Ok, WeightEntryResponse.MapFrom(weightEntry));
    }
}