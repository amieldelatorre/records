using Application.Common;
using Domain.Entities;

namespace Application.Features.WeightEntryFeatures;

public class WeightEntryResponse : BaseEntityResponse
{
    public required double Value { get; init; }
    public string? Comment { get; init; }
    public required DateOnly EntryDate { get; init; }
    public required Guid UserId { get; init; }

    public static WeightEntryResponse MapFrom(WeightEntry weightEntry)
    {
        return new WeightEntryResponse
        {
            Id = weightEntry.Id,
            Comment = weightEntry.Comment,
            Value = weightEntry.Value,
            EntryDate = weightEntry.EntryDate,
            UserId = weightEntry.UserId,
            DateCreated = weightEntry.DateCreated,
            DateUpdated = weightEntry.DateUpdated
        };
    }
}