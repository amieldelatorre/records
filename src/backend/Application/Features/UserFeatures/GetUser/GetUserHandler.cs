using Application.Common;
using Application.Repositories.Database;

namespace Application.Features.UserFeatures.GetUser;

public class GetUserHandler(
    IUserRepository userRepository,
    Serilog.ILogger logger)
{
    private const string FeatureName = "UserGet";

    public async Task<UserResult> Handle(Guid userId)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(
            Application.Features.SharedConfiguration.DefaultRequestTimeout));
        var user = await userRepository.Get(userId, cancellationTokenSource.Token);
        
        if (user == null)
            return new UserResult(ResultStatusTypes.NotFound);
        return new UserResult(ResultStatusTypes.Ok, UserResponse.MapFrom(user));
    }
}