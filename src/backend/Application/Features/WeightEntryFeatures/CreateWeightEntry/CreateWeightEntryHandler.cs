using Application.Common;
using Application.Repositories.Database;

namespace Application.Features.WeightEntryFeatures.CreateWeightEntry;

public class CreateWeightEntryHandler(
    IWeightEntryRepository weightEntryRepository,
    Serilog.ILogger logger)
{
    private const string FeatureName = "WeightEntryCreate";

    public async Task<WeightEntryResult> Handle(Guid userId, CreateWeightEntryRequest request)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(
            TimeSpan.FromSeconds(Application.Features.SharedConfiguration.DefaultRequestTimeout));
        
        var validator = new CreateWeightEntryValidator(weightEntryRepository, userId);
        var validationResult = await validator.ValidateAsync(request, cancellationTokenSource.Token);
        if (!validationResult.IsValid)
            return new WeightEntryResult(ResultStatusTypes.ValidationError, validationResult.ToDictionary());
        
        // Recreate cancellation token
        cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(
            Application.Features.SharedConfiguration.DefaultRequestTimeout));
        var weightEntry = CreateWeightEntryMapper.Map(userId, request);
        await weightEntryRepository.Create(weightEntry, cancellationTokenSource.Token);
        var result = new WeightEntryResult(ResultStatusTypes.Created, WeightEntryResponse.MapFrom(weightEntry));
        return result;
    }
}