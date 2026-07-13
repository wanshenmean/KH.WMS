using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Attributes;
using KH.WMS.Core.Models.Dtos;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Core.Controllers;

/// <summary>
/// CRUD 控制器基类
/// 自动提供标准 CRUD + 导入导出端点，子类无需重复定义
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
[ApiController]
public abstract class CrudController<TEntity> : ControllerBase
    where TEntity : BaseEntity<long>, new()
{
    private readonly ICrudService<TEntity> _service;

    protected CrudController(ICrudService<TEntity> service)
    {
        _service = service;
    }

    /// <summary>
    /// 根据 ID 获取详情
    /// </summary>
    [HttpGet("{id}"), Cache(Enable = false)]
    public virtual async Task<ApiResponse> GetById(long id)
    {
        return await _service.GetByIdAsync(id);
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    [HttpPost("pagelist"), Cache(Enable = false)]
    public virtual async Task<ApiResponse> GetPagedList(AdvancedQueryRequestDto query)
    {
        return await _service.GetPagedListAsync(query);
    }

    /// <summary>
    /// 获取全部列表（下拉选择等场景）
    /// </summary>
    [HttpGet("all")]
    public virtual async Task<ApiResponse> GetAll()
    {
        return await _service.GetListAsync();
    }

    /// <summary>
    /// 新增
    /// </summary>
    [HttpPost("create"), Cache(Enable = false)]
    public virtual async Task<ApiResponse> Create([FromBody] TEntity entity)
    {
        return await _service.CreateAsync(entity);
    }

    /// <summary>
    /// 更新
    /// </summary>
    [HttpPost("update"), Cache(Enable = false)]
    public virtual async Task<ApiResponse> Update([FromBody] TEntity entity)
    {
        return await _service.UpdateAsync(entity);
    }

    /// <summary>
    /// 删除
    /// </summary>
    [HttpDelete("delete/{id}"), Cache(Enable = false)]
    public virtual async Task<ApiResponse> Delete(long id)
    {
        return await _service.DeleteAsync(id);
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    [HttpDelete("batch"), Cache(Enable = false)]
    public virtual async Task<ApiResponse> BatchDelete([FromBody] List<long> ids)
    {
        return await _service.BatchDeleteAsync(ids);
    }

    /// <summary>
    /// 设置启用/禁用状态（仅 IEnableDisableEntity 实体可用）
    /// </summary>
    [HttpPut("status/{id}"), Cache(Enable = false)]
    public virtual async Task<ApiResponse> SetStatus(long id, [FromBody] SetStatusDto dto)
    {
        return await _service.SetStatusAsync(id, dto.Status);
    }

    /// <summary>
    /// 导出
    /// </summary>
    [HttpPost("export"), Cache(Enable = false)]
    public virtual async Task<ApiResponse> Export([FromBody] ExportRequestDto request)
    {
        return await _service.ExportAsync(request, request?.Columns, request?.ExportAll ?? true);
    }

    /// <summary>
    /// 导入
    /// </summary>
    [HttpPost("import"), Cache(Enable = false)]
    public virtual async Task<ApiResponse> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "请选择要导入的文件");

        await using var stream = file.OpenReadStream();
        return await _service.ImportAsync(stream, file.FileName);
    }

    /// <summary>
    /// 下载导入模板
    /// </summary>
    [HttpGet("template"), Cache(Enable = false)]
    public virtual async Task<ApiResponse> DownloadTemplate()
    {
        return await _service.DownloadTemplateAsync();
    }
}
