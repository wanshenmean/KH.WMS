using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Algorithms.Strategy.Models;

namespace KH.WMS.Algorithms.Strategy.Interfaces
{
    /// <summary>
    /// 策略结果接口
    /// </summary>
    public interface IPolicyResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// 是否已处理（策略已生效）
        /// </summary>
        bool IsHandled { get; }

        /// <summary>
        /// 输出数据
        /// </summary>
        object? Output { get; }

        /// <summary>
        /// 错误消息
        /// </summary>
        string? ErrorMessage { get; }

        /// <summary>
        /// 执行日志列表
        /// </summary>
        List<PolicyExecutionLog> ExecutionLogs { get; }

        /// <summary>
        /// 执行耗时（毫秒）
        /// </summary>
        long Duration { get; }

        /// <summary>
        /// 添加执行日志
        /// </summary>
        void AddExecutionLog(PolicyExecutionLog log);

        /// <summary>
        /// 设置执行耗时
        /// </summary>
        void SetDuration(long duration);
    }
}
