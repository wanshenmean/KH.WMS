# 05 请求管道、统一响应、异常、TraceId、日志

## 这个概念解决什么问题

请求管道负责把一个 HTTP 请求从进入系统到返回响应的过程串起来。统一响应、异常处理、TraceId 和日志解决的是线上排查和前后端协作问题：

- 前端拿到稳定的响应格式。
- 后端异常统一记录，不需要每个接口重复 try/catch。
- 每个响应带 TraceId，便于从接口返回追到日志。
- 异常日志包含请求路径、查询参数、脱敏后的请求体、SQL 和方法调用链。

KH.WMS 的对外接口统一使用 `ApiResponse`，服务内部分支流程常用 `ServiceResult`。这两个类型用途不同，不能混着理解。

## 什么时候需要看

- 接口返回格式不统一，前端无法按 `code/message/data/traceId` 处理。
- 接口异常没有进入全局异常响应。
- 日志里找不到 TraceId、请求体、SQL 或方法链。
- 不知道业务失败应该 `return ApiResponse.Fail` 还是直接抛异常。
- `ExtDataCrudController` 读取不到原始请求体。
- 想排查敏感字段是否会被异常日志记录。

## 业务开发应该怎么用

### Controller 对外统一返回 `ApiResponse`

对 HTTP 接口，优先返回：

```csharp
return ApiResponse.Ok(data);
return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "参数错误");
return ApiResponse.NotFound("数据不存在");
```

CRUD 基类已经统一返回 `ApiResponse`，普通业务 Controller 不需要再包一层。

### Service 内部流程可以用 `ServiceResult`

`ServiceResult` 更适合服务内部、跨模块 Contract 或业务步骤结果：

```csharp
var taskResult = await _taskContract.CreatePutawayTaskAsync(request);
if (!taskResult.Success)
    return ServiceResult.Fail(taskResult.Message);
```

它不是 HTTP 响应格式，不带 `Code`、`Timestamp`、`TraceId`。

### 什么时候返回失败，什么时候抛异常

推荐边界：

- 可预期的业务校验失败：返回 `ApiResponse.Fail` 或 `ServiceResult.Fail`。
- 资源不存在并且当前流程不能继续：可返回 NotFound，也可由 Service 抛 `NotFoundException` 交给全局异常处理。
- 系统错误、数据不一致、非法状态、数据库异常：抛异常，让事务回滚和全局异常 Filter 统一处理。
- 手动 catch 后不要吞异常，除非你明确要把它转成业务失败结果。

尤其在有事务的流程里，吞异常会让外层以为流程成功，从而提交半成品数据。

## 底层逻辑和实现

### 请求处理主线

`Program.cs` 和 `UseCustomMiddleware` 共同构成请求管道：

1. `app.Use(...)` 启用请求体缓冲，并创建 `ErrorLogScope`。
2. `UseCustomMiddleware` 挂载异常处理中间件、License、HTTPS、静态文件、CORS、请求日志、路由、MiniProfiler、认证、授权、端点映射。
3. MVC 执行全局 Filter，例如 `ApiAuthorizeFilter`、`TraceIdResultFilter`。
4. Controller 调用 Service。
5. Service 可能经过 AOP 代理，记录方法链、SQL、性能。
6. 正常返回时，`TraceIdResultFilter` 给 `ApiResponse` 注入 TraceId。
7. 异常抛出时，`GlobalExceptionFilter` 构造统一 `ApiResponse` 并记录详细日志。

### 请求体为什么能多次读取

ASP.NET Core 请求体默认只能读一次。KH.WMS 在 `Program.cs` 里提前执行：

```csharp
context.Request.EnableBuffering();
```

这样后续两个地方可以安全重读 body：

- `GlobalExceptionFilter` 异常时读取 JSON 请求体用于日志。
- `ExtDataCrudController` 在模型绑定后重读原始 JSON，提取 `extDataRaw`。

如果删除这段中间件，ExtData 和异常请求体日志都会受影响。

### 全局异常如何转响应

`GlobalExceptionFilter` 实现 `IAsyncExceptionFilter`。它会：

- 读取请求方法、路径、query、用户、TraceId。
- 在请求体小于等于 100KB 且是 JSON 时读取 body。
- 对 `password/token/secret/refreshtoken/oldpassword/newpassword/authorization` 等字段脱敏。
- 记录异常类型、消息和堆栈。
- flush `ErrorLogScope` 中的方法链和 SQL。
- 按异常类型映射 `ResponseCode` 和 HTTP 状态码。

常见映射：

| 异常 | 响应 Code |
| --- | --- |
| `BusinessException` | `BAD_REQUEST` |
| `NotFoundException` | `NOT_FOUND` |
| `ValidationException` | `VALIDATION_ERROR` |
| `UnauthorizedAccessException` | `UNAUTHORIZED` |
| `ArgumentException` | `BAD_REQUEST` |
| 其他异常 | `INTERNAL_SERVER_ERROR` |

### TraceId 如何进入响应

`TraceIdResultFilter` 只处理 `ObjectResult.Value` 是 `ApiResponse` 的情况：

```csharp
if (context.Result is ObjectResult { Value: ApiResponse response })
{
    response.TraceId ??= context.HttpContext.TraceIdentifier;
}
```

如果接口返回匿名对象、字符串、文件流，Filter 不会给它注入 TraceId。普通业务接口不要绕过 `ApiResponse`。

### Serilog 在日志链路里的位置

`Program.cs` 启动时调用：

```csharp
builder.Host.AddSerilog();
```

这个调用最终进入 `SerilogSetup.AddSerilog`。它做的是宿主级日志配置：把 ASP.NET Core 的 `ILogger<T>` 输出接到 Serilog，并按配置写入控制台和文件。业务开发不需要自己创建 Serilog logger，也不应该在业务方法里手写文件路径。

当前 Serilog 主要提供这些能力：

| 能力 | 实现位置 | 使用者需要知道什么 |
| --- | --- | --- |
| 日志落盘 | `SerilogSetup.AddSerilog` | 默认读取 `Serilog:LogPath`、`RetentionDays`、`MaxFileSizeMB`、`WriteToConsole` 等配置 |
| 分级文件 | `SerilogSetup` | 普通日志写 `log-.txt`，错误日志写 `error-.txt`，警告日志写 `warning-.txt`，Debug 写 `debug-.txt` |
| 请求上下文增强 | `UserLogEnricher`、`CorrelationIdEnricher` | 日志模板里会带 `TenantId`、`UserId`、`RequestId` 等上下文字段 |
| 模块分文件 | `ModuleLogEnricher`、`WmsLogModules` | 带 `ModuleCode` 的日志可以被路由到 `Logs/wms/` 下的模块文件 |
| 自定义文件名 | `LoggerService.PushFileName` + `SerilogSetup.WriteFileNameLogs` | `ILoggerService.LogInfoToFile(...)` 这类方法会把日志写到 `Logs/custom/` |

几种常见日志来源要分清：

| 日志来源 | 入口 | 写什么 | 什么时候看 |
| --- | --- | --- | --- |
| 全局异常日志 | `GlobalExceptionFilter` 使用 `ILogger<GlobalExceptionFilter>` | path、method、TraceId、user、query、脱敏 body、异常类型、堆栈 | 接口 500、统一响应异常 |
| 异常调用链 | `ErrorLogScope.Flush()` 后由 `GlobalExceptionFilter` 写 error | AOP 方法链、参数、返回值、SQL | 要还原异常前执行到哪一步 |
| 方法日志 | `LoggingInterceptor` 使用 `ILoggerService` | `[LogInterceptor]` 方法进入/退出、参数、返回值 | 关键业务流程排查 |
| SQL 日志 | `SqlSugarSetup.OnLogExecuting` | SQL 和参数；`EnableSqlLog=true` 时写 Debug/控制台，同时异常缓冲始终追加 SQL | 查 SQL 是否执行、参数是否正确 |
| 业务主动日志 | 注入 `ILoggerService` | 操作日志、业务日志、性能日志、自定义文件日志 | 需要记录业务事件而不是异常 |

`ILoggerService` 的实现是 `LoggerService`，它注册为 `WithoutInterceptor=true`，内部再使用 `ILogger<LoggerService>` 输出。因此它仍然走 Serilog 的文件、级别、模板和 enricher，只是给业务代码提供了更统一的封装。

业务开发建议：

- 普通业务事件优先注入 `ILoggerService`，使用 `LogBusiness`、`LogOperation` 或普通 `LogInfo/LogWarning/LogError`。
- 框架 Filter、Middleware、底层组件可以继续使用 `ILogger<T>`。
- 不要为了“写入 error-.txt”手动拼文件路径；使用 error 级别日志，Serilog sink 会按规则路由。
- Debug 方法参数、SQL、调用链是否写入文件，受 `Serilog:MinimumLevel:Default` 和 Debug sink 影响；但异常时 `ErrorLogScope` 会通过 error 日志强制带出缓冲内容。

### 自定义日志文件怎么用

自定义日志文件用于把某类业务日志单独汇总到一个文件里，例如导入日志、库存审计日志、接口对账日志、外部系统回调日志。它不是替代全局异常日志，也不是让业务代码自己写文件。

使用方式是注入 `ILoggerService`，调用带 `ToFile` 后缀的方法：

| 方法 | 用途 |
| --- | --- |
| `LogInfoToFile(fileName, message, args)` | 记录普通信息到指定自定义文件 |
| `LogErrorToFile(fileName, message, args)` | 记录错误信息到指定自定义文件 |
| `LogErrorToFile(fileName, exception, message, args)` | 记录带异常堆栈的错误到指定自定义文件 |
| `LogOperationToFile(fileName, operation, userName, userId, data)` | 记录操作类日志到指定自定义文件 |
| `LogBusinessToFile(fileName, businessType, message, data)` | 记录业务类日志到指定自定义文件 |

`fileName` 只写逻辑文件名，不要写路径和扩展名。例如写 `inbound-import`，不要写 `Logs/custom/inbound-import.txt`。

实际输出路径由 Serilog 配置决定：

```text
{Serilog:LogPath}/custom/{fileName}-{yyyyMMdd}.txt
```

如果没有配置 `Serilog:LogPath`，默认是：

```text
Logs/custom/{fileName}-{yyyyMMdd}.txt
```

底层逻辑是：

1. `LoggerService.LogInfoToFile("inbound-import", ...)` 调用 `PushFileName("inbound-import")`。
2. `PushFileName` 把 `FileName=inbound-import` 写入 Serilog `LogContext`。
3. `SerilogSetup.WriteFileNameLogs` 用 `WriteTo.Map` 读取 `FileName`。
4. Serilog 把日志写到 `Logs/custom/inbound-import-{yyyyMMdd}.txt`。

这类日志仍然会经过 Serilog 的主日志管道。也就是说，一条 `LogErrorToFile` 通常既会出现在自定义文件里，也会按 error 级别进入 `error-.txt`。

#### 示例：在 Service 里记录导入日志

```csharp
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Logging;
using KH.WMS.Core.Models;
using KH.WMS.Core.UserProvide;

[RegisteredService(ServiceType = typeof(IInboundImportService))]
public class InboundImportService(
    ILoggerService logger,
    IUserContext userContext) : IInboundImportService
{
    private const string ImportLogFile = "inbound-import";

    public async Task<ServiceResult> ImportAsync(InboundImportDto dto)
    {
        logger.LogInfoToFile(
            ImportLogFile,
            "开始入库单导入: FileName={FileName}, UserId={UserId}",
            dto.FileName,
            userContext.UserId);

        try
        {
            // 业务导入逻辑
            var importedCount = await ImportRowsAsync(dto);

            logger.LogBusinessToFile(
                ImportLogFile,
                "InboundImport",
                "入库单导入完成",
                new
                {
                    dto.FileName,
                    ImportedCount = importedCount,
                    userContext.UserId
                });

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            logger.LogErrorToFile(
                ImportLogFile,
                ex,
                "入库单导入失败: FileName={FileName}, UserId={UserId}",
                dto.FileName,
                userContext.UserId);

            throw;
        }
    }
}
```

这个示例里要注意两点：

- `catch` 里记录完异常后继续 `throw`，让事务和 `GlobalExceptionFilter` 正常处理。
- `LogBusinessToFile` 的 `data` 参数会序列化成 JSON，适合放少量关键字段，不要塞整张导入明细或大对象。

#### 示例：记录库存审计操作

```csharp
_logger.LogOperationToFile(
    "inventory-audit",
    "库存冻结",
    _userContext.UserName,
    _userContext.UserId,
    new
    {
        request.WarehouseId,
        request.ContainerCode,
        request.MaterialCode,
        request.Qty
    });
```

#### 自定义日志文件的注意事项

- 文件名使用固定白名单式名称，例如 `inbound-import`、`inventory-audit`、`erp-callback`，不要使用前端传入的原始字符串。
- 不要把密码、token、身份证、手机号等敏感信息写入自定义日志。全局异常 Filter 只脱敏请求体，不会自动脱敏你手动写入的业务对象。
- 自定义文件 sink 的最低级别是 `Information`，因此适合记录业务事件、操作结果和错误，不适合做大量 Debug 明细。
- 文件按天滚动，保留天数和大小限制跟 `Serilog:RetentionDays`、`Serilog:MaxFileSizeMB` 一致。
- 如果只是排查一次异常链路，优先用 TraceId、`GlobalExceptionFilter`、`ErrorLogScope`；自定义文件更适合长期保留某类业务流水。

### `ErrorLogScope` 的作用

`LoggingInterceptor` 和 SqlSugar SQL AOP 会把方法调用和 SQL 写入请求级缓冲区。异常发生时，`GlobalExceptionFilter` 会把这些内容作为“异常调用链”写到 error 日志里。

这样排查时可以从一个 TraceId 看到：

- 进了哪个 Service 方法。
- 参数是什么。
- 执行了哪些 SQL。
- 异常在哪一步抛出。

## 一次异常请求的完整链路

以“提交接口里数据库写入后抛异常”为例：

1. 请求进入 `Program.cs` 的 buffering 中间件。
2. `ErrorLogScope.Begin()` 创建请求级日志缓冲。
3. `UseCustomMiddleware` 中的异常处理、License、CORS、请求日志、路由、MiniProfiler、认证、授权依次执行。
4. MVC 匹配 Controller Action。
5. `ApiAuthorizeFilter` 验证 token。
6. Action 调用 Service。
7. Service 经 AOP 代理进入 `LoggingInterceptor`，方法参数写入 `ErrorLogScope`。
8. Repository 执行 SQL，`SqlSugarSetup.OnLogExecuting` 把 SQL 写入 `ErrorLogScope`。
9. Service 抛异常。
10. `ExceptionInterceptor` 记录异常并重新抛出。
11. 如果 Action 或 Service 事务边界没有吞异常，事务回滚。
12. `GlobalExceptionFilter` 捕获异常。
13. Filter 读取请求路径、query、用户、TraceId、脱敏 body。
14. Filter flush `ErrorLogScope`，把方法链和 SQL 写入 error 日志。
15. Filter 构造 `ApiResponse` 并设置 HTTP 状态码。
16. `TraceIdResultFilter` 对 `ApiResponse` 补 TraceId。
17. buffering 中间件 finally 清理 `ErrorLogScope`。

排查时如果日志缺了 SQL 或方法链，要看第 7、8 步有没有真正进入：方法是否经过 AOP，SQL 是否由 SqlSugar 执行。

## 返回失败与抛异常对事务的影响

这点很容易踩坑。

### 返回失败不会自动触发异常回滚

如果外层事务 Filter 只看到 Action 正常返回，它会提交事务：

```csharp
[Transaction]
public async Task<ApiResponse> Submit()
{
    await repo.AddAsync(entity);
    return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "业务失败");
}
```

从 MVC Filter 的角度看，这个 Action 没有异常，事务会提交。也就是说，`ApiResponse.Fail` 表示业务失败响应，不等于事务异常。

如果已经写了数据，又发现流程不能继续，应该抛异常或显式 rollback，而不是只返回失败。

### 手动事务里返回失败前要 rollback

```csharp
await _unitOfWork.BeginTransactionAsync();
try
{
    await repo.AddAsync(entity);

    if (!ok)
    {
        await _unitOfWork.RollbackAsync();
        return ServiceResult.Fail("业务失败");
    }

    await _unitOfWork.CommitAsync();
    return ServiceResult.Ok();
}
catch
{
    await _unitOfWork.RollbackAsync();
    throw;
}
```

如果方法选择 `ServiceResult.Fail` 风格，就要确保失败分支已经处理事务。

### CRUD 钩子里建议抛异常

在 `CrudService` 的 `BeforeCreateAsync`、`BeforeUpdateAsync` 等钩子里，推荐抛业务异常：

```csharp
protected override async Task BeforeCreateAsync(MyEntity entity)
{
    if (await _repository.ExistsAsync(x => x.Code == entity.Code))
        throw new BusinessException("编码已存在");
}
```

`CrudService` 外层 catch 会 rollback，然后异常进入全局异常 Filter。

## TraceId 怎么用于排查

前端拿到响应：

```json
{
  "code": 500,
  "message": "服务器内部错误",
  "traceId": "0HMS..."
}
```

排查步骤：

1. 用 `traceId` 搜应用日志。
2. 找 `[全局异常]` 日志，确认 path、query、user、body。
3. 找 `[异常调用链]`，确认进入了哪些方法和 SQL。
4. 如果没有调用链，看目标 Service 是否经过 DI/AOP。
5. 如果有 SQL 但没有方法链，看是否直接在 Controller 或非代理服务里访问数据库。
6. 如果有方法链但没有 SQL，看是否失败发生在查询前、缓存命中或走了其他数据访问路径。

## 响应类型选择表

| 场景 | 推荐类型 | 原因 |
| --- | --- | --- |
| Controller 对前端返回 | `ApiResponse` | 带 code、message、data、traceId |
| CRUD 基类接口 | `ApiResponse` | 基类已统一 |
| 跨模块 Contract 内部结果 | `ServiceResult` / `ServiceResult<T>` | 不绑定 HTTP |
| 配置校验短路 | `ServiceResult` / `ServiceResult<T>` | `ConfigValidationInterceptor` 只支持这类 |
| 文件下载 | `FileResult` 等 MVC 类型 | 不适合包 ApiResponse |
| 系统异常 | 抛异常 | 交给全局异常 Filter |

## 最小示例

### 普通接口返回

```csharp
[HttpGet("{id}")]
public async Task<ApiResponse> GetById(long id)
{
    var entity = await _service.GetAsync(id);
    if (entity == null)
        return ApiResponse.NotFound("数据不存在");

    return ApiResponse.Ok(entity);
}
```

### 业务流程里保留异常给事务和全局 Filter

```csharp
public async Task<ApiResponse> SubmitAsync(long id)
{
    var entity = await GetEntityOrThrowAsync(id);
    entity.Submit();
    await _repository.UpdateAsync(entity);
    return ApiResponse.Ok(message: "提交成功");
}
```

不要写成：

```csharp
try
{
    ...
}
catch
{
    return ApiResponse.Ok("忽略错误");
}
```

这会让调用方和事务边界都误判成功。

## 常见误区与排查清单

### 响应没有 TraceId

- Action 是否返回 `ApiResponse`。
- 是否返回了 `FileResult`、字符串、匿名对象。
- 是否绕过 MVC 直接写 response。

### 异常没有统一格式

- 异常是否发生在 MVC Filter 能接管的管道内。
- 是否被业务代码 catch 并吞掉。
- 是否中间件提前写出了响应。

### 日志没有请求体

- Content-Type 是否包含 `application/json`。
- ContentLength 是否大于 0 且不超过 100KB。
- 请求体是否已启用 buffering。

### 日志里泄露敏感数据

- 全局异常 Filter 会脱敏常见字段，但业务日志里的 `[LogInterceptor(LogParameters=true)]` 仍可能记录参数。
- 敏感方法不要打开返回值记录。
- DTO 命名敏感字段时尽量使用可被脱敏规则识别的名称。
