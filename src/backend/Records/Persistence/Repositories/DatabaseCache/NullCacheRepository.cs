using Application.Common;
using Application.Repositories.DatabaseCache;

namespace Persistence.Repositories.DatabaseCache;

public class NullCacheRepository : ICacheRepository
{
    private static Task _emptyTask = Task.FromResult(0);

    public Task SetKey<T>(string key, T value, int expireSeconds)
    {
        return _emptyTask;
    }

    public Task<CacheRetrievalResult<T>> GetKey<T>(string key)
    {
        return Task.FromResult(new CacheRetrievalResult<T>(false ));
    }

    public Task RemoveKey(string key)
    {
        return _emptyTask;
    }
}