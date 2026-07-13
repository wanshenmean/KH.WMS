using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Core.License.DTOs
{
    /// <summary>
    /// License 信息响应 DTO
    /// </summary>
    public class LicenseInfoDto
    {
        public string MachineCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int ValidDays { get; set; }
        public string LicenseType { get; set; } = string.Empty;
        public bool IsExpired { get; set; }
        public int RemainingDays { get; set; }
        public Guid LicenseId { get; set; }
        public bool IsValid { get; set; }
    }
}
