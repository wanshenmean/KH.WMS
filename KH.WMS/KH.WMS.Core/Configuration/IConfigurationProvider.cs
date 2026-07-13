using Microsoft.Extensions.Configuration;

namespace KH.WMS.Core.Configuration;

/// <summary>
/// 配置提供者接口
/// </summary>
public interface IConfigurationProvider
{
    /// <summary>
    /// 获取配置值
    /// </summary>
    string? GetValue(string key);

    /// <summary>
    /// 获取配置节
    /// </summary>
    IConfigurationSection GetSection(string key);

    /// <summary>
    /// 绑定到对象
    /// </summary>
    T Bind<T>(string sectionName) where T : class, new();

    /// <summary>
    /// 绑定到对象
    /// </summary>
    void Bind<T>(string sectionName, T instance) where T : class;

    /// <summary>
    /// 获取连接字符串
    /// </summary>
    string? GetConnectionString(string name);

    /// <summary>
    /// 检查配置是否存在
    /// </summary>
    bool Exists(string key);
}


