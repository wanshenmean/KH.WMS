---
title: "KH.WMS.Algorithms API 方法调用手册"
description: "KH.WMS.Algorithms API 方法调用手册：说明适用场景、当前实现、设计边界与开发或排障入口。"
status: reference
audience: "参与 KH.WMS 开发、测试与运维的团队成员"
reviewed: "2026-07-14"
sourcePaths:
  - "KH.WMS/KH.WMS.Server"
  - "KH.WMS/KH.WMS.Core"
  - "KH.WMS/Modules"
---

# KH.WMS.Algorithms API 方法调用手册

> 适用对象：只能引用 `KH.WMS.Algorithms.dll`、不能查看源码的外部开发人员。  
> 目标框架：`.NET 8.0`  
> 主命名空间：`KH.WMS.Algorithms.Strategy`

本文档只记录当前代码中真实存在、可立即调用的 API。文档按“先方法、后场景”的顺序组织；每个 API 方法都提供输入参数、输出参数和调用示例。Web Controller 不作为本文档主 API。

## 目录

- [1. 快速开始](#1-快速开始)
- [2. 核心调用模型](#2-核心调用模型)
- [3. API 方法参考](#3-api-方法参考)
- [4. 内置策略调用指南](#4-内置策略调用指南)
- [5. 常见错误与排查](#5-常见错误与排查)
- [6. 兼容说明](#6-兼容说明)

---

## 1. 快速开始

### 1.1 当前可用能力

| 能力 | 调用入口 | 当前可用编码 | 输出模型 |
|---|---|---|---|
| 入库上架决策 | `IPolicyEngine.ExecuteAsync` | `DEFAULT_PUTAWAY` | `PutawayResult` |
| 货位分配 | `IPolicyEngine.ExecuteAsync` | `ABC_CLASS`、`CATEGORY_ZONE`、`CENTRALIZED`、`DOUBLE_DEEP` | `LocationAllocationResult` |
| 库存分配 | `IPolicyEngine.ExecuteAsync` | `FIFO`、`FEFO`、`BATCH`、`UTILIZATION_PRIORITY` | `InventoryAllocationResult` |
| 下架决策 | `IPolicyEngine.ExecuteAsync` | `DEFAULT_PICKING` | `PickingResult` |
| 策略链执行 | `IPolicyEngine.ExecuteAsync(PolicyType, ...)` 或 `ExecuteChainAsync` | 数据库链或注册链 | 取决于策略链最后一个成功策略 |
| 路径排序 | `CranePathOptimizer.Optimize` | `S_SHAPE`、`Z_SHAPE`、`U_SHAPE` | `List<PickingTaskItem>` |

### 1.2 引用 DLL

```xml
<ItemGroup>
  <Reference Include="KH.WMS.Algorithms">
    <HintPath>libs\KH.WMS.Algorithms.dll</HintPath>
  </Reference>
  <Reference Include="KH.WMS.Core">
    <HintPath>libs\KH.WMS.Core.dll</HintPath>
  </Reference>
</ItemGroup>
```

常用命名空间：

```csharp
using KH.WMS.Algorithms.Strategy;
using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.DTOs;
using KH.WMS.Algorithms.Strategy.Enums;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.QueryServices;
using KH.WMS.Algorithms.Strategy.Services;
using KH.WMS.Algorithms.Strategy.Strategies;
```

### 1.3 注册服务

```csharp
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.RegisterType<PolicyRegistry>().As<IPolicyRegistry>().SingleInstance();
        containerBuilder.RegisterType<PolicyEngine>().As<IPolicyEngine>().InstancePerLifetimeScope();

        containerBuilder.RegisterType<InventoryQueryService>().As<IInventoryQueryService>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<LocationQueryService>().As<ILocationQueryService>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<WarehouseQueryService>().As<IWarehouseQueryService>().InstancePerLifetimeScope();

        containerBuilder.RegisterType<StrategyConfigService>().As<IStrategyConfigService>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<StrategyChainService>().As<IStrategyChainService>().InstancePerLifetimeScope();
        containerBuilder.RegisterType<StrategyQueryService>().As<IStrategyQueryService>().InstancePerLifetimeScope();

        containerBuilder.RegisterModule<StrategyAutofacModule>();
    });
```

注册前置条件：

| 前置项 | 中文说明 |
|---|---|
| `ISqlSugarClient` | 三个 QueryService 构造函数依赖数据库客户端。 |
| `ILoggerService` | `PolicyEngine` 构造函数依赖日志服务。 |
| `PolicyRegistry` 单例 | 策略扫描后会注册到该对象，必须跨 Scope 保持同一个注册表。 |
| `StrategyAutofacModule` | 自动扫描当前算法程序集中的 `IPolicyStrategy` 实现。 |

### 1.4 最小调用示例

```csharp
public async Task<InventoryAllocationResult> RunFifoAsync(
    IPolicyEngine policyEngine,
    IInventoryQueryService inventoryQueryService,
    CancellationToken cancellationToken = default)
{
    var context = new PolicyContext
    {
        WarehouseCode = "WH01",
        BusinessCode = "SO202607060001",
        DocType = "SALES_OUTBOUND"
    };

    context.SetData(StrategyParams.SVC_INVENTORY_QUERY, inventoryQueryService);
    context.SetData(StrategyParams.InventoryAllocationInput.WAREHOUSE_CODE, "WH01");
    context.SetData(StrategyParams.InventoryAllocationInput.MATERIAL_CODE, "M001");
    context.SetData(StrategyParams.InventoryAllocationInput.REQUIRED_QTY, 10m);

    var result = await policyEngine.ExecuteAsync("FIFO", context, cancellationToken);
    return GetOutput<InventoryAllocationResult>(result);
}

public static T GetOutput<T>(IPolicyResult result)
{
    if (!result.IsSuccess || !result.IsHandled)
    {
        throw new InvalidOperationException(result.ErrorMessage ?? "策略未成功处理");
    }

    if (result.Output is not T output)
    {
        throw new InvalidCastException(
            $"策略返回类型不匹配，期望 {typeof(T).Name}，实际 {result.Output?.GetType().Name ?? "null"}");
    }

    return output;
}
```

---

## 2. 核心调用模型

### 2.1 `PolicyContext`

`PolicyContext` 是所有策略方法的统一输入容器。

| 字段/方法 | 类型 | 是否必填 | 中文说明 | 示例值 |
|---|---|---:|---|---|
| `InputParameters` | `object?` | 否 | 调用方自定义输入对象，当前内置策略不强依赖。 | `new { Source = "ERP" }` |
| `ContextData` | `IDictionary<string, object?>` | 否 | 策略专用参数、服务实例、上游策略结果。 | 通过 `SetData` 写入 |
| `BusinessCode` | `string?` | 建议 | 业务单据号，用于日志定位。 | `"SO202607060001"` |
| `WarehouseId` | `long?` | 按场景 | 仓库 ID，上架、货位、下架、策略链匹配常用。 | `1` |
| `WarehouseCode` | `string?` | 按场景 | 仓库编码，库存分配常用。 | `"WH01"` |
| `ZoneId` | `long?` | 否 | 库区 ID，用于策略链匹配或限定货位范围。 | `10` |
| `MaterialId` | `long?` | 按场景 | 物料 ID，上架、货位策略常用。 | `10001` |
| `MaterialCategoryId` | `long?` | 否 | 物料分类 ID，品类分区策略常用。 | `20001` |
| `DocType` | `string?` | 否 | 单据类型，策略链匹配和站台匹配常用。 | `"SALES_OUTBOUND"` |
| `OrderType` | `string?` | 否 | 订单类型，当前数据库链匹配不使用该字段。 | `"SALES"` |
| `GetData<T>(string key)` | `T?` | 否 | 按键读取上下文值，缺失或无法转换时返回 `default`。 | `context.GetData<decimal>("RequiredQty")` |
| `SetData<T>(string key, T? value)` | `void` | 否 | 按键写入上下文值。 | `context.SetData("RequiredQty", 10m)` |

### 2.2 `StrategyParams`

所有上下文键都建议使用 `StrategyParams`，不要手写字符串。

| 分组 | 常量 | 字符串值 | 类型 | 中文说明 |
|---|---|---|---|---|
| 服务 | `SVC_SQL_SUGAR_CLIENT` | `ISqlSugarClient` | `ISqlSugarClient` | 数据库客户端。 |
| 服务 | `SVC_INVENTORY_QUERY` | `IInventoryQueryService` | `IInventoryQueryService` | 库存查询服务。 |
| 服务 | `SVC_WAREHOUSE_QUERY` | `IWarehouseQueryService` | `IWarehouseQueryService` | 仓库、库区、巷道、站台查询服务。 |
| 服务 | `SVC_LOCATION_QUERY` | `ILocationQueryService` | `ILocationQueryService` | 货位查询服务。 |
| 通用 | `Common.WAREHOUSE_ID` | `WarehouseId` | `long` | 备用仓库 ID。 |
| 通用 | `Common.MATERIAL_ID` | `MaterialId` | `long` | 备用物料 ID。 |
| 通用 | `Common.PATH_OPTIMIZATION` | `PathOptimization` | `string` | 路径优化模式。 |
| 上架 | `PutawayInput.DOC_TYPE_ID` | `DocTypeId` | `long?` | 单据类型 ID。 |
| 上架 | `PutawayInput.INBOUND_STATION_ID` | `InboundStationId` | `string` | 入库站台 ID。 |
| 上架 | `PutawayInput.INBOUND_STATION_CODE` | `InboundStationCode` | `string` | 入库站台编码。 |
| 上架 | `PutawayInput.MATERIAL_CATEGORY_ID` | `MaterialCategoryId` | `long?` | 物料分类 ID。 |
| 上架 | `PutawayInput.ENABLE_ZONE_PARTITION` | `EnableZonePartition` | `bool?` | 是否启用品类分区。 |
| 上架 | `PutawayInput.ENABLE_ABC_CLASS` | `EnableAbcClass` | `bool?` | 是否启用 ABC 分类。 |
| 上架 | `PutawayInput.ENABLE_NEARBY` | `EnableNearby` | `bool?` | 是否启用低负载巷道推荐。 |
| 货位 | `LocationAllocationInput.MATERIAL_ABC_CLASS` | `MaterialAbcClass` | `string` | 物料 ABC 分类。 |
| 货位 | `LocationAllocationInput.TARGET_BATCH_NO` | `TargetBatchNo` | `string` | 目标批次号。 |
| 货位 | `LocationAllocationInput.MAX_NEARBY_COUNT` | `MaxNearbyCount` | `int?` | 附近货位最大查找数量。 |
| 货位 | `LocationAllocationInput.MAX_RECOMMEND_COUNT` | `MaxRecommendCount` | `int?` | 最大推荐货位数量。 |
| 货位 | `LocationAllocationInput.SORT_RULES` | `SortRules` | `string` | 排序规则 JSON。 |
| 货位 | `LocationAllocationInput.ENABLE_DOUBLE_DEEP` | `EnableDoubleDeep` | `bool?` | 是否启用双深约束。 |
| 货位 | `LocationAllocationInput.DOUBLE_DEEP_MODE` | `DoubleDeepMode` | `string` | 双深分配模式。 |
| 货位 | `LocationAllocationInput.STRATEGY_PARAMS` | `StrategyParams` | `string` | 策略默认参数 JSON。 |
| 货位 | `LocationAllocationInput.STEP_PARAMS` | `StepParams` | `string` | 步骤参数 JSON。 |
| 库存 | `InventoryAllocationInput.WAREHOUSE_CODE` | `WarehouseCode` | `string` | 仓库编码。 |
| 库存 | `InventoryAllocationInput.MATERIAL_CODE` | `MaterialCode` | `string` | 物料编码。 |
| 库存 | `InventoryAllocationInput.REQUIRED_QTY` | `RequiredQty` | `decimal` | 需求数量。 |
| 库存 | `InventoryAllocationInput.TARGET_BATCH_NO` | `TargetBatchNo` | `string` | 指定批次。 |
| 下架 | `PickingInput.DESTINATION_STATION_ID` | `DestinationStationId` | `long` | 目标出库站台 ID。 |
| 下架 | `PickingInput.DESTINATION_STATION_CODE` | `DestinationStationCode` | `string` | 目标出库站台编码。 |
| 下架 | `PickingInput.DESTINATION_ZONE_ID` | `DestinationZoneId` | `long` | 目标区域 ID。 |
| 下架 | `PickingInput.DESTINATION_ZONE_CODE` | `DestinationZoneCode` | `string` | 目标区域编码。 |
| 下架 | `PickingInput.DOC_TYPE_ID` | `DocTypeId` | `long?` | 单据类型 ID。 |
| 下架 | `PickingInput.ENABLE_PALLET_FIRST` | `EnablePalletFirst` | `bool?` | 是否整托优先。 |
| 下架 | `PickingInput.SOURCE_AISLE_NO` | `SourceAisleNo` | `int` | 来源巷道号。 |

输出键：

| 常量 | 字符串值 | 输出模型 | 中文说明 |
|---|---|---|---|
| `PutawayOutput.RESULT` | `PutawayResult` | `PutawayResult` | 上架策略结果。 |
| `LocationAllocationOutput.RESULT` | `LocationAllocationResult` | `LocationAllocationResult` | 货位分配结果。 |
| `InventoryAllocationOutput.RESULT` | `InventoryAllocationResult` | `InventoryAllocationResult` | 库存分配结果。 |
| `PickingOutput.RESULT` | `PickingResult` | `PickingResult` | 下架策略结果。 |

### 2.3 `IPolicyResult`

| 字段/方法 | 类型 | 中文说明 |
|---|---|---|
| `IsSuccess` | `bool` | 策略执行是否成功完成。`Skipped` 也是 `true`。 |
| `IsHandled` | `bool` | 策略是否真正处理并产出结果。转换 `Output` 前必须检查。 |
| `Output` | `object?` | 策略输出对象。 |
| `ErrorMessage` | `string?` | 失败、跳过或部分失败说明。 |
| `ExecutionLogs` | `List<PolicyExecutionLog>` | 策略链执行日志。 |
| `Duration` | `long` | 执行耗时，毫秒。 |
| `AddExecutionLog(PolicyExecutionLog log)` | `void` | 添加执行日志。 |
| `SetDuration(long duration)` | `void` | 设置耗时。 |

---

## 3. API 方法参考

### 3.1 `IPolicyEngine`

#### `ExecuteAsync(PolicyType policyType, IPolicyContext context, CancellationToken cancellationToken = default)`

用途：按策略类型执行。引擎会优先按数据库策略链配置匹配链；没有匹配链时回退为执行注册表中该类型的策略。

输入参数：

| 参数名 | 类型 | 必填 | 中文说明 | 示例值 |
|---|---|---:|---|---|
| `policyType` | `PolicyType` | 是 | 策略类型。 | `PolicyType.InventoryAllocation` |
| `context` | `IPolicyContext` | 是 | 策略上下文。 | `new PolicyContext { WarehouseCode = "WH01" }` |
| `cancellationToken` | `CancellationToken` | 否 | 取消令牌。 | `CancellationToken.None` |

输出参数：

| 返回类型 | 字段名 | 字段类型 | 中文说明 |
|---|---|---|---|
| `IPolicyResult` | `IsSuccess` | `bool` | 是否执行成功。 |
| `IPolicyResult` | `IsHandled` | `bool` | 是否有策略处理成功。 |
| `IPolicyResult` | `Output` | `object?` | 输出模型，按实际策略转换。 |
| `IPolicyResult` | `ErrorMessage` | `string?` | 失败或跳过原因。 |
| `IPolicyResult` | `ExecutionLogs` | `List<PolicyExecutionLog>` | 策略链日志。 |
| `IPolicyResult` | `Duration` | `long` | 耗时毫秒。 |

前置条件：已注册 `IPolicyEngine`、`IPolicyRegistry`、内置策略、必要 QueryService。使用数据库链时还需注册 `IStrategyChainService` 和配置仓储。

失败/空结果：没有适用策略时通常返回 `IsSuccess=true`、`IsHandled=false`；执行异常时返回 `IsSuccess=false`。

示例：

```csharp
var context = new PolicyContext { WarehouseCode = "WH01", BusinessCode = "SO001" };
context.SetData(StrategyParams.SVC_INVENTORY_QUERY, inventoryQueryService);
context.SetData(StrategyParams.InventoryAllocationInput.WAREHOUSE_CODE, "WH01");
context.SetData(StrategyParams.InventoryAllocationInput.MATERIAL_CODE, "M001");
context.SetData(StrategyParams.InventoryAllocationInput.REQUIRED_QTY, 10m);

IPolicyResult result = await policyEngine.ExecuteAsync(PolicyType.InventoryAllocation, context);
```

#### `ExecuteAsync(string policyCode, IPolicyContext context, CancellationToken cancellationToken = default)`

用途：按策略编码执行单个策略。

输入参数：

| 参数名 | 类型 | 必填 | 中文说明 | 示例值 |
|---|---|---:|---|---|
| `policyCode` | `string` | 是 | 已注册策略编码。 | `"FIFO"` |
| `context` | `IPolicyContext` | 是 | 策略上下文。 | `context` |
| `cancellationToken` | `CancellationToken` | 否 | 取消令牌。 | `CancellationToken.None` |

输出参数：

| 返回类型 | 字段名 | 字段类型 | 中文说明 |
|---|---|---|---|
| `IPolicyResult` | `Output` | `object?` | 单策略输出，按策略编码转换为 `PutawayResult`、`LocationAllocationResult`、`InventoryAllocationResult` 或 `PickingResult`。 |
| `IPolicyResult` | `ErrorMessage` | `string?` | 策略未注册、未启用、不适用或执行异常说明。 |

前置条件：`policyCode` 对应策略已被 `StrategyAutofacModule` 或注册表注册。

失败/空结果：策略不存在时返回失败；策略未启用或不适用时返回跳过。

示例：

```csharp
IPolicyResult result = await policyEngine.ExecuteAsync("FIFO", context);
InventoryAllocationResult allocation = GetOutput<InventoryAllocationResult>(result);
```

#### `ExecuteChainAsync(string chainCode, IPolicyContext context, CancellationToken cancellationToken = default)`

用途：执行已注册到 `IPolicyRegistry` 的内存策略链。

输入参数：

| 参数名 | 类型 | 必填 | 中文说明 | 示例值 |
|---|---|---:|---|---|
| `chainCode` | `string` | 是 | 策略链编码。 | `"LOCATION_CHAIN"` |
| `context` | `IPolicyContext` | 是 | 策略上下文。 | `context` |
| `cancellationToken` | `CancellationToken` | 否 | 取消令牌。 | `CancellationToken.None` |

输出参数：

| 返回类型 | 字段名 | 字段类型 | 中文说明 |
|---|---|---|---|
| `IPolicyResult` | `Output` | `object?` | 链中最后一个成功处理策略的输出。 |
| `IPolicyResult` | `ExecutionLogs` | `List<PolicyExecutionLog>` | 链内各步骤执行日志。 |

前置条件：调用前已通过 `IPolicyRegistry.RegisterChain` 注册链。

失败/空结果：注册表中找不到链时返回失败。

示例：

```csharp
IPolicyResult result = await policyEngine.ExecuteChainAsync("LOCATION_CHAIN", context);
```

### 3.2 `IPolicyRegistry`

输出模型：`IPolicyStrategy` 字段包括 `Name`、`Code`、`Priority`、`Author`、`Description`、`PolicyType`、`IsEnabled`、`ApplicableWarehouseIds`、`ApplicableZoneIds`、`ApplicableMaterialIds`、`ApplicableDocTypes`；`IPolicyChain` 字段包括 `Name`、`Code`、`PipelineMode`；`IPolicyFilter` 字段包括 `Name`、`Order`。

| 方法签名 | 用途 | 输入参数中文说明 | 输出参数中文说明 | 示例 |
|---|---|---|---|---|
| `void RegisterStrategy(IPolicyStrategy strategy)` | 注册策略实例。 | `strategy`：策略对象，必填。 | 无返回值。 | `registry.RegisterStrategy(myStrategy);` |
| `void RegisterFilter(IPolicyFilter filter)` | 注册过滤器。 | `filter`：过滤器对象，必填。 | 无返回值。 | `registry.RegisterFilter(myFilter);` |
| `void RegisterChain(IPolicyChain chain)` | 注册策略链。 | `chain`：策略链对象，必填。 | 无返回值。 | `registry.RegisterChain(chain);` |
| `IEnumerable<IPolicyStrategy> GetStrategies(PolicyType policyType)` | 获取指定类型策略。 | `policyType`：策略类型，必填。 | 返回策略集合。 | `var list = registry.GetStrategies(PolicyType.InventoryAllocation);` |
| `IEnumerable<IPolicyFilter> GetFilters()` | 获取全部过滤器。 | 无输入参数。 | 返回过滤器集合。 | `var filters = registry.GetFilters();` |
| `IPolicyStrategy? GetStrategy(string code)` | 按编码获取策略。 | `code`：策略编码，必填。 | 返回策略；不存在返回 `null`。 | `var fifo = registry.GetStrategy("FIFO");` |
| `IPolicyChain? GetChain(string chainCode)` | 按编码获取链。 | `chainCode`：策略链编码，必填。 | 返回策略链；不存在返回 `null`。 | `var chain = registry.GetChain("LOCATION_CHAIN");` |
| `IPolicyFilter? GetFilter(string name)` | 按名称获取过滤器。 | `name`：过滤器名称，必填。 | 返回过滤器；不存在返回 `null`。 | `var filter = registry.GetFilter("BusinessFilter");` |
| `IPolicyChain CreateChain(string chainCode, PolicyType policyType)` | 从注册表中指定类型策略创建链。 | `chainCode`：新链编码；`policyType`：策略类型。 | 返回新策略链。 | `var chain = registry.CreateChain("INV_CHAIN", PolicyType.InventoryAllocation);` |

### 3.3 `IPolicyChain`

输出模型：链执行返回 `IPolicyResult`；链对象自身字段为 `Name`、`Code`、`PipelineMode`。

| 方法签名 | 用途 | 输入参数中文说明 | 输出参数中文说明 | 示例 |
|---|---|---|---|---|
| `IPolicyChain AddStrategy(IPolicyStrategy strategy)` | 向链中添加策略。 | `strategy`：策略实例，必填。 | 返回当前链对象。 | `chain.AddStrategy(fifoStrategy);` |
| `IPolicyChain AddStrategy(IPolicyStrategy strategy, string? stepParams, string? strategyParams)` | 添加策略并携带参数。 | `strategy`：策略实例；`stepParams`：步骤参数 JSON；`strategyParams`：策略默认参数 JSON。 | 返回当前链对象。 | `chain.AddStrategy(abc, "{\"MaxRecommendCount\":10}", null);` |
| `IPolicyChain AddFilter(IPolicyFilter filter)` | 向链中添加过滤器。 | `filter`：过滤器实例，必填。 | 返回当前链对象。 | `chain.AddFilter(filter);` |
| `Task<IPolicyResult> ExecuteAsync(IPolicyContext context, CancellationToken cancellationToken = default)` | 执行策略链。 | `context`：上下文；`cancellationToken`：取消令牌。 | 返回链执行结果。 | `var result = await chain.ExecuteAsync(context);` |

### 3.4 `IPolicyStrategy`

| 方法签名 | 用途 | 输入参数中文说明 | 输出参数中文说明 | 示例 |
|---|---|---|---|---|
| `Task<bool> IsApplicableAsync(IPolicyContext context, CancellationToken cancellationToken = default)` | 判断策略是否适用当前上下文。 | `context`：上下文；`cancellationToken`：取消令牌。 | 返回 `true` 表示适用。 | `bool ok = await strategy.IsApplicableAsync(context);` |
| `Task<IPolicyResult> ExecuteAsync(IPolicyContext context, CancellationToken cancellationToken = default)` | 执行策略。 | `context`：上下文；`cancellationToken`：取消令牌。 | 返回策略执行结果。 | `IPolicyResult result = await strategy.ExecuteAsync(context);` |

输出相关字段：

| 字段 | 类型 | 中文说明 |
|---|---|---|
| `Name` | `string` | 策略名称。 |
| `Code` | `string` | 策略编码。 |
| `Priority` | `int` | 优先级，数字越大越先执行。 |
| `PolicyType` | `PolicyType` | 策略类型。 |
| `IsEnabled` | `bool` | 是否启用。 |

### 3.5 `IPolicyFilter`

| 方法签名 | 用途 | 输入参数中文说明 | 输出参数中文说明 | 示例 |
|---|---|---|---|---|
| `Task<IPolicyContext> OnBeforeExecutionAsync(IPolicyContext context, CancellationToken cancellationToken = default)` | 策略链执行前处理上下文。 | `context`：原上下文；`cancellationToken`：取消令牌。 | 返回处理后的上下文。 | `context = await filter.OnBeforeExecutionAsync(context);` |
| `Task<IPolicyResult> OnAfterExecutionAsync(IPolicyContext context, IPolicyResult result, CancellationToken cancellationToken = default)` | 策略链执行后处理结果。 | `context`：上下文；`result`：原结果；`cancellationToken`：取消令牌。 | 返回处理后的结果。 | `result = await filter.OnAfterExecutionAsync(context, result);` |

### 3.6 `PolicyContext`

| 方法签名 | 用途 | 输入参数中文说明 | 输出参数中文说明 | 示例 |
|---|---|---|---|---|
| `T? GetData<T>(string key)` | 从上下文字典读取值。 | `key`：上下文键，必填。 | 返回指定类型值；缺失、为空或转换失败返回 `default`。 | `decimal qty = context.GetData<decimal>(StrategyParams.InventoryAllocationInput.REQUIRED_QTY);` |
| `void SetData<T>(string key, T? value)` | 写入上下文字典。 | `key`：上下文键；`value`：要写入的值。 | 无返回值。 | `context.SetData(StrategyParams.InventoryAllocationInput.REQUIRED_QTY, 10m);` |

### 3.7 `PolicyResult`

| 方法签名 | 用途 | 输入参数中文说明 | 输出参数中文说明 | 示例 |
|---|---|---|---|---|
| `static PolicyResult Success(object? output = null)` | 创建成功且已处理结果。 | `output`：输出对象，可空。 | 返回 `IsSuccess=true`、`IsHandled=true` 的结果。 | `var result = PolicyResult.Success(output);` |
| `static PolicyResult Failure(string errorMessage)` | 创建失败结果。 | `errorMessage`：错误消息，必填。 | 返回 `IsSuccess=false` 的结果。 | `var result = PolicyResult.Failure("库存不足");` |
| `static PolicyResult Skipped(string reason = "策略不适用")` | 创建跳过结果。 | `reason`：跳过原因。 | 返回 `IsSuccess=true`、`IsHandled=false` 的结果。 | `var result = PolicyResult.Skipped("无可用货位");` |
| `void AddExecutionLog(PolicyExecutionLog log)` | 添加执行日志。 | `log`：日志对象，必填。 | 无返回值。 | `result.AddExecutionLog(log);` |
| `void SetDuration(long duration)` | 设置执行耗时。 | `duration`：毫秒数。 | 无返回值。 | `result.SetDuration(35);` |

### 3.8 `IInventoryQueryService`

返回模型 `InventoryInfoDTO` 字段：

| 字段 | 类型 | 中文说明 |
|---|---|---|
| `Id` | `long` | 库存明细 ID。 |
| `HeaderId` | `long` | 库存头 ID。 |
| `ContainerCode` | `string` | 容器编号。 |
| `WarehouseId` / `WarehouseCode` | `long` / `string?` | 仓库 ID / 仓库编码。 |
| `LocationId` / `LocationCode` | `long?` / `string?` | 货位 ID / 货位编码。 |
| `ZoneCode` | `string?` | 库区编码。 |
| `InventoryStatus` | `string` | 库存状态。 |
| `MaterialId` / `MaterialCode` | `long` / `string` | 物料 ID / 物料编码。 |
| `BatchNo` / `SerialNo` | `string?` / `string?` | 批次号 / 序列号。 |
| `Qty` / `LockedQty` | `decimal` / `decimal` | 库存数量 / 锁定数量。 |
| `Unit` | `string?` | 单位。 |
| `ProductionDate` / `ExpiryDate` | `DateOnly?` / `DateOnly?` | 生产日期 / 过期日期。 |
| `InboundDocNo` / `InboundTime` | `string?` / `DateTime?` | 入库单号 / 入库时间。 |
| `SupplierId` | `long?` | 供应商 ID。 |

| 方法签名 | 用途 | 输入参数中文说明 | 输出参数中文说明 | 示例 |
|---|---|---|---|---|
| `GetByFIFOAsync(string warehouseCode, string materialCode, CancellationToken cancellationToken = default)` | 按 FIFO 查询库存。 | `warehouseCode`：仓库编码；`materialCode`：物料编码。 | `List<InventoryInfoDTO>`：按入库时间排序的库存。 | `var list = await inventoryQuery.GetByFIFOAsync("WH01", "M001");` |
| `GetByFEFOAsync(string warehouseCode, string materialCode, CancellationToken cancellationToken = default)` | 按 FEFO 查询库存。 | `warehouseCode`：仓库编码；`materialCode`：物料编码。 | `List<InventoryInfoDTO>`：按过期日期排序的库存。 | `var list = await inventoryQuery.GetByFEFOAsync("WH01", "M001");` |
| `GetByBatchAsync(string warehouseCode, string materialCode, string batchNo, CancellationToken cancellationToken = default)` | 查询指定批次库存。 | `warehouseCode`：仓库编码；`materialCode`：物料编码；`batchNo`：批次号。 | `List<InventoryInfoDTO>`：指定批次库存。 | `var list = await inventoryQuery.GetByBatchAsync("WH01", "M001", "B001");` |
| `GetByMaterialAsync(string warehouseCode, string materialCode, CancellationToken cancellationToken = default)` | 查询指定物料库存。 | `warehouseCode`：仓库编码；`materialCode`：物料编码。 | `List<InventoryInfoDTO>`：物料库存。 | `var list = await inventoryQuery.GetByMaterialAsync("WH01", "M001");` |
| `GetByLocationAsync(string locationCode, CancellationToken cancellationToken = default)` | 查询指定货位库存。 | `locationCode`：货位编码。 | `List<InventoryInfoDTO>`：货位库存。 | `var list = await inventoryQuery.GetByLocationAsync("L0101");` |
| `GetByAreaAsync(string warehouseCode, string areaCode, string materialCode, CancellationToken cancellationToken = default)` | 查询指定区域库存。 | `warehouseCode`：仓库编码；`areaCode`：区域编码；`materialCode`：物料编码。 | `List<InventoryInfoDTO>`：区域库存。 | `var list = await inventoryQuery.GetByAreaAsync("WH01", "A01", "M001");` |

### 3.9 `ILocationQueryService`

返回模型 `MdLocationDTO` 字段：`Id`、`LocationCode`、`WarehouseId`、`WarehouseCode`、`ZoneId`、`ZoneCode`、`AisleNo`、`RowNo`、`ColNo`、`LayerNo`、`Side`、`Depth`、`LocationType`、`AbcClass`、`Status`、`LockStatus`、`IsDisabled`、`DisableReason`、`Remark`、`CreatedBy`、`CreatedByName`、`CreatedTime`、`LastModifiedBy`、`LastModifiedByName`、`LastModifiedTime`。

| 方法签名 | 用途 | 输入参数中文说明 | 输出参数中文说明 | 示例 |
|---|---|---|---|---|
| `GetEmptyLocationsAsync(long warehouseId, long? zoneId = null, string? locationType = null, CancellationToken cancellationToken = default)` | 查询空闲可用货位。 | `warehouseId`：仓库 ID；`zoneId`：库区 ID；`locationType`：货位类型。 | `List<MdLocationDTO>`：空闲货位列表。 | `var locations = await locationQuery.GetEmptyLocationsAsync(1, 10);` |
| `GetLocationsByZoneAsync(long zoneId, CancellationToken cancellationToken = default)` | 查询库区货位。 | `zoneId`：库区 ID。 | `List<MdLocationDTO>`：库区货位列表。 | `var locations = await locationQuery.GetLocationsByZoneAsync(10);` |
| `GetLocationsNearAsync(string locationCode, long warehouseId, int maxCount = 10, CancellationToken cancellationToken = default)` | 查询附近货位。 | `locationCode`：参考货位编码；`warehouseId`：仓库 ID；`maxCount`：最大数量。 | `List<MdLocationDTO>`：附近货位列表。 | `var locations = await locationQuery.GetLocationsNearAsync("L0101", 1, 5);` |
| `GetLocationCodesWithInventoryAsync(long warehouseId, long materialId, string? batchNo = null, CancellationToken cancellationToken = default)` | 查询已有指定物料库存的货位编码。 | `warehouseId`：仓库 ID；`materialId`：物料 ID；`batchNo`：批次号。 | `List<string>`：货位编码列表。 | `var codes = await locationQuery.GetLocationCodesWithInventoryAsync(1, 10001);` |
| `GetPairLocationAsync(long locationId, CancellationToken cancellationToken = default)` | 查询双深配对货位。 | `locationId`：参考货位 ID。 | `MdLocationDTO?`：配对货位，找不到为 `null`。 | `var pair = await locationQuery.GetPairLocationAsync(101);` |
| `GetFrontLocationStatusAsync(long locationId, CancellationToken cancellationToken = default)` | 查询前排货位状态。 | `locationId`：后排或参考货位 ID。 | `string?`：前排状态，如 `EMPTY`、`OCCUPIED`。 | `var status = await locationQuery.GetFrontLocationStatusAsync(102);` |

### 3.10 `IWarehouseQueryService`

常见输出模型字段：

| 模型 | 字段和中文说明 |
|---|---|
| `MdWarehouseZoneDTO` | `Id`、`WarehouseId`、`ZoneCode`、`ZoneName`、`ZoneType`、`ParentZoneId`、`AbcClass`、`SortNo`、`Status`。 |
| `MdAisleDTO` | `Id`、`AisleNo`、`AisleCode`、`AisleName`、`WarehouseId`、`ZoneId`、`EquipmentCode`、`Status`、`SortNo`、`Remark`。 |
| `MdPortDTO` | `Id`、`PortCode`、`PortName`、`WarehouseId`、`ZoneId`、`ConveyorLineId`、`PortType`、`EquipmentCode`、`Status`、`Remark`。 |
| `MdTransferPointDTO` | `Id`、`PointCode`、`PointName`、`WarehouseId`、`ZoneId`、`ConveyorLineId`、`AisleId`、`PointType`、`Status`。 |
| `LogicalZoneDTO` | `Id`、`ZoneCode`、`ZoneName`、`ZoneType`、`WarehouseId`、`PhysicalZones`。 |
| `LogicalZonePhysicalMapping` | `PhysicalZoneId`、`PhysicalZoneCode`、`PhysicalZoneName`、`PhysicalZoneType`、`Priority`。 |

| 方法签名 | 用途 | 输入参数中文说明 | 输出参数中文说明 | 示例 |
|---|---|---|---|---|
| `GetMaterialTurnoverClassAsync(long materialId, CancellationToken cancellationToken = default)` | 查询物料 ABC 周转分类。 | `materialId`：物料 ID。 | `string?`：分类编码，如 `A`、`B`、`C`。 | `var abc = await warehouseQuery.GetMaterialTurnoverClassAsync(10001);` |
| `GetZonesByAbcClassAsync(long warehouseId, string abcClass, CancellationToken cancellationToken = default)` | 按 ABC 分类查询库区。 | `warehouseId`：仓库 ID；`abcClass`：ABC 分类。 | `List<MdWarehouseZoneDTO>`：匹配库区。 | `var zones = await warehouseQuery.GetZonesByAbcClassAsync(1, "A");` |
| `GetAislesByZoneAsync(long zoneId, CancellationToken cancellationToken = default)` | 查询库区巷道。 | `zoneId`：库区 ID。 | `List<MdAisleDTO>`：巷道列表。 | `var aisles = await warehouseQuery.GetAislesByZoneAsync(10);` |
| `GetAisleLoadAsync(int aisleNo, long warehouseId, CancellationToken cancellationToken = default)` | 查询巷道负载。 | `aisleNo`：巷道号；`warehouseId`：仓库 ID。 | `(int Total, int Occupied)`：总货位数和已占用数。 | `var load = await warehouseQuery.GetAisleLoadAsync(1, 1);` |
| `GetLeastLoadedAisleAsync(long warehouseId, long? zoneId = null, CancellationToken cancellationToken = default)` | 查询负载最低巷道。 | `warehouseId`：仓库 ID；`zoneId`：库区 ID。 | `MdAisleDTO?`：巷道，找不到为 `null`。 | `var aisle = await warehouseQuery.GetLeastLoadedAisleAsync(1, 10);` |
| `GetPortByDocTypeAsync(long docTypeId, string direction, long warehouseId, CancellationToken cancellationToken = default)` | 按单据类型查询站台。 | `docTypeId`：单据类型 ID；`direction`：方向；`warehouseId`：仓库 ID。 | `MdPortDTO?`：站台。 | `var port = await warehouseQuery.GetPortByDocTypeAsync(5, AlgoConstants.PortDirection.INBOUND, 1);` |
| `GetZonesByWarehouseAsync(long warehouseId, CancellationToken cancellationToken = default)` | 查询仓库库区。 | `warehouseId`：仓库 ID。 | `List<MdWarehouseZoneDTO>`：库区列表。 | `var zones = await warehouseQuery.GetZonesByWarehouseAsync(1);` |
| `GetStorageZonesAsync(long warehouseId, CancellationToken cancellationToken = default)` | 查询存储类库区。 | `warehouseId`：仓库 ID。 | `List<MdWarehouseZoneDTO>`：可上架/存储库区。 | `var zones = await warehouseQuery.GetStorageZonesAsync(1);` |
| `GetLogicalZonesAsync(long warehouseId, string? zoneType = null, CancellationToken cancellationToken = default)` | 查询逻辑分区。 | `warehouseId`：仓库 ID；`zoneType`：逻辑分区类型。 | `List<LogicalZoneDTO>`：逻辑分区及物理映射。 | `var zones = await warehouseQuery.GetLogicalZonesAsync(1, AlgoConstants.ZoneType.LOGICAL_STORAGE);` |
| `GetInboundTransferPointAsync(long warehouseId, long aisleId, CancellationToken cancellationToken = default)` | 按巷道查询入库接驳点。 | `warehouseId`：仓库 ID；`aisleId`：巷道 ID。 | `MdTransferPointDTO?`：接驳点。 | `var point = await warehouseQuery.GetInboundTransferPointAsync(1, 100);` |
| `GetInboundPortByConveyorAsync(long warehouseId, long conveyorLineId, CancellationToken cancellationToken = default)` | 按输送线查询入库站台。 | `warehouseId`：仓库 ID；`conveyorLineId`：输送线 ID。 | `MdPortDTO?`：入库站台。 | `var port = await warehouseQuery.GetInboundPortByConveyorAsync(1, 20);` |
| `GetOutboundPortByAisleAsync(long warehouseId, int aisleNo, CancellationToken cancellationToken = default)` | 按巷道查询出库站台。 | `warehouseId`：仓库 ID；`aisleNo`：巷道号。 | `MdPortDTO?`：出库站台。 | `var port = await warehouseQuery.GetOutboundPortByAisleAsync(1, 3);` |

### 3.11 `IStrategyQueryService`

输出模型字段：`StrategyRuntimeInfo` 包含 `Code`、`Name`、`PolicyType`、`Author`、`Description`、`IsEnabled`、`Priority`；`StrategyChainRuntimeInfo` 包含 `Code`、`Name`；`StrategyTypeInfo` 包含 `Code`、`Name`、`StrategyCount`；`StrategyOptionsResult` 包含 `Types`、`RuleCodeMap`、`ParamSchemaMap`。

| 方法签名 | 用途 | 输入参数中文说明 | 输出参数中文说明 | 示例 |
|---|---|---|---|---|
| `GetRegisteredStrategies(string? strategyType = null)` | 查询已注册策略。 | `strategyType`：策略类型过滤，可空。 | `List<StrategyRuntimeInfo>`：策略运行时信息。 | `var list = strategyQuery.GetRegisteredStrategies("InventoryAllocation");` |
| `GetRegisteredChains()` | 查询已注册链。 | 无输入参数。 | `List<StrategyChainRuntimeInfo>`：链运行时信息。 | `var chains = strategyQuery.GetRegisteredChains();` |
| `GetStrategyTypes()` | 查询策略类型及数量。 | 无输入参数。 | `List<StrategyTypeInfo>`：策略类型信息。 | `var types = strategyQuery.GetStrategyTypes();` |
| `GetStrategyOptions()` | 查询配置选项。 | 无输入参数。 | `StrategyOptionsResult`：类型、规则编码、参数 Schema 聚合数据。 | `var options = strategyQuery.GetStrategyOptions();` |

### 3.12 `IStrategyConfigService`

输出模型 `CfgStrategyConfig` 字段：`StrategyCode`、`StrategyName`、`StrategyType`、`RuleCode`、`WarehouseId`、`ZoneId`、`MaterialId`、`MaterialCategoryId`、`DocType`、`Priority`、`IsDefault`、`Status`、`SortOrder`、`ExecutionMode`、`StrategyParams`、`Description`、`Remark`。

| 方法签名 | 用途 | 输入参数中文说明 | 输出参数中文说明 | 示例 |
|---|---|---|---|---|
| `GetPagedListAsync(int pageIndex, int pageSize, string? strategyType = null, string? keyword = null, long? warehouseId = null, byte? status = null)` | 分页查询策略配置。 | `pageIndex`：页码；`pageSize`：页大小；其余为过滤条件。 | `(Items, Total)`：配置列表和总数。 | `var page = await configService.GetPagedListAsync(1, 20, "InventoryAllocation");` |
| `GetByTypeAsync(string strategyType, long? warehouseId = null)` | 按类型查询启用配置。 | `strategyType`：策略类型；`warehouseId`：仓库 ID。 | `List<CfgStrategyConfig>`：配置列表。 | `var configs = await configService.GetByTypeAsync("InventoryAllocation", 1);` |
| `GetByCodeAsync(string strategyCode)` | 按配置编码查询。 | `strategyCode`：策略配置编码。 | `CfgStrategyConfig?`：配置，找不到为 `null`。 | `var config = await configService.GetByCodeAsync("INV_FIFO");` |
| `DeleteAsync(List<long> ids)` | 批量删除配置。 | `ids`：配置 ID 列表。 | `bool`：是否成功。 | `bool ok = await configService.DeleteAsync(new List<long> { 1, 2 });` |
| `ValidateParams(string? paramsJson, out string errorMessage)` | 校验参数 JSON。 | `paramsJson`：待校验 JSON；`errorMessage`：错误输出。 | `bool`：是否有效。 | `bool ok = configService.ValidateParams(json, out var error);` |
| `IsCodeUniqueAsync(string strategyCode, long? excludeId = null)` | 检查编码唯一性。 | `strategyCode`：配置编码；`excludeId`：排除 ID。 | `bool`：是否唯一。 | `bool unique = await configService.IsCodeUniqueAsync("INV_FIFO");` |

### 3.13 `IStrategyChainService`

输出模型：`CfgStrategyChainConfig` 字段包括 `ChainCode`、`ChainName`、`ChainType`、`WarehouseId`、`ZoneId`、`DocType`、`Priority`、`IsDefault`、`Status`、`StepCount`、`Description`、`Remark`；`CfgStrategyChainStep` 字段包括 `ChainId`、`StepNo`、`StepName`、`StrategyConfigId`、`RuleCode`、`StepMode`、`IsEnabled`、`StepParams`、`Remark`；`StrategyChainDetail` 包含 `Chain` 和 `Steps`。

| 方法签名 | 用途 | 输入参数中文说明 | 输出参数中文说明 | 示例 |
|---|---|---|---|---|
| `GetByTypeAsync(string chainType, long? warehouseId = null)` | 按类型查询链。 | `chainType`：链类型；`warehouseId`：仓库 ID。 | `List<CfgStrategyChainConfig>`：链配置列表。 | `var chains = await chainService.GetByTypeAsync("InventoryAllocation", 1);` |
| `GetByCodeAsync(string chainCode)` | 按编码查询链。 | `chainCode`：链编码。 | `CfgStrategyChainConfig?`：链配置。 | `var chain = await chainService.GetByCodeAsync("INV_CHAIN");` |
| `GetStepsByChainIdAsync(long chainId)` | 查询链步骤。 | `chainId`：链 ID。 | `List<CfgStrategyChainStep>`：步骤列表。 | `var steps = await chainService.GetStepsByChainIdAsync(1);` |
| `GetChainDetailAsync(long chainId)` | 查询链详情。 | `chainId`：链 ID。 | `StrategyChainDetail?`：链和步骤。 | `var detail = await chainService.GetChainDetailAsync(1);` |
| `CreateAsync(StrategyChainCreateRequest request)` | 创建链和步骤。 | `request.Chain`：链配置；`request.Steps`：步骤列表。 | `long`：新链 ID。 | `long id = await chainService.CreateAsync(request);` |
| `UpdateAsync(StrategyChainUpdateRequest request)` | 更新链和步骤。 | `request.Chain`：链配置；`request.Steps`：步骤列表。 | `bool`：是否成功。 | `bool ok = await chainService.UpdateAsync(request);` |
| `IsCodeUniqueAsync(string chainCode, long? excludeId = null)` | 检查链编码唯一性。 | `chainCode`：链编码；`excludeId`：排除 ID。 | `bool`：是否唯一。 | `bool unique = await chainService.IsCodeUniqueAsync("INV_CHAIN");` |
| `ValidateChainCompositionAsync(string chainType, params string[] requiredStrategyTypes)` | 校验链组成。 | `chainType`：链类型；`requiredStrategyTypes`：要求包含的策略类型。 | `List<string>`：警告列表，空列表表示通过。 | `var warnings = await chainService.ValidateChainCompositionAsync("InventoryAllocation", "InventoryAllocation");` |

### 3.14 `CranePathOptimizer`

#### `Optimize(List<PickingTaskItem> tasks, string pathMode)`

用途：对下架任务按堆垛机路径模式排序。

输入参数：

| 参数名 | 类型 | 必填 | 中文说明 | 示例值 |
|---|---|---:|---|---|
| `tasks` | `List<PickingTaskItem>` | 是 | 待排序下架任务。 | `picking.Tasks` |
| `pathMode` | `string` | 是 | 路径模式。 | `AlgoConstants.PathOptimization.S_SHAPE` |

输出参数：

| 返回类型 | 字段名 | 字段类型 | 中文说明 |
|---|---|---|---|
| `List<PickingTaskItem>` | `FromLocationCode` | `string` | 来源货位编码。 |
| `List<PickingTaskItem>` | `Qty` | `decimal` | 下架数量。 |
| `List<PickingTaskItem>` | `Priority` | `int` | 执行优先级。 |
| `List<PickingTaskItem>` | `InventoryDetailId` | `long?` | 库存明细 ID。 |

示例：

```csharp
List<PickingTaskItem> ordered = CranePathOptimizer.Optimize(
    picking.Tasks,
    AlgoConstants.PathOptimization.S_SHAPE);
```

---

## 4. 内置策略调用指南

### 4.1 策略总览

| 策略编码 | 策略类型 | 业务用途 | 输出模型 |
|---|---|---|---|
| `DEFAULT_PUTAWAY` | `Putaway` | 入库上架时推荐入库站台、目标库区、巷道、路径。 | `PutawayResult` |
| `ABC_CLASS` | `LocationAllocation` | 按物料 ABC 分类推荐货位。 | `LocationAllocationResult` |
| `CATEGORY_ZONE` | `LocationAllocation` | 按物料分类/品类推荐货位。 | `LocationAllocationResult` |
| `CENTRALIZED` | `LocationAllocation` | 相同物料或批次集中存放。 | `LocationAllocationResult` |
| `DOUBLE_DEEP` | `LocationAllocation` | 按双深货架前后排约束推荐货位。 | `LocationAllocationResult` |
| `FIFO` | `InventoryAllocation` | 按入库时间先进先出分配库存。 | `InventoryAllocationResult` |
| `FEFO` | `InventoryAllocation` | 按过期日期先到期先出分配库存。 | `InventoryAllocationResult` |
| `BATCH` | `InventoryAllocation` | 优先按指定批次分配库存。 | `InventoryAllocationResult` |
| `UTILIZATION_PRIORITY` | `InventoryAllocation` | 优先清空低库存货位。 | `InventoryAllocationResult` |
| `DEFAULT_PICKING` | `Picking` | 根据库存分配结果生成下架任务和路径。 | `PickingResult` |

### 4.2 `DEFAULT_PUTAWAY`

输入参数：

| 参数来源 | 参数名 | 类型 | 必填 | 中文说明 |
|---|---|---|---:|---|
| `PolicyContext` | `WarehouseId` | `long?` | 是 | 仓库 ID。 |
| `PolicyContext` | `MaterialId` | `long?` | 建议 | 物料 ID。 |
| `PolicyContext` | `MaterialCategoryId` | `long?` | 否 | 物料分类 ID。 |
| `ContextData` | `SVC_WAREHOUSE_QUERY` | `IWarehouseQueryService` | 是 | 仓库查询服务。 |
| `ContextData` | `PutawayInput.DOC_TYPE_ID` | `long?` | 否 | 单据类型 ID。 |
| `ContextData` | `Common.PATH_OPTIMIZATION` | `string` | 否 | 路径优化模式。 |

输出参数：

| 模型 | 字段 | 类型 | 中文说明 |
|---|---|---|---|
| `PutawayResult` | `InboundStationId` / `InboundStationCode` | `string?` / `string?` | 入库站台 ID / 编码。 |
| `PutawayResult` | `AisleId` / `AisleCode` | `long?` / `string?` | 巷道 ID / 编码。 |
| `PutawayResult` | `TargetZoneId` / `TargetZoneCode` | `long?` / `string?` | 目标库区 ID / 编码。 |
| `PutawayResult` | `Route` | `List<string>` | 上架路径。 |
| `PutawayResult` | `PathOptimization` | `string?` | 路径优化模式。 |
| `PutawayResult` | `RequireLocationAllocation` | `bool` | 是否需要继续调用货位分配。 |
| `PutawayResult` | `AllocationParameters` | `Dictionary<string, object?>` | 传给货位分配的参数。 |

示例：

```csharp
var context = new PolicyContext { WarehouseId = 1, MaterialId = 10001, BusinessCode = "IN001" };
context.SetData(StrategyParams.SVC_WAREHOUSE_QUERY, warehouseQueryService);
context.SetData(StrategyParams.PutawayInput.DOC_TYPE_ID, 5L);
context.SetData(StrategyParams.Common.PATH_OPTIMIZATION, AlgoConstants.PathOptimization.S_SHAPE);

var result = await policyEngine.ExecuteAsync("DEFAULT_PUTAWAY", context);
var putaway = GetOutput<PutawayResult>(result);
```

常见失败原因：仓库基础数据缺失、无可用巷道、查询服务未注册。

### 4.3 货位分配策略

适用策略：`ABC_CLASS`、`CATEGORY_ZONE`、`CENTRALIZED`、`DOUBLE_DEEP`。

输入参数：

| 参数来源 | 参数名 | 类型 | 必填 | 中文说明 |
|---|---|---|---:|---|
| `PolicyContext` | `WarehouseId` | `long?` | 是 | 仓库 ID。 |
| `PolicyContext` | `ZoneId` | `long?` | 否 | 限定库区。 |
| `PolicyContext` | `MaterialId` | `long?` | 按策略 | 物料 ID。 |
| `PolicyContext` | `MaterialCategoryId` | `long?` | 按策略 | 物料分类 ID。 |
| `ContextData` | `SVC_LOCATION_QUERY` | `ILocationQueryService` | 是 | 货位查询服务。 |
| `ContextData` | `SVC_WAREHOUSE_QUERY` | `IWarehouseQueryService` | 部分策略 | 仓库查询服务。 |
| `ContextData` | `LocationAllocationInput.MATERIAL_ABC_CLASS` | `string` | 否 | ABC 分类。 |
| `ContextData` | `LocationAllocationInput.TARGET_BATCH_NO` | `string` | 否 | 目标批次。 |
| `ContextData` | `LocationAllocationInput.MAX_RECOMMEND_COUNT` | `int?` | 否 | 最大推荐数量。 |
| `ContextData` | `LocationAllocationInput.SORT_RULES` | `string` | 否 | 排序规则 JSON。 |
| `ContextData` | `LocationAllocationInput.ENABLE_DOUBLE_DEEP` | `bool?` | 否 | 是否启用双深约束。 |
| `ContextData` | `LocationAllocationInput.DOUBLE_DEEP_MODE` | `string` | 否 | 双深模式。 |

输出参数：

| 模型 | 字段 | 类型 | 中文说明 |
|---|---|---|---|
| `LocationAllocationResult` | `Locations` | `List<LocationRecommendation>` | 推荐货位列表。 |
| `LocationRecommendation` | `LocationId` / `LocationCode` | `string` / `string` | 货位 ID / 编码。 |
| `LocationRecommendation` | `AisleNo`、`RowNo`、`ColNo`、`LayerNo` | `int?` | 巷道、行、列、层。 |
| `LocationRecommendation` | `Depth` / `Side` | `int?` / `int?` | 深度 / 巷道侧。 |
| `LocationRecommendation` | `ZoneCode` | `string?` | 库区编码。 |
| `LocationRecommendation` | `Score` | `decimal` | 评分。 |
| `LocationRecommendation` | `Reason` | `string` | 推荐原因。 |

示例：

```csharp
var context = new PolicyContext { WarehouseId = 1, MaterialId = 10001 };
context.SetData(StrategyParams.SVC_WAREHOUSE_QUERY, warehouseQueryService);
context.SetData(StrategyParams.SVC_LOCATION_QUERY, locationQueryService);
context.SetData(StrategyParams.LocationAllocationInput.MATERIAL_ABC_CLASS, "A");
context.SetData(StrategyParams.LocationAllocationInput.MAX_RECOMMEND_COUNT, 10);

var result = await policyEngine.ExecuteAsync("ABC_CLASS", context);
var allocation = GetOutput<LocationAllocationResult>(result);
```

策略差异：

| 策略编码 | 关键输入 | 中文说明 |
|---|---|---|
| `ABC_CLASS` | `WarehouseId`、`MaterialId`、`MATERIAL_ABC_CLASS` | 按 ABC 分类推荐。 |
| `CATEGORY_ZONE` | `WarehouseId`、`MaterialCategoryId` | 按品类/物料分类推荐。 |
| `CENTRALIZED` | `WarehouseId`、`MaterialId`、`TARGET_BATCH_NO` | 优先靠近已有同物料/同批次库存。 |
| `DOUBLE_DEEP` | `WarehouseId`、`ENABLE_DOUBLE_DEEP`、`DOUBLE_DEEP_MODE` | 按前后排约束推荐。 |

常见失败原因：货位已满、库区不匹配、双深约束过滤后无结果。

### 4.4 库存分配策略

适用策略：`FIFO`、`FEFO`、`BATCH`、`UTILIZATION_PRIORITY`。

输入参数：

| 参数来源 | 参数名 | 类型 | 必填 | 中文说明 |
|---|---|---|---:|---|
| `PolicyContext` | `WarehouseCode` | `string?` | 建议 | 仓库编码。 |
| `PolicyContext` | `BusinessCode` | `string?` | 建议 | 业务单据号。 |
| `ContextData` | `SVC_INVENTORY_QUERY` | `IInventoryQueryService` | 是 | 库存查询服务。 |
| `ContextData` | `InventoryAllocationInput.WAREHOUSE_CODE` | `string` | 是 | 仓库编码。 |
| `ContextData` | `InventoryAllocationInput.MATERIAL_CODE` | `string` | 是 | 物料编码。 |
| `ContextData` | `InventoryAllocationInput.REQUIRED_QTY` | `decimal` | 是 | 需求数量。 |
| `ContextData` | `InventoryAllocationInput.TARGET_BATCH_NO` | `string` | `BATCH` 必填 | 指定批次。 |

输出参数：

| 模型 | 字段 | 类型 | 中文说明 |
|---|---|---|---|
| `InventoryAllocationResult` | `Items` | `List<InventoryAllocationItem>` | 分配明细。 |
| `InventoryAllocationResult` | `TotalAllocatedQty` | `decimal` | 已分配总量。 |
| `InventoryAllocationResult` | `RequiredQty` | `decimal` | 需求数量。 |
| `InventoryAllocationResult` | `IsFullySatisfied` | `bool` | 是否满足需求。 |
| `InventoryAllocationResult` | `ShortageQty` | `decimal` | 缺口数量。 |
| `InventoryAllocationItem` | `InventoryDetailId` / `InventoryHeaderId` | `long` / `long` | 库存明细 ID / 库存头 ID。 |
| `InventoryAllocationItem` | `LocationId` / `LocationCode` | `string` / `string` | 货位 ID / 编码。 |
| `InventoryAllocationItem` | `ContainerCode` | `string?` | 容器编号。 |
| `InventoryAllocationItem` | `MaterialId` | `long` | 物料 ID。 |
| `InventoryAllocationItem` | `BatchNo` / `SerialNo` | `string?` / `string?` | 批次号 / 序列号。 |
| `InventoryAllocationItem` | `AvailableQty` / `AllocatedQty` | `decimal` / `decimal` | 可用数量 / 分配数量。 |
| `InventoryAllocationItem` | `ManufactureDate` / `ExpiryDate` / `InboundTime` | `DateOnly?` / `DateOnly?` / `DateTime?` | 生产、过期、入库时间。 |
| `InventoryAllocationItem` | `Priority` / `Reason` | `int` / `string` | 优先级 / 分配原因。 |

示例：

```csharp
var context = new PolicyContext { WarehouseCode = "WH01", BusinessCode = "SO001" };
context.SetData(StrategyParams.SVC_INVENTORY_QUERY, inventoryQueryService);
context.SetData(StrategyParams.InventoryAllocationInput.WAREHOUSE_CODE, "WH01");
context.SetData(StrategyParams.InventoryAllocationInput.MATERIAL_CODE, "M001");
context.SetData(StrategyParams.InventoryAllocationInput.REQUIRED_QTY, 10m);

var result = await policyEngine.ExecuteAsync("FIFO", context);
var allocation = GetOutput<InventoryAllocationResult>(result);
```

策略差异：

| 策略编码 | 关键输入 | 中文说明 |
|---|---|---|
| `FIFO` | 仓库编码、物料编码、需求数量 | 按入库时间先进先出。 |
| `FEFO` | 仓库编码、物料编码、需求数量 | 按过期日期先到期先出。 |
| `BATCH` | 仓库编码、物料编码、需求数量、目标批次 | 优先指定批次，不足按 FIFO 补充。 |
| `UTILIZATION_PRIORITY` | 仓库编码、物料编码、需求数量 | 优先清空低库存货位。 |

常见失败原因：无可用库存、库存不足、仓库编码或物料编码错误。

### 4.5 `DEFAULT_PICKING`

输入参数：

| 参数来源 | 参数名 | 类型 | 必填 | 中文说明 |
|---|---|---|---:|---|
| `PolicyContext` | `WarehouseId` | `long?` | 建议 | 仓库 ID。 |
| `ContextData` | `InventoryAllocationOutput.RESULT` | `InventoryAllocationResult` | 是 | 上游库存分配结果。 |
| `ContextData` | `SVC_WAREHOUSE_QUERY` | `IWarehouseQueryService` | 是 | 仓库查询服务。 |
| `ContextData` | `PickingInput.DESTINATION_STATION_ID` | `long` | 否 | 指定目标出库站台 ID。 |
| `ContextData` | `PickingInput.DESTINATION_STATION_CODE` | `string` | 否 | 指定目标出库站台编码。 |
| `ContextData` | `PickingInput.DOC_TYPE_ID` | `long?` | 否 | 单据类型 ID。 |
| `ContextData` | `PickingInput.SOURCE_AISLE_NO` | `int` | 否 | 来源巷道号。 |
| `ContextData` | `Common.PATH_OPTIMIZATION` | `string` | 否 | 路径优化模式。 |

输出参数：

| 模型 | 字段 | 类型 | 中文说明 |
|---|---|---|---|
| `PickingResult` | `Tasks` | `List<PickingTaskItem>` | 下架任务。 |
| `PickingResult` | `DestinationStationId` / `DestinationStationCode` | `long?` / `string?` | 目标站台 ID / 编码。 |
| `PickingResult` | `DestinationZoneId` / `DestinationZoneCode` | `long?` / `string?` | 目标区域 ID / 编码。 |
| `PickingResult` | `PickRoute` / `DeliveryRoute` | `List<string>` / `List<string>` | 取货路径 / 送达路径。 |
| `PickingResult` | `PathOptimization` | `string?` | 路径优化模式。 |
| `PickingResult` | `RequireInventoryAllocation` | `bool` | 是否需要库存分配。 |
| `PickingResult` | `AllocationParameters` | `Dictionary<string, object?>` | 下游参数。 |
| `PickingTaskItem` | `FromLocationId` / `FromLocationCode` | `string` / `string` | 来源货位 ID / 编码。 |
| `PickingTaskItem` | `AisleId` / `ContainerCode` | `long?` / `string?` | 巷道 ID / 容器编号。 |
| `PickingTaskItem` | `MaterialId` / `BatchNo` / `SerialNo` | `long` / `string?` / `string?` | 物料、批次、序列号。 |
| `PickingTaskItem` | `Qty` / `Priority` / `IsFullPallet` | `decimal` / `int` / `bool` | 数量、优先级、是否整托。 |
| `PickingTaskItem` | `InventoryDetailId` / `InventoryHeaderId` | `long?` / `long?` | 库存明细 ID / 库存头 ID。 |

示例：

```csharp
var inventoryResult = await policyEngine.ExecuteAsync("FIFO", context);
var allocation = GetOutput<InventoryAllocationResult>(inventoryResult);

context.SetData(StrategyParams.InventoryAllocationOutput.RESULT, allocation);
context.SetData(StrategyParams.SVC_WAREHOUSE_QUERY, warehouseQueryService);
context.SetData(StrategyParams.PickingInput.DESTINATION_STATION_CODE, "OUT01");

var pickingResult = await policyEngine.ExecuteAsync("DEFAULT_PICKING", context);
var picking = GetOutput<PickingResult>(pickingResult);
```

常见失败原因：未传入库存分配结果、库存分配明细为空、出库口或巷道基础数据缺失。

---

## 5. 常见错误与排查

| 现象 | 常见原因 | 处理方式 |
|---|---|---|
| `策略 {code} 未注册` | 未注册 `StrategyAutofacModule`，或策略未进入 `IPolicyRegistry`。 | 检查 DI 注册和 `IStrategyQueryService.GetRegisteredStrategies()`。 |
| `IsSuccess = true` 但 `IsHandled = false` | 策略跳过、未启用、不适用、无库存或无货位。 | 不要转换 `Output`，先看 `ErrorMessage`。 |
| `Output` 为 `null` | 策略未处理或执行失败。 | 先判断 `IsSuccess` 和 `IsHandled`。 |
| 库存分配无结果 | 仓库编码、物料编码、需求数量错误；库存状态不可用；未注入 `IInventoryQueryService`。 | 检查库存参数和库存数据。 |
| 货位分配无结果 | 仓库/库区参数错误，货位已满，双深约束过滤后为空。 | 检查货位状态和双深参数。 |
| 上架无法推荐巷道 | 仓库或库区缺少可用巷道。 | 检查仓库、库区、巷道基础数据。 |
| 下架任务为空 | 未先执行库存分配或分配结果为空。 | 先执行库存分配并写入 `InventoryAllocationOutput.RESULT`。 |
| 策略链未生效 | 链类型、仓库、库区、单据类型不匹配，或没有默认链。 | 检查 `CfgStrategyChainConfig`。 |
| 链步骤被跳过 | `StrategyConfigId` 不存在，或 `RuleCode` 未注册。 | 检查 `CfgStrategyChainStep` 和 `CfgStrategyConfig.RuleCode`。 |
| 参数 JSON 未生效 | `StepParams` 或 `StrategyParams` JSON 字段名不一致。 | 使用文档中的 `StrategyParams` 键名。 |

---

## 6. 兼容说明

- `PolicyType.OutboundAllocation` 当前仅作为历史兼容枚举值保留；新开发不应基于它新建策略链或调用流程。
- 出库库存选择应使用 `PolicyType.InventoryAllocation` 和 `FIFO`、`FEFO`、`BATCH`、`UTILIZATION_PRIORITY` 等库存分配策略。
- `PolicyType.Wave` 当前没有内置策略实现。文档不提供正式调用流程。
- 本文档未记录当前代码不存在的策略编码、结果模型或补货能力；如后续代码新增实现，应同步更新本文档。

## 继续阅读

- [学习路径](/learning-path)
- [培训资料下载](/training-materials)
- [架构总览](/backend/架构设计/KH.WMS架构总览)
