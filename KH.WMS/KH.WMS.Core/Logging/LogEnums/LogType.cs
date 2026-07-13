namespace KH.WMS.Core.Logging.LogEnums
{
    /// <summary>
    /// 日志类型枚举（仅保留 LoggerService 实际使用的类型）
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// 系统日志（默认，LogInfo/LogDebug/LogWarning）
        /// </summary>
        System = 0,

        /// <summary>
        /// 操作日志（LogOperation）
        /// </summary>
        Operation = 1,

        /// <summary>
        /// 业务日志（LogBusiness）
        /// </summary>
        Business = 2,

        /// <summary>
        /// 异常日志（LogError/LogException）
        /// </summary>
        Exception = 3,

        /// <summary>
        /// 性能日志（LogPerformance）
        /// </summary>
        Performance = 4,
    }
}
