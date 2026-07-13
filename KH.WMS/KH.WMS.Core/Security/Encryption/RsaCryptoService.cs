using System.Security.Cryptography;
using System.Text;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Core.Security.Encryption;

/// <summary>
/// RSA 加解密服务实现（登录密码加密专用，单例保证密钥对不变）
/// </summary>
[RegisteredService(ServiceType = typeof(IRsaCryptoService), Lifetime = ServiceLifetime.Singleton)]
public class RsaCryptoService : IRsaCryptoService
{
    private readonly string _publicKeyPem;
    private readonly RSA _rsa;

    public RsaCryptoService()
    {
        _rsa = RSA.Create(2048);

        // 导出 PKCS#1 格式公钥（jsencrypt 兼容）
        var publicKeyBytes = _rsa.ExportRSAPublicKey();
        var base64 = Convert.ToBase64String(publicKeyBytes);
        var sb = new StringBuilder();
        sb.Append("-----BEGIN RSA PUBLIC KEY-----\n");
        for (var i = 0; i < base64.Length; i += 64)
        {
            var lineLength = Math.Min(64, base64.Length - i);
            sb.Append(base64.Substring(i, lineLength));
            sb.Append('\n');
        }
        sb.Append("-----END RSA PUBLIC KEY-----");
        _publicKeyPem = sb.ToString();
    }

    public string GetPublicKey()
    {
        return _publicKeyPem;
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            throw new ArgumentException("Cipher text cannot be empty", nameof(cipherText));

        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = _rsa.Decrypt(cipherBytes, RSAEncryptionPadding.Pkcs1);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
