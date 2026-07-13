using MiniExcelLibs;

namespace KH.WMS.Core.ImportExport;

/// <summary>
/// Excel 导入导出静态工具类（基于 MiniExcel）
/// </summary>
public static class ExcelHelper
{
    /// <summary>
    /// 导出数据为 Excel 字节数组
    /// </summary>
    public static async Task<byte[]> ExportAsync<T>(IEnumerable<T> data, string? sheetName = null)
    {
        using var memoryStream = new MemoryStream();

        if (sheetName == null)
        {
            await memoryStream.SaveAsAsync(data);
        }
        else
        {
            var sheets = new Dictionary<string, object>
            {
                { sheetName, data }
            };
            await memoryStream.SaveAsAsync(sheets);
        }

        return memoryStream.ToArray();
    }

    /// <summary>
    /// 从 Excel 流导入数据
    /// </summary>
    public static List<T> ImportAsync<T>(Stream stream) where T : class, new()
    {
        return stream.Query<T>().ToList();
    }

    /// <summary>
    /// 生成导入模板（仅包含表头，无数据行）
    /// </summary>
    public static async Task<byte[]> GenerateTemplateAsync<T>(string? sheetName = null) where T : class, new()
    {
        using var memoryStream = new MemoryStream();

        var emptyData = new List<T>();

        if (sheetName == null)
        {
            await memoryStream.SaveAsAsync(emptyData);
        }
        else
        {
            var sheets = new Dictionary<string, object>
            {
                { sheetName, emptyData }
            };
            await memoryStream.SaveAsAsync(sheets);
        }

        return memoryStream.ToArray();
    }
}
