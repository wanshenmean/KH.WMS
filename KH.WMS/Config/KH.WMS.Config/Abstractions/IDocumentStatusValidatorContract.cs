

namespace KH.WMS.Config.Abstractions;

/// <summary>
/// 单据状态验证器契约
/// 供各业务模块校验单据状态转换合法性、操作权限等
/// ConfigModule 负责实现
/// </summary>
public interface IDocumentStatusValidatorContract
{
    /// <summary>
    /// 校验状态转换是否合法，不合法时抛出异常
    /// </summary>
    Task ValidateTransitionAsync(string docTypeCode, string fromStatus, string toStatus);

    /// <summary>
    /// 判断状态转换是否合法（不抛异常）
    /// </summary>
    Task<bool> CanTransitionAsync(string docTypeCode, string fromStatus, string toStatus);

    /// <summary>
    /// 获取指定单据类型下某状态码的配置信息
    /// </summary>
    Task<CfgDocumentStatus?> GetStatusConfigAsync(string docTypeCode, string status);

    /// <summary>
    /// 获取指定单据类型下当前状态允许的目标状态列表
    /// </summary>
    Task<List<string>> GetAllowedTransitionsAsync(string docTypeCode, string fromStatus);

    /// <summary>
    /// 校验当前状态是否允许编辑，不允许时抛出异常
    /// </summary>
    Task ValidateAllowEditAsync(string docTypeCode, string currentStatus);

    /// <summary>
    /// 校验当前状态是否允许删除，不允许时抛出异常
    /// </summary>
    Task ValidateAllowDeleteAsync(string docTypeCode, string currentStatus);

    /// <summary>
    /// 获取指定单据类型的初始状态编码
    /// </summary>
    Task<string?> GetInitialStatusAsync(string docTypeCode);
}
