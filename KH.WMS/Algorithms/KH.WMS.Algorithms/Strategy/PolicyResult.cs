using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.Models;

namespace KH.WMS.Algorithms.Strategy
{
    /// <summary>
    /// 策略结果实现
    /// </summary>
    public class PolicyResult : IPolicyResult
    {
        public bool IsSuccess { get; private set; }
        public bool IsHandled { get; private set; }
        public object? Output { get; private set; }
        public string? ErrorMessage { get; private set; }
        public List<PolicyExecutionLog> ExecutionLogs { get; } = new();
        public long Duration { get; private set; }

        public static PolicyResult Success(object? output = null)
        {
            return new PolicyResult
            {
                IsSuccess = true,
                IsHandled = true,
                Output = output
            };
        }

        public static PolicyResult Failure(string errorMessage)
        {
            return new PolicyResult
            {
                IsSuccess = false,
                IsHandled = false,
                ErrorMessage = errorMessage
            };
        }

        public static PolicyResult Skipped(string reason = "策略不适用")
        {
            return new PolicyResult
            {
                IsSuccess = true,
                IsHandled = false,
                ErrorMessage = reason
            };
        }

        public void AddExecutionLog(PolicyExecutionLog log)
        {
            ExecutionLogs.Add(log);
        }

        public void SetDuration(long duration)
        {
            Duration = duration;
        }

        /// <summary>
        /// 将当前结果标记为成功（原地更新，保留已有的执行日志）
        /// </summary>
        internal void MarkAsSuccess(object? output)
        {
            IsSuccess = true;
            IsHandled = true;
            Output = output;
            ErrorMessage = null;
        }

        /// <summary>
        /// 标记流水线部分步骤失败（#11）：保留 IsSuccess=true 与 Output（部分成功结果仍可用），
        /// 仅在 ErrorMessage 追加告警，供调用方感知链中存在失败步骤。
        /// </summary>
        internal void MarkPartialFailure(string message)
        {
            ErrorMessage = string.IsNullOrEmpty(ErrorMessage) ? message : $"{ErrorMessage}; {message}";
        }
    }
}
