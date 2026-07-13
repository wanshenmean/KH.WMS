using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Caching;
using KH.WMS.Core.Constants;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Config.Contracts;

/// <summary>
/// 通用扩展字段契约实现
/// 用于非单据类实体（库存操作、基础数据等）的扩展字段配置
/// 数据源：cfg_ext_field_type + cfg_ext_field
/// </summary>
[RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(ICfgExtFieldContract))]
public class CfgExtFieldContract(
    IRepository<CfgExtFieldType, long> extFieldTypeRepository,
    IRepository<CfgExtField, long> extFieldRepository,
    Core.Caching.ICacheService cache) : ICfgExtFieldContract
{
    private const string CACHE_PREFIX = CacheConstants.Data.PREFIX;

    /// <inheritdoc />
    public async Task<List<CfgExtField>> GetFieldsAsync(string entityCode, string fieldLevel = "HEADER")
    {
        var cacheKey = $"{CACHE_PREFIX}{entityCode}:{fieldLevel}";
        if (cache.TryGet<List<CfgExtField>>(cacheKey, out var cached))
            return cached!;

        var entityType = await extFieldTypeRepository.GetFirstOrDefaultAsync(e => e.EntityCode == entityCode);
        if (entityType == null) return [];

        var fields = await extFieldRepository.GetListAsync(
            f => f.EntityTypeId == entityType.Id && f.FieldLevel == fieldLevel);

        cache.Set(cacheKey, fields.OrderBy(f => f.SortOrder).ToList(), TimeSpan.FromHours(2));
        return fields.OrderBy(f => f.SortOrder).ToList();
    }

    /// <inheritdoc />
    public List<Dictionary<string, object?>> BuildFormColumns(List<CfgExtField> fields)
    {
        var result = new List<Dictionary<string, object?>>();
        foreach (var field in fields)
        {
            var column = new Dictionary<string, object?>
            {
                ["prop"] = field.FieldKey,
                ["label"] = field.FieldName,
                ["type"] = MapFieldType(field.FieldType),
                ["isExt"] = true,
            };

            if (field.IsRequired == BoolFlag.YES)
                column["required"] = true;

            if (!string.IsNullOrEmpty(field.DefaultValue))
                column["defaultValue"] = field.DefaultValue;

            if (field.FieldType == "DECIMAL")
            {
                column["precision"] = 2;
                column["min"] = 0;
            }

            result.Add(column);
        }
        return result;
    }

    /// <inheritdoc />
    public string? SerializeExtData(Dictionary<string, object?> allData, List<CfgExtField> fields)
    {
        var extDict = new Dictionary<string, object?>();
        foreach (var field in fields)
        {
            if (allData.TryGetValue(field.FieldKey, out var value) && value != null)
            {
                extDict[field.FieldKey] = value;
            }
        }
        return extDict.Count > 0
            ? System.Text.Json.JsonSerializer.Serialize(extDict)
            : null;
    }

    /// <inheritdoc />
    public Dictionary<string, object?> DeserializeExtData(string? extDataJson)
    {
        if (string.IsNullOrEmpty(extDataJson)) return [];
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(extDataJson);
            var result = new Dictionary<string, object?>();
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                result[prop.Name] = ExtractValue(prop.Value);
            }
            return result;
        }
        catch
        {
            return [];
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, object?>> DeserializeProcessableExtDataAsync(string entityCode, string? extDataJson, string fieldLevel = "HEADER")
    {
        var allData = DeserializeExtData(extDataJson);
        if (allData.Count == 0) return allData;

        var fields = await GetFieldsAsync(entityCode, fieldLevel);
        var processableKeys = fields
            .Where(f => f.IsProcessable == BoolFlag.YES)
            .Select(f => f.FieldKey)
            .ToHashSet();

        return allData
            .Where(kv => processableKeys.Contains(kv.Key))
            .ToDictionary();
    }

    /// <inheritdoc />
    public void ClearCache(string entityCode)
    {
        cache.Remove($"{CACHE_PREFIX}{entityCode}:HEADER");
        cache.Remove($"{CACHE_PREFIX}{entityCode}:LINE");
    }

    private static object? ExtractValue(System.Text.Json.JsonElement element)
    {
        return element.ValueKind switch
        {
            System.Text.Json.JsonValueKind.String => element.GetString(),
            System.Text.Json.JsonValueKind.Number => element.TryGetInt64(out var l) ? (object)l : element.GetDecimal(),
            System.Text.Json.JsonValueKind.True => true,
            System.Text.Json.JsonValueKind.False => false,
            System.Text.Json.JsonValueKind.Null => null,
            _ => element.GetRawText(),
        };
    }

    private static string MapFieldType(string fieldType) => fieldType switch
    {
        "STRING" => "input",
        "INT" => "number",
        "DECIMAL" => "number",
        "DATETIME" => "date",
        "BOOLEAN" => "switch",
        _ => "input",
    };
}
