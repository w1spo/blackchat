using System.Security.Cryptography;
using System.Text;

namespace BlackChat;

public class EncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _salt;

    public EncryptionService()
    {
        _salt = Encoding.UTF8.GetBytes(
            Environment.MachineName +
            Environment.UserName +
            "BlackChatUltraSecureSalt2024!@#$%"
        );

        _key = GenerateSecureKey();
    }

    private byte[] GenerateSecureKey()
    {
        var hardwareId = GetHardwareIdentifier();
        var salt2 = "X7kL9mP2qR4vW8nY3tU6" + DateTime.Now.Year;
        var combined = $"{hardwareId}|{salt2}|BlackChat2024Secure";

        using var deriveBytes = new Rfc2898DeriveBytes(
            Encoding.UTF8.GetBytes(combined),
            _salt,
            100000,
            HashAlgorithmName.SHA512
        );

        return deriveBytes.GetBytes(32);
    }

    private string GetHardwareIdentifier()
    {
        try
        {
            var identifiers = new List<string>();

            try
            {
                using var searcher = new System.Management.ManagementObjectSearcher(
                    "SELECT ProcessorId FROM Win32_Processor"
                );
                foreach (var obj in searcher.Get())
                {
                    var id = obj["ProcessorId"]?.ToString();
                    if (!string.IsNullOrEmpty(id))
                        identifiers.Add(id);
                }
            }
            catch { }

            try
            {
                using var searcher = new System.Management.ManagementObjectSearcher(
                    "SELECT SerialNumber FROM Win32_DiskDrive WHERE Index=0"
                );
                foreach (var obj in searcher.Get())
                {
                    var serial = obj["SerialNumber"]?.ToString();
                    if (!string.IsNullOrEmpty(serial))
                        identifiers.Add(serial);
                }
            }
            catch { }

            try
            {
                foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                        ni.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                    {
                        var mac = ni.GetPhysicalAddress()?.ToString();
                        if (!string.IsNullOrEmpty(mac))
                        {
                            identifiers.Add(mac);
                            break;
                        }
                    }
                }
            }
            catch { }

            if (identifiers.Count == 0)
            {
                var guidPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "BlackChat",
                    "machine.guid"
                );

                if (File.Exists(guidPath))
                {
                    return File.ReadAllText(guidPath);
                }
                else
                {
                    var guid = Guid.NewGuid().ToString();
                    Directory.CreateDirectory(Path.GetDirectoryName(guidPath));
                    File.WriteAllText(guidPath, guid);
                    return guid;
                }
            }

            return string.Join("|", identifiers);
        }
        catch
        {
            return $"{Environment.MachineName}|{Environment.UserName}|BlackChat";
        }
    }

    public (string encryptedText, byte[] iv) Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return ("", Array.Empty<byte>());

        var compressedData = CompressData(plainText);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(compressedData, 0, compressedData.Length);
            cs.FlushFinalBlock();
        }

        var encryptedData = ms.ToArray();

        using var hmac = new HMACSHA256(_key);
        var hmacHash = hmac.ComputeHash(encryptedData);

        var combined = new byte[encryptedData.Length + hmacHash.Length];
        Buffer.BlockCopy(encryptedData, 0, combined, 0, encryptedData.Length);
        Buffer.BlockCopy(hmacHash, 0, combined, encryptedData.Length, hmacHash.Length);

        return (Convert.ToBase64String(combined), aes.IV);
    }

    public string Decrypt(string encryptedText, byte[] iv)
    {
        if (string.IsNullOrEmpty(encryptedText) || iv == null || iv.Length == 0)
            return "";

        try
        {
            var combined = Convert.FromBase64String(encryptedText);

            using var hmac = new HMACSHA256(_key);
            var hmacSize = hmac.HashSize / 8;

            if (combined.Length <= hmacSize)
                return "[Decryption failed]";

            var encryptedData = new byte[combined.Length - hmacSize];
            var receivedHmac = new byte[hmacSize];

            Buffer.BlockCopy(combined, 0, encryptedData, 0, encryptedData.Length);
            Buffer.BlockCopy(combined, encryptedData.Length, receivedHmac, 0, hmacSize);

            var computedHmac = hmac.ComputeHash(encryptedData);
            if (!computedHmac.SequenceEqual(receivedHmac))
                return "[Decryption failed - Data corrupted]";

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(encryptedData);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);

            var decryptedData = new byte[encryptedData.Length];
            var bytesRead = cs.Read(decryptedData, 0, decryptedData.Length);

            return DecompressData(decryptedData.Take(bytesRead).ToArray());
        }
        catch (CryptographicException)
        {
            return "[Decryption failed - Invalid key or data]";
        }
        catch (Exception)
        {
            return "[Decryption failed - Unknown error]";
        }
    }

    private byte[] CompressData(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);

        using var ms = new MemoryStream();
        using (var gzip = new System.IO.Compression.GZipStream(
            ms,
            System.IO.Compression.CompressionLevel.Optimal))
        {
            gzip.Write(bytes, 0, bytes.Length);
        }

        return ms.ToArray();
    }

    private string DecompressData(byte[] compressedData)
    {
        try
        {
            using var ms = new MemoryStream(compressedData);
            using var gzip = new System.IO.Compression.GZipStream(
                ms,
                System.IO.Compression.CompressionMode.Decompress);
            using var result = new MemoryStream();

            gzip.CopyTo(result);
            return Encoding.UTF8.GetString(result.ToArray());
        }
        catch
        {
            try
            {
                return Encoding.UTF8.GetString(compressedData);
            }
            catch
            {
                return "[Decryption failed]";
            }
        }
    }
}