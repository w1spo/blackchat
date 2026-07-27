using System.Security.Cryptography;
using System.Text;

namespace BlackChat;

public class KeyManager
{
    private readonly string _userDir;
    private readonly string _aesKeyFile;
    private readonly string _ecdsaPrivateFile;
    private readonly string _ecdsaPublicFile;

    public KeyManager(string username)
    {
        var baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BlackChat",
            username);
        _userDir = baseDir;
        Directory.CreateDirectory(_userDir);
        _aesKeyFile = Path.Combine(_userDir, "aes_key.bin");
        _ecdsaPrivateFile = Path.Combine(_userDir, "ecdsa_private.pem");
        _ecdsaPublicFile = Path.Combine(_userDir, "ecdsa_public.pem");
    }

    public byte[] GetOrCreateAesKey()
    {
        if (File.Exists(_aesKeyFile))
        {
            var encrypted = File.ReadAllBytes(_aesKeyFile);
            return ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
        }
        else
        {
            var key = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            var encrypted = ProtectedData.Protect(key, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(_aesKeyFile, encrypted);
            return key;
        }
    }

    public (byte[] privateKey, byte[] publicKey) GetOrCreateEcdsaKeys()
    {
        if (File.Exists(_ecdsaPrivateFile) && File.Exists(_ecdsaPublicFile))
        {
            var privatePem = File.ReadAllText(_ecdsaPrivateFile);
            var publicPem = File.ReadAllText(_ecdsaPublicFile);
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportFromPem(privatePem);
            var privateKey = ecdsa.ExportECPrivateKey();
            var publicKey = ecdsa.ExportSubjectPublicKeyInfo();
            return (privateKey, publicKey);
        }
        else
        {
            using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            var privateKey = ecdsa.ExportECPrivateKey();
            var publicKey = ecdsa.ExportSubjectPublicKeyInfo();
            var privatePem = PemEncoding.Write("EC PRIVATE KEY", privateKey);
            var publicPem = PemEncoding.Write("PUBLIC KEY", publicKey);
            File.WriteAllText(_ecdsaPrivateFile, privatePem);
            File.WriteAllText(_ecdsaPublicFile, publicPem);
            return (privateKey, publicKey);
        }
    }

    public byte[] GetPrivateKey()
    {
        if (!File.Exists(_ecdsaPrivateFile))
            throw new InvalidOperationException("Private key not found.");
        var privatePem = File.ReadAllText(_ecdsaPrivateFile);
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(privatePem);
        return ecdsa.ExportECPrivateKey();
    }

    public byte[] GetPublicKey()
    {
        if (!File.Exists(_ecdsaPublicFile))
            throw new InvalidOperationException("Public key not found.");
        var publicPem = File.ReadAllText(_ecdsaPublicFile);
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(publicPem);
        return ecdsa.ExportSubjectPublicKeyInfo();
    }

    public string GetPublicKeyBase64()
    {
        return Convert.ToBase64String(GetPublicKey());
    }

    public string SignData(byte[] data)
    {
        using var ecdsa = ECDsa.Create();
        var privatePem = File.ReadAllText(_ecdsaPrivateFile);
        ecdsa.ImportFromPem(privatePem);
        var signature = ecdsa.SignData(data, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(signature);
    }

    public static bool VerifySignature(byte[] data, string signatureBase64, string publicKeyBase64)
    {
        try
        {
            var signature = Convert.FromBase64String(signatureBase64);
            var publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            return ecdsa.VerifyData(data, signature, HashAlgorithmName.SHA256);
        }
        catch
        {
            return false;
        }
    }
}