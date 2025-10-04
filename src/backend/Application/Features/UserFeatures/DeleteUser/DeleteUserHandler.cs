using Application.Common;
using Application.Repositories.Database;

namespace Application.Features.UserFeatures.DeleteUser;

public class DeleteUserHandler(
    IUserRepository userRepository,
    Serilog.ILogger logger)
{
    private const string FeatureName = "UserDelete";

    public async Task<UserResult> Handle(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.Get(userId, cancellationToken);
        if (user == null)
            return new UserResult(ResultStatusTypes.NotFound);

        await userRepository.Delete(user, cancellationToken);
        return new UserResult(ResultStatusTypes.Ok);
    }
}