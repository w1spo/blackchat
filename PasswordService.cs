using System.Security.Cryptography;
using System.Text;

namespace BlackChat;

public class PasswordService
{
    public (string hash, string salt) HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var saltBytes = new byte[32];
        rng.GetBytes(saltBytes);
        var salt = Convert.ToBase64String(saltBytes);

        using var sha256 = SHA256.Create();
        var combined = Encoding.UTF8.GetBytes(password + salt);
        var hashBytes = sha256.ComputeHash(combined);
        var hash = Convert.ToBase64String(hashBytes);

        return (hash, salt);
    }

    public bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        using var sha256 = SHA256.Create();
        var combined = Encoding.UTF8.GetBytes(password + storedSalt);
        var hashBytes = sha256.ComputeHash(combined);
        var computedHash = Convert.ToBase64String(hashBytes);

        return computedHash == storedHash;
    }
}