using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace KH.WMS.Engines.DataMap;

/// <summary>
/// 数据映射配置服务（带验证）
/// </summary>
[SelfRegisteredService(Lifetime = ServiceLifetime.Scoped)]
public class DataMappingConfigValidationService
{
    private readonly ILogger<DataMappingConfigValidationService> _logger;
    private readonly DataMappingConfigValidator _validator;
    private readonly IRepository<IntDataMappingConfig, long> _repository;

    public DataMappingConfigValidationService(IRepository<IntDataMappingConfig, long> repository,
        ILogger<DataMappingConfigValidationService> logger)
    {
        _repository = repository;
        _logger = logger;
        _validator = new DataMappingConfigValidator();
    }

    /// <summary>
    /// 创建映射配置（带验证）
    /// </summary>
    /// <param name="config">配置对象</param>
    /// <param name="autoFix">是否自动修复可修复的错误</param>
    /// <returns>创建结果</returns>
    public async Task<(bool Success, string? Message, IntDataMappingConfig? Data)> CreateWithValidationAsync(
        IntDataMappingConfig config, bool autoFix = false)
    {
        try
        {
            // 1. 执行验证
            var validationResult = _validator.Validate(config, autoFix);

            // 2. 如果有错误，返回错误信息
            if (!validationResult.IsValid)
            {
                var errorMsg = "配置验证失败:\n" + string.Join("\n",
                    validationResult.Errors.Select(e => $"  [{e.Code}] {e.Field}: {e.Message}"));

                if (validationResult.Suggestions.Any())
                {
                    errorMsg += "\n\n修复建议:\n" + string.Join("\n",
                        validationResult.Suggestions.Select(s => $"  - {s.Description}"));
                }

                return (false, errorMsg, null);
            }

            // 3. 如果有警告，记录日志
            if (validationResult.Warnings.Any())
            {
                foreach (var warning in validationResult.Warnings)
                {
                    _logger.LogWarning("配置验证警告: {Field} - {Message}",
                        warning.Field, warning.Message);
                }
            }

            // 4. 检查编码是否已存在
            var exists = await _repository.ExistsAsync(x => x.MappingCode == config.MappingCode);

            if (exists)
            {
                return (false, $"映射编码 {config.MappingCode} 已存在", null);
            }

            // 5. 设置默认值
            config.CreatedTime = DateTime.Now;
            if (string.IsNullOrEmpty(config.ScriptType))
                config.ScriptType = DataMapConstants.ScriptLanguage.CSHARP;

            // 6. 插入数据库
            await _repository.AddAsync(config);

            _logger.LogInformation("创建数据映射配置成功: {MappingCode}", config.MappingCode);

            return (true, "创建成功", config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建数据映射配置失败: {MappingCode}", config.MappingCode);
            return (false, $"创建失败: {ex.Message}", null);
        }
    }

    /// <summary>
    /// 更新映射配置（带验证）
    /// </summary>
    /// <param name="config">配置对象</param>
    /// <param name="autoFix">是否自动修复可修复的错误</param>
    /// <returns>更新结果</returns>
    public async Task<(bool Success, string? Message)> UpdateWithValidationAsync(
        IntDataMappingConfig config, bool autoFix = false)
    {
        try
        {
            // 1. 执行验证
            var validationResult = _validator.Validate(config, autoFix);

            // 2. 如果有错误，返回错误信息
            if (!validationResult.IsValid)
            {
                var errorMsg = "配置验证失败:\n" + string.Join("\n",
                    validationResult.Errors.Select(e => $"  [{e.Code}] {e.Field}: {e.Message}"));

                if (validationResult.Suggestions.Any())
                {
                    errorMsg += "\n\n修复建议:\n" + string.Join("\n",
                        validationResult.Suggestions.Select(s => $"  - {s.Description}"));
                }

                return (false, errorMsg);
            }

            // 3. 如果有警告，记录日志
            if (validationResult.Warnings.Any())
            {
                foreach (var warning in validationResult.Warnings)
                {
                    _logger.LogWarning("配置验证警告: {Field} - {Message}",
                        warning.Field, warning.Message);
                }
            }

            // 4. 更新数据库
            config.LastModifiedTime = DateTime.Now;
            await _repository.UpdateAsync(config);

            _logger.LogInformation("更新数据映射配置成功: {MappingCode}", config.MappingCode);

            return (true, "更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新数据映射配置失败: {MappingCode}", config.MappingCode);
            return (false, $"更新失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 仅验证配置（不保存）
    /// </summary>
    /// <param name="config">配置对象</param>
    /// <param name="autoFix">是否自动修复可修复的错误</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateOnly(IntDataMappingConfig config, bool autoFix = false)
    {
        return _validator.Validate(config, autoFix);
    }

    /// <summary>
    /// 仅验证转换脚本
    /// </summary>
    /// <param name="transformScript">转换脚本内容</param>
    /// <param name="scriptType">脚本类型（可选，默认 CSHARP）</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateTransformScript(string transformScript, string? scriptType = null)
    {
        return _validator.ValidateTransformScriptOnly(transformScript, scriptType);
    }

    /// <summary>
    /// 仅验证映射规则
    /// </summary>
    /// <param name="mappingRulesJson">映射规则JSON字符串</param>
    /// <param name="autoFix">是否自动修复可修复的错误</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateMappingRules(string mappingRulesJson, bool autoFix = false)
    {
        return _validator.ValidateMappingRulesOnly(mappingRulesJson, autoFix);
    }

    /// <summary>
    /// 仅验证验证规则
    /// </summary>
    /// <param name="validationRulesJson">验证规则JSON字符串</param>
    /// <returns>验证结果</returns>
    public ValidationResult ValidateValidationRules(string validationRulesJson)
    {
        return _validator.ValidateValidationRulesOnly(validationRulesJson);
    }
}
