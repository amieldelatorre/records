using Application.Repositories.FeatureToggle;
using Unleash;

namespace Persistence.Repositories.FeatureToggle;

public class UnleashFeatureToggleRepository(IUnleash unleashClient, Serilog.ILogger logger) : IFeatureToggleRepository
{
    public Task<bool> IsFeatureEnabled(string featureName)
    {
        try
        {
            return Task.FromResult(unleashClient.IsEnabled(featureName));
        }
        catch (Exception ex)
        {
            logger.Error("could not get feature toggle from unleash: {error}", ex.Message);
            return Task.FromResult(false);
        }
    }
}