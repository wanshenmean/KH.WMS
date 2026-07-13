namespace KH.WMS.Core.Api.Responses;

/// <summary>
/// 分页请求参数
/// </summary>
public class Pagination
{
    private int _pageIndex = 1;
    private int _pageSize = 20;

    /// <summary>
    /// 页码（从1开始）
    /// </summary>
    public int PageIndex
    {
        get => _pageIndex;
        set => _pageIndex = value < 1 ? 1 : value;
    }

    /// <summary>
    /// 每页数量
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 20 : value;
    }

    /// <summary>
    /// 计算跳过数量
    /// </summary>
    public int Skip => (PageIndex - 1) * PageSize;

    /// <summary>
    /// 获取数量
    /// </summary>
    public int Take => PageSize;

    /// <summary>
    /// 创建分页参数
    /// </summary>
    public static Pagination Create(int pageIndex = 1, int pageSize = 20)
    {
        return new Pagination
        {
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }
}

/// <summary>
/// 分页结果
/// </summary>
public class PagedResult<T>
{
    /// <summary>
    /// 数据列表
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// 总记录数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// 每页数量
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int PageCount => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevious => PageIndex > 1;

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNext => PageIndex < PageCount;

    /// <summary>
    /// 创建分页结果
    /// </summary>
    public static PagedResult<T> Create(List<T> items, int total, int pageIndex, int pageSize)
    {
        return new PagedResult<T>
        {
            Items = items,
            Total = total,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 创建空分页结果
    /// </summary>
    public static PagedResult<T> Empty(int pageIndex = 1, int pageSize = 20)
    {
        return new PagedResult<T>
        {
            Items = new List<T>(),
            Total = 0,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }
}

/// <summary>
/// 分页响应包装
/// </summary>
public class PagedResponse<T> : ApiResponse<PagedResult<T>>
{
    /// <summary>
    /// 创建成功分页响应
    /// </summary>
    public static PagedResponse<T> Ok(List<T> items, int total, int pageIndex, int pageSize, string message = "查询成功")
    {
        return new PagedResponse<T>
        {
            Code = ResponseCode.SUCCESS,
            Message = message,
            Data = PagedResult<T>.Create(items, total, pageIndex, pageSize)
        };
    }

    /// <summary>
    /// 创建空分页响应
    /// </summary>
    public static PagedResponse<T> Empty(int pageIndex = 1, int pageSize = 20, string message = "暂无数据")
    {
        return new PagedResponse<T>
        {
            Code = ResponseCode.SUCCESS,
            Message = message,
            Data = PagedResult<T>.Empty(pageIndex, pageSize)
        };
    }
}
