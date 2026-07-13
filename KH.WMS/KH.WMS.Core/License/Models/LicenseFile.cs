using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KH.WMS.Core.License.Models
{
    /// <summary>
    /// License 文件结构（最终序列化为 JSON 后 Base64 编码存储为 .lic 文件）
    /// </summary>
    public class LicenseFile
    {
        /// <summary>
        /// License 数据
        /// </summary>
        [JsonProperty("data")]
        public LicenseData Data { get; set; } = new();

        /// <summary>
        /// RSA 签名（Base64 编码）
        /// </summary>
        [JsonProperty("signature")]
        public string Signature { get; set; } = string.Empty;
    }
}
