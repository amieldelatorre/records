using FluentValidation;

namespace Application.Features.WeightEntryFeatures.CreateWeightEntry;

public class CreateWeightEntryValidator : AbstractValidator<CreateWeightEntryRequest>
{
    public CreateWeightEntryValidator()
    {
        RuleFor(x => x.Value)
            .GreaterThan(0);
    }
}