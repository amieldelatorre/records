using Domain.Entities;

namespace Application.Features.UserFeatures.UpdateUser;

public class UpdateUserMapper
{
    public static void Map(UpdateUserRequest updateUserRequest, User originalUser)
    {
        var now = DateTime.UtcNow;
        originalUser.FirstName = updateUserRequest.FirstName.Trim();
        originalUser.LastName = updateUserRequest.LastName.Trim();
        originalUser.Email = updateUserRequest.Email.Trim();
        originalUser.DateUpdated = now;
    }
}