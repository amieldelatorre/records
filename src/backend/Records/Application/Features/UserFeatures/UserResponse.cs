using Application.Common;
using Domain.Entities;

namespace Application.Features.UserFeatures;

public class UserResponse : BaseEntityResponse
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }

    public static UserResponse MapFrom(User user)
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