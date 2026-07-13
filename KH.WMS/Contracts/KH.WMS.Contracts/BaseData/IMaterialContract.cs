namespace KH.WMS.Contracts.BaseData;

/// <summary>
/// 物料跨模块接口契约
/// 供 Inbound 等模块查询物料信息
/// BaseDataModule 负责实现
/// </summary>
public interface IMaterialContract
{
    /// <summary>
    /// 根据物料编码获取物料信息
    /// </summary>
    Task<MaterialInfo?> GetByCodeAsync(string materialCode);

    /// <summary>
    /// 根据物料编码批量获取物料信息
    /// </summary>
    Task<List<MaterialInfo>> GetByCodesAsync(List<string> materialCodes);
}
