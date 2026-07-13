using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Models.Dtos;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Core.Controllers;

/// <summary>
/// 支持 ExtData 扩展字段的 CRUD 控制器基类
/// 继承 CrudController，在 Create/Update 时从请求体提取 extDataRaw 写入实体的 ExtData，
/// 在 GetById/GetPagedList 时将 ExtData JSON 反序列化展开为扁平属性
/// </summary>
/// <typeparam name="TEntity">实体类型，须包含 string? ExtData 属性</typeparam>
public abstract class ExtDataCrudController<TEntity>(ICrudService<TEntity> service) : CrudController<TEntity>(service)
    where TEntity : BaseEntity<long>, new()
{
    private static readonly PropertyInfo? ExtDataProp =
        typeof(TEntity).GetProperty("ExtData", BindingFlags.Public | BindingFlags.Instance);

    /// <summary>
    /// 获取实体的 ExtData 属性值
    /// </summary>
    private static string? GetExtData(TEntity entity)
    {
        return ExtDataProp?.GetValue(entity) as string;
    }

    /// <summary>
    /// 设置实体的 ExtData 属性值
    /// </summary>
    private static void SetExtData(TEntity entity, string? value)
    {
        ExtDataProp?.SetValue(entity, value);
    }

    /// <summary>
    /// 将 ExtData JSON 字符串展开合并到 JsonNode 中
    /// </summary>
    private static void FlattenExtData(string? extDataJson, JsonObject node)
    {
        if (string.IsNullOrEmpty(extDataJson)) return;
        try
        {
            var extObj = JsonNode.Parse(extDataJson) as JsonObject;
            if (extObj == null) return;
            foreach (var prop in extObj)
            {
                if (!node.ContainsKey(prop.Key))
                    node[prop.Key] = prop.Value?.DeepClone();
            }
        }
        catch
        {
            // ExtData 格式异常时忽略
        }
    }

    /// <summary>
    /// 从请求体原始 JSON 中提取 extDataRaw 并设置到实体
    /// 依赖 Program.cs 中的 EnableBuffering 中间件，Body 流可回溯重读
    /// </summary>
    private async Task ExtractExtDataFromRequest(TEntity entity)
    {
        if (!HttpContext.Request.Body.CanRead) return;
        try
        {
            HttpContext.Request.Body.Position = 0;
            using var reader = new StreamReader(HttpContext.Request.Body, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            HttpContext.Request.Body.Position = 0;

            if (!string.IsNullOrEmpty(rawBody))
            {
                var node = JsonNode.Parse(rawBody) as JsonObject;
                if (node != null && node.ContainsKey("extDataRaw") && node["extDataRaw"] != null)
                {
                    SetExtData(entity, node["extDataRaw"]!.GetValue<string>());
                }
            }
        }
        catch
        {
            // 回退到默认行为
        }
    }

    /// <summary>
    /// 新增 — 从请求体提取 extDataRaw，写入实体的 ExtData
    /// </summary>
    [HttpPost("create")]
    public override async Task<ApiResponse> Create([FromBody] TEntity entity)
    {
        await ExtractExtDataFromRequest(entity);
        return await service.CreateAsync(entity);
    }

    /// <summary>
    /// 更新 — 同新增，提取 extDataRaw 写入 ExtData
    /// </summary>
    [HttpPost("update")]
    public override async Task<ApiResponse> Update([FromBody] TEntity entity)
    {
        await ExtractExtDataFromRequest(entity);
        return await service.UpdateAsync(entity);
    }

    /// <summary>
    /// 根据 ID 获取详情 — 展开 ExtData 为扁平属性（编辑/查看回显）
    /// 分页查询的展平由前端 load 函数处理
    /// </summary>
    [HttpGet("{id}")]
    public override async Task<ApiResponse> GetById(long id)
    {
        var result = await service.GetByIdAsync(id);
        if (result.Data is TEntity entity)
        {
            var json = JsonSerializer.SerializeToNode(entity) as JsonObject ?? new JsonObject();
            FlattenExtData(GetExtData(entity), json);
            result.Data = json;
        }
        return result;
    }
}
