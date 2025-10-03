using Application.Repositories.Database;
using FluentValidation;

namespace Application.Features.WeightEntryFeatures.CreateWeightEntry;

public class CreateWeightEntryValidator : AbstractValidator<CreateWeightEntryRequest>
{
    public CreateWeightEntryValidator(IWeightEntryRepository weightEntryRepository, Guid userId)
    {
        RuleFor(x => x.Value)
            .GreaterThan(0);
        
        var utc14 = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Kiritimati");
        var maxEntryDateTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, utc14).Date;
        var maxEntryDate = DateOnly.FromDateTime(maxEntryDateTime);
        
        RuleFor(x => x.EntryDate)
            .LessThanOrEqualTo(maxEntryDate).WithMessage("'EntryDate' cannot be greater than the current date in UTC+14")
            .MustAsync(async (entryDate, cancellation) =>
            {
                var exists = await weightEntryRepository.WeightEntryExistsForDate(userId, entryDate, cancellation);
                return !exists;
            }).WithMessage("'EntryDate' caught creating duplicate entry, an entry for the date already exists. " +
                           "Only one entry per date is allowed.");
        
    }
}