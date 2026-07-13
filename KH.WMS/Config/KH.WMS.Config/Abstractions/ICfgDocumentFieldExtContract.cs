

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 单据扩展字段服务
    /// 负责配置查询、前端 formColumn 转换、ExtData JSON 序列化/反序列化
    /// </summary>
    public interface ICfgDocumentFieldExtContract
    {
        /// <summary>
        /// 根据单据类型编码获取扩展字段配置
        /// </summary>
        Task<List<CfgDocumentField>> GetFieldsAsync(string docTypeCode, string fieldLevel = "HEADER");

        /// <summary>
        /// 将 CfgDocumentField 转换为前端 KhForm 的 column 配置
        /// </summary>
        List<Dictionary<string, object?>> BuildFormColumns(List<CfgDocumentField> fields);

        /// <summary>
        /// 从数据中提取扩展字段值，序列化为 ExtData JSON
        /// </summary>
        string? SerializeExtData(Dictionary<string, object?> allData, List<CfgDocumentField> fields);

        /// <summary>
        /// 将 ExtData JSON 反序列化为 Dictionary（所有字段）
        /// </summary>
        Dictionary<string, object?> DeserializeExtData(string? extDataJson);

        /// <summary>
        /// 反序列化 ExtData 并只提取 IsProcessable=1 的业务字段
        /// </summary>
        Task<Dictionary<string, object?>> DeserializeProcessableExtDataAsync(string docTypeCode, string? extDataJson, string fieldLevel = "HEADER");

        /// <summary>
        /// 清除指定单据类型的配置缓存
        /// </summary>
        void ClearCache(string docTypeCode);
    }
}
