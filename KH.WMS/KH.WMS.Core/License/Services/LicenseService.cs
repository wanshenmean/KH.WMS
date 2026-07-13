using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.License.Crypto;
using KH.WMS.Core.License.Interfaces;
using KH.WMS.Core.License.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.License.Services
{
    /// <summary>
    /// License 核心服务实现
    /// </summary>
    [RegisteredService(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton, WithoutInterceptor = true)]
    public class LicenseService : ILicenseService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LicenseService> _logger;
        private readonly string _licenseFilePath;
        private readonly string _publicKeyPath;
        private readonly string _privateKeyPath;
        private RsaLicenseVerifier? _verifier;
        private string? _cachedMachineCode;
        private LicenseData? _cachedLicenseData;
        private DateTime _cacheExpiry;
        private string? _lastErrorMessage;
        private readonly object _cacheLock = new();

        // License/密钥文件名（伪装为系统 DLL，降低被随意翻改的风险；路径基于应用根目录）
        private const string LicenseFileName = "System.Private.Core.dll";
        private const string PublicKeyFileName = "System.Private.Security.dll";
        private const string PrivateKeyFileName = "System.Private.Crypto.dll";

        public LicenseService(IConfiguration configuration, ILogger<LicenseService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            _licenseFilePath = Path.Combine(basePath, LicenseFileName);
            _publicKeyPath = Path.Combine(basePath, PublicKeyFileName);
            _privateKeyPath = Path.Combine(basePath, PrivateKeyFileName);
        }

        /// <inheritdoc />
        public string GetMachineCode()
        {
            if (_cachedMachineCode != null)
                return _cachedMachineCode;

            try
            {
                _cachedMachineCode = MachineCodeGenerator.GetMachineCode();
                _logger.LogInformation("机器码获取成功: {MachineCode}", _cachedMachineCode);
                return _cachedMachineCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取机器码失败");
                throw new InvalidOperationException("无法获取服务器机器码，请确保具有足够的系统权限", ex);
            }
        }

        /// <inheritdoc />
        public LicenseData? ValidateLicense()
        {
            var now = DateTime.Now;

            lock (_cacheLock)
            {
                // 如果缓存有效，直接返回缓存结果
                if (_cachedLicenseData != null && now < _cacheExpiry)
                {
                    return _cachedLicenseData;
                }
            }

            var verifier = GetVerifier();
            if (verifier == null)
            {
                _lastErrorMessage = "公钥文件不存在，请配置有效的公钥";
                return null;
            }

            if (!File.Exists(_licenseFilePath))
            {
                _lastErrorMessage = "License 文件不存在，请导入有效的 License 文件";
                return null;
            }

            try
            {
                var licenseContent = File.ReadAllText(_licenseFilePath);
                var machineCode = GetMachineCode();

                var licenseData = verifier.Verify(licenseContent, machineCode, out var errorMessage);

                if (licenseData == null)
                {
                    _lastErrorMessage = errorMessage;
                    lock (_cacheLock)
                    {
                        _cachedLicenseData = null;
                    }
                    return null;
                }

                _lastErrorMessage = null;

                lock (_cacheLock)
                {
                    _cachedLicenseData = licenseData;
                    var cacheMinutes = int.TryParse(_configuration["License:ValidateIntervalMinutes"], out var minutes)
                        ? minutes
                        : 5;
                    _cacheExpiry = now.AddMinutes(cacheMinutes);
                }

                return licenseData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "License 验证异常");
                _lastErrorMessage = $"License 验证异常: {ex.Message}";
                return null;
            }
        }

        /// <inheritdoc />
        public bool ImportLicense(string licenseContent)
        {
            var verifier = GetVerifier();
            if (verifier == null)
            {
                _lastErrorMessage = "公钥文件不存在，无法验证 License";
                return false;
            }

            // 先验证 License 有效性
            var machineCode = GetMachineCode();
            var licenseData = verifier.Verify(licenseContent, machineCode, out var errorMessage);

            if (licenseData == null)
            {
                _lastErrorMessage = errorMessage;
                return false;
            }

            // 确保目录存在
            var directory = Path.GetDirectoryName(_licenseFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 写入文件
            File.WriteAllText(_licenseFilePath, licenseContent.Trim());

            // 更新缓存
            lock (_cacheLock)
            {
                _cachedLicenseData = licenseData;
                var cacheMinutes = int.TryParse(_configuration["License:ValidateIntervalMinutes"], out var minutes)
                    ? minutes
                    : 5;
                _cacheExpiry = DateTime.Now.AddMinutes(cacheMinutes);
            }

            _lastErrorMessage = null;
            _logger.LogInformation("License 导入成功，过期时间: {ExpiresAt}", licenseData.ExpiresAt);
            return true;
        }

        /// <inheritdoc />
        public LicenseData? GetCurrentLicense()
        {
            if (!File.Exists(_licenseFilePath))
                return null;

            try
            {
                var licenseContent = File.ReadAllText(_licenseFilePath);
                var verifier = GetVerifier();
                if (verifier == null) return null;

                var machineCode = GetMachineCode();
                // 不验证机器码和过期，只解析内容
                var licenseJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(licenseContent.Trim()));
                var licenseFile = Newtonsoft.Json.JsonConvert.DeserializeObject<LicenseFile>(licenseJson);
                return licenseFile?.Data;
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc />
        public string? GetValidationErrorMessage()
        {
            return _lastErrorMessage;
        }

        /// <inheritdoc />
        public void EnsureDefaultLicense()
        {
            // 如果 License 文件已存在，跳过初始化
            if (File.Exists(_licenseFilePath))
            {
                _logger.LogInformation("License 文件已存在，跳过默认授权初始化");
                return;
            }

            try
            {
                // 1. 确保目录存在
                var licenseDir = Path.GetDirectoryName(_licenseFilePath);
                if (!string.IsNullOrEmpty(licenseDir))
                    Directory.CreateDirectory(licenseDir);

                var keysDir = Path.GetDirectoryName(_publicKeyPath);
                if (!string.IsNullOrEmpty(keysDir))
                    Directory.CreateDirectory(keysDir);

                // 2. 如果密钥对不存在，生成 RSA 密钥对
                if (!File.Exists(_publicKeyPath) || !File.Exists(_privateKeyPath))
                {
                    _logger.LogInformation("首次启动，正在生成 RSA 密钥对...");
                    var (publicKeyPem, privateKeyPem) = RsaKeyHelper.GenerateKeyPair();
                    File.WriteAllText(_publicKeyPath, publicKeyPem);
                    File.WriteAllText(_privateKeyPath, privateKeyPem);
                    _logger.LogInformation("RSA 密钥对生成成功");
                }

                // 3. 获取机器码
                var machineCode = GetMachineCode();

                // 4. 使用私钥生成默认 180 天 License
                var defaultDays = int.TryParse(_configuration["License:DefaultValidDays"], out var days) && days > 0
                    ? days
                    : 180;

                _logger.LogInformation("正在生成默认授权 License，有效天数: {DefaultDays}，机器码: {MachineCode}", defaultDays, machineCode);

                var privateKeyPemContent = File.ReadAllText(_privateKeyPath);
                using var rsa = RsaKeyHelper.CreateFromPrivateKey(privateKeyPemContent);
                var signer = new RsaLicenseSigner(rsa);
                var licenseContent = signer.GenerateLicense(machineCode, defaultDays, "Standard");

                // 5. 保存 License 文件
                File.WriteAllText(_licenseFilePath, licenseContent);

                // 6. 重置验证器缓存，使其使用新生成的公钥
                _verifier = null;

                _logger.LogInformation("默认授权 License 生成成功，过期时间: {ExpiresAt}", DateTime.Now.AddDays(defaultDays).ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "默认授权 License 初始化失败");
            }
        }

        private RsaLicenseVerifier? GetVerifier()
        {
            if (_verifier != null)
                return _verifier;

            if (!File.Exists(_publicKeyPath))
            {
                _logger.LogWarning("公钥文件不存在: {PublicKeyPath}", _publicKeyPath);
                return null;
            }

            try
            {
                var publicKeyPem = File.ReadAllText(_publicKeyPath);
                var rsa = RsaKeyHelper.CreateFromPublicKey(publicKeyPem);
                _verifier = new RsaLicenseVerifier(rsa);
                return _verifier;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载公钥失败: {PublicKeyPath}", _publicKeyPath);
                return null;
            }
        }
    }
}
