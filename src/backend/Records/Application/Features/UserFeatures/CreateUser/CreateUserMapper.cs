using Domain.Entities;

namespace Application.Features.UserFeatures.CreateUser;

public static class CreateUserMapper
{
    public static User Map(CreateUserRequest createUserRequest)
    {
        var now = DateTime.UtcNow;
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = createUserRequest.FirstName,
            LastName = createUserRequest.LastName,
            Email = createUserRequest.Email,
            Password = createUserRequest.Password,
            DateCreated = now,
            DateUpdated = now,
        };
    }

    public static UserResponse Map(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            DateCreated = user.DateCreated,
            DateUpdated = user.DateUpdated,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
        };
    }
}