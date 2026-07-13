using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Constants;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 全局配置
    /// 按分组管理的系统级业务配置，支持多种值类型
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_global_config")]
    [SugarIndex("uk_group_key_scope", nameof(ConfigGroup), OrderByType.Asc, nameof(ConfigKey), OrderByType.Asc, nameof(ScopeLevel), OrderByType.Asc, nameof(ScopeId), OrderByType.Asc)]
    [SugarIndex("idx_group", nameof(ConfigGroup), OrderByType.Asc)]
    [SugarIndex("idx_scope", nameof(ScopeLevel), OrderByType.Asc, nameof(ScopeId), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(Status), OrderByType.Asc)]
    public class CfgGlobalConfig : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 配置分组（INBOUND-入库 / OUTBOUND-出库 / INVENTORY-库存 / LOCATION-库位分配 / TASK-任务 / EQUIPMENT-设备 / CODE-编码 / NOTIFICATION-通知 / SYSTEM-系统）
        /// </summary>
        
        [SugarColumn(Length = 30, IsNullable = false, ColumnDescription = "配置分组")]
        public string ConfigGroup { get; set; } = string.Empty;

        /// <summary>
        /// 配置键（如 QC_ENABLED、QC_TIMING、AUTO_CREATE_TASK）
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "配置键")]
        public string ConfigKey { get; set; } = string.Empty;

        /// <summary>
        /// 配置名称（界面显示用，如"是否启用质检"）
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "配置名称")]
        public string ConfigName { get; set; } = string.Empty;

        /// <summary>
        /// 配置值
        /// </summary>
        
        [SugarColumn(Length = 500, IsNullable = false, ColumnDescription = "配置值")]
        public string ConfigValue { get; set; } = string.Empty;

        /// <summary>
        /// 默认值（用于重置）
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "默认值")]
        public string DefaultValue { get; set; }

        /// <summary>
        /// 值类型（BOOLEAN / NUMBER / STRING / ENUM / JSON）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "值类型", DefaultValue = "STRING")]
        public string ValueType { get; set; } = "STRING";

        /// <summary>
        /// 可选值（ENUM类型时用，JSON数组格式，如 ["BEFORE_INBOUND","AFTER_INBOUND"]）
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true, ColumnDescription = "可选值")]
        public string Options { get; set; }

        /// <summary>
        /// 描述说明
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "描述说明")]
        public string Description { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "排序号", DefaultValue = "0")]
        public int SortNo { get; set; }

        /// <summary>
        /// 是否启用（1启用 0禁用）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否启用", DefaultValue = "1")]
        public byte Status { get; set; } = BoolFlag.YES;

        /// <summary>
        /// 作用域级别（GLOBAL-全局 / WAREHOUSE-仓库 / ZONE-库区 / DOC_TYPE-单据类型）
        /// 配置解析时按 DOC_TYPE → ZONE → WAREHOUSE → GLOBAL 逐级查找最高优先级值
        /// </summary>
        
        [SugarColumn(Length = 30, IsNullable = false, ColumnDescription = "作用域级别", DefaultValue = "GLOBAL")]
        public string ScopeLevel { get; set; } = ConfigScopeLevels.GLOBAL;

        /// <summary>
        /// 作用域ID（仓库ID/库区ID/单据类型ID），GLOBAL 级别时为 null
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "作用域ID")]
        public long? ScopeId { get; set; }

        /// <summary>
        /// 优先级，数值越大优先级越高。同一 ScopeLevel + ScopeId 下按 Priority 取最高值
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "优先级", DefaultValue = "0")]
        public int Priority { get; set; }
    }
}
