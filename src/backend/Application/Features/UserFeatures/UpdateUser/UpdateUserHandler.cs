using Application.Common;
using Application.Repositories.Database;

namespace Application.Features.UserFeatures.UpdateUser;

public class UpdateUserHandler (
    IUserRepository userRepository,
    Serilog.ILogger logger)
{
    private const string FeatureName = "UserUpdate";

    public async Task<UserResult> Handle(Guid userId, UpdateUserRequest request)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(
            Application.Features.SharedConfiguration.DefaultRequestTimeout));

        var user = await userRepository.Get(userId, cancellationTokenSource.Token);
        if (user == null)
            return new UserResult(ResultStatusTypes.NotFound);

        // Recreate cancellation token
        cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(
            Application.Features.SharedConfiguration.DefaultRequestTimeout));
        var validator = new UpdateUserValidator(userRepository);
        var validationResult = await validator.ValidateAsync(request, cancellationTokenSource.Token);
        if (!validationResult.IsValid)
            return new UserResult(ResultStatusTypes.ValidationError, validationResult.ToDictionary());

        UpdateUserMapper.Map(request, user);
        // Recreate cancellation token
        cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(
            Application.Features.SharedConfiguration.DefaultRequestTimeout));
        
        await userRepository.Update(user, cancellationTokenSource.Token);
        return new UserResult(ResultStatusTypes.Ok, UserResponse.MapFrom(user));
    }
}