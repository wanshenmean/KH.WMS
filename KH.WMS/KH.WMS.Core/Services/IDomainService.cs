using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.Services;

/// <summary>
/// 领域服务接口
/// </summary>
public interface IDomainService
{
}

/// <summary>
/// 领域服务基类
/// </summary>
public abstract class DomainService : IDomainService
{
    protected readonly ILogger _logger;

    protected DomainService(ILogger logger)
    {
        _logger = logger;
    }
}

/// <summary>
/// 业务逻辑验证结果
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();

    public static ValidationResult Success()
    {
        return new ValidationResult { IsValid = true };
    }

    public static ValidationResult Fail(string error)
    {
        return new ValidationResult
        {
            IsValid = false,
            Errors = new List<string> { error }
        };
    }

    public static ValidationResult Fail(List<string> errors)
    {
        return new ValidationResult
        {
            IsValid = false,
            Errors = errors
        };
    }

    public void AddError(string error)
    {
        IsValid = false;
        Errors.Add(error);
    }
}
