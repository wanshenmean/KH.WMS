using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Algorithms.Strategy.Models
{
    /// <summary>
    /// 策略执行日志
    /// </summary>
    public class PolicyExecutionLog
    {
        public Guid PolicyId { get; set; }
        public string? PolicyName { get; set; }
        public string PolicyCode { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public long Duration { get; set; }
        public DateTime ExecutedAt { get; set; } = DateTime.Now;
    }
}
