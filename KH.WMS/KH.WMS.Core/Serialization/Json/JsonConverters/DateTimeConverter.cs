using System.Text.Json;
using System.Text.Json.Serialization;

namespace KH.WMS.Core.Serialization.JsonConverters;

/// <summary>
/// DateTime 转换器
/// </summary>
public class DateTimeConverter : JsonConverter<DateTime>
{
    private readonly string _format;

    public DateTimeConverter() : this("yyyy-MM-dd HH:mm:ss")
    {
    }

    public DateTimeConverter(string format)
    {
        _format = format;
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (DateTime.TryParse(reader.GetString(), out var value))
            {
                return value;
            }
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_format));
    }
}

/// <summary>
/// DateTimeOffset 转换器
/// </summary>
public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    private readonly string _format;

    public DateTimeOffsetConverter() : this("yyyy-MM-dd HH:mm:ss")
    {
    }

    public DateTimeOffsetConverter(string format)
    {
        _format = format;
    }

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (DateTimeOffset.TryParse(reader.GetString(), out var value))
            {
                return value;
            }
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_format));
    }
}

/// <summary>
/// 可空 DateTime 转换器
/// </summary>
public class NullableDateTimeConverter : JsonConverter<DateTime?>
{
    private readonly string _format;

    public NullableDateTimeConverter() : this("yyyy-MM-dd HH:mm:ss")
    {
    }

    public NullableDateTimeConverter(string format)
    {
        _format = format;
    }

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String)
        {
            if (DateTime.TryParse(reader.GetString(), out var value))
            {
                return value;
            }
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToString(_format));
        else
            writer.WriteNullValue();
    }
}
