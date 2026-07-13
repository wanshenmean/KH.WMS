namespace KH.WMS.Modules.SystemModule.DTOs;

/// <summary>
/// 单条日志（解析自 Serilog 文本行）
/// </summary>
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    /// <summary>级别缩写：INF/WRN/ERR/FAT</summary>
    public string Level { get; set; } = string.Empty;
    /// <summary>模块代码（WMS 模块枚举值，常为空）</summary>
    public string ModuleCode { get; set; } = string.Empty;
    /// <summary>日志类型：System/Business/Api 等</summary>
    public string LogType { get; set; } = string.Empty;
    public string? TenantId { get; set; }
    public string? UserId { get; set; }
    /// <summary>请求追踪 ID（同一次请求的多条日志共享）</summary>
    public string? RequestId { get; set; }
    public string Message { get; set; } = string.Empty;
    /// <summary>异常堆栈（多行，可能为空）</summary>
    public string? Exception { get; set; }
    /// <summary>原始整行（调试/展开用）</summary>
    public string? RawLine { get; set; }
}

/// <summary>
/// 日志查询条件
/// </summary>
public class LogQueryRequest
{
    /// <summary>开始日期（含），默认近 3 天</summary>
    public DateTime? StartDate { get; set; }
    /// <summary>结束日期（含），默认今天</summary>
    public DateTime? EndDate { get; set; }
    /// <summary>级别筛选（INF/WRN/ERR/FAT），为空则不限</summary>
    public List<string>? Levels { get; set; }
    /// <summary>日志类型模糊匹配（如 System/Business）</summary>
    public string? LogType { get; set; }
    /// <summary>消息内容关键字（同时匹配异常堆栈）</summary>
    public string? Keyword { get; set; }
    /// <summary>按 RequestId 追踪（链路）</summary>
    public string? RequestId { get; set; }
    public string? UserId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// 日志查询结果（分页）
/// </summary>
public class LogQueryResult
{
    public List<LogEntry> Items { get; set; } = new();
    public int Total { get; set; }
}

// ========== 文件浏览模式 ==========

/// <summary>日志文件信息（供前端文件树）</summary>
public class LogFileInfo
{
    /// <summary>分类：全部日志/错误日志/警告日志/模块日志</summary>
    public string Category { get; set; } = string.Empty;
    /// <summary>文件名（相对 Logs 目录，如 log-20260625.txt 或 wms/Inbound-20260625.txt）</summary>
    public string FileName { get; set; } = string.Empty;
    /// <summary>日期（来自文件名）</summary>
    public string Date { get; set; } = string.Empty;
    /// <summary>文件大小（KB）</summary>
    public long SizeKB { get; set; }
}

/// <summary>原始日志内容读取请求</summary>
public class LogContentRequest
{
    /// <summary>文件名列表（多选时按顺序拼接）</summary>
    public List<string>? FileNames { get; set; }
    /// <summary>起始行（0 基，过滤后）</summary>
    public int StartLine { get; set; } = 0;
    /// <summary>取的行数</summary>
    public int LineCount { get; set; } = 500;
    /// <summary>关键字（行内包含匹配，同时匹配异常堆栈续行）</summary>
    public string? Keyword { get; set; }
    /// <summary>级别过滤（INF/WRN/ERR/FAT），按行首 [XXX] 匹配</summary>
    public List<string>? Levels { get; set; }

    /// <summary>日志类型过滤（如 System/Business/Api），按 [LogType:xxx] 匹配</summary>
    public string? LogType { get; set; }

    /// <summary>RequestId 追踪，按 [RequestId:xxx] 匹配</summary>
    public string? RequestId { get; set; }

    /// <summary>模块代码（如 2001=入库/2002=出库），按行内 [2001] 匹配</summary>
    public string? ModuleCode { get; set; }

    /// <summary>时间范围开始（含），按行首时间戳匹配，为空则不限</summary>
    public DateTime? StartTime { get; set; }

    /// <summary>时间范围结束（含），按行首时间戳匹配，为空则不限</summary>
    public DateTime? EndTime { get; set; }
}

/// <summary>原始日志内容读取结果</summary>
public class LogContentResult
{
    public List<LogLine> Lines { get; set; } = new();
    /// <summary>是否还有更多（用于前端"加载更多"）</summary>
    public bool HasMore { get; set; }
}

/// <summary>单行原始日志</summary>
public class LogLine
{
    /// <summary>全局行号（过滤后，从1开始）</summary>
    public long LineNo { get; set; }
    /// <summary>来源文件（拼接时标注）</summary>
    public string? SourceFile { get; set; }
    /// <summary>原始内容（去 BOM）</summary>
    public string Content { get; set; } = string.Empty;
}
