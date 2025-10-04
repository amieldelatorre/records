using Application.Common;
using Application.Repositories.Database;

namespace Application.Features.WeightEntryFeatures.GetWeightEntry;

public class GetWeightEntryHandler(
    IWeightEntryRepository weightEntryRepository,
    Serilog.ILogger logger)
{
    private const string FeatureName  = "WeightEntryGet";

    public async Task<WeightEntryResult> Handle(Guid userId, Guid weightEntryId)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(
            Application.Features.SharedConfiguration.DefaultRequestTimeout));
        
        var weightEntry = await weightEntryRepository.Get(weightEntryId, userId, cancellationTokenSource.Token);
        if (weightEntry == null)
            return new WeightEntryResult(ResultStatusTypes.NotFound);
        return new WeightEntryResult(ResultStatusTypes.Ok, WeightEntryResponse.MapFrom(weightEntry));
    }
}