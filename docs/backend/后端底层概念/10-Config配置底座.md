# 10 Config 配置底座

## 这个概念解决什么问题

Config 配置底座解决的是“业务规则可配置，而不是硬编码”的问题。

KH.WMS 的 Config 模块负责：

- 全局配置解析。
- 配置作用域优先级。
- 配置缓存和启动预热。
- 单据类型和单据状态。
- 编码规则和编码序列。
- 扩展字段定义。
- 各类配置字典和配置表维护。

它使用独立配置库，配置实体通过 `[ConfigDb]` 路由到 SQLite。业务模块只应该调用 Config 提供的抽象 Contract，不应该复制 Config 模块结构或直接硬编码配置表查询。

## 什么时候需要看

- 新增一个可配置业务开关。
- 业务规则要按仓库、库区、单据类型差异化。
- `ResolveConfigBoolAsync` 结果和预期不一致。
- 配置修改后接口仍使用旧值。
- 要新增扩展字段、编码规则、单据状态。
- 不确定配置表为什么不在业务库里。

## 业务开发应该怎么用

### 读取字符串配置

```csharp
var value = await _configResolver.ResolveConfigValueAsync(
    "CONTAINER",
    "ALLOW_MIXED_BATCH",
    new ConfigScopeContext
    {
        WarehouseId = warehouseId,
        ZoneId = zoneId,
        DocTypeCode = docTypeCode
    });
```

### 读取布尔配置

```csharp
var allowMixedMaterial = await _configResolver
    .ResolveConfigBoolAsync("CONTAINER", "ALLOW_MIXED_MATERIAL");
```

### 按业务边界使用配置

- 简单业务开关：直接注入 `IConfigResolverContract` 读取。
- 前置校验链：优先做成 `IValidator`，通过 `[ConfigValidation]` 挂到业务方法。
- 单据状态：使用 `IDocumentStatusValidatorContract`。
- 编码规则：使用 `ICodeGeneratorService`。
- 扩展字段：使用 `ICfgExtFieldContract` 或 `ICfgDocumentFieldExtContract`。

不要在业务 Service 里直接查 `CfgGlobalConfig`，除非你正在维护 Config 模块本身。

## 底层逻辑和实现

### 配置库和业务库分离

Config 实体普遍标记 `[ConfigDb]`：

```csharp
[ConfigDb]
public class CfgGlobalConfig : BaseEntity<long>, IEnableDisableEntity
{
}
```

仓储会自动路由到 `SqlSugarSetup.ConfigDb`。配置库默认是 SQLite 文件，路径由 `DatabaseOptions.ConfigDbPath` 决定。

这种分离的意义：

- 配置可以独立初始化和迁移。
- 业务库切换数据库类型时，配置库仍保持轻量。
- 配置表不混入业务事务和业务数据模型。

### 作用域优先级

`IConfigResolverContract` 支持四级作用域：

1. `DOC_TYPE`
2. `ZONE`
3. `WAREHOUSE`
4. `GLOBAL`

解析顺序从精确到通用：

```text
DOC_TYPE -> ZONE -> WAREHOUSE -> GLOBAL
```

例如同时配置了全局规则和某个单据类型规则，传入 `DocTypeCode` 时会优先取单据类型级配置。

### 缓存 key

`ConfigResolverContract` 会把 group、key、warehouse、zone、doc type 拼成缓存 key：

```text
config:{group}:{key}:w{warehouseId}:z{zoneId}:d{docTypeCode}
```

同一个配置项在不同作用域下会有不同缓存。

### 启动预热

`Program.cs` 启动后会解析 `IConfigResolverContract` 并调用：

```csharp
await configService.WarmUpAsync();
```

当前预热的是 GLOBAL 级别启用配置，目的是减少首次请求时的数据库查询。

### 扩展字段也属于 Config

两类扩展字段：

- `CfgExtField`：非单据类实体扩展字段。
- `CfgDocumentField`：单据类扩展字段。

它们提供前端表单列配置、ExtData 序列化/反序列化、可处理字段筛选等能力。

### Config 不是普通业务模块模板

Config 模块里很多 Controller 和 Service 是配置维护用 CRUD，它们指向配置库，且承载底座能力。

普通业务模块不要照搬 Config 模块：

- 不要给业务实体加 `[ConfigDb]`。
- 不要把业务规则表随意放进配置库。
- 不要绕过 Config Contract 直接查配置表。

## 配置命中过程举例

假设业务读取：

```csharp
ResolveConfigBoolAsync("CONTAINER", "ALLOW_MIXED_BATCH", scope)
```

并且 `scope` 中有：

```text
WarehouseId = 1
ZoneId = 10
DocTypeCode = INBOUND_ORDER
```

解析过程是：

1. 先拼缓存 key，查缓存。
2. 如果有 `DocTypeCode`，通过 `IConfigScopeResolver` 把 `INBOUND_ORDER` 解析成单据类型 ID。
3. 查 `ScopeLevel = DOC_TYPE`、`ScopeId = 单据类型ID` 的启用配置。
4. 如果没找到，查 `ScopeLevel = ZONE`、`ScopeId = 10`。
5. 如果没找到，查 `ScopeLevel = WAREHOUSE`、`ScopeId = 1`。
6. 如果没找到，查 `ScopeLevel = GLOBAL`、`ScopeId = null`。
7. 多条命中时按 `Priority` 降序取第一条。
8. 结果写缓存。
9. 布尔解析时，`true` 或 `1` 视为 true。

如果你认为应该命中仓库配置，但实际命中了单据类型配置，原因通常是更高优先级作用域存在配置。

## 缓存一致性边界

`ConfigResolverContract` 会把解析结果缓存 1 小时。`WarmUpAsync` 只预热 GLOBAL 级别配置。

这意味着：

- 修改配置表后，接口不一定立刻读到新值。
- 不同作用域配置缓存 key 不同，清理一个 key 不等于清理所有作用域。
- 如果配置维护接口没有清理缓存，就会出现“数据库已改，运行仍旧值”。

排查时要看：

1. 配置表实际值。
2. 当前请求传入的 scope。
3. 缓存 key 是否已存在旧值。
4. 是否需要调用相关 `ClearCache` 或移除缓存前缀。

## 配置项建模建议

新增配置项时至少明确：

- `ConfigGroup`：业务分组，例如 `CONTAINER`、`INBOUND`。
- `ConfigKey`：稳定 key，避免中文和页面文案。
- `ConfigValue`：字符串值，布尔建议用 `true/false` 或 `1/0`。
- `ScopeLevel`：GLOBAL、WAREHOUSE、ZONE、DOC_TYPE。
- `ScopeId`：作用域 ID，GLOBAL 为空。
- `Priority`：同作用域多条配置的优先级。
- `Status`：是否启用。

不要把复杂对象随意塞成大 JSON 配置，除非确实是策略参数或扩展字段定义。复杂规则更适合策略链或独立配置表。

## Config 与 Validator 的关系

很多配置项不是由业务 Service 直接读取，而是被 Validator 使用：

```text
[ConfigValidation(ValidatorCodes.MIXED_BATCH)]
    -> MixedBatchValidator
        -> IConfigResolverContract.ResolveConfigBoolAsync("CONTAINER", "ALLOW_MIXED_BATCH")
```

这让主流程保持干净：

- 主流程只声明“我要校验混批”。
- Validator 决定如何读取配置和检查数据。
- Config 决定配置值和作用域优先级。

## 最小示例

### 校验是否允许混批

```csharp
if (!await _configResolver.ResolveConfigBoolAsync("CONTAINER", "ALLOW_MIXED_BATCH"))
{
    ...
}
```

### 按单据类型读取配置

```csharp
var scope = new ConfigScopeContext
{
    WarehouseId = warehouseId,
    DocTypeCode = docTypeCode
};

var mode = await _configResolver.ResolveConfigValueAsync("INBOUND", "RECEIVE_MODE", scope);
```

### 获取单据扩展字段

```csharp
var fields = await _documentFieldExtContract.GetFieldsAsync(docTypeCode, "HEADER");
var columns = _documentFieldExtContract.BuildFormColumns(fields);
```

## 常见误区与排查清单

- 读取结果为空：配置项是否启用，group/key 是否一致，作用域是否传错。
- 修改配置后仍旧值：缓存是否未清理，是否命中了更高优先级作用域。
- 配置表在业务库找不到：配置实体走 `[ConfigDb]`，在 SQLite 配置库。
- 业务模块直接查 `CfgGlobalConfig`：优先改成调用 `IConfigResolverContract`。
- 把业务单据表放配置库：配置库只放配置和规则，不放业务流水。
- 扩展字段不显示：检查字段配置、field level、缓存、前端 column 构建。
