using Application.Repositories.Database;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories.Database;

public class PostgreSqlWeightEntryRepository(DataContext dbContext) : IWeightEntryRepository
{
    public async Task Create(WeightEntry entity, CancellationToken cancellationToken)
    {
        dbContext.WeightEntries.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<WeightEntry?> GetByUserId(Guid weightEntryId, Guid userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<WeightEntry?> GetWeightEntryByDateAndUserId(Guid userId, DateOnly entryDate, CancellationToken cancellationToken)
    {
        var weightEntry = await dbContext.WeightEntries.FirstOrDefaultAsync(
            w => w.UserId == userId && w.EntryDate == entryDate, cancellationToken);
        return weightEntry;
    }

    public async Task<bool> WeightEntryExistsForDate(Guid userId, DateOnly date, CancellationToken cancellationToken)
    {
        var weightEntry = await GetWeightEntryByDateAndUserId(userId, date, cancellationToken);
        return weightEntry != null;
    }

    public Task<WeightEntry?> Get(Guid guid, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Update(WeightEntry entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Delete(WeightEntry entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}