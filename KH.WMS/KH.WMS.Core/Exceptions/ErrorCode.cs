namespace KH.WMS.Core.Exceptions;

/// <summary>
/// 错误码枚举
/// </summary>
public static class ErrorCodes
{
    // 通用错误 1xxx
    public const int SUCCESS = 0;
    public const int SYSTEM_ERROR = 1000;
    public const int NETWORK_ERROR = 1001;
    public const int TIMEOUT_ERROR = 1002;
    public const int UNKNOWN_ERROR = 1999;
    // 业务错误 2xxx
    public const int BUSINESS_ERROR = 2000;
    public const int OPERATION_FAILED = 2001;
    public const int STATE_ERROR = 2002;

    // 验证错误 3xxx
    public const int VALIDATION_ERROR = 3000;
    public const int REQUIRED_FIELD_MISSING = 3001;
    public const int INVALID_FORMAT = 3002;
    public const int INVALID_VALUE = 3003;
    public const int OUT_OF_RANGE = 3004;
    // 认证授权错误 4xxx
    public const int UNAUTHORIZED = 4001;
    public const int TOKEN_EXPIRED = 4002;
    public const int TOKEN_INVALID = 4003;
    public const int FORBIDDEN = 4004;
    public const int LOGIN_FAILED = 4005;
    public const int PASSWORD_EXPIRED = 4006;

    // 资源错误 5xxx
    public const int NOT_FOUND = 5001;
    public const int ALREADY_EXISTS = 5002;
    public const int RESOURCE_LOCKED = 5003;
    public const int VERSION_CONFLICT = 5004;

    // 并发错误 6xxx
    public const int CONFLICT_ERROR = 6001;
    public const int DUPLICATE_ERROR = 6002;

    // 外部服务错误 7xxx
    public const int EXTERNAL_SERVICE_ERROR = 7001;
    public const int EXTERNAL_SERVICE_UNAVAILABLE = 7002;
    /// <summary>
    /// 获取错误描述
    /// </summary>
    public static string GetMessage(int errorCode)
    {
        return errorCode switch
        {
            SUCCESS => "操作成功",
            SYSTEM_ERROR => "系统错误",
            NETWORK_ERROR => "网络错误",
            TIMEOUT_ERROR => "请求超时",
            UNKNOWN_ERROR => "未知错误",

            BUSINESS_ERROR => "业务处理失败",
            OPERATION_FAILED => "操作失败",
            STATE_ERROR => "状态错误",

            VALIDATION_ERROR => "数据验证失败",
            REQUIRED_FIELD_MISSING => "必填字段缺失",
            INVALID_FORMAT => "格式不正确",
            INVALID_VALUE => "值不正确",
            OUT_OF_RANGE => "值超出范围",

            UNAUTHORIZED => "未授权",
            TOKEN_EXPIRED => "令牌已过期",
            TOKEN_INVALID => "令牌无效",
            FORBIDDEN => "无权访问",
            LOGIN_FAILED => "登录失败",
            PASSWORD_EXPIRED => "密码已过期",

            NOT_FOUND => "资源未找到",
            ALREADY_EXISTS => "资源已存在",
            RESOURCE_LOCKED => "资源已锁定",
            VERSION_CONFLICT => "版本冲突",

            CONFLICT_ERROR => "数据冲突",
            DUPLICATE_ERROR => "数据重复",

            EXTERNAL_SERVICE_ERROR => "外部服务错误",
            EXTERNAL_SERVICE_UNAVAILABLE => "外部服务不可用",

            _ => "未知错误"
        };
    }
}
