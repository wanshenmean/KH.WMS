using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Core.Configuration;

/// <summary>
/// 配置提供者实现
/// </summary>
public class ConfigurationProvider : IConfigurationProvider
{
    private readonly IConfiguration _configuration;

    public ConfigurationProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? GetValue(string key)
    {
        return _configuration.GetValue<string>(key);
    }

    public IConfigurationSection GetSection(string key)
    {
        return _configuration.GetSection(key);
    }

    public T Bind<T>(string sectionName) where T : class, new()
    {
        var section = _configuration.GetSection(sectionName);
        var options = new T();
        section.Bind(options);
        return options;
    }

    public void Bind<T>(string sectionName, T instance) where T : class
    {
        var section = _configuration.GetSection(sectionName);
        section.Bind(instance);
    }

    public string? GetConnectionString(string name)
    {
        return _configuration.GetConnectionString(name);
    }

    public bool Exists(string key)
    {
        return _configuration.GetChildren().Any(x => x.Key == key);
    }
}
