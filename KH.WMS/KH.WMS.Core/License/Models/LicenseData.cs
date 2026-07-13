using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KH.WMS.Core.License.Models
{
    /// <summary>
    /// License 数据模型（序列化后用于签名）
    /// </summary>
    public class LicenseData
    {
        /// <summary>
        /// 绑定的机器码
        /// </summary>
        [JsonProperty("machineCode")]
        public string MachineCode { get; set; } = string.Empty;

        /// <summary>
        /// 产品名称
        /// </summary>
        [JsonProperty("productName")]
        public string ProductName { get; set; } = "WMS-V2";

        /// <summary>
        /// 签发时间
        /// </summary>
        [JsonProperty("issuedAt")]
        public DateTime IssuedAt { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        [JsonProperty("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// 有效天数
        /// </summary>
        [JsonProperty("validDays")]
        public int ValidDays { get; set; }

        /// <summary>
        /// License 类型（Standard/Professional/Enterprise）
        /// </summary>
        [JsonProperty("licenseType")]
        public string LicenseType { get; set; } = "Standard";

        /// <summary>
        /// License 唯一标识
        /// </summary>
        [JsonProperty("licenseId")]
        public Guid LicenseId { get; set; }
    }
}
