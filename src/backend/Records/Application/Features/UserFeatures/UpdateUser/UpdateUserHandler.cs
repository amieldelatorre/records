using Application.Common;
using Application.Repositories.DatabaseCache;

namespace Application.Features.UserFeatures.UpdateUser;

public class UpdateUserHandler (
    FeatureStatus featureStatus,
    ICachedUserRepository cachedUserRepository,
    Serilog.ILogger logger)
{
    private const string FeatureName = "UserUpdate";

    public async Task<UserResult> Handle(Guid userId, UpdateUserRequest request)
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

        var validator = new UpdateUserValidator();
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return new UserResult(ResultStatusTypes.ValidationError, validationResult.ToDictionary());

        UpdateUserMapper.Map(request, user);
        await cachedUserRepository.Update(user, cancellationTokenSource.Token);
        return new UserResult(ResultStatusTypes.Ok, UserResponse.MapFrom(user));
    }
}