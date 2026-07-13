namespace KH.WMS.Config.Abstractions;

/// <summary>
/// 全局配置服务契约
/// 提供分层配置解析能力，支持 GLOBAL / WAREHOUSE / ZONE / DOC_TYPE 四级作用域
/// ConfigModule 负责实现
/// </summary>
public interface IConfigResolverContract
{
    /// <summary>
    /// 解析配置值（基于作用域上下文，按优先级 DOC_TYPE → ZONE → WAREHOUSE → GLOBAL 逐级查找）
    /// </summary>
    Task<string> ResolveConfigValueAsync(string group, string key, ConfigScopeContext? scope = null);

    /// <summary>
    /// 解析布尔配置值（基于作用域上下文）
    /// </summary>
    Task<bool> ResolveConfigBoolAsync(string group, string key, ConfigScopeContext? scope = null);

    /// <summary>
    /// 预热：将所有 GLOBAL 级别的配置项加载到缓存
    /// 建议在应用启动时调用
    /// </summary>
    Task WarmUpAsync();

}
