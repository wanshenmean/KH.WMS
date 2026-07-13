using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using Microsoft.AspNetCore.Http;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.SystemModule.Controllers
{
    /// <summary>
    /// 文件管理控制器（上传/下载/删除）
    /// </summary>
    [ApiController]
    [Route("api/file")]
    public class FileController : ControllerBase
    {
        private readonly ISysAttachmentService _attachmentService;

        public FileController(ISysAttachmentService attachmentService)
        {
            _attachmentService = attachmentService;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        [HttpPost("upload")]
        public async Task<ApiResponse> Upload(IFormFile file, string? businessType = null, long? businessId = null)
        {
            return await _attachmentService.UploadAsync(file, businessType, businessId);
        }

        /// <summary>
        /// 下载文件（通过静态文件路径返回）
        /// </summary>
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(long id)
        {
            var attachment = await _attachmentService.GetAttachmentAsync(id);
            if (attachment == null)
                return NotFound(new { message = "附件不存在" });

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), attachment.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "文件不存在" });

            var contentType = attachment.MimeType ?? "application/octet-stream";
            return PhysicalFile(filePath, contentType, attachment.FileName, enableRangeProcessing: true);
        }
    }
}
