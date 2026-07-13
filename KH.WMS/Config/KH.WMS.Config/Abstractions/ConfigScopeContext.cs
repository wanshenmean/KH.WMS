namespace KH.WMS.Config.Abstractions;

/// <summary>
/// 配置作用域上下文
/// <para>封装一次配置解析请求中的作用域信息，解耦配置解析框架与具体业务语义。</para>
/// <para>调用方按需填充已知的作用域，解析器按优先级逐级查找。</para>
/// </summary>
public sealed class ConfigScopeContext
{
    /// <summary>仓库ID（仓库级作用域）</summary>
    public long? WarehouseId { get; init; }

    /// <summary>库区ID（库区级作用域）</summary>
    public long? ZoneId { get; init; }

    /// <summary>单据类型编码（单据类型级作用域，由 IConfigScopeResolver 解析为 ID）</summary>
    public string? DocTypeCode { get; init; }

    /// <summary>创建空上下文（仅查 GLOBAL 级别）</summary>
    public static ConfigScopeContext Empty => new();

    /// <summary>创建带仓库的上下文</summary>
    public static ConfigScopeContext ForWarehouse(long warehouseId) => new() { WarehouseId = warehouseId };

    /// <summary>创建带仓库和库区的上下文</summary>
    public static ConfigScopeContext ForZone(long warehouseId, long zoneId) => new() { WarehouseId = warehouseId, ZoneId = zoneId };

    /// <summary>创建带单据类型的上下文</summary>
    public static ConfigScopeContext ForDocType(string docTypeCode) => new() { DocTypeCode = docTypeCode };
}
