# KH.WMS 后端排错与日志追踪指引

> 本文用于培训接口、业务、权限、数据库和策略问题的排查方法。

## 目录

- [1. 阅读路径](#1-阅读路径)
- [2. 关键追踪字段](#2-关键追踪字段)
- [3. 日志位置](#3-日志位置)
- [4. 常见 HTTP 状态](#4-常见-http-状态)
- [5. Autofac 服务无法解析](#5-autofac-服务无法解析)
- [6. Controller 不被发现](#6-controller-不被发现)
- [7. 数据库问题](#7-数据库问题)
- [8. 策略问题](#8-策略问题)
- [9. 前后端联调问题](#9-前后端联调问题)
- [10. 排错流程模板](#10-排错流程模板)
- [11. 培训演练场景](#11-培训演练场景)
- [12. 课后检查](#12-课后检查)

## 1. 阅读路径

1. 先读 `KH.WMS后端开发指引.md` 的错误处理、TraceId、日志章节。
2. 再读 `KH.WMS.Core-API-参考文档.md` 的 Logging、Exceptions、Api Responses。
3. 策略问题同时查 `KH.WMS.Algorithms外部调用与扩展手册.md`。

## 2. 关键追踪字段

| 字段 | 来源 | 用途 |
| --- | --- | --- |
| `TraceId` | `ApiResponse.TraceId` | 前端报错后回传给后端排查 |
| `X-Correlation-ID` | 请求头或 `HttpContext.TraceIdentifier` | 串联一次请求日志 |
| `X-Business-ID` | 请求头 | 按单据号或业务号聚合日志 |
| `BusinessId` | Serilog Enricher | 日志中的业务关联字段 |

## 3. 日志位置

后端日志默认在：

- `KH.WMS/KH.WMS.Server/Logs`

常见日志类型：

- 普通请求日志。
- 错误日志。
- 警告日志。
- 性能或慢调用日志。

排查口诀：

```text
前端报错拿 TraceId
  -> 后端按 CorrelationId/TraceId 查日志
  -> 看请求路径、用户、耗时、异常
  -> 再定位 Controller/Service/SQL/策略
```

## 4. 常见 HTTP 状态

| 状态 | 常见原因 | 排查入口 |
| --- | --- | --- |
| 401 | Token 缺失、过期、刷新失败 | `request.js`、JWT 配置、登录接口 |
| 403 | 权限不足 | `PermissionController`、权限树、前端 permission Store |
| 404 | 路由错误、Controller 未注册 | Controller `[Route]`、程序集名、Swagger |
| 422 | 字段校验失败 | `ValidationException`、请求参数 |
| 429 | 限流命中 | RateLimit 配置 |
| 500 | 未处理异常、服务解析失败、数据库错误 | 后端日志、GlobalExceptionFilter |

## 5. Autofac 服务无法解析

常见原因：

- Service 没有 `[RegisteredService]`。
- `ServiceType` 指定错误。
- 构造函数参数顺序和基类不一致。
- 依赖接口没有实现类。
- 跨模块直接注入了对方 Service，而不是 Contract。

检查顺序：

1. 看报错中的接口类型。
2. 搜索实现类是否存在。
3. 检查是否有 `[RegisteredService(ServiceType = typeof(...))]`。
4. 检查构造函数依赖是否也能解析。
5. 检查是否应该改成注入 Contract。

## 6. Controller 不被发现

常见原因：

- 新模块程序集名不包含 `.Modules.`。
- Controller 没有继承正确基类或没有路由。
- 模块 dll 未被宿主加载。

检查入口：

- `KH.WMS.Server/Program.cs` 中 ApplicationPartManager 扫描逻辑。
- Swagger 是否出现对应 Controller。
- 模块 `.csproj` 和程序集名。

## 7. 数据库问题

常见问题：

- 连接字符串错误。
- DbType 配置错误。
- 表结构与实体不一致。
- 枚举或日期字段序列化异常。
- 事务未提交或提前回滚。

排查入口：

- `appsettings.json` 的 `DbConnection`。
- MiniProfiler SQL。
- Serilog SQL 日志。
- `IUnitOfWork` 调用链。

## 8. 策略问题

常见问题：

| 问题 | 可能原因 |
| --- | --- |
| 策略未注册 | 未注册 `StrategyAutofacModule` 或外部策略未扫描 |
| `Output` 为 null | 策略失败、跳过或未先判断 `IsSuccess` |
| 无可用库存 | 参数错误、库存不足、库存状态不可用 |
| 无可用货位 | 库位满、库区错误、状态不正确 |
| 策略链未生效 | 仓库、单据类型、链类型匹配失败 |

排查入口：

- `KH.WMS.Algorithms外部调用与扩展手册.md`
- 策略编码速查表。
- 上下文参数键速查表。
- 策略注册表查询接口。

## 9. 前后端联调问题

常见问题：

- 前端请求路径与后端 `[Route]` 不一致。
- 前端动态路由 `component` 找不到真实页面。
- 前端判断成功码只看一个值。
- 后端自定义接口返回 `IActionResult` 绕过 TraceId 注入。
- 字典或权限缓存未刷新。

## 10. 排错流程模板

遇到问题时不要直接改代码，先按固定路径定位。

接口失败：

```text
确认前端请求路径
  -> Swagger 验证接口
  -> 看 HTTP 状态
  -> 查 ApiResponse message/traceId
  -> 查后端日志
  -> 定位 Controller/Service/SQL
```

启动失败：

```text
看控制台异常
  -> 判断配置/DI/数据库
  -> 查服务注册
  -> 查 appsettings
  -> 最小化启动验证
```

业务状态不对：

```text
确认操作入口
  -> 查主单状态
  -> 查任务状态
  -> 查库存/库位/容器状态
  -> 查跨模块 Contract 调用
  -> 查事务是否回滚
```

策略结果不对：

```text
确认策略编码
  -> 确认策略已注册
  -> 检查 PolicyContext
  -> 检查 QueryService 数据
  -> 检查 IsSuccess/ErrorMessage
  -> 检查 Output 类型
```

## 11. 培训演练场景

建议课堂设计 5 个故障：

1. Token 过期导致 401：让成员观察前端刷新 Token 和后端认证日志。
2. 菜单 component 写错：让成员定位为什么页面变成占位页。
3. Service 忘记 `[RegisteredService]`：让成员排查 Autofac 无法解析。
4. 新模块程序集名不含 `.Modules.`：让成员排查 Swagger 看不到 Controller。
5. FIFO 策略缺少 `REQUIRED_QTY`：让成员从策略错误定位上下文参数。

每个演练都要求记录：

- 现象。
- 第一定位入口。
- 最终原因。
- 修复方式。
- 以后如何避免。

## 12. 课后检查

- 能根据 TraceId 查后端日志。
- 能区分 401、403、422、500 的排查方向。
- 能排查服务无法解析和 Controller 不出现在 Swagger 的问题。
- 能说明策略问题从注册、参数、数据、结果四层排查。
