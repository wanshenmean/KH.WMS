# 04 AOP 拦截器与特性使用

## 这个概念解决什么问题

AOP 拦截器解决的是横切逻辑复用问题：日志、缓存、配置校验、异常记录、性能统计不应该散落在每个业务方法里。

KH.WMS 里业务服务默认由 Autofac + Castle DynamicProxy 生成代理，代理会挂上这些拦截器：

- `LoggingInterceptor`
- `CachingInterceptor`
- `ConfigValidationInterceptor`
- `ExceptionInterceptor`
- `PerformanceInterceptor`

业务开发通常不需要手动注入拦截器，也不需要直接调用拦截器。正确用法是：服务走 DI 代理，需要某项能力时给类或方法加对应特性。

## 什么时候需要看

- 想给某个业务方法记录参数、返回值、方法链。
- 想缓存某个查询结果。
- 想在业务方法执行前跑一组配置驱动校验。
- 想知道异常记录和全局异常响应的关系。
- 想排查慢方法、MiniProfiler 里没有方法步骤。
- 误以为 `[Transaction]` 是 AOP 拦截器。

## 业务开发应该怎么用

### 日志拦截器 `LoggingInterceptor`

作用：

- 根据 `[LogInterceptor]` 记录方法进入、退出、参数、返回值。
- 写入 `MiniProfiler.Current.Step`，便于性能链路查看。
- 写入 `ErrorLogScope`，异常时由全局异常 Filter flush 到 error 日志。
- 记录当前用户信息和调用层级。

什么时候手动使用：

- 对关键业务流程加 `[LogInterceptor]`。
- 对排查困难的方法临时打开参数或返回值记录。
- 对包含敏感数据的方法谨慎关闭返回值记录。

示例：

```csharp
[LogInterceptor(LogParameters = true, LogReturnValue = false)]
public async Task<ApiResponse> CreateAsync(MyEntity entity)
{
    ...
}
```

注意：

- 默认只有加了 `[LogInterceptor]` 的方法或类才会进入详细业务日志记录逻辑。
- 参数和返回值会尝试 JSON 序列化，不适合记录大对象或敏感字段。

### 缓存拦截器 `CachingInterceptor`

作用：

- 读取 `[Cache]` 特性。
- 根据目标类型、方法名、参数生成缓存 key。
- 命中缓存时直接返回。
- 未命中时执行原方法，并缓存 `ApiResponse.Data != null` 的返回结果。

什么时候手动使用：

- 适合稳定、读多写少的查询方法。
- 适合字典、配置、下拉列表等接口。
- 不适合创建、更新、删除、提交、取消等写操作。

示例：

```csharp
[Cache(Duration = 300, KeyPrefix = "warehouse:all")]
public async Task<ApiResponse> GetAll()
{
    ...
}
```

CRUD 基类里很多端点显式写了 `[Cache(Enable = false)]`，因为标准 CRUD 查询、写操作默认不应该被 AOP 缓存误伤。需要缓存时应在明确的查询方法上单独打开。

### 配置校验拦截器 `ConfigValidationInterceptor`

作用：

- 读取方法上的多个 `[ConfigValidation]`。
- 从 DI 容器解析所有 `IValidator`。
- 按特性声明顺序执行校验器。
- 任一校验失败时短路返回 `ServiceResult.Fail`，原业务方法不执行。

什么时候手动使用：

- 业务规则受配置项控制，例如是否允许混料、是否要求批次号、是否要求效期。
- 同一套校验会被多个业务方法复用。
- 想把前置校验和主业务流程拆开。

示例：

```csharp
[ConfigValidation(ValidatorCodes.BIND_DATA_NOT_EMPTY)]
[ConfigValidation(ValidatorCodes.MIXED_MATERIAL)]
public async Task<ServiceResult> ContainerBindAsync(List<ContainerBindDto> binds)
{
    ...
}
```

限制：

- 只对 `Task<ServiceResult>` 和 `Task<ServiceResult<T>>` 风格方法短路。
- 返回 `ApiResponse` 的方法不会被它短路。
- Validator 本身应注册为 `WithoutInterceptor=true`，避免递归拦截。

### 异常拦截器 `ExceptionInterceptor`

作用：

- 捕获服务方法同步抛出的异常。
- 按异常类型记录日志。
- 重新抛出异常。

它不负责把异常转成接口响应。接口响应由 MVC 全局异常 Filter 处理。

什么时候手动使用：

- 业务开发一般不直接使用。
- 只需要保证服务通过 DI 代理调用，异常会经过代理记录，再交给请求管道处理。

注意：

- 对异步异常的完整处理能力受实现方式影响，接口层最终统一响应仍要依赖 `GlobalExceptionFilter`。
- 不要在业务里为了“触发异常拦截器”去 catch 后再 throw 包装异常。

### 性能拦截器 `PerformanceInterceptor`

作用：

- 统计方法耗时。
- 超过阈值时记录 warning。

什么时候手动使用：

- 业务开发不需要加特性，它随服务代理挂载。
- 排查慢接口时，可以结合日志、MiniProfiler、SQL 日志一起看。

默认阈值：

- 构造函数默认 `thresholdMs = 1000`。

## 事务不是 Autofac AOP

KH.WMS 的事务自动管理不是 `TransactionInterceptor`，当前代码里事务是 MVC Filter 方案：

- `[Transaction]` 实现 `IFilterFactory`。
- MVC 创建 `TransactionActionFilter`。
- Filter 在 Action 执行前 `BeginTransactionAsync`。
- Action 无异常则 `CommitAsync`。
- Action 有异常则 `RollbackAsync`。

所以：

- `[Transaction]` 应标在 Controller 或 Action 上。
- Service 方法内部事务一般手动使用 `IUnitOfWork`，或依赖 `CrudService` 内置事务。
- 不要把事务不生效的问题当成 Autofac AOP 排查。

## 底层逻辑和实现

### 拦截器如何挂上服务

`ServiceRegistrar` 默认把以下拦截器挂到服务代理：

```csharp
private Type[] interceptors = new Type[]
{
    typeof(LoggingInterceptor),
    typeof(CachingInterceptor),
    typeof(ConfigValidationInterceptor),
    typeof(ExceptionInterceptor),
    typeof(PerformanceInterceptor)
};
```

接口服务使用：

```csharp
EnableInterfaceInterceptors().InterceptedBy(interceptors)
```

自注册服务使用：

```csharp
EnableClassInterceptors().InterceptedBy(interceptors)
```

`WithoutInterceptor=true` 时直接注册，不挂这些拦截器。

### 拦截器触发条件

这里的“触发”要拆成两层：第一层是服务注册时有没有挂代理链，第二层是拦截器进入后是否真的执行核心能力。

只要服务不是 `WithoutInterceptor=true`，并且调用确实经过 Castle 代理，五个拦截器都会被挂到代理链上。方法上没有特性，不代表完全没经过 AOP；它只代表某些“需要特性驱动”的拦截器会直接放行。

| 拦截器 | 是否自动挂到代理链 | 方法不写特性时会怎样 | 是否需要手动加特性触发核心能力 |
| --- | --- | --- | --- |
| `LoggingInterceptor` | 是 | 进入拦截器，但顶层方法读不到 `[LogInterceptor]` 时不打开 `CallTraceContext.Current.IsRecord`，不会记录详细进入/退出、参数、返回值，也不会把该方法写入 `ErrorLogScope` | 需要。关键流程、临时排查、需要方法链时加 `[LogInterceptor]` |
| `CachingInterceptor` | 是 | 读不到 `[Cache]` 或 `Enable=false` 时直接 `invocation.Proceed()`，不生成 key、不读写缓存 | 需要。只读、稳定、读多写少查询才加 `[Cache]` |
| `ConfigValidationInterceptor` | 是 | 只从实现方法读取 `[ConfigValidation]`；没有特性就直接执行原方法 | 需要。配置驱动前置校验才加 `[ConfigValidation]` |
| `ExceptionInterceptor` | 是 | 不需要任何特性，代理方法同步抛异常时按异常类型记录并重新抛出 | 不需要。保持服务走 DI 代理即可 |
| `PerformanceInterceptor` | 是 | 不需要任何特性，会统计代理方法耗时，超过阈值写 warning | 不需要。排查慢调用时看日志和 MiniProfiler |

如果服务设置了 `WithoutInterceptor=true`，上表全部失效，因为 `ServiceRegistrar` 走的是无拦截器注册分支，没有 `EnableInterfaceInterceptors()` / `EnableClassInterceptors()`，也没有 `InterceptedBy(interceptors)`。

所以排查时按这三个问题判断：

1. 服务有没有被注册成代理服务。
2. 调用有没有经过代理对象，而不是 `new` 或类内部 `this.Method()`。
3. 目标能力是否需要特性，以及特性加的位置和返回类型是否满足拦截器实现。

## 拦截器顺序带来的影响

当前拦截器数组顺序是：

```text
Logging -> Caching -> ConfigValidation -> Exception -> Performance -> 原方法
```

Castle DynamicProxy 会按注册顺序组成调用链。对业务开发最重要的是理解这些影响：

- `CachingInterceptor` 如果命中缓存，会直接设置 `ReturnValue` 并返回，后面的校验、异常、性能和原方法都不会执行。
- `[Cache]` 不应该加在需要实时校验权限、状态或配置的写操作上。
- `ConfigValidationInterceptor` 校验失败时会短路，原方法不执行，后续原方法里的事务也不会开始。
- `ExceptionInterceptor` 只记录再抛，不负责统一响应。
- `PerformanceInterceptor` 更接近原方法，统计的是它包住的后续调用耗时。

因此，一个方法同时加 `[Cache]` 和 `[ConfigValidation]` 时要谨慎。大多数业务方法不应该这样组合；查询方法用缓存，提交方法用配置校验。

## 每个拦截器的“手动使用点”

| 拦截器 | 手动动作 | 常见业务场景 | 不适合场景 |
| --- | --- | --- | --- |
| `LoggingInterceptor` | 加 `[LogInterceptor]` | 关键提交、复杂流程排查 | 大对象、敏感参数、超高频方法 |
| `CachingInterceptor` | 加 `[Cache]` | 字典、配置、下拉、只读查询 | 写操作、状态敏感查询、用户隔离不清的查询 |
| `ConfigValidationInterceptor` | 加 `[ConfigValidation]` | 配置驱动前置校验 | CRUD 标准 Action、返回 `ApiResponse` 的方法 |
| `ExceptionInterceptor` | 无需手动加特性 | 服务异常记录 | 不能替代全局异常 Filter |
| `PerformanceInterceptor` | 无需手动加特性 | 慢方法定位 | 不能替代 SQL/接口级 profiling |

业务开发一般不应该“手动调用拦截器类”。所谓手动使用，指的是在业务方法或类上选择是否加对应特性。异常和性能拦截器没有业务特性开关，是否生效取决于服务是否经过代理。

几个常见判断：

- 新增、提交、分配、冻结、解冻、过账这类写流程：可以加 `[LogInterceptor]`，不应该加 `[Cache]`。
- 字典、配置、下拉、仓库列表等读接口：可以考虑 `[Cache]`，但要确认 key 不会因为复杂对象 `ToString()` 失真。
- 混料、混批、批次必填、效期必填等配置驱动规则：适合 `[ConfigValidation]`，但方法返回类型要是 `Task<ServiceResult>` 或 `Task<ServiceResult<T>>`。
- 只是想让异常变成统一 HTTP 响应：不要找 AOP，应该依赖 `GlobalExceptionFilter`。
- 只是想知道接口慢在哪里：先看 `PerformanceInterceptor`、SQL 日志和 MiniProfiler；需要方法参数链路时再给关键方法加 `[LogInterceptor]`。

## 代码级触发条件

### `[LogInterceptor]` 被哪里读取

`LoggingInterceptor` 会依次尝试：

1. `invocation.MethodInvocationTarget` 上的方法特性。
2. `invocation.Method` 上的方法特性。
3. `invocation.TargetType` 上的类特性。

所以特性加在实现类方法上最稳。加在接口方法上可能取决于 `invocation.Method` 是否能读到。

### `[Cache]` 被哪里读取

`CachingInterceptor` 读取顺序：

1. 实现方法上的 `[Cache]`。
2. 接口方法上的 `[Cache]`。
3. 实现类上的 `[Cache]`。

缓存 key 默认：

```text
{TargetType}.{Method}:{arg0}_{arg1}_...
```

如果参数是复杂对象，默认 `ToString()` 往往只是类型名，容易产生 key 冲突。复杂查询要么不要直接用 `[Cache]`，要么提供稳定参数或单独封装 key。

### `[ConfigValidation]` 被哪里读取

`ConfigValidationInterceptor` 当前从 `invocation.MethodInvocationTarget` 读取 `ConfigValidationAttribute`。这意味着特性应加在实现类方法上，而不是只加在接口方法上。

它还会检查返回类型：

```text
Task<ServiceResult>
Task<ServiceResult<T>>
```

不符合就直接执行原方法，不短路。

## 调试 AOP 的具体断点

如果怀疑拦截器没有生效，可以按顺序打断点：

1. `ServiceRegistrar.RegisterService`：确认服务注册时启用了 `EnableInterfaceInterceptors`。
2. `ServiceRegistrar.RegisterServiceWithoutInterceptor`：确认服务没有被排除。
3. 具体拦截器的 `Intercept` 方法：确认代理链是否进入。
4. 对应特性读取方法：确认特性能否从方法或类上读到。
5. 原业务方法：确认是否被短路。

如果 1 没进，问题在注册扫描；如果 3 没进，问题在代理调用；如果 3 进了但不执行逻辑，问题在特性、返回类型或拦截器内部判断。

## 典型反例

### 给写操作加缓存

```csharp
[Cache(Duration = 600)]
public async Task<ApiResponse> SubmitAsync(SubmitDto dto)
{
    ...
}
```

风险：

- 第二次相同参数可能直接返回缓存结果。
- 原业务方法不执行。
- 状态、库存、任务不会更新。

### 配置校验方法返回 `ApiResponse`

```csharp
[ConfigValidation(ValidatorCodes.MIXED_BATCH)]
public async Task<ApiResponse> SubmitAsync(...)
{
}
```

当前不会短路，因为返回类型不符合 `ConfigValidationInterceptor` 要求。应改成服务内部 `Task<ServiceResult>` 方法，或在方法体内显式校验。

## 最小示例

一个带日志、缓存、配置校验的服务方法可能像这样：

```csharp
[RegisteredService(ServiceType = typeof(IDemoService))]
public class DemoService : IDemoService
{
    [LogInterceptor(LogParameters = true, LogReturnValue = false)]
    [ConfigValidation(ValidatorCodes.BIND_DATA_NOT_EMPTY)]
    public async Task<ServiceResult> SubmitAsync(List<ContainerBindDto> binds)
    {
        ...
        return ServiceResult.Ok();
    }

    [Cache(Duration = 600, KeyPrefix = "demo:lookup")]
    public async Task<ApiResponse> GetLookupAsync(string type)
    {
        ...
        return ApiResponse.Ok(data);
    }
}
```

## 常见误区与排查清单

### 拦截器不生效

- 服务是否通过 DI 解析，而不是手动 `new`。
- 调用方是否注入接口。
- 服务是否被 `WithoutInterceptor=true` 排除。
- 是否是类内部 `this.Method()` 自调用。自调用不会重新经过代理。
- 方法返回类型是否符合对应拦截器要求。

### 缓存结果不对

- key 默认由类型、方法、参数拼接，参数对象的 `ToString()` 可能不够稳定。
- 写接口不要打开缓存。
- 数据更新后是否清理相关缓存。
- `ApiResponse.Data` 为 `null` 时不会写入缓存。

### 配置校验没有短路

- 方法是否返回 `Task<ServiceResult>` 或 `Task<ServiceResult<T>>`。
- Validator 的 `Code` 是否和 `ValidatorCodes` 一致。
- Validator 是否注册为 `IValidator`。
- Validator 是否因为找不到配置服务而直接放行。

### 异常处理边界

- AOP 异常拦截器主要记录和再抛。
- HTTP 响应统一格式由 `GlobalExceptionFilter` 负责。
- Service 内不要到处 catch 后吞异常，否则事务和全局异常都可能失去判断依据。
