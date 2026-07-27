//Original by h1ghwaay
//Remarked By szaman251.

using System.Security.Cryptography;
using System.Text;

namespace BlackChat;

public class EncryptionService
{
    private readonly KeyManager _keyManager;
    private readonly byte[] _publicChatKey;

    public EncryptionService(KeyManager keyManager)
    {
        _keyManager = keyManager;
        _publicChatKey = _keyManager.GetPublicChatKey();
    }

    // Szyfrowanie dla public chat – stały klucz
    public (string encrypted, string iv, string tag) EncryptPublic(string plainText)
    {
        return EncryptWithKey(plainText, _publicChatKey);
    }

    public string DecryptPublic(string encrypted, string iv, string tag)
    {
        return DecryptWithKey(encrypted, iv, tag, _publicChatKey);
    }

    // Szyfrowanie dla private chat – klucz sesji z ECDH
    public (string encrypted, string iv, string tag) EncryptPrivate(string plainText, byte[] otherPublicKey)
    {
        var shared = _keyManager.DeriveSharedSecret(otherPublicKey);
        return EncryptWithKey(plainText, shared);
    }

    public string DecryptPrivate(string encrypted, string iv, string tag, byte[] otherPublicKey)
    {
        var shared = _keyManager.DeriveSharedSecret(otherPublicKey);
        return DecryptWithKey(encrypted, iv, tag, shared);
    }

    // Szyfrowanie dla group – używa podanego klucza grupy (byte[])
    public (string encrypted, string iv, string tag) EncryptGroup(string plainText, byte[] groupKey)
    {
        return EncryptWithKey(plainText, groupKey);
    }

    public string DecryptGroup(string encrypted, string iv, string tag, byte[] groupKey)
    {
        return DecryptWithKey(encrypted, iv, tag, groupKey);
    }

    private (string encrypted, string iv, string tag) EncryptWithKey(string plainText, byte[] key)
    {
        if (string.IsNullOrEmpty(plainText)) return ("", "", "");
        var nonce = new byte[12];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(nonce);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(key);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);
        return (Convert.ToBase64String(cipherBytes),
                Convert.ToBase64String(nonce),
                Convert.ToBase64String(tag));
    }

    private string DecryptWithKey(string encrypted, string iv, string tag, byte[] key)
    {
        if (string.IsNullOrEmpty(encrypted) || string.IsNullOrEmpty(iv) || string.IsNullOrEmpty(tag))
            return "";
        try
        {
            var cipherBytes = Convert.FromBase64String(encrypted);
            var nonce = Convert.FromBase64String(iv);
            var tagBytes = Convert.FromBase64String(tag);
            var plainBytes = new byte[cipherBytes.Length];
            using var aes = new AesGcm(key);
            aes.Decrypt(nonce, cipherBytes, tagBytes, plainBytes);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (CryptographicException) { return "[Decryption failed - auth error]"; }
        catch { return "[Decryption failed]"; }
    }
}