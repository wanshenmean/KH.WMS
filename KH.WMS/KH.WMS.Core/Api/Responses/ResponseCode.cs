namespace KH.WMS.Core.Api.Responses;

/// <summary>
/// 响应状态码
/// </summary>
public static class ResponseCode
{
    // 成功 2xx
    public const int SUCCESS = 200;
    public const int CREATED = 201;
    public const int ACCEPTED = 202;
    public const int NO_CONTENT = 204;

    // 客户端错误 4xx
    public const int BAD_REQUEST = 400;
    public const int UNAUTHORIZED = 401;
    public const int FORBIDDEN = 403;
    /// <summary>
    /// License 授权校验未通过（与 RBAC 的 403 区分：前端据此跳转授权恢复页 /license，而非当作"无菜单权限"）
    /// </summary>
    public const int LICENSE_REQUIRED = 402;
    public const int NOT_FOUND = 404;
    public const int METHOD_NOT_ALLOWED = 405;
    public const int REQUEST_TIMEOUT = 408;
    public const int CONFLICT = 409;
    public const int VALIDATION_ERROR = 422;
    public const int RATE_LIMIT_EXCEEDED = 429;

    // 服务器错误 5xx
    public const int INTERNAL_SERVER_ERROR = 500;
    public const int NOT_IMPLEMENTED = 501;
    public const int BAD_GATEWAY = 502;
    public const int SERVICE_UNAVAILABLE = 503;
    public const int GATEWAY_TIMEOUT = 504;

    /// <summary>
    /// 获取响应码对应的消息
    /// </summary>
    public static string GetMessage(int code)
    {
        return code switch
        {
            SUCCESS => "操作成功",
            CREATED => "创建成功",
            ACCEPTED => "请求已接受",
            NO_CONTENT => "无内容返回",

            BAD_REQUEST => "请求参数错误",
            UNAUTHORIZED => "未授权访问",
            FORBIDDEN => "无权限访问",
            LICENSE_REQUIRED => "系统授权校验未通过",
            NOT_FOUND => "资源未找到",
            METHOD_NOT_ALLOWED => "请求方法不允许",
            REQUEST_TIMEOUT => "请求超时",
            CONFLICT => "数据冲突",
            VALIDATION_ERROR => "数据验证失败",
            RATE_LIMIT_EXCEEDED => "请求过于频繁",

            INTERNAL_SERVER_ERROR => "服务器内部错误",
            NOT_IMPLEMENTED => "功能未实现",
            BAD_GATEWAY => "网关错误",
            SERVICE_UNAVAILABLE => "服务不可用",
            GATEWAY_TIMEOUT => "网关超时",

            _ => "未知错误"
        };
    }

    /// <summary>
    /// 获取响应码对应的 HTTP 状态码
    /// </summary>
    public static int GetHttpStatusCode(int code)
    {
        return code switch
        {
            SUCCESS or CREATED or ACCEPTED or NO_CONTENT => 200,
            BAD_REQUEST => 400,
            UNAUTHORIZED => 401,
            LICENSE_REQUIRED => 402,
            FORBIDDEN => 403,
            NOT_FOUND => 404,
            METHOD_NOT_ALLOWED => 405,
            REQUEST_TIMEOUT => 408,
            CONFLICT => 409,
            VALIDATION_ERROR => 422,
            RATE_LIMIT_EXCEEDED => 429,
            INTERNAL_SERVER_ERROR => 500,
            NOT_IMPLEMENTED => 501,
            BAD_GATEWAY => 502,
            SERVICE_UNAVAILABLE => 503,
            GATEWAY_TIMEOUT => 504,
            _ => 500
        };
    }
}
