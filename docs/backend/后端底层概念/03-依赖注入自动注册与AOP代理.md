# 03 依赖注入自动注册与 AOP 代理

## 这个概念解决什么问题

依赖注入自动注册解决的是“新增服务后不用到启动类里手写注册”的问题。AOP 代理解决的是“日志、缓存、校验、异常、性能统计这些横切逻辑不用写进每个业务方法”的问题。

KH.WMS 使用 Autofac + Castle DynamicProxy：

- `ServiceRegistrar` 扫描带注册特性的类。
- 普通服务注册到接口或自身。
- 默认启用拦截器代理。
- 设置 `WithoutInterceptor=true` 时只注册服务，不挂 AOP。

理解这一层后，才能判断为什么某个服务能注入但 AOP 不生效，或者为什么某个底层服务必须关闭拦截器。

## 什么时候需要看

- 新增 Service、Contract、Validator、底层工具服务。
- 报错 `Unable to resolve service for type ...`。
- `[LogInterceptor]`、`[Cache]`、`[ConfigValidation]` 加了但不生效。
- 不知道该用 `[RegisteredService]` 还是 `[SelfRegisteredService]`。
- 一个类实现多个接口，不知道为什么注入错接口。
- 需要把某个服务排除出 AOP，避免递归、循环依赖或性能问题。

## 业务开发应该怎么用

### 有接口的服务

绝大多数业务 Service 都应该有接口，并使用 `[RegisteredService]`：

```csharp
[RegisteredService(ServiceType = typeof(IInboundOrderService))]
public class InboundOrderService : IInboundOrderService
{
}
```

如果类只实现一个接口，`ServiceType` 有时可以省略。但业务开发推荐显式写上，减少多接口或继承接口时的误判。

### 没有接口的服务

少数工具类没有接口时，用 `[SelfRegisteredService]`：

```csharp
[SelfRegisteredService]
public class LocalToolService
{
}
```

这类服务通过具体类型注入：

```csharp
public class DemoService(LocalToolService tool)
{
}
```

### 泛型服务

`CrudService<TEntity>` 是开放泛型注册：

```csharp
[RegisteredService(ServiceType = typeof(ICrudService<>))]
public class CrudService<TEntity> : ICrudService<TEntity>
{
}
```

业务模块如果继承 `CrudService<TEntity>` 并实现自己的接口，也要给子类加注册特性。

### 什么时候用 `WithoutInterceptor`

`WithoutInterceptor=true` 表示“只注册 DI，不启用 AOP 代理”。常见场景：

- `IUnitOfWork`、数据库上下文、缓存、用户上下文等底层服务。
- AOP 拦截器自己依赖的服务，避免递归拦截。
- Validator 这类被拦截器动态解析和调用的服务。
- 极底层、频繁调用、没有业务日志价值的服务。

示例：

```csharp
[RegisteredService(WithoutInterceptor = true, ServiceType = typeof(IValidator))]
public class MixedBatchValidator : IValidator
{
}
```

不要因为“日志太多”就随意关业务 Service 的拦截器。优先调整特性使用或日志级别。

### `WithoutInterceptor=true` 的准确含义

`WithoutInterceptor=true` 不是“这个服务不写日志”这么简单，它走的是 `ServiceRegistrar.RegisterServiceWithoutInterceptor` 分支。这个分支只执行普通 Autofac 注册：

```csharp
builder.RegisterType(implementationType).As(serviceType).InstancePerLifetimeScope();
```

它不会调用：

```csharp
EnableInterfaceInterceptors()
EnableClassInterceptors()
InterceptedBy(interceptors)
```

所以结论要分清楚：

| 情况 | DI 能否注入 | 是否生成 Castle 代理 | 是否进入 `Logging/Caching/ConfigValidation/Exception/Performance` 拦截器 |
| --- | --- | --- | --- |
| 普通 `[RegisteredService]` | 能 | 是 | 会进入代理链 |
| `[RegisteredService(WithoutInterceptor = true)]` | 能 | 否 | 完全不会进入 |
| 普通服务但方法没加 `[Cache]`/`[ConfigValidation]` | 能 | 是 | 会进入代理链，但对应能力会直接放行 |
| 手动 `new Service()` | 不经过 DI | 否 | 完全不会进入 |

也就是说，如果一个服务设置了 `WithoutInterceptor=true`，即使方法上写了 `[Cache]`、`[LogInterceptor]`、`[ConfigValidation]`，这些特性也不会通过 AOP 自动执行。特性只是元数据，必须有代理链读取它才会生效。

业务开发遇到这几个类型时要默认认为它们“不应该被 AOP 包住”：

- `UnitOfWork`、`SqlSugarDbContext`：事务和数据库上下文是底层状态，不适合再套业务拦截器。
- `LoggerService`、缓存服务、用户上下文：很多拦截器本身依赖它们，打开拦截器容易形成递归或噪声。
- `IValidator` 实现：`ConfigValidationInterceptor` 会动态解析并执行 Validator，Validator 本身再被拦截没有业务价值。
- `LicenseService`、`JwtTokenService` 这类底座服务：调用频繁，并且通常处在请求管道或鉴权链路里。

真正要关闭业务 Service 的拦截器时，应该能回答两个问题：这个服务的缓存、配置校验、异常记录、性能记录全部都不需要吗？调用链排查少掉它是否可接受？如果只是某个方法日志太多，优先调整 `[LogInterceptor]` 参数、日志级别或不要给该方法加日志特性。

## 底层逻辑和实现

### 自动注册扫描

`ServiceExtensions` 在 Autofac 容器构建时执行：

```csharp
protected override void Load(ContainerBuilder builder)
{
    List<Assembly> assemblies = AssemblyService.GetReferencedAssemblies();

    IServiceRegistrar registrar = new ServiceRegistrar();
    registrar.Register(builder, assemblies.ToArray());
}
```

`ServiceRegistrar` 会扫描这些程序集里带以下特性的类：

- `RegisteredServiceAttribute`
- `SelfRegisteredServiceAttribute`

### `[RegisteredService]` 的含义

关键属性：

- `Lifetime`：生命周期，默认 `Scoped`。
- `ServiceType`：明确注册成哪个接口。
- `WithoutInterceptor`：是否跳过 AOP 代理。

注册逻辑大致是：

1. 找到实现类的接口。
2. 如果指定了 `ServiceType`，用它作为服务类型。
3. 如果是开放泛型，用 `RegisterGeneric`。
4. 如果不是开放泛型，用 `RegisterType`。
5. 默认启用接口拦截器 `EnableInterfaceInterceptors()`。

接口代理生效的前提是：调用方注入的是接口，而不是自己 `new` 实现类。

### `[SelfRegisteredService]` 的含义

自注册服务没有独立接口，注册成自身类型：

- 默认启用类拦截器 `EnableClassInterceptors()`。
- 如果关闭拦截器，就只是普通自身类型注册。

类代理通常要求方法可被代理，业务上仍建议优先使用接口服务。

### 生命周期怎么选

| 生命周期 | 适用场景 | 注意 |
| --- | --- | --- |
| `Scoped` | 绝大多数业务 Service、Repository、UnitOfWork | 每个请求一个作用域，推荐默认 |
| `Singleton` | 无状态、线程安全、配置类、内存缓存服务 | 不要直接依赖 Scoped 服务 |
| `Transient` | 轻量、无状态、短生命周期对象 | 每次解析都会创建实例 |

业务开发默认用 `Scoped` 即可。

### 拦截器挂载顺序

`ServiceRegistrar` 中默认挂载这些拦截器：

1. `LoggingInterceptor`
2. `CachingInterceptor`
3. `ConfigValidationInterceptor`
4. `ExceptionInterceptor`
5. `PerformanceInterceptor`

不是每个拦截器都会对每个方法做事。有些拦截器需要特性触发，例如缓存和配置校验；有些会记录或包装调用。

这里也要区分两句话：

- “拦截器挂上了”：注册时 `InterceptedBy(interceptors)` 把五个拦截器接到代理链上。
- “拦截器做事了”：运行时拦截器读到特性、返回类型满足要求、内部条件满足后才执行核心逻辑。

例如一个普通业务服务方法什么特性都不加，只要它通过接口代理调用，仍然会经过代理链；但 `CachingInterceptor` 读不到 `[Cache]` 会直接 `invocation.Proceed()`，`ConfigValidationInterceptor` 读不到 `[ConfigValidation]` 也会直接放行。反过来，如果服务本身 `WithoutInterceptor=true`，代理链都不存在，任何方法特性都不会被读取。

## 注册决策树

`ServiceRegistrar` 对每个带注册特性的类型大致按下面逻辑处理：

```text
发现类型
  -> 是 [RegisteredService] 吗？
      -> 没有接口？
          -> 当成自注册处理，并输出建议
      -> 指定 ServiceType？
          -> 用 ServiceType 注册
      -> 开放泛型？
          -> RegisterGeneric(实现).As(接口)
      -> WithoutInterceptor=true？
          -> 只注册，不启用代理
      -> 否则
          -> EnableInterfaceInterceptors + InterceptedBy(...)

  -> 是 [SelfRegisteredService] 吗？
      -> WithoutInterceptor=true？
          -> 只注册自身类型
      -> 否则
          -> EnableClassInterceptors + InterceptedBy(...)
```

这解释了几个常见现象：

- `[RegisteredService]` 标在无接口类上也可能注册成功，但会变成自注册风格，不适合接口注入。
- 实现多个接口时，如果不写 `ServiceType`，可能注册到不是你预期的第一个接口。
- `WithoutInterceptor=true` 不影响 DI 注入，只影响代理和 AOP。

## 代理生效条件拆解

### 接口代理

接口代理生效需要同时满足：

1. 类通过 `[RegisteredService]` 注册。
2. 注册时没有 `WithoutInterceptor=true`。
3. 调用方注入的是接口类型。
4. 调用发生在代理对象上。

下面这种会经过代理：

```csharp
public class DemoController(IDemoService service)
{
    public Task<ApiResponse> Submit() => service.SubmitAsync();
}
```

下面这种不会经过代理：

```csharp
var service = new DemoService(...);
await service.SubmitAsync();
```

类内部自调用也不会重新经过代理：

```csharp
public class DemoService : IDemoService
{
    public Task AAsync()
    {
        return BAsync(); // this.BAsync，不经过代理
    }

    [Cache]
    public Task<ApiResponse> BAsync()
    {
        ...
    }
}
```

如果 `BAsync` 必须经过 `[Cache]` 或 `[ConfigValidation]`，不要依赖同类内部自调用触发它。

### 类代理

`[SelfRegisteredService]` 使用类代理。类代理对方法可代理性更敏感，业务开发通常不要把核心业务服务设计成自注册类。推荐“接口 + `[RegisteredService(ServiceType=...)]`”。

## 生命周期深水区

### 为什么业务 Service 默认 Scoped

业务 Service 通常依赖：

- `IUnitOfWork`
- Repository
- `IUserContext`
- 当前请求缓存

这些都是请求作用域语义。用 Singleton 注册业务 Service 会导致：

- 无法安全依赖 Scoped 服务。
- 用户上下文可能错乱。
- 事务上下文不符合请求边界。

所以除非确定无状态且线程安全，不要把业务 Service 改成 Singleton。

### Singleton 可以依赖什么

Singleton 适合依赖：

- 配置对象。
- 线程安全缓存。
- 无状态工具。
- 不依赖请求上下文的服务。

Singleton 不应直接持有 `IUnitOfWork`、Repository、`IUserContext`。

## 排查注入问题的具体步骤

1. 找构造函数参数类型，例如 `IInventoryContract`。
2. `rg "ServiceType = typeof\\(IInventoryContract\\)"` 查是否有实现注册。
3. 查实现类所在项目是否输出 DLL。
4. 查实现类是否 public、非 abstract。
5. 查实现类构造函数里的每个参数是否都能注入。
6. 如果只有具体类注册，确认调用方是不是注入了接口。
7. 如果是泛型服务，确认注入的是关闭泛型接口，例如 `ICrudService<MdLocation>`。

## 排查 AOP 问题的具体步骤

1. 看注册特性是否 `WithoutInterceptor=true`。
2. 看调用方是否通过接口拿到服务。
3. 看是否同类内部自调用。
4. 看方法特性是否加在实现方法或类上。
5. 看返回类型是否符合拦截器要求。
6. 在 `ServiceRegistrar.RegisterService` 或 `RegisterServiceWithoutInterceptor` 打断点，看最终走了哪条分支。
7. 在对应拦截器 `Intercept` 入口打断点，确认是否进入代理链。

## 最小示例

### 模块 Service

```csharp
public interface ILocationService : ICrudService<MdLocation>
{
    Task<ApiResponse> GetAvailableByZoneAsync(long warehouseId, string zoneCode);
}

[RegisteredService(ServiceType = typeof(ILocationService))]
public class LocationService(
    IRepository<MdLocation, long> repository,
    IUnitOfWork unitOfWork,
    IDetailSaveService detailSaveService)
    : CrudService<MdLocation>(repository, unitOfWork, detailSaveService),
      ILocationService
{
}
```

### 跨模块 Contract 实现

```csharp
[RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(IInventoryContract))]
public class InventoryContract(IUnitOfWork unitOfWork) : IInventoryContract
{
}
```

### Validator

```csharp
[RegisteredService(WithoutInterceptor = true, ServiceType = typeof(IValidator))]
public class MixedMaterialValidator : IValidator
{
    public string Code => ValidatorCodes.MIXED_MATERIAL;
}
```

## 常见误区与排查清单

### 服务无法注入

- 实现类是否加了注册特性。
- 实现类所在程序集是否被加载。
- 注入的是接口还是实现类，和注册方式是否一致。
- `ServiceType` 是否写成了错误接口。
- 多个实现注册到同一个接口时，是否存在覆盖或解析集合的需求。

### AOP 不生效

- 调用方是否通过接口注入服务。
- 服务是否设置了 `WithoutInterceptor=true`。
- 方法是不是直接在同一个类内部调用。内部 `this.SomeMethod()` 不会重新经过代理。
- 特性是否加在实现方法、接口方法或类上，拦截器是否支持读取该位置。
- 返回类型是否符合拦截器要求，例如 `ConfigValidationInterceptor` 要求 `Task<ServiceResult>` 或 `Task<ServiceResult<T>>`。

### 不要这样做

- 不要在业务代码里手动 `new Service()`，这样绕过 DI 和 AOP。
- 不要给 Validator、UnitOfWork、UserContext 这类底层服务打开拦截器。
- 不要在 Service 没接口时仍使用 `[RegisteredService]` 后注入接口，运行时一定解析不到。
- 不要依赖“第一个接口自动选择”的隐式行为，业务类推荐写 `ServiceType`。
