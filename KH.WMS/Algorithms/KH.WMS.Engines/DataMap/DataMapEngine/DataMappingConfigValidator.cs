
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;

namespace KH.WMS.Engines.DataMap;


/// <summary>
/// 数据映射配置验证器
/// </summary>
public class DataMappingConfigValidator
{
    private const string CSharpScriptTemplate = @"
        using System;
        using System.Linq;
        using System.Collections.Generic;
        using System.Dynamic;

        public class ScriptGlobals
        {{
            public IDictionary<string, object> Data {{ get; set; }}
            public Action<string>? Log {{ get; set; }}
        }}
";

    /// <summary>
    /// 验证数据映射配置（完整验证）
    /// </summary>
    /// <param name="config">配置对象</param>
    /// <param name="autoFix">是否自动修复可修复的错误</param>
    /// <returns>验证结果</returns>
    public ValidationResult Validate(IntDataMappingConfig config, bool autoFix = false)
    {
        var result = new ValidationResult { IsValid = true };

        // 1. 验证基本字段
        ValidateBasicFields(config, result);

        // 2. 验证映射规则
        ValidateMappingRules(config, result, autoFix);

        // 3. 验证目标类型
        ValidateTargetType(config, result);

        // 4. 验证转换脚本
        ValidateTransformScript(config, result);

        // 5. 验证验证规则
        ValidateValidationRules(config, result, autoFix);

        // 设置总体验证结果
        result.IsValid = result.Errors.Count == 0;

        return result;
    }

    /// <summary>
    /// 仅验证转换脚本
    /// </summary>
    /// <param name="transformScript">转换脚本内容</param>
    /// <param name="scriptType">脚本类型</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateTransformScriptOnly(string transformScript, string? scriptType = null)
    {
        var result = new ValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(transformScript))
        {
            result.Errors.Add(new ValidationError
            {
                Code = "TRANSFORM_SCRIPT_EMPTY",
                Field = "TransformScript",
                Message = "转换脚本不能为空"
            });
            result.IsValid = false;
            return result;
        }

        var actualScriptType = (scriptType ?? DataMapConstants.ScriptLanguage.CSHARP).ToUpper();

        switch (actualScriptType)
        {
            case DataMapConstants.ScriptLanguage.CSHARP:
                ValidateCSharpScript(transformScript, result);
                break;

            case DataMapConstants.ScriptLanguage.LUA:
                result.Warnings.Add(new ValidationWarning
                {
                    Field = "TransformScript",
                    Message = "LUA 脚本验证暂未实现"
                });
                break;

            case DataMapConstants.ScriptLanguage.JAVASCRIPT:
                result.Warnings.Add(new ValidationWarning
                {
                    Field = "TransformScript",
                    Message = "JavaScript 脚本验证暂未实现"
                });
                break;

            default:
                result.Errors.Add(new ValidationError
                {
                    Code = "SCRIPT_TYPE_INVALID",
                    Field = "ScriptType",
                    Message = "脚本类型无效",
                    Details = "有效值: CSHARP, LUA, JAVASCRIPT"
                });
                result.IsValid = false;
                break;
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    /// <summary>
    /// 仅验证映射规则
    /// </summary>
    /// <param name="mappingRulesJson">映射规则JSON字符串</param>
    /// <param name="autoFix">是否自动修复可修复的错误</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateMappingRulesOnly(string mappingRulesJson, bool autoFix = false)
    {
        var result = new ValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(mappingRulesJson))
        {
            result.Errors.Add(new ValidationError
            {
                Code = "MAPPING_RULES_EMPTY",
                Field = "MappingRules",
                Message = "映射规则不能为空"
            });
            result.IsValid = false;
            return result;
        }

        try
        {
            var rules = JsonConvert.DeserializeObject<List<MappingRule>>(mappingRulesJson);
            if (rules == null || rules.Count == 0)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "MAPPING_RULES_EMPTY",
                    Field = "MappingRules",
                    Message = "映射规则解析后为空"
                });
                result.IsValid = false;
                return result;
            }

            // 验证每个规则
            for (int i = 0; i < rules.Count; i++)
            {
                ValidateMappingRule(rules[i], i, result, autoFix);
            }

            // 检查是否有重复的 TargetField
            var duplicateFields = rules
                .GroupBy(r => r.TargetField)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateFields.Any())
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "DUPLICATE_TARGET_FIELD",
                    Field = "MappingRules",
                    Message = "存在重复的目标字段",
                    Details = $"重复字段: {string.Join(", ", duplicateFields)}"
                });
            }
        }
        catch (JsonException ex)
        {
            result.Errors.Add(new ValidationError
            {
                Code = "MAPPING_RULES_INVALID_JSON",
                Field = "MappingRules",
                Message = "映射规则JSON格式错误",
                Details = ex.Message
            });
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    /// <summary>
    /// 仅验证验证规则
    /// </summary>
    /// <param name="validationRulesJson">验证规则JSON字符串</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateValidationRulesOnly(string validationRulesJson)
    {
        var result = new ValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(validationRulesJson))
        {
            result.Errors.Add(new ValidationError
            {
                Code = "VALIDATION_RULES_EMPTY",
                Field = "ValidationRules",
                Message = "验证规则不能为空"
            });
            result.IsValid = false;
            return result;
        }

        try
        {
            var jToken = JToken.Parse(validationRulesJson);

            if (jToken.Type == JTokenType.Array)
            {
                var rules = jToken as JArray;
                ValidateValidationRulesArray(rules, result);
            }
            else if (jToken.Type == JTokenType.Object)
            {
                var ruleObj = jToken as JObject;
                ValidateValidationRulesObject(ruleObj, result);
            }
            else
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "VALIDATION_RULES_INVALID_FORMAT",
                    Field = "ValidationRules",
                    Message = "验证规则格式错误",
                    Details = "应该是 JSON 数组或对象"
                });
            }
        }
        catch (JsonException ex)
        {
            result.Errors.Add(new ValidationError
            {
                Code = "VALIDATION_RULES_INVALID_JSON",
                Field = "ValidationRules",
                Message = "验证规则JSON格式错误",
                Details = ex.Message
            });
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    /// <summary>
    /// 验证基本字段
    /// </summary>
    private void ValidateBasicFields(IntDataMappingConfig config, ValidationResult result)
    {
        // 验证 MappingCode
        if (string.IsNullOrWhiteSpace(config.MappingCode))
        {
            result.Errors.Add(new ValidationError
            {
                Code = "MAPPING_CODE_EMPTY",
                Field = "MappingCode",
                Message = "映射编码不能为空"
            });
        }
        else if (config.MappingCode.Length > 50)
        {
            result.Errors.Add(new ValidationError
            {
                Code = "MAPPING_CODE_TOO_LONG",
                Field = "MappingCode",
                Message = "映射编码长度不能超过50个字符",
                Details = $"当前长度: {config.MappingCode.Length}"
            });
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(config.MappingCode, @"^[A-Za-z0-9_]+$"))
        {
            result.Errors.Add(new ValidationError
            {
                Code = "MAPPING_CODE_INVALID_FORMAT",
                Field = "MappingCode",
                Message = "映射编码只能包含字母、数字和下划线"
            });
        }

        // 验证 MappingName
        if (string.IsNullOrWhiteSpace(config.MappingName))
        {
            result.Warnings.Add(new ValidationWarning
            {
                Field = "MappingName",
                Message = "映射名称为空，建议设置"
            });
        }

        // 验证方向
        if (!string.IsNullOrEmpty(config.Direction))
        {
            var validDirections = new[] { DataMapConstants.InterfaceDirection.INBOUND, DataMapConstants.InterfaceDirection.OUTBOUND, DataMapConstants.InterfaceDirection.BIDIRECTIONAL };
            if (!validDirections.Contains(config.Direction.ToUpper()))
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "DIRECTION_INVALID",
                    Field = "Direction",
                    Message = "方向值无效",
                    Details = "有效值: INBOUND, OUTBOUND, BIDIRECTIONAL"
                });
            }
        }

        // 验证脚本类型
        if (!string.IsNullOrEmpty(config.ScriptType))
        {
            var validScriptTypes = new[] { DataMapConstants.ScriptLanguage.CSHARP, DataMapConstants.ScriptLanguage.LUA, DataMapConstants.ScriptLanguage.JAVASCRIPT };
            if (!validScriptTypes.Contains(config.ScriptType.ToUpper()))
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "SCRIPT_TYPE_INVALID",
                    Field = "ScriptType",
                    Message = "脚本类型无效",
                    Details = "有效值: CSHARP, LUA, JAVASCRIPT"
                });

                // 建议：默认使用 CSHARP
                result.Suggestions.Add(new FixSuggestion
                {
                    Type = FixType.SetDefaultValue,
                    Description = "将脚本类型设置为 CSHARP",
                    SuggestedValue = DataMapConstants.ScriptLanguage.CSHARP,
                    CanAutoFix = true
                });
            }
        }
    }

    /// <summary>
    /// 验证映射规则
    /// </summary>
    private void ValidateMappingRules(IntDataMappingConfig config, ValidationResult result, bool autoFix)
    {
        if (string.IsNullOrWhiteSpace(config.MappingRules))
        {
            result.Errors.Add(new ValidationError
            {
                Code = "MAPPING_RULES_EMPTY",
                Field = "MappingRules",
                Message = "映射规则不能为空"
            });
            return;
        }

        try
        {
            var rules = JsonConvert.DeserializeObject<List<MappingRule>>(config.MappingRules);
            if (rules == null || rules.Count == 0)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "MAPPING_RULES_EMPTY",
                    Field = "MappingRules",
                    Message = "映射规则解析后为空"
                });
                return;
            }

            // 验证每个规则
            for (int i = 0; i < rules.Count; i++)
            {
                ValidateMappingRule(rules[i], i, result, autoFix);
            }

            // 检查是否有重复的 TargetField
            var duplicateFields = rules
                .GroupBy(r => r.TargetField)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateFields.Any())
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "DUPLICATE_TARGET_FIELD",
                    Field = "MappingRules",
                    Message = "存在重复的目标字段",
                    Details = $"重复字段: {string.Join(", ", duplicateFields)}"
                });
            }
        }
        catch (JsonException ex)
        {
            result.Errors.Add(new ValidationError
            {
                Code = "MAPPING_RULES_INVALID_JSON",
                Field = "MappingRules",
                Message = "映射规则JSON格式错误",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// 验证单个映射规则
    /// </summary>
    private void ValidateMappingRule(MappingRule rule, int index, ValidationResult result, bool autoFix, string parentField = "")
    {
        var fieldPrefix = string.IsNullOrEmpty(parentField) ? $"MappingRules[{index}]" : $"{parentField}.Children[{index}]";

        // 验证 SourceField
        if (string.IsNullOrWhiteSpace(rule.SourceField))
        {
            result.Errors.Add(new ValidationError
            {
                Code = "SOURCE_FIELD_EMPTY",
                Field = $"{fieldPrefix}.SourceField",
                Message = "源字段不能为空"
            });
        }

        // 验证 TargetField
        if (string.IsNullOrWhiteSpace(rule.TargetField))
        {
            result.Errors.Add(new ValidationError
            {
                Code = "TARGET_FIELD_EMPTY",
                Field = $"{fieldPrefix}.TargetField",
                Message = "目标字段不能为空"
            });
        }

        // 验证 DataType 和 IsArray 的一致性
        if (!string.IsNullOrEmpty(rule.DataType))
        {
            var isArrayType = rule.DataType.Contains("[]") || rule.DataType.Contains("List") || rule.DataType.Contains("Array") || rule.DataType.Contains("Collection");

            if (isArrayType && !rule.IsArray)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "DATATYPE_ARRAY_MISMATCH",
                    Field = $"{fieldPrefix}.IsArray",
                    Message = "数据类型是数组，但 IsArray 设置为 false",
                    Details = $"DataType: {rule.DataType}, IsArray: {rule.IsArray}"
                });

                // 建议：自动修复
                if (autoFix)
                {
                    result.Suggestions.Add(new FixSuggestion
                    {
                        Type = FixType.FixBoolean,
                        Description = $"将 {fieldPrefix}.IsArray 设置为 true",
                        SuggestedValue = true,
                        CanAutoFix = true
                    });
                }
            }

            if (!isArrayType && rule.IsArray)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Field = $"{fieldPrefix}.DataType",
                    Message = $"IsArray 为 true，但 DataType ({rule.DataType}) 不包含数组标记"
                });
            }
        }

        // 验证 Children 与 IsArray 的一致性
        if (rule.IsArray)
        {
            if (rule.Children == null || rule.Children.Count == 0)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Field = $"{fieldPrefix}.Children",
                    Message = "IsArray 为 true，但没有定义 Children 规则，数组元素将不会被映射"
                });
            }
        }
        else
        {
            if (rule.Children != null && rule.Children.Count > 0)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "CHILDREN_WITHOUT_ARRAY",
                    Field = $"{fieldPrefix}.Children",
                    Message = "定义了 Children 规则，但 IsArray 为 false",
                    Details = "如果字段不是数组，不应该有 Children"
                });
            }
        }

        // 递归验证 Children
        if (rule.Children != null && rule.Children.Count > 0)
        {
            for (int i = 0; i < rule.Children.Count; i++)
            {
                ValidateMappingRule(rule.Children[i], i, result, autoFix, fieldPrefix);
            }
        }

        // 验证 Converter
        if (!string.IsNullOrEmpty(rule.Converter))
        {
            var validConverters = new[] { "TOUPPER", "TOLOWER", "TRIM", "TOSTRING", "TOINT", "TODECIMAL", "TODATETIME" };
            var converterUpper = rule.Converter.ToUpper();
            if (!validConverters.Contains(converterUpper))
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Field = $"{fieldPrefix}.Converter",
                    Message = $"转换器 {rule.Converter} 不是内置转换器",
                });
            }
        }
    }

    /// <summary>
    /// 验证目标类型
    /// </summary>
    private void ValidateTargetType(IntDataMappingConfig config, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(config.TargetType))
        {
            result.Warnings.Add(new ValidationWarning
            {
                Field = "TargetType",
                Message = "未指定目标类型，映射结果将作为动态对象返回"
            });
            return;
        }

        try
        {
            var type = Type.GetType(config.TargetType);
            if (type == null)
            {
                // 尝试在当前程序集中查找
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                type = assemblies
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName == config.TargetType || t.Name == config.TargetType);
            }

            if (type == null)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "TARGET_TYPE_NOT_FOUND",
                    Field = "TargetType",
                    Message = "目标类型在系统中不存在",
                    Details = $"类型: {config.TargetType}"
                });

                // 建议：检查常见的拼写错误
                var suggestions = SuggestCorrectTypeName(config.TargetType);
                if (suggestions.Any())
                {
                    result.Suggestions.Add(new FixSuggestion
                    {
                        Type = FixType.FixTypeName,
                        Description = "可能是拼写错误，建议的类型名称",
                        SuggestedValue = suggestions.FirstOrDefault(),
                        CanAutoFix = false
                    });
                }
            }
            else
            {
                // 验证类型是否有无参构造函数
                var constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                {
                    result.Errors.Add(new ValidationError
                    {
                        Code = "TARGET_TYPE_NO_CONSTRUCTOR",
                        Field = "TargetType",
                        Message = "目标类型没有无参构造函数",
                        Details = "映射到强类型需要无参构造函数"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add(new ValidationError
            {
                Code = "TARGET_TYPE_LOAD_ERROR",
                Field = "TargetType",
                Message = "加载目标类型时出错",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// 验证转换脚本
    /// </summary>
    private void ValidateTransformScript(IntDataMappingConfig config, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(config.TransformScript))
            return;

        var scriptType = (config.ScriptType ?? DataMapConstants.ScriptLanguage.CSHARP).ToUpper();

        switch (scriptType)
        {
            case DataMapConstants.ScriptLanguage.CSHARP:
                ValidateCSharpScript(config.TransformScript, result);
                break;

            case DataMapConstants.ScriptLanguage.LUA:
                result.Warnings.Add(new ValidationWarning
                {
                    Field = "TransformScript",
                    Message = "LUA 脚本验证暂未实现"
                });
                break;

            case DataMapConstants.ScriptLanguage.JAVASCRIPT:
                result.Warnings.Add(new ValidationWarning
                {
                    Field = "TransformScript",
                    Message = "JavaScript 脚本验证暂未实现"
                });
                break;
        }
    }

    /// <summary>
    /// 验证 C# 脚本
    /// </summary>
    private void ValidateCSharpScript(string script, ValidationResult result)
    {
        try
        {
            // 基本语法检查
            if (string.IsNullOrWhiteSpace(script))
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Field = "TransformScript",
                    Message = "转换脚本为空"
                });
                return;
            }

            // 检查括号匹配
            var openBraces = script.Count(c => c == '{');
            var closeBraces = script.Count(c => c == '}');
            if (openBraces != closeBraces)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "SCRIPT_BRACES_MISMATCH",
                    Field = "TransformScript",
                    Message = "大括号不匹配",
                    Details = $"开括号 {openBraces} 个，闭括号 {closeBraces} 个"
                });
            }

            var openParens = script.Count(c => c == '(');
            var closeParens = script.Count(c => c == ')');
            if (openParens != closeParens)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "SCRIPT_PARENS_MISMATCH",
                    Field = "TransformScript",
                    Message = "圆括号不匹配",
                    Details = $"开括号 {openParens} 个，闭括号 {closeParens} 个"
                });
            }

            var openBrackets = script.Count(c => c == '[');
            var closeBrackets = script.Count(c => c == ']');
            if (openBrackets != closeBrackets)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "SCRIPT_BRACKETS_MISMATCH",
                    Field = "TransformScript",
                    Message = "方括号不匹配",
                    Details = $"开括号 {openBrackets} 个，闭括号 {closeBrackets} 个"
                });
            }

            // 检查是否使用 data 变量
            var scriptLower = script.ToLower();
            if (!scriptLower.Contains("data"))
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Field = "TransformScript",
                    Message = "脚本中没有使用 data 变量，脚本应该通过 data 参数访问数据"
                });
            }

            // 检查是否有 return 语句
            if (!scriptLower.Contains("return"))
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Field = "TransformScript",
                    Message = "建议脚本包含 return 语句返回转换后的数据"
                });
            }

            // 检查是否包含危险的调用。
            // 注意：此黑名单仅作纵深防御，无法阻挡字符串拼接/间接调用等绕过手段。
            // 真正的防护是：ScriptGlobals 不再注入 ISqlSugarClient，且脚本配置仅限可信管理员维护。
            var dangerousPatterns = new[]
            {
                // 文件系统
                "System.IO.File.", "System.IO.Directory.", "File.Delete", "File.Move", "File.WriteAll", "File.AppendAll", "File.Copy",
                // 进程
                "System.Diagnostics", "Process.Start", "Process.Get",
                // 反射 / 程序集加载（可绕过到任意类型/方法，是黑名单的主要绕过面）
                "System.Reflection", "Assembly.Load", "Assembly.Get", "Type.GetType", ".GetMethod", ".Invoke(", "Activator.CreateInstance",
                // AppDomain
                "AppDomain",
                // 网络
                "System.Net", "HttpClient", "WebClient", "Socket", "TcpClient", "TcpListener",
                // 注册表 / 环境
                "Microsoft.Win32", "Registry", "Environment.Exit", "Environment.GetEnvironmentVariable",
                // 直接数据库连接（绕过 ORM）
                "SqlConnection", "OleDbConnection", "OdbcConnection", "MySqlConnection", "NpgsqlConnection", "SQLiteConnection",
                // ORM 访问（Db 已从全局变量移除，禁止脚本直接操作数据库）
                "ISqlSugarClient", "SqlSugarClient", ".Ado.", ".Queryable<", ".Insertable<", ".Updateable<", ".Deleteable<"
            };

            foreach (var pattern in dangerousPatterns)
            {
                if (script.Contains(pattern))
                {
                    result.Errors.Add(new ValidationError
                    {
                        Code = "SCRIPT_DANGEROUS_CALL",
                        Field = "TransformScript",
                        Message = $"脚本包含危险的调用: {pattern}",
                        Details = "转换脚本不允许访问文件系统、启动进程或直接访问数据库"
                    });
                }
            }

            // 尝试创建脚本来验证语法（不执行）
            try
            {
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

                var fullScript = CSharpScriptTemplate + "\nreturn " + script + ";";

                // 尝试创建脚本（只检查语法，不执行）
                // 注意：新版本的 Roslyn API 不支持 GetDiagnostics，所以只能尝试创建
                var createdScript = CSharpScript.Create(fullScript, scriptOptions);

                // 如果创建成功，说明基本语法正确
                // 如果有语法错误，Create 方法会抛出异常
            }
            catch (Exception compilationEx)
            {
                // 编译失败，添加错误信息
                result.Errors.Add(new ValidationError
                {
                    Code = "SCRIPT_COMPILATION_ERROR",
                    Field = "TransformScript",
                    Message = "C# 脚本语法错误",
                    Details = compilationEx.Message
                });
            }
        }
        catch (Exception ex)
        {
            result.Warnings.Add(new ValidationWarning
            {
                Field = "TransformScript",
                Message = $"脚本验证时发生异常: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// 验证验证规则
    /// </summary>
    private void ValidateValidationRules(IntDataMappingConfig config, ValidationResult result, bool autoFix)
    {
        if (string.IsNullOrWhiteSpace(config.ValidationRules))
            return;

        try
        {
            var jToken = JToken.Parse(config.ValidationRules);

            if (jToken.Type == JTokenType.Array)
            {
                var rules = jToken as JArray;
                ValidateValidationRulesArray(rules, result);
            }
            else if (jToken.Type == JTokenType.Object)
            {
                var ruleObj = jToken as JObject;
                ValidateValidationRulesObject(ruleObj, result);
            }
            else
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "VALIDATION_RULES_INVALID_FORMAT",
                    Field = "ValidationRules",
                    Message = "验证规则格式错误",
                    Details = "应该是 JSON 数组或对象"
                });
            }
        }
        catch (JsonException ex)
        {
            result.Errors.Add(new ValidationError
            {
                Code = "VALIDATION_RULES_INVALID_JSON",
                Field = "ValidationRules",
                Message = "验证规则JSON格式错误",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// 验证验证规则数组格式
    /// </summary>
    private void ValidateValidationRulesArray(JArray rules, ValidationResult result)
    {
        for (int i = 0; i < rules.Count; i++)
        {
            var rule = rules[i];
            if (rule.Type == JTokenType.Object)
            {
                ValidateValidationRulesObject(rule as JObject, result, i);
            }
            else
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "VALIDATION_RULE_ITEM_INVALID",
                    Field = $"ValidationRules[{i}]",
                    Message = "验证规则项必须是对象"
                });
            }
        }
    }

    /// <summary>
    /// 验证单个验证规则对象
    /// </summary>
    private void ValidateValidationRulesObject(JObject? rule, ValidationResult result, int index = -1)
    {
        if (rule == null)
            return;

        var fieldPrefix = index >= 0 ? $"ValidationRules[{index}]" : "ValidationRules";

        // 检查是否是新的格式（包含 requiredFields 和/或 fieldValidators）
        var hasRequiredFields = rule["requiredFields"] != null;
        var hasFieldValidators = rule["fieldValidators"] != null;

        if (hasRequiredFields || hasFieldValidators)
        {
            // 验证 requiredFields
            if (hasRequiredFields)
            {
                var requiredFields = rule["requiredFields"];
                if (requiredFields.Type != JTokenType.Array)
                {
                    result.Errors.Add(new ValidationError
                    {
                        Code = "VALIDATION_REQUIRED_FIELDS_NOT_ARRAY",
                        Field = $"{fieldPrefix}.requiredFields",
                        Message = "requiredFields 必须是数组格式"
                    });
                }
                else
                {
                    var array = (JArray)requiredFields;
                    if (!array.Any())
                    {
                        result.Warnings.Add(new ValidationWarning
                        {
                            Field = $"{fieldPrefix}.requiredFields",
                            Message = "requiredFields 数组为空"
                        });
                    }
                    else
                    {
                        foreach (var item in array)
                        {
                            if (item.Type != JTokenType.String)
                            {
                                result.Errors.Add(new ValidationError
                                {
                                    Code = "VALIDATION_REQUIRED_FIELD_NOT_STRING",
                                    Field = $"{fieldPrefix}.requiredFields",
                                    Message = "requiredFields 数组中的每个元素必须是字符串"
                                });
                            }
                        }
                    }
                }
            }

            // 验证 fieldValidators
            if (hasFieldValidators)
            {
                var fieldValidators = rule["fieldValidators"];
                if (fieldValidators.Type != JTokenType.Object)
                {
                    result.Errors.Add(new ValidationError
                    {
                        Code = "VALIDATION_FIELD_VALIDATORS_NOT_OBJECT",
                        Field = $"{fieldPrefix}.fieldValidators",
                        Message = "fieldValidators 必须是对象格式"
                    });
                }
                else
                {
                    var validatorObj = (JObject)fieldValidators;
                    if (!validatorObj.HasValues)
                    {
                        result.Warnings.Add(new ValidationWarning
                        {
                            Field = $"{fieldPrefix}.fieldValidators",
                            Message = "fieldValidators 对象为空"
                        });
                    }
                    else
                    {
                        foreach (var validator in validatorObj.Properties())
                        {
                            var validatorFieldPrefix = $"{fieldPrefix}.fieldValidators.{validator.Name}";

                            if (validator.Value.Type != JTokenType.Object)
                            {
                                result.Errors.Add(new ValidationError
                                {
                                    Code = "VALIDATION_FIELD_VALIDATOR_NOT_OBJECT",
                                    Field = validatorFieldPrefix,
                                    Message = $"字段 '{validator.Name}' 的验证器配置必须是对象格式"
                                });
                                continue;
                            }

                            var validatorConfig = (JObject)validator.Value;
                            var hasType = false;
                            var hasMessage = false;

                            foreach (var prop in validatorConfig.Properties())
                            {
                                var propName = prop.Name.ToLower();

                                // 验证 type
                                if (propName == "type")
                                {
                                    hasType = true;
                                    if (prop.Value.Type != JTokenType.String)
                                    {
                                        result.Errors.Add(new ValidationError
                                        {
                                            Code = "VALIDATION_TYPE_NOT_STRING",
                                            Field = $"{validatorFieldPrefix}.type",
                                            Message = "type 必须是字符串"
                                        });
                                    }
                                    else
                                    {
                                        var typeValue = prop.Value.ToString().ToLower();
                                        var validTypes = new[] { "required", "email", "url", "pattern", "minlength", "maxlength",
                                            "min", "max", "range", "number", "string", "date", "datetime", "enum", "boolean" };
                                        if (!validTypes.Contains(typeValue))
                                        {
                                            result.Warnings.Add(new ValidationWarning
                                            {
                                                Field = $"{validatorFieldPrefix}.type",
                                                Message = $"验证类型 '{prop.Value}' 不是常见类型"
                                            });
                                        }
                                    }
                                }

                                // 验证 message
                                if (propName == "message")
                                {
                                    hasMessage = true;
                                    if (prop.Value.Type != JTokenType.String)
                                    {
                                        result.Errors.Add(new ValidationError
                                        {
                                            Code = "VALIDATION_MESSAGE_NOT_STRING",
                                            Field = $"{validatorFieldPrefix}.message",
                                            Message = "message 必须是字符串"
                                        });
                                    }
                                }

                                // 验证 values (enum 类型需要)
                                if (propName == "values")
                                {
                                    if (prop.Value.Type != JTokenType.Array)
                                    {
                                        result.Errors.Add(new ValidationError
                                        {
                                            Code = "VALIDATION_VALUES_NOT_ARRAY",
                                            Field = $"{validatorFieldPrefix}.values",
                                            Message = "values 必须是数组格式"
                                        });
                                    }
                                }

                                // 验证 min/max (number 类型需要)
                                if (propName == "min" || propName == "max")
                                {
                                    if (prop.Value.Type != JTokenType.Integer && prop.Value.Type != JTokenType.Float)
                                    {
                                        result.Errors.Add(new ValidationError
                                        {
                                            Code = "VALIDATION_MINMAX_NOT_NUMBER",
                                            Field = $"{validatorFieldPrefix}.{prop.Name}",
                                            Message = $"{prop.Name} 必须是数字"
                                        });
                                    }
                                }

                                // 验证 minLength/maxLength
                                if (propName == "minlength" || propName == "maxlength")
                                {
                                    if (prop.Value.Type != JTokenType.Integer)
                                    {
                                        result.Errors.Add(new ValidationError
                                        {
                                            Code = "VALIDATION_LENGTH_NOT_INTEGER",
                                            Field = $"{validatorFieldPrefix}.{prop.Name}",
                                            Message = $"{prop.Name} 必须是整数"
                                        });
                                    }
                                }

                                // 验证 pattern
                                if (propName == "pattern")
                                {
                                    if (prop.Value.Type != JTokenType.String)
                                    {
                                        result.Errors.Add(new ValidationError
                                        {
                                            Code = "VALIDATION_PATTERN_NOT_STRING",
                                            Field = $"{validatorFieldPrefix}.pattern",
                                            Message = "pattern 必须是字符串"
                                        });
                                    }
                                }
                            }

                            // 至少需要 type
                            if (!hasType)
                            {
                                result.Warnings.Add(new ValidationWarning
                                {
                                    Field = validatorFieldPrefix,
                                    Message = $"字段 '{validator.Name}' 的验证器配置缺少 type 属性"
                                });
                            }
                        }
                    }
                }
            }

            // 至少需要 requiredFields 或 fieldValidators 中的一个
            if (!hasRequiredFields && !hasFieldValidators)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Field = fieldPrefix,
                    Message = "验证规则不包含任何有效配置，建议添加 requiredFields 或 fieldValidators"
                });
            }

            return;
        }

        // 旧格式：单个规则对象（包含 field 和 type）
        var field = rule["field"]?.ToString();
        if (string.IsNullOrEmpty(field))
        {
            result.Warnings.Add(new ValidationWarning
            {
                Field = fieldPrefix,
                Message = "验证规则配置不标准，建议使用 requiredFields/fieldValidators 格式"
            });
            return;
        }

        // 必须有 type 或 rule 属性
        var type = rule["type"]?.ToString();
        var ruleValue = rule["rule"]?.ToString();

        if (string.IsNullOrEmpty(type) && string.IsNullOrEmpty(ruleValue))
        {
            result.Errors.Add(new ValidationError
            {
                Code = "VALIDATION_RULE_NO_TYPE",
                Field = fieldPrefix,
                Message = "验证规则缺少 'type' 或 'rule' 属性"
            });
        }

        // 验证 type 值
        if (!string.IsNullOrEmpty(type))
        {
            var validTypes = new[] { "required", "email", "url", "pattern", "minLength", "maxLength", "min", "max", "range" };
            if (!validTypes.Contains(type.ToLower()))
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Field = fieldPrefix,
                    Message = $"验证类型 '{type}' 不是常见类型"
                });
            }
        }
    }

    /// <summary>
    /// 建议正确的类型名称
    /// </summary>
    private List<string> SuggestCorrectTypeName(string typeName)
    {
        var suggestions = new List<string>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // 计算编辑距离
        foreach (var assembly in assemblies.Take(10)) // 只检查前10个程序集以提高性能
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsPublic && !t.IsNested)
                    .Select(t => t.FullName)
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();

                foreach (var type in types)
                {
                    var distance = LevenshteinDistance(typeName, type);
                    if (distance <= 3 && distance > 0) // 编辑距离小于等于3
                    {
                        suggestions.Add(type);
                    }
                }
            }
            catch
            {
                // 某些程序集可能无法加载
            }
        }

        return suggestions.Distinct().Take(5).ToList();
    }

    /// <summary>
    /// 计算编辑距离（Levenshtein Distance）
    /// </summary>
    private int LevenshteinDistance(string s1, string s2)
    {
        var d = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
            d[i, 0] = i;

        for (int j = 0; j <= s2.Length; j++)
            d[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[s1.Length, s2.Length];
    }
}
