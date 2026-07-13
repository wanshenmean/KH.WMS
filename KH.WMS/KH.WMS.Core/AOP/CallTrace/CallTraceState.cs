using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Core.AOP.CallTrace
{
    /// <summary>
    /// 调用链追踪状态
    /// </summary>
    public class CallTraceState
    {
        /// <summary>
        /// 追踪ID（8位短ID）
        /// </summary>
        public string TraceId { get; set; } = string.Empty;

        /// <summary>
        /// 调用层级深度
        /// </summary>
        public int Depth { get; set; }

        public bool IsRecord { get; set; }
    }
}
