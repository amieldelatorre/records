using Application.Common;
using Application.Repositories.Database;

namespace Application.Features.WeightEntryFeatures.CreateWeightEntry;

public class CreateWeightEntryHandler(
    IWeightEntryRepository weightEntryRepository,
    Serilog.ILogger logger)
{
    private const string FeatureName = "WeightEntryCreate";

    public async Task<WeightEntryResult> Handle(Guid userId, CreateWeightEntryRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateWeightEntryValidator(weightEntryRepository, userId);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return new WeightEntryResult(ResultStatusTypes.ValidationError, validationResult.ToDictionary());
        
        var weightEntry = CreateWeightEntryMapper.Map(userId, request);
        await weightEntryRepository.Create(weightEntry, cancellationToken);
        var result = new WeightEntryResult(ResultStatusTypes.Created, WeightEntryResponse.MapFrom(weightEntry));
        return result;
    }
}