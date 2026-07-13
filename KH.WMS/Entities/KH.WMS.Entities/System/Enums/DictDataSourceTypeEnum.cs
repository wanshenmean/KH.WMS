namespace KH.WMS.Entities.System.Enums
{
    /// <summary>
    /// 字典数据来源类型枚举
    /// </summary>
    public enum DictDataSourceTypeEnum : byte
    {
        /// <summary>
        /// 静态字典项（使用 SysDictItem）
        /// </summary>
        Static = 0,

        /// <summary>
        /// SQL动态查询
        /// </summary>
        Sql = 1
    }
}
