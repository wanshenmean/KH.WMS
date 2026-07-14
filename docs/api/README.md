---
title: "KH.WMS 三个基础模块 API 文档"
description: "KH.WMS 三个基础模块 API 文档：说明适用场景、当前实现、设计边界与开发或排障入口。"
status: reference
audience: "接口调用方、扩展开发人员与模块维护者"
reviewed: "2026-07-14"
sourcePaths:
  - "KH.WMS"
---

# KH.WMS 三个基础模块 API 文档

本文档集面向 `0.KH.WMS` 的后端开发者和 DLL/API 使用者，覆盖：

- `KH.WMS.Algorithms`：策略引擎、内置算法、策略配置与查询端点。
- `KH.WMS.Config`：配置 CRUD、分层配置解析、状态流转和扩展字段契约。
- `KH.WMS.Core`：通用 CRUD、响应模型、仓储、事务、缓存、认证、日志、安全和宿主扩展。

## 文档入口

| 文档 | 内容 |
| --- | --- |
| [KH.WMS.Algorithms API](./KH.WMS.Algorithms-API.md) | 策略执行、扩展点、内置策略编码、HTTP API |
| [KH.WMS.Config API](./KH.WMS.Config-API.md) | 配置资源端点、配置解析、状态校验、扩展字段 |
| [KH.WMS.Core API](./KH.WMS.Core-API.md) | Core 公共服务、通用 HTTP 契约、License API |
| [公开类型索引](./PUBLIC-TYPE-INDEX.md) | 三个程序集全部公开顶层类型，按命名空间分组 |

仓库中已有的深度手册仍可作为补充阅读：

- [Algorithms 外部调用与扩展手册](../backend/KH.WMS.Algorithms外部调用与扩展手册.md)
- [Core API 参考文档](../backend/KH.WMS.Core-API-参考文档.md)
- [后端配置驱动开发指引](../backend/KH.WMS后端配置驱动开发指引.md)

## 版本基线

| 项目 | 值 |
| --- | --- |
| GitHub 仓库 | `Z8018/0.KH.WMS` |
| 本地分支 | `simple_dev` |
| 源码提交 | `132d47d88ba25e0353289d65ceb2825d3aafa7f0` |
| 文档生成日期 | 2026-07-10 |
| 目标框架 | `.NET 8.0` |
| `KH.WMS.Algorithms` 程序集版本 | `0.1.0.0` |
| `KH.WMS.Config` 程序集版本 | `1.0.0.0` |
| `KH.WMS.Core` 程序集版本 | `0.1.0.0` |

> 版本说明：本文以当前工作区检出的上述提交为源码基线。远端连接未用于推断任何 API；签名、路由和继承关系均从本地源码及成功编译的程序集提取。

## 覆盖方式

文档同时覆盖两种 API：

1. **HTTP API**：控制器路由、HTTP 方法、请求体、响应结构和继承自 `CrudController<TEntity>` 的端点。
2. **.NET API**：供模块间调用或 DLL 使用者注入的接口、扩展方法、上下文与结果模型。

编译程序集得到的公开面统计如下。方法和属性数量只统计各类型“自身声明”的公共成员，不重复计算继承成员。

| 程序集 | 公开顶层类型 | 自身声明的公共方法 | 自身声明的公共属性 |
| --- | ---: | ---: | ---: |
| `KH.WMS.Algorithms` | 84 | 155 | 420 |
| `KH.WMS.Config` | 88 | 81 | 235 |
| `KH.WMS.Core` | 236 | 591 | 284 |

## 全局 HTTP 约定

### 认证

`KH.WMS.Server` 默认注册全局 `ApiAuthorizeFilter`。除非端点标记 `[AllowAnonymous]`，请求应携带：

```http
Authorization: Bearer <access-token>
Content-Type: application/json
```

### JSON 命名

默认宿主将 `System.Text.Json` 属性命名策略设为 camelCase。例如 C# 属性 `PageIndex` 在请求中写作 `pageIndex`。

### 统一响应

通用 CRUD 和多数查询接口返回 `ApiResponse`：

```json
{
  "code": 200,
  "message": "操作成功",
  "timestamp": 1783651200000,
  "data": {},
  "traceId": "..."
}
```

部分 Algorithms 配置端点返回 `{ success, data, message }`，Config 的若干专用端点直接返回字符串、布尔值、列表或空响应。各模块文档已单独标明。

### 通用 CRUD 路由

继承 `CrudController<TEntity>` 的资源默认具有以下端点，其中 `{base}` 是控制器声明的 `[Route]`：

| 方法 | 路由 | 请求 | 说明 |
| --- | --- | --- | --- |
| `GET` | `{base}/{id}` | 路径参数 `id` | 按 ID 查询 |
| `POST` | `{base}/pagelist` | `AdvancedQueryRequestDto` | 分页、过滤、排序 |
| `GET` | `{base}/all` | 无 | 查询全部 |
| `POST` | `{base}/create` | 实体 JSON | 新增 |
| `POST` | `{base}/update` | 实体 JSON | 更新 |
| `DELETE` | `{base}/delete/{id}` | 路径参数 `id` | 删除 |
| `DELETE` | `{base}/batch` | `long[]` | 批量删除 |
| `PUT` | `{base}/status/{id}` | `{ "status": 0|1 }` | 仅实现 `IEnableDisableEntity` 的实体可用 |
| `POST` | `{base}/export` | `ExportRequestDto` | `data` 返回 Base64 Excel |
| `POST` | `{base}/import` | `multipart/form-data`，字段 `file` | 导入 Excel |
| `GET` | `{base}/template` | 无 | `data` 返回 Base64 模板 |

分页请求示例：

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "keyword": "FIFO",
  "sortConditions": [
    { "field": "sortOrder", "direction": "asc" }
  ],
  "filters": [
    { "field": "status", "operator": "equals", "value": 1 }
  ]
}
```

支持的过滤操作符为 `equals`、`contains`、`gt`、`lt`、`gte`、`lte`、`in`、`notnull`、`isnull`、`startswith`、`endwith`。

## 校验结果

下列项目已在文档基线上执行并通过：

```text
dotnet build KH.WMS/Algorithms/KH.WMS.Algorithms/KH.WMS.Algorithms.csproj --no-restore
dotnet build KH.WMS/Config/KH.WMS.Config/KH.WMS.Config.csproj --no-restore
dotnet build KH.WMS/KH.WMS.Core/KH.WMS.Core.csproj --no-restore
```

三个项目均为 0 警告、0 错误。

## 继续阅读

- [公开类型索引](/api/PUBLIC-TYPE-INDEX)
- [跨模块 Contract](/backend/KH.WMS后端Contract与模块协作指引)
