using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Entities.System.Enums;

namespace KH.WMS.Entities.System
{
    /// <summary>
    /// 系统菜单（含按钮权限）
    /// 目录 ParentId = 0，菜单 ParentId = 对应目录的 Id
    /// 菜单内按钮权限通过 Buttons JSON 字段存储
    /// </summary>
    [SugarTable("sys_permission")]
    [SugarIndex("uk_permission_code", nameof(PermissionCode), OrderByType.Asc, true)]
    [SugarIndex("idx_parent", nameof(ParentId), OrderByType.Asc)]
    [SugarIndex("idx_menu_type", nameof(MenuType), OrderByType.Asc)]
    public class SysPermission : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 权限编码（唯一标识，如 system、user、role）
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "权限编码")]
        public string PermissionCode { get; set; } = string.Empty;

        /// <summary>
        /// 权限名称（显示名称，如 系统管理、用户管理）
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "权限名称")]
        public string PermissionName { get; set; } = string.Empty;

        /// <summary>
        /// 父级ID（目录=0，菜单=对应目录的Id）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "父级ID", DefaultValue = "0")]
        public long ParentId { get; set; } = 0;

        /// <summary>
        /// 菜单类型（0=目录, 1=菜单）
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "菜单类型 0目录 1菜单", DefaultValue = "0")]
        public byte MenuType { get; set; } = (byte)MenuTypeEnum.Directory;

        /// <summary>
        /// 路由地址（如 /system/user，仅菜单有值）
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "路由地址")]
        public string? Path { get; set; }

        /// <summary>
        /// 组件路径（如 system/user/index，仅菜单有值）
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "组件路径")]
        public string? Component { get; set; }

        /// <summary>
        /// 图标（如 system、user）
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnDescription = "图标")]
        public string? Icon { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "排序号", DefaultValue = "0")]
        public int SortNo { get; set; } = 0;

        /// <summary>
        /// 是否可见（1=可见 0=隐藏）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否可见", DefaultValue = "1")]
        public byte IsVisible { get; set; } = 1;

        /// <summary>
        /// 状态（1=启用 0=禁用）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "状态", DefaultValue = "1")]
        public byte Status { get; set; } = 1;

        /// <summary>
        /// 按钮权限列表（JSON 数组，仅菜单类型有值）
        /// 存储格式：[{"buttonCode":"btn_add","buttonName":"新增","permKey":"sys:user:add",...}]
        /// </summary>
        [SugarColumn(ColumnDataType = "nvarchar(max)", IsNullable = true,
            ColumnDescription = "按钮权限JSON")]
        public string? Buttons { get; set; }

        /// <summary>
        /// 是否外链（1=是 0=否）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否外链", DefaultValue = "0")]
        public byte IsExternal { get; set; } = 0;

        /// <summary>
        /// 是否缓存页面（1=缓存 0=不缓存）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否缓存", DefaultValue = "0")]
        public byte IsCache { get; set; } = 0;

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
