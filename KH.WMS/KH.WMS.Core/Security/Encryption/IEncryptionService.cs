namespace KH.WMS.Core.Security.Encryption;

/// <summary>
/// 加密服务接口
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// 加密字符串
    /// </summary>
    string Encrypt(string plainText);

    /// <summary>
    /// 解密字符串
    /// </summary>
    string Decrypt(string cipherText);

    /// <summary>
    /// 加密字节数组
    /// </summary>
    byte[] EncryptBytes(byte[] plainBytes);

    /// <summary>
    /// 解密字节数组
    /// </summary>
    byte[] DecryptBytes(byte[] cipherBytes);
}
