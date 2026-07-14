---
title: "KH.WMS 后端测试与验收指引"
description: "KH.WMS 后端测试与验收指引：说明适用场景、当前实现、设计边界与开发或排障入口。"
status: reference
audience: "参与 KH.WMS 开发、测试与运维的团队成员"
reviewed: "2026-07-14"
sourcePaths:
  - "KH.WMS/KH.WMS.Server"
  - "KH.WMS/KH.WMS.Core"
  - "KH.WMS/Modules"
---

# KH.WMS 后端测试与验收指引

> 本文定义后端功能完成后的基础验证标准。培训目标是让成员知道“功能写完后怎么证明它能用”。

## 目录

- [1. 阅读路径](#1-阅读路径)
- [2. 基础命令](#2-基础命令)
- [3. 功能验收清单](#3-功能验收清单)
- [4. 常见验收场景](#4-常见验收场景)
- [5. 不通过标准](#5-不通过标准)
- [6. 验收记录模板](#6-验收记录模板)
- [7. 培训演练建议](#7-培训演练建议)
- [8. 课后检查](#8-课后检查)

## 1. 阅读路径

1. 先读 `KH.WMS后端开发指引.md` 的快速命令、接口、错误处理。
2. 再读本文执行验收。
3. 与前端联调时同步阅读 `KH.WMS前后端联调与接口契约指引.md`。

## 2. 基础命令

```powershell
cd D:\Git\0.KH.WMS\KH.WMS
dotnet restore KH.WMS.sln
dotnet build KH.WMS.sln
dotnet run --project KH.WMS.Server\KH.WMS.Server.csproj
```

访问入口：

- Swagger：`http://localhost:9291/swagger`
- MiniProfiler：`http://localhost:9291/profiler`

## 3. 功能验收清单

### 编译验收

- `dotnet restore` 成功。
- `dotnet build` 成功。
- 没有新增无法解析服务的启动错误。
- Swagger 能正常打开。

### 接口验收

- 新增 Controller 能在 Swagger 中看到。
- 默认 CRUD 端点可访问。
- 自定义 Action 路由正确。
- 成功响应符合 `ApiResponse`。
- 失败响应有明确 `Code`、`Message`、`TraceId`。

### 数据验收

- 新增实体表映射正确。
- 新增、修改、删除、分页查询可用。
- 主从表保存时明细没有丢失。
- 多表写入失败时能回滚。
- 枚举、时间、扩展字段序列化正常。

### 权限验收

- 未登录访问受保护接口返回 401。
- 无权限用户访问受保护资源返回 403 或业务拒绝。
- 菜单/按钮权限能被前端正确消费。

### 配置验收

- 字典新增/修改后前端可刷新看到。
- 扩展字段配置后 `/form-config` 返回正确。
- 状态流转不允许非法跳转。
- 编码规则生成编号符合预期。

### 日志验收

- 失败请求能在日志中按 TraceId 查到。
- 关键业务流程建议带 `X-Business-ID`。
- 异常不吞掉堆栈。
- 慢 SQL 或慢调用可通过 MiniProfiler 辅助定位。

## 4. 常见验收场景

新增基础资料维护：

- 新增实体、Service、Controller。
- Swagger 测 `pagelist/create/update/delete`。
- 前端页面联调列表、表单、删除。

新增跨模块业务动作：

- 验证 Contract 是否正确注册。
- 验证调用链事务回滚。
- 验证被调模块状态是否正确变化。
- 验证日志可追踪。

新增配置能力：

- 验证配置页面可维护。
- 验证业务服务读取配置。
- 验证配置缺失时有默认值或明确错误。

## 5. 不通过标准

以下情况不能算完成：

- 只在本地页面看起来能点，但 Swagger 不可验证。
- 失败时返回 500 且没有业务错误信息。
- 多表写入失败后出现部分数据。
- 前端需要硬编码后端状态文案才能展示。
- 新接口没有权限或权限无法验证。

## 6. 验收记录模板

建议每个功能验收记录包含：

```text
功能名称：
涉及模块：
后端接口：
前端页面：
数据库对象：
配置项：
权限项：
验证环境：
验证账号：
验证步骤：
验证结果：
失败记录：
TraceId：
遗留问题：
```

示例：

```text
功能名称：物料品牌维护
涉及模块：BaseDataModule
后端接口：api/material-brand
前端页面：src/views/basedata/material-brand.vue
验证步骤：
  1. Swagger 创建品牌
  2. 前端列表查询
  3. 编辑品牌名称
  4. 删除测试品牌
验证结果：通过
```

## 7. 培训演练建议

课堂可以安排三个层级：

基础级：

- 新增一个 CRUD 接口并 Swagger 验证。

进阶级：

- 新增带扩展字段的表单配置接口。

综合级：

- 新增跨模块业务动作，并验证事务回滚。

每个演练都要求成员提交：

- 代码落点说明。
- 接口截图或 Swagger 结果。
- 失败场景验证。
- 是否涉及权限和配置。

## 8. 课后检查

- 能独立跑后端 build。
- 能用 Swagger 验证接口。
- 能设计一个新增 CRUD 功能的验收清单。
- 能说明哪些场景必须验证事务回滚。

## 继续阅读

- [学习路径](/learning-path)
- [培训资料下载](/training-materials)
- [架构总览](/backend/架构设计/KH.WMS架构总览)
