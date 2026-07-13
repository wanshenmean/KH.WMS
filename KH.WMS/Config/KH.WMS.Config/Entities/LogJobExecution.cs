using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 作业执行日志
    /// </summary>
    [ConfigDb]
    [SugarTable("log_job_execution")]
    [SugarIndex("idx_job", nameof(JobId), OrderByType.Asc)]
    [SugarIndex("idx_fire_time", nameof(FireTime), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(Status), OrderByType.Asc)]
    public class LogJobExecution : BaseEntity<long>
    {

        /// <summary>
        /// 作业ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "作业ID")]
        public long JobId { get; set; }

        /// <summary>
        /// 触发器ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "触发器ID")]
        public long? TriggerId { get; set; }

        /// <summary>
        /// 执行编号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "执行编号")]
        public string? ExecutionNo { get; set; }

        /// <summary>
        /// 触发时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "触发时间")]
        public DateTime? FireTime { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "开始时间")]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "结束时间")]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 执行时长(毫秒)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "执行时长(毫秒)")]
        public int? DurationMs { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "状态")]
        public string? Status { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true, ColumnDescription = "结果消息")]
        public string? ResultMessage { get; set; }

        /// <summary>
        /// 错误堆栈
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true, ColumnDescription = "错误堆栈")]
        public string? ErrorStack { get; set; }

        /// <summary>
        /// 输入参数
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true, ColumnDescription = "输入参数")]
        public string? InputParams { get; set; }

        /// <summary>
        /// 输出结果
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true, ColumnDescription = "输出结果")]
        public string? OutputResult { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "重试次数", DefaultValue = "0")]
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// 重试时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "重试时间")]
        public DateTime? RetryTime { get; set; }

        /// <summary>
        /// 服务器IP
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "服务器IP")]
        public string? ServerIp { get; set; }

        /// <summary>
        /// 线程名称
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnDescription = "线程名称")]
        public string? ThreadName { get; set; }
    }
}
