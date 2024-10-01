using System.Diagnostics;
using Application.Common;
using Application.Repositories;
using Application.Repositories.DatabaseCache;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Persistence.Repositories.DatabaseCache;

public class ValkeyCacheRepository(
    IDatabase valkeyDatabase,
    Serilog.ILogger logger
    ) : ICacheRepository
{
    public async Task Set<T>(string key, T value, int expireSeconds)
    {
        try
        {
            var json = JsonConvert.SerializeObject(value);
            await valkeyDatabase.StringSetAsync(key, json, TimeSpan.FromSeconds(expireSeconds));
            logger.Debug("added item to cache: {key}", key);
        }
        catch (Exception ex)
        {
            logger.Error("failed to add item to cache: {key}", key);
        }
    }

    public async Task<CacheRetrievalResult<T>> Get<T>(string key)
    {
        try
        {
            string? json = await valkeyDatabase.StringGetAsync(key);

            if (string.IsNullOrEmpty(json))
                return new CacheRetrievalResult<T>(false);

            var result = JsonConvert.DeserializeObject<T>(json);
            Debug.Assert(result != null);
            var cacheRetrievalResult = new CacheRetrievalResult<T>(true, result);
            logger.Debug("retrieved item from cache: {key}", key);
            return cacheRetrievalResult;
        }
        catch (Exception ex)
        {
            logger.Error("failed to get item from cache: {key}", key);
            return new CacheRetrievalResult<T>(false);
        }

    }

    public async Task Delete(string key)
    {
        try
        {
            var keyExists = await valkeyDatabase.KeyExistsAsync(key);
            if (keyExists)
            {
                await valkeyDatabase.KeyDeleteAsync(key);
                logger.Debug("deleted item from cache: {key}", key);
            }
        }
        catch (Exception ex)
        {
           logger.Error("failed to delete item from cache: {key}", key);
        }
    }
}