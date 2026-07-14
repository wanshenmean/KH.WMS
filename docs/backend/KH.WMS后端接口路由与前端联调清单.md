---
title: "KH.WMS 后端接口路由与前端联调清单"
description: "KH.WMS 后端接口路由与前端联调清单：说明适用场景、当前实现、设计边界与开发或排障入口。"
status: reference
audience: "前端开发人员与联调负责人"
reviewed: "2026-07-14"
sourcePaths:
  - "KH.WMS.Client/src"
---


# KH.WMS 后端接口路由与前端联调清单

> 本文用于接口速查和前后端联调培训。它不替代 Swagger，而是帮助新人知道接口从哪里来、前端应该怎么对接。

## 目录

- [1. 阅读路径](#1-阅读路径)
- [2. 默认 CRUD 端点](#2-默认-crud-端点)
- [3. 常见模块路由](#3-常见模块路由)
- [4. Swagger 联调步骤](#4-swagger-联调步骤)
- [5. 前端对接检查](#5-前端对接检查)
- [6. 自定义 Action 设计建议](#6-自定义-action-设计建议)
- [7. 路由命名规范](#7-路由命名规范)
- [8. 联调案例：新增物料品牌](#8-联调案例新增物料品牌)
- [9. 课后检查](#9-课后检查)

## 1. 阅读路径

1. 先读 `KH.WMS后端开发指引.md` 的路由与控制器章节。
2. 再读 `KH.WMS前端请求封装与接口开发指引.md`。
3. 联调时以 Swagger 为准，本文件作为路由和约定速查。

## 2. 默认 CRUD 端点

多数 Controller 继承 `CrudController<TEntity>` 或 `ExtDataCrudController<TEntity>`。

常用约定：

| 前端动作 | 后端端点 | 前端封装 |
| --- | --- | --- |
| 分页查询 | `POST /api/{module}/pagelist` | `useCrudApi(module).pageList` |
| 详情 | `GET /api/{module}/{id}` | `detail(id)` |
| 新增 | `POST /api/{module}/create` | `create(data)` |
| 修改 | `POST /api/{module}/update` | `update(data)` |
| 删除 | `DELETE /api/{module}/delete/{id}` | `delete(id)` |
| 表单配置 | `GET /api/{module}/form-config` | `formConfig()` |

注意：

- 实际端点以基类实现和 Swagger 为准。
- 自定义 Action 应优先返回 `ApiResponse`。
- 业务扩展接口需要明确路由名，避免与默认 CRUD 冲突。

## 3. 常见模块路由

### 基础资料

| 路由 | 模块 | 前端 API |
| --- | --- | --- |
| `api/material` | BaseDataModule | `src/api/basedata.js` |
| `api/material-category` | BaseDataModule | `src/api/basedata.js` |
| `api/material-unit` | BaseDataModule | `src/api/basedata.js` |
| `api/material-turnover` | BaseDataModule | `src/api/basedata.js` |
| `api/container` | BaseDataModule | `src/api/basedata.js` |
| `api/container-type` | BaseDataModule | `src/api/basedata.js` |
| `api/customer` | BaseDataModule | `src/api/basedata.js` |
| `api/supplier` | BaseDataModule | `src/api/basedata.js` |
| `api/turnover-class` | BaseDataModule | `src/api/basedata.js` |

### 仓储基础

| 路由 | 模块 | 前端 API |
| --- | --- | --- |
| `api/warehouse` | WarehouseModule | `src/api/warehouse.js` |
| `api/warehouse-zone` | WarehouseModule | `src/api/warehouse.js` |
| `api/aisle` | WarehouseModule | `src/api/warehouse.js` |
| `api/location` | WarehouseModule | `src/api/warehouse.js` |
| `api/logical-zone` | WarehouseModule | `src/api/warehouse.js` |
| `api/port` | WarehouseModule | `src/api/warehouse.js` |
| `api/conveyor-line` | WarehouseModule | `src/api/warehouse.js` |
| `api/transfer-point` | WarehouseModule | `src/api/warehouse.js` |

### 配置

| 路由 | 模块 | 前端 API |
| --- | --- | --- |
| `api/global-config` | ConfigModule | `src/api/system.js` 或配置 API |
| `api/ext-field` | ConfigModule | 配置页面 |
| `api/ext-field-type` | ConfigModule | 配置页面 |
| `api/document-type` | ConfigModule | 配置页面 |
| `api/document-status` | ConfigModule | 配置页面 |
| `api/document-field` | ConfigModule | 配置页面 |
| `api/document-type-port` | ConfigModule | 配置页面 |
| `api/code-rule` | ConfigModule | 配置页面 |
| `api/code-sequence` | ConfigModule | 配置页面 |

### 入库、库存、出库、任务

| 路由 | 模块 | 前端 API |
| --- | --- | --- |
| `api/inbound-order` | InboundModule | `src/api/inbound.js` |
| `api/inbound-order-line` | InboundModule | `src/api/inbound.js` |
| `api/inbound-container-bind` | InboundModule | `src/api/inbound.js` |
| `api/inventory-header` | InventoryModule | `src/api/inventory.js` |
| `api/inventory-detail` | InventoryModule | `src/api/inventory.js` |
| `api/inventory-movement` | InventoryModule | `src/api/inventory.js` |
| `api/inventory-freeze` | InventoryModule | `src/api/inventory.js` |
| `api/inventory-adjust` | InventoryModule | `src/api/inventory.js` |
| `api/inventory-stocktake` | InventoryModule | `src/api/inventory.js` |
| `api/outbound-order` | OutboundModule | `src/api/outbound.js` |
| `api/outbound-wave` | OutboundModule | `src/api/outbound.js` |
| `api/outbound-allocation` | OutboundModule | `src/api/outbound.js` |
| `api/task-header` | TaskModule | `src/api/task.js` |
| `api/task-confirm` | TaskModule | `src/api/task.js` |
| `api/transfer-plan` | TaskModule | `src/api/task.js` |
| `api/wcs-task-log` | TaskModule | `src/api/task.js` |
| `api/adhoc` | TaskModule | `src/api/adhoc.js` |

### 系统

| 路由 | 模块 | 前端 API |
| --- | --- | --- |
| `api/user` | SystemModule | `src/api/user.js` / `src/api/auth.js` |
| `api/role` | SystemModule | `src/api/system.js` |
| `api/permission` | SystemModule | `src/api/system.js` |
| `api/dict` | SystemModule | `src/api/system.js` |
| `api/parameter` | SystemModule | `src/api/system.js` |
| `api/operate-log` | SystemModule | `src/api/system.js` |
| `api/attachment` | SystemModule | `src/api/system.js` |
| `api/file` | SystemModule | 文件上传/下载 |

## 4. Swagger 联调步骤

1. 启动后端：`dotnet run --project KH.WMS.Server\KH.WMS.Server.csproj`。
2. 打开 `http://localhost:9291/swagger`。
3. 登录获取 Token。
4. 在 Swagger 授权框填入 Bearer Token。
5. 先验证后端接口，再联调前端页面。
6. 失败时复制 `TraceId` 给后端排查日志。

## 5. 前端对接检查

- 前端 API 文件是否位于对应业务域。
- 前端请求路径是否与 Controller `[Route]` 一致。
- 页面是否通过 `src/api` 封装调用，而不是直接散写 axios。
- 分页查询是否使用 `buildPageQuery`。
- 后端返回是否符合 `ApiResponse`。
- 自定义 Action 是否被 Swagger 正确识别。

## 6. 自定义 Action 设计建议

默认 CRUD 无法表达业务动作时，可以新增自定义 Action。

推荐场景：

- 任务确认。
- 出库分配。
- 容器绑定。
- 批量导入/导出。
- 获取动态表单配置。
- 状态推进。

设计建议：

- 路由使用业务动作命名，例如 `confirm`、`allocate`、`bind-container`。
- 请求体使用明确 DTO。
- 返回 `ApiResponse` 或 `ApiResponse<T>`。
- 失败时返回业务错误，不要直接抛通用 500。
- 涉及状态变化时要考虑事务。

示例：

```csharp
[HttpPost("confirm")]
public async Task<ApiResponse> ConfirmAsync(ConfirmRequest request)
{
    return await service.ConfirmAsync(request);
}
```

前端对应：

```js
export function confirmTask(data) {
  return request.post('/api/task-confirm/confirm', data)
}
```

## 7. 路由命名规范

推荐：

- 小写。
- 单词用 `-` 分隔。
- 路由表达资源，Action 表达动作。
- 与前端 API 文件业务域一致。

示例：

| 资源 | 推荐路由 |
| --- | --- |
| 物料 | `api/material` |
| 入库单 | `api/inbound-order` |
| 出库波次 | `api/outbound-wave` |
| 库存调整 | `api/inventory-adjust` |
| 任务确认 | `api/task-confirm` |

避免：

- 大小写混杂。
- 同一资源多个路由前缀。
- 路由中包含页面概念。
- Action 返回结构和其他接口不一致。

## 8. 联调案例：新增物料品牌

后端：

1. 新增实体 `MdMaterialBrand`。
2. 新增 Service/Interface。
3. 新增 Controller `[Route("api/material-brand")]`。
4. Swagger 验证 `pagelist/create/update/delete`。

前端：

1. 在 `src/api/basedata.js` 增加品牌 API 或使用 `useCrudApi('material-brand')`。
2. 在 `src/views/basedata/material-brand.vue` 新增页面。
3. 菜单 `component` 配置为 `basedata/material-brand`。
4. 联调列表、新增、编辑、删除。

验收：

- Swagger 成功。
- 前端页面成功。
- 菜单可访问。
- 权限按钮正确。
- 错误时有 `message` 和 `traceId`。

## 9. 课后检查

- 能根据前端页面找到后端 Controller。
- 能根据后端路由找到前端 API 文件。
- 能用 Swagger 验证接口。
- 能说明默认 CRUD 端点和自定义端点的区别。


## 继续阅读

- [前端开发指引 V3.0](/backend/KH.WMS前端开发指引%20V3.0)
- [前端架构设计思路](/backend/架构设计/KH.WMS前端架构设计思路)
- [前后端联调与接口契约](/backend/KH.WMS前后端联调与接口契约指引)
