using Domain.Entities;

namespace Application.Repositories.Database;

public interface IWeightEntryRepository : IBaseRepository<WeightEntry>
{
    public Task<WeightEntry?> GetByUserId(Guid weightEntryId, Guid userId, CancellationToken cancellationToken);
    public Task<WeightEntry?> GetWeightEntryByDateAndUserId(Guid userId, DateOnly date, CancellationToken cancellationToken);
    public Task<bool> WeightEntryExistsForDate(Guid userId, DateOnly date, CancellationToken cancellationToken);
}