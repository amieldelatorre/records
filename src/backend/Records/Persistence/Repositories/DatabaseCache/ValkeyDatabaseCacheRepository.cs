using Application.Common;
using Application.Repositories;
using Application.Repositories.DatabaseCache;

namespace Persistence.Repositories.DatabaseCache;

public class ValkeyDatabaseCacheRepository : IDatabaseCacheRepository
{
    public Task SetKey<T>(string key, T value, int expireSeconds)
    {
        throw new NotImplementedException();
    }

    public Task<CacheRetrievalResult<T>> GetKey<T>(string key)
    {
        throw new NotImplementedException();
    }

    public Task RemoveKey(string key)
    {
        throw new NotImplementedException();
    }
}