namespace KH.WMS.Core.Security.Encryption;

/// <summary>
/// RSA 加解密服务接口（登录密码加密专用）
/// </summary>
public interface IRsaCryptoService
{
    /// <summary>
    /// 获取 PEM 格式公钥
    /// </summary>
    string GetPublicKey();

    /// <summary>
    /// 使用私钥解密（Base64 密文 → 明文）
    /// </summary>
    string Decrypt(string cipherText);
}
