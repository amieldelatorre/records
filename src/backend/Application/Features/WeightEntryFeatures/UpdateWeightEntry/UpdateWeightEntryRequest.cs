namespace Application.Features.WeightEntryFeatures.UpdateWeightEntry;

public class UpdateWeightEntryRequest
{
    public required decimal Value { get; set; }
    public string? Comment { get; set; }
    public required DateOnly EntryDate { get; set; }
}