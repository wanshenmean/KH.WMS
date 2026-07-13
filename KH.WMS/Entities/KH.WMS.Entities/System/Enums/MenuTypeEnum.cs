namespace KH.WMS.Entities.System.Enums
{
    /// <summary>
    /// 菜单类型枚举
    /// </summary>
    public enum MenuTypeEnum : byte
    {
        /// <summary>
        /// 目录（一级菜单分类，ParentId = 0）
        /// </summary>
        Directory = 0,

        /// <summary>
        /// 菜单（具体页面，ParentId = 目录的Id）
        /// </summary>
        Menu = 1
    }
}
