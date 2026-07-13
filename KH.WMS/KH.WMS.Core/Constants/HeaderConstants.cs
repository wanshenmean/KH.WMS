namespace KH.WMS.Core.Constants;

/// <summary>
/// HTTP 请求头常量
/// </summary>
public static class HeaderConstants
{
    /// <summary>
    /// 认证相关
    /// </summary>
    public static class Authentication
    {
        public const string AUTHORIZATION = "Authorization";
        public const string BEARER_PREFIX = "Bearer ";
        public const string X_ACCESS_TOKEN = "X-Access-Token";
        public const string X_REFRESH_TOKEN = "X-Refresh-Token";
    }

    /// <summary>
    /// 追踪相关
    /// </summary>
    public static class Tracing
    {
        public const string X_REQUEST_ID = "X-Request-ID";
        public const string X_CORRELATION_ID = "X-Correlation-ID";
        public const string X_TRACE_ID = "X-Trace-ID";
        public const string X_BUSINESS_ID = "X-Business-ID";
    }

    /// <summary>
    /// 客户端相关
    /// </summary>
    public static class Client
    {
        public const string USER_AGENT = "User-Agent";
        public const string X_CLIENT_VERSION = "X-Client-Version";
        public const string X_CLIENT_ID = "X-Client-Id";
        public const string X_DEVICE_ID = "X-Device-ID";
        public const string X_DEVICE_TYPE = "X-Device-Type";
        public const string X_OS = "X-OS";
        public const string X_OS_VERSION = "X-OS-Version";
    }

    /// <summary>
    /// 请求上下文
    /// </summary>
    public static class Context
    {
        public const string X_TENANT_ID = "X-Tenant-Id";
        public const string X_ORGANIZATION_ID = "X-Organization-Id";
        public const string X_LANGUAGE = "X-Language";
        public const string X_TIMEZONE = "X-Timezone";
    }

    /// <summary>
    /// 分页相关
    /// </summary>
    public static class Pagination
    {
        public const string X_PAGE_INDEX = "X-Page-Index";
        public const string X_PAGE_SIZE = "X-Page-Size";
        public const string X_TOTAL_COUNT = "X-Total-Count";
    }

    /// <summary>
    /// 内容协商
    /// </summary>
    public static class Content
    {
        public const string CONTENT_TYPE = "Content-Type";
        public const string ACCEPT = "Accept";
        public const string ACCEPT_ENCODING = "Accept-Encoding";
        public const string ACCEPT_LANGUAGE = "Accept-Language";
    }

    /// <summary>
    /// 数据格式
    /// </summary>
    public static class ContentTypes
    {
        public const string APPLICATION_JSON = "application/json";
        public const string APPLICATION_XML = "application/xml";
        public const string TEXT_PLAIN = "text/plain";
        public const string MULTIPART_FORM_DATA = "multipart/form-data";
    }

    /// <summary>
    /// 缓存控制
    /// </summary>
    public static class Cache
    {
        public const string CACHE_CONTROL = "Cache-Control";
        public const string ETAG = "ETag";
        public const string IF_NONE_MATCH = "If-None-Match";
        public const string IF_MODIFIED_SINCE = "If-Modified-Since";
    }

    /// <summary>
    /// 安全相关
    /// </summary>
    public static class Security
    {
        public const string X_CSRF_TOKEN = "X-CSRF-Token";
        public const string X_REQUESTED_WITH = "X-Requested-With";
        public const string X_FORWARDED_FOR = "X-Forwarded-For";
        public const string X_REAL_IP = "X-Real-IP";
    }

    /// <summary>
    /// API 相关
    /// </summary>
    public static class Api
    {
        public const string X_API_KEY = "X-API-Key";
        public const string X_API_VERSION = "X-API-Version";
        public const string X_SIGNATURE = "X-Signature";
        public const string X_TIMESTAMP = "X-Timestamp";
    }
}
