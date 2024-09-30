using Application.Common;

namespace Application.Repositories.DatabaseCache;

public interface ICacheRepository
{
    Task SetKey<T>(string key, T value, int expireSeconds);
    Task<CacheRetrievalResult<T>> GetKey<T>(string key);
    Task RemoveKey(string key);
}