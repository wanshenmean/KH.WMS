namespace KH.WMS.Core.Security.Hashing;

/// <summary>
/// 哈希服务接口
/// </summary>
public interface IHashService
{
    /// <summary>
    /// 哈希字符串
    /// </summary>
    string Hash(string plainText);

    /// <summary>
    /// 验证哈希
    /// </summary>
    bool Verify(string plainText, string hash);

    /// <summary>
    /// 哈希字节数组
    /// </summary>
    string HashBytes(byte[] plainBytes);

    /// <summary>
    /// 使用 HMAC 哈希
    /// </summary>
    string Hmac(string plainText, string key);
}
