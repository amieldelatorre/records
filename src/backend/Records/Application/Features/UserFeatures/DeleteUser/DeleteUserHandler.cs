using Application.Common;
using Application.Repositories.DatabaseCache;

namespace Application.Features.UserFeatures.DeleteUser;

public class DeleteUserHandler(
    FeatureStatus featureStatus,
    ICachedUserRepository cachedUserRepository,
    Serilog.ILogger logger)
{
    private const string FeatureName = "UserDelete";

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

        await cachedUserRepository.Delete(user, cancellationTokenSource.Token);
        return new UserResult(ResultStatusTypes.Ok);
    }
}