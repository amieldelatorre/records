using Application.Common;
using Application.Features.PasswordFeatures;
using Application.Repositories.Database;
using Microsoft.Extensions.Logging;

namespace Application.Features.UserFeatures.UpdateUserPassword;

public class UpdateUserPasswordHandler(
    IUserRepository userRepository,
    ILogger<UpdateUserPasswordHandler> logger)
{
    private const string FeatureName = "UserPasswordUpdate";

    public async Task<UserResult> Handle(Guid userId, UpdateUserPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await userRepository.Get(userId, cancellationToken);
        if (user == null)
            return new UserResult(ResultStatusTypes.NotFound);

        // Check current password given matches password in database
        var passwordHasher = new Pbkdf2PasswordHasher();
        var isCurrentPasswordCorrect = passwordHasher.Verify(request.CurrentPassword, user.PasswordHash, user.PasswordSalt);
        if (!isCurrentPasswordCorrect)
            return new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                { "CurrentPassword", ["'Current Password' is incorrect."] }
            });

        var validator = new UpdateUserPasswordValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return new UserResult(ResultStatusTypes.ValidationError, validationResult.ToDictionary());

        UpdateUserPasswordMapper.Map(request, user);
        await userRepository.Update(user, cancellationToken);
        logger.LogInformation("user '{userId}' password successfully updated", user.Id);
        return new UserResult(ResultStatusTypes.Ok, UserResponse.MapFrom(user));
    }
}