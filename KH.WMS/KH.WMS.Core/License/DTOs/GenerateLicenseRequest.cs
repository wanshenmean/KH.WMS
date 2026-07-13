using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Core.License.DTOs
{
    /// <summary>
    /// 生成 License 请求
    /// </summary>
    public class GenerateLicenseRequest
    {
        /// <summary>
        /// 目标机器码
        /// </summary>
        public string MachineCode { get; set; } = string.Empty;

        /// <summary>
        /// 有效天数
        /// </summary>
        public int ValidDays { get; set; }

        /// <summary>
        /// License 类型（Standard/Professional/Enterprise）
        /// </summary>
        public string LicenseType { get; set; } = "Standard";
    }
}
