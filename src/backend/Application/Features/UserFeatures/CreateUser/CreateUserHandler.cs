using Application.Common;
using Application.Repositories.Database;
using Microsoft.Extensions.Logging;

namespace Application.Features.UserFeatures.CreateUser;

public class CreateUserHandler(
    IUserRepository userRepository,
    ILogger<CreateUserHandler> logger)
{
    private const string FeatureName = "UserCreate";

    public async Task<UserResult> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateUserValidator(userRepository);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return new UserResult(ResultStatusTypes.ValidationError, validationResult.ToDictionary());

        var user = CreateUserMapper.Map(request);
        await userRepository.Create(user, cancellationToken);
        var result = new UserResult(ResultStatusTypes.Created, UserResponse.MapFrom(user));
        return result;
    }
}