using KH.WMS.Core.Models.Entities;
using SqlSugar;

namespace KH.WMS.Entities.System
{
    /// <summary>
    /// 操作日志
    /// </summary>
    [SugarTable("sys_operate_log")]
    [SugarIndex("idx_module", nameof(Module), OrderByType.Asc)]
    [SugarIndex("idx_type", nameof(BusinessType), OrderByType.Asc)]
    [SugarIndex("idx_operator", nameof(OperatorId), OrderByType.Asc)]
    [SugarIndex("idx_time", nameof(OperTime), OrderByType.Asc)]
    public class SysOperateLog : BaseEntity<long>
    {

        /// <summary>
        /// 模块
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "模块")]
        public string? Module { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "业务类型")]
        public int? BusinessType { get; set; }

        /// <summary>
        /// 方法名称
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "方法名称")]
        public string? Method { get; set; }

        /// <summary>
        /// 请求方式
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = true, ColumnDescription = "请求方式")]
        public string? RequestMethod { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "操作人ID")]
        public long? OperatorId { get; set; }

        /// <summary>
        /// 操作人名称
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "操作人名称")]
        public string? OperatorName { get; set; }

        /// <summary>
        /// 请求URL
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "请求URL")]
        public string? OperUrl { get; set; }

        /// <summary>
        /// 操作IP
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "操作IP")]
        public string? OperIp { get; set; }

        /// <summary>
        /// 操作地点
        /// </summary>
        [SugarColumn(Length = 255, IsNullable = true, ColumnDescription = "操作地点")]
        public string? OperLocation { get; set; }

        /// <summary>
        /// 请求参数
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true, ColumnDescription = "请求参数")]
        public string? OperParam { get; set; }

        /// <summary>
        /// 操作结果
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true, ColumnDescription = "操作结果")]
        public string? OperResult { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "状态", DefaultValue = "0")]
        public int Status { get; set; } = 0;

        /// <summary>
        /// 错误消息
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true, ColumnDescription = "错误消息")]
        public string? ErrorMsg { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "操作时间", DefaultValue = "CURRENT_TIMESTAMP")]
        public DateTime OperTime { get; set; } = DateTime.Now;
    }
}
