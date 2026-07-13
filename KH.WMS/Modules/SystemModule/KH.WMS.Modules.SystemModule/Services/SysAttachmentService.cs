using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Core.UserProvide;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.Interfaces;
using Microsoft.AspNetCore.Http;

namespace KH.WMS.Modules.SystemModule.Services
{
    /// <summary>
    /// 系统附件服务实现
    /// </summary>
    [RegisteredService(ServiceType = typeof(ISysAttachmentService))]
    public class SysAttachmentService(
        IRepository<SysAttachment, long> repository,
        IUnitOfWork unitOfWork,
        IUserContext userContext) : CrudService<SysAttachment>(repository, unitOfWork), ISysAttachmentService
    {
        private const string UploadDir = "Uploads";
        private readonly IRepository<SysAttachment, long> _repository = repository;
        private readonly IUserContext _userContext = userContext;

        private static readonly HashSet<string> ImageExts = new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp", ".svg" };
        private static readonly HashSet<string> DocExts = new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv" };
        private static readonly HashSet<string> VideoExts = new(StringComparer.OrdinalIgnoreCase) { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv" };

        /// <inheritdoc />
        public async Task<ApiResponse> UploadAsync(IFormFile file, string? businessType = null, long? businessId = null)
        {
            if (file == null || file.Length == 0)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "请选择要上传的文件");

            var ext = Path.GetExtension(file.FileName);
            var relativeDir = DateTime.Now.ToString("yyyy/MM/dd");
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var relativePath = $"{relativeDir}/{fileName}";
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), UploadDir, relativeDir, fileName);

            // 确保目录存在
            var dir = Path.GetDirectoryName(fullPath)!;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // 保存文件
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 判断文件类型
            string fileType = GetFileType(ext);

            var attachment = new SysAttachment
            {
                FileName = file.FileName,
                FilePath = $"/{UploadDir}/{relativePath}",
                FileSize = file.Length,
                FileType = fileType,
                FileExt = ext,
                MimeType = file.ContentType,
                BusinessType = businessType,
                BusinessId = businessId,
                UploadBy = _userContext.UserId,
                UploadTime = DateTime.Now,
            };

            var id = await _repository.AddAsync(attachment);
            attachment.Id = id;
            return ApiResponse.Ok(attachment, "上传成功");
        }

        /// <inheritdoc />
        public async Task<SysAttachment?> GetAttachmentAsync(long id)
        {
            return await _repository.GetByIdAsync(id);
        }

        /// <summary>
        /// 根据扩展名判断文件类型分类
        /// </summary>
        private static string GetFileType(string ext)
        {
            if (ImageExts.Contains(ext)) return "图片";
            if (DocExts.Contains(ext)) return "文档";
            if (VideoExts.Contains(ext)) return "视频";
            return "其他";
        }
    }
}
