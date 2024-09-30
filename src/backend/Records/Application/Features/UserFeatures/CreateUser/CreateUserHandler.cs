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
    Serilog.ILogger logger
    )
{
    private const string FeatureName = "CreateUser";

    public async Task<Result<CreateUserResponse>> Handle(CreateUserRequest request)
    {
        if (!(await featureStatus.IsFeatureEnabled(FeatureName)))
           return new Result<CreateUserResponse>()
           {
               Errors = FeatureStatus.GetFeatureDisabledMessage(),
               ResponseResultStatus = ResponseResultStatusTypes.FeatureDisabled,
               Item = null
           };

        var validator = new CreateUserValidator();
        var validationResult = await validator.ValidateAsync(request);
        // TODO: Return validation result
        var user = CreateUserMapper.Map(request);
        // TODO: Check if email is unique before proceeding
        await cachedUserRepository.Create(user);
        var result = new Result<CreateUserResponse>()
        {
            ResponseResultStatus = ResponseResultStatusTypes.Ok,
            Item = CreateUserMapper.Map(user)
        };
        return result;
    }
}