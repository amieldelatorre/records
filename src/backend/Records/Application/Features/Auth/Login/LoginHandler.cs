using Application.Features.Password;
using Application.Repositories.DatabaseCache;

namespace Application.Features.Auth.Login;

public class LoginHandler(ICachedUserRepository userRepository)
{
    public async Task<bool> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var passwordHasher = new Pbkdf2PasswordHasher();
        var user = await userRepository.GetByEmail(request.Email, cancellationToken);
        if (user == null)
            return false;
        
        var credentialsIsValid = passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt);
        return credentialsIsValid;
    }

    public static Dictionary<string, List<string>> GetInvalidCredentialsMessage()
    {
        return new Dictionary<string, List<string>>
        {
            {"Credentials", ["Invalid credentials."]}
        };
    }
}