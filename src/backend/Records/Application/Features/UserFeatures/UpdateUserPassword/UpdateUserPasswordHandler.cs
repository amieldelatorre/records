using Application.Common;
using Application.Features.Password;
using Application.Repositories.DatabaseCache;

namespace Application.Features.UserFeatures.UpdateUserPassword;

public class UpdateUserPasswordHandler(
    FeatureStatus featureStatus,
    ICachedUserRepository cachedUserRepository,
    Serilog.ILogger logger)
{
    private const string FeatureName = "UserPasswordUpdate";

    public async Task<UserResult> Handle(Guid userId, UpdateUserPasswordRequest request)
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

        // Check current password given matches password in database
        var passwordHasher = new Pbkdf2PasswordHasher();
        var isCurrentPasswordCorrect = passwordHasher.Verify(request.CurrentPassword, user.PasswordHash, user.PasswordSalt);
        if (!isCurrentPasswordCorrect)
            return new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                { "CurrentPassword", ["'Current Password' is incorrect."] }
            });

        var validator = new UpdateUserPasswordValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationTokenSource.Token);
        if (!validationResult.IsValid)
            return new UserResult(ResultStatusTypes.ValidationError, validationResult.ToDictionary());

        UpdateUserPasswordMapper.Map(request, user);
        await cachedUserRepository.Update(user, cancellationTokenSource.Token);
        return new UserResult(ResultStatusTypes.Ok, UserResponse.MapFrom(user));
    }
}