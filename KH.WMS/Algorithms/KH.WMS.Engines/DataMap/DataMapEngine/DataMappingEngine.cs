using System.Dynamic;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SqlSugar;

namespace KH.WMS.Engines.DataMap;

/// <summary>
/// 数据映射引擎实现
/// </summary>
/// <remarks>
/// 提供灵活的数据映射功能，支持：
/// - 基于配置的字段映射
/// - 动态对象映射
/// - 批量数据映射
/// - 反向映射
/// - 脚本转换
/// - 值转换器
///
/// 使用场景：
/// - 不同系统间的数据转换
/// - API响应格式化
/// - 数据导入导出
/// - 字段重命名和类型转换
/// </remarks>
[RegisteredService(Lifetime = ServiceLifetime.Scoped)]
public class DataMappingEngine : IDataMappingEngine
{
    private readonly ISqlSugarClient _db;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db">SqlSugar数据库客户端</param>
    public DataMappingEngine(ISqlSugarClient db)
    {
        _db = db;
    }

    /// <summary>
    /// 动态映射(使用配置)
    /// </summary>
    /// <param name="source">源对象</param>
    /// <param name="mappingCode">映射配置编码</param>
    /// <returns>映射后的动态对象</returns>
    /// <exception cref="Exception">当映射配置不存在时抛出异常</exception>
    public async Task<object> MapDynamicAsync(object source, string mappingCode)
    {
        // 1. 从数据库加载映射配置
        var config = await _db.Queryable<IntDataMappingConfig>()
            .Where(x => x.MappingCode == mappingCode)
            .FirstAsync();

        if (config == null)
            throw new Exception($"映射配置不存在: {mappingCode}");

        // 2. 解析映射规则
        var rules = JsonConvert.DeserializeObject<List<MappingRule>>(config.MappingRules);
        if (rules == null || rules.Count == 0)
            return source;

        // 3. 使用 MapObject 执行映射（消除重复代码）
        var target = MapObject(source, rules);

        // 4. 执行脚本转换
        if (!string.IsNullOrEmpty(config.TransformScript))
        {
            target = await ExecuteTransformScriptAsync(target, config.TransformScript, config.ScriptType ?? DataMapConstants.ScriptLanguage.CSHARP);
        }

        return target;
    }

    public async Task<object> MapDynamicAsync(object source, long mappingId)
    {
        // 1. 从数据库加载映射配置
        var config = await _db.Queryable<IntDataMappingConfig>()
            .Where(x => x.Id == mappingId)
            .FirstAsync();

        if (config == null)
            throw new Exception($"映射配置不存在: {mappingId}");

        // 2. 解析映射规则
        var rules = JsonConvert.DeserializeObject<List<MappingRule>>(config.MappingRules);
        if (rules == null || rules.Count == 0)
            return source;

        // 3. 使用 MapObject 执行映射（消除重复代码）
        var target = MapObject(source, rules);

        // 4. 执行脚本转换
        if (!string.IsNullOrEmpty(config.TransformScript))
        {
            target = await ExecuteTransformScriptAsync(target, config.TransformScript, config.ScriptType ?? DataMapConstants.ScriptLanguage.CSHARP);
        }

        return target;
    }

    /// <summary>
    /// 动态映射到强类型对象(使用配置)
    /// </summary>
    /// <param name="source">源对象</param>
    /// <param name="mappingCode">映射配置编码</param>
    /// <returns>映射后的强类型对象</returns>
    /// <remarks>
    /// 将动态对象映射到指定的强类型，使用反射设置属性值
    /// 支持复杂类型和数组/集合类型
    /// </remarks>
    public async Task<(bool Success, string? ErrorMessage, T? Data)> MapToAsync<T>(object source, string mappingCode) where T : class, new()
    {
        try
        {
            // 1. 使用动态映射获取字典结果
            var dynamicResult = await MapDynamicAsync(source, mappingCode);

            if (dynamicResult == null)
                return (false, "映射结果为空", null);

            // 2. 将动态对象序列化为JSON（保持数组结构）
            var json = JsonConvert.SerializeObject(dynamicResult, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            // 3. 直接反序列化为目标类型（自动处理数组）
            var result = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            if (result == null)
                return (false, "反序列化失败", null);

            return (true, null, result);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    /// <summary>
    /// 映射数组（支持嵌套）
    /// </summary>
    /// <param name="sourceArray">源数组</param>
    /// <param name="rules">映射规则</param>
    /// <returns>映射后的数组</returns>
    /// <remarks>
    /// 递归处理数组元素，支持多层嵌套数组
    /// </remarks>
    private List<object> MapArray(object sourceArray, List<MappingRule> rules)
    {
        var result = new List<object>();

        var jArray = sourceArray as JToken;
        if (jArray == null || jArray.Type != JTokenType.Array)
            return result;

        // 遍历数组元素，对每个元素递归应用规则
        foreach (var item in jArray.Children())
        {
            var mappedItem = MapObject(item, rules);
            result.Add(mappedItem);
        }

        return result;
    }

    /// <summary>
    /// 映射单个对象（支持嵌套）
    /// </summary>
    /// <param name="source">源对象</param>
    /// <param name="rules">映射规则</param>
    /// <returns>映射后的对象</returns>
    /// <remarks>
    /// 应用一组规则到单个对象，支持递归处理嵌套数组和对象
    /// </remarks>
    private IDictionary<string, object> MapObject(object source, List<MappingRule> rules)
    {
        var target = new ExpandoObject() as IDictionary<string, object>;

        foreach (var rule in rules)
        {
            try
            {
                var sourceValue = GetPropertyValue(source, rule.SourceField);

                // 应用默认值
                if (sourceValue == null && !string.IsNullOrEmpty(rule.DefaultValue))
                {
                    sourceValue = rule.DefaultValue;
                }

                // 处理嵌套数组（递归）
                if (rule.IsArray && sourceValue != null && rule.Children != null && rule.Children.Count > 0)
                {
                    target[rule.TargetField] = MapArray(sourceValue, rule.Children);
                    continue;
                }

                // 应用转换器
                if (sourceValue != null && !string.IsNullOrEmpty(rule.Converter))
                {
                    sourceValue = ApplyConverter(sourceValue, rule.Converter);
                }

                if (!string.IsNullOrEmpty(rule.TargetField))
                    target[rule.TargetField] = sourceValue ?? "";
            }
            catch
            {
                target[rule.TargetField] = rule.DefaultValue ?? "";
            }
        }

        return target;
    }

    /// <summary>
    /// 批量映射
    /// </summary>
    /// <param name="sourceList">源对象列表</param>
    /// <param name="mappingCode">映射配置编码</param>
    /// <returns>映射后的对象列表</returns>
    /// <remarks>
    /// 对每个源对象应用相同的映射配置，提高批量处理效率
    /// </remarks>
    public async Task<List<object>> MapListDynamicAsync(List<object> sourceList, string mappingCode)
    {
        var results = new List<object>();

        foreach (var source in sourceList)
        {
            var mapped = await MapDynamicAsync(source, mappingCode);
            results.Add(mapped);
        }

        return results;
    }

    /// <summary>
    /// 反向映射
    /// </summary>
    /// <param name="source">源对象（原映射的目标）</param>
    /// <param name="mappingCode">映射配置编码</param>
    /// <returns>反向映射后的对象</returns>
    /// <remarks>
    /// 将已映射的数据反向转换回原始格式
    /// 通过交换源和目标字段实现反向映射
    /// </remarks>
    public async Task<object> MapReverseAsync(object source, string mappingCode)
    {
        // 反向映射 - 交换源和目标字段
        var config = await _db.Queryable<IntDataMappingConfig>()
            .Where(x => x.MappingCode == mappingCode)
            .FirstAsync();

        if (config == null)
            throw new Exception($"映射配置不存在: {mappingCode}");

        var rules = JsonConvert.DeserializeObject<List<MappingRule>>(config.MappingRules);
        if (rules == null || rules.Count == 0)
            return source;

        // 交换源和目标字段
        var reversedRules = rules.Select(r => new MappingRule
        {
            SourceField = r.TargetField,
            TargetField = r.SourceField,
            DataType = r.DataType,
            Required = r.Required,
            DefaultValue = r.DefaultValue,
            Converter = r.Converter
        }).ToList();

        var reversedConfig = new IntDataMappingConfig
        {
            MappingRules = JsonConvert.SerializeObject(reversedRules),
            TransformScript = config.TransformScript,
            ScriptType = config.ScriptType
        };

        // 执行映射
        var target = new ExpandoObject() as IDictionary<string, object>;

        foreach (var rule in reversedRules)
        {
            try
            {
                var sourceValue = GetPropertyValue(source, rule.SourceField);

                if (sourceValue != null && !string.IsNullOrEmpty(rule.Converter))
                {
                    sourceValue = ApplyConverter(sourceValue, rule.Converter);
                }

                target[rule.TargetField] = sourceValue ?? "";
            }
            catch
            {
                target[rule.TargetField] = rule.DefaultValue ?? "";
            }
        }

        if (!string.IsNullOrEmpty(reversedConfig.TransformScript))
        {
            target = await ExecuteTransformScriptAsync(target, reversedConfig.TransformScript, reversedConfig.ScriptType ?? DataMapConstants.ScriptLanguage.CSHARP);
        }

        return target;
    }

    /// <summary>
    /// 获取属性值（支持嵌套路径和数组索引）
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="path">属性路径（支持 "field", "field.subfield", "items[0].name" 等格式）</param>
    /// <returns>属性值</returns>
    /// <remarks>
    /// 支持多种对象类型：
    /// - JObject (JSON.NET)
    /// - JArray (JSON数组)
    /// - JValue (JSON值)
    /// - 普通对象（反射）
    /// - 字典对象（IDictionary）
    /// - 动态对象（ExpandoObject）
    ///
    /// 支持的路径格式：
    /// - "fieldName" - 简单字段
    /// - "user.name" - 嵌套字段
    /// - "items[0]" - 数组索引
    /// - "users[0].name" - 嵌套+数组
    /// </remarks>
    private object? GetPropertyValue(object? obj, string path)
    {
        if (obj == null || string.IsNullOrEmpty(path))
            return null;

        // 分割路径（支持点号分隔的嵌套路径）
        var parts = path.Split('.');
        var current = obj;

        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            if (string.IsNullOrEmpty(part))
                return null;

            current = GetSingleProperty(current, part);

            if (current == null)
                return null;

            // 如果是最后一个部分，返回值
            if (i == parts.Length - 1)
            {
                return ConvertJTokenToValue(current);
            }
        }

        return null;
    }

    /// <summary>
    /// 获取单个属性值（不支持嵌套路径）
    /// </summary>
    private object? GetSingleProperty(object? obj, string propertyName)
    {
        if (obj == null || string.IsNullOrEmpty(propertyName))
            return null;

        // 处理数组索引，如 items[0] 或 items[0].name
        var arrayMatch = System.Text.RegularExpressions.Regex.Match(propertyName, @"^(.+)\[(\d+)\]$");
        if (arrayMatch.Success)
        {
            var arrayName = arrayMatch.Groups[1].Value;
            var index = int.Parse(arrayMatch.Groups[2].Value);

            // 先获取数组对象
            var arrayObj = GetSingleProperty(obj, arrayName);
            if (arrayObj == null)
                return null;

            // 处理 JArray
            if (arrayObj is JArray jArray)
            {
                if (index >= jArray.Count)
                    return null;
                return jArray[index];
            }

            // 处理普通数组
            if (arrayObj is Array array)
            {
                if (index >= array.Length)
                    return null;
                return array.GetValue(index);
            }

            // 处理 IList
            if (arrayObj is System.Collections.IList list)
            {
                if (index >= list.Count)
                    return null;
                return list[index];
            }

            return null;
        }

        // 1. 处理 JObject (JSON.NET)
        if (obj is JObject jObject)
        {
            var jItem = jObject[propertyName];
            return jItem; // 返回JToken，由调用者决定是否转换
        }

        // 2. 处理 JToken (其他JSON类型)
        if (obj is JToken jToken)
        {
            var childToken = jToken[propertyName];
            return childToken;
        }

        // 3. 处理字典对象
        if (obj is IDictionary<string, object> dict)
        {
            return dict.TryGetValue(propertyName, out var value) ? value : null;
        }

        // 4. 处理普通对象（反射）
        var type = obj.GetType();
        var property = type.GetProperty(propertyName);
        if (property != null)
        {
            return property.GetValue(obj);
        }

        // 5. 尝试从字段获取
        var field = type.GetField(propertyName);
        if (field != null)
        {
            return field.GetValue(obj);
        }

        return null;
    }

    /// <summary>
    /// 将JToken转换为实际值
    /// </summary>
    /// <remarks>
    /// 对于数组(Aray)和对象(Object)，保持为JToken，让序列化器正确处理
    /// 对于简单类型，提取实际值
    /// </remarks>
    private object? ConvertJTokenToValue(object? obj)
    {
        if (obj == null)
            return null;

        if (obj is JToken jToken)
        {
            return jToken.Type switch
            {
                JTokenType.String => jToken.Value<string>(),
                JTokenType.Integer => jToken.Value<int>(),
                JTokenType.Float => jToken.Value<double>(),
                JTokenType.Boolean => jToken.Value<bool>(),
                JTokenType.Null => null,
                JTokenType.Array => jToken,  // 保持为JToken，不转字符串
                JTokenType.Object => jToken, // 保持为JToken，不转字符串
                JTokenType.Date => jToken.Value<DateTime>(),
                JTokenType.Guid => jToken.Value<Guid>(),
                JTokenType.Uri => jToken.Value<Uri>(),
                JTokenType.TimeSpan => jToken.Value<TimeSpan>(),
                JTokenType.Bytes => jToken.Value<byte[]>(),
                _ => jToken.ToString()
            };
        }

        return obj;
    }

    /// <summary>
    /// 应用转换器
    /// </summary>
    /// <param name="value">源值</param>
    /// <param name="converter">转换器名称</param>
    /// <returns>转换后的值</returns>
    /// <remarks>
    /// 内置转换器：
    /// - TOUPPER: 转换为大写
    /// - TOLOWER: 转换为小写
    /// - TRIM: 去除首尾空格
    /// - TOSTRING: 转换为字符串
    /// - TOINT: 转换为整数
    /// - TODECIMAL: 转换为小数
    /// - TODATETIME: 转换为日期时间
    /// </remarks>
    private object? ApplyConverter(object value, string converter)
    {
        if (value == null) return null;

        return converter.ToUpper() switch
        {
            "TOUPPER" => value.ToString()?.ToUpper(),
            "TOLOWER" => value.ToString()?.ToLower(),
            "TRIM" => value.ToString()?.Trim(),
            "TOSTRING" => value.ToString(),
            "TOINT" => Convert.ToInt32(value),
            "TODECIMAL" => Convert.ToDecimal(value),
            "TODATETIME" => Convert.ToDateTime(value),
            _ => value
        };
    }

    /// <summary>
    /// 执行转换脚本
    /// </summary>
    /// <param name="data">数据字典</param>
    /// <param name="script">C# 脚本内容</param>
    /// <param name="scriptType">脚本类型（固定为 CSHARP）</param>
    /// <returns>转换后的数据</returns>
    /// <remarks>
    /// 支持 C# 脚本类型：
    /// - CSHARP: C#脚本 (使用 Roslyn 编译和执行)
    ///
    /// 脚本可用的全局变量：
    /// - data: IDictionary<string, object> 类型，包含当前要转换的数据
    /// - db: ISqlSugarClient 类型，数据库客户端
    /// - log: Action<string> 类型，日志输出函数
    ///
    /// 脚本返回值：
    /// - 必须返回 IDictionary<string, object> 类型的数据
    /// </remarks>
    private async Task<IDictionary<string, object>> ExecuteTransformScriptAsync(
        IDictionary<string, object> data, string script, string scriptType)
    {
        if (string.IsNullOrWhiteSpace(script))
            return data;

        try
        {
            // 目前只支持 C# 脚本
            if (scriptType.ToUpper() != DataMapConstants.ScriptLanguage.CSHARP)
            {
                throw new NotSupportedException($"不支持的脚本类型: {scriptType}，目前只支持 CSHARP");
            }

            return await ExecuteCSharpScriptAsync(data, script);
        }
        catch (Exception ex)
        {
            // 记录错误但返回原始数据
            Console.WriteLine($"脚本执行失败: {ex.Message}");
            return data;
        }
    }

    /// <summary>
    /// 执行 C# 脚本
    /// </summary>
    /// <param name="data">数据字典</param>
    /// <param name="script">C# 脚本内容</param>
    /// <returns>转换后的数据</returns>
    /// <remarks>
    /// C# 脚本示例：
    /// <code>
    /// // 添加计算字段
    /// if (data.ContainsKey("quantity") && data.ContainsKey("unitPrice"))
    /// {
    ///     var qty = Convert.ToDecimal(data["quantity"]);
    ///     var price = Convert.ToDecimal(data["unitPrice"]);
    ///     data["totalAmount"] = qty * price;
    /// }
    ///
    /// // 字段格式化
    /// if (data.ContainsKey("materialCode"))
    /// {
    ///     data["materialCode"] = data["materialCode"].ToString()?.ToUpper();
    /// }
    ///
    /// // 条件逻辑
    /// if (data.ContainsKey("quantity"))
    /// {
    ///     var qty = Convert.ToDecimal(data["quantity"]);
    ///     data["priority"] = qty > 1000 ? "HIGH" : "NORMAL";
    /// }
    ///
    /// // 添加时间戳
    /// data["transformTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    ///
    /// return data;
    /// </code>
    /// </remarks>
    private async Task<IDictionary<string, object>> ExecuteCSharpScriptAsync(
        IDictionary<string, object> data, string script)
    {
        try
        {
            // 创建脚本选项，添加必要的引用和命名空间。
            // 安全：刻意不引入 SqlSugar 程序集/命名空间，且 ScriptGlobals 不再注入 ISqlSugarClient，
            // 阻断脚本直接读写/删除任意数据库表（含用户、密码哈希等敏感数据）。
            var scriptOptions = ScriptOptions.Default
                .AddReferences(
                    typeof(Enumerable).Assembly,
                    typeof(List<>).Assembly,
                    typeof(IDictionary<string, object>).Assembly
                )
                .AddImports(
                    "System",
                    "System.Linq",
                    "System.Collections.Generic",
                    "System.Dynamic"
                );

            // 创建日志委托
            Action<string> log = (message) => Console.WriteLine($"[C# Script] {message}");

            // 编译并执行脚本
            var scriptState = await CSharpScript.RunAsync<IDictionary<string, object>>(
                script,
                scriptOptions,
                globals: new ScriptGlobals { Data = data, Log = log }
            );

            return scriptState.ReturnValue ?? data;
        }
        catch (CompilationErrorException ex)
        {
            throw new Exception($"C# 脚本编译错误: {string.Join("\n", ex.Diagnostics)}");
        }
        catch (Exception ex)
        {
            throw new Exception($"C# 脚本执行错误: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// C# 脚本全局变量类。
    /// 安全：不再暴露 Db（ISqlSugarClient），脚本只能基于入参 Data 做转换。
    /// </summary>
    private class ScriptGlobals
    {
        public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public Action<string>? Log { get; set; }
    }
}
