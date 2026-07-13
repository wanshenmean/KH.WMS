namespace KH.WMS.Core.Constants;

/// <summary>
/// 错误消息常量
/// </summary>
public static class ErrorConstants
{
    /// <summary>
    /// 通用错误
    /// </summary>
    public static class Common
    {
        public const string OPERATION_FAILED = "操作失败";
        public const string SYSTEM_ERROR = "系统错误";
        public const string NETWORK_ERROR = "网络错误";
        public const string TIMEOUT_ERROR = "请求超时";
        public const string UNKNOWN_ERROR = "未知错误";
    }

    /// <summary>
    /// 验证错误
    /// </summary>
    public static class Validation
    {
        public const string REQUIRED_FIELD_MISSING = "必填字段缺失";
        public const string INVALID_FORMAT = "格式不正确";
        public const string INVALID_VALUE = "值不正确";
        public const string OUT_OF_RANGE = "值超出范围";
        public const string DUPLICATE_VALUE = "值已存在";
    }

    /// <summary>
    /// 认证授权错误
    /// </summary>
    public static class Authentication
    {
        public const string UNAUTHORIZED = "未授权访问";
        public const string AUTHENTICATION_FAILED = "认证失败";
        public const string LOGIN_FAILED = "登录失败";
        public const string INVALID_CREDENTIALS = "用户名或密码错误";
        public const string TOKEN_EXPIRED = "登录已过期";
        public const string TOKEN_INVALID = "令牌无效";
        public const string PASSWORD_EXPIRED = "密码已过期";
        public const string ACCOUNT_LOCKED = "账户已锁定";
        public const string ACCOUNT_DISABLED = "账户已禁用";
        public const string ACCOUNT_KICKED = "账号已在其他设备登录，请重新登录";
    }

    /// <summary>
    /// 权限错误
    /// </summary>
    public static class Authorization
    {
        public const string FORBIDDEN = "无权限访问";
        public const string INSUFFICIENT_PERMISSIONS = "权限不足";
        public const string ROLE_REQUIRED = "需要指定角色";
    }

    /// <summary>
    /// 资源错误
    /// </summary>
    public static class Resource
    {
        public const string NOT_FOUND = "资源未找到";
        public const string ALREADY_EXISTS = "资源已存在";
        public const string RESOURCE_LOCKED = "资源已锁定";
        public const string VERSION_CONFLICT = "版本冲突";
        public const string RESOURCE_DELETED = "资源已删除";
    }

    /// <summary>
    /// 数据错误
    /// </summary>
    public static class Data
    {
        public const string CONFLICT_ERROR = "数据冲突";
        public const string DUPLICATE_ERROR = "数据重复";
        public const string REFERENCE_ERROR = "引用数据不存在";
        public const string CONSTRAINT_ERROR = "约束违反";
    }

    /// <summary>
    /// 业务错误
    /// </summary>
    public static class Business
    {
        public const string OPERATION_NOT_ALLOWED = "不允许的操作";
        public const string INVALID_STATE = "状态不正确";
        public const string BUSINESS_RULE_VIOLATION = "违反业务规则";
    }

    /// <summary>
    /// 文件错误
    /// </summary>
    public static class File
    {
        public const string FILE_NOT_FOUND = "文件不存在";
        public const string FILE_TYPE_NOT_ALLOWED = "不允许的文件类型";
        public const string FILE_SIZE_EXCEEDED = "文件大小超限";
        public const string FILE_UPLOAD_FAILED = "文件上传失败";
        public const string FILE_DOWNLOAD_FAILED = "文件下载失败";
    }
}
