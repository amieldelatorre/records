using Application.Common;
using Application.Repositories.DatabaseCache;

namespace Persistence.Repositories.DatabaseCache;

public class NullCacheRepository : ICacheRepository
{

    public Task Set<T>(string key, T value, int expireSeconds)
    {
        return Task.CompletedTask;
    }

    public Task<CacheRetrievalResult<T>> Get<T>(string key)
    {
        return Task.FromResult(new CacheRetrievalResult<T>(false));
    }

    public Task Delete(string key)
    {
        return Task.CompletedTask;
    }
}