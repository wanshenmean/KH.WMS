using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.System
{
    /// <summary>
    /// 字典项
    /// </summary>
    [SugarTable("sys_dict_item")]
    [SugarIndex("idx_dict_type", nameof(DictTypeId), OrderByType.Asc)]
    [SugarIndex("idx_value", nameof(DictTypeId), OrderByType.Asc, nameof(ItemValue), OrderByType.Asc)]
    public class SysDictItem : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 字典类型ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "字典类型ID")]
        public long DictTypeId { get; set; }

        /// <summary>
        /// 字典值（如 EMPTY、INBOUND、PUTAWAY）
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "字典值")]
        public string ItemValue { get; set; } = string.Empty;

        /// <summary>
        /// 显示标签（如 空库位、入库口、上架任务）
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "显示标签")]
        public string ItemLabel { get; set; } = string.Empty;

        /// <summary>
        /// 标签颜色（前端状态标签颜色，如 success、warning、danger）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "标签颜色")]
        public string? TagColor { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "排序", DefaultValue = "0")]
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否启用", DefaultValue = "1")]
        public byte IsActive { get; set; } = 1;

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
