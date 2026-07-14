---
title: "KH.WMS 后端 Contract 与模块协作指引"
description: "KH.WMS 后端 Contract 与模块协作指引：说明适用场景、当前实现、设计边界与开发或排障入口。"
status: reference
audience: "参与 KH.WMS 开发、测试与运维的团队成员"
reviewed: "2026-07-14"
sourcePaths:
  - "KH.WMS/KH.WMS.Server"
  - "KH.WMS/KH.WMS.Core"
  - "KH.WMS/Modules"
---

# KH.WMS 后端 Contract 与模块协作指引

> 本文用于培训跨模块调用规则。目标是让成员知道什么时候新增 Contract，怎么定义 Contract，以及为什么不能直接引用其他模块的 Service。

## 目录

- [1. 阅读路径](#1-阅读路径)
- [2. Contract 的作用](#2-contract-的作用)
- [3. 什么时候新增 Contract](#3-什么时候新增-contract)
- [4. 放置规则](#4-放置规则)
- [5. 定义方、实现方、调用方职责](#5-定义方实现方调用方职责)
- [6. 事务边界](#6-事务边界)
- [7. 常见 Contract](#7-常见-contract)
- [8. Contract 设计规范](#8-contract-设计规范)
- [9. 培训案例：入库调用任务和库存](#9-培训案例入库调用任务和库存)
- [10. 禁止做法](#10-禁止做法)
- [11. 课后检查](#11-课后检查)

## 1. 阅读路径

1. 先读 `KH.WMS后端开发指引.md` 第 2 章“后端的拆分与依赖”。
2. 再读本文掌握跨模块调用规范。
3. 最后结合具体模块代码查看 `KH.WMS/Contracts/KH.WMS.Contracts` 和各模块 `Contracts` / `Services`。

## 2. Contract 的作用

Contract 是模块之间的协议层，用于隔离模块实现。

没有 Contract 时：

```text
InboundModule -> InventoryModule.Services.InventoryService
```

推荐方式：

```text
InboundModule -> KH.WMS.Contracts.Inventory.IInventoryContract
              -> InventoryModule 实现该接口
```

## 3. 什么时候新增 Contract

需要新增 Contract 的情况：

- 一个模块需要调用另一个模块的业务能力。
- 某个能力会被多个模块复用。
- 调用方不应该知道被调模块的 Service 实现细节。
- 需要跨模块传递稳定请求对象或事件对象。

不一定需要新增 Contract 的情况：

- 模块内部 Controller 调自己的 Service。
- 纯实体查询且没有跨模块行为。
- 只在当前模块内部使用的辅助方法。

## 4. 放置规则

| 类型 | 推荐位置 | 示例 |
| --- | --- | --- |
| 跨模块接口 | `KH.WMS/Contracts/KH.WMS.Contracts/{业务域}` | `IInventoryContract.cs` |
| 跨模块请求对象 | `KH.WMS/Contracts/KH.WMS.Contracts/{业务域}` | `InventoryGenerationRequest.cs` |
| 跨模块事件 | `KH.WMS/Contracts/KH.WMS.Contracts/Events` | `InboundCompletedEvent.cs` |
| 实现类 | 对应业务模块 `Services` 或 `Contracts` | `InventoryContract.cs` |
| 模块内部协作 | 当前模块 `Contracts` | `MaterialContract.cs` |

## 5. 定义方、实现方、调用方职责

定义方：

- 将接口和请求对象放在 `KH.WMS.Contracts`。
- 保持接口语义稳定。
- 不暴露模块内部 Service 类型。

实现方：

- 在自己模块实现 Contract。
- 使用 `[RegisteredService(ServiceType = typeof(IXxxContract))]` 注册。
- 负责本模块业务一致性。

调用方：

- 注入 `IXxxContract`。
- 不引用对方模块 `Services`。
- 控制上层业务事务边界。

## 6. 事务边界

常见原则：

- 调用方是业务流程编排者时，由调用方控制事务。
- 被调 Contract 不应随意开启自己的独立事务。
- 如果被调能力必须独立提交，应在接口语义中明确说明。

示例：

```text
InboundOrderService
  -> BeginTransaction
  -> IContainerContract.UpdateStatusAsync
  -> ITaskContract.CreatePutawayTaskAsync
  -> Commit
```

## 7. 常见 Contract

| Contract | 定义位置 | 典型调用方 | 用途 |
| --- | --- | --- | --- |
| `IContainerContract` | `KH.WMS.Contracts/Container` | Inbound、Task | 容器注册、状态变更 |
| `IInventoryContract` | `KH.WMS.Contracts/Inventory` | Inbound、Outbound、Task | 生成、锁定、扣减库存 |
| `ITaskContract` | `KH.WMS.Contracts/Task` | Inbound、Outbound、Transfer | 创建上架/拣选任务 |
| `ILocationContract` | `KH.WMS.Contracts/Warehouse` | Task、Inventory、Algorithms | 库位查询、占用、释放 |
| `IInboundOrderContract` | `KH.WMS.Contracts/Inbound` | Task | 推进入库单状态 |
| `IOutboundContract` | `KH.WMS.Contracts/Outbound` | Task、Inventory | 推进出库单/波次状态 |

## 8. Contract 设计规范

接口命名：

- 使用 `I{业务对象}Contract`。
- 方法名表达业务能力，而不是表达数据库操作。
- 请求对象使用 `{动作}Request` 或 `{业务}Request`。
- 返回值使用稳定 DTO、`ServiceResult<T>` 或 `ApiResponse<T>`，按现有模块风格保持一致。

推荐：

```csharp
Task<ServiceResult<string>> CreatePutawayTaskAsync(PutawayTaskRequest request);
Task<long> GenerateInventoryFromPutawayAsync(InventoryGenerationRequest request);
```

不推荐：

```csharp
Task<object> DoSomething(dynamic data);
Task<bool> UpdateTable(string table, object row);
```

请求对象设计：

- 只包含跨模块协作所需字段。
- 不引用调用方页面 DTO。
- 不直接暴露被调模块内部实体，除非该实体本就是公共领域对象。
- 字段命名要和业务含义一致。

返回值设计：

- 创建类方法返回业务编号或主键。
- 校验类方法返回明确成功/失败和原因。
- 查询类方法返回稳定 DTO。
- 不要返回匿名对象或 `dynamic`。

版本兼容：

- Contract 一旦被多个模块使用，不要随意删除字段或改语义。
- 新增字段优先保持可选。
- 行为变化要同步培训文档和联调文档。

## 9. 培训案例：入库调用任务和库存

入库收货上架链路：

```text
InboundOrderService
  -> IContainerContract.Register / UpdateStatus
  -> ITaskContract.CreatePutawayTaskAsync
  -> TaskConfirmService
  -> IInventoryContract.GenerateInventoryFromPutawayAsync
  -> IInboundOrderContract.MarkLinePutawayAsync
```

课堂提问：

- 哪个模块是流程编排者？
- 哪个模块拥有容器状态？
- 哪个模块拥有任务生命周期？
- 哪个模块拥有库存数量？
- 事务应该由谁控制？
- 如果库存生成失败，入库单状态能不能推进？

标准答案倾向：

- 流程编排者通常是当前业务动作所在 Service。
- 数据归属模块负责修改自己的数据。
- 跨模块通过 Contract 暴露能力。
- 主流程事务由编排者控制，Contract 不私自提交独立事务。

## 10. 禁止做法

| 禁止做法 | 风险 |
| --- | --- |
| 一个模块直接 `using` 另一个模块 `Services` | 依赖方向混乱，改一个模块影响一大片 |
| Contract 请求对象引用调用方 DTO | 接口不稳定，模块互相泄露细节 |
| 被调 Contract 私自提交事务 | 上层流程无法回滚 |
| Contract 返回匿名对象 | 调用方无法形成稳定依赖 |
| Contract 中塞页面展示字段 | 后端协作协议被前端展示绑死 |

## 11. 课后检查

- 能判断跨模块调用是否需要 Contract。
- 能说出 Contract 定义方、实现方、调用方职责。
- 能解释为什么不能直接引用其他模块 Service。
- 能识别事务边界是否放错。

## 继续阅读

- [学习路径](/learning-path)
- [培训资料下载](/training-materials)
- [架构总览](/backend/架构设计/KH.WMS架构总览)
