using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.License.Models;

namespace KH.WMS.Core.License.Interfaces
{
    /// <summary>
    /// License 核心服务接口
    /// </summary>
    public interface ILicenseService
    {
        /// <summary>
        /// 获取当前服务器的机器码
        /// </summary>
        string GetMachineCode();

        /// <summary>
        /// 验证当前 License 是否有效，返回 LicenseData
        /// </summary>
        LicenseData? ValidateLicense();

        /// <summary>
        /// 导入 License（Base64 编码的字符串）
        /// </summary>
        bool ImportLicense(string licenseContent);

        /// <summary>
        /// 获取当前 License 详细信息（如果不存在返回 null）
        /// </summary>
        LicenseData? GetCurrentLicense();

        /// <summary>
        /// 获取 License 验证失败的详细信息
        /// </summary>
        string? GetValidationErrorMessage();

        /// <summary>
        /// 首次启动初始化：自动生成密钥对和默认 License（如果不存在）
        /// </summary>
        void EnsureDefaultLicense();
    }
}
