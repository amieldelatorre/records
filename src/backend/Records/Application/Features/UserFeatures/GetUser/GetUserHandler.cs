using Application.Common;
using Application.Repositories.DatabaseCache;

namespace Application.Features.UserFeatures.GetUser;

public class GetUserHandler(
    FeatureStatus featureStatus,
    ICachedUserRepository cachedUserRepository,
    Serilog.ILogger logger)
{
    private const string FeatureName = "UserGet";

    public async Task<UserResult> Handle(Guid userId)
    {
        if (!await featureStatus.IsFeatureEnabled(FeatureName))
        {
            logger.Debug("feature '{featureName}' is not enabled. Skipping feature and returning early", FeatureName);
            return new UserResult(ResultStatusTypes.FeatureDisabled, FeatureStatus.GetFeatureDisabledMessage());
        }

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
        var user = await cachedUserRepository.Get(userId, cancellationTokenSource.Token);
        if (user == null)
            return new UserResult(ResultStatusTypes.NotFound);

        return new UserResult(ResultStatusTypes.Ok, UserResponse.MapFrom(user));
    }
}