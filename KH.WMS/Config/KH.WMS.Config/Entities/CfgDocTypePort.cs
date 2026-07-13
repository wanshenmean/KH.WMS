using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 单据类型站台映射配置
    /// 配置不同单据类型对应的入库口/出库口，支持指定口子或指定区域
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_doc_type_port")]
    [SugarIndex("idx_doctype", nameof(DocTypeId), OrderByType.Asc)]
    [SugarIndex("idx_port", nameof(PortId), OrderByType.Asc)]
    [SugarIndex("idx_zone", nameof(ZoneId), OrderByType.Asc)]
    [SugarIndex("idx_doctype_direction", nameof(DocTypeId), OrderByType.Asc, nameof(Direction), OrderByType.Asc)]
    public class CfgDocTypePort : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 单据类型ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "单据类型ID")]
        public long DocTypeId { get; set; }

        /// <summary>
        /// 方向（INBOUND-入库 / OUTBOUND-出库）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "方向")]
        public string Direction { get; set; } = string.Empty;

        /// <summary>
        /// 站台ID（指定具体口子时使用，与ZoneId二选一）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "站台ID")]
        public long? PortId { get; set; }

        /// <summary>
        /// 库区ID（指定区域时使用，该库区下对应方向的站台都可用，与PortId二选一）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "库区ID")]
        public long? ZoneId { get; set; }

        /// <summary>
        /// 优先级（数字越小优先级越高）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "优先级", DefaultValue = "100")]
        public int Priority { get; set; } = 100;

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否启用", DefaultValue = "1")]
        public byte IsActive { get; set; } = 1;

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
