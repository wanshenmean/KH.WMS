namespace KH.WMS.Core.Constants;

/// <summary>
/// 系统参数常量（对应 sys_parameter 表的 ParamCode）
/// </summary>
public static class SysParamConstants
{
    /// <summary>
    /// 默认密码
    /// </summary>
    public const string DEFAULT_PASSWORD = "SYS_DEFAULT_PASSWORD";

    /// <summary>
    /// 默认密码值
    /// </summary>
    public const string DEFAULT_PASSWORD_VALUE = "123456";

    /// <summary>
    /// 是否允许多设备同时登录（1=允许 0=不允许）
    /// </summary>
    public const string ALLOW_MULTI_LOGIN = "SYS_ALLOW_MULTI_LOGIN";

    /// <summary>
    /// 是否允许多设备登录的"允许"值
    /// </summary>
    public const string MULTI_LOGIN_ENABLED = "1";

    /// <summary>
    /// Token 过期时间（分钟）
    /// </summary>
    public const string TOKEN_EXPIRE_MIN = "SYS_TOKEN_EXPIRE_MIN";

    /// <summary>
    /// Token 过期时间默认值（分钟）
    /// </summary>
    public const string TOKEN_EXPIRE_MIN_DEFAULT = "30";

    /// <summary>
    /// 库存锁定策略（1=锁定 0=不锁定）
    /// </summary>
    public const string LOCK_ON_INVENTORY = "WH_LOCK_ON_INVENTORY";

    /// <summary>
    /// 日志保留天数
    /// </summary>
    public const string LOG_RETAIN_DAYS = "LOG_RETAIN_DAYS";

    /// <summary>
    /// 所有系统内置参数定义（ParamCode → DefaultValue）
    /// </summary>
    public static readonly Dictionary<string, string> RequiredParams = new()
    {
        { DEFAULT_PASSWORD, DEFAULT_PASSWORD_VALUE },
        { ALLOW_MULTI_LOGIN, MULTI_LOGIN_ENABLED },
        { TOKEN_EXPIRE_MIN, TOKEN_EXPIRE_MIN_DEFAULT },
        { LOCK_ON_INVENTORY, "1" },
        { LOG_RETAIN_DAYS, "90" },
    };
}
