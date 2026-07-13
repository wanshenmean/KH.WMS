using System.Collections.Generic;

namespace KH.WMS.Core.Logging;

/// <summary>
/// 请求级日志缓冲：在请求期间收集方法参数/SQL 等详情，
/// 异常时由 GlobalExceptionFilter flush 到 LogError（Error 级别，不受 MinimumLevel 限制）。
/// 正常请求结束时丢弃缓冲区，零文件 IO。
/// </summary>
public static class ErrorLogScope
{
    private static readonly AsyncLocal<List<string>?> _buffer = new();

    /// <summary>请求开始时创建缓冲区</summary>
    public static void Begin()
    {
        _buffer.Value = new List<string>();
    }

    /// <summary>追加一条详情日志（方法参数/SQL等），仅在缓冲作用域内有效</summary>
    public static void Append(string entry)
    {
        _buffer.Value?.Add(entry);
    }

    /// <summary>取出缓冲区全部内容并清空（异常时调用，交给 LogError 写入文件）</summary>
    public static List<string>? Flush()
    {
        var buffer = _buffer.Value;
        _buffer.Value = null;
        return buffer;
    }

    /// <summary>丢弃缓冲区（请求正常结束时调用）</summary>
    public static void Clear()
    {
        _buffer.Value = null;
    }
}
