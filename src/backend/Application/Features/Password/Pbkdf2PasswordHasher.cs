using System.Security.Cryptography;

namespace Application.Features.Password;

public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBits = 128; // Gives 16 bytes
    private const int HashSizeBits = 256; // Gives 32 bytes

    // https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html#pbkdf2
    private const int Iterations = 210_000;
    private readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA512;

    public PasswordHashResponse Hash(string password)
    {
        var salt = Convert.ToHexString(RandomNumberGenerator.GetBytes(SaltSizeBits / 8));
        return Hash(password, salt);
    }

    public PasswordHashResponse Hash(string password, string salt)
    {
        var saltBytes = Convert.FromHexString(salt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            _hashAlgorithmName,
            HashSizeBits / 8);

        var passwordHashResponse = new PasswordHashResponse
        {
            HashedPassword = Convert.ToHexString(hash),
            Salt = Convert.ToHexString(saltBytes),
        };
        return passwordHashResponse;
    }

    public bool Verify(string password, string hashedPassword, string salt)
    {
        var generatedHash = Hash(password, salt);

        return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(generatedHash.HashedPassword),
            Convert.FromHexString(hashedPassword));
    }
}