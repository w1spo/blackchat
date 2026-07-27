using System.Security.Cryptography;
using System.Text;

namespace BlackChat;

public class KeyManager
{
    private readonly string _userDir;
    private readonly string _aesKeyFile;
    private readonly string _ecdhPrivateFile;
    private readonly string _ecdhPublicFile;

    public KeyManager(string username)
    {
        var baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BlackChat",
            username);
        _userDir = baseDir;
        Directory.CreateDirectory(_userDir);
        _aesKeyFile = Path.Combine(_userDir, "aes_key.bin");
        _ecdhPrivateFile = Path.Combine(_userDir, "ecdh_private.pem");
        _ecdhPublicFile = Path.Combine(_userDir, "ecdh_public.pem");
    }

    // ---------- STAŁY KLUCZ DLA CZATU PUBLICZNEGO ----------
    public byte[] GetPublicChatKey()
    {
        var password = "BlackChatPublicKey2026!@#";
        var salt = Encoding.UTF8.GetBytes("PublicChatSalt_Static_2024");
        using var derive = new Rfc2898DeriveBytes(
            Encoding.UTF8.GetBytes(password),
            salt,
            10000,
            HashAlgorithmName.SHA256);
        return derive.GetBytes(32);
    }

    // ---------- KLUCZ AES UŻYTKOWNIKA (DPAPI) ----------
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

    // ---------- PARA ECDH ----------
    public (byte[] privateKey, byte[] publicKey) GetOrCreateEcdhKeys()
    {
        if (File.Exists(_ecdhPrivateFile) && File.Exists(_ecdhPublicFile))
        {
            var privatePem = File.ReadAllText(_ecdhPrivateFile);
            var publicPem = File.ReadAllText(_ecdhPublicFile);
            using var ecdh = ECDiffieHellman.Create();
            ecdh.ImportFromPem(privatePem);
            var privateKey = ecdh.ExportECPrivateKey();
            var publicKey = ecdh.ExportSubjectPublicKeyInfo();
            return (privateKey, publicKey);
        }
        else
        {
            using var ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
            var privateKey = ecdh.ExportECPrivateKey();
            var publicKey = ecdh.ExportSubjectPublicKeyInfo();
            var privatePem = PemEncoding.Write("EC PRIVATE KEY", privateKey);
            var publicPem = PemEncoding.Write("PUBLIC KEY", publicKey);
            File.WriteAllText(_ecdhPrivateFile, privatePem);
            File.WriteAllText(_ecdhPublicFile, publicPem);
            return (privateKey, publicKey);
        }
    }

    public byte[] GetEcdhPrivateKey()
    {
        if (!File.Exists(_ecdhPrivateFile))
            throw new InvalidOperationException("ECDH private key not found.");
        var privatePem = File.ReadAllText(_ecdhPrivateFile);
        using var ecdh = ECDiffieHellman.Create();
        ecdh.ImportFromPem(privatePem);
        return ecdh.ExportECPrivateKey();
    }

    public byte[] GetEcdhPublicKey()
    {
        if (!File.Exists(_ecdhPublicFile))
            throw new InvalidOperationException("ECDH public key not found.");
        var publicPem = File.ReadAllText(_ecdhPublicFile);
        using var ecdh = ECDiffieHellman.Create();
        ecdh.ImportFromPem(publicPem);
        return ecdh.ExportSubjectPublicKeyInfo();
    }

    public string GetEcdhPublicKeyBase64()
    {
        return Convert.ToBase64String(GetEcdhPublicKey());
    }

    // ---------- UZGADNIANIE SEKRETU ECDH ----------
    public byte[] DeriveSharedSecret(byte[] otherPublicKey)
    {
        using var ecdh = ECDiffieHellman.Create();
        var privatePem = File.ReadAllText(_ecdhPrivateFile);
        ecdh.ImportFromPem(privatePem);
        using var other = ECDiffieHellman.Create();
        other.ImportSubjectPublicKeyInfo(otherPublicKey, out _);
        var secret = ecdh.DeriveKeyMaterial(other.PublicKey);
        using var sha = SHA256.Create();
        return sha.ComputeHash(secret);
    }

    // ---------- SZYFROWANIE DANYCH DLA ODBIORCY (EPHEMERAL ECDH) ----------
    public string EncryptDataWithPublicKey(byte[] data, byte[] recipientPublicKey)
    {
        using var ephemeral = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var ephemeralPublic = ephemeral.PublicKey.ExportSubjectPublicKeyInfo();

        using var other = ECDiffieHellman.Create();
        other.ImportSubjectPublicKeyInfo(recipientPublicKey, out _);
        var secret = ephemeral.DeriveKeyMaterial(other.PublicKey);
        var key = SHA256.HashData(secret);

        using var aes = new AesGcm(key);
        var nonce = new byte[12];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(nonce);

        var cipher = new byte[data.Length];
        var tag = new byte[16];
        aes.Encrypt(nonce, data, cipher, tag);

        var combined = new byte[ephemeralPublic.Length + nonce.Length + cipher.Length + tag.Length];
        int offset = 0;
        Buffer.BlockCopy(ephemeralPublic, 0, combined, offset, ephemeralPublic.Length);
        offset += ephemeralPublic.Length;
        Buffer.BlockCopy(nonce, 0, combined, offset, nonce.Length);
        offset += nonce.Length;
        Buffer.BlockCopy(cipher, 0, combined, offset, cipher.Length);
        offset += cipher.Length;
        Buffer.BlockCopy(tag, 0, combined, offset, tag.Length);

        return Convert.ToBase64String(combined);
    }

    // ---------- ODSZYFROWANIE DANYCH OTRZYMANYCH OD NADAWCY ----------
    public byte[] DecryptDataWithPrivateKey(string encryptedBase64)
    {
        var combined = Convert.FromBase64String(encryptedBase64);

        // Długość klucza publicznego P-256 w formacie SubjectPublicKeyInfo wynosi 91 bajtów
        const int keyLen = 91;
        if (combined.Length < keyLen + 12 + 16)
            throw new ArgumentException("Invalid encrypted data length.");

        var ephemeralPublic = new byte[keyLen];
        Buffer.BlockCopy(combined, 0, ephemeralPublic, 0, keyLen);
        int offset = keyLen;

        var nonce = new byte[12];
        Buffer.BlockCopy(combined, offset, nonce, 0, 12);
        offset += 12;

        int cipherLen = combined.Length - offset - 16;
        var cipher = new byte[cipherLen];
        Buffer.BlockCopy(combined, offset, cipher, 0, cipherLen);
        offset += cipherLen;

        var tag = new byte[16];
        Buffer.BlockCopy(combined, offset, tag, 0, 16);

        using var ephemeral = ECDiffieHellman.Create();
        ephemeral.ImportSubjectPublicKeyInfo(ephemeralPublic, out _);

        var privatePem = File.ReadAllText(_ecdhPrivateFile);
        using var ecdhPrivate = ECDiffieHellman.Create();
        ecdhPrivate.ImportFromPem(privatePem);

        var secret = ecdhPrivate.DeriveKeyMaterial(ephemeral.PublicKey);
        var key = SHA256.HashData(secret);

        using var aes = new AesGcm(key);
        var plain = new byte[cipher.Length];
        aes.Decrypt(nonce, cipher, tag, plain);
        return plain;
    }

    // ---------- PODPISYWANIE ECDSA (OSOBNA PARA LUB TE SAME KLUCZE) ----------
    public string SignData(byte[] data)
    {
        using var ecdsa = ECDsa.Create();
        var privatePem = File.ReadAllText(_ecdhPrivateFile);
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