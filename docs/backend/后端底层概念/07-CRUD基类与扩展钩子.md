---
title: "07 CRUD 基类与扩展钩子"
description: "07 CRUD 基类与扩展钩子：说明适用场景、当前实现、设计边界与开发或排障入口。"
status: reference
audience: "后端开发人员、排障人员与底座维护者"
reviewed: "2026-07-14"
sourcePaths:
  - "KH.WMS/KH.WMS.Server"
  - "KH.WMS/KH.WMS.Core"
  - "KH.WMS/Modules"
---

# 07 CRUD 基类与扩展钩子

## 这个概念解决什么问题

CRUD 基类解决的是“标准增删改查不要每个模块重复写”的问题。它把 Controller 端点、Service 流程、查询、排序、分页、事务、审计字段、主从表保存、状态启停、导入导出统一起来。

业务开发要做的是：

- Controller 继承 `CrudController<TEntity>`。
- Service 继承 `CrudService<TEntity>`。
- 业务差异写到 Service 钩子里。

这样可以保持接口风格一致，也能让事务、日志、导入导出、主从表保存走统一底座。

## 什么时候需要看

- 新增一个普通维护页面、基础资料页面、配置页面。
- 需要定制创建、更新、删除前后的业务校验。
- 需要追加查询条件、默认排序、结果加工。
- 需要主从表保存。
- 需要启用/禁用状态。
- 导入导出结果不符合预期。
- 不确定应该重写 Controller 还是 Service。

## 业务开发应该怎么用

### Controller 优先继承基类

普通 CRUD Controller 只需要指定路由并注入服务：

```csharp
[Route("api/location")]
public class LocationController(ILocationService service)
    : CrudController<MdLocation>(service)
{
}
```

基类已经提供这些端点：

| 端点 | 方法 | 作用 |
| --- | --- | --- |
| `GET {id}` | `GetById` | 按 ID 查询详情 |
| `POST pagelist` | `GetPagedList` | 高级筛选、排序、分页 |
| `GET all` | `GetAll` | 获取全部列表 |
| `POST create` | `Create` | 新增 |
| `POST update` | `Update` | 更新 |
| `DELETE delete/{id}` | `Delete` | 删除 |
| `DELETE batch` | `BatchDelete` | 批量删除 |
| `PUT status/{id}` | `SetStatus` | 启用/禁用 |
| `POST export` | `Export` | 导出 |
| `POST import` | `Import` | 导入 |
| `GET template` | `DownloadTemplate` | 下载导入模板 |

只有新增非标准接口时，才在 Controller 子类里加 Action。

### Service 继承 `CrudService<TEntity>`

```csharp
[RegisteredService(ServiceType = typeof(ILocationService))]
public class LocationService(
    IRepository<MdLocation, long> repository,
    IUnitOfWork unitOfWork,
    IDetailSaveService detailSaveService)
    : CrudService<MdLocation>(repository, unitOfWork, detailSaveService),
      ILocationService
{
}
```

### 业务差异写钩子

常用钩子：

| 需求 | 推荐钩子 |
| --- | --- |
| 新增前唯一性校验、默认值补充 | `BeforeCreateAsync` |
| 新增后写日志、派生数据 | `AfterCreateAsync` |
| 更新前状态校验、唯一性校验 | `BeforeUpdateAsync` |
| 更新后同步派生数据 | `AfterUpdateAsync` |
| 删除前检查引用 | `BeforeDeleteAsync` |
| 删除后清理辅助数据 | `AfterDeleteAsync` |
| 批量删除前校验 | `BeforeBatchDeleteAsync` |
| 分页查询追加业务过滤 | `BuildQueryExpression` |
| 复杂联查或追加 Queryable 条件 | `ApplyAdditionalQuery` |
| 查询结果二次加工 | `AfterQueryAsync` |
| 默认排序 | `ApplyDefaultSorting` |
| 导出前校验 | `BeforeExportAsync` |
| 导出数据转换 | `TransformExportData` |
| 导入前清洗 | `BeforeImportAsync` |
| 导入后汇总 | `AfterImportAsync` |
| 状态启停前校验 | `BeforeSetStatusAsync` |

不要为了加一个校验去重写 Controller 的 `Create`，优先重写 `BeforeCreateAsync`。

## 底层逻辑和实现

### 查询流程

`GetPagedListAsync` 的执行顺序：

1. `BuildQueryExpression(query)` 生成业务基础条件。
2. `ExpressionHelper.BuildFilter<TEntity>(query.Filters)` 生成前端高级筛选条件。
3. 两个表达式 `And` 合并。
4. `_repository.AsQueryable().Where(...)` 构造查询。
5. `ApplyAdditionalQuery(queryable, query)` 追加联查或复杂条件。
6. `ApplySorting(queryable, query.SortConditions)` 应用前端排序或默认排序。
7. `ToPageListAsync` 分页。
8. `AfterQueryAsync(query, items)` 查询后加工。
9. 返回 `ApiResponse.Ok(new { items, total })`。

### 新增流程

`CreateAsync` 的执行顺序：

1. 开启事务。
2. `BeforeCreateAsync(entity)`。
3. `FillAuditFields(entity, isCreate: true)`。
4. 插入主表并返回主键。
5. `DetailSaveService.SaveDetailsAsync` 保存 OneToMany 明细。
6. `DetailSaveService.SaveOneToOneAsync` 保存 OneToOne 明细。
7. `AfterCreateAsync(entity)`。
8. 提交事务。
9. 异常时回滚并重新抛出。

### 更新流程

`UpdateAsync` 的执行顺序：

1. 开启事务。
2. 按 ID 获取旧实体。
3. `BeforeUpdateAsync(entity)`。
4. `CopyProperties` 把非空普通字段复制到旧实体。
5. `CopyDetailProperties` 复制主从表导航属性。
6. 填充修改时间。
7. 更新主表。
8. 保存 OneToMany 和 OneToOne 明细。
9. `AfterUpdateAsync(existing)`。
10. 提交事务。

注意：`CopyProperties` 只复制非空值，且跳过主键和级联导航属性。这会影响“把某字段更新为 null”的需求，遇到这种情况要单独处理。

### 删除流程

`DeleteAsync` 会判断实体是否有级联导航属性：

- 有 OneToMany 或 OneToOne 导航属性时，用 `GetByIdWithNavAsync` 加载完整实体，再 `DeleteWithNavAsync`。
- 没有级联导航属性时，只删主表。

删除前后会执行 `BeforeDeleteAsync` 和 `AfterDeleteAsync`。

### 主从表保存

`DetailSaveService` 会扫描实体上配置了 `[Navigate]` 的属性：

- `NavigateType.OneToMany` 且类型是 `List<T>`：逐条新增或更新明细，更新时会删除本次未提交的旧明细。
- `NavigateType.OneToOne`：保存单个子实体。
- `NavigateType.ManyToOne`：视为外部引用，不参与级联保存。

如果实体上有 `List<T>` 但没加 `[Navigate]`，会记录 warning 并跳过。

### 状态启停

`SetStatusAsync` 只对实现 `IEnableDisableEntity` 的实体生效。状态字段查找优先级：

1. 实体上的 `[StatusFieldName]` 指定字段。
2. `Status`。
3. `IsActive`。

状态字段类型支持 `byte` 和 `int`。

### 导入导出

导出：

- 使用查询流程取数。
- 如果前端传了列配置，按中文表头、字典映射、ExtData 展开导出。
- 如果没传列配置，直接导出实体。

导入：

- 使用 `ExcelHelper.ImportAsync<TEntity>` 读取。
- 每行调用 `CreateAsync`。
- 单行失败会记录错误并继续下一行。

## 钩子执行时能做什么，不能做什么

| 钩子 | 可以做 | 不建议做 |
| --- | --- | --- |
| `BeforeCreateAsync` | 唯一性校验、默认值补充、业务前置检查 | 开新事务、写大量无关表 |
| `AfterCreateAsync` | 写派生记录、触发同事务内后续数据 | 调外部不可回滚接口 |
| `BeforeUpdateAsync` | 状态校验、禁止编辑校验、字段合法性校验 | 直接修改旧实体之外的大范围数据 |
| `AfterUpdateAsync` | 同步明细汇总、刷新状态 | 发送不可回滚消息 |
| `BeforeDeleteAsync` | 引用检查、状态检查 | 先删其他数据再让主删除继续 |
| `AfterDeleteAsync` | 清理同事务辅助数据 | 依赖已被删实体再次查询完整导航 |
| `AfterQueryAsync` | 字典翻译、补充展示字段 | 再执行大量 N+1 查询 |
| `BeforeImportAsync` | 清洗 Excel 行、批量预校验 | 对每行单独查库造成慢导入 |

如果某个钩子里要调用外部系统、WCS、消息队列，要意识到数据库事务回滚不了外部副作用。更稳妥的做法是提交后异步补偿或设计幂等。

## 主从表保存细节

### OneToMany 更新怎么判断新增、更新、删除

`DetailSaveService.SaveDetailsAsync` 的规则：

1. 扫描主实体上 `[Navigate(NavigateType.OneToMany)]` 的 `List<TDetail>` 属性。
2. 根据 `Navigate` 的外键名找到明细外键属性。
3. 每个提交的明细都设置外键为主表 `entity.Id`。
4. 明细 `Id == 0` 时新增。
5. 明细 `Id != 0` 时更新。
6. 更新主表时，查询数据库已有明细 ID。
7. 本次没有提交的旧明细会被删除。

这意味着：更新主从表时，前端提交的明细列表代表“最终明细集合”，不是“增量补丁”。如果前端漏传某条旧明细，后端会把它删掉。

### OneToOne 保存规则

`SaveOneToOneAsync` 扫描 `[Navigate(NavigateType.OneToOne)]`：

- 子实体为空则跳过。
- 设置子实体外键。
- 子实体 `Id == 0` 时新增。
- 子实体 `Id != 0` 时更新。

### ManyToOne 为什么不保存

`ManyToOne` 表示当前实体引用外部实体，例如仓库、物料、客户。CRUD 基类不会级联保存这些外部主数据，避免一个业务单据更新时误改基础资料。

## 字段复制细节

`UpdateAsync` 不是直接把前端实体全量 update，而是：

1. 先从数据库加载 existing。
2. 把 source 非空普通属性复制到 existing。
3. 跳过主键。
4. 跳过级联导航属性。
5. 单独复制明细导航属性。
6. 更新 existing。

结果是：

- 未传字段不会覆盖数据库旧值。
- 传 null 不能清空字段。
- 清空字段需要重写 `CopyProperties` 或在 `BeforeUpdateAsync` / `AfterUpdateAsync` 做专门处理。

这是便利也是限制。编辑页面如果要支持清空字段，要明确处理。

## 查询扩展落点选择

| 需求 | 推荐位置 |
| --- | --- |
| 所有查询都要过滤租户/仓库 | `BuildQueryExpression` |
| 按前端高级筛选组合字段 | 前端 `filters` + `ExpressionHelper` |
| 需要 join 其他表 | `ApplyAdditionalQuery` |
| 默认按业务字段排序 | `ApplyDefaultSorting` |
| 查询后补充展示字段 | `AfterQueryAsync` |
| 导出时字段翻译 | `TransformExportData` 或列配置 |

不要把分页查询整段复制出来重写，除非基类流程已经无法表达需求。

## 最小示例

### 新增前校验唯一性

```csharp
protected override async Task BeforeCreateAsync(MdLocation entity)
{
    var exists = await _repository.ExistsAsync(x => x.LocationCode == entity.LocationCode);
    if (exists)
        throw new BusinessException($"货位编码 {entity.LocationCode} 已存在");
}
```

### 查询只看当前仓库

```csharp
protected override Expression<Func<MdLocation, bool>> BuildQueryExpression(AdvancedQueryRequestDto query)
{
    return x => x.WarehouseId == CurrentWarehouseId;
}
```

### 默认按创建时间倒序

```csharp
protected override ISugarQueryable<MdLocation> ApplyDefaultSorting(ISugarQueryable<MdLocation> queryable)
{
    return queryable.OrderBy(x => x.CreatedTime, OrderByType.Desc);
}
```

## 常见误区与排查清单

- 在 Controller 重写标准 CRUD，只为加校验：应优先使用 Service 钩子。
- 在 `BeforeCreateAsync` 里自己提交事务：CRUD 外层已经开了事务。
- 主从表没保存：检查 `[Navigate]`、集合类型是否 `List<T>`、外键名是否正确。
- 更新时字段清空不生效：`CopyProperties` 默认跳过 null，需要定制处理。
- 状态接口返回“不支持启用禁用”：实体是否实现 `IEnableDisableEntity`。
- 默认排序不符合页面预期：重写 `ApplyDefaultSorting`。
- 导入部分成功部分失败：这是当前设计，查看返回的 `errors` 列表。

## 继续阅读

- [底层机制索引](/backend/后端底层概念/README)
- [后端 V3 教程](/backend/后端开发指引V3教程/README)
- [后端排错与日志追踪](/backend/KH.WMS后端排错与日志追踪指引)
