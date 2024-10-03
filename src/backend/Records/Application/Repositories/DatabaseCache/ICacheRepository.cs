using Application.Common;

namespace Application.Repositories.DatabaseCache;

public interface ICacheRepository
{
    Task Set<T>(string key, T value, int expireSeconds);
    Task<CacheRetrievalResult<T>> Get<T>(string key);
    Task Delete(string key);
}