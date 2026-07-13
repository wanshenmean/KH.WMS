# 08 ExtData 动态字段

## 这个概念解决什么问题

ExtData 动态字段解决的是“业务实体字段不固定，但又不想频繁改表结构”的问题。

典型场景：

- 不同客户需要不同扩展字段。
- 不同单据类型需要不同头字段或明细字段。
- 页面表单字段由配置生成。
- 某些扩展字段只用于展示，某些扩展字段需要参与业务处理。

KH.WMS 的设计是：

- 实体保留一个 `string? ExtData` 字段，用 JSON 存扩展值。
- 前端提交时把扩展字段值合并成 `extDataRaw`。
- `ExtDataCrudController<TEntity>` 从原始请求体提取 `extDataRaw`，写入实体 `ExtData`。
- 详情查询时把 `ExtData` 展平成普通 JSON 属性，便于前端回显。
- 扩展字段定义由 Config 模块的 Contract 查询和转换。

## 什么时候需要看

- 新实体需要动态字段。
- 前端提交了扩展字段，但数据库 `ExtData` 没值。
- 详情接口返回了 `ExtData` 字符串，但页面字段没回显。
- 不知道普通 CRUD 和 ExtData CRUD 怎么选。
- 需要按单据类型或实体编码获取动态字段配置。
- 业务流程需要读取扩展字段参与处理。

## 业务开发应该怎么用

### 什么时候选普通 CRUD

选择 `CrudController<TEntity>`：

- 字段固定。
- 字段需要频繁参与查询、排序、索引、统计。
- 字段是核心业务语义，例如物料编码、批次、数量、状态。
- 字段需要强类型约束和数据库约束。

### 什么时候选 ExtData CRUD

选择 `ExtDataCrudController<TEntity>`：

- 字段由配置决定。
- 字段只是附加信息或客户化字段。
- 不同单据类型或不同实体编码字段不同。
- 字段一般不作为高频查询索引。

核心字段不要滥用 ExtData，否则后续查询、统计、校验都会变复杂。

### 实体需要什么

实体必须有公开的 `ExtData` 属性：

```csharp
public string? ExtData { get; set; }
```

`ExtDataCrudController` 通过反射查找这个属性。如果没有，提交不会报错，但扩展字段也不会保存。

### Controller 怎么写

```csharp
[Route("api/demo")]
public class DemoController(IDemoService service)
    : ExtDataCrudController<DemoEntity>(service)
{
}
```

只要继承 `ExtDataCrudController<TEntity>`，`create`、`update`、`get by id` 会自动增强。

### `GET /form-config` 是拿来干什么的

`[HttpGet("form-config")] public async Task<IActionResult> GetFormConfig()` 是“给前端拿动态字段配置”的接口。它不是保存接口，也不负责读取某条业务数据的 `ExtData` 值。

它解决的是前端渲染问题：

- 页面打开时，不知道有哪些客户化字段。
- 新增/编辑弹窗需要把配置字段插到基础字段里。
- 表格需要把可展示的扩展字段插到基础列里。
- 主从单据需要同时知道头表扩展字段和明细行扩展字段。

后端返回的是字段元数据，不是字段值。典型响应结构是：

```json
{
  "success": true,
  "data": {
    "columns": [
      {
        "prop": "customerBatch",
        "label": "客户批次",
        "type": "input",
        "isExt": true,
        "required": true
      }
    ],
    "lineColumns": [
      {
        "prop": "temperature",
        "label": "温度要求",
        "type": "input",
        "isExt": true
      }
    ]
  }
}
```

`columns` 表示头表/实体级扩展字段，前端合并到主表单或主表格列；`lineColumns` 表示明细行扩展字段，前端合并到可编辑明细表列。没有明细行的页面通常只返回 `columns`。

`BuildFormColumns` 会把配置表里的字段定义转换成前端 `KhForm` / `KhTable` 能识别的列配置：

| 配置字段 | 前端列配置 | 含义 |
| --- | --- | --- |
| `FieldKey` | `prop` | 前端表单字段名，也是 `ExtData` JSON 的 key |
| `FieldName` | `label` | 页面显示名称 |
| `FieldType` | `type` | 控件类型，`STRING -> input`、`INT/DECIMAL -> number`、`DATETIME -> date`、`BOOLEAN -> switch` |
| `IsRequired` | `required` | 是否必填 |
| `DefaultValue` | `defaultValue` | 默认值 |
| 固定值 | `isExt: true` | 标记这是扩展字段，便于前端提取和清理 |

也就是说，`form-config` 的职责只有一个：把配置库里的字段定义翻译成前端可以渲染的 columns。

### `form-config` 后端怎么写

非单据类实体，例如物料、客户、供应商、库存明细，按实体编码获取字段配置，使用 `ICfgExtFieldContract`。当前 `MaterialController` 就是这个模式：

```csharp
[Route("api/material")]
public class MaterialController(IMaterialService materialService)
    : ExtDataCrudController<MdMaterial>(materialService)
{
    [HttpGet("form-config")]
    public async Task<IActionResult> GetFormConfig()
    {
        var extService = HttpContext.RequestServices
            .GetRequiredService<ICfgExtFieldContract>();

        var fields = await extService.GetFieldsAsync("MD_MATERIAL", "HEADER");
        var columns = extService.BuildFormColumns(fields);

        return Ok(new { success = true, data = new { columns } });
    }
}
```

这个接口实际路由是 `GET /api/material/form-config`。`"MD_MATERIAL"` 必须能在 `cfg_ext_field_type.EntityCode` 中找到，否则返回空 columns。

单据类实体，例如采购入库单、销售出库单，按单据类型编码获取字段配置，使用 `ICfgDocumentFieldExtContract`。当前 `InboundOrderController` 是这个模式：

```csharp
[HttpGet("form-config")]
public async Task<IActionResult> GetFormConfig()
{
    var extService = HttpContext.RequestServices
        .GetRequiredService<ICfgDocumentFieldExtContract>();

    var fields = await extService.GetFieldsAsync(
        BizConstants.OrderTypes.PURCHASE_IN,
        "HEADER");
    var columns = extService.BuildFormColumns(fields);

    var lineFields = await extService.GetFieldsAsync(
        BizConstants.OrderTypes.PURCHASE_IN,
        "LINE");
    var lineColumns = extService.BuildFormColumns(lineFields);

    return Ok(new { success = true, data = new { columns, lineColumns } });
}
```

这里的 `BizConstants.OrderTypes.PURCHASE_IN` 必须能在 `cfg_document_type.TypeCode` 中找到，`HEADER` 字段会绑定到单据头 `ExtData`，`LINE` 字段会绑定到明细行 `ExtData`。

### `form-config` 前端怎么用

前端有两种使用方式。

第一种是标准 ExtData 页面，推荐显式使用 `useExtFields('/api/xxx/form-config')`。这是物料、客户、供应商页面正在使用的方式：

```js
import { useExtFields } from '@/utils/useExtFields'
import { useCrudApi } from '@/utils/crud'

const crudApi = useCrudApi('material')

const {
  loadExtConfig,
  mergedColumns,
  mergedTableColumns,
  extractAndCleanExtData,
  withFlatExtLoad,
} = useExtFields('/api/material/form-config')

onMounted(() => {
  loadExtConfig()
})

const tableColumns = computed(() => mergedTableColumns(baseTableColumns))
const formColumns = computed(() => mergedColumns(baseFormColumns))
const load = withFlatExtLoad(crudApi, searchColumns)

const beforeSubmit = (data) => {
  const raw = extractAndCleanExtData(data)
  if (raw) data.extDataRaw = raw
}
```

这段前端代码做了四件事：

1. `loadExtConfig()` 调用 `/api/material/form-config`，把后端返回的 `columns` 存到前端状态。
2. `mergedColumns(baseFormColumns)` 把扩展字段插入新增/编辑表单。
3. `mergedTableColumns(baseTableColumns)` 把扩展字段插入表格列。
4. `beforeSubmit` 调用 `extractAndCleanExtData(data)`，把扩展字段从表单对象中摘出来，序列化成 `data.extDataRaw`。

`extractAndCleanExtData` 里的“clean”很关键：扩展字段不能作为实体普通属性直接提交，否则模型绑定或后端实体更新时会出现“实体没有这个属性”的语义混乱。正确提交方式是普通字段留在 `data` 上，扩展字段集中放进 `extDataRaw`。

第二种是 `KhPage` 的内置 CRUD 模式。`KhPage` 在传了 `module` 且没有传 `formColumns` 时，会尝试调用 `useCrudApi(module).formConfig()`，也就是 `GET /api/{module}/form-config`。但当前 ExtData 页面通常需要同时合并表单列、表格列、分页列表展平，所以更推荐显式使用 `useExtFields`，再把合并后的 `formColumns`、`tableColumns`、`load` 传给 `KhPage`。

### 前端保存时两种 `extDataRaw`

标准 `ExtDataCrudController<TEntity>` 页面，`extDataRaw` 是 JSON 字符串：

```json
{
  "materialCode": "MAT001",
  "materialName": "测试物料",
  "extDataRaw": "{\"customerBatch\":\"B20260708\"}"
}
```

这是因为 `ExtDataCrudController` 会从原始请求体读取 `extDataRaw`，然后直接写入实体的 `string? ExtData`。

主从单据页面不走 `ExtDataCrudController.Create/Update` 的标准端点，而是走自定义 DTO，例如 `InboundOrderCreateDto`。这种场景里前端传的是对象，由 Controller 手动序列化：

```json
{
  "order": {
    "orderType": "PURCHASE_IN",
    "warehouseId": 1
  },
  "lines": [
    {
      "lineNo": 1,
      "materialCode": "MAT001",
      "orderedQty": 10
    }
  ],
  "extDataRaw": {
    "customerBatch": "B20260708"
  },
  "lineExtDataRaw": {
    "1": {
      "temperature": "2-8C"
    }
  }
}
```

后端 `ApplyExtDataAsync` 会把 `ExtDataRaw` 序列化到 `request.Order.ExtData`，再按 `LineNo` 把 `LineExtDataRaw` 序列化到每一行的 `line.ExtData`。所以不要把单据 DTO 页面和标准 `ExtDataCrudController` 页面混成一种提交格式。

### 前端提交格式

提交时应包含 `extDataRaw`，值是 JSON 字符串：

```json
{
  "id": 1001,
  "docNo": "IN202607070001",
  "warehouseId": 1,
  "extDataRaw": "{\"customerBatch\":\"B20260707\",\"temperature\":\"2-8C\"}"
}
```

后端会把 `extDataRaw` 的值写入实体 `ExtData` 字段。

### 详情返回格式

实体里保存：

```json
{
  "customerBatch": "B20260707",
  "temperature": "2-8C"
}
```

`GetById` 返回时会把这些扩展字段展平到响应对象上：

```json
{
  "id": 1001,
  "docNo": "IN202607070001",
  "warehouseId": 1,
  "extData": "{\"customerBatch\":\"B20260707\",\"temperature\":\"2-8C\"}",
  "customerBatch": "B20260707",
  "temperature": "2-8C"
}
```

如果实体本身已有同名属性，展平时不会覆盖实体原属性。

## 底层逻辑和实现

### 为什么能读取 `extDataRaw`

模型绑定会先读取请求体生成 `TEntity`。正常情况下请求体流已经读完，不能再次读取。

`Program.cs` 中启用了：

```csharp
context.Request.EnableBuffering();
```

所以 `ExtDataCrudController` 可以把 body position 重置到 0，读取原始 JSON：

```csharp
HttpContext.Request.Body.Position = 0;
var rawBody = await reader.ReadToEndAsync();
HttpContext.Request.Body.Position = 0;
```

然后解析 `extDataRaw`，写入实体 `ExtData`。

### Create/Update 做了什么

`ExtDataCrudController` 覆盖了：

- `Create`：先 `ExtractExtDataFromRequest(entity)`，再调用 `service.CreateAsync(entity)`。
- `Update`：先 `ExtractExtDataFromRequest(entity)`，再调用 `service.UpdateAsync(entity)`。

真正的事务、审计字段、主从表保存仍然由 `CrudService` 负责。

### GetById 做了什么

`GetById` 调用原 Service 查询实体后：

1. 把实体序列化成 `JsonObject`。
2. 解析实体 `ExtData` JSON。
3. 遍历扩展字段键值。
4. 如果响应对象还没有同名属性，就加入展平字段。

分页查询没有在后端基类里统一展平，当前注释里说明分页展平由前端 load 函数处理。

### 扩展字段配置 Contract

Config 模块提供两套扩展字段 Contract：

| Contract | 先查什么 | 再查什么 | 适用场景 | 配置维度 |
| --- | --- | --- | --- | --- |
| `ICfgExtFieldContract` | `CfgExtFieldType.EntityCode` | `CfgExtField.EntityTypeId + FieldLevel` | 非单据实体，例如物料、客户、供应商、库存明细、库存移动 | “这个实体/页面”有哪些扩展字段 |
| `ICfgDocumentFieldExtContract` | `CfgDocumentType.TypeCode` | `CfgDocumentField.DocTypeId + FieldLevel` | 单据类实体，例如采购入库、销售出库、生产领料 | “这个单据类型”有哪些扩展字段 |

它们提供的能力类似：

- `GetFieldsAsync`：按实体编码或单据类型获取字段定义。
- `BuildFormColumns`：转换成前端 KhForm 的 column 配置。
- `SerializeExtData`：从完整数据字典里抽取扩展字段并序列化。
- `DeserializeExtData`：把 `ExtData` JSON 还原成字典。
- `DeserializeProcessableExtDataAsync`：只提取 `IsProcessable=1` 的字段。
- `ClearCache`：清理字段配置缓存。

选择规则不要写成“有明细就用 `ICfgDocumentFieldExtContract`”。两套 Contract 都支持 `HEADER` / `LINE`，真正的判断标准是字段配置跟谁绑定：

- 字段跟实体类型绑定，用 `ICfgExtFieldContract`。例如 `MD_MATERIAL`、`MD_CUSTOMER`、`INV_MOVEMENT` 这类页面，字段是“这个实体固定有哪些扩展字段”。
- 字段跟单据类型绑定，用 `ICfgDocumentFieldExtContract`。例如入库单里采购入库、生产入库字段不同，出库单里销售出库、生产领料字段不同，字段是“这个单据类型有哪些扩展字段”。
- 同一张业务表如果会承载多个业务类型，并且不同类型字段差异明显，应优先考虑 `ICfgDocumentFieldExtContract`，否则后续会在 `EntityCode` 下面堆很多条件判断。
- 一个基础资料页面即使有从表，只要字段不是按单据类型变化，仍然应使用 `ICfgExtFieldContract`。

底层查询逻辑也不同：

```csharp
// ICfgExtFieldContract
var entityType = await extFieldTypeRepository
    .GetFirstOrDefaultAsync(e => e.EntityCode == entityCode);

var fields = await extFieldRepository.GetListAsync(
    f => f.EntityTypeId == entityType.Id && f.FieldLevel == fieldLevel);
```

```csharp
// ICfgDocumentFieldExtContract
var docType = await docTypeRepository
    .GetFirstOrDefaultAsync(d => d.TypeCode == docTypeCode);

var fields = await docFieldRepository.GetListAsync(
    f => f.DocTypeId == docType.Id && f.FieldLevel == fieldLevel);
```

这也是为什么 `GetFieldsAsync("MD_MATERIAL", "HEADER")` 和 `GetFieldsAsync("PURCHASE_IN", "HEADER")` 不能混用：一个参数会去 `cfg_ext_field_type.EntityCode` 里找，另一个参数会去 `cfg_document_type.TypeCode` 里找。

两套实现都会把结果缓存 2 小时，缓存 key 是“编码 + FieldLevel”。配置维护服务在新增、修改、删除字段后会调用 `ClearCache`：

- `CfgExtFieldConfigService` 会按 `EntityTypeId` 找回 `EntityCode`，清理 `HEADER` / `LINE` 缓存。
- `CfgDocumentFieldService` 会按 `DocTypeId` 找回 `TypeCode`，清理 `HEADER` / `LINE` 缓存。

如果数据库配置已经改了，但 `/form-config` 返回还是旧字段，优先查配置维护服务是否走了标准 CRUD 钩子，或者临时重启/清缓存验证。

### `IsProcessable` 的意义

不是所有扩展字段都应该参与业务处理。比如备注、展示标签可能只用于页面显示。

当业务流程需要读取扩展字段，例如计算策略参数、校验特殊规则时，应优先用 `DeserializeProcessableExtDataAsync` 取出配置为可处理的字段，避免把展示字段误当业务参数。

## 保存链路拆解

一次 ExtData 新增请求会经历：

1. 前端按动态表单配置渲染扩展字段。
2. 用户填写普通字段和扩展字段。
3. 前端把扩展字段收集成 JSON 字符串 `extDataRaw`。
4. 请求进入 `Program.cs`，`EnableBuffering` 让 body 可重读。
5. MVC 模型绑定把普通字段绑定成 `TEntity`。
6. `ExtDataCrudController.Create` 覆盖基类方法。
7. `ExtractExtDataFromRequest` 把 body position 重置到 0。
8. 解析原始 JSON，读取 `extDataRaw`。
9. 通过反射把 `extDataRaw` 写入实体 `ExtData` 属性。
10. 调用 `service.CreateAsync(entity)`。
11. `CrudService` 开事务、保存实体。
12. 数据库中 `ExtData` 列保存 JSON 字符串。

任何一步断掉都会导致扩展字段没保存。排查时不要只看 Service，Controller 覆盖和请求体 buffering 同样关键。

### 主从单据保存链路有什么不同

入库单、出库单这类主从单据不是简单地把整个实体提交给 `ExtDataCrudController<TEntity>.Create`。它们有自定义 DTO：

```csharp
public class InboundOrderCreateDto
{
    public InboundOrder Order { get; set; } = new();
    public List<InboundOrderLine> Lines { get; set; } = new();
    public Dictionary<string, object?>? ExtDataRaw { get; set; }
    public Dictionary<string, Dictionary<string, object?>?>? LineExtDataRaw { get; set; }
}
```

所以保存链路变成：

1. 前端调用 `/api/inbound-order/form-config`，拿到 `columns` 和 `lineColumns`。
2. 前端把 `columns` 合并到单据头表单，把 `lineColumns` 合并到明细编辑表格。
3. 保存时，单据头扩展字段收集成 `extDataRaw` 对象。
4. 保存时，明细行扩展字段按行号收集成 `lineExtDataRaw` 对象。
5. Controller 的 `ApplyExtDataAsync` 读取 `ICfgDocumentFieldExtContract`。
6. `SerializeExtData(request.ExtDataRaw, headerFields)` 写入 `request.Order.ExtData`。
7. 遍历 `request.Lines`，按 `LineNo` 找 `request.LineExtDataRaw`，再写入每行 `line.ExtData`。
8. Service 保存主表和明细表。

这里不依赖 `ExtDataCrudController` 重读请求体，因为请求体已经被绑定成 `InboundOrderCreateDto`。如果你新写主从接口，就要显式设计 `ExtDataRaw` / `LineExtDataRaw`，并在 Controller 或 Service 入口处序列化；只继承 `CrudController<TEntity>` 不会自动处理这些 DTO 里的扩展字段。

## 回显链路拆解

详情回显时：

1. 前端调用 `GET {id}`。
2. `ExtDataCrudController.GetById` 调用 `service.GetByIdAsync(id)`。
3. Service 通常通过 `GetByIdWithNavAsync` 查实体。
4. Controller 把实体序列化为 `JsonObject`。
5. 读取实体 `ExtData` 字符串。
6. 尝试解析为 `JsonObject`。
7. 遍历扩展字段。
8. 如果响应 JSON 中没有同名属性，则添加扩展字段。
9. 返回包含普通字段、`extData` 原字符串、展平扩展字段的对象。

如果 `ExtData` JSON 格式坏了，当前实现会 catch 并忽略，不会让接口失败。因此“接口正常但字段不回显”时，要检查数据库里的 JSON 是否合法。

### 前端列表和详情为什么处理不同

`ExtDataCrudController.GetById` 会把单条详情里的 `ExtData` 展平成普通字段，所以标准详情接口能直接回显扩展字段。

分页列表不同。当前后端基类没有统一覆盖 `GetPagedList` 来展开每一行，前端通过 `useExtFields.withFlatExtLoad` 处理：

```js
const load = withFlatExtLoad(crudApi, searchColumns)
```

内部逻辑是先调用 `crudApi.pageList`，再对每一行执行：

```js
const extData = typeof row.extData === 'string'
  ? JSON.parse(row.extData)
  : row.extData

Object.assign(row, extData)
```

所以“详情能看到扩展字段，列表看不到”时，不一定是后端没保存，先检查页面是否用了 `withFlatExtLoad`，表格列是否用了 `mergedTableColumns(baseTableColumns)`。

主从单据详情也不同。入库单、出库单用自定义 `GetDetail` 返回 DTO，Service 会把头表 `ExtData` 反序列化到 `ExtDataFlattened`，把每行 `ExtData` 反序列化到 `LineExtDataFlattened`。前端再用 `mergeExtDataToForm` 和 `mergeLineExtDataToForm` 合并回表单和明细行。

## 配置字段和数据字段的关系

扩展字段有两层：

- 字段定义：存在配置库，例如字段 key、名称、类型、是否必填、是否可处理、排序。
- 字段值：存在业务实体的 `ExtData` JSON 中。

字段定义决定页面怎么展示和业务怎么解释字段值；字段值只是一段 JSON。

这意味着：

- 删除字段定义不会自动删除历史 `ExtData` 里的值。
- 修改字段 key 会导致历史值无法按新 key 回显。
- 修改字段类型不会自动转换历史 JSON 值。
- `IsRequired` 主要影响前端表单和业务校验，数据库 `ExtData` 本身不会自动强制。

## Header 和 Line 的使用边界

字段级别通常分为：

- `HEADER`：头表扩展字段，随主实体保存。
- `LINE`：明细扩展字段，随明细实体保存。

如果一个单据有头表和明细表：

- 头字段应该存头实体 `ExtData`。
- 明细字段应该存明细实体 `ExtData`。
- 不要把明细逐行差异塞进头表 ExtData。

否则后续导出、业务处理、明细校验都会变复杂。

## ExtData 参与业务处理的推荐写法

如果扩展字段要参与业务规则，不建议直接到处手写 JSON 解析。优先使用 Contract：

```csharp
var processable = await _cfgDocumentFieldExtContract
    .DeserializeProcessableExtDataAsync(docTypeCode, entity.ExtData, "HEADER");
```

这样可以统一尊重配置里的 `IsProcessable`。只有明确知道字段 key 且不需要配置过滤时，才直接反序列化。

## 查询和统计的限制

ExtData 本质是 JSON 字符串列。它适合保存扩展信息，但不适合：

- 高频 where 条件。
- 数据库索引。
- 排序。
- 大规模统计聚合。
- 强类型关联。

如果一个扩展字段逐渐变成核心业务字段，应该考虑迁移为正式列，而不是继续留在 ExtData。

## 最小示例

### 基础资料：物料扩展字段

```csharp
using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.BaseData;
using KH.WMS.Modules.BaseDataModule.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

[Route("api/material")]
public class MaterialController(IMaterialService materialService)
    : ExtDataCrudController<MdMaterial>(materialService)
{
    [HttpGet("form-config")]
    public async Task<IActionResult> GetFormConfig()
    {
        var extService = HttpContext.RequestServices
            .GetRequiredService<ICfgExtFieldContract>();

        var fields = await extService.GetFieldsAsync("MD_MATERIAL", "HEADER");
        var columns = extService.BuildFormColumns(fields);

        return Ok(new { success = true, data = new { columns } });
    }
}
```

对应前端：

```js
const crudApi = useCrudApi('material')

const {
  loadExtConfig,
  mergedColumns,
  mergedTableColumns,
  extractAndCleanExtData,
  withFlatExtLoad,
} = useExtFields('/api/material/form-config')

onMounted(loadExtConfig)

const formColumns = computed(() => mergedColumns(baseFormColumns))
const tableColumns = computed(() => mergedTableColumns(baseTableColumns))
const load = withFlatExtLoad(crudApi, searchColumns)

const beforeSubmit = (data) => {
  const raw = extractAndCleanExtData(data)
  if (raw) data.extDataRaw = raw
}
```

这个例子里：

- `form-config` 决定表单和表格有哪些扩展列。
- `ExtDataCrudController<MdMaterial>` 决定 `create/update` 时 `extDataRaw` 能写入 `MdMaterial.ExtData`。
- `withFlatExtLoad` 决定分页列表能看到扩展字段。
- `GetById` 展平决定编辑弹窗能回显扩展字段。

### 主从单据：入库单头字段和明细字段

```csharp
[HttpGet("form-config")]
public async Task<IActionResult> GetFormConfig()
{
    var extService = HttpContext.RequestServices
        .GetRequiredService<ICfgDocumentFieldExtContract>();

    var fields = await extService.GetFieldsAsync(
        BizConstants.OrderTypes.PURCHASE_IN,
        "HEADER");
    var columns = extService.BuildFormColumns(fields);

    var lineFields = await extService.GetFieldsAsync(
        BizConstants.OrderTypes.PURCHASE_IN,
        "LINE");
    var lineColumns = extService.BuildFormColumns(lineFields);

    return Ok(new { success = true, data = new { columns, lineColumns } });
}

private async Task ApplyExtDataAsync(InboundOrderCreateDto request)
{
    var extService = HttpContext.RequestServices
        .GetRequiredService<ICfgDocumentFieldExtContract>();

    if (request.ExtDataRaw != null && request.ExtDataRaw.Count > 0)
    {
        var headerFields = await extService.GetFieldsAsync(
            BizConstants.OrderTypes.PURCHASE_IN,
            "HEADER");
        request.Order.ExtData = extService.SerializeExtData(
            request.ExtDataRaw,
            headerFields);
    }

    if (request.LineExtDataRaw != null && request.LineExtDataRaw.Count > 0)
    {
        var lineFields = await extService.GetFieldsAsync(
            BizConstants.OrderTypes.PURCHASE_IN,
            "LINE");

        foreach (var line in request.Lines)
        {
            var lineIndex = line.LineNo.ToString();
            if (request.LineExtDataRaw.TryGetValue(lineIndex, out var lineExtData)
                && lineExtData != null)
            {
                line.ExtData = extService.SerializeExtData(
                    lineExtData,
                    lineFields);
            }
        }
    }
}
```

对应前端保存：

```js
const submitData = {
  order: orderForm.value,
  lines: submitLines,
  extDataRaw: extractExtData(orderForm.value),
  lineExtDataRaw: extractLineExtData(submitLines),
}
```

这种页面不要期待 `ExtDataCrudController` 自动提取 `extDataRaw`，因为提交的是自定义 DTO。扩展字段能否保存，取决于 `ApplyExtDataAsync` 是否执行，以及 `LineNo` 和 `lineExtDataRaw` 的 key 是否能对上。

### 业务中读取可处理扩展字段

```csharp
var extValues = await _cfgDocumentFieldExtContract
    .DeserializeProcessableExtDataAsync(docTypeCode, entity.ExtData, "HEADER");

if (extValues.TryGetValue("temperature", out var temperature))
{
    // 只处理配置为 IsProcessable=1 的字段
}
```

## 常见误区与排查清单

### `ExtData` 没保存

- Controller 是否继承 `ExtDataCrudController<TEntity>`。
- 实体是否有公开 `ExtData` 属性。
- 前端是否提交了 `extDataRaw`。
- 标准 `ExtDataCrudController` 页面里，`extDataRaw` 是否是 JSON 字符串，而不是对象。
- 主从 DTO 页面里，是否有 `ApplyExtDataAsync` 或同等逻辑把 `ExtDataRaw` / `LineExtDataRaw` 序列化进实体。
- 请求体 buffering 是否仍在 `Program.cs` 中启用。

### `/form-config` 返回空

- 使用 `ICfgExtFieldContract` 时，检查 `cfg_ext_field_type.EntityCode` 是否等于代码里的实体编码，例如 `MD_MATERIAL`。
- 使用 `ICfgDocumentFieldExtContract` 时，检查 `cfg_document_type.TypeCode` 是否等于代码里的单据类型编码，例如 `PURCHASE_IN`。
- 检查字段配置的 `FieldLevel` 是 `HEADER` 还是 `LINE`，不要把头字段配置成行字段。
- 检查字段是否挂在正确的 `EntityTypeId` 或 `DocTypeId` 下。
- 检查接口是否被前端正确调用，例如 `/api/material/form-config`、`/api/inbound-order/form-config`。

### Contract 选错

- 基础资料、库存快照、库存移动这类“按实体固定字段”的页面，用 `ICfgExtFieldContract`。
- 入库单、出库单这类“按单据类型变化字段”的页面，用 `ICfgDocumentFieldExtContract`。
- 不要把 `MD_MATERIAL` 传给 `ICfgDocumentFieldExtContract`，它会去 `cfg_document_type.TypeCode` 查。
- 不要把 `PURCHASE_IN` 传给 `ICfgExtFieldContract`，它会去 `cfg_ext_field_type.EntityCode` 查。

### 详情不回显

- 数据库 `ExtData` 是否是合法 JSON。
- `GetById` 返回的 `Data` 是否是实体类型。
- 扩展字段 key 是否和前端 column `prop` 一致。
- 是否和实体已有属性重名，被展平逻辑跳过。
- 主从单据详情是否返回了 `ExtDataFlattened` / `LineExtDataFlattened`，前端是否调用 `mergeExtDataToForm` / `mergeLineExtDataToForm`。

### 分页列表没有展平字段

- 当前后端基类只覆盖 `GetById` 展平。
- 分页列表展平按现有设计由前端 load 函数处理，或业务 Service 自行在 `AfterQueryAsync` 中加工。
- 前端是否用了 `withFlatExtLoad(crudApi, searchColumns)`。
- 表格列是否用了 `mergedTableColumns(baseTableColumns)`。

### 明细扩展字段没保存

- `/form-config` 是否返回 `lineColumns`。
- 前端明细列是否用了 `mergedLineColumns(baseLineColumns)`。
- 保存时是否传了 `lineExtDataRaw`。
- `lineExtDataRaw` 的 key 是否和后端匹配。当前入库/出库示例按 `LineNo.ToString()` 匹配，不是按数组下标或数据库 Id 匹配。
- 前端构造 `submitLines` 时是否先设置了 `lineNo: i + 1`，再调用 `extractLineExtData(submitLines)`。当前后端 `ApplyExtDataAsync` 读取的是请求里的 `LineNo`，不是 Service 后面兜底生成的行号。

### 配置改了但页面还是旧字段

- `CfgExtFieldConfigService` / `CfgDocumentFieldService` 是否走了标准新增、修改、删除流程。
- 标准流程会在 `AfterCreateAsync`、`AfterUpdateAsync`、`AfterDeleteAsync` 中清理 Contract 缓存。
- 如果直接改数据库，缓存不会自动清理，可以重启服务或调用对应 Contract 的 `ClearCache` 验证。

### 滥用 ExtData

- 高频查询字段不要放 ExtData。
- 需要数据库约束或索引的字段不要放 ExtData。
- 核心业务状态不要放 ExtData。
- 参与复杂统计报表的字段慎用 ExtData。
