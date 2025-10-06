using Domain.Entities;

namespace Application.Features.WeightEntryFeatures.UpdateWeightEntry;

public static class UpdateWeightEntryMapper
{
    public static void Map(UpdateWeightEntryRequest updateWeightEntryRequest, WeightEntry originalWeightEntry)
    {
        var now = DateTime.UtcNow;
        originalWeightEntry.Value = updateWeightEntryRequest.Value;
        originalWeightEntry.Comment = updateWeightEntryRequest.Comment;
        originalWeightEntry.EntryDate = updateWeightEntryRequest.EntryDate;
        originalWeightEntry.DateUpdated = now;
    }
}