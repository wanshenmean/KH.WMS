using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Services;
using KH.WMS.Entities.System;
using Microsoft.AspNetCore.Http;

namespace KH.WMS.Modules.SystemModule.Interfaces
{
    /// <summary>
    /// 系统附件服务接口
    /// </summary>
    public interface ISysAttachmentService : ICrudService<SysAttachment>
    {
        /// <summary>
        /// 上传文件
        /// </summary>
        Task<ApiResponse> UploadAsync(IFormFile file, string? businessType = null, long? businessId = null);

        /// <summary>
        /// 获取文件记录（用于下载）
        /// </summary>
        Task<SysAttachment?> GetAttachmentAsync(long id);
    }
}
