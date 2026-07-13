using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Engines.DataMap
{
    /// <summary>
    /// 接口调用日志
    /// </summary>
    [SugarTable("int_interface_log")]
    [SugarIndex("idx_trace", nameof(TraceId), OrderByType.Asc)]
    [SugarIndex("idx_system", nameof(SystemCode), OrderByType.Asc)]
    [SugarIndex("idx_interface", nameof(InterfaceCode), OrderByType.Asc)]
    [SugarIndex("idx_business", nameof(BusinessType), OrderByType.Asc, nameof(BusinessNo), OrderByType.Asc)]
    [SugarIndex("idx_time", nameof(RequestTime), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(Status), OrderByType.Asc)]
    public class IntInterfaceLog : BaseEntity<long>
    {
        /// <summary>
        /// 追踪ID
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "追踪ID")]
        public string? TraceId { get; set; }

        /// <summary>
        /// 系统编码
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "系统编码")]
        public string SystemCode { get; set; } = string.Empty;

        /// <summary>
        /// 接口编码
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "接口编码")]
        public string InterfaceCode { get; set; } = string.Empty;

        /// <summary>
        /// 接口方向
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "接口方向")]
        public string? Direction { get; set; }

        /// <summary>
        /// 请求头
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "请求头")]
        public string? RequestHeaders { get; set; }

        /// <summary>
        /// 请求数据
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "请求数据")]
        public string? RequestData { get; set; }

        /// <summary>
        /// 请求时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "请求时间")]
        public DateTime? RequestTime { get; set; }

        /// <summary>
        /// 响应头
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "响应头")]
        public string? ResponseHeaders { get; set; }

        /// <summary>
        /// 响应数据
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "响应数据")]
        public string? ResponseData { get; set; }

        /// <summary>
        /// 响应时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "响应时间")]
        public DateTime? ResponseTime { get; set; }

        /// <summary>
        /// 状态（SUCCESS-成功 / FAIL-失败 / TIMEOUT-超时）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "状态")]
        public string? Status { get; set; }

        /// <summary>
        /// 错误码
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "错误码")]
        public string? ErrorCode { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "错误信息")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 执行耗时（毫秒）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "执行耗时(ms)")]
        public int? ExecuteMilliseconds { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "重试次数", DefaultValue = "0")]
        public int RetryCount { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "业务类型")]
        public string? BusinessType { get; set; }

        /// <summary>
        /// 业务单号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "业务单号")]
        public string? BusinessNo { get; set; }
    }
}
