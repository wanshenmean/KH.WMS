using KH.WMS.Entities.Constants;

namespace KH.WMS.Modules.SystemModule.DTOs
{
    /// <summary>
    /// 保存字典类型请求 DTO
    /// </summary>
    public class SaveDictTypeDto
    {
        public long? Id { get; set; }
        public string DictCode { get; set; } = string.Empty;
        public string DictName { get; set; } = string.Empty;

        /// <summary>
        /// 数据来源类型 0=静态 1=SQL
        /// </summary>
        public byte DataSourceType { get; set; }

        /// <summary>
        /// SQL查询语句（DataSourceType=1时必填）
        /// </summary>
        public string? SqlQuery { get; set; }

        /// <summary>
        /// 值列名
        /// </summary>
        public string? ValueColumn { get; set; }

        /// <summary>
        /// 标签列名
        /// </summary>
        public string? LabelColumn { get; set; }

        /// <summary>
        /// 缓存过期时间(分钟)
        /// </summary>
        public int CacheMinutes { get; set; } = 30;

        public byte IsActive { get; set; } = BizConstants.BoolFlag.YES;
        public string? Remark { get; set; }
    }
}
