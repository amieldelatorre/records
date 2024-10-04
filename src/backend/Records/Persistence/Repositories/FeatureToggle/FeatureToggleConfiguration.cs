using Application.Configuration.EnvironmentVariables;

namespace Persistence.Repositories.FeatureToggle;

public static class FeatureToggleConfiguration
{
    public static BoolEnvironmentVariable EnableFeatureToggles = new ("ENABLE_FEATURE_TOGGLES", false, true);
}