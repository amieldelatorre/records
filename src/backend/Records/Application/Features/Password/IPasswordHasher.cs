namespace Application.Features.Password;

public struct PasswordHashResponse
{
    public required string HashedPassword { get; set; }
    public required string Salt { get; set; }
}

public interface IPasswordHasher
{
    PasswordHashResponse Hash(string password);
    bool Verify(string password, string hashedPassword, string salt);
}