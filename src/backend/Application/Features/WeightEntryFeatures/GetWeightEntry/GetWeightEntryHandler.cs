using Application.Common;
using Application.Repositories.Database;
using Microsoft.Extensions.Logging;

namespace Application.Features.WeightEntryFeatures.GetWeightEntry;

public class GetWeightEntryHandler(
    IWeightEntryRepository weightEntryRepository,
    ILogger<GetWeightEntryHandler> logger)
{
    private const string FeatureName  = "WeightEntryGet";

    public async Task<WeightEntryResult> Handle(Guid userId, Guid weightEntryId, CancellationToken cancellationToken)
    {
        var weightEntry = await weightEntryRepository.Get(weightEntryId, userId, cancellationToken);
        if (weightEntry == null)
            return new WeightEntryResult(ResultStatusTypes.NotFound);
        return new WeightEntryResult(ResultStatusTypes.Ok, WeightEntryResponse.MapFrom(weightEntry));
    }
}