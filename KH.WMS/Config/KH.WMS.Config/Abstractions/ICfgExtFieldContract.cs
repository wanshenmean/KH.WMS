

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 通用扩展字段服务
    /// 用于非单据类实体（库存操作、基础数据等）的扩展字段配置
    /// 职责与 ICfgDocumentFieldExtContract 对称，数据源为 CfgExtField
    /// </summary>
    public interface ICfgExtFieldContract
    {
        /// <summary>
        /// 根据实体编码获取扩展字段配置
        /// </summary>
        Task<List<CfgExtField>> GetFieldsAsync(string entityCode, string fieldLevel = "HEADER");

        /// <summary>
        /// 将 CfgExtField 转换为前端 KhForm 的 column 配置
        /// </summary>
        List<Dictionary<string, object?>> BuildFormColumns(List<CfgExtField> fields);

        /// <summary>
        /// 从数据中提取扩展字段值，序列化为 ExtData JSON
        /// </summary>
        string? SerializeExtData(Dictionary<string, object?> allData, List<CfgExtField> fields);

        /// <summary>
        /// 将 ExtData JSON 反序列化为 Dictionary（所有字段）
        /// </summary>
        Dictionary<string, object?> DeserializeExtData(string? extDataJson);

        /// <summary>
        /// 反序列化 ExtData 并只提取 IsProcessable=1 的业务字段
        /// </summary>
        Task<Dictionary<string, object?>> DeserializeProcessableExtDataAsync(string entityCode, string? extDataJson, string fieldLevel = "HEADER");

        /// <summary>
        /// 清除指定实体编码的配置缓存
        /// </summary>
        void ClearCache(string entityCode);
    }
}
