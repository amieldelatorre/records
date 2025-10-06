using Application.Features.WeightEntryFeatures.ListWeightEntry;
using Domain.Entities;

namespace Application.Repositories.Database;

public interface IWeightEntryRepository : IBaseRepository<WeightEntry>
{
    public Task<WeightEntry?> Get(Guid weightEntryId, Guid userId, CancellationToken cancellationToken);
    public Task<List<WeightEntry>> List(Guid userId, ListWeightEntryQueryParameters queryParameters, CancellationToken cancellationToken);
    public Task<int> ListTotalCount(Guid userId, ListWeightEntryQueryParameters queryParameters, CancellationToken cancellationToken);
    public Task<WeightEntry?> GetWeightEntryByDateAndUserId(Guid userId, DateOnly date, CancellationToken cancellationToken);
    public Task<bool> WeightEntryExistsForDate(Guid userId, DateOnly date, CancellationToken cancellationToken);
}