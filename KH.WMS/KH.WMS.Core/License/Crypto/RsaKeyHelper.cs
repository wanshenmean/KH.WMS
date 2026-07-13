using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Core.License.Crypto
{
    /// <summary>
    /// RSA 密钥工具类
    /// </summary>
    public static class RsaKeyHelper
    {
        /// <summary>
        /// RSA 密钥长度（位）。决定密钥强度策略，变更需同步评估兼容性。
        /// </summary>
        private const int RsaKeySize = 2048;

        /// <summary>
        /// 生成 RSA 密钥对
        /// </summary>
        /// <returns>(公钥PEM, 私钥PEM)</returns>
        public static (string PublicKeyPem, string PrivateKeyPem) GenerateKeyPair()
        {
            using var rsa = RSA.Create(RsaKeySize);

            var publicKeyPem = ExportPublicKeyToPem(rsa);
            var privateKeyPem = ExportPrivateKeyToPem(rsa);

            return (publicKeyPem, privateKeyPem);
        }

        /// <summary>
        /// 从 PEM 格式公钥创建 RSA 实例
        /// </summary>
        public static RSA CreateFromPublicKey(string pem)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(pem);
            return rsa;
        }

        /// <summary>
        /// 从 PEM 格式私钥创建 RSA 实例
        /// </summary>
        public static RSA CreateFromPrivateKey(string pem)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(pem);
            return rsa;
        }

        /// <summary>
        /// 使用私钥对数据进行签名（SHA256 + RSA）
        /// </summary>
        public static string SignData(string data, RSA privateKey)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signatureBytes = privateKey.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signatureBytes);
        }

        /// <summary>
        /// 使用公钥验证签名
        /// </summary>
        public static bool VerifyData(string data, string signatureBase64, RSA publicKey)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signatureBytes = Convert.FromBase64String(signatureBase64);
            return publicKey.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        /// <summary>
        /// 导出公钥为 PEM 格式
        /// </summary>
        private static string ExportPublicKeyToPem(RSA rsa)
        {
            var publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();
            var base64 = Convert.ToBase64String(publicKeyBytes);
            var sb = new StringBuilder();
            sb.AppendLine("-----BEGIN PUBLIC KEY-----");
            for (var i = 0; i < base64.Length; i += 64)
            {
                var lineLength = Math.Min(64, base64.Length - i);
                sb.AppendLine(base64.Substring(i, lineLength));
            }
            sb.AppendLine("-----END PUBLIC KEY-----");
            return sb.ToString();
        }

        /// <summary>
        /// 导出私钥为 PEM 格式（PKCS#8）
        /// </summary>
        private static string ExportPrivateKeyToPem(RSA rsa)
        {
            var privateKeyBytes = rsa.ExportPkcs8PrivateKey();
            var base64 = Convert.ToBase64String(privateKeyBytes);
            var sb = new StringBuilder();
            sb.AppendLine("-----BEGIN PRIVATE KEY-----");
            for (var i = 0; i < base64.Length; i += 64)
            {
                var lineLength = Math.Min(64, base64.Length - i);
                sb.AppendLine(base64.Substring(i, lineLength));
            }
            sb.AppendLine("-----END PRIVATE KEY-----");
            return sb.ToString();
        }
    }
}
