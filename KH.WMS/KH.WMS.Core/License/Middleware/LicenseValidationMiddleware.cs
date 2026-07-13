using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.License.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.License.Middleware
{
    /// <summary>
    /// License 验证中间件 - 拦截未授权请求。
    /// License 失效时返回 HTTP 402 + 统一 ApiResponse(Code=LICENSE_REQUIRED)，
    /// 与 RBAC 的 403 区分；前端拦截器据 402 跳转授权恢复页 /license。
    /// </summary>
    public class LicenseValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LicenseValidationMiddleware> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 白名单路径（不需要 License 验证）：登录链路 + license 恢复端点 + 文档/健康检查。
        /// 用精确/带边界匹配，避免 StartsWith 前缀绕过（如 /api/license/import-xxx、/healthxxxx）。
        /// </summary>
        private static readonly string[] WhitelistPaths =
        {
            "/api/user/login",
            "/api/user/public-key",
            "/api/user/logout",
            "/api/license/machine-code",
            "/api/license/import",
            "/api/license/info",
            "/api/license/upload",
            "/swagger",
            "/health",
            "/healthchecks"
        };

        /// <summary>
        /// 与系统全局(System.Text.Json + CamelCase + UnsafeRelaxedJsonEscaping)一致的序列化选项，
        /// 保证中间件手写的 402 响应体能被前端按 data.code/data.message 正确解析。
        /// </summary>
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        public LicenseValidationMiddleware(RequestDelegate next, ILogger<LicenseValidationMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context, ILicenseService licenseService)
        {
            // 开关：License:Enabled=false 时不拦截（仅 dev/测试，生产须保持 true）
            var enabled = _configuration.GetValue<bool?>("License:Enabled") ?? true;
            if (!enabled)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value ?? string.Empty;

            // 白名单路径放行
            if (IsWhitelisted(path))
            {
                await _next(context);
                return;
            }

            // 验证 License
            var licenseData = licenseService.ValidateLicense();

            if (licenseData == null)
            {
                var errorMessage = licenseService.GetValidationErrorMessage() ?? "系统授权校验未通过";
                _logger.LogWarning("请求被拒绝(License 无效): {ErrorMessage}, 路径: {Path}", errorMessage, path);

                var apiResponse = ApiResponse.Fail(ResponseCode.LICENSE_REQUIRED, errorMessage);
                context.Response.StatusCode = ResponseCode.LICENSE_REQUIRED; // 402
                context.Response.ContentType = "application/json; charset=utf-8";

                var json = JsonSerializer.Serialize(apiResponse, JsonOptions);
                await context.Response.WriteAsync(json);
                return;
            }

            await _next(context);
        }

        /// <summary>
        /// 精确或带边界匹配：path == w，或 path 以 w + "/" 或 w + "?" 开头（兼容 query 形式）
        /// </summary>
        private static bool IsWhitelisted(string path)
        {
            foreach (var whitelistPath in WhitelistPaths)
            {
                if (path.Equals(whitelistPath, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (path.StartsWith(whitelistPath + "/", StringComparison.OrdinalIgnoreCase))
                    return true;
                if (path.StartsWith(whitelistPath + "?", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
