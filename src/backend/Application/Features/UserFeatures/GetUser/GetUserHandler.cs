using Application.Common;
using Application.Repositories.Database;
using Microsoft.Extensions.Logging;

namespace Application.Features.UserFeatures.GetUser;

public class GetUserHandler(
    IUserRepository userRepository,
    ILogger<GetUserHandler> logger)
{
    private const string FeatureName = "UserGet";

    public async Task<UserResult> Handle(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.Get(userId, cancellationToken);
        
        if (user == null)
            return new UserResult(ResultStatusTypes.NotFound);
        logger.LogInformation("user '{userId}' successfully retrieved", user.Id);
        return new UserResult(ResultStatusTypes.Ok, UserResponse.MapFrom(user));
    }
}