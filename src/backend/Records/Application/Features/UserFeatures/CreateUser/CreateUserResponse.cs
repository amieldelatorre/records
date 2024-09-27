using Application.Common;
using Domain.Common;

namespace Application.Features.UserFeatures.CreateUser;

public class CreateUserResponse : BaseEntityResponse
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
}