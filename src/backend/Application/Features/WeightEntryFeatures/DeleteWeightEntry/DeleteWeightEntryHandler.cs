using Application.Common;
using Application.Repositories.Database;
using Microsoft.Extensions.Logging;

namespace Application.Features.WeightEntryFeatures.DeleteWeightEntry;

public class DeleteWeightEntryHandler(
    IWeightEntryRepository weightEntryRepository,
    ILogger<DeleteWeightEntryHandler> logger)
{
    public async Task<WeightEntryResult> Handle(Guid userId, Guid weightEntryId, CancellationToken cancellationToken)
    {
        var weightEntry = await weightEntryRepository.Get(weightEntryId, userId, cancellationToken);
        if (weightEntry == null)
            return new WeightEntryResult(ResultStatusTypes.NotFound);

        await weightEntryRepository.Delete(weightEntry, cancellationToken);
        return new WeightEntryResult(ResultStatusTypes.Ok);
    }
}