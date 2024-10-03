using Application.Common;
using Persistence.Repositories.DatabaseCache;

namespace UnitTests.Infrastructure.Persistence.Repositories.DatabaseCache;

public class NullCacheRepositoryTests
{
    [Test]
    // This should always return a result with a null value with the IsInCache attribute as false
    public void GetString()
    {
        var nullCacheRepository = new NullCacheRepository();
        var cacheKey = "anyKey";
        var result = nullCacheRepository.Get<string>(cacheKey).GetAwaiter().GetResult();
        var expected = new CacheRetrievalResult<string>(false);

        Assert.That(result, Is.EqualTo(expected));
        Assert.That(result.Value, Is.EqualTo(null));
    }

    [Test]
    public void GetBool()
    {
        var nullCacheRepository = new NullCacheRepository();
        var cacheKey = "anyKey";
        var result = nullCacheRepository.Get<bool>(cacheKey).GetAwaiter().GetResult();
        var expected = new CacheRetrievalResult<bool>(false);

        Assert.That(result, Is.EqualTo(expected));
        Assert.That(result.Value, Is.EqualTo(false));
    }
}