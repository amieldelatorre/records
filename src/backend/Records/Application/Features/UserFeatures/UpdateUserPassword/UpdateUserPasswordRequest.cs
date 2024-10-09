namespace Application.Features.UserFeatures.UpdateUserPassword;

public class UpdateUserPasswordRequest
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}