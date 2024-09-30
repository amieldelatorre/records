using Domain.Entities;

namespace Application.Features.UserFeatures.CreateUser;

public static class CreateUserMapper
{
    public static User Map(CreateUserRequest createUserRequest)
    {
        var now = DateTime.UtcNow;
        return new User()
        {
            FirstName = createUserRequest.FirstName,
            LastName = createUserRequest.LastName,
            Email = createUserRequest.Email,
            Password = createUserRequest.Password,
            DateCreated = now,
            DateUpdated = now,
        };
    }

    public static CreateUserResponse Map(User user)
    {
        return new CreateUserResponse()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            DateCreated = user.DateCreated,
            DateUpdated = user.DateUpdated,
        };
    }
}