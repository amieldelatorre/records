using System.Runtime.InteropServices.JavaScript;
using Application.Common;
using Application.Repositories;
using Application.Repositories.Database;
using Application.Repositories.DatabaseCache;
using Domain.Entities;

namespace Application.Features.UserFeatures.CreateUser;

public class CreateUserHandler(
    FeatureStatus featureStatus,
    ICachedUserRepository cachedUserRepository,
    Serilog.ILogger logger)
{
    private const string FeatureName = "UserCreate";

    public async Task<UserResult> Handle(CreateUserRequest request)
    {
        if (!(await featureStatus.IsFeatureEnabled(FeatureName)))
        {
            logger.Debug("feature '{featureName}' is not enabled. Skipping feature and returning early", FeatureName);
            return new UserResult(ResultStatusTypes.FeatureDisabled, FeatureStatus.GetFeatureDisabledMessage());
        }

        var validator = new CreateUserValidator();
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return new UserResult(ResultStatusTypes.ValidationError, validationResult.ToDictionary());

        var user = CreateUserMapper.Map(request);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
        if (await cachedUserRepository.EmailExists(user.Email, cancellationTokenSource.Token))
        {
            var errorResult = new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>> {
            {
              "Email", ["'Email' is already in use."]
            } });

            return errorResult;
        }

        await cachedUserRepository.Create(user);
        var result = new UserResult(ResultStatusTypes.Created, UserResponse.MapFrom(user));
        return result;
    }
}