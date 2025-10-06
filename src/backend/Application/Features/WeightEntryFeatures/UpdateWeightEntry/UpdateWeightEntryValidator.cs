using Application.Repositories.Database;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Validators;

namespace Application.Features.WeightEntryFeatures.UpdateWeightEntry;

public class UpdateWeightEntryValidator : AbstractValidator<UpdateWeightEntryRequest>
{
    public UpdateWeightEntryValidator(IWeightEntryRepository weightEntryRepository, Guid userId, WeightEntry originalWeightEntry)
    {
        RuleFor(x => x.Value)
            .GreaterThan(0);
        
        var utc14 = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Kiritimati");
        var maxEntryDateTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, utc14).Date;
        var maxEntryDate = DateOnly.FromDateTime(maxEntryDateTime);

        RuleFor(x => x.EntryDate)
            .LessThanOrEqualTo(maxEntryDate)
            .WithMessage("'EntryDate' cannot be greater than the current date in UTC+14");
        RuleFor(x => x.EntryDate)
            .MustAsync(async (entryDate, cancellation) =>
            {
                var exists = await weightEntryRepository.WeightEntryExistsForDate(userId, entryDate, cancellation);
                return !exists;
            })
            .When(x => x.EntryDate != originalWeightEntry.EntryDate)
            .WithMessage("'EntryDate' caught creating duplicate entry, an entry for the date already exists. " +
                           "Only one entry per date is allowed.");
    }
}