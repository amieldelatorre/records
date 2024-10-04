using Application.Features.Password;
using Application.Repositories.DatabaseCache;

namespace Application.Features.AuthFeatures.Login;

public class LoginHandler(ICachedUserRepository userRepository)
{
    public async Task<LoginResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var passwordHasher = new Pbkdf2PasswordHasher();
        var user = await userRepository.GetByEmail(request.Email, cancellationToken);
        if (user == null)
            return new LoginResponse
            {
                Success = false,
                User = null
            };

        var credentialsIsValid = passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt);
        return new LoginResponse
        {
            Success = credentialsIsValid,
            User = user
        };
    }

    public static Dictionary<string, List<string>> GetInvalidCredentialsMessage()
    {
        return new Dictionary<string, List<string>>
        {
            {"Credentials", ["Invalid credentials."]}
        };
    }
}