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
    /// 关联ID日志增强器
    /// </summary>
    public class CorrelationIdEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return;

            // 从请求头获取关联ID
            var correlationId = httpContext.Request.Headers[HeaderConstants.Tracing.X_CORRELATION_ID].FirstOrDefault();

            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = httpContext.TraceIdentifier;
            }

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));

            // 添加业务关联ID（如果有）
            var businessId = httpContext.Request.Headers[HeaderConstants.Tracing.X_BUSINESS_ID].FirstOrDefault();
            if (!string.IsNullOrEmpty(businessId))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("BusinessId", businessId));
            }
        }
    }

}
