namespace KH.WMS.Core.Constants;

/// <summary>
/// 缓存键常量
/// </summary>
public static class CacheConstants
{
    /// <summary>
    /// 缓存键前缀
    /// </summary>
    public const string KEY_PREFIX = "App:";

    /// <summary>
    /// 用户缓存键
    /// </summary>
    public static class User
    {
        public const string PREFIX = KEY_PREFIX + "User:";
        public const string INFO = PREFIX + "Info:";
        public const string PERMISSIONS = PREFIX + "Permissions:";
        public const string ROLES = PREFIX + "Roles:";
        public const string MENUS = PREFIX + "Menus:";

        public static string GetUserInfoKey(long userId) => INFO + userId;
        public static string GetUserPermissionsKey(long userId) => PERMISSIONS + userId;
        public static string GetUserRolesKey(long userId) => ROLES + userId;
        public static string GetUserMenusKey(long userId) => MENUS + userId;
    }

    /// <summary>
    /// 系统配置缓存键
    /// </summary>
    public static class Config
    {
        public const string PREFIX = KEY_PREFIX + "Config:";
        public const string SYSTEM = PREFIX + "System";
        public const string DICT = PREFIX + "Dict:";
        public const string SETTING = PREFIX + "Setting:";

        public static string GetDictKey(string dictType) => DICT + dictType;
        public static string GetSettingKey(string settingKey) => SETTING + settingKey;
    }

    /// <summary>
    /// 数据缓存键
    /// </summary>
    public static class Data
    {
        public const string PREFIX = KEY_PREFIX + "Data:";
        public const string ENTITY = PREFIX + "Entity:";
        public const string LIST = PREFIX + "List:";

        public static string GetEntityKey<T>(long id) => ENTITY + typeof(T).Name + ":" + id;
        public static string GetListKey<T>() => LIST + typeof(T).Name;
    }

    /// <summary>
    /// 令牌缓存键
    /// </summary>
    public static class Token
    {
        public const string PREFIX = KEY_PREFIX + "Token:";
        public const string ACCESS = PREFIX + "Access:";
        public const string REFRESH = PREFIX + "Refresh:";
        public const string BLACKLIST = PREFIX + "Blacklist:";

        public static string GetAccessTokenKey(string token) => ACCESS + token;
        public static string GetRefreshTokenKey(string token) => REFRESH + token;
        public static string GetBlacklistKey(string token) => BLACKLIST + token;
    }

    /// <summary>
    /// 限流缓存键
    /// </summary>
    public static class RateLimit
    {
        public const string PREFIX = KEY_PREFIX + "RateLimit:";
        public const string IP = PREFIX + "IP:";
        public const string USER = PREFIX + "User:";
        public const string API = PREFIX + "API:";

        public static string GetIpKey(string ip) => IP + ip;
        public static string GetUserKey(long userId) => USER + userId;
        public static string GetApiKey(string apiPath) => API + apiPath;
    }

    /// <summary>
    /// 验证码缓存键
    /// </summary>
    public static class Captcha
    {
        public const string PREFIX = KEY_PREFIX + "Captcha:";
        public const string SMS = PREFIX + "SMS:";
        public const string EMAIL = PREFIX + "Email:";

        public static string GetSmsKey(string phone) => SMS + phone;
        public static string GetEmailKey(string email) => EMAIL + email;
    }

    /// <summary>
    /// 系统参数缓存键（启动时从 sys_parameter 表加载，带默认值兜底）
    /// </summary>
    public static class SysParam
    {
        public const string PREFIX = KEY_PREFIX + "SysParam:";

        /// <summary>
        /// 获取缓存键
        /// </summary>
        public static string GetKey(string paramCode) => PREFIX + paramCode;

        /// <summary>
        /// 系统参数默认值（数据丢失或误删时的兜底，key 为 ParamCode）
        /// </summary>
        public static readonly Dictionary<string, string> Defaults = new()
        {
            { SysParamConstants.DEFAULT_PASSWORD, SysParamConstants.DEFAULT_PASSWORD_VALUE },
            { SysParamConstants.ALLOW_MULTI_LOGIN, SysParamConstants.MULTI_LOGIN_ENABLED },
            { SysParamConstants.TOKEN_EXPIRE_MIN, SysParamConstants.TOKEN_EXPIRE_MIN_DEFAULT },
            { SysParamConstants.LOCK_ON_INVENTORY, "1" },
            { SysParamConstants.LOG_RETAIN_DAYS, "90" },
        };
    }

    /// <summary>
    /// 文件缓存键
    /// </summary>
    public static class File
    {
        public const string PREFIX = KEY_PREFIX + "File:";
        public const string INFO = PREFIX + "Info:";

        public static string GetFileInfoKey(string fileId) => INFO + fileId;
    }
}
