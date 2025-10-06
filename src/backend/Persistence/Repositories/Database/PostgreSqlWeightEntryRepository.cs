using Application.Features.WeightEntryFeatures.ListWeightEntry;
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

    public async Task<WeightEntry?> Get(Guid weightEntryId, Guid userId, CancellationToken cancellationToken)
    {
        var weightEntry = await dbContext.WeightEntries.FirstOrDefaultAsync(
            w => w.UserId == userId && w.Id == weightEntryId, cancellationToken);
        return weightEntry;
    }

    private IQueryable<WeightEntry> FilterList(Guid userId, ListWeightEntryQueryParameters queryParameters)
    {
        var weightEntries = dbContext.WeightEntries.Where(w => w.UserId == userId);
        if (queryParameters.DateFrom.HasValue)
            weightEntries = weightEntries.Where(w => w.EntryDate >= queryParameters.DateFrom.Value);
        if (queryParameters.DateTo.HasValue)
            weightEntries = weightEntries.Where(w => w.EntryDate <= queryParameters.DateTo.Value);

        return weightEntries;
    }

    public async Task<List<WeightEntry>> List(Guid userId, ListWeightEntryQueryParameters queryParameters, 
        CancellationToken cancellationToken)
    {
        var weightEntriesQuery = FilterList(userId, queryParameters);
                
        weightEntriesQuery = weightEntriesQuery.OrderByDescending(w => w.EntryDate)
            .Skip(queryParameters.Offset)
            .Take(queryParameters.Limit);
        return await weightEntriesQuery.ToListAsync(cancellationToken);
    }

    public async Task<int> ListTotalCount(Guid userId, ListWeightEntryQueryParameters queryParameters, CancellationToken cancellationToken)
    {
        var weightEntriesQuery = FilterList(userId, queryParameters);
        return await weightEntriesQuery.CountAsync(cancellationToken);
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

    public async Task Delete(WeightEntry weightEntry, CancellationToken cancellationToken)
    {
        dbContext.WeightEntries.Remove(weightEntry);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}