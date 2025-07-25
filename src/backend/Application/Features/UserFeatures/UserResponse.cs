using Application.Common;
using Domain.Entities;

namespace Application.Features.UserFeatures;

public class UserResponse : BaseEntityResponse
{
    public required string Username { get; set; }
    public required string Email { get; set; }

    public static UserResponse MapFrom(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            DateCreated = user.DateCreated,
            DateUpdated = user.DateUpdated,
        };
    }
}