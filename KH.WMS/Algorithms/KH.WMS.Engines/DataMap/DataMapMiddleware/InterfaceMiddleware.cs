using System.Text.Json;
using KH.WMS.Core.License.Interfaces;
using KH.WMS.Core.License.Models;
using KH.WMS.Core.License.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KH.WMS.Engines.DataMap;

/// <summary>
/// 入站接口中间件 - 自动拦截和处理入站接口请求
/// </summary>
public class InterfaceMiddleware
{
    private const string InterfaceHeaderKey = "X-Interface-Code";
    private readonly RequestDelegate _next;
    private readonly ILogger<InterfaceMiddleware> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public InterfaceMiddleware(
        RequestDelegate next,
        ILogger<InterfaceMiddleware> logger,
        IWebHostEnvironment webHostEnvironment)
    {
        _next = next;
        _logger = logger;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task InvokeAsync(HttpContext context, ILicenseService licenseService)
    {
        var path = context.Request.Path.Value ?? "";
        var method = context.Request.Method;

        // 检查是否是入站接口请求
        var interfaceScheduler = context.RequestServices.GetService<InterfaceScheduler>();
        if (interfaceScheduler == null)
        {
            await _next(context);
            return;
        }

        var licenseData = licenseService.ValidateLicense();
        if(licenseData == null /*&& _webHostEnvironment.IsProduction()*/)
        {
            var errorMessage = licenseService.GetValidationErrorMessage() ?? "系统配置校验未通过";
            _logger.LogWarning("请求被拒绝: {ErrorMessage}, 路径: {Path}", errorMessage, path);

            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json; charset=utf-8";

            var response = new
            {
                type = "https://httpstatuses.com/403",
                title = "Forbidden",
                status = 403,
                detail = errorMessage,
                instance = path
            };

            var json = JsonConvert.SerializeObject(response);

            await context.Response.WriteAsync(json);
            return;
        }

        string? interfaceCode = null;

        // 方式1: 通过请求头中的接口编码
        if (context.Request.Headers.TryGetValue(InterfaceHeaderKey, out var interfaceCodeHeader))
        {
            interfaceCode = interfaceCodeHeader.ToString();
            _logger.LogInformation("检测到入站接口请求（通过请求头）: {InterfaceCode}", interfaceCode);
        }

        // 方式2: 通过路径匹配
        var matchedInterface = await interfaceScheduler.MatchInterfaceByPathAsync(path, method);

        // 如果通过路径匹配到了接口，并且没有通过请求头指定接口编码，则使用路径匹配的接口
        if (matchedInterface != null && string.IsNullOrEmpty(interfaceCode))
        {
            interfaceCode = matchedInterface.InterfaceCode;
            _logger.LogInformation("检测到入站接口请求（通过路径匹配）: {InterfaceCode} - {Path}", interfaceCode, path);
        }

        // 如果找到了接口配置，处理请求
        if (!string.IsNullOrEmpty(interfaceCode))
        {
            // 入站接口鉴权：必须携带与配置一致的接口密钥（X-Interface-Key），
            // 防止匿名外部请求触发业务处理（原仅校验 License，存在匿名绕过）。
            if (!ValidateInboundApiKey(context))
            {
                _logger.LogWarning("入站接口请求被拒绝（密钥无效或未提供）: {InterfaceCode} - {Path}", interfaceCode, path);
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "入站接口密钥无效或未提供",
                    timestamp = DateTime.Now
                }));
                return;
            }

            // 如果是通过请求头指定的接口编码，需要重新匹配接口配置
            if (matchedInterface == null || matchedInterface.InterfaceCode != interfaceCode)
            {
                matchedInterface = await interfaceScheduler.GetInterfaceByCodeAsync(interfaceCode);
            }

            if (matchedInterface != null)
            {
                await ProcessInterfaceRequestAsync(context, interfaceCode, matchedInterface);
                return;
            }
            else
            {
                _logger.LogWarning("未找到接口配置: {InterfaceCode}", interfaceCode);
            }
        }

        // 不是入站接口请求，继续处理
        await _next(context);
    }

    /// <summary>
    /// 校验入站接口密钥：请求头 X-Interface-Key 必须与 appsettings 中 DataMap:InboundApiKey 一致。
    /// 未配置密钥时 fail-closed（拒绝），避免回到完全匿名；比对使用固定时间比较以防时序侧信道。
    /// </summary>
    private static bool ValidateInboundApiKey(HttpContext context)
    {
        var configuration = context.RequestServices.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        var expected = configuration["DataMap:InboundApiKey"];
        if (string.IsNullOrWhiteSpace(expected))
            return false;

        if (!context.Request.Headers.TryGetValue("X-Interface-Key", out var provided))
            return false;

        var expectedBytes = System.Text.Encoding.UTF8.GetBytes(expected);
        var providedBytes = System.Text.Encoding.UTF8.GetBytes(provided.ToString());
        if (providedBytes.Length != expectedBytes.Length)
            return false;

        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }

    /// <summary>
    /// 处理入站接口请求
    /// </summary>
    private async Task ProcessInterfaceRequestAsync(HttpContext context, string interfaceCode, IntInterfaceConfig config)
    {
        try
        {
            var path = context.Request.Path.Value ?? "";
            _logger.LogInformation("接收入站接口请求: {InterfaceCode} - {Path}", interfaceCode, path);

            // 获取 InterfaceScheduler 服务
            var interfaceScheduler = context.RequestServices.GetRequiredService<InterfaceScheduler>();

            // 处理入站接口请求
            var (success, message, data) = await interfaceScheduler.ProcessInboundRequestAsync(
                interfaceCode,
                context.Request,
                context.RequestAborted);

            if (!success)
            {
                // 处理失败，返回错误响应
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                var errorResponse = JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = message,
                    timestamp = DateTime.Now
                });
                await context.Response.WriteAsync(errorResponse);
                return;
            }

            // 处理成功，返回成功响应
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            var successResponse = JsonConvert.SerializeObject(new
            {
                success = true,
                message = message,
                data = data,
                timestamp = DateTime.Now
            });
            await context.Response.WriteAsync(successResponse);
        }
        catch (Exception ex)
        {
            var path = context.Request.Path.Value ?? "";
            _logger.LogError(ex, "处理入站接口请求时发生异常: {InterfaceCode} - {Path}", interfaceCode, path);

            // 返回错误响应
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var errorResponse = JsonConvert.SerializeObject(new
            {
                success = false,
                message = "处理接口请求时发生异常",
                error = ex.Message,
                timestamp = DateTime.Now
            });
            await context.Response.WriteAsync(errorResponse);
        }
    }
}

/// <summary>
/// 入站接口中间件扩展方法
/// </summary>
public static class InterfaceMiddlewareExtensions
{
    public static IApplicationBuilder UseInterfaceMiddleware(this IApplicationBuilder builder, IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var interfaceScheduler = scope.ServiceProvider.GetRequiredService<InterfaceScheduler>();
        // 确保 InterfaceScheduler 在中间件之前被解析和初始化
        interfaceScheduler.LoadEnabledInboundInterfacesAsync().Wait();

        return builder.UseMiddleware<InterfaceMiddleware>();
    }
}
