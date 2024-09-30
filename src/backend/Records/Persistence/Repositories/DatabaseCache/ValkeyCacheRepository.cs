using System.Diagnostics;
using Application.Common;
using Application.Repositories;
using Application.Repositories.DatabaseCache;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Persistence.Repositories.DatabaseCache;

public class ValkeyCacheRepository(IDatabase valkeyDatabase) : ICacheRepository
{
    public async Task SetKey<T>(string key, T value, int expireSeconds)
    {
        var json = JsonConvert.SerializeObject(value);
        await valkeyDatabase.StringSetAsync(key, json, TimeSpan.FromSeconds(expireSeconds));
    }

    public async Task<CacheRetrievalResult<T>> GetKey<T>(string key)
    {
        string? json = await valkeyDatabase.StringGetAsync(key);

        if (string.IsNullOrEmpty(json))
            return new CacheRetrievalResult<T>(false);

        var result = JsonConvert.DeserializeObject<T>(json);
        Debug.Assert(result != null);
        var cacheRetrievalResult = new CacheRetrievalResult<T>(true, result);
        return cacheRetrievalResult;
    }

    public async Task RemoveKey(string key)
    {
        var keyExists = await valkeyDatabase.KeyExistsAsync(key);
        if (keyExists)
            await valkeyDatabase.KeyDeleteAsync(key);
    }
}