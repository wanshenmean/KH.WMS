namespace KH.WMS.Core.Serialization;

/// <summary>
/// JSON 序列化器接口
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    /// 序列化为 JSON
    /// </summary>
    string Serialize<T>(T obj, bool indented = false);

    /// <summary>
    /// 序列化为 JSON（流）
    /// </summary>
    void Serialize<T>(T obj, Stream stream, bool indented = false);

    /// <summary>
    /// 从 JSON 反序列化
    /// </summary>
    T? Deserialize<T>(string json);

    /// <summary>
    /// 从 JSON 反序列化（流）
    /// </summary>
    T? Deserialize<T>(Stream stream);

    /// <summary>
    /// 从 JSON 反序列化为指定类型
    /// </summary>
    object? Deserialize(string json, Type type);
}
