using KH.WMS.Core.Api.Responses;
using KH.WMS.Modules.SystemModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.SystemModule.Controllers;

/// <summary>
/// 数据库字典（表结构/列信息）
/// </summary>
[ApiController]
[Route("api/system-db")]
public class DbDictionaryController(IDbDictionaryService dbDictionaryService) : ControllerBase
{
    private readonly IDbDictionaryService _dbDictionaryService = dbDictionaryService;

    /// <summary>获取数据库字典（JSON：表+列结构）</summary>
    [HttpGet("dictionary")]
    public async Task<ApiResponse> Get()
    {
        var tables = await _dbDictionaryService.GetAsync();
        return ApiResponse.Ok(tables);
    }

    /// <summary>导出数据库字典 Excel（返回 Base64）</summary>
    [HttpGet("dictionary/export")]
    public async Task<ApiResponse> Export()
    {
        var bytes = await _dbDictionaryService.ExportAsync();
        return ApiResponse.Ok(Convert.ToBase64String(bytes), "数据库字典导出成功");
    }
}
