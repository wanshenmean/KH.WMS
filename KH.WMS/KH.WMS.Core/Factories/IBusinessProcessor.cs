using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KH.WMS.Core.Factories;

/// <summary>
/// 业务处理器接口
/// </summary>
public interface IBusinessProcessor
{
    /// <summary>
    /// 处理器类型
    /// </summary>
    string ProcessorType { get; }

    /// <summary>
    /// 处理业务逻辑
    /// </summary>
    Task<(bool Success, object? Data, string? ErrorMessage)> ProcessAsync(
        JToken jsonData,
        IServiceProvider serviceProvider);
}


