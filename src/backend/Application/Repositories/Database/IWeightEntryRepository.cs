using Domain.Entities;

namespace Application.Repositories.Database;

public interface IWeightEntryRepository : IBaseRepository<WeightEntry>
{
    public Task<WeightEntry?> GetByUserId(Guid weightEntryId, Guid userId, CancellationToken cancellationToken);
}