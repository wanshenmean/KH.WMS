
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KH.WMS.Core.Serialization.Json;

/// <summary>
/// Newtonsoft.Json 配置
/// </summary>
public static class NewtonsoftSettings
{
    /// <summary>
    /// 配置 Newtonsoft.Json 设置
    /// </summary>
    public static JsonSerializerSettings GetSettings()
    {
        return new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include,
            DateFormatString = "yyyy-MM-dd HH:mm:ss",
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new Newtonsoft.Json.Converters.StringEnumConverter()
            }
        };
    }

    /// <summary>
    /// 配置紧凑设置（生产环境）
    /// </summary>
    public static JsonSerializerSettings GetCompactSettings()
    {
        return new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new Newtonsoft.Json.Converters.StringEnumConverter()
            }
        };
    }
}

/// <summary>
/// Newtonsoft.Json 序列化器实现
/// </summary>
public class NewtonsoftJsonSerializer : KH.WMS.Core.Serialization.IJsonSerializer
{
    private readonly JsonSerializerSettings _settings;

    public NewtonsoftJsonSerializer(JsonSerializerSettings settings)
    {
        _settings = settings;
    }

    public string Serialize<T>(T obj, bool indented = false)
    {
        var settings = indented ? _settings : NewtonsoftSettings.GetCompactSettings();
        return JsonConvert.SerializeObject(obj, settings);
    }

    public void Serialize<T>(T obj, Stream stream, bool indented = false)
    {
        var settings = indented ? _settings : NewtonsoftSettings.GetCompactSettings();
        var json = JsonConvert.SerializeObject(obj, settings);
        using var writer = new StreamWriter(stream);
        writer.Write(json);
    }

    public T? Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, _settings);
    }

    public T? Deserialize<T>(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        return JsonConvert.DeserializeObject<T>(json, _settings);
    }

    public object? Deserialize(string json, Type type)
    {
        return JsonConvert.DeserializeObject(json, type, _settings);
    }
}
