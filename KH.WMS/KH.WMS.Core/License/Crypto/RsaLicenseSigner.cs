using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.License.Models;
using Newtonsoft.Json;

namespace KH.WMS.Core.License.Crypto
{
    /// <summary>
    /// License 签名器 - 用于生成 License 文件
    /// </summary>
    public class RsaLicenseSigner
    {
        private readonly RSA _privateKey;

        public RsaLicenseSigner(RSA privateKey)
        {
            _privateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
        }

        /// <summary>
        /// 生成 License 文件
        /// </summary>
        /// <param name="machineCode">目标机器码</param>
        /// <param name="validDays">有效天数</param>
        /// <param name="licenseType">License 类型</param>
        /// <param name="productName">产品名称</param>
        /// <returns>Base64 编码的 License 文件内容</returns>
        public string GenerateLicense(string machineCode, int validDays, string licenseType = "Standard", string productName = "WMS-V2")
        {
            var issuedAt = DateTime.Now;
            var expiresAt = issuedAt.AddDays(validDays);

            var licenseData = new LicenseData
            {
                MachineCode = machineCode,
                ProductName = productName,
                IssuedAt = issuedAt,
                ExpiresAt = expiresAt,
                ValidDays = validDays,
                LicenseType = licenseType,
                LicenseId = Guid.NewGuid()
            };

            return GenerateLicense(licenseData);
        }

        /// <summary>
        /// 从 LicenseData 生成 License 文件
        /// </summary>
        public string GenerateLicense(LicenseData licenseData)
        {
            // 序列化 LicenseData 为 JSON（用于签名和展示）
            var jsonData = JsonConvert.SerializeObject(licenseData, Formatting.None);

            // RSA 签名
            var signature = RsaKeyHelper.SignData(jsonData, _privateKey);

            // 构造 LicenseFile
            var licenseFile = new LicenseFile
            {
                Data = licenseData,
                Signature = signature
            };

            // 序列化为 JSON 后 Base64 编码
            var licenseJson = JsonConvert.SerializeObject(licenseFile, Formatting.None);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(licenseJson));
        }

        /// <summary>
        /// 生成 License 文件并保存到指定路径
        /// </summary>
        public void GenerateLicenseToFile(string machineCode, int validDays, string outputPath,
            string licenseType = "Standard", string productName = "WMS-V2")
        {
            var content = GenerateLicense(machineCode, validDays, licenseType, productName);
            File.WriteAllText(outputPath, content, Encoding.UTF8);
        }
    }
}
