using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 作业定义
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_job_definition")]
    [SugarIndex("uk_job_code", nameof(JobCode), OrderByType.Asc, true)]
    public class CfgJobDefinition : BaseEntity<long>, IEnableDisableEntity
    {

        /// <summary>
        /// 作业编码
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "作业编码")]
        public string JobCode { get; set; } = string.Empty;

        /// <summary>
        /// 作业名称
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "作业名称")]
        public string JobName { get; set; } = string.Empty;

        /// <summary>
        /// 作业分组
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "作业分组")]
        public string? JobGroup { get; set; }

        /// <summary>
        /// 作业类型
        /// </summary>
        
        [SugarColumn(Length = 30, IsNullable = false, ColumnDescription = "作业类型")]
        public string JobType { get; set; } = string.Empty;

        /// <summary>
        /// 作业类
        /// </summary>
        
        [SugarColumn(Length = 200, IsNullable = false, ColumnDescription = "作业类")]
        public string JobClass { get; set; } = string.Empty;

        /// <summary>
        /// 作业方法
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "作业方法")]
        public string JobMethod { get; set; } = string.Empty;

        /// <summary>
        /// 作业参数
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true, ColumnDescription = "作业参数")]
        public string? JobParams { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "描述")]
        public string? Description { get; set; }

        /// <summary>
        /// 超时秒数
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "超时秒数")]
        public int? TimeoutSeconds { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "重试次数")]
        public int? RetryCount { get; set; }

        /// <summary>
        /// 重试间隔
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "重试间隔")]
        public int? RetryInterval { get; set; }

        /// <summary>
        /// 并发策略
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = true, ColumnDescription = "并发策略")]
        public string? ConcurrentPolicy { get; set; }

        /// <summary>
        /// 失败告警
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "失败告警", DefaultValue = "0")]
        public byte AlertOnFail { get; set; } = 0;

        /// <summary>
        /// 告警接收人
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "告警接收人")]
        public string? AlertReceivers { get; set; }

        /// <summary>
        /// 是否内置
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否内置", DefaultValue = "0")]
        public byte IsBuiltin { get; set; } = 0;

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否启用", DefaultValue = "1")]
        public byte IsActive { get; set; } = 1;
    }
}
