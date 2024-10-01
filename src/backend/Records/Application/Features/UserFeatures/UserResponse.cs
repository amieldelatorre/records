using Application.Common;

namespace Application.Features.UserFeatures;

public class UserResponse : BaseEntityResponse
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
}