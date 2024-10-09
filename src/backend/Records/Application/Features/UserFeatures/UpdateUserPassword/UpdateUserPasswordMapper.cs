using Application.Features.Password;
using Domain.Entities;

namespace Application.Features.UserFeatures.UpdateUserPassword;

public class UpdateUserPasswordMapper
{
    public static void Map(UpdateUserPasswordRequest request, User originalUser)
    {
        var hasher = new Pbkdf2PasswordHasher();
        var passwordHashResponse = hasher.Hash(request.NewPassword);
        var now = DateTime.UtcNow;

        originalUser.PasswordHash = passwordHashResponse.HashedPassword;
        originalUser.PasswordSalt = passwordHashResponse.Salt;
        originalUser.DateUpdated = now;
    }
}