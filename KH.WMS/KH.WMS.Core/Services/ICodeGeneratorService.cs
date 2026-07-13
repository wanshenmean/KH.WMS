namespace KH.WMS.Core.Services;

/// <summary>
/// 编码生成服务接口
/// 根据 CfgCodeRule 规则生成各类业务编码（入库单号、出库单号、物料编码等）
/// </summary>
public interface ICodeGeneratorService
{
    /// <summary>
    /// 根据规则类型生成编码
    /// </summary>
    /// <param name="ruleType">规则类型（如 INBOUND_DOC、OUTBOUND_DOC、MATERIAL 等）</param>
    /// <returns>生成的编码字符串</returns>
    Task<string> GenerateAsync(string ruleType);
}
