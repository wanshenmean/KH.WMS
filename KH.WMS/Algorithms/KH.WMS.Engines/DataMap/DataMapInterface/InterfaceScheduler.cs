using System.Collections.Concurrent;
using System.Dynamic;
using System.Globalization;
using System.Text.RegularExpressions;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SqlSugar;
using ICacheService = KH.WMS.Core.Caching.ICacheService;

namespace KH.WMS.Engines.DataMap;

/// <summary>
/// 接口调度服务 - 用于处理入站接口的自动接收和处理
/// </summary>
[SelfRegisteredService(Lifetime = ServiceLifetime.Scoped, WithoutInterceptor = true)]
public class InterfaceScheduler
{
    private const string CACHE_KEY_PREFIX = "InterfaceConfig:";
    private const string CACHE_KEY_ALL = "InterfaceConfig:All";

    private readonly ISqlSugarClient _db;
    private readonly ILogger<InterfaceScheduler> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly BusinessProcessorFactory _processorFactory;
    private readonly ICacheService _cache;
    private readonly IDataMappingEngine _mappingEngine;

    public InterfaceScheduler(
        ISqlSugarClient db,
        ILogger<InterfaceScheduler> logger,
        IServiceProvider serviceProvider,
        BusinessProcessorFactory processorFactory,
        ICacheService cache,
        IDataMappingEngine mappingEngine)
    {
        _db = db;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _processorFactory = processorFactory;
        _cache = cache;
        _mappingEngine = mappingEngine;
    }

    /// <summary>
    /// 加载所有启用的入站接口配置
    /// </summary>
    public async Task LoadEnabledInboundInterfacesAsync()
    {
        try
        {
            var interfaces = await _db.Queryable<IntInterfaceConfig>()
                .Where(x => x.Status == DataMapConstants.InterfaceStatus.ACTIVE)
                .Where(x => x.Direction == DataMapConstants.InterfaceDirection.INBOUND || x.Direction == DataMapConstants.InterfaceDirection.BIDIRECTIONAL)
                .ToListAsync();

            // 清除旧缓存（包括列表缓存）
            _cache.Remove(CACHE_KEY_ALL);
            _cache.Remove(CACHE_KEY_PREFIX);

            // 只缓存所有接口列表（使用滑动过期，24小时有访问则自动延长）
            _cache.Set(CACHE_KEY_ALL, interfaces, TimeSpan.FromHours(24));

            // 打印加载的接口详情
            Console.WriteLine($"========== 成功加载 {interfaces.Count} 个入站接口配置 ==========");
            foreach (var iface in interfaces)
            {
                Console.WriteLine($"  [{iface.InterfaceCode}] {iface.InterfaceName}");
                Console.WriteLine($"    - 系统: {iface.SystemCode}");
                Console.WriteLine($"    - 方向: {iface.Direction}");
                Console.WriteLine($"    - 类型: {iface.InterfaceType}");
                Console.WriteLine($"    - 路径: {iface.RequestPath}");
                Console.WriteLine($"    - 方法: {iface.HttpMethod}");
                Console.WriteLine($"    - 触发: {iface.TriggerType}");
                Console.WriteLine($"    - 处理器: {iface.ProcessorType ?? "未配置"}");
                Console.WriteLine($"    - 超时: {iface.Timeout}ms, 重试: {iface.RetryCount}次");
                Console.WriteLine($"    - 状态: {iface.Status}");
            }
            Console.WriteLine($"==============================================");

            _logger.LogInformation($"成功加载 {interfaces.Count} 个入站接口配置到缓存");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载入站接口配置失败");
        }
    }

    /// <summary>
    /// 处理入站接口请求
    /// </summary>
    public async Task<(bool Success, string Message, object? Data)> ProcessInboundRequestAsync(
        string interfaceCode,
        HttpRequest request,
        CancellationToken cancellationToken = default)
    {
        // 从缓存获取所有接口配置
        var interfaces = _cache.Get<List<IntInterfaceConfig>>(CACHE_KEY_ALL);

        // 从列表中查找指定接口
        IntInterfaceConfig? config = null;
        if (interfaces != null)
        {
            // todo 优化：如果接口数量较多，可以考虑在缓存中单独存储一个字典，key为InterfaceCode，value为接口配置，这样查询效率更高
            // todo 优化：如果接口配置变更频繁，考虑在接口配置表中增加一个版本号字段，每次变更时更新版本号，这样在缓存中存储一个版本号，当请求过来时先检查版本号是否匹配，如果不匹配则说明缓存过期需要重新加载，这样可以避免每次请求都查询数据库
            // todo 优化：接口的数据映射要从WmsDataMappingConfigz表中查询，不是使用接口的RequestTemplate字段，RequestTemplate字段可以去掉，接口配置表只负责接口的基本信息和调用配置，数据映射和转换脚本都放在数据映射表中，这样职责更清晰，接口配置表也更简洁
            config = interfaces.FirstOrDefault(x => x.InterfaceCode == interfaceCode);
        }

        // 缓存不存在时的降级处理
        if (config == null)
        {
            _logger.LogWarning($"缓存中未找到接口配置 {interfaceCode}，尝试从数据库加载");

            // 从数据库查询
            config = await _db.Queryable<IntInterfaceConfig>()
                .Where(x => x.Status == DataMapConstants.InterfaceStatus.ACTIVE)
                .Where(x => x.InterfaceCode == interfaceCode)
                .FirstAsync();

            if (config != null)
            {
                // 重新加载整个缓存（因为单个接口变更了）
                await LoadEnabledInboundInterfacesAsync();
                _logger.LogInformation($"已从数据库重新加载接口配置 {interfaceCode} 并刷新缓存");
            }
            else
            {
                return (false, $"接口 {interfaceCode} 不存在或未启用", null);
            }
        }

        // 验证HTTP方法
        var requestMethod = request.Method.ToUpper();
        if (!string.IsNullOrEmpty(config.HttpMethod) && config.HttpMethod.ToUpper() != requestMethod)
        {
            return (false, $"HTTP方法不匹配，期望: {config.HttpMethod}, 实际: {requestMethod}", null);
        }

        // 记录开始时间
        var startTime = DateTime.Now;
        var traceId = Guid.NewGuid().ToString("N");

        try
        {
            // 读取请求数据
            string requestData;
            using (var reader = new StreamReader(request.Body, leaveOpen: true))
            {
                requestData = await reader.ReadToEndAsync(cancellationToken);
            }

            // 记录调用日志
            var callLog = new IntInterfaceLog
            {
                TraceId = traceId,
                SystemCode = config.SystemCode,
                InterfaceCode = interfaceCode,
                Direction = config.Direction,
                RequestData = requestData,
                RequestHeaders = JsonConvert.SerializeObject(GetRequestHeaders(request)),
                RequestTime = startTime,
                Status = DataMapConstants.CallLogStatus.SUCCESS,
                ExecuteMilliseconds = 0,
                CreatedTime = DateTime.Now,
            };

            // 步骤1: 验证请求数据
            //var validationResult = ValidateRequestData(requestData, config.RequestTemplate);
            //if (!validationResult.IsValid)
            //{
            //    callLog.Status = "FAIL";
            //    callLog.ErrorMessage = validationResult.ErrorMessage;
            //    await SaveCallLogAsync(callLog);
            //    return (false, $"请求数据验证失败: {validationResult.ErrorMessage}", null);
            //}

            // 步骤2: 数据映射

            var processedData = await _mappingEngine.MapDynamicAsync(JsonConvert.DeserializeObject<object>(requestData), config.MappingConfigId.GetValueOrDefault());

            // 步骤3: 调用业务处理逻辑
            var businessResult = await ExecuteBusinessLogicAsync(JsonConvert.SerializeObject(processedData), config);
            if (!businessResult.Success)
            {
                callLog.Status = DataMapConstants.CallLogStatus.FAIL;
                callLog.ErrorMessage = businessResult.ErrorMessage;
                await SaveCallLogAsync(callLog);
                return (false, businessResult.ErrorMessage ?? "业务处理失败", null);
            }

            // 步骤4: 构建响应数据
            var responseData = BuildResponseData(businessResult.Data, config.ResponseTemplate);

            // 更新调用日志
            callLog.ResponseData = responseData;
            callLog.ResponseTime = DateTime.Now;
            callLog.ExecuteMilliseconds = (int)(DateTime.Now - startTime).TotalMilliseconds;
            callLog.Status = DataMapConstants.CallLogStatus.SUCCESS;
            await SaveCallLogAsync(callLog);

            // 异步接口则异步返回
            if (config.InterfaceType == DataMapConstants.InterfaceType.ASYNC)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ProcessAsyncInterfaceAsync(config, JsonConvert.SerializeObject(processedData), traceId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"异步接口 {interfaceCode} 处理失败");
                    }
                }, cancellationToken);
            }

            _logger.LogInformation($"入站接口 {interfaceCode} 处理成功，耗时: {callLog.ExecuteMilliseconds}ms");

            return (true, "接收成功", new { data = businessResult.Data, traceId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"入站接口 {interfaceCode} 处理异常");

            // 记录失败日志
            await SaveCallLogAsync(new IntInterfaceLog
            {
                TraceId = traceId,
                SystemCode = config.SystemCode,
                InterfaceCode = interfaceCode,
                Direction = config.Direction,
                RequestTime = startTime,
                ResponseTime = DateTime.Now,
                Status = DataMapConstants.CallLogStatus.FAIL,
                ErrorMessage = ex.Message,
                ExecuteMilliseconds = (int)(DateTime.Now - startTime).TotalMilliseconds,
                CreatedTime = DateTime.Now,
            });

            return (false, $"处理异常: {ex.Message}", null);
        }
    }

    /// <summary>
    /// 验证请求数据
    /// </summary>
    private (bool IsValid, string? ErrorMessage) ValidateRequestData(string requestData, string? requestTemplate)
    {
        try
        {
            if (string.IsNullOrEmpty(requestTemplate))
                return (true, null);

            var requestJson = JToken.Parse(requestData);
            var templateJson = JToken.Parse(requestTemplate);

            // 递归验证JSON结构
            return ValidateJsonStructure(requestJson, templateJson);
        }
        catch (JsonException ex)
        {
            return (false, $"JSON格式错误: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"验证失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 递归验证JSON结构
    /// </summary>
    private (bool IsValid, string? ErrorMessage) ValidateJsonStructure(JToken actual, JToken template, string path = "")
    {
        switch (template.Type)
        {
            case JTokenType.Object:
                if (actual.Type != JTokenType.Object)
                    return (false, $"{path}: 期望对象，实际 {actual.Type}");

                foreach (var prop in template.Children<JProperty>())
                {
                    var actualProp = actual[prop.Name];
                    if (actualProp == null)
                        return (false, $"{path}.{prop.Name}: 缺少必需字段");

                    var result = ValidateJsonStructure(actualProp, prop.Value, $"{path}.{prop.Name}");
                    if (!result.IsValid)
                        return result;
                }
                break;

            case JTokenType.Array:
                if (actual.Type != JTokenType.Array)
                    return (false, $"{path}: 期望数组，实际 {actual.Type}");

                if (template.Count() > 0 && !actual.Any())
                    return (false, $"{path}: 数组不能为空");
                break;

            case JTokenType.String:
                if (actual.Type != JTokenType.String)
                    return (false, $"{path}: 期望字符串，实际 {actual.Type}");

                var strVal = template.Value<string>();
                if (!string.IsNullOrEmpty(strVal) && strVal.StartsWith("{") && actual.Value<string>() == "")
                    return (false, $"{path}: 字符串不能为空");
                break;

            case JTokenType.Integer:
            case JTokenType.Float:
                if (actual.Type != JTokenType.Integer && actual.Type != JTokenType.Float)
                    return (false, $"{path}: 期望数字，实际 {actual.Type}");
                break;

            case JTokenType.Boolean:
                if (actual.Type != JTokenType.Boolean)
                    return (false, $"{path}: 期望布尔值，实际 {actual.Type}");
                break;

            case JTokenType.Null:
                // 模板中的null表示可选字段，跳过验证
                break;
        }

        return (true, null);
    }

    /// <summary>
    /// 从JSON中提取值（支持嵌套路径）
    /// </summary>
    private object? ExtractValueFromJson(JToken root, string path)
    {
        var parts = path.Split('.');
        var current = root;

        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];

            // 处理数组路径，如 items[0].materialCode
            var arrayMatch = Regex.Match(part, @"^(.+)\[(\d+)\]$");
            if (arrayMatch.Success)
            {
                var arrayName = arrayMatch.Groups[1].Value;
                var index = int.Parse(arrayMatch.Groups[2].Value);

                var arrayProp = current[arrayName];
                if (arrayProp == null || arrayProp.Type != JTokenType.Array)
                    return null;

                var array = arrayProp.ElementAtOrDefault(index);
                if (array == null)
                    return null;

                current = array;
            }
            else
            {
                var prop = current[part];
                if (prop == null)
                    return null;

                current = prop;
            }

            // 如果是最后一个元素，返回值
            if (i == parts.Length - 1)
            {
                return current.Type switch
                {
                    JTokenType.String => current.Value<string>(),
                    JTokenType.Integer => current.Value<int>(),
                    JTokenType.Float => current.Value<double>(),
                    JTokenType.Boolean => current.Value<bool>(),
                    JTokenType.Null => null,
                    JTokenType.Array => JsonConvert.SerializeObject(current),
                    JTokenType.Object => JsonConvert.SerializeObject(current),
                    _ => current.ToString()
                };
            }
        }

        return null;
    }

    /// <summary>
    /// 执行转换脚本
    /// 支持语法：
    /// 1. 变量引用: {data.fieldName}
    /// 2. 函数调用: {ToUpper({data.fieldName})}
    /// 3. 表达式: {data.field1 + data.field2}
    /// 4. 条件: {data.field1 ? "yes" : "no"}
    /// 5. 格式化: {FormatDate({data.date}, "yyyy-MM-dd")}
    /// </summary>
    private (bool Success, string? ErrorMessage, string? Data) ExecuteTransformScript(string data, string script)
    {
        try
        {
            var jsonData = JToken.Parse(data);
            var result = script;

            // 递归处理所有表达式 {expression}
            result = ProcessExpressions(result, jsonData);

            // 验证结果是否为有效JSON
            try
            {
                JToken.Parse(result);
            }
            catch
            {
                // 如果不是有效JSON，尝试将其包装为JSON字符串
                result = JsonConvert.SerializeObject(result);
            }

            return (true, null, result);
        }
        catch (JsonException ex)
        {
            return (false, $"JSON解析失败: {ex.Message}", null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    /// <summary>
    /// 递归处理表达式
    /// </summary>
    private string ProcessExpressions(string input, JToken data)
    {
        var result = input;
        var maxIterations = 100; // 防止无限循环
        var iteration = 0;

        // 持续处理直到没有更多花括号表达式
        while (Regex.IsMatch(result, @"\{[^{}]+\}") && iteration < maxIterations)
        {
            iteration++;

            // 找到最内层的表达式（没有嵌套花括号的）
            var matches = Regex.Matches(result, @"\{[^{}]+\}");

            foreach (Match match in matches.Cast<Match>().Reverse())
            {
                var expression = match.Value.Substring(1, match.Value.Length - 2); // 去掉花括号
                var evaluated = EvaluateExpression(expression, data);
                result = result.Substring(0, match.Index) + evaluated + result.Substring(match.Index + match.Length);
            }
        }

        return result;
    }

    /// <summary>
    /// 计算表达式
    /// </summary>
    private string EvaluateExpression(string expression, JToken data)
    {
        expression = expression.Trim();

        // 1. 处理三元运算符 condition ? trueValue : falseValue
        var ternaryMatch = Regex.Match(expression, @"^(.+?)\s*\?\s*(.+?)\s*:\s*(.+)$");
        if (ternaryMatch.Success)
        {
            var condition = EvaluateExpression(ternaryMatch.Groups[1].Value, data);
            var trueValue = EvaluateExpression(ternaryMatch.Groups[2].Value, data);
            var falseValue = EvaluateExpression(ternaryMatch.Groups[3].Value, data);

            return IsTruthy(condition) ? trueValue : falseValue;
        }

        // 2. 处理函数调用 FunctionName(arg1, arg2, ...)
        var functionMatch = Regex.Match(expression, @"^(\w+)\((.*)\)$");
        if (functionMatch.Success)
        {
            var functionName = functionMatch.Groups[1].Value;
            var argsString = functionMatch.Groups[2].Value;
            var args = ParseArguments(argsString, data);

            return ExecuteFunction(functionName, args);
        }

        // 3. 处理变量引用 data.fieldName 或 fieldName
        if (expression.StartsWith("data."))
        {
            var path = expression.Substring(5);
            var value = ExtractValueFromJson(data, path);
            return ConvertToString(value);
        }

        // 4. 处理直接字段名
        var directValue = ExtractValueFromJson(data, expression);
        if (directValue != null)
        {
            return ConvertToString(directValue);
        }

        // 5. 处理字符串字面量
        if (expression.StartsWith("\"") && expression.EndsWith("\""))
        {
            return expression.Substring(1, expression.Length - 2);
        }

        // 6. 处理数字字面量
        if (decimal.TryParse(expression, out var num))
        {
            return num.ToString();
        }

        return expression;
    }

    /// <summary>
    /// 解析参数列表
    /// </summary>
    private List<string> ParseArguments(string argsString, JToken data)
    {
        var args = new List<string>();
        if (string.IsNullOrWhiteSpace(argsString))
            return args;

        var depth = 0;
        var currentArg = string.Empty;

        foreach (var ch in argsString)
        {
            if (ch == '(' || ch == '{')
            {
                depth++;
                currentArg += ch;
            }
            else if (ch == ')' || ch == '}')
            {
                depth--;
                currentArg += ch;
            }
            else if (ch == ',' && depth == 0)
            {
                var arg = ProcessExpressions(currentArg.Trim(), data);
                args.Add(arg);
                currentArg = string.Empty;
            }
            else
            {
                currentArg += ch;
            }
        }

        if (!string.IsNullOrWhiteSpace(currentArg))
        {
            var arg = ProcessExpressions(currentArg.Trim(), data);
            args.Add(arg);
        }

        return args;
    }

    /// <summary>
    /// 执行内置函数
    /// </summary>
    private string ExecuteFunction(string functionName, List<string> args)
    {
        return functionName.ToUpper() switch
        {
            "TOUPPER" when args.Count > 0 => args[0].ToUpper(),
            "TOLOWER" when args.Count > 0 => args[0].ToLower(),
            "TRIM" when args.Count > 0 => args[0].Trim(),
            "SUBSTRING" when args.Count >= 3 => args[0].Substring(int.Parse(args[1]), int.Parse(args[2])),
            "SUBSTRING" when args.Count == 2 => args[0].Substring(int.Parse(args[1])),
            "CONCAT" => string.Join("", args),
            "LENGTH" when args.Count > 0 => args[0].Length.ToString(),
            "REPLACE" when args.Count == 3 => args[0].Replace(args[1], args[2]),
            "SPLIT" when args.Count == 2 => string.Join(",", args[0].Split(new[] { args[1] }, StringSplitOptions.None)),
            "JOIN" when args.Count >= 2 => string.Join(args[0], args.Skip(1)),
            "TOINT" when args.Count > 0 && int.TryParse(args[0], out var i) => i.ToString(),
            "TODOUBLE" when args.Count > 0 && double.TryParse(args[0], out var d) => d.ToString(),
            "TODECIMAL" when args.Count > 0 && decimal.TryParse(args[0], out var m) => m.ToString(),
            "ROUND" when args.Count == 2 && decimal.TryParse(args[0], out var dm) => Math.Round(dm, int.Parse(args[1])).ToString(),
            "ROUND" when args.Count == 1 && decimal.TryParse(args[0], out var dm2) => Math.Round(dm2).ToString(),
            "ABS" when args.Count > 0 && decimal.TryParse(args[0], out var abs) => Math.Abs(abs).ToString(),
            "FLOOR" when args.Count > 0 && decimal.TryParse(args[0], out var floor) => Math.Floor(floor).ToString(),
            "CEILING" when args.Count > 0 && decimal.TryParse(args[0], out var ceil) => Math.Ceiling(ceil).ToString(),
            "FORMATDATE" when args.Count == 2 && DateTime.TryParse(args[0], out var fdt) => fdt.ToString(args[1]),
            "FORMATDATE" when args.Count == 2 && DateTime.TryParseExact(args[0], "yyyy-MM-dd", null, DateTimeStyles.None, out var fdt2) => fdt2.ToString(args[1]),
            "NOW" when args.Count == 0 => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            "NOW" when args.Count == 1 => DateTime.Now.ToString(args[0]),
            "TODAY" when args.Count == 0 => DateTime.Today.ToString("yyyy-MM-dd"),
            "ADDDAYS" when args.Count == 2 && DateTime.TryParse(args[0], out var addDt) => addDt.AddDays(int.Parse(args[1])).ToString("yyyy-MM-dd"),
            "ADDYEARS" when args.Count == 2 && DateTime.TryParse(args[0], out var addYear) => addYear.AddYears(int.Parse(args[1])).ToString("yyyy-MM-dd"),
            "ADDMONTHS" when args.Count == 2 && DateTime.TryParse(args[0], out var addMonth) => addMonth.AddMonths(int.Parse(args[1])).ToString("yyyy-MM-dd"),
            "DATEDIFF" when args.Count == 2 && DateTime.TryParse(args[0], out var d1) && DateTime.TryParse(args[1], out var d2) => (d2 - d1).Days.ToString(),
            "IFNULL" when args.Count >= 2 => string.IsNullOrEmpty(args[0]) ? args[1] : args[0],
            "COALESCE" when args.Count > 0 => args.FirstOrDefault(a => !string.IsNullOrEmpty(a)) ?? "",
            "EQ" when args.Count == 2 => args[0] == args[1] ? "true" : "false",
            "NE" when args.Count == 2 => args[0] != args[1] ? "true" : "false",
            "GT" when args.Count == 2 && decimal.TryParse(args[0], out var gt1) && decimal.TryParse(args[1], out var gt2) => gt1 > gt2 ? "true" : "false",
            "LT" when args.Count == 2 && decimal.TryParse(args[0], out var lt1) && decimal.TryParse(args[1], out var lt2) => lt1 < lt2 ? "true" : "false",
            "GTE" when args.Count == 2 && decimal.TryParse(args[0], out var gte1) && decimal.TryParse(args[1], out var gte2) => gte1 >= gte2 ? "true" : "false",
            "LTE" when args.Count == 2 && decimal.TryParse(args[0], out var lte1) && decimal.TryParse(args[1], out var lte2) => lte1 <= lte2 ? "true" : "false",
            "AND" => args.All(a => IsTruthy(a)) ? "true" : "false",
            "OR" => args.Any(a => IsTruthy(a)) ? "true" : "false",
            "NOT" when args.Count == 1 => !IsTruthy(args[0]) ? "true" : "false",
            "CONTAINS" when args.Count == 2 => args[0].Contains(args[1]) ? "true" : "false",
            "STARTSWITH" when args.Count == 2 => args[0].StartsWith(args[1]) ? "true" : "false",
            "ENDSWITH" when args.Count == 2 => args[0].EndsWith(args[1]) ? "true" : "false",
            "ISEMPTY" when args.Count == 1 => string.IsNullOrEmpty(args[0]) ? "true" : "false",
            "ISNULL" when args.Count == 1 => args[0] == null || args[0] == "null" ? "true" : "false",
            "GENERATEGUID" => Guid.NewGuid().ToString("N"),
            "TOSTRING" when args.Count > 0 => args[0].ToString(),
            _ => throw new ArgumentException($"未知函数: {functionName}")
        };
    }

    /// <summary>
    /// 判断值是否为真
    /// </summary>
    private bool IsTruthy(string value)
    {
        if (string.IsNullOrEmpty(value) || value == "null" || value == "false" || value == "0")
            return false;
        return true;
    }

    /// <summary>
    /// 转换为字符串
    /// </summary>
    private string ConvertToString(object? value)
    {
        return value?.ToString() ?? "";
    }

    /// <summary>
    /// 执行业务逻辑（使用工厂模式动态调用）
    /// </summary>
    private async Task<(bool Success, object? Data, string? ErrorMessage)> ExecuteBusinessLogicAsync(
        string data, IntInterfaceConfig config)
    {
        try
        {
            if (config == null || string.IsNullOrEmpty(config.ProcessorType))
            {
                // 如果没有配置处理器，返回默认成功
                return (true, new { message = "接收成功，未配置业务处理器" }, null);
            }
            // 获取业务处理器
            var processor = _processorFactory.GetProcessor(config.ProcessorType);
            if (processor == null)
            {
                // 如果没有配置处理器，返回默认成功
                return (true, new { message = "接收成功，未配置业务处理器" }, null);
            }

            // 解析JSON数据
            var jsonData = JToken.Parse(data);

            // 调用处理器
            return await processor.ProcessAsync(jsonData, _serviceProvider);
        }
        catch (JsonException ex)
        {
            return (false, null, $"JSON解析失败: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, null, $"业务处理异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理异步接口
    /// </summary>
    private async Task ProcessAsyncInterfaceAsync(IntInterfaceConfig config, string data, string traceId)
    {
        // 异步处理逻辑
        await Task.Delay(100);
    }

    /// <summary>
    /// 构建响应数据
    /// </summary>
    private string BuildResponseData(object? data, string? responseTemplate)
    {
        if (string.IsNullOrEmpty(responseTemplate))
        {
            return JsonConvert.SerializeObject(new { success = true, message = "处理成功", data }, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }

        try
        {
            // 替换模板中的占位符
            var result = responseTemplate;
            if (data != null)
            {
                var dataStr = JsonConvert.SerializeObject(data);
                result = result.Replace("\"{data}\"", dataStr);
            }
            return result;
        }
        catch
        {
            return JsonConvert.SerializeObject(new { success = true, message = "处理成功", data }, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }
    }

    /// <summary>
    /// 获取请求头
    /// </summary>
    private Dictionary<string, string> GetRequestHeaders(HttpRequest request)
    {
        var headers = new Dictionary<string, string>();
        foreach (var header in request.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value.ToArray());
        }
        return headers;
    }

    /// <summary>
    /// 保存调用日志
    /// </summary>
    private async Task SaveCallLogAsync(IntInterfaceLog log)
    {
        try
        {
            await _db.Insertable(log).ExecuteCommandAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存接口调用日志失败");
        }
    }

    /// <summary>
    /// 重新加载接口配置
    /// </summary>
    public async Task ReloadInterfacesAsync()
    {
        await LoadEnabledInboundInterfacesAsync();
    }

    /// <summary>
    /// 获取所有启用的接口
    /// </summary>
    public async Task<List<IntInterfaceConfig>> GetEnabledInterfacesAsync()
    {
        var interfaces = _cache.Get<List<IntInterfaceConfig>>(CACHE_KEY_ALL);
        if ((!interfaces?.Any()) ?? true)
        {
            await LoadEnabledInboundInterfacesAsync();

            interfaces = _cache.Get<List<IntInterfaceConfig>>(CACHE_KEY_ALL);
        }
        return await Task.FromResult(interfaces) ?? new List<IntInterfaceConfig>();
    }

    /// <summary>
    /// 根据接口编码获取接口配置
    /// </summary>
    public async Task<IntInterfaceConfig?> GetInterfaceByCodeAsync(string interfaceCode)
    {
        var interfaces = _cache.Get<List<IntInterfaceConfig>>(CACHE_KEY_ALL);
        return await Task.FromResult(interfaces?.FirstOrDefault(x => x.InterfaceCode == interfaceCode));
    }

    /// <summary>
    /// 根据请求路径匹配接口
    /// </summary>
    public async Task<IntInterfaceConfig?> MatchInterfaceByPathAsync(string path, string httpMethod)
    {
        var interfaces = await GetEnabledInterfacesAsync();
        return interfaces.FirstOrDefault(x =>
        {
            if (!string.IsNullOrEmpty(x.RequestPath))
            {
                // 支持路径参数匹配
                var configPath = x.RequestPath.TrimEnd('/');
                var requestPath = path.TrimEnd('/');

                // 替换路径参数为通配符
                configPath = Regex.Replace(configPath, @"\{[^}]+\}", "*");

                // 简单匹配（实际应该使用更复杂的路由匹配）
                if (configPath.EndsWith("*"))
                {
                    var prefix = configPath[..^1];
                    if (requestPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return string.IsNullOrEmpty(x.HttpMethod) ||
                               x.HttpMethod.Equals(httpMethod, StringComparison.OrdinalIgnoreCase);
                    }
                }
                else if (configPath.Equals(requestPath, StringComparison.OrdinalIgnoreCase))
                {
                    return string.IsNullOrEmpty(x.HttpMethod) ||
                           x.HttpMethod.Equals(httpMethod, StringComparison.OrdinalIgnoreCase);
                }
            }
            return false;
        });
    }
}
