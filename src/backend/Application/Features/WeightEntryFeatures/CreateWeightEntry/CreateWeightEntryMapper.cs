using Domain.Entities;

namespace Application.Features.WeightEntryFeatures.CreateWeightEntry;

public static class CreateWeightEntryMapper
{
    public static WeightEntry Map(Guid userId, CreateWeightEntryRequest createWeightEntryRequest)
    {
        var now = DateTime.UtcNow;
        return new WeightEntry
        {
            Value = createWeightEntryRequest.Value,
            Comment = createWeightEntryRequest.Comment?.Trim(),
            EntryDate = createWeightEntryRequest.EntryDate,
            UserId = userId,
            DateCreated = now,
            DateUpdated = now,
        };
    }
}