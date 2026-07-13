using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Api.Responses;

namespace KH.WMS.Core.Models.Dtos;

/// <summary>
/// DTO 基类
/// </summary>
public abstract class BaseDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    
    public long Id { get; set; }
}

/// <summary>
/// 创建请求 DTO 基类
/// </summary>
public abstract class CreateRequestDto
{
}

/// <summary>
/// 更新请求 DTO 基类
/// </summary>
public abstract class UpdateRequestDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    
    public long Id { get; set; }
}

/// <summary>
/// 删除请求 DTO 基类
/// </summary>
public class DeleteRequestDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    
    public long Id { get; set; }
}

/// <summary>
/// 批量删除请求 DTO
/// </summary>
public class BatchDeleteRequestDto
{
    /// <summary>
    /// ID列表
    /// </summary>
    
    public List<long> Ids { get; set; } = new();

    /// <summary>
    /// 是否物理删除
    /// </summary>
    public bool IsPhysicalDelete { get; set; } = false;
}

/// <summary>
/// 查询请求 DTO 基类
/// </summary>
public abstract class QueryRequestDto : Pagination
{
    /// <summary>
    /// 搜索关键字
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}

/// <summary>
/// 排序条件
/// </summary>
public class SortCondition
{
    /// <summary>
    /// 排序字段
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// 排序方向（asc / desc）
    /// </summary>
    public string Direction { get; set; } = "asc";
}

/// <summary>
/// 过滤条件
/// </summary>
public class FilterCondition
{
    /// <summary>
    /// 字段名
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// 操作符（equals/contains/gt/lt/gte/lte/in/notnull/isnull/startswith/endwith）
    /// </summary>
    public string Operator { get; set; } = "contains";

    /// <summary>
    /// 过滤值
    /// </summary>
    public object? Value { get; set; }
}

/// <summary>
/// 高级查询请求 DTO 基类
/// </summary>
public class AdvancedQueryRequestDto : Pagination
{
    /// <summary>
    /// 搜索关键字（子类在 BuildQueryExpression 中自行决定匹配哪些字段）
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 多字段排序条件
    /// </summary>
    public List<SortCondition>? SortConditions { get; set; }

    /// <summary>
    /// 动态过滤条件
    /// </summary>
    public List<FilterCondition>? Filters { get; set; }
}

/// <summary>
/// 导出列配置（从前端传入，用于生成中文表头和字典值翻译）
/// </summary>
public class ExportColumnDto
{
    /// <summary>字段名（对应实体属性）</summary>
    public string Prop { get; set; } = "";

    /// <summary>中文列标题</summary>
    public string Label { get; set; } = "";

    /// <summary>字典映射（value → label），可选，用于将原始值翻译为中文标签</summary>
    public Dictionary<string, string>? DictMap { get; set; }
}

/// <summary>
/// 设置启用/禁用状态请求 DTO
/// </summary>
public class SetStatusDto
{
    /// <summary>
    /// 状态值（1=启用 0=禁用）
    /// </summary>
    public byte Status { get; set; }
}

/// <summary>
/// 导出请求参数
/// </summary>
public class ExportRequestDto : AdvancedQueryRequestDto
{
    /// <summary>导出列配置（中文表头 + 字典映射），为空时直接导出实体</summary>
    public List<ExportColumnDto>? Columns { get; set; }

    /// <summary>是否导出全部数据（true 跳过分页，false 仅导出当前页）</summary>
    public bool ExportAll { get; set; } = true;
}
