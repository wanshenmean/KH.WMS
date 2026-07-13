using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.License.DTOs;
using KH.WMS.Core.License.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Core.License.Controllers
{


    /// <summary>
    /// License 管理控制器
    /// </summary>
    [ApiController]
    [Route("api/license")]
    public class LicenseController : ControllerBase
    {
        private readonly ILicenseAppService _licenseAppService;
        //dotnet run -- -m <机器码> -d 365 -t Standard -o license.lic
        public LicenseController(ILicenseAppService licenseAppService)
        {
            _licenseAppService = licenseAppService;
        }

        /// <summary>
        /// 获取当前服务器的机器码
        /// </summary>
        [HttpGet("machine-code")]
        public async Task<IActionResult> GetMachineCode()
        {
            var result = _licenseAppService.GetMachineCode();
            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }
            return Ok(new { machineCode = result.Value });
        }

        /// <summary>
        /// 获取当前 License 信息
        /// </summary>
        [HttpGet("info")]
        public async Task<IActionResult> GetLicenseInfo()
        {
            var result = _licenseAppService.GetLicenseInfo();
            if (result.IsFailure)
            {
                return Ok(new { isValid = false, message = result.Error });
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// 生成 License 文件（需要私钥，仅管理员使用）
        /// </summary>
        [HttpPost("generate")]
        [Authorize]
        public async Task<IActionResult> GenerateLicense([FromBody] GenerateLicenseRequest request)
        {
            var result = _licenseAppService.GenerateLicenseFile(request);
            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            // 返回 Base64 内容，前端可下载为 .lic 文件
            return Ok(new
            {
                licenseContent = result.Value,
                fileName = $"license_{request.MachineCode.Substring(0, 8)}_{request.ValidDays}days.lic"
            });
        }

        /// <summary>
        /// 导入 License 文件
        /// </summary>
        [HttpPost("import")]
        [Authorize]
        public async Task<IActionResult> ImportLicense([FromBody] ImportLicenseRequest request)
        {
            var result = _licenseAppService.ImportLicense(request);
            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }
            return Ok(new { message = "License 导入成功" });
        }

        /// <summary>
        /// 上传 License 文件（.lic 文件上传方式）
        /// </summary>
        [HttpPost("upload")]
        [RequestSizeLimit(10 * 1024)] // 10KB 限制
        [Authorize]
        public async Task<IActionResult> UploadLicense(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "请上传 License 文件" });
            }

            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            var result = _licenseAppService.ImportLicense(new ImportLicenseRequest { LicenseContent = content });
            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }
            return Ok(new { message = "License 导入成功" });
        }
    }
}
