namespace Application.Repositories.FeatureToggle;

public interface IFeatureToggleRepository
{
    Task<bool> IsFeatureEnabled(string featureName);
}