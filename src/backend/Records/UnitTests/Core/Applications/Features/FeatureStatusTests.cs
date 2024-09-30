using Application.Features;
using Application.Repositories.DatabaseCache;
using Application.Repositories.FeatureToggle;
using Persistence.Repositories.DatabaseCache;
using Persistence.Repositories.FeatureToggle;

namespace UnitTests;

public class FeatureStatusTests
{
    [Test]
    // The IsFeatureEnabled function should always return true when using the NullFeatureToggleRepository
    public void IsFeatureEnabledNullRepositoryTest()
    {
        IFeatureToggleRepository featureToggleRepository = new NullFeatureToggleRepository();
        ICacheRepository cacheRepository = new NullCacheRepository();
        var logger = Common.Logger.GetLogger();

        var featureStatus = new FeatureStatus(featureToggleRepository, cacheRepository, logger);
        var featureName = "anyFeature";
        var result = featureStatus.IsFeatureEnabled(featureName).GetAwaiter().GetResult();

        Assert.That(result, Is.EqualTo(true));
    }
}