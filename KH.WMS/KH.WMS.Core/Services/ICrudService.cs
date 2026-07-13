using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Models.Dtos;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Core.Services;

/// <summary>
/// CRUD 应用服务接口
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public interface ICrudService<TEntity>
    where TEntity : BaseEntity<long>, new()
{
    /// <summary>
    /// 根据 ID 获取详情
    /// </summary>
    Task<ApiResponse> GetByIdAsync(long id);

    /// <summary>
    /// 分页查询
    /// </summary>
    Task<ApiResponse> GetPagedListAsync(AdvancedQueryRequestDto query);

    /// <summary>
    /// 获取全部列表（下拉选择等场景）
    /// </summary>
    Task<ApiResponse> GetListAsync();

    /// <summary>
    /// 新增
    /// </summary>
    Task<ApiResponse> CreateAsync(TEntity entity);

    /// <summary>
    /// 更新
    /// </summary>
    Task<ApiResponse> UpdateAsync(TEntity entity);

    /// <summary>
    /// 删除
    /// </summary>
    Task<ApiResponse> DeleteAsync(long id);

    /// <summary>
    /// 批量删除
    /// </summary>
    Task<ApiResponse> BatchDeleteAsync(List<long> ids);

    /// <summary>
    /// 设置启用/禁用状态（仅 IEnableDisableEntity 实体可用）
    /// </summary>
    Task<ApiResponse> SetStatusAsync(long id, byte status);

    /// <summary>
    /// 导出
    /// </summary>
    Task<ApiResponse> ExportAsync(AdvancedQueryRequestDto query, List<ExportColumnDto>? columns = null, bool exportAll = true);

    /// <summary>
    /// 导入
    /// </summary>
    Task<ApiResponse> ImportAsync(Stream fileStream, string fileName);

    /// <summary>
    /// 下载导入模板
    /// </summary>
    Task<ApiResponse> DownloadTemplateAsync();
}
