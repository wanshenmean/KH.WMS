using KH.WMS.Core.Api.Responses;
using KH.WMS.Modules.SystemModule.DTOs;
using KH.WMS.Modules.SystemModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.SystemModule.Controllers;

/// <summary>
/// 系统文本日志查询（读取 Serilog 文件日志，供前端展示/搜索）
/// </summary>
[ApiController]
[Route("api/system-log")]
public class SystemLogController(ILogQueryService logQueryService) : ControllerBase
{
    private readonly ILogQueryService _logQueryService = logQueryService;

    /// <summary>按条件查询文本日志（分页）</summary>
    [HttpPost("query")]
    public async Task<ApiResponse> Query([FromBody] LogQueryRequest request)
    {
        var result = await _logQueryService.QueryAsync(request ?? new LogQueryRequest());
        return ApiResponse.Ok(result);
    }

    /// <summary>获取可用日志日期列表（降序）</summary>
    [HttpGet("dates")]
    public async Task<ApiResponse> GetDates()
    {
        var dates = await _logQueryService.GetLogDatesAsync();
        return ApiResponse.Ok(dates);
    }

    /// <summary>获取所有日志文件列表（供前端文件树：分类+文件名+日期+大小）</summary>
    [HttpGet("files")]
    public async Task<ApiResponse> GetFiles()
    {
        var files = await _logQueryService.GetLogFilesAsync();
        return ApiResponse.Ok(files);
    }

    /// <summary>
    /// 读取原始日志内容（多文件拼接 + 分页 + 关键字/级别过滤）。
    /// 请求体: { fileNames: [...], startLine, lineCount, keyword?, levels? }
    /// 返回: { lines: [{ lineNo, sourceFile, content }], hasMore }
    /// </summary>
    [HttpPost("content")]
    public async Task<ApiResponse> GetContent([FromBody] LogContentRequest request)
    {
        var result = await _logQueryService.GetLogContentAsync(request ?? new LogContentRequest());
        return ApiResponse.Ok(result);
    }
}
