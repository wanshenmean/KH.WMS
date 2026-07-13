# 09 跨模块 Contract 契约

## 这个概念解决什么问题

跨模块 Contract 解决的是“模块之间怎么调用，但不互相绑死实现”的问题。

在 KH.WMS 中，一个模块需要另一个模块的能力时，不应该直接引用对方模块的 Service。正确方式是：

- 公共接口和请求模型放在 `KH.WMS.Contracts`。
- 具体实现放在被调用模块的 `Contracts/` 目录。
- 实现类用 `[RegisteredService(ServiceType = typeof(接口))]` 暴露。
- 调用方只注入接口。

这样可以降低模块耦合，避免循环引用，也让每个模块保留自己的业务规则和数据访问边界。

## 什么时候需要看

- 入库模块要创建任务。
- 出库模块要扣减库存或锁定库存。
- 任务模块完成后要通知库存或单据模块。
- 出现模块间循环引用。
- 想直接注入另一个模块的 `XxxService`，但不确定是否合理。

## 业务开发应该怎么用

### 调用库存能力

调用方注入 `IInventoryContract`：

```csharp
public class OutboundService(IInventoryContract inventoryContract)
{
    public async Task<ServiceResult> AllocateAsync(long detailId, decimal qty)
    {
        var lockedQty = await inventoryContract.LockInventoryAsync(detailId, qty);
        if (lockedQty == null)
            return ServiceResult.Fail("库存可用量不足");

        return ServiceResult.Ok();
    }
}
```

调用方不应该直接引用 `InventoryContract` 或库存模块内部 Service。

### 调用任务能力

入库组盘后创建上架任务，调用 `ITaskContract`：

```csharp
var result = await _taskContract.CreatePutawayTaskAsync(request);
if (!result.Success)
    return ServiceResult.Fail(result.Message);
```

任务编号生成、任务头/行创建由 Task 模块负责，入库模块只提交请求。

## 底层逻辑和实现

### 接口放哪里

业务模块间契约统一放在公共 Contracts 项目：

```text
KH.WMS/Contracts/KH.WMS.Contracts/
  Inventory/IInventoryContract.cs
  Task/ITaskContract.cs
  Warehouse/ILocationContract.cs
  BaseData/IMaterialContract.cs
  Container/IContainerContract.cs
  Inbound/IInboundOrderContract.cs
  Outbound/IOutboundContract.cs
```

### 实现放哪里

实现放在能力所属模块：

```text
KH.WMS/Modules/InventoryModule/.../Contracts/InventoryContract.cs
KH.WMS/Modules/TaskModule/.../Contracts/TaskContract.cs
KH.WMS/Modules/WarehouseModule/.../Contracts/LocationContract.cs
KH.WMS/Modules/BaseDataModule/.../Contracts/MaterialContract.cs
```

例如库存契约实现：

```csharp
[RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(IInventoryContract))]
public class InventoryContract(IUnitOfWork unitOfWork) : IInventoryContract
{
}
```

### Contract 和 Service 的区别

| 类型 | 面向谁 | 放什么 |
| --- | --- | --- |
| Service | 本模块 Controller 和本模块内部流程 | 完整业务流程、CRUD、页面接口 |
| Contract | 其他模块 | 稳定、窄口径、跨模块能力 |

Contract 不应该暴露本模块内部所有 Service 方法，只暴露跨模块确实需要的能力。

### 事务边界谁负责

Contract 不自动替调用方兜住整个业务流程的事务。

例如入库组盘流程里：

1. 入库模块开启事务。
2. 调用容器 Contract 注册容器。
3. 写组盘头和明细。
4. 调用任务 Contract 创建任务。
5. 更新容器状态。
6. 提交事务。

这里的原子性由入库流程外层控制。Contract 内部可以做自己的数据写入，但不能替外层决定整个流程何时提交。

如果 Contract 内部直接用同一个请求作用域里的 `IUnitOfWork` 获取仓储，它会参与当前事务上下文。

## Contract 调用链实例

### 入库调用任务 Contract

`InboundContainerBindService` 申请上架时：

1. 入库 Service 校验组盘头状态。
2. 入库 Service 调用策略引擎得到上架目标。
3. 入库 Service 构造 `PutawayTaskRequest`。
4. 调用 `_taskContract.CreatePutawayTaskAsync(request)`。
5. `TaskContract` 内部调用 `ICodeGeneratorService.GenerateAsync` 生成任务号。
6. `TaskContract` 用 `IUnitOfWork.GetRepository<TaskHeader,long>()` 写任务头。
7. `TaskContract` 用 `IUnitOfWork.GetRepository<TaskLine,long>()` 写任务行。
8. 返回 `ServiceResult<string>`，Data 是任务号。
9. 入库 Service 根据结果继续更新组盘状态和容器状态。

这里 Task 模块不需要知道“入库为什么要创建上架任务”，只负责“给定请求，创建任务”。

### 库存 Contract 的并发语义

`IInventoryContract.LockInventoryAsync` 不只是简单改字段，它用条件更新保证可用量足够才增加 `LockedQty`：

```text
WHERE Id = inventoryDetailId
  AND (Qty - LockedQty) >= lockQty
```

所以调用方看到 `null` 时，含义不是“系统异常”，而是“库存不存在、参数无效、可用量不足或并发下被别人先锁定”。这类返回要在业务流程里转成明确失败消息。

## Contract 设计颗粒度

一个好的 Contract 方法应该：

- 名字表达业务能力，而不是数据库操作。
- 请求参数是调用方能理解的 DTO。
- 返回结果表达业务结果，而不是直接返回内部实体过多细节。
- 不泄露被调用模块的 Repository、Service、查询实现。

反例：

```csharp
Task<IRepository<TaskHeader, long>> GetTaskRepositoryAsync();
```

这种把内部仓储暴露给外部模块，会让模块边界失效。

更好的方式：

```csharp
Task<ServiceResult<string>> CreatePutawayTaskAsync(PutawayTaskRequest request);
```

## Contract 返回值选择

| 场景 | 返回值建议 | 说明 |
| --- | --- | --- |
| 成功/失败 + 消息 | `ServiceResult` | 适合内部流程 |
| 成功/失败 + 数据 | `ServiceResult<T>` | 例如创建任务返回任务号 |
| 简单查询判断 | `bool` / `decimal` / DTO | 例如容器是否有库存、容器数量 |
| 找不到或失败可区分 | 可空类型或 `ServiceResult<T>` | 例如库存锁定失败返回 null |
| 对前端 HTTP 响应 | 不建议 Contract 返回 `ApiResponse` | Contract 不绑定 HTTP |

## 版本和兼容性

Contract 一旦被多个模块调用，就要比模块内部接口更谨慎：

- 不要随意改方法签名。
- 新增字段优先给 DTO 加可空属性。
- 不要删除调用方还在用的返回字段。
- 行为变更要回查所有调用点。

排查调用点：

```powershell
rg "IInventoryContract|ITaskContract|ILocationContract|IMaterialContract|IContainerContract|IOutboundContract|IInboundOrderContract" KH.WMS
```

## 最小示例

### 定义契约

```csharp
namespace KH.WMS.Contracts.Inventory;

public interface IInventoryContract
{
    Task<decimal?> LockInventoryAsync(long inventoryDetailId, decimal lockQty);
}
```

### 实现契约

```csharp
[RegisteredService(ServiceType = typeof(IInventoryContract))]
public class InventoryContract(IUnitOfWork unitOfWork) : IInventoryContract
{
    public async Task<decimal?> LockInventoryAsync(long inventoryDetailId, decimal lockQty)
    {
        var repo = unitOfWork.GetRepository<InvInventoryDetail, long>();
        ...
    }
}
```

### 调用契约

```csharp
public class OutboundAllocationService(IInventoryContract inventoryContract)
{
    public async Task<ServiceResult> LockAsync(long detailId, decimal qty)
    {
        var result = await inventoryContract.LockInventoryAsync(detailId, qty);
        return result == null
            ? ServiceResult.Fail("库存锁定失败")
            : ServiceResult.Ok();
    }
}
```

## 常见误区与排查清单

- 调用方直接注入对方模块 `XxxService`：改成 Contract。
- Contract 接口放在实现模块项目里：调用方会被迫引用实现模块，容易循环引用。
- Contract 暴露过多内部方法：收窄成跨模块真正需要的能力。
- Contract 实现没加 `[RegisteredService(ServiceType=...)]`：运行时无法注入。
- 以为 Contract 会自动开完整业务事务：跨模块流程事务由外层业务流程控制。
- 在 Contract 里返回 `ApiResponse` 给另一个 Service：内部流程更适合 `ServiceResult` 或明确业务结果。
