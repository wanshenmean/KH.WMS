using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KH.WMS.Core.Serialization.Json;

/// <summary>
/// JSON 配置
/// </summary>
public static class JsonSetup
{
    /// <summary>
    /// 配置 JSON 选项
    /// </summary>
    public static IServiceCollection AddJsonConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var jsonOptions = configuration.GetSection("Json").Get<JsonSerializerOptions>();
        jsonOptions ??= GetDefaultOptions();

        services.AddSingleton(jsonOptions);
        services.AddSingleton<KH.WMS.Core.Serialization.IJsonSerializer, SystemJsonSerializer>();

        return services;
    }

    /// <summary>
    /// 获取默认 JSON 选项
    /// </summary>
    public static JsonSerializerOptions GetDefaultOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters =
            {
                new JsonConverters.DateTimeConverter(),
                new JsonConverters.LongConverter(),
                new JsonConverters.EnumConverter()
            }
        };
    }
}

/// <summary>
/// 系统 JSON 序列化器实现（使用 System.Text.Json）
/// </summary>
public class SystemJsonSerializer : KH.WMS.Core.Serialization.IJsonSerializer
{
    private readonly JsonSerializerOptions _options;

    public SystemJsonSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }

    public string Serialize<T>(T obj, bool indented = false)
    {
        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;

        return JsonSerializer.Serialize(obj, options);
    }

    public void Serialize<T>(T obj, Stream stream, bool indented = false)
    {
        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;

        JsonSerializer.Serialize(stream, obj, options);
    }

    public T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, _options);
    }

    public T? Deserialize<T>(Stream stream)
    {
        return JsonSerializer.Deserialize<T>(stream, _options);
    }

    public object? Deserialize(string json, Type type)
    {
        return JsonSerializer.Deserialize(json, type, _options);
    }
}
