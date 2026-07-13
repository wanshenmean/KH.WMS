using System.Security.Cryptography;
using System.Text;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;

namespace KH.WMS.Core.Security.Hashing;

/// <summary>
/// 密码哈希器 - 使用 PBKDF2
/// </summary>
[RegisteredService(ServiceType = typeof(IHashService))]
public class PasswordHasher : IHashService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    public string Hash(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be empty", nameof(plainText));

        // 生成随机盐值
        var salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // 使用 PBKDF2 生成哈希
        using var pbkdf2 = new Rfc2898DeriveBytes(plainText, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);

        // 组合盐值和哈希值
        var hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public bool Verify(string plainText, string hash)
    {
        if (string.IsNullOrEmpty(plainText))
            return false;

        if (string.IsNullOrEmpty(hash))
            return false;

        try
        {
            var hashBytes = Convert.FromBase64String(hash);

            if (hashBytes.Length != SaltSize + HashSize)
                return false;

            // 提取盐值
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // 计算哈希
            using var pbkdf2 = new Rfc2898DeriveBytes(plainText, salt, Iterations, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(HashSize);

            // 比较哈希值
            for (int i = 0; i < HashSize; i++)
            {
                if (hashBytes[SaltSize + i] != computedHash[i])
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string HashBytes(byte[] plainBytes)
    {
        if (plainBytes == null || plainBytes.Length == 0)
            throw new ArgumentException("Plain bytes cannot be empty", nameof(plainBytes));

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(plainBytes);
        return Convert.ToBase64String(hashBytes);
    }

    public string Hmac(string plainText, string key)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be empty", nameof(plainText));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(plainText));
        return Convert.ToBase64String(hashBytes);
    }
}

/// <summary>
/// SHA256 哈希器
/// </summary>
public class Sha256Hasher : IHashService
{
    public string Hash(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be empty", nameof(plainText));

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainText));
        return Convert.ToBase64String(hashBytes);
    }

    public bool Verify(string plainText, string hash)
    {
        return Hash(plainText) == hash;
    }

    public string HashBytes(byte[] plainBytes)
    {
        if (plainBytes == null || plainBytes.Length == 0)
            throw new ArgumentException("Plain bytes cannot be empty", nameof(plainBytes));

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(plainBytes);
        return Convert.ToBase64String(hashBytes);
    }

    public string Hmac(string plainText, string key)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be empty", nameof(plainText));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(plainText));
        return Convert.ToBase64String(hashBytes);
    }
}

/// <summary>
/// MD5 哈希器（不推荐用于安全场景）
/// </summary>
public class Md5Hasher : IHashService
{
    public string Hash(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be empty", nameof(plainText));

        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(plainText));
        return Convert.ToHexString(hashBytes).ToLower();
    }

    public bool Verify(string plainText, string hash)
    {
        return Hash(plainText) == hash.ToLower();
    }

    public string HashBytes(byte[] plainBytes)
    {
        if (plainBytes == null || plainBytes.Length == 0)
            throw new ArgumentException("Plain bytes cannot be empty", nameof(plainBytes));

        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(plainBytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }

    public string Hmac(string plainText, string key)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be empty", nameof(plainText));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        using var hmac = new HMACMD5(Encoding.UTF8.GetBytes(key));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(plainText));
        return Convert.ToHexString(hashBytes).ToLower();
    }
}
