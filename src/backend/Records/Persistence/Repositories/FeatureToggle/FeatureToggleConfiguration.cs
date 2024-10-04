using Application.Configuration;

namespace Persistence.Repositories.FeatureToggle;

public static class FeatureToggleConfiguration
{
    public static EnvironmentVariable<bool> EnableFeatureToggles = new ("ENABLE_FEATURE_TOGGLES", false, true);
}