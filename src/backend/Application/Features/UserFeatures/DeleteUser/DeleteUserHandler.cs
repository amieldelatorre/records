using Application.Common;
using Application.Repositories.Database;

namespace Application.Features.UserFeatures.DeleteUser;

public class DeleteUserHandler(
    IUserRepository userRepository,
    Serilog.ILogger logger)
{
    private const string FeatureName = "UserDelete";

    public async Task<UserResult> Handle(Guid userId)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(
            Application.Features.SharedConfiguration.DefaultRequestTimeout));

        var user = await userRepository.Get(userId, cancellationTokenSource.Token);
        if (user == null)
            return new UserResult(ResultStatusTypes.NotFound);

        await userRepository.Delete(user, cancellationTokenSource.Token);
        return new UserResult(ResultStatusTypes.Ok);
    }
}