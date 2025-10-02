using Application.Repositories.Database;
using FluentValidation;

namespace Application.Features.WeightEntryFeatures.CreateWeightEntry;

public class CreateWeightEntryValidator : AbstractValidator<CreateWeightEntryRequest>
{
    public CreateWeightEntryValidator(IWeightEntryRepository weightEntryRepository, Guid userId)
    {
        RuleFor(x => x.Value)
            .GreaterThan(0);
        
        RuleFor(x => x.EntryDate)
            .MustAsync(async (entryDate, cancellation) =>
            {
                var exists = await weightEntryRepository.WeightEntryExistsForDate(userId, entryDate, cancellation);
                return !exists;
            }).WithMessage("'EntryDate' contains duplicate entry, an entry for the date already exists.");
    }
}