using System.Text.Json;
using System.Text.Json.Serialization;

namespace KH.WMS.Core.Serialization.JsonConverters;

/// <summary>
/// Int 转换器 - 反序列化时兼容 boolean 类型（true→1, false→0）
/// 解决前端 switch/checkbox 组件传出 boolean 导致 int 字段绑定失败的问题
/// </summary>
public class BoolToIntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.True => 1,
            JsonTokenType.False => 0,
            JsonTokenType.Number => reader.GetInt32(),
            JsonTokenType.String => int.TryParse(reader.GetString(), out var v) ? v : 0,
            _ => 0,
        };
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

/// <summary>
/// 可空 Int 转换器 - 反序列化时兼容 boolean 类型
/// </summary>
public class NullableBoolToIntConverter : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.True => 1,
            JsonTokenType.False => 0,
            JsonTokenType.Number => reader.GetInt32(),
            JsonTokenType.String => int.TryParse(reader.GetString(), out var v) ? v : null,
            _ => null,
        };
    }

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteNumberValue(value.Value);
        else
            writer.WriteNullValue();
    }
}
