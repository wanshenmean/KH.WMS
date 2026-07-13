using System.Security.Cryptography;
using System.Text;

namespace KH.WMS.Core.Security.Encryption;

/// <summary>
/// AES 加密服务实现
/// </summary>
public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public AesEncryptionService(string key, string? iv = null)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        // 使用 SHA256 哈希生成 256 位密钥
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));

        // 如果未提供 IV，使用密钥的前 16 字节
        _iv = string.IsNullOrEmpty(iv)
            ? _key.Take(16).ToArray()
            : Encoding.UTF8.GetBytes(iv.PadRight(16).Substring(0, 16));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be empty", nameof(plainText));

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = EncryptBytes(plainBytes);
        return Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            throw new ArgumentException("Cipher text cannot be empty", nameof(cipherText));

        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = DecryptBytes(cipherBytes);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public byte[] EncryptBytes(byte[] plainBytes)
    {
        if (plainBytes == null || plainBytes.Length == 0)
            throw new ArgumentException("Plain bytes cannot be empty", nameof(plainBytes));

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        using var msEncrypt = new MemoryStream();
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
            csEncrypt.Write(plainBytes, 0, plainBytes.Length);
        }
        return msEncrypt.ToArray();
    }

    public byte[] DecryptBytes(byte[] cipherBytes)
    {
        if (cipherBytes == null || cipherBytes.Length == 0)
            throw new ArgumentException("Cipher bytes cannot be empty", nameof(cipherBytes));

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var msDecrypt = new MemoryStream(cipherBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var msPlain = new MemoryStream();
        csDecrypt.CopyTo(msPlain);
        return msPlain.ToArray();
    }
}

/// <summary>
/// RSA 加密服务实现
/// </summary>
public class RsaEncryptionService : IEncryptionService
{
    private readonly RSA _rsa;

    public RsaEncryptionService(int keySize = 2048)
    {
        _rsa = RSA.Create(keySize);
    }

    public RsaEncryptionService(string publicKey)
    {
        _rsa = RSA.Create();
        _rsa.FromXmlString(publicKey);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be empty", nameof(plainText));

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = _rsa.Encrypt(plainBytes, RSAEncryptionPadding.OaepSHA256);
        return Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            throw new ArgumentException("Cipher text cannot be empty", nameof(cipherText));

        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = _rsa.Decrypt(cipherBytes, RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public byte[] EncryptBytes(byte[] plainBytes)
    {
        if (plainBytes == null || plainBytes.Length == 0)
            throw new ArgumentException("Plain bytes cannot be empty", nameof(plainBytes));

        return _rsa.Encrypt(plainBytes, RSAEncryptionPadding.OaepSHA256);
    }

    public byte[] DecryptBytes(byte[] cipherBytes)
    {
        if (cipherBytes == null || cipherBytes.Length == 0)
            throw new ArgumentException("Cipher bytes cannot be empty", nameof(cipherBytes));

        return _rsa.Decrypt(cipherBytes, RSAEncryptionPadding.OaepSHA256);
    }

    /// <summary>
    /// 导出公钥
    /// </summary>
    public string ExportPublicKey()
    {
        return _rsa.ToXmlString(false);
    }

    /// <summary>
    /// 导出私钥
    /// </summary>
    public string ExportPrivateKey()
    {
        return _rsa.ToXmlString(true);
    }

    public void Dispose()
    {
        _rsa?.Dispose();
    }
}
