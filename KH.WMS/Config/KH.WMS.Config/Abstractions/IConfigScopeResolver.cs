namespace KH.WMS.Config.Abstractions;

/// <summary>
/// 配置作用域解析器抽象
/// <para>将"作用域标识"解析为"作用域ID"，解耦配置解析框架与具体业务实体。</para>
/// <para>配置层不再直接依赖 CfgDocumentType 等业务实体，由业务方提供解析实现。</para>
/// </summary>
public interface IConfigScopeResolver
{
    /// <summary>
    /// 解析作用域ID
    /// </summary>
    /// <param name="scopeLevel">作用域级别（字符串，由业务方定义常量，如 "DOC_TYPE"）</param>
    /// <param name="scopeIdentifier">作用域标识（如单据类型编码 "PURCHASE_INBOUND"）</param>
    /// <returns>作用域ID（解析失败返回 null）</returns>
    Task<long?> ResolveScopeIdAsync(string scopeLevel, string scopeIdentifier);
}
