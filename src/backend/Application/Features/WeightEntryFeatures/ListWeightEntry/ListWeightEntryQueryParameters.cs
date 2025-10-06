namespace Application.Features.WeightEntryFeatures.ListWeightEntry;

public class ListWeightEntryQueryParameters
{
    // These DateOnly fields are inclusive
    public DateOnly? DateFrom { get; set; } = null;
    public DateOnly? DateTo { get; set; } = null;
    public int Limit { get; set; } = 30;
    public int Offset { get; set; } = 0;
}