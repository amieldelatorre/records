using Application.Common;

namespace Application.Features.WeightEntryFeatures;

public class WeightEntryResult : BaseResult
{
    public bool ShouldSerializeWeightEntry() => Errors.Count == 0 && WeightEntry != null;
    public WeightEntryResponse? WeightEntry { get; set; }

    public WeightEntryResult(ResultStatusTypes resultStatus, Dictionary<string, List<string>> errors) : base(resultStatus, errors)
    {
        WeightEntry = null;
    }

    public WeightEntryResult(ResultStatusTypes resultStatus, IDictionary<string, string[]> errors) : base(resultStatus, errors)
    {
        WeightEntry = null;
    }

    public WeightEntryResult(ResultStatusTypes resultStatus, WeightEntryResponse weightEntry) : base(resultStatus)
    {
        WeightEntry = weightEntry;
    }

    public WeightEntryResult(ResultStatusTypes resultStatus) : base(resultStatus)
    {
        WeightEntry = null;
    }
}