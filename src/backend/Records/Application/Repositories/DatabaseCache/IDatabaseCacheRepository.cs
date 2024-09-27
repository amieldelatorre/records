using Application.Common;

namespace Application.Repositories.DatabaseCache;

public interface IDatabaseCacheRepository
{
    Task SetKey<T>(string key, T value, int expireSeconds);
    Task<CacheRetrievalResult<T>> GetKey<T>(string key);
    Task RemoveKey(string key);
}