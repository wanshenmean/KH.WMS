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
    /// License 验证器 - 用于 WMS 系统验证 License
    /// </summary>
    public class RsaLicenseVerifier
    {
        private readonly RSA _publicKey;

        public RsaLicenseVerifier(RSA publicKey)
        {
            _publicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
        }

        /// <summary>
        /// 验证并解析 License 文件内容
        /// </summary>
        /// <param name="licenseContent">Base64 编码的 License 内容</param>
        /// <param name="currentMachineCode">当前服务器的机器码</param>
        /// <param name="errorMessage">验证失败时的错误信息</param>
        /// <returns>验证成功返回 LicenseData，失败返回 null</returns>
        public LicenseData? Verify(string licenseContent, string currentMachineCode, out string? errorMessage)
        {
            errorMessage = null;

            try
            {
                // 1. Base64 解码
                var licenseJson = Encoding.UTF8.GetString(Convert.FromBase64String(licenseContent.Trim()));

                // 2. 反序列化 LicenseFile
                var licenseFile = JsonConvert.DeserializeObject<LicenseFile>(licenseJson);
                if (licenseFile?.Data == null)
                {
                    errorMessage = "License 文件格式无效";
                    return null;
                }

                var data = licenseFile.Data;

                // 3. 验证签名
                var jsonData = JsonConvert.SerializeObject(data, Formatting.None);
                var signatureValid = RsaKeyHelper.VerifyData(jsonData, licenseFile.Signature, _publicKey);
                if (!signatureValid)
                {
                    errorMessage = "License 签名验证失败，文件可能已被篡改";
                    return null;
                }

                // 4. 验证机器码
                if (!string.Equals(data.MachineCode, currentMachineCode, StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = $"License 机器码不匹配，当前机器码: {currentMachineCode}";
                    return null;
                }

                // 5. 验证有效期
                if (DateTime.Now > data.ExpiresAt)
                {
                    errorMessage = $"License 已过期，过期时间: {data.ExpiresAt:yyyy-MM-dd HH:mm:ss}";
                    return null;
                }

                return data;
            }
            catch (FormatException)
            {
                errorMessage = "License 文件内容不是有效的 Base64 编码";
                return null;
            }
            catch (JsonException)
            {
                errorMessage = "License 文件内容不是有效的 JSON 格式";
                return null;
            }
            catch (Exception ex)
            {
                errorMessage = $"License 验证异常: {ex.Message}";
                return null;
            }
        }
    }
}
