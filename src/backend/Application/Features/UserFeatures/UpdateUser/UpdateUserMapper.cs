using Domain.Entities;

namespace Application.Features.UserFeatures.UpdateUser;

public class UpdateUserMapper
{
    public static void Map(UpdateUserRequest updateUserRequest, User originalUser)
    {
        var now = DateTime.UtcNow;
        originalUser.Username = updateUserRequest.Username.Trim();
        originalUser.Email = updateUserRequest.Email.Trim();
        originalUser.DateUpdated = now;
    }
}