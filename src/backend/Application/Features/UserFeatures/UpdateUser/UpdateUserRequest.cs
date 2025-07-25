namespace Application.Features.UserFeatures.UpdateUser;

public class UpdateUserRequest
{
    public required string Username { get; set; }
    public required string Email { get; set; }
}