using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.System
{
    /// <summary>
    /// 系统附件
    /// </summary>
    [SugarTable("sys_attachment")]
    [SugarIndex("idx_business", nameof(BusinessType), OrderByType.Asc, nameof(BusinessId), OrderByType.Asc)]
    [SugarIndex("idx_upload_by", nameof(UploadBy), OrderByType.Asc)]
    public class SysAttachment : BaseEntity<long>
    {

        /// <summary>
        /// 文件名称
        /// </summary>
        
        [SugarColumn(Length = 200, IsNullable = false, ColumnDescription = "文件名称")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 文件路径
        /// </summary>
        
        [SugarColumn(Length = 500, IsNullable = false, ColumnDescription = "文件路径")]
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "文件大小", DefaultValue = "0")]
        public long FileSize { get; set; } = 0;

        /// <summary>
        /// 文件类型
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "文件类型")]
        public string? FileType { get; set; }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "文件扩展名")]
        public string? FileExt { get; set; }

        /// <summary>
        /// MIME类型
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnDescription = "MIME类型")]
        public string? MimeType { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "业务类型")]
        public string? BusinessType { get; set; }

        /// <summary>
        /// 业务ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "业务ID")]
        public long? BusinessId { get; set; }

        /// <summary>
        /// 上传人
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "上传人")]
        public long? UploadBy { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "上传时间", DefaultValue = "CURRENT_TIMESTAMP")]
        public DateTime UploadTime { get; set; } = DateTime.Now;
    }
}
