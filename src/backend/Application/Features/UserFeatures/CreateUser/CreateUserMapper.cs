using Application.Features.PasswordFeatures;
using Domain.Entities;

namespace Application.Features.UserFeatures.CreateUser;

public static class CreateUserMapper
{
    public static User Map(CreateUserRequest createUserRequest)
    {
        var hasher = new Pbkdf2PasswordHasher();
        var passwordHashResponse = hasher.Hash(createUserRequest.Password);

        var now = DateTime.UtcNow;
        return new User
        {
            Id = Guid.NewGuid(),
            Username = createUserRequest.Username.Trim(),
            Email = createUserRequest.Email.Trim(),
            PasswordHash = passwordHashResponse.HashedPassword,
            PasswordSalt = passwordHashResponse.Salt,
            DateCreated = now,
            DateUpdated = now,
        };
    }
}