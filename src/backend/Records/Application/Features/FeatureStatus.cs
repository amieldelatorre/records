using Application.Repositories.DatabaseCache;
using Application.Repositories.FeatureToggle;

namespace Application.Features;

public class FeatureStatus(
    IFeatureToggleRepository featureToggleRepository,
    ICacheRepository cacheRepository,
    Serilog.ILogger logger)
{
    private static string FeatureToggleCacheKey(string featureName) => $"featureToggle::name:{featureName}";
    private static string FeatureTogglePrefix(string featureName) => $"records_{featureName}";
    private const int DefaultCacheExpirationSeconds = 60;

    public async Task<bool> IsFeatureEnabled(string featureName)
    {
        var featureToggleName = FeatureTogglePrefix(featureName);
        var featureToggleCacheKey = FeatureToggleCacheKey(featureName);
        bool featureIsEnabled;

        try
        {
            var cacheValue = await cacheRepository.Get<string>(featureToggleCacheKey);
            if (cacheValue.IsInCache)
            {
                logger.Debug($"feature '{featureName}' is not in cache");
                _ = bool.TryParse(cacheValue.Value, out featureIsEnabled);
            }

            featureIsEnabled = await featureToggleRepository.IsFeatureEnabled(featureToggleName);
            await cacheRepository.Set(featureToggleCacheKey, featureIsEnabled.ToString(), DefaultCacheExpirationSeconds);
        }
        catch (Exception ex)
        {
            logger.Error("error checking if feature '{featureName}' is enabled. Defaulting to 'false'. {error}", featureName, ex.Message);
            featureIsEnabled = false;
        }
        return featureIsEnabled;
    }

    public static Dictionary<string, List<string>> GetFeatureDisabledMessage()
    {
        var result = new Dictionary<string, List<string>>();
        result["API"] = ["Feature is disabled."];
        return result;
    }
}