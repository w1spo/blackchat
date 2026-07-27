using System.Security.Cryptography;
using System.Text;

namespace BlackChat;

public class PasswordService
{
    private const int Iterations = 600000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public (string hash, string salt) HashPassword(string password)
    {
        var saltBytes = new byte[SaltSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);

        using var derive = new Rfc2898DeriveBytes(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256);
        var hashBytes = derive.GetBytes(HashSize);

        return (Convert.ToBase64String(hashBytes),
                Convert.ToBase64String(saltBytes));
    }

    public bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        var saltBytes = Convert.FromBase64String(storedSalt);
        using var derive = new Rfc2898DeriveBytes(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256);
        var hashBytes = derive.GetBytes(HashSize);
        var computedHash = Convert.ToBase64String(hashBytes);
        return computedHash == storedHash;
    }
}