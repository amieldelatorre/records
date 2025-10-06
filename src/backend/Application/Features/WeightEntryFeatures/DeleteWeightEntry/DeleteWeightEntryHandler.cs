using Application.Common;
using Application.Repositories.Database;

namespace Application.Features.WeightEntryFeatures.DeleteWeightEntry;

public class DeleteWeightEntryHandler(
    IWeightEntryRepository weightEntryRepository,
    Serilog.ILogger logger)
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