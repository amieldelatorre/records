using Application.Repositories.FeatureToggle;

namespace Persistence.Repositories.FeatureToggle;

public class NullFeatureToggleRepository : IFeatureToggleRepository
{
    public async Task<bool> IsFeatureEnabled(string featureName)
    {
        return await Task.FromResult(true);
    }
}