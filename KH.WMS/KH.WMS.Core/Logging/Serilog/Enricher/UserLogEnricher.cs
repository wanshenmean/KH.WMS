using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.Constants;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace KH.WMS.Core.Logging.Serilog.Enricher
{

    /// <summary>
    /// 用户信息日志增强器
    /// </summary>
    public class UserLogEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserLogEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return;

            // 用户信息
            var user = httpContext.User;
            var userId = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userName = user?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserName", userName));

            // 租户信息
            var tenantId = user?.FindFirst("TenantId")?.Value;
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TenantId", tenantId));

            // 请求信息
            var requestId = httpContext.TraceIdentifier;
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestId", requestId));

            var correlationId = httpContext.Request.Headers[HeaderConstants.Tracing.X_CORRELATION_ID].FirstOrDefault();
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));

            var path = httpContext.Request.Path;
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestPath", path));

            var method = httpContext.Request.Method;
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestMethod", method));

            var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ClientIp", clientIp));

            // UserAgent
            var userAgent = httpContext.Request.Headers[HeaderConstants.Client.USER_AGENT].FirstOrDefault();
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserAgent", userAgent));
        }
    }

}
