using System.Security.Cryptography;
using System.Text;

namespace BlackChat;

public class EncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(byte[] aesKey)
    {
        _key = aesKey;
    }

    public (string encryptedText, string iv, string tag) Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return ("", "", "");

        var nonce = new byte[12];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(nonce);

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_key);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        return (Convert.ToBase64String(cipherBytes),
                Convert.ToBase64String(nonce),
                Convert.ToBase64String(tag));
    }

    public string Decrypt(string encryptedText, string ivBase64, string tagBase64)
    {
        if (string.IsNullOrEmpty(encryptedText) || string.IsNullOrEmpty(ivBase64) || string.IsNullOrEmpty(tagBase64))
            return "";

        try
        {
            var cipherBytes = Convert.FromBase64String(encryptedText);
            var nonce = Convert.FromBase64String(ivBase64);
            var tag = Convert.FromBase64String(tagBase64);
            var plainBytes = new byte[cipherBytes.Length];

            using var aes = new AesGcm(_key);
            aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (CryptographicException)
        {
            return "[Decryption failed - authentication error]";
        }
        catch
        {
            return "[Decryption failed - unknown error]";
        }
    }
}