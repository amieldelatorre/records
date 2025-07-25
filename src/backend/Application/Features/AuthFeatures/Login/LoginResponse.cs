using Domain.Entities;

namespace Application.Features.AuthFeatures.Login;

public class LoginResponse
{
    public bool Success { get; set; }
    public User? User { get; set; }
}