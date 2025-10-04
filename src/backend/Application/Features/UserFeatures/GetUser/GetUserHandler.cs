using Application.Common;
using Application.Repositories.Database;

namespace Application.Features.UserFeatures.GetUser;

public class GetUserHandler(
    IUserRepository userRepository,
    Serilog.ILogger logger)
{
    private const string FeatureName = "UserGet";

    public async Task<UserResult> Handle(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.Get(userId, cancellationToken);
        
        if (user == null)
            return new UserResult(ResultStatusTypes.NotFound);
        return new UserResult(ResultStatusTypes.Ok, UserResponse.MapFrom(user));
    }
}