using Application.Common;
using Application.Repositories.Database;

namespace Application.Features.UserFeatures.CreateUser;

public class CreateUserHandler(
    IUserRepository userRepository,
    Serilog.ILogger logger)
{
    private const string FeatureName = "UserCreate";

    public async Task<UserResult> Handle(CreateUserRequest request)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(
            TimeSpan.FromSeconds(Application.Features.SharedConfiguration.DefaultRequestTimeout));
        
        var validator = new CreateUserValidator(userRepository);
        var validationResult = await validator.ValidateAsync(request, cancellationTokenSource.Token);
        if (!validationResult.IsValid)
            return new UserResult(ResultStatusTypes.ValidationError, validationResult.ToDictionary());


        // Recreate cancellation token
        cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(
            Application.Features.SharedConfiguration.DefaultRequestTimeout));

        var user = CreateUserMapper.Map(request);
        await userRepository.Create(user, cancellationTokenSource.Token);
        var result = new UserResult(ResultStatusTypes.Created, UserResponse.MapFrom(user));
        return result;
    }
}