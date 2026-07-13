using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Core.License.DTOs
{
    /// <summary>
    /// 导入 License 请求
    /// </summary>
    public class ImportLicenseRequest
    {
        /// <summary>
        /// License 文件内容（Base64 编码的字符串，或直接粘贴 .lic 文件的文本内容）
        /// </summary>
        public string LicenseContent { get; set; } = string.Empty;
    }
}
