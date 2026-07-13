using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.License.DTOs;
using KH.WMS.Core.License.Results;
using SqlSugar;

namespace KH.WMS.Core.License.Interfaces
{
    /// <summary>
    /// License 应用服务接口
    /// </summary>
    public interface ILicenseAppService
    {
        /// <summary>
        /// 获取当前服务器的机器码
        /// </summary>
        Result<string> GetMachineCode();

        /// <summary>
        /// 获取当前 License 信息
        /// </summary>
        Result<LicenseInfoDto> GetLicenseInfo();

        /// <summary>
        /// 生成 License 文件内容（Base64 编码）
        /// </summary>
        Result<string> GenerateLicenseFile(GenerateLicenseRequest request);

        /// <summary>
        /// 导入 License
        /// </summary>
        Result ImportLicense(ImportLicenseRequest request);
    }
}
