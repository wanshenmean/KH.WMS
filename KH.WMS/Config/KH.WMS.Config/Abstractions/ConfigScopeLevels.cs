namespace KH.WMS.Config.Abstractions;

/// <summary>
/// 配置作用域级别常量
/// <para>定义配置分层解析的标准作用域，配置层与业务方共享同一套语义。</para>
/// <para>解析优先级从高到低：DOC_TYPE → ZONE → WAREHOUSE → GLOBAL。</para>
/// </summary>
public static class ConfigScopeLevels
{
    /// <summary>全局级别，适用于所有仓库、所有单据类型的通用配置</summary>
    public const string GLOBAL = "GLOBAL";

    /// <summary>仓库级别，针对特定仓库的配置，覆盖全局配置</summary>
    public const string WAREHOUSE = "WAREHOUSE";

    /// <summary>库区级别，针对特定库区的配置，覆盖仓库级配置</summary>
    public const string ZONE = "ZONE";

    /// <summary>单据类型级别，针对特定单据类型的配置，最高优先级</summary>
    public const string DOC_TYPE = "DOC_TYPE";

    /// <summary>
    /// 解析优先级（从高到低）
    /// </summary>
    public static readonly string[] PriorityOrder = { DOC_TYPE, ZONE, WAREHOUSE, GLOBAL };
}
