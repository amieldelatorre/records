using Application.Common;
using Application.Repositories.Database;
using Microsoft.Extensions.Logging;

namespace Application.Features.UserFeatures.UpdateUser;

public class UpdateUserHandler (
    IUserRepository userRepository,
    ILogger<UpdateUserHandler> logger)
{
    private const string FeatureName = "UserUpdate";

    public async Task<UserResult> Handle(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await userRepository.Get(userId, cancellationToken);
        if (user == null)
            return new UserResult(ResultStatusTypes.NotFound);

        var validator = new UpdateUserValidator(userRepository);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return new UserResult(ResultStatusTypes.ValidationError, validationResult.ToDictionary());

        UpdateUserMapper.Map(request, user);
        await userRepository.Update(user, cancellationToken);
        return new UserResult(ResultStatusTypes.Ok, UserResponse.MapFrom(user));
    }
}