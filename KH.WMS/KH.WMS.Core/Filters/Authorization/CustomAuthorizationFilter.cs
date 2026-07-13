using System.Security.Claims;
using KH.WMS.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Core.Filters.Authorization;

/// <summary>
/// 自定义授权过滤器
/// </summary>
public class CustomAuthorizationFilter : IAuthorizationFilter
{
    private readonly string _policy;
    private readonly string[] _roles;
    private readonly ILoggerService _logger;

    public CustomAuthorizationFilter(string policy, string[] roles, ILoggerService logger)
    {
        _policy = policy;
        _roles = roles;
        _logger = logger;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // 检查是否已认证
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("未授权的访问请求: {Path}", context.HttpContext.Request.Path);
            context.Result = new UnauthorizedResult();
            return;
        }

        // 检查角色
        if (_roles.Length > 0 && !_roles.Any(role => user.IsInRole(role)))
        {
            _logger.LogWarning("用户 {User} 无角色访问: {Path}", user.Identity?.Name, context.HttpContext.Request.Path);
            context.Result = new ForbidResult();
            return;
        }

        // 检查策略
        if (!string.IsNullOrEmpty(_policy))
        {
            // 这里可以添加策略检查逻辑
            _logger.LogDebug("策略检查: {Policy}", _policy);
        }
    }
}

/// <summary>
/// 授权特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class CustomAuthorizeAttribute : Attribute, IFilterFactory
{
    public string Policy { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();

    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerService>();
        return new CustomAuthorizationFilter(Policy, Roles, logger);
    }

    public CustomAuthorizeAttribute()
    {
    }

    public CustomAuthorizeAttribute(string policy)
    {
        Policy = policy;
    }

    public CustomAuthorizeAttribute(params string[] roles)
    {
        Roles = roles;
    }
}
