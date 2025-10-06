using FluentValidation;

namespace Application.Features.WeightEntryFeatures.ListWeightEntry;

public class ListWeightEntryQueryParametersValidator : AbstractValidator<ListWeightEntryQueryParameters>
{
    public ListWeightEntryQueryParametersValidator()
    {
        RuleFor(q => q.Limit).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(q => q.Offset).GreaterThanOrEqualTo(0);
        RuleFor(q => q)
            .Custom((q, context) =>
            {
                if (q.DateTo.HasValue && q.DateFrom.HasValue && q.DateFrom.Value > q.DateTo.Value)
                {
                    context.AddFailure(
                        "DateFilters",
                        $"'{nameof(ListWeightEntryQueryParameters.DateTo)}' must be greater than or equal " +
                        $"to '{nameof(ListWeightEntryQueryParameters.DateFrom)}'.");
                }
            });
    }
}