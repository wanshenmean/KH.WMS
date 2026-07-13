using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.License.Crypto;
using KH.WMS.Core.License.DTOs;
using KH.WMS.Core.License.Interfaces;
using KH.WMS.Core.License.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace KH.WMS.Core.License.Services
{
    /// <summary>
    /// License 应用服务实现
    /// </summary>
    [RegisteredService(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped)]
    public class LicenseAppService : ILicenseAppService
    {
        private readonly ILicenseService _licenseService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LicenseAppService> _logger;

        public LicenseAppService(
            ILicenseService licenseService,
            IConfiguration configuration,
            ILogger<LicenseAppService> logger)
        {
            _licenseService = licenseService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc />
        public Result<string> GetMachineCode()
        {
            try
            {
                var machineCode = _licenseService.GetMachineCode();
                return Result<string>.Success(machineCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取机器码失败");
                return Result<string>.Failure($"获取机器码失败: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public Result<LicenseInfoDto> GetLicenseInfo()
        {
            try
            {
                var licenseData = _licenseService.ValidateLicense();
                if (licenseData == null)
                {
                    var errorMsg = _licenseService.GetValidationErrorMessage() ?? "License 无效";
                    return Result<LicenseInfoDto>.Failure(errorMsg);
                }

                var now = DateTime.Now;
                var remainingDays = (licenseData.ExpiresAt - now).Days;
                if (remainingDays < 0) remainingDays = 0;

                var info = new LicenseInfoDto
                {
                    MachineCode = licenseData.MachineCode,
                    ProductName = licenseData.ProductName,
                    IssuedAt = licenseData.IssuedAt,
                    ExpiresAt = licenseData.ExpiresAt,
                    ValidDays = licenseData.ValidDays,
                    LicenseType = licenseData.LicenseType,
                    IsExpired = now > licenseData.ExpiresAt,
                    RemainingDays = remainingDays,
                    LicenseId = licenseData.LicenseId,
                    IsValid = true
                };

                return Result<LicenseInfoDto>.Success(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 License 信息失败");
                return Result<LicenseInfoDto>.Failure($"获取 License 信息失败: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public Result<string> GenerateLicenseFile(GenerateLicenseRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.MachineCode))
                    return Result<string>.Failure("机器码不能为空");

                if (request.ValidDays <= 0)
                    return Result<string>.Failure("有效天数必须大于 0");

                // 读取私钥
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var privateKeyPath = Path.Combine(basePath,
                    _configuration["License:PrivateKeyPath"] ?? "Keys/private_key.pem");

                if (!File.Exists(privateKeyPath))
                {
                    return Result<string>.Failure("私钥文件不存在，无法生成 License。请将私钥文件放置到正确位置。");
                }

                var privateKeyPem = File.ReadAllText(privateKeyPath);
                var rsa = RsaKeyHelper.CreateFromPrivateKey(privateKeyPem);
                var signer = new RsaLicenseSigner(rsa);

                var licenseContent = signer.GenerateLicense(
                    request.MachineCode,
                    request.ValidDays,
                    request.LicenseType);

                return Result<string>.Success(licenseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成 License 失败");
                return Result<string>.Failure($"生成 License 失败: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public Result ImportLicense(ImportLicenseRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.LicenseContent))
                    return Result.Failure("License 内容不能为空");

                var success = _licenseService.ImportLicense(request.LicenseContent);
                if (!success)
                {
                    var errorMsg = _licenseService.GetValidationErrorMessage() ?? "License 导入失败";
                    return Result.Failure(errorMsg);
                }

                return Result.Success("License 导入成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导入 License 失败");
                return Result.Failure($"导入 License 失败: {ex.Message}");
            }
        }
    }
}
