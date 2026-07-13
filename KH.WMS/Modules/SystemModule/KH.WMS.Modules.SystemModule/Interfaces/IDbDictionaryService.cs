using KH.WMS.Modules.SystemModule.DTOs;

namespace KH.WMS.Modules.SystemModule.Interfaces;

/// <summary>
/// 数据库字典服务：反射读 SqlSugar 实体特性，生成数据字典
/// </summary>
public interface IDbDictionaryService
{
    /// <summary>获取数据库字典（JSON 结构）</summary>
    Task<List<TableDictionary>> GetAsync();

    /// <summary>导出 Excel（返回字节）</summary>
    Task<byte[]> ExportAsync();
}
