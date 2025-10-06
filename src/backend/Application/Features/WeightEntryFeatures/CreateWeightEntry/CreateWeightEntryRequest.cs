namespace Application.Features.WeightEntryFeatures.CreateWeightEntry;

public class CreateWeightEntryRequest
{
    public required decimal Value { get; set; }
    public string? Comment { get; set; }
    public required DateOnly EntryDate { get; set; }
}