using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Engines.DataMap
{
    /// <summary>
    /// 接口配置
    /// 定义WMS与外部系统（ERP/MES/WCS等）的接口信息
    /// </summary>
    [SugarTable("int_interface_config")]
    [SugarIndex("uk_interface_code", nameof(InterfaceCode), OrderByType.Asc)]
    [SugarIndex("idx_system", nameof(SystemCode), OrderByType.Asc)]
    [SugarIndex("idx_direction", nameof(Direction), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(Status), OrderByType.Asc)]
    public class IntInterfaceConfig : BaseEntity<long>
    {
        /// <summary>
        /// 接口编码（如 ERP_PURCHASE_ORDER_SYNC、MES_PRODUCTION_FEEDBACK）
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "接口编码")]
        public string InterfaceCode { get; set; } = string.Empty;

        /// <summary>
        /// 接口名称
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "接口名称")]
        public string InterfaceName { get; set; } = string.Empty;

        /// <summary>
        /// 所属系统（ERP/MES/TMS/WCS/RCS）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "所属系统")]
        public string SystemCode { get; set; } = string.Empty;

        /// <summary>
        /// 接口方向（OUTBOUND-出站 / INBOUND-入站 / BIDIRECTIONAL-双向）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "接口方向", DefaultValue = DataMapConstants.InterfaceDirection.OUTBOUND)]
        public string Direction { get; set; } = DataMapConstants.InterfaceDirection.OUTBOUND;

        /// <summary>
        /// 接口类型（SYNC-同步 / ASYNC-异步）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "接口类型", DefaultValue = DataMapConstants.InterfaceType.SYNC)]
        public string InterfaceType { get; set; } = DataMapConstants.InterfaceType.SYNC;

        /// <summary>
        /// 触发方式（MANUAL-手动 / EVENT-事件 / SCHEDULE-定时）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "触发方式")]
        public string? TriggerType { get; set; }

        /// <summary>
        /// HTTP方法（GET/POST/PUT/DELETE）
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = true, ColumnDescription = "HTTP方法")]
        public string? HttpMethod { get; set; }

        /// <summary>
        /// 请求路径
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "请求路径")]
        public string? RequestPath { get; set; }

        /// <summary>
        /// 请求模板（JSON）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "请求模板(JSON)")]
        public string? RequestTemplate { get; set; }

        /// <summary>
        /// 响应模板（JSON）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "响应模板(JSON)")]
        public string? ResponseTemplate { get; set; }

        /// <summary>
        /// 数据映射配置ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "数据映射配置ID")]
        public long? MappingConfigId { get; set; }

        /// <summary>
        /// 转换脚本（Lua/C#）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "转换脚本(Lua/C#)")]
        public string? TransformScript { get; set; }

        /// <summary>
        /// 业务处理器类型（如 ReceiptFromExternal、ShipmentFromExternal）
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnDescription = "业务处理器类型")]
        public string? ProcessorType { get; set; }

        /// <summary>
        /// 速率限制（次/分钟）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "速率限制(次/分钟)")]
        public int? RateLimit { get; set; }

        /// <summary>
        /// 节流时间（秒）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "节流时间(秒)")]
        public int? ThrottleSeconds { get; set; }

        /// <summary>
        /// 超时时间（毫秒）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "超时时间(ms)")]
        public int? Timeout { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "重试次数")]
        public int? RetryCount { get; set; }

        /// <summary>
        /// 状态（ACTIVE-正常 / INACTIVE-停用）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "状态", DefaultValue = DataMapConstants.InterfaceStatus.ACTIVE)]
        public string Status { get; set; } = DataMapConstants.InterfaceStatus.ACTIVE;

        /// <summary>
        /// 版本号
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "版本号")]
        public string? Version { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "描述")]
        public string? Description { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
