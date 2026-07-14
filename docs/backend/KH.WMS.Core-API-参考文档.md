---
title: "KH.WMS.Core API 参考文档"
description: "KH.WMS.Core API 参考文档：说明适用场景、当前实现、设计边界与开发或排障入口。"
status: reference
audience: "参与 KH.WMS 开发、测试与运维的团队成员"
reviewed: "2026-07-14"
sourcePaths:
  - "KH.WMS/KH.WMS.Server"
  - "KH.WMS/KH.WMS.Core"
  - "KH.WMS/Modules"
---

# KH.WMS.Core API 参考文档

> **本文档适用于 `KH.WMS.Core` 类库的 DLL 使用者。** 外部团队无法查看源码，所有功能需通过本文档调用。本文档涵盖了该类库中所有公开类型、方法、配置项和最佳实践。

---

## 目录

- [一、快速入门](#一快速入门)
  - [1.1 项目简介](#11-项目简介)
  - [1.2 环境要求](#12-环境要求)
  - [1.3 安装与引用](#13-安装与引用)
  - [1.4 最小启动示例](#14-最小启动示例)
- [二、配置指南](#二配置指南)
  - [2.1 依赖注入配置](#21-依赖注入配置)
  - [2.2 中间件管道配置](#22-中间件管道配置)
  - [2.3 数据库配置](#23-数据库配置)
  - [2.4 认证配置（JWT）](#24-认证配置jwt)
  - [2.5 日志配置](#25-日志配置)
  - [2.6 缓存配置](#26-缓存配置)
  - [2.7 CORS 配置](#27-cors-配置)
  - [2.8 License 配置](#28-license-配置)
  - [2.9 Swagger 配置](#29-swagger-配置)
  - [2.10 监控配置（MiniProfiler）](#210-监控配置miniprofiler)
  - [2.11 限流配置](#211-限流配置)
- [三、API 参考](#三api-参考)
  - [3.1 Services 服务层](#31-services-服务层)
  - [3.2 Controllers 控制器](#32-controllers-控制器)
  - [3.3 Database 数据库](#33-database-数据库)
  - [3.4 Authentication 认证](#34-authentication-认证)
  - [3.5 Caching 缓存](#35-caching-缓存)
  - [3.6 Logging 日志](#36-logging-日志)
  - [3.7 Security 安全](#37-security-安全)
  - [3.8 Serialization 序列化](#38-serialization-序列化)
  - [3.9 Mapping 对象映射](#39-mapping-对象映射)
  - [3.10 License 许可证](#310-license-许可证)
  - [3.11 AOP 与特性](#311-aop-与特性)
  - [3.12 Filters 过滤器](#312-filters-过滤器)
  - [3.13 Middlewares 中间件](#313-middlewares-中间件)
  - [3.14 Factories 工厂](#314-factories-工厂)
  - [3.15 User Context 用户上下文](#315-user-context-用户上下文)
  - [3.16 Import/Export 导入导出](#316-importexport-导入导出)
  - [3.17 Validation 验证](#317-validation-验证)
  - [3.18 Modularity 模块化](#318-modularity-模块化)
  - [3.19 DependencyInjection 依赖注入](#319-dependencyinjection-依赖注入)
  - [3.20 Helpers 工具](#320-helpers-工具)
  - [3.21 Constants 常量](#321-constants-常量)
  - [3.22 Exceptions 异常](#322-exceptions-异常)
  - [3.23 Api Responses](#323-api-responses)
  - [3.24 Models / DTOs](#324-models--dtos)
  - [3.25 Configuration 配置](#325-configuration-配置)
  - [3.26 Setup 配置入口](#326-setup-配置入口)
- [四、附录](#四附录)
  - [4.1 错误码速查表](#41-错误码速查表)
  - [4.2 常用常量速查](#42-常用常量速查)

---

## 一、快速入门

### 1.1 项目简介

`KH.WMS.Core` 是一个基于 **.NET 8** 的 WMS（仓储管理系统）核心类库，封装了通用基础设施能力，旨在为上层业务应用提供开箱即用的标准化基础组件。该库以 DLL 形式分发，外部使用者无需了解内部实现细节，只需通过 NuGet 引用并按本文档调用即可。

**核心功能模块：**

| 模块 | 技术选型 | 说明 |
|------|----------|------|
| ORM 数据库访问 | SqlSugarCore 5.1.4 | 提供仓储模式、工作单元、数据库迁移等 |
| JWT 认证 | Microsoft.AspNetCore.Authentication.JwtBearer 8.0 | Token 生成、验证、刷新 |
| 内存缓存 | Microsoft.Extensions.Caching.Memory | 轻量级本地缓存，支持 AOP 缓存拦截 |
| 结构化日志 | Serilog 10.0 + Serilog.Sinks.File/Seq | 按级别/模块分类、自动归档清理 |
| 加密与安全 | AES (System.Security.Cryptography) / RSA (System.Security.Cryptography) | 对称/非对称加密、密码哈希（PBKDF2） |
| Excel 导入导出 | MiniExcel 1.34.2 | 高性能低内存的 Excel 操作 |
| 对象映射 | AutoMapper 16.1 | 实体与 DTO 之间的自动映射 |
| JSON 序列化 | Newtonsoft.Json / System.Text.Json | 统一序列化配置，含自定义转换器 |
| 模块化加载 | Autofac 9.0 + Castle.Core | 自动注册、AOP 拦截器、动态代理 |
| IOC 容器 | Autofac + Autofac.Extras.DynamicProxy | 属性注入、拦截器、模块化注册 |
| API 文档 | Swashbuckle 10.1 (Swagger / OpenAPI) | 自动生成 API 文档 |
| 性能监控 | MiniProfiler 4.5 | 请求链路性能分析 |
| 统一 API 响应 | 自定义 ApiResponse / PagedResponse | 标准化输出格式 |
| 异常处理 | 全局异常过滤器 + 中间件 | 统一错误码与错误消息 |
| 许可证验证 | RSA 签名验证 | 机器码绑定 + 授权文件校验 |
| 限流 | 滑动窗口算法 | 基于内存的请求限流 |

### 1.2 环境要求

**运行时环境：**

| 项目 | 版本/要求 |
|------|-----------|
| .NET SDK | 8.0.x（推荐 8.0.200+） |
| 目标框架 | net8.0 |
| 操作系统 | Windows / Linux / macOS（支持容器化部署） |
| 数据库 | SQL Server / MySQL / PostgreSQL / Oracle（由 SqlSugar 驱动支持） |

**核心 NuGet 依赖（自动引入）：**

| 包名 | 版本 | 用途 |
|------|------|------|
| Autofac | 9.0.0 | IOC 容器扩展 |
| Autofac.Extensions.DependencyInjection | 10.0.0 | ASP.NET Core 集成 |
| Autofac.Extras.DynamicProxy | 7.1.0 | AOP 动态代理 |
| Castle.Core.AsyncInterceptor | 2.1.0 | 异步拦截器支持 |
| SqlSugarCore | 5.1.4.214 | ORM 框架 |
| Serilog.AspNetCore | 10.0.0 | 结构化日志 |
| Serilog.Sinks.File | 7.0.0 | 文件日志输出 |
| Serilog.Sinks.Seq | 9.0.0 | Seq 集中式日志 |
| Serilog.Enrichers.CorrelationId | 3.0.1 | 关联 ID 增强 |
| AutoMapper | 16.1.1 | 对象映射 |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.11 | JWT 认证 |
| Swashbuckle.AspNetCore | 10.1.7 | Swagger/OpenAPI |
| MiniExcel | 1.34.2 | Excel 导入导出 |
| MiniProfiler.AspNetCore.Mvc | 4.5.4 | 性能监控 |
| System.Management | 10.0.5 | 硬件信息获取（机器码） |

### 1.3 安装与引用

**通过 NuGet 安装：**

```shell
dotnet add package KH.WMS.Core --version x.x.x
```

**或直接在 `.csproj` 中添加：**

```xml
<ItemGroup>
  <PackageReference Include="KH.WMS.Core" Version="x.x.x" />
</ItemGroup>
```

> **注意：** `KH.WMS.Core` 的依赖项（如 Autofac、SqlSugar、Serilog 等）会自动传递引入，无需在宿主项目中重复添加。

### 1.4 最小启动示例

以下示例展示如何在 `Program.cs` 中以最简方式启动一个使用 `KH.WMS.Core` 的 Web 应用。

```csharp
using KH.WMS.Core.Setup;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ── 1. 主机配置 ──────────────────────────────────────────
// 加载 appsettings.json / appsettings.{Environment}.json 和环境变量
// 注册 IHttpContextAccessor
builder.Host.ConfigureDefaults();

// 配置 Serilog（控制台 + 文件输出，按天滚动）
builder.Host.AddCustomConfiguration();

// ── 2. 基础设施服务 ──────────────────────────────────────
// 统一入口，自动注册以下模块：
//   • 数据库（SqlSugar）
//   • 缓存（内存缓存）
//   • 认证（JWT Bearer）
//   • 日志（Serilog）
//   • 性能监控（MiniProfiler）
//   • API 文档（Swagger）
//   • CORS
//   • 限流（滑动窗口）
//   • HTTP 客户端
//   • MVC 过滤器（ApiAuthorizeFilter、TraceIdResultFilter）
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// 添加控制器和 API 探索器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ── 3. 中间件管道 ────────────────────────────────────────
// 按以下顺序注册中间件（顺序不可随意调整）：
//   ① 异常处理（最外层捕获）
//   ② License 验证（自动初始化默认授权）
//   ③ HTTPS 重定向（生产环境）
//   ④ 静态文件
//   ⑤ CORS
//   ⑥ 请求日志
//   ⑦ 路由
//   ⑧ MiniProfiler 性能监控
//   ⑨ 认证
//   ⑩ 授权
//   ⑪ 端点映射（Controller + RazorPages）
app.UseCustomMiddleware(app.Environment);

app.Run();
```

**关键 API 说明：**

| 方法 | 所属类 | 作用 |
|------|--------|------|
| `HostSetup.ConfigureDefaults()` | `HostSetup` | 加载配置文件、注册 `IHttpContextAccessor` |
| `HostSetup.AddCustomConfiguration()` | `HostSetup` | 配置 Serilog（控制台 + 文件日志，按天滚动） |
| `ServiceCollectionSetup.AddInfrastructure(configuration, environment)` | `ServiceCollectionSetup` | 统一添加所有基础设施服务 |
| `MiddlewareSetup.UseCustomMiddleware(env)` | `MiddlewareSetup` | 按推荐顺序配置完整中间件管道 |

> **提示：** `AddInfrastructure()` 内部会自动调用各子模块的 Setup 方法（如 `AddSqlSugarSetup`、`AddCacheSetup`、`AddAuthenticationSetup`、`AddLoggingSetup` 等），如需单独配置某个模块，可跳过 `AddInfrastructure` 方法，直接调用对应的 Setup 扩展方法。详见 [二、配置指南](#二配置指南)。

---

## 二、配置指南

> 本章节详细说明各模块的配置方式，包括 `appsettings.json` 配置项、依赖注入注册方法和中间件管道配置。

### 2.1 依赖注入配置

`KH.WMS.Core` 使用 **Autofac** 作为第二 IOC 容器，在 ASP.NET Core 默认 DI 容器之上实现程序集扫描、批量注册和 AOP 拦截器自动附加。

#### 说明

依赖注入由 `ServiceExtensions`（继承 `Autofac.Module`）驱动，其内部调用 `ServiceRegistrar` 扫描所有引用的程序集，查找标记了以下特性的服务类，并按生命周期自动注册到 Autofac 容器：

| 特性 | 说明 | 拦截方式 |
|------|------|---------|
| `[RegisteredService]` | 带接口层的服务注册，实现类会注册到其首个接口（或指定的 `ServiceType`） | `EnableInterfaceInterceptors()` |
| `[SelfRegisteredService]` | 不带接口的自注册，服务自身作为实现类型 | `EnableClassInterceptors()` |

自动附加的 5 个拦截器（按执行顺序）：

| 拦截器 | 说明 |
|--------|------|
| `LoggingInterceptor` | 方法调用日志，记录入参、出参、耗时 |
| `CachingInterceptor` | 根据 `[CacheAttribute]` 缓存方法返回值 |
| `ConfigValidationInterceptor` | 根据 `[ConfigValidationAttribute]` 验证配置参数 |
| `ExceptionInterceptor` | 捕获方法内未处理异常，统一包装 |
| `PerformanceInterceptor` | 记录方法执行耗时，超阈值告警 |

每个特性支持通过 `WithoutInterceptor = true` 跳过拦截器（适用于 `LoggerService` 等基础服务）。

#### 配置步骤

**步骤 1：在 Program.cs 中启用 Autofac**

```csharp
using Autofac;
using Autofac.Extensions.DependencyInjection;
using KH.WMS.Core.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        // 自动扫描所有引用程序集，注册带 [RegisteredService] / [SelfRegisteredService] 的服务
        containerBuilder.RegisterModule(new ServiceExtensions());
    });
```

**步骤 2：在服务类上标记特性**

```csharp
// 带接口的服务注册
[RegisteredService(Lifetime = ServiceLifetime.Scoped)]
public class ProductService : IProductService
{
    // 自动附加 5 个拦截器
}

// 自注册服务
[SelfRegisteredService(Lifetime = ServiceLifetime.Singleton)]
public class CacheManager
{
    // 自动附加 5 个类拦截器
}

// 跳过拦截器的基础服务
[SelfRegisteredService(Lifetime = ServiceLifetime.Singleton, WithoutInterceptor = true)]
public class LoggerService : ILoggerService
{
    // 不附加拦截器，避免递归
}
```

#### 代码示例

**Program.cs 完整写法：**

```csharp
using Autofac;
using Autofac.Extensions.DependencyInjection;
using KH.WMS.Core.DependencyInjection;
using KH.WMS.Core.Setup;

var builder = WebApplication.CreateBuilder(args);

// 1. Autofac 容器
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.RegisterModule(new ServiceExtensions());
    });

// 2. 基础设施
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
app.UseCustomMiddleware(app.Environment);
app.Run();
```

#### 特性参数说明

**`[RegisteredService]` 属性：**

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Lifetime` | `ServiceLifetime` | `Scoped` | 生命周期：Singleton / Scoped / Transient |
| `WithoutInterceptor` | `bool` | `false` | 是否跳过拦截器 |
| `ServiceType` | `Type?` | `null` | 指定注册的服务类型（默认使用首个接口） |

**`[SelfRegisteredService]` 属性：**

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Lifetime` | `ServiceLifetime` | `Scoped` | 生命周期 |
| `WithoutInterceptor` | `bool` | `false` | 是否跳过拦截器 |

### 2.2 中间件管道配置

`KH.WMS.Core` 通过 `MiddlewareSetup.UseCustomMiddleware()` 方法提供了一套预定义的中间件执行顺序。**中间件的注册顺序决定了请求的处理流程，请勿随意调整。**

#### 说明

`UseCustomMiddleware(IWebHostEnvironment env)` 方法内部按以下顺序注册中间件：

| 序号 | 中间件 | 方法 | 说明 |
|------|--------|------|------|
| ① | 异常处理 | `UseExceptionHandling()` | 最外层捕获，确保任何未处理异常都能返回统一格式的错误响应 |
| ② | License 验证 | `UseLicenseValidation()` | 白名单路径放行，其余请求验证 RSA 签名许可证；首次启动自动初始化 180 天默认授权 |
| ③ | HTTPS 重定向 | `UseHttpsRedirection()` | **仅生产环境**生效 |
| ④ | 静态文件 | `UseCustomStaticFiles(env)` | 处理 wwwroot 下的静态资源 |
| ⑤ | CORS | `UseCustomCors()` | 默认使用 `DefaultPolicy` 策略 |
| ⑥ | 请求日志 | `UseRequestLogging()` | 记录每个请求的方法、路径、状态码、耗时 |
| ⑦ | 路由 | `UseRouting()` | ASP.NET Core 路由中间件 |
| ⑧ | MiniProfiler | `UseMiniProfilerCustom()` | 性能监控，支持 SQL 语句细粒度分析 |
| ⑨ | 认证 | `UseAuthentication()` | JWT Bearer 认证 |
| ⑩ | 授权 | `UseAuthorization()` | 基于策略/角色的授权 |
| ⑪ | 端点映射 | `MapControllers()` + `MapRazorPages()` | 映射 Controller 和 Razor Pages 端点 |

> **注意：** `UseAuthentication` 和 `UseAuthorization` 必须位于 `UseRouting` 之后、`UseEndpoints` 之前，这是 ASP.NET Core 的固定要求。MiniProfiler 必须在 `UseRouting` 之后注册才能正确捕获路由数据。

#### 配置步骤

**步骤 1：在 Program.cs 中调用 `UseCustomMiddleware`**

```csharp
var app = builder.Build();
app.UseCustomMiddleware(app.Environment);
```

**步骤 2（可选）：跳过统一方法，手动注册中间件**

如果需要对中间件顺序进行微调，可以绕过 `UseCustomMiddleware`，手动调用各方法：

```csharp
var app = builder.Build();

app.UseExceptionHandling();
app.UseLicenseValidation();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCustomStaticFiles(app.Environment);
app.UseCustomCors();
app.UseRequestLogging();
app.UseRouting();
app.UseMiniProfilerCustom();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapRazorPages();
});
```

#### 代码示例

```csharp
using KH.WMS.Core.Setup;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Host.AddCustomConfiguration();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// 使用推荐的完整中间件管道
app.UseCustomMiddleware(app.Environment);

app.Run();
```

### 2.3 数据库配置

`KH.WMS.Core` 使用 **SqlSugarCore 5.1.4** 作为 ORM 框架，提供仓储模式、工作单元（UnitOfWork）、CodeFirst 建表和 AOP 事件扩展。

#### 说明

数据库配置由 `SqlSugarSetup.AddSqlSugar()` 方法完成，支持多种数据库类型，通过 `appsettings.json` 的 `DbConnection` 节点进行配置。

**DatabaseOptions 配置属性：**

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `ConnectionString` | `string` | `""` | 数据库连接字符串 |
| `DbType` | `string` | `"sqlserver"` | 数据库类型：`mysql` / `sqlserver` / `postgresql` / `oracle` / `sqlite` |
| `EnableSqlLog` | `bool` | `true` | 是否启用 SQL 日志输出到控制台和 MiniProfiler |
| `CommandTimeout` | `int` | `30` | 命令超时时间（秒） |

**内置 AOP 事件：**

- **OnLogExecuting**：每次 SQL 执行前触发，将 SQL 语句和参数记录到 MiniProfiler；当 `EnableSqlLog = true` 时同时输出到控制台；自动将枚举参数转为字符串。
- **DataExecuting（InsertByObject）**：自动填充 `RootEntity` 的 `CreatedTime`（当前时间）、`CreatedBy`（当前用户ID）、`CreatedByName`（当前用户名）。
- **DataExecuting（UpdateByObject）**：自动填充 `RootEntity` 的 `LastModifiedTime`、`LastModifiedBy`、`LastModifiedByName`。

**枚举存储策略：** 所有枚举类型的属性在数据库中自动映射为 `nvarchar(20)` 字符串存储。

#### 配置步骤

**步骤 1：在 `appsettings.json` 中配置数据库连接**

```json
{
  "DbConnection": {
    "ConnectionString": "Server=localhost;Database=KH_WMS;User Id=sa;Password=YourPassword;TrustServerCertificate=True;",
    "DbType": "sqlserver",
    "EnableSqlLog": true,
    "CommandTimeout": 30
  }
}
```

**步骤 2：通过 `AddSqlSugarSetup` 注册服务**

```csharp
// 方式一：通过统一入口
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// 方式二：单独注册数据库
builder.Services.AddSqlSugarSetup(builder.Configuration);
```

#### 多数据库配置示例

```json
{
  "DbConnection": {
    "ConnectionString": "Server=192.168.1.100;Database=KH_WMS;Uid=root;Pwd=root123;",
    "DbType": "mysql",
    "EnableSqlLog": false,
    "CommandTimeout": 60
  }
}
```

支持的数据类型值：

| DbType 值 | 对应数据库 | 说明 |
|-----------|-----------|------|
| `"sqlserver"` | SQL Server | 默认值 |
| `"mysql"` | MySQL | — |
| `"postgresql"` | PostgreSQL | — |
| `"oracle"` | Oracle | — |
| `"sqlite"` | SQLite | 适用于本地开发和测试 |

#### 数据库初始化（CodeFirst）

`KH.WMS.Core` 内置 `IDatabaseInitService` 接口，可在应用启动时同步执行建库建表操作：

```csharp
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbInit = scope.ServiceProvider.GetRequiredService<IDatabaseInitService>();
    dbInit.InitDatabase();
}
```

建议将此初始化代码置于 `UseCustomMiddleware` 之前，确保中间件依赖的表已创建。

#### 代码示例

```csharp
using KH.WMS.Core.Setup;
using KH.WMS.Core.Database;

var builder = WebApplication.CreateBuilder(args);

// 注册数据库服务
builder.Services.AddSqlSugarSetup(builder.Configuration);

// 或使用统一入口
// builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

var app = builder.Build();

// 启动时同步初始化数据库
using (var scope = app.Services.CreateScope())
{
    var dbInit = scope.ServiceProvider.GetRequiredService<IDatabaseInitService>();
    dbInit.InitDatabase();
}

app.UseCustomMiddleware(app.Environment);
app.Run();
```

### 2.4 认证配置（JWT）

`KH.WMS.Core` 基于 **Microsoft.AspNetCore.Authentication.JwtBearer 8.0** 实现了 JWT Token 的生成、验证和刷新机制。

#### 说明

认证配置由 `AuthenticationSetup.AddAuthenticationSetup()` 方法完成。它从 `appsettings.json` 的 `Jwt` 节点绑定配置到 `JwtTokenOptions`，并通过 PostConfigure 模式确保 JWT Bearer 认证始终注册。

**JwtTokenOptions 配置属性：**

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Secret` | `string` | `"BB3647441FFA4B5DB4E64A29B53CE525"` | 签名密钥（至少 16 字符，建议 32 字符） |
| `Issuer` | `string` | `"KH.WMS"` | 令牌发行者 |
| `Audience` | `string` | `"KH.WMS"` | 令牌受众 |
| `AccessTokenExpirationMinutes` | `int` | `30` | 访问令牌过期时间（分钟） |
| `RefreshTokenExpirationDays` | `int` | `7` | 刷新令牌过期时间（天） |
| `ClockSkewSeconds` | `int` | `5` | 时钟偏差宽容秒数 |
| `ValidateIssuer` | `bool` | `true` | 是否验证 Issuer |
| `ValidateAudience` | `bool` | `true` | 是否验证 Audience |
| `ValidateLifetime` | `bool` | `true` | 是否验证 Lifetime |

**TokenValidationParameters 验证参数：**

| 参数 | 值来源 | 说明 |
|------|--------|------|
| `ValidateIssuerSigningKey` | `true`（固定） | 始终验证签名密钥 |
| `SaveSigninToken` | `true`（固定） | 保存原始 Token 以便后续使用 |
| `MapInboundClaims` | `false`（固定） | 关闭 Claim 类型映射，保留 JWT 标准名称（exp、iss、aud） |
| `IssuerSigningKey` | `new SymmetricSecurityKey(Secret)` | 基于 Secret 创建对称密钥 |

**JWT Bearer Events 处理：**

- **OnAuthenticationFailed**：认证失败时，通过 `ILoggerFactory` 记录错误日志。
- **OnChallenge**：401 未授权时，返回统一 JSON 格式：`{ "message": "授权未通过", "status": false, "code": 401 }`。

#### 配置步骤

**步骤 1：在 `appsettings.json` 中配置 JWT**

```json
{
  "Jwt": {
    "Secret": "BB3647441FFA4B5DB4E64A29B53CE525",
    "Issuer": "KH.WMS",
    "Audience": "KH.WMS",
    "AccessTokenExpirationMinutes": 30,
    "RefreshTokenExpirationDays": 7,
    "ClockSkewSeconds": 5,
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true
  }
}
```

> **注意：** 生产环境请务必更换 `Secret` 为一个不低于 32 字符的随机字符串。

**步骤 2：注册认证服务**

```csharp
// 方式一：通过统一入口
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// 方式二：单独注册 JWT 认证
builder.Services.AddAuthenticationSetup(builder.Configuration);
```

**步骤 3：确保中间件顺序正确**

```csharp
app.UseAuthentication();  // 在 UseRouting 之后
app.UseAuthorization();   // 在 UseAuthentication 之后
```

#### Token 生成示例

```csharp
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IJwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // 验证用户身份（略）
        
        // 生成 Token
        var token = _jwtTokenService.GenerateToken(userId: "123", userName: "admin", roles: new[] { "Admin" });
        
        // 同时生成刷新令牌
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        
        return Ok(new { token.AccessToken, token.ExpiresAt, refreshToken });
    }
}
```

### 2.5 日志配置

`KH.WMS.Core` 使用 **Serilog** 作为结构化日志框架，支持控制台输出、多级别文件输出、按模块分离日志、Seq 集中式日志和自定义日志文件名。

#### 说明

日志配置包含两层：**主机级配置**（通过 `HostSetup.AddCustomConfiguration()` 配置基本的控制台 + 文件日志）和 **服务级配置**（通过 `LoggingSetup.AddLoggingSetup()` 注册 `ILoggerService` 服务）。

**SerilogOptions 配置属性：**

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `MinimumLevel` | `string` | `"Information"` | 最低日志级别：Verbose / Debug / Information / Warning / Error / Fatal |
| `LogDirectory` | `string` | `"Logs"` | 日志文件存储目录 |
| `RetentionDays` | `int` | `30` | 日志保留天数 |
| `MaxFileSizeMB` | `int` | `100` | 单个日志文件最大大小（MB），超过后自动滚动 |
| `SplitByModule` | `bool` | `false` | 是否按 WMS 模块分离日志文件 |
| `WriteToConsole` | `bool` | `true` | 是否输出到控制台 |
| `WriteToFile` | `bool` | `true` | 是否输出到文件 |
| `CustomLogFileName` | `string?` | `null` | 自定义普通日志文件名（不含路径和扩展名） |
| `CustomErrorFileName` | `string?` | `null` | 自定义错误日志文件名 |
| `CustomWarningFileName` | `string?` | `null` | 自定义警告日志文件名 |

**SerilogFileConfiguration.ConfigureFileLogging 参数：**

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `logPath` | `string` | `"Logs"` | 日志文件路径 |
| `retentionDays` | `int` | `30` | 保留天数 |
| `maxFileSizeMB` | `int` | `100` | 单个文件最大大小（MB） |
| `restrictedToMinimumLevel` | `LogEventLevel` | `Information` | 最低日志级别 |

**默认输出文件：**

| 文件 | 级别 | 说明 |
|------|------|------|
| `Logs/log-.txt` | Information 及以上 | 普通日志，按天滚动 |
| `Logs/error-.txt` | Error 及以上 | 错误日志 |
| `Logs/warning-.txt` | Warning | 警告日志（不含 Error） |

**日志输出模板（文件）：**
```
{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ModuleCode}] [LogType:{LogType}] [TenantId:{TenantId}] [UserId:{UserId}] [RequestId:{RequestId}] {Message:lj}{NewLine}{Exception}
```

**内置 Enricher（日志增强器）：**

| Enricher | 添加的属性 | 说明 |
|----------|-----------|------|
| `LogEnricher` | — | 基础日志上下文 |
| `ModuleLogEnricher` | `ModuleCode` | WMS 模块代码（2001-2021） |
| `LogTypeEnricher` | `LogType` | 日志类型分类 |
| `UserLogEnricher` | `UserId` | 当前操作用户 ID |
| `CorrelationIdEnricher` | `RequestId` | HTTP 请求关联 ID |

#### 配置步骤

**步骤 1：在 Program.cs 中配置主机级 Serilog（推荐）**

```csharp
using KH.WMS.Core.Setup;

var builder = WebApplication.CreateBuilder(args);

// 主机级 Serilog 配置（控制台 + 按天滚动的文件日志）
builder.Host.AddCustomConfiguration();
```

**步骤 2（可选）：使用 `AddSerilog` 方法进行详细配置**

```csharp
using KH.WMS.Core.Logging.Serilog;

builder.Host.AddSerilog(
    appName: "KH.WMS",
    logDirectory: "Logs",
    retentionDays: 30,
    maxFileSizeMB: 5,
    logFileName: "myapp",
    errorFileName: "myapp-error",
    warningFileName: "myapp-warning"
);
```

**步骤 3：通过 `appsettings.json` 覆盖默认配置**

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteToConsole": true,
    "LogPath": "Logs",
    "RetentionDays": 30,
    "MaxFileSizeMB": 5,
    "FileNames": {
      "Log": "myapp",
      "Error": "myapp-error",
      "Warning": "myapp-warning"
    }
  }
}
```

**步骤 4：注册日志服务**

```csharp
// 方式一：统一入口
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// 方式二：单独注册
builder.Services.AddLoggingSetup(builder.Configuration);
```

#### 完整代码示例

```csharp
using KH.WMS.Core.Logging.Serilog;
using KH.WMS.Core.Setup;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 配置 Serilog 日志
builder.Host.AddSerilog(
    appName: "KH.WMS",
    logDirectory: "Logs",
    retentionDays: 30,
    maxFileSizeMB: 5
);

// 注册基础设施服务
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
app.UseCustomMiddleware(app.Environment);
app.Run();
```

#### 按模块分离日志

当 `SplitByModule = true` 或通过代码启用模块日志时，`SerilogSetup` 按 WMS 模块代码（2001-2021）动态路由日志到 `Logs/wms/{ModuleName}-.txt` 文件。支持的模块包括：Inbound、Outbound、Inventory、Owner、Product、Warehouse、Zone、Location、Counting、Transfer、Wave、Picking、Loading、Unloading、Processing、Replenishment、Strategy、Order、Transport、Billing、Report。

### 2.6 缓存配置

`KH.WMS.Core` 使用 **Microsoft.Extensions.Caching.Memory** 实现本地内存缓存，通过统一的 `ICacheService` 接口对外提供服务。

#### 说明

缓存配置由 `CacheSetup.AddCacheSetup()` 方法完成，内部注册 ASP.NET Core 内置的 `IMemoryCache`，并封装为 `MemoryCacheService` 以统一接口对外暴露。

**注册的服务：**

| 接口 | 实现 | 生命周期 |
|------|------|---------|
| `ICacheService` | `MemoryCacheService` | Singleton |
| `IMemoryCacheService` | `MemoryCacheService` | Singleton |

**CacheEntryOptions 预定义缓存策略：**

| 策略 | 过期时间 | 说明 |
|------|---------|------|
| `Default` | 30 分钟（绝对过期） | 默认策略 |
| `Short` | 5 分钟（绝对过期） | 短期缓存 |
| `Medium` | 30 分钟（绝对过期） | 中期缓存 |
| `Long` | 2 小时（绝对过期） | 长期缓存 |
| `Sliding` | 10 分钟（滑动过期） | 访问则延长 |
| `Never` | 永不过期 | 需设置 `CacheItemPriority.NeverRemove`，谨慎使用 |

**自定义缓存策略：**

```csharp
// 自定义绝对过期时间 + 滑动过期时间
var options = CacheEntryOptions.Create(
    absoluteExpiration: TimeSpan.FromMinutes(10),
    slidingExpiration: TimeSpan.FromMinutes(2)
);
```

#### 配置步骤

**步骤 1：注册缓存服务**

```csharp
// 方式一：通过统一入口
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// 方式二：单独注册缓存
builder.Services.AddCacheSetup(builder.Configuration);
```

**步骤 2：在业务代码中注入并使用**

```csharp
public class ProductService : IProductService
{
    private readonly ICacheService _cache;

    public ProductService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<ProductDto> GetProductAsync(int id)
    {
        // 尝试从缓存获取
        var product = _cache.GetOrCreate($"product:{id}", () =>
        {
            // 缓存未命中时从数据库加载
            return LoadFromDatabase(id);
        }, expiration: TimeSpan.FromMinutes(10));

        return product;
    }
}
```

#### 缓存键命名规范

建议遵循 `{业务模块}:{对象类型}:{标识符}` 的三段式命名规范：

| 缓存键模式 | 示例 | 说明 |
|-----------|------|------|
| `{module}:{entity}:{id}` | `wms:product:123` | 单个实体 |
| `{module}:{entity}:list:{params}` | `wms:product:list:page=1` | 列表查询 |
| `{module}:{entity}:code:{code}` | `wms:warehouse:code:WH001` | 按编码查询 |
| `sys:{category}` | `sys:config:global` | 系统配置 |

#### 代码示例

```csharp
using KH.WMS.Core.Caching;
using KH.WMS.Core.Caching.Memory;
using KH.WMS.Core.Setup;

var builder = WebApplication.CreateBuilder(args);

// 注册缓存
builder.Services.AddCacheSetup(builder.Configuration);

// 使用缓存示例
var cache = builder.Services.BuildServiceProvider().GetRequiredService<ICacheService>();

// 设置缓存
cache.Set("greeting", "Hello WMS", TimeSpan.FromMinutes(30));

// 获取缓存
var value = cache.Get<string>("greeting");

// 获取或创建
var data = cache.GetOrCreate("config:system", () =>
{
    return LoadSystemConfig();
}, CacheEntryOptions.Long);

// 删除缓存
cache.Remove("greeting");
```

### 2.7 CORS 配置

`KH.WMS.Core` 通过 `CorsMiddlewareExtensions` 提供了开箱即用的跨域资源共享（CORS）配置，内置三种预定义策略。

#### 说明

CORS 配置由 `CorsMiddlewareExtensions.AddCustomCors()` 方法完成。它从 `appsettings.json` 的 `Cors` 节点读取配置，并注册三种命名策略：

**三种预定义策略：**

| 策略名 | 允许来源 | 说明 |
|--------|---------|------|
| `DefaultPolicy` | 根据 `CorsOptions` 动态配置 | 灵活策略，支持精确来源 + 凭据 |
| `DevelopmentPolicy` | 所有来源 (`*`) | 开发环境使用，支持预检缓存 3600 秒 |
| `ProductionPolicy` | 仅配置的 `AllowedOrigins` | 生产环境，支持凭据 |

**CorsOptions 配置属性：**

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `AllowAnyOrigin` | `bool` | `false` | 是否允许任意来源（与 `AllowCredentials` 冲突，二选一） |
| `AllowedOrigins` | `string[]` | `[]` | 允许的来源列表（`AllowAnyOrigin = false` 时使用） |
| `AllowAnyMethod` | `bool` | `true` | 是否允许任何 HTTP 方法 |
| `AllowAnyHeader` | `bool` | `true` | 是否允许任何请求头 |
| `ExposedHeaders` | `string[]?` | `null` | 暴露给客户端的响应头列表 |
| `SetPreflightMaxAge` | `bool` | `false` | 是否设置预检请求缓存时间 |
| `PreflightMaxAgeSeconds` | `int` | `600` | 预检请求缓存时间（秒） |

#### 配置步骤

**步骤 1：在 `appsettings.json` 中配置 CORS**

```json
{
  "Cors": {
    "AllowAnyOrigin": false,
    "AllowedOrigins": ["http://localhost:3000", "https://wms.example.com"],
    "AllowAnyMethod": true,
    "AllowAnyHeader": true,
    "ExposedHeaders": ["X-Trace-Id", "X-Request-Id"],
    "SetPreflightMaxAge": true,
    "PreflightMaxAgeSeconds": 600
  }
}
```

**步骤 2：注册 CORS 服务**

```csharp
// 方式一：通过统一入口
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// 方式二：单独注册 CORS
builder.Services.AddCustomCors(builder.Configuration);
```

**步骤 3：在中间件管道中使用 CORS**

```csharp
// 使用 DefaultPolicy（默认）
app.UseCustomCors();

// 指定策略
app.UseCustomCors("DevelopmentPolicy");
app.UseCustomCors("ProductionPolicy");
```

#### 代码示例

```csharp
using KH.WMS.Core.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// 注册 CORS 服务
builder.Services.AddCustomCors(builder.Configuration);

var app = builder.Build();

// 开发环境使用宽松策略
if (app.Environment.IsDevelopment())
{
    app.UseCustomCors("DevelopmentPolicy");
}
else
{
    app.UseCustomCors("ProductionPolicy");
}

// ... 后续中间件
app.UseRouting();
app.Run();
```

#### DefaultPolicy 行为说明

- 当 `AllowAnyOrigin = true`：调用 `AllowAnyOrigin()`，不支持 `AllowCredentials()`。
- 当 `AllowAnyOrigin = false`：调用 `WithOrigins(AllowedOrigins)` + `AllowCredentials()`。
- 方法：默认调用 `AllowAnyMethod()`；如设置为 `false`，则限制为 `GET, POST, PUT, DELETE, OPTIONS, PATCH`。
- 请求头：默认调用 `AllowAnyHeader()`；如设置为 `false`，则限制为 `Content-Type, Authorization, X-Requested-With`。

### 2.8 License 配置

`KH.WMS.Core` 实现了基于 **RSA 签名验证** 的许可证（License）机制，通过机器码绑定 + 授权文件校验来保护系统安全。

#### 说明

License 验证由 `LicenseValidationMiddleware` 中间件执行，在 `UseCustomMiddleware` 中于异常处理之后、认证授权之前注册。首次启动时自动初始化默认授权。

**白名单路径（无需 License 验证）：**

| 路径 | 说明 |
|------|------|
| `/api/license/machine-code` | 获取机器码 |
| `/api/license/import` | 导入许可证文件 |
| `/api/license/info` | 查询许可证信息 |
| `/swagger` | Swagger 文档页面 |
| `/health` | 健康检查端点 |
| `/healthchecks` | 健康检查端点 |

**License 验证流程：**

1. 请求到达中间件，判断路径是否在白名单中 → 白名单则放行。
2. 非白名单请求，调用 `ILicenseService.ValidateLicense()` 验证 RSA 签名。
3. 验证通过则继续执行；验证失败返回 403 Forbidden 及错误信息。
4. 首次启动时，`MiddlewareSetup.UseCustomMiddleware` 自动调用 `licenseService.EnsureDefaultLicense()` 生成 RSA 密钥对并创建默认 180 天授权。

#### 配置步骤

**步骤 1：注册 License 服务**

License 服务在 `AddInfrastructure` 中自动注册，无需手动配置。

**步骤 2：启用 License 验证中间件**

```csharp
// 方式一：通过统一中间件管道
app.UseCustomMiddleware(app.Environment);

// 方式二：手动注册
app.UseLicenseValidation();
```

**步骤 3：通过 License API 管理许可证**

内置的 `LicenseController` 提供了以下 API：

| 接口 | 方法 | 说明 |
|------|------|------|
| `/api/license/machine-code` | GET | 获取当前服务器的机器码 |
| `/api/license/info` | GET | 查询当前许可证信息 |
| `/api/license/import` | POST | 导入许可证文件 |
| `/api/license/generate` | POST | 生成许可证（管理员使用） |

#### 代码示例

```csharp
using KH.WMS.Core.License.Middleware;
using KH.WMS.Core.Setup;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseExceptionHandling();
app.UseLicenseValidation();  // 在认证授权之前注册
// ... 后续中间件

app.Run();
```

#### appsettings.json 配置

License 模块的 RSA 密钥对和授权数据存储在应用数据目录中，无需在 `appsettings.json` 中配置。首次启动自动生成以下文件：

| 文件 | 说明 |
|------|------|
| `App_Data/license/private_key.pem` | RSA 私钥（用于生成许可证） |
| `App_Data/license/public_key.pem` | RSA 公钥（用于验证许可证） |
| `App_Data/license/default.lic` | 默认授权文件（180 天有效期） |

### 2.9 Swagger 配置

`KH.WMS.Core` 基于 **Swashbuckle.AspNetCore 10.1** 提供 API 文档自动生成能力，支持 OpenAPI 规范、JWT 认证集成和 XML 注释。

#### 说明

Swagger 配置由 `SwaggerSetup` 完成，包含服务注册和中间件使用两个阶段。配置信息从 `appsettings.json` 的 `Swagger` 节点读取。

**SwaggerOptions 配置属性：**

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Title` | `string` | `"API Documentation"` | 文档标题 |
| `Version` | `string` | `"v1"` | API 版本 |
| `Description` | `string` | `"API Documentation"` | 文档描述 |
| `ContactName` | `string` | `""` | 联系人名称 |
| `ContactEmail` | `string` | `""` | 联系人邮箱 |
| `ContactUrl` | `string` | `""` | 联系人 URL |
| `LicenseName` | `string` | `""` | 许可证名称 |
| `LicenseUrl` | `string` | `""` | 许可证 URL |
| `RoutePrefix` | `string` | `"swagger"` | Swagger UI 路由前缀 |
| `EnableJwt` | `bool` | `true` | 是否启用 JWT 认证安全定义 |

**内置过滤器（可选启用）：**

| 过滤器 | 类型 | 说明 |
|--------|------|------|
| `SwaggerDefaultValues` | `IOperationFilter` | 为操作参数添加默认值描述（代码中注释，需手动启用） |
| `SwaggerHeaderFilter` | `IDocumentFilter` | 在生成的文档中添加自定义 Header 参数（代码中注释，需手动启用） |

#### 配置步骤

**步骤 1：在 `appsettings.json` 中配置 Swagger**

```json
{
  "Swagger": {
    "Title": "KH.WMS API 文档",
    "Version": "v1",
    "Description": "KH.WMS 仓储管理系统 API 接口文档",
    "ContactName": "KH.WMS 团队",
    "ContactEmail": "support@khwms.com",
    "ContactUrl": "https://khwms.com",
    "LicenseName": "MIT",
    "LicenseUrl": "https://opensource.org/licenses/MIT",
    "RoutePrefix": "swagger",
    "EnableJwt": true
  }
}
```

**步骤 2：注册 Swagger 服务**

```csharp
// 方式一：通过统一入口
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// 方式二：单独注册 Swagger
builder.Services.AddApiDocumentationSetup(builder.Configuration);
```

**步骤 3：在中间件管道中使用 Swagger**

```csharp
var app = builder.Build();

// 推荐在开发环境启用
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation(builder.Configuration);
}
```

#### 代码示例

```csharp
using KH.WMS.Core.Api.Documentation.Swagger;
using KH.WMS.Core.Setup;

var builder = WebApplication.CreateBuilder(args);

// 注册服务
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// 中间件管道
app.UseCustomMiddleware(app.Environment);

// Swagger（开发环境启用）
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation(builder.Configuration);
}

app.Run();
```

#### Swagger UI 默认行为

- Swagger UI 访问地址：`/swagger`（可通过 `RoutePrefix` 修改）。
- 模型默认折叠（`DefaultModelsExpandDepth(-1)`），减少页面干扰。
- JWT 认证安全定义启用时，Swagger UI 显示 "Authorize" 按钮，输入 Token 即可为后续请求自动附加 `Bearer` 头。
- 自动加载入口程序集的 XML 注释文件（需在 `.csproj` 中启用 XML 文档生成）。

### 2.10 监控配置（MiniProfiler）

`KH.WMS.Core` 集成 **MiniProfiler 4.5** 实现请求链路的性能监控，支持 SQL 语句细粒度分析、请求耗时追踪和页面级性能展示。

#### 说明

MiniProfiler 配置由 `MiniProfilerSetup` 完成，包含服务注册和中间件使用两个阶段。使用内置的 `MiniProfilerMemoryStorage` 进行内存存储。

**MiniProfilerSettings 配置属性：**

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `RouteBasePath` | `string` | `"/profiler"` | MiniProfiler 路由基础路径 |
| `EnableInProduction` | `bool` | `false` | 是否在生产环境启用 |
| `TrackConnectionOpenClose` | `bool` | `true` | 是否跟踪数据库连接的打开和关闭 |
| `StackTraceLength` | `int` | `5` | 堆栈跟踪长度 |

**MiniProfiler 功能特性：**

- SQL 执行追踪：通过 `SqlSugarSetup` 的 AopEvents，所有 SQL 语句自动记录到 MiniProfiler 的 CustomTiming。
- 自动脚本注入：`MiniProfilerInjectorMiddleware` 自动在 HTML 响应的 `</head>` 前注入 MiniProfiler 脚本。
- 环境感知：开发环境默认启用所有请求的分析；生产环境需设置 `EnableInProduction = true`。
- 按请求分析：每个 HTTP 请求生成独立的性能分析结果。

#### 配置步骤

**步骤 1：在 `appsettings.json` 中配置 MiniProfiler**

```json
{
  "MiniProfiler": {
    "RouteBasePath": "/profiler",
    "EnableInProduction": false,
    "TrackConnectionOpenClose": true,
    "StackTraceLength": 5
  }
}
```

**步骤 2：注册 MiniProfiler 服务**

```csharp
// 方式一：通过统一入口
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// 方式二：单独注册 MiniProfiler
builder.Services.AddMonitoringSetup(builder.Configuration, builder.Environment);
```

**步骤 3：在中间件管道中使用（必须在 UseRouting 之后）**

```csharp
app.UseRouting();
app.UseMiniProfilerCustom();  // 或 app.UseMiniProfiler()
```

#### 代码示例

```csharp
using KH.WMS.Core.Setup;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddControllers();
builder.Services.AddRazorPages();  // MiniProfiler UI 需要

var app = builder.Build();

app.UseCustomMiddleware(app.Environment);  // 内部已包含 UseMiniProfilerCustom

app.Run();
```

#### 查看性能数据

1. **MiniProfiler UI**：访问 `/profiler` 路径可查看完整的性能分析结果列表。
2. **页面内嵌面板**：在 HTML 页面左下角显示 MiniProfiler 面板，按 `Alt+P` 快捷切换。
3. **SQL 分析**：每个 SQL 语句的执行时间、参数和返回行数都会被记录。

#### 效果示例

当请求处理完成后，MiniProfiler 面板中会显示：

- 总请求时间
- SQL 查询次数及总耗时（`SQL:` 分类）
- 各中间件/控制器处理耗时
- 数据库连接打开/关闭时间

### 2.11 限流配置

`KH.WMS.Core` 实现了基于内存的**滑动窗口限流**算法，通过 `ICacheService` 按客户端标识记录请求计数，在时间窗口内超过阈值则返回 429 状态码。

#### 说明

限流配置由 `RateLimitMiddleware` 中间件实现，通过 `RateLimitMiddlewareExtensions` 提供注册方法。限流算法基于滑动时间窗口，以内存缓存作为计数器存储。

**RateLimitOptions 配置属性：**

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `RequestLimit` | `int` | `500` | 时间窗口内允许的最大请求数 |
| `WindowSeconds` | `int` | `60` | 时间窗口（秒） |
| `KeyPrefix` | `string` | `"ratelimit"` | 缓存键前缀 |

**客户端标识识别优先级：**

| 优先级 | 来源 | 示例 |
|--------|------|------|
| 1（最高） | JWT 中的 `NameIdentifier` Claim | `user:123` |
| 2 | `X-API-Key` 请求头 | `apikey:abc123` |
| 3（兜底） | 客户端 IP 地址 | `ip:192.168.1.1` |

**触发限流时的响应（HTTP 429）：**

```json
{
  "Code": "RATE_LIMIT_EXCEEDED",
  "Message": "请求过于频繁，请在 60 秒后重试",
  "RetryAfter": 60
}
```

#### 配置步骤

**步骤 1：在 `appsettings.json` 中配置限流参数**

```json
{
  "RateLimit": {
    "RequestLimit": 500,
    "WindowSeconds": 60,
    "KeyPrefix": "ratelimit"
  }
}
```

也可为特定 API 设置更严格的限制：

```json
{
  "RateLimit": {
    "RequestLimit": 100,
    "WindowSeconds": 10,
    "KeyPrefix": "ratelimit"
  }
}
```

**步骤 2：注册限流服务**

```csharp
// 方式一：通过统一入口
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// 方式二：单独注册限流
builder.Services.AddRateLimiting(builder.Configuration);
```

**步骤 3：在中间件管道中使用限流中间件**

```csharp
// 方式一：通过统一中间件管道（默认启用）
app.UseCustomMiddleware(app.Environment);

// 方式二：手动注册（可指定自定义选项）
app.UseRateLimiting(new RateLimitOptions
{
    RequestLimit = 100,
    WindowSeconds = 10,
    KeyPrefix = "ratelimit"
});
```

> **注意：** 在当前的 `UseCustomMiddleware()` 实现中，限流中间件已注释（`//app.UseRateLimiting()`），如需启用请取消注释或手动添加。

#### 代码示例

```csharp
using KH.WMS.Core.Middlewares;
using KH.WMS.Core.Setup;

var builder = WebApplication.CreateBuilder(args);

// 注册服务
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddControllers();

var app = builder.Build();

// 中间件管道
app.UseExceptionHandling();
app.UseLicenseValidation();
app.UseHttpsRedirection();
app.UseCustomStaticFiles(app.Environment);
app.UseCustomCors();
app.UseRequestLogging();

// 启用限流
app.UseRateLimiting(new RateLimitOptions
{
    RequestLimit = 100,
    WindowSeconds = 10,
    KeyPrefix = "ratelimit"
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
```

#### 自定义限流策略

可通过继承 `RateLimitMiddleware` 或替换其构造参数实现自定义策略：

```csharp
// 创建自定义限流选项
var customOptions = new RateLimitOptions
{
    RequestLimit = 1000,      // 每分钟 1000 次
    WindowSeconds = 60,
    KeyPrefix = "custom"
};

app.UseRateLimiting(customOptions);
```

---

## 三、API 参考

> 本章节按模块列出 `KH.WMS.Core` 中的所有公开类型和成员。每个模块包含接口定义、实现类、方法签名、参数说明和示例代码。内容尚待逐步补充完整。

### 3.1 Services 服务层

> 服务层封装了业务逻辑的核心能力，包括 CRUD 基类、领域服务基类、主从表保存、编码生成等。外部团队通过 DI 注入接口后调用。

---

#### 3.1.1 IApplicationService / ApplicationService

**命名空间**: `KH.WMS.Core.Services`

**说明**: 应用服务基类接口（标记接口），提供对象映射能力。所有应用服务（如 `CrudService<TEntity>`）均继承自此基类。

**构造函数参数**:

| 参数名 | 类型 | 说明 |
|--------|------|------|
| `mappingService` | `IMappingService` | 对象映射服务（AutoMapper 封装） |

**方法**:

---

##### `TDestination Map<TDestination>(object source)`

**说明**: 将源对象映射到目标类型

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `source` | `object` | 是 | 源对象实例 |

**返回值**: `TDestination` — 映射后的目标类型对象

**使用示例**:
```csharp
// 在业务服务中
var dto = Map<OrderDto>(orderEntity);
```

---

##### `List<TDestination> MapList<TSource, TDestination>(IEnumerable<TSource> source)`

**说明**: 将源集合映射为目标类型集合

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `source` | `IEnumerable<TSource>` | 是 | 源集合 |

**返回值**: `List<TDestination>` — 映射后的目标类型集合

**使用示例**:
```csharp
var dtoList = MapList<OrderEntity, OrderDto>(orderEntities);
```

---

#### 3.1.2 ICrudService\<TEntity\> / CrudService\<TEntity\>

**命名空间**: `KH.WMS.Core.Services`

**泛型约束**: `TEntity : BaseEntity<long>, new()`

**说明**: 通用 CRUD 应用服务，提供完整的增删改查、分页、导入导出、模板下载功能。子类通过重写 **protected virtual 钩子方法** 实现业务定制。

**构造函数参数**:

| 参数名 | 类型 | 说明 |
|--------|------|------|
| `mappingService` | `IMappingService` | 对象映射服务 |
| `repository` | `IRepository<TEntity, long>` | 实体仓储 |
| `db` | `ISqlSugarClient` | SqlSugar 数据库客户端 |
| `importExportService` | `IImportExportService` | 导入导出服务 |
| `unitOfWork` | `IUnitOfWork` | 工作单元（事务管理） |
| `detailSaveService` | `IDetailSaveService?` | 主从表保存服务（可选） |

##### 接口方法

---

###### `Task<ApiResponse> GetByIdAsync(long id)`

**说明**: 根据主键 ID 获取实体详情（含级联导航属性）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `id` | `long` | 是 | 实体主键 ID |

**返回值**: `Task<ApiResponse>`
- 成功时: `Code = 200`, `Data` 为实体对象
- 失败时: `Code = 404`, `Message = "{TEntity.Name}不存在"`

**使用示例**:
```csharp
var result = await _crudService.GetByIdAsync(1);
if (result.Code == 200) {
    var entity = result.Data as TEntity;
}
```

---

###### `Task<ApiResponse> GetPagedListAsync(AdvancedQueryRequestDto query)`

**说明**: 高级分页查询，支持自定义表达式过滤、多字段排序、额外查询条件

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `query` | `AdvancedQueryRequestDto` | 是 | 高级查询请求 DTO（含分页、过滤条件、排序条件） |

**返回值**: `Task<ApiResponse>`
- 成功时: `Data` 为 `{ items: List<TEntity>, total: int }` 匿名对象
- 内部调用 `BuildQueryExpression` → `ApplyAdditionalQuery` → `ApplySorting` → `AfterQueryAsync`

**使用示例**:
```csharp
var query = new AdvancedQueryRequestDto
{
    PageIndex = 1,
    PageSize = 20,
    Filters = new List<FilterCondition> { /* ... */ },
    SortConditions = new List<SortCondition> { new() { Field = "CreatedTime", Direction = "desc" } }
};
var result = await _crudService.GetPagedListAsync(query);
```

---

###### `Task<ApiResponse> GetListAsync()`

**说明**: 获取全部实体列表（适用于下拉选择等场景）

**返回值**: `Task<ApiResponse>`
- 成功时: `Data` 为 `List<TEntity>`
- 内部调用 `BuildListExpression()` 构建过滤表达式

**使用示例**:
```csharp
var result = await _crudService.GetListAsync();
var allEntities = result.Data as List<TEntity>;
```

---

###### `Task<ApiResponse> CreateAsync(TEntity entity)`

**说明**: 新增实体（事务内：主表插入 + 从表保存 + 审计字段填充）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `entity` | `TEntity` | 是 | 待新增的实体对象 |

**返回值**: `Task<ApiResponse>`
- 成功时: `Code = 200`, `Data` 为新增实体的主键 ID, `Message = "新增成功"`
- 失败时: 事务回滚，抛出异常

**内部调用链**: `BeforeCreateAsync` → `FillAuditFields(isCreate: true)` → 插入主表 → `SaveDetailsAsync` → `SaveOneToOneAsync` → `AfterCreateAsync`

---

###### `Task<ApiResponse> UpdateAsync(TEntity entity)`

**说明**: 更新实体（事务内：主表更新 + 从表增量保存 + 审计字段填充）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `entity` | `TEntity` | 是 | 待更新的实体对象（需包含有效 Id） |

**返回值**: `Task<ApiResponse>`
- 成功时: `Code = 200`, `Message = "更新成功"`
- 实体不存在时抛出 `NotFoundException`

**内部调用链**: `GetEntityOrThrowAsync` → `BeforeUpdateAsync` → `CopyProperties` + `CopyDetailProperties` → `FillAuditFields(isCreate: false)` → 更新主表 → `SaveDetailsAsync` → `SaveOneToOneAsync` → `AfterUpdateAsync`

---

###### `Task<ApiResponse> DeleteAsync(long id)`

**说明**: 根据 ID 删除实体（有级联导航属性时级联删除子表数据）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `id` | `long` | 是 | 待删除实体的主键 ID |

**返回值**: `Task<ApiResponse>`
- 成功时: `Code = 200`, `Message = "删除成功"`
- 失败时: `Code = 404`, `Message = "{TEntity.Name}不存在"`

**内部调用链**: `BeforeDeleteAsync` → 删除（含级联）→ `AfterDeleteAsync`

---

###### `Task<ApiResponse> BatchDeleteAsync(List<long> ids)`

**说明**: 批量删除实体

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `ids` | `List<long>` | 是 | 待删除实体的主键 ID 列表 |

**返回值**: `Task<ApiResponse>`
- 成功时: `Code = 200`, `Message = "删除成功"`
- 参数为空时: `Code = 400`, `Message = "请选择要删除的数据"`

**内部调用链**: `BeforeBatchDeleteAsync` → 删除 → `AfterBatchDeleteAsync`

---

###### `Task<ApiResponse> ExportAsync(AdvancedQueryRequestDto query, List<ExportColumnDto>? columns = null, bool exportAll = true)`

**说明**: 导出数据到 Excel（支持列配置：中文表头 + ExtData 展开 + 字典值翻译）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `query` | `AdvancedQueryRequestDto` | 是 | 高级查询请求 DTO |
| `columns` | `List<ExportColumnDto>?` | 否 | 导出列配置（为 null 时直接导出实体） |
| `exportAll` | `bool` | 否 | 是否导出全部数据（false 时分页导出） |

**返回值**: `Task<ApiResponse>`
- 成功时: `Data` 为 Base64 编码的 Excel 文件字节流, `Message = "导出成功"`

**内部调用链**: `BeforeExportAsync` → 查询 → `TransformToExportRows`（有列配置）或 `TransformExportData`（无列配置）→ `_importExportService.ExportAsync`

---

###### `Task<ApiResponse> ImportAsync(Stream fileStream, string fileName)`

**说明**: 从 Excel 文件导入数据

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `fileStream` | `Stream` | 是 | Excel 文件流 |
| `fileName` | `string` | 是 | 文件名（用于判断文件格式） |

**返回值**: `Task<ApiResponse>`
- 成功时: `Data` 为 `{ successCount: int, failCount: int, errors: List<string> }`, `Message = "导入完成"`

**内部调用链**: `_importExportService.ImportAsync` → `BeforeImportAsync` → 逐行 `CreateAsync` → `AfterImportAsync`

---

###### `Task<ApiResponse> DownloadTemplateAsync()`

**说明**: 下载导入模板 Excel 文件

**返回值**: `Task<ApiResponse>`
- 成功时: `Data` 为 Base64 编码的 Excel 模板文件字节流, `Message = "模板下载成功"`

**使用示例**:
```csharp
var result = await _crudService.DownloadTemplateAsync();
var base64 = result.Data as string;
```

---

##### Protected Virtual 钩子方法（重写点）

子类通过重写以下方法实现业务定制：

###### 查询钩子

| 方法签名 | 说明 |
|---------|------|
| `Expression<Func<TEntity, bool>> BuildQueryExpression(AdvancedQueryRequestDto query)` | 构建分页查询表达式，添加业务过滤条件（如 `x => x.IsDeleted == false`） |
| `Expression<Func<T, bool>> BuildQueryExpression<T>(AdvancedQueryRequestDto query)` | 泛型重载，用于跨实体查询 |
| `Expression<Func<TEntity, bool>> BuildListExpression()` | 构建 `GetListAsync` 的查询表达式 |
| `Expression<Func<T, bool>> BuildListExpression<T>()` | 泛型重载 |
| `ISugarQueryable<TEntity> ApplyAdditionalQuery(ISugarQueryable<TEntity> queryable, AdvancedQueryRequestDto query)` | 对 queryable 做额外处理（联表查询、附加条件等） |
| `ISugarQueryable<T> ApplyAdditionalQuery<T>(ISugarQueryable<T> queryable, AdvancedQueryRequestDto query)` | 泛型重载 |
| `Task<List<TEntity>> AfterQueryAsync(AdvancedQueryRequestDto query, List<TEntity> items)` | 查询结果后处理（如数据脱敏、填充附加信息） |

**示例：重写查询钩子**
```csharp
public class OrderService : CrudService<Order>
{
    protected override Expression<Func<Order, bool>> BuildQueryExpression(AdvancedQueryRequestDto query)
    {
        return x => x.Status != OrderStatus.Deleted;
    }

    protected override ISugarQueryable<Order> ApplyAdditionalQuery(
        ISugarQueryable<Order> queryable, AdvancedQueryRequestDto query)
    {
        return queryable.LeftJoin<Warehouse>((o, w) => o.WarehouseId == w.Id)
                        .Select((o, w) => new Order { /* 联表字段 */ });
    }
}
```

###### 增删改钩子

| 方法签名 | 说明 |
|---------|------|
| `Task BeforeCreateAsync(TEntity entity)` | 新增前处理（校验唯一性、数据预处理等） |
| `Task AfterCreateAsync(TEntity entity)` | 新增后处理 |
| `Task BeforeUpdateAsync(TEntity entity)` | 更新前处理（校验等） |
| `Task AfterUpdateAsync(TEntity entity)` | 更新后处理 |
| `Task BeforeDeleteAsync(long id, TEntity entity)` | 删除前处理（关联检查等） |
| `Task AfterDeleteAsync(long id, TEntity entity)` | 删除后处理 |
| `Task BeforeBatchDeleteAsync(List<long> ids)` | 批量删除前处理 |
| `Task AfterBatchDeleteAsync(List<long> ids)` | 批量删除后处理 |

**示例：重写增删改钩子**
```csharp
public class MaterialService : CrudService<Material>
{
    protected override async Task BeforeCreateAsync(Material entity)
    {
        // 检查编码唯一性
        var exists = await _repository.GetAsync(x => x.Code == entity.Code);
        if (exists != null)
            throw new BusinessException("物料编码已存在");
    }

    protected override async Task BeforeDeleteAsync(long id, Material entity)
    {
        // 检查是否被引用
        var inUse = await _db.Queryable<Stock>().AnyAsync(s => s.MaterialId == id);
        if (inUse)
            throw new BusinessException("该物料存在库存记录，无法删除");
    }
}
```

###### 导入导出钩子

| 方法签名 | 说明 |
|---------|------|
| `Task BeforeExportAsync(AdvancedQueryRequestDto query)` | 导出前处理 |
| `List<TEntity> TransformExportData(List<TEntity> exportData)` | 对导出数据做转换（无列配置时） |
| `List<Dictionary<string, object?>> TransformToExportRows(List<TEntity> items, List<ExportColumnDto> columns)` | 将实体列表按列配置转换为 Dictionary 行列表（中文表头 + ExtData 展开 + 字典值翻译） |
| `string GetExportFileName()` | 获取导出文件的工作表名称（默认返回 `typeof(TEntity).Name`） |
| `Task<List<TEntity>> BeforeImportAsync(List<TEntity> rows)` | 导入前处理（数据清洗、校验等） |
| `Task AfterImportAsync(int successCount, List<string> errors)` | 导入后处理（记录导入日志等） |

###### 审计与属性复制钩子

| 方法签名 | 说明 |
|---------|------|
| `void FillAuditFields(TEntity entity, bool isCreate)` | 填充审计字段（`CreatedTime` / `LastModifiedTime`）。子类可重写以填充 `CreatedBy`、`LastModifiedBy` 等用户上下文字段 |
| `void CopyProperties(TEntity source, TEntity target)` | 将源实体属性复制到目标实体（跳过主键和所有级联导航属性） |
| `void CopyDetailProperties(TEntity source, TEntity target)` | 将源实体的级联导航属性（OneToMany + OneToOne）复制到目标实体 |

###### 排序钩子

| 方法签名 | 说明 |
|---------|------|
| `ISugarQueryable<TEntity> ApplySorting(ISugarQueryable<TEntity> queryable, List<SortCondition>? sortConditions)` | 应用多字段排序（自动过滤无效字段）。无有效排序时调用 `ApplyDefaultSorting` |
| `ISugarQueryable<T> ApplySorting<T>(ISugarQueryable<T> queryable, List<SortCondition>? sortConditions)` | 泛型重载 |
| `ISugarQueryable<TEntity> ApplyDefaultSorting(ISugarQueryable<TEntity> queryable)` | 默认排序（默认按 `CreatedTime ASC`，子类可重写为按 `CreatedTime DESC` 等） |
| `ISugarQueryable<T> ApplyDefaultSorting<T>(ISugarQueryable<T> queryable)` | 泛型重载 |

**示例：重写排序钩子**
```csharp
protected override ISugarQueryable<Order> ApplyDefaultSorting(ISugarQueryable<Order> queryable)
{
    return queryable.OrderBy(o => o.CreatedTime, OrderByType.Desc);
}
```

---

#### 3.1.3 IDomainService / DomainService + ValidationResult

**命名空间**: `KH.WMS.Core.Services`

**说明**: 领域服务基类，提供日志记录能力。所有领域服务应继承此类。

```csharp
public abstract class DomainService : IDomainService
{
    protected readonly ILogger _logger;

    protected DomainService(ILogger logger)
    {
        _logger = logger;
    }
}
```

**构造函数参数**:

| 参数名 | 类型 | 说明 |
|--------|------|------|
| `logger` | `ILogger` | Microsoft.Extensions.Logging 日志实例 |

**使用示例**:
```csharp
public class InventoryDomainService : DomainService
{
    public InventoryDomainService(ILogger<InventoryDomainService> logger) : base(logger)
    {
    }

    public async Task CheckStockAsync(long materialId)
    {
        _logger.LogInformation("Checking stock for material {MaterialId}", materialId);
        // 业务逻辑...
    }
}
```

---

##### ValidationResult

**说明**: 业务逻辑验证结果类，用于替代 `(bool, string)` 元组模式。

**属性**:

| 属性名 | 类型 | 说明 |
|--------|------|------|
| `IsValid` | `bool` | 是否通过验证（默认 `true`） |
| `Errors` | `List<string>` | 错误消息列表 |

**静态方法**:

| 方法签名 | 说明 |
|---------|------|
| `ValidationResult Success()` | 创建验证通过的结果 |
| `ValidationResult Fail(string error)` | 创建包含单条错误的验证失败结果 |
| `ValidationResult Fail(List<string> errors)` | 创建包含多条错误的验证失败结果 |

**实例方法**:

| 方法签名 | 说明 |
|---------|------|
| `void AddError(string error)` | 追加一条错误消息，同时将 `IsValid` 置为 `false` |

**使用示例**:
```csharp
public ValidationResult ValidateOrder(Order order)
{
    var result = ValidationResult.Success();

    if (string.IsNullOrWhiteSpace(order.OrderNo))
        result.AddError("订单号不能为空");

    if (order.TotalAmount <= 0)
        return ValidationResult.Fail("订单金额必须大于零");

    var errors = new List<string>();
    if (order.Lines == null || order.Lines.Count == 0)
        errors.Add("订单明细不能为空");
    if (errors.Count > 0)
        return ValidationResult.Fail(errors);

    return result; // IsValid == true
}
```

---

#### 3.1.4 IDetailSaveService / DetailSaveService

**命名空间**: `KH.WMS.Core.Services`

**说明**: 主从表保存服务，自动扫描实体上的 `[Navigate(NavigateType.OneToMany)]` 和 `[Navigate(NavigateType.OneToOne)]` 导航属性，完成从表的增删改操作。

---

##### `Task SaveDetailsAsync<TEntity>(TEntity entity, bool isCreate) where TEntity : BaseEntity<long>, new()`

**说明**: 保存主实体的 **OneToMany** 从表数据（增量比对：新增/修改/删除）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `entity` | `TEntity` | 是 | 主实体实例（需已持久化，包含有效 Id） |
| `isCreate` | `bool` | 是 | 是否为新增场景（新增时跳过删除检测） |

**内部逻辑**:
1. 扫描实体上标记了 `[Navigate(NavigateType.OneToMany)]` 的 `List<T>` 属性
2. 遍历每个从表集合：
   - `Id == 0` → 新增，设置外键和 `CreatedTime`
   - `Id > 0` → 更新，设置 `LastModifiedTime`
3. 非新增场景时，删除已提交集合中不存在的记录

---

##### `Task SaveOneToOneAsync<TEntity>(TEntity entity, bool isCreate) where TEntity : BaseEntity<long>, new()`

**说明**: 保存主实体的 **OneToOne** 导航属性（新增/更新单一子实体）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `entity` | `TEntity` | 是 | 主实体实例（需已持久化，包含有效 Id） |
| `isCreate` | `bool` | 是 | 是否为新增场景 |

**内部逻辑**:
1. 扫描实体上标记了 `[Navigate(NavigateType.OneToOne)]` 的属性
2. 设置外键值
3. `childId == 0` → 插入；否则 → 更新

**使用示例**:
```csharp
// 通常在 CrudService 内部自动调用，无需手动调用
// 需在实体上配置导航特性：
public class PurchaseOrder : BaseEntity<long>
{
    [Navigate(NavigateType.OneToMany)]
    public List<PurchaseOrderLine> Lines { get; set; }

    [Navigate(NavigateType.OneToOne)]
    public PurchaseOrderExt ExtInfo { get; set; }
}
```

---

#### 3.1.5 ICodeGeneratorService / CodeGeneratorService

**命名空间**: `KH.WMS.Core.Services`

**说明**: 编码生成服务，根据 `cfg_code_rule` 编码规则表和 `cfg_code_sequence` 序列表生成各类业务编码。通过原生 SQL 访问（行锁保证并发安全）。

---

##### `Task<string> GenerateAsync(string ruleType)`

**说明**: 根据规则类型生成业务编码

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `ruleType` | `string` | 是 | 规则类型标识，例如 `INBOUND_DOC`、`OUTBOUND_DOC`、`MATERIAL` 等 |

**返回值**: `Task<string>` — 生成的编码字符串

**异常**:
- `InvalidOperationException` — 未找到有效的编码规则时抛出

**内部逻辑**:
1. 查找活跃的编码规则（行锁 `UPDLOCK`，按 `EffectiveDate` / `ExpiryDate` 过滤）
2. 根据 `SequenceType`（DAILY / MONTHLY / YEARLY）生成序列键
3. 获取或创建序列记录（过期时自动重置）
4. 自增序列值并持久化
5. 按规则拼装编码：`{Prefix}{Separator}{DateFormat}{Separator}{序列号填充}`

**使用示例**:
```csharp
public class InboundService : ApplicationService
{
    private readonly ICodeGeneratorService _codeGenerator;

    public InboundService(IMappingService mappingService, ICodeGeneratorService codeGenerator)
        : base(mappingService)
    {
        _codeGenerator = codeGenerator;
    }

    public async Task<string> CreateInboundOrderAsync()
    {
        var orderNo = await _codeGenerator.GenerateAsync("INBOUND_DOC");
        // orderNo 示例: "PO-20250101-0001"
        return orderNo;
    }
}
```

**支持的规则类型**（以数据库 `cfg_code_rule.RuleType` 配置为准，常见示例）:

| 规则类型 | 说明 |
|---------|------|
| `INBOUND_DOC` | 入库单号 |
| `OUTBOUND_DOC` | 出库单号 |
| `MATERIAL` | 物料编码 |
| `TRANSFER_ORDER` | 调拨单号 |
| `INVENTORY_CHECK` | 盘点单号 |

---

#### 3.1.6 ServiceBase

> **注意**: 以下为旧版兼容基类，位于命名空间 `WIDESEAWCS_Core.BaseServices`。新项目建议使用 `CrudService<TEntity>`。

**说明**: 旧版通用服务基类，提供基础的增删改查、分页、导入导出功能。

**泛型约束**: `TEntity : class, new()`，`TRepository : IRepository<TEntity>`

**构造函数参数**:

| 参数名 | 类型 | 说明 |
|--------|------|------|
| `BaseDal` | `TRepository` | 实体仓储实例 |

**公共属性**:

| 属性名 | 类型 | 说明 |
|--------|------|------|
| `BaseDal` | `TRepository` | 实体仓储 |
| `Db` | `ISqlSugarClient` | SqlSugar 数据库客户端 |
| `Repository` | `IRepository<TEntity>` | 实体仓储（与 BaseDal 相同） |

**公共方法**:

| 方法签名 | 说明 |
|---------|------|
| `PageGridData<TEntity> GetPageData(PageDataOptions options)` | 分页查询（含过滤条件、排序） |
| `WebResponseContent UpdateData(SaveModel saveModel)` | 新增或更新数据（含主从表） |
| `WebResponseContent DeleteData(object key)` | 根据主键删除 |
| `WebResponseContent DeleteData(object[] keys)` | 根据主键数组批量删除 |
| `WebResponseContent DeleteData(TEntity entity)` | 删除实体 |
| `WebResponseContent DeleteData(List<TEntity> entities)` | 批量删除实体集合 |
| `WebResponseContent Export(PageDataOptions options)` | 导出 Excel |
| `WebResponseContent Import(List<IFormFile> files)` | 从 Excel 导入 |
| `WebResponseContent Upload(List<IFormFile> files)` | 上传文件（未实现） |
| `WebResponseContent DownLoadTemplate()` | 下载导入模板 |
| `WebResponseContent ExportSeedData()` | 导出种子数据为 JSON |

**辅助类型**:

| 类型 | 说明 |
|------|------|
| `PageDataOptions` | 分页查询参数（`Page`、`Rows`、`Sort`、`Order`、`Filter`、`Wheres` 等） |
| `SearchParameters` | 查询条件参数（`Name`、`Value`、`DisplayType`） |
| `SaveModel` | 保存模型（`MainData`、`DetailData`、`DelKeys`、`Extra`） |

---

#### 3.1.7 ServiceFunFilter

**命名空间**: `WIDESEAWCS_Core.BaseServices`

**说明**: 旧版服务功能过滤器基类，提供声明式的钩子委托属性用于业务定制。`ServiceBase<TEntity, TRepository>` 继承自此基类。子类通过赋值委托属性而非重写方法来实现业务逻辑注入。

**泛型约束**: `T : class`

**受保护属性（可被子类赋值）**:

| 属性名 | 类型 | 说明 |
|--------|------|------|
| `IsMultiTenancy` | `bool` | 是否开启多租户功能 |
| `SummaryExpress` | `Func<IQueryable<T>, object>?` | 查询表格统计表达式（求和、平均值等） |
| `LimitCurrentUserPermission` | `bool` | 是否开启用户数据权限（默认 `false`；开启后用户只能操作自己及下级角色创建的数据） |
| `Limit` | `int` | 默认导出最大行数（0 不限制） |
| `LimitUpFileSizee` | `int` | 上传文件大小限制，单位 MB（默认 3） |
| `QuerySql` | `string?` | 自定义原生查询 SQL（SELECT 必须返回表所有列） |
| `QueryRelativeList` | `Action<List<SearchParameters>>?` | 查询前对搜索条件进行增删 |
| `QueryRelativeExpression` | `Func<IQueryable<T>, IQueryable<T>>?` | 查询前通过表达式修改查询条件 |
| `Columns` | `Expression<Func<T, object>>?` | 指定查询列 |
| `OrderByExpression` | `Expression<Func<T, Dictionary<object, OrderByType>>>?` | 设置查询排序参数及方式 |
| `OrderByParameters` | `Dictionary<string, OrderByType>?` | 排序参数字典 |
| `EnableWebOrderBy` | `bool` | 是否启用 Web 端排序（默认 `true`） |
| `TableName` | `string?` | 设置查询的表名（已弃用） |
| `GetPageDataOnExecuted` | `Action<PageGridData<T>>?` | 分页查询后处理 |
| `AddOnExecute` | `Func<SaveModel, WebResponseContent>?` | 新增前处理（原始 SaveModel 数据） |
| `AddOnExecuting` | `Func<T, object, WebResponseContent>?` | 新增保存数据库前处理 |
| `AddOnExecuted` | `Func<T, object, WebResponseContent>?` | 新增保存数据库后处理（已开启事务） |
| `AddWorkFlowExecuting` | `Func<T, bool>?` | 进入审批流程之前 |
| `AddWorkFlowExecuted` | `Action<T, List<int>>?` | 写入审批流程数据之后 |
| `UpdateOnExecute` | `Func<SaveModel, WebResponseContent>?` | 更新前处理（原始 SaveModel 数据） |
| `UpdateIgnoreColOnExecute` | `Func<SaveModel, List<string>>?` | 更新时忽略的字段列表 |
| `UpdateOnExecuting` | `Func<T, object, object, List<object>, WebResponseContent>?` | 更新保存数据库前处理 |
| `UpdateOnExecuted` | `Func<T, object, object, List<object>, WebResponseContent>?` | 更新保存数据库后处理 |
| `DelOnExecuting` | `Func<object[], WebResponseContent>?` | 删除前处理 |
| `DelOnExecuted` | `Func<object[], WebResponseContent>?` | 删除后处理 |
| `AuditOnExecuting` | `Func<List<T>, WebResponseContent>?` | 审核前处理 |
| `AuditOnExecuted` | `Func<List<T>, WebResponseContent>?` | 审核后处理 |
| `ExportOnExecuting` | `Func<List<T>, List<string>, WebResponseContent>?` | 导出前处理 |
| `ExportColumns` | `Expression<Func<T, object>>?` | 导出时指定列 |
| `DownLoadTemplateColumns` | `Expression<Func<T, object>>?` | 下载模板时指定列 |
| `ImportOnExecuted` | `Func<List<T>, WebResponseContent>?` | 导入后处理 |
| `ImportOnExecuting` | `Func<List<T>, WebResponseContent>?` | 导入前处理 |
| `ImportIgnoreSelectValidationColumns` | `Expression<Func<T, object>>?` | 导入时不验证下拉框数据源的字段 |
| `UploadFolder` | `string?` | 自定义上传文件夹 |

**使用示例**:
```csharp
public class OrderService : ServiceBase<Order, IRepository<Order>>
{
    public OrderService(IRepository<Order> baseDal) : base(baseDal)
    {
        // 通过委托注入业务逻辑，无需重写方法
        AddOnExecuted = (order, details) =>
        {
            // 订单创建后的额外处理
            _logger.LogInformation("Order {OrderNo} created", order.OrderNo);
            return WebResponseContent.Instance.OK();
        };

        DelOnExecuting = (keys) =>
        {
            // 删除前的校验
            return WebResponseContent.Instance.OK();
        };
    }
}
```

### 3.2 Controllers 控制器

#### 3.2.1 CrudController\<TEntity\>

**命名空间**: `KH.WMS.Core.Controllers`

**说明**: CRUD 控制器基类，自动提供标准 CRUD + 导入导出 HTTP 端点。子类只需继承并注入对应的 `ICrudService<TEntity>` 即可获得完整的 RESTful API。

**泛型约束**: `where TEntity : BaseEntity<long>, new()`

**基类**: `ControllerBase`

**特性**: `[ApiController]`

**构造函数**:

| 参数名 | 类型 | 说明 |
|--------|------|------|
| `service` | `ICrudService<TEntity>` | CRUD 服务实例 |

---

##### HTTP 端点

###### `GET /{id}` — `GetById`

**说明**: 根据 ID 获取实体详情

**路由**: `[HttpGet("{id}")]`

**特性**: `[Cache(Enable = false)]`

**参数**:

| 参数名 | 类型 | 来源 | 必填 | 说明 |
|--------|------|------|------|------|
| `id` | `long` | 路径参数 | 是 | 实体主键 ID |

**返回值**: `Task<ApiResponse>`

- 成功时: `Data` 为实体对象
- 失败时: Code=404

**内部调用链**: `_service.GetByIdAsync(id)`

**使用示例**:
```csharp
// GET /api/product/1
var result = await controller.GetById(1);
```

---

###### `POST /pagelist` — `GetPagedList`

**说明**: 分页查询

**路由**: `[HttpPost("pagelist")]`

**特性**: `[Cache(Enable = false)]`

**参数**:

| 参数名 | 类型 | 来源 | 必填 | 说明 |
|--------|------|------|------|------|
| `query` | `AdvancedQueryRequestDto` | Body (JSON) | 是 | 高级查询请求，包含 `Keyword`、`PageIndex`、`PageSize`、`SortConditions`、`Filters` |

**AdvancedQueryRequestDto 属性**:

| 属性名 | 类型 | 说明 |
|--------|------|------|
| `PageIndex` | `int` | 页码，从 1 开始（默认 1） |
| `PageSize` | `int` | 每页数量（默认 20） |
| `Keyword` | `string?` | 搜索关键字 |
| `SortConditions` | `List<SortCondition>?` | 排序条件列表（`Field` + `Direction`） |
| `Filters` | `List<FilterCondition>?` | 过滤条件列表（`Field` + `Operator` + `Value`） |

**返回值**: `Task<ApiResponse>`

- 成功时: `Data` 为 `PagedResult<TEntity>`，包含 `Items`、`Total`、`PageIndex`、`PageSize`、`PageCount`

**内部调用链**: `_service.GetPagedListAsync(query)`

**使用示例**:
```csharp
// POST /api/product/pagelist
var query = new AdvancedQueryRequestDto
{
    PageIndex = 1,
    PageSize = 20,
    Keyword = "test",
    SortConditions = new List<SortCondition>
    {
        new() { Field = "CreatedTime", Direction = "desc" }
    }
};
var result = await controller.GetPagedList(query);
```

---

###### `GET /all` — `GetAll`

**说明**: 获取全部列表（用于下拉选择等场景）

**路由**: `[HttpGet("all")]`

**参数**: 无

**返回值**: `Task<ApiResponse>`

- 成功时: `Data` 为 `List<TEntity>`

**内部调用链**: `_service.GetListAsync()`

**使用示例**:
```csharp
// GET /api/product/all
var result = await controller.GetAll();
```

---

###### `POST /create` — `Create`

**说明**: 新增实体

**路由**: `[HttpPost("create")]`

**特性**: `[Cache(Enable = false)]`

**参数**:

| 参数名 | 类型 | 来源 | 必填 | 说明 |
|--------|------|------|------|------|
| `entity` | `TEntity` | Body (JSON) | 是 | 实体对象（无需传递 ID） |

**返回值**: `Task<ApiResponse>`

- 成功时: `Data` 为新增后的实体（含生成的主键 ID），Code=200
- 失败时: Code=400（校验失败）

**内部调用链**: `_service.CreateAsync(entity)`

**使用示例**:
```csharp
// POST /api/product/create
var entity = new Product { Name = "测试商品", Code = "P001" };
var result = await controller.Create(entity);
```

---

###### `POST /update` — `Update`

**说明**: 更新实体

**路由**: `[HttpPost("update")]`

**特性**: `[Cache(Enable = false)]`

**参数**:

| 参数名 | 类型 | 来源 | 必填 | 说明 |
|--------|------|------|------|------|
| `entity` | `TEntity` | Body (JSON) | 是 | 实体对象（需包含主键 ID） |

**返回值**: `Task<ApiResponse>`

- 成功时: Code=200
- 失败时: Code=404（实体不存在）

**内部调用链**: `_service.UpdateAsync(entity)`

**使用示例**:
```csharp
// POST /api/product/update
var entity = new Product { Id = 1, Name = "更新后的商品名" };
var result = await controller.Update(entity);
```

---

###### `DELETE /delete/{id}` — `Delete`

**说明**: 根据 ID 删除实体

**路由**: `[HttpDelete("delete/{id}")]`

**特性**: `[Cache(Enable = false)]`

**参数**:

| 参数名 | 类型 | 来源 | 必填 | 说明 |
|--------|------|------|------|------|
| `id` | `long` | 路径参数 | 是 | 要删除的实体主键 ID |

**返回值**: `Task<ApiResponse>`

- 成功时: Code=200
- 失败时: Code=404（实体不存在）

**内部调用链**: `_service.DeleteAsync(id)`

**使用示例**:
```csharp
// DELETE /api/product/delete/1
var result = await controller.Delete(1);
```

---

###### `DELETE /batch` — `BatchDelete`

**说明**: 批量删除实体

**路由**: `[HttpDelete("batch")]`

**特性**: `[Cache(Enable = false)]`

**参数**:

| 参数名 | 类型 | 来源 | 必填 | 说明 |
|--------|------|------|------|------|
| `ids` | `List<long>` | Body (JSON) | 是 | 要删除的实体主键 ID 列表 |

**返回值**: `Task<ApiResponse>`

- 成功时: Code=200
- ids 为空时: Code=400

**内部调用链**: `_service.BatchDeleteAsync(ids)`

**使用示例**:
```csharp
// DELETE /api/product/batch
var ids = new List<long> { 1, 2, 3 };
var result = await controller.BatchDelete(ids);
```

---

###### `POST /export` — `Export`

**说明**: 导出数据到 Excel

**路由**: `[HttpPost("export")]`

**特性**: `[Cache(Enable = false)]`

**参数**:

| 参数名 | 类型 | 来源 | 必填 | 说明 |
|--------|------|------|------|------|
| `request` | `ExportRequestDto` | Body (JSON) | 是 | 导出请求（继承 `AdvancedQueryRequestDto`，额外包含 `Columns` 和 `ExportAll`） |

**ExportRequestDto 额外属性**:

| 属性名 | 类型 | 说明 |
|--------|------|------|
| `Columns` | `List<ExportColumnDto>?` | 导出列配置（`Prop` 字段名、`Label` 中文标题、`DictMap` 字典映射），为空时直接导出实体 |
| `ExportAll` | `bool` | 是否导出全部数据（true 跳过分页、false 仅导出当前页，默认 true） |

**返回值**: `Task<ApiResponse>`

- 成功时: `Data` 为 Base64 编码的 Excel 文件字节流

**内部调用链**: `_service.ExportAsync(request, request.Columns, request.ExportAll)`

**使用示例**:
```csharp
// POST /api/product/export
var request = new ExportRequestDto
{
    PageIndex = 1,
    PageSize = 20,
    ExportAll = true,
    Columns = new List<ExportColumnDto>
    {
        new() { Prop = "Name", Label = "商品名称" },
        new() { Prop = "Code", Label = "商品编码" }
    }
};
var result = await controller.Export(request);
```

---

###### `POST /import` — `Import`

**说明**: 从 Excel 文件导入数据

**路由**: `[HttpPost("import")]`

**特性**: `[Cache(Enable = false)]`

**参数**:

| 参数名 | 类型 | 来源 | 必填 | 说明 |
|--------|------|------|------|------|
| `file` | `IFormFile` | Form Data | 是 | Excel 文件（多部分表单上传） |

**返回值**: `Task<ApiResponse>`

- 成功时: Code=200
- 文件为空时: Code=400, Message="请选择要导入的文件"

**内部调用链**: `file.OpenReadStream()` → `_service.ImportAsync(stream, file.FileName)`

**使用示例**:
```csharp
// POST /api/product/import (multipart/form-data)
// 前端使用 FormData 上传文件
var file = Request.Form.Files[0];
var result = await controller.Import(file);
```

---

###### `GET /template` — `DownloadTemplate`

**说明**: 下载导入模板 Excel 文件

**路由**: `[HttpGet("template")]`

**特性**: `[Cache(Enable = false)]`

**参数**: 无

**返回值**: `Task<ApiResponse>`

- 成功时: `Data` 为 Base64 编码的 Excel 模板文件字节流

**内部调用链**: `_service.DownloadTemplateAsync()`

**使用示例**:
```csharp
// GET /api/product/template
var result = await controller.DownloadTemplate();
var base64 = result.Data as string;
```

---

#### 3.2.2 ExtDataCrudController\<TEntity\>

**命名空间**: `KH.WMS.Core.Controllers`

**说明**: 支持 ExtData 扩展字段的 CRUD 控制器基类。继承自 `CrudController<TEntity>`，在 `Create`/`Update` 时从请求体中提取 `extDataRaw` 字段并写入实体的 `ExtData` 属性，在 `GetById` 时将 `ExtData` JSON 反序列化展开为扁平属性。

**泛型约束**: `where TEntity : BaseEntity<long>, new()`（实体需包含 `string? ExtData` 属性）

**基类**: `CrudController<TEntity>`

**构造函数**:

| 参数名 | 类型 | 说明 |
|--------|------|------|
| `service` | `ICrudService<TEntity>` | CRUD 服务实例 |

---

##### ExtData JSON 展平机制

ExtDataCrudController 通过以下两个核心操作实现扩展字段的读写：

**1. 写入时 — `ExtractExtDataFromRequest`**

从 HTTP 请求 Body 的原始 JSON 中查找 `extDataRaw` 字段，将其值（JSON 字符串）直接赋给实体的 `ExtData` 属性。该操作依赖 `Program.cs` 中配置的 `EnableBuffering` 中间件，使 Body 流可回溯重读。

```csharp
// 请求体示例
{
    "Name": "商品名称",
    "Code": "P001",
    "extDataRaw": "{\"Color\":\"红色\",\"Size\":\"XL\",\"Weight\":\"1.5kg\"}"
}
```

**2. 读取时 — `FlattenExtData`**

将 `ExtData` JSON 字符串解析为 `JsonObject`，将其中的键值对逐个合并到实体的 JSON 序列化结果中（不覆盖实体已有属性），从而实现前端编辑/查看时的扁平化回显。

---

##### 重写的端点

###### `POST /create` — `Create`

**说明**: 新增实体，从请求体中提取 `extDataRaw` 字段写入实体的 `ExtData`

**路由**: `[HttpPost("create")]`

**参数**:

| 参数名 | 类型 | 来源 | 必填 | 说明 |
|--------|------|------|------|------|
| `entity` | `TEntity` | Body (JSON) | 是 | 实体对象，JSON 中可包含 `extDataRaw` 字段 |

**内部逻辑**:
1. 读取 HTTP 请求 Body 原始 JSON
2. 提取 `extDataRaw` 字段值，写入 `entity.ExtData`
3. 调用 `service.CreateAsync(entity)`

---

###### `POST /update` — `Update`

**说明**: 更新实体，同新增逻辑提取 `extDataRaw` 写入 `ExtData`

**路由**: `[HttpPost("update")]`

**参数**:

| 参数名 | 类型 | 来源 | 必填 | 说明 |
|--------|------|------|------|------|
| `entity` | `TEntity` | Body (JSON) | 是 | 实体对象，JSON 中可包含 `extDataRaw` 字段 |

**内部逻辑**:
1. 读取 HTTP 请求 Body 原始 JSON
2. 提取 `extDataRaw` 字段值，写入 `entity.ExtData`
3. 调用 `service.UpdateAsync(entity)`

---

###### `GET /{id}` — `GetById`

**说明**: 根据 ID 获取详情，将 `ExtData` JSON 反序列化展开为扁平属性返回给前端

**路由**: `[HttpGet("{id}")]`

**参数**:

| 参数名 | 类型 | 来源 | 必填 | 说明 |
|--------|------|------|------|------|
| `id` | `long` | 路径参数 | 是 | 实体主键 ID |

**返回值**: `Task<ApiResponse>`

- 成功时: `Data` 为 `JsonObject`，包含实体属性 + ExtData 展开的扁平属性
- 不覆盖实体已有属性，`ExtData` 中的键与实体属性重名时以实体属性为准

**内部逻辑**:
1. 调用 `service.GetByIdAsync(id)`
2. 将结果实体序列化为 `JsonObject`
3. 解析 `ExtData` JSON 并合并到该 `JsonObject`
4. 返回合并后的 `JsonObject` 作为 `ApiResponse.Data`

**使用示例**:
```csharp
// 请求 Body:
// { "Name": "商品", "extDataRaw": "{\"Color\":\"红色\"}" }

// 数据库存储:
// ExtData = "{\"Color\":\"红色\"}"

// GET /api/product/1 返回:
// {
//   "Name": "商品",
//   "Code": "P001",
//   "Color": "红色"   ← ExtData 展开的扁平属性
// }
```

---

### 3.3 Database 数据库

#### 3.3.1 IDbContext

**命名空间**: `KH.WMS.Core.Database`

**说明**: 数据库上下文接口，提供 SqlSugar 客户端访问、事务管理和仓储获取能力。

**继承**: `IDisposable`

---

##### 属性

| 属性名 | 类型 | 访问级别 | 说明 |
|--------|------|----------|------|
| `Db` | `ISqlSugarClient` | get | 数据库操作对象（SqlSugar 客户端），可直接使用 SqlSugar 的全部功能 |
| `HasActiveTransaction` | `bool` | get | 是否正在活动事务中 |
| `TransactionDepth` | `int` | get | 事务嵌套深度（层数） |
| `CurrentIsolationLevel` | `System.Data.IsolationLevel?` | get | 当前事务隔离级别，无活动事务时为 `null` |

---

##### 方法

###### `Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)`

**说明**: 开始事务

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `isolationLevel` | `System.Data.IsolationLevel` | 否 | 事务隔离级别，默认 `ReadCommitted` |

---

###### `Task CommitTransactionAsync()`

**说明**: 提交事务

---

###### `Task RollbackTransactionAsync()`

**说明**: 回滚事务

---

###### `IRepository<T, TKey> GetRepository<T, TKey>()`

**说明**: 获取泛型仓储实例

**泛型约束**:

| 参数 | 约束 |
|------|------|
| `T` | `class, new()` |
| `TKey` | `struct` |

**使用示例**:
```csharp
var repo = dbContext.GetRepository<Product, long>();
var product = await repo.GetByIdAsync(1);
```

---

#### 3.3.2 IRepository\<T, TKey\> / RepositoryBase\<T, TKey\>

**命名空间**: `KH.WMS.Core.Database.Repositories`

**说明**: 仓储模式接口与实现，封装 SqlSugar 的常用数据库操作，提供泛型 CRUD、导航属性操作和复杂查询能力。

**IRepository 泛型约束**:

| 参数 | 约束 |
|------|------|
| `T` | `class` |
| `TKey` | `struct` |

**RepositoryBase 特性**: `[RegisteredService]`，带日志拦截器 `[LogInterceptor(LogParameters = true, LogReturnValue = true)]`

**构造函数**:

| 参数名 | 类型 | 说明 |
|--------|------|------|
| `db` | `ISqlSugarClient` | SqlSugar 客户端实例 |
| `logger` | `ILogger?` | 可选的日志记录器 |

---

##### 基础 CRUD 方法

###### `Task<T?> GetByIdAsync(TKey id)`

**说明**: 根据 ID 获取实体

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `id` | `TKey` | 是 | 主键 ID |

**返回值**: `Task<T?>` — 实体对象，未找到时返回 `null`

---

###### `Task<List<T>> GetAllAsync()`

**说明**: 获取所有实体

**返回值**: `Task<List<T>>`

---

###### `Task<List<T>> GetListAsync(Expression<Func<T, bool>> expression)`

**说明**: 根据条件查询实体列表

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `expression` | `Expression<Func<T, bool>>` | 是 | Lambda 查询条件，如 `x => x.IsDeleted == false` |

**返回值**: `Task<List<T>>`

---

###### `Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> expression)`

**说明**: 根据条件查询单个实体

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `expression` | `Expression<Func<T, bool>>` | 是 | Lambda 查询条件 |

**返回值**: `Task<T?>` — 实体对象，未找到时返回 `null`

---

###### `Task<(List<T> Items, int Total)> GetPagedListAsync(int pageIndex, int pageSize, Expression<Func<T, bool>>? expression = null)`

**说明**: 分页查询

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `pageIndex` | `int` | 是 | 页码（从 1 开始） |
| `pageSize` | `int` | 是 | 每页数量 |
| `expression` | `Expression<Func<T, bool>>?` | 否 | 可选查询条件 |

**返回值**: `Task<(List<T> Items, int Total)>` — 包含当前页数据列表和总记录数

---

###### `Task<TKey> AddAsync(T entity)`

**说明**: 插入单个实体并返回主键

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `entity` | `T` | 是 | 要插入的实体 |

**返回值**: `Task<TKey>` — 生成的主键值

---

###### `Task<List<TKey>> AddAsync(List<T> entities)`

**说明**: 批量插入实体并返回主键列表

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `entities` | `List<T>` | 是 | 要插入的实体列表 |

**返回值**: `Task<List<TKey>>` — 生成的主键值列表

---

###### `Task<T> AddReturnEntityAsync(T entity)`

**说明**: 插入单个实体并返回包含数据库默认值的完整实体

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `entity` | `T` | 是 | 要插入的实体 |

**返回值**: `Task<T>` — 插入后的完整实体

---

###### `Task<List<T>> AddReturnEntityAsync(List<T> entities)`

**说明**: 批量插入实体并返回包含数据库默认值的完整实体列表

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `entities` | `List<T>` | 是 | 要插入的实体列表 |

**返回值**: `Task<List<T>>` — 插入后的完整实体列表

---

###### `Task<bool> UpdateAsync(T entity)`

**说明**: 更新单个实体

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `entity` | `T` | 是 | 要更新的实体 |

**返回值**: `Task<bool>` — 是否成功更新（影响行数 > 0）

---

###### `Task<bool> UpdateAsync(List<T> entities)`

**说明**: 批量更新实体

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `entities` | `List<T>` | 是 | 要更新的实体列表 |

**返回值**: `Task<bool>` — 是否成功更新

---

###### `Task<bool> DeleteAsync(TKey id)`

**说明**: 根据 ID 删除实体

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `id` | `TKey` | 是 | 要删除的实体主键 ID |

**返回值**: `Task<bool>` — 是否成功删除

---

###### `Task<bool> DeleteAsync(List<TKey> ids)`

**说明**: 批量删除实体（按 ID 列表）

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `ids` | `List<TKey>` | 是 | 要删除的实体主键 ID 列表 |

**返回值**: `Task<bool>` — 是否成功删除

---

###### `Task<bool> DeleteAsync(Expression<Func<T, bool>> expression)`

**说明**: 根据条件删除实体

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `expression` | `Expression<Func<T, bool>>` | 是 | Lambda 删除条件 |

**返回值**: `Task<bool>` — 是否成功删除

---

##### 查询/聚合方法

###### `Task<bool> ExistsAsync(Expression<Func<T, bool>> expression)`

**说明**: 检查是否存在符合条件的实体

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `expression` | `Expression<Func<T, bool>>` | 是 | Lambda 查询条件 |

**返回值**: `Task<bool>` — 存在返回 `true`

---

###### `Task<int> CountAsync(Expression<Func<T, bool>>? expression = null)`

**说明**: 获取符合条件的实体数量

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `expression` | `Expression<Func<T, bool>>?` | 否 | 可选计数条件，为 `null` 时统计全部 |

**返回值**: `Task<int>`

---

##### 导航属性方法（主从表操作）

###### `Task<bool> AddWithNavAsync(T entity)`

**说明**: 导航插入（主表 + 从表一起插入，自动处理第一层导航属性）

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `entity` | `T` | 是 | 包含导航属性数据的主实体 |

**返回值**: `Task<bool>`

---

###### `Task<bool> UpdateWithNavAsync(T entity)`

**说明**: 导航更新（主表 + 从表一起更新，自动处理从表的增删改）

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `entity` | `T` | 是 | 包含导航属性数据的主实体 |

**返回值**: `Task<bool>`

---

###### `Task<bool> DeleteWithNavAsync(T entity)`

**说明**: 导航删除（主表 + 从表一起删除）

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `entity` | `T` | 是 | 要删除的主实体 |

**返回值**: `Task<bool>`

---

###### `Task<T?> GetByIdWithNavAsync(TKey id)`

**说明**: 根据 ID 获取实体（包含导航属性，主表 + 从表一起查询）

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `id` | `TKey` | 是 | 主键 ID |

**返回值**: `Task<T?>` — 包含导航属性数据的主实体

---

###### `Task<List<T>> GetAllWithNavAsync()`

**说明**: 获取所有实体（包含导航属性，主表 + 从表一起查询）

**返回值**: `Task<List<T>>`

---

###### `Task<List<T>> GetListWithNavAsync(Expression<Func<T, bool>> expression)`

**说明**: 根据条件查询实体列表（包含导航属性，主表 + 从表一起查询）

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `expression` | `Expression<Func<T, bool>>` | 是 | Lambda 查询条件 |

**返回值**: `Task<List<T>>`

---

###### `Task<T?> GetFirstOrDefaultWithNavAsync(Expression<Func<T, bool>> expression)`

**说明**: 根据条件查询单个实体（包含导航属性，主表 + 从表一起查询）

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `expression` | `Expression<Func<T, bool>>` | 是 | Lambda 查询条件 |

**返回值**: `Task<T?>` — 未找到时返回 `null`

---

###### `Task<List<T>> GetListByDetailAsync<TDetail>(Expression<Func<T, IEnumerable<TDetail>>> navProperty, Expression<Func<TDetail, bool>> detailFilter) where TDetail : class, new()`

**说明**: 根据从表条件查询主表（主表 + 从表一起查询）。通过 `Any()` 表达式树动态构建查询条件。

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `navProperty` | `Expression<Func<T, IEnumerable<TDetail>>>` | 是 | 导航属性表达式，如 `x => x.OrderLines` |
| `detailFilter` | `Expression<Func<TDetail, bool>>` | 是 | 从表过滤条件，如 `d => d.MaterialId == 123` |

**返回值**: `Task<List<T>>` — 满足从表条件的主表列表（包含从表数据）

**使用示例**:
```csharp
var orders = await repository.GetListByDetailAsync<InboundOrderLine>(
    x => x.OrderLines,
    d => d.MaterialId == 123
);
```

---

###### `Task<T?> GetFirstOrDefaultByDetailAsync<TDetail>(Expression<Func<T, IEnumerable<TDetail>>> navProperty, Expression<Func<TDetail, bool>> detailFilter) where TDetail : class, new()`

**说明**: 根据从表条件查询单个主表（主表 + 从表一起查询）

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `navProperty` | `Expression<Func<T, IEnumerable<TDetail>>>` | 是 | 导航属性表达式 |
| `detailFilter` | `Expression<Func<TDetail, bool>>` | 是 | 从表过滤条件 |

**返回值**: `Task<T?>` — 满足条件的单个主表

**使用示例**:
```csharp
var order = await repository.GetFirstOrDefaultByDetailAsync<InboundOrderLine>(
    x => x.OrderLines,
    d => d.MaterialCode == "M001"
);
```

---

##### 高级查询方法

###### `ISugarQueryable<T> AsQueryable()`

**说明**: 获取基础可查询对象 `ISugarQueryable<T>`，用于需要跨表 Join、聚合等仓储预定义方法无法覆盖的复杂查询场景

**返回值**: `ISugarQueryable<T>` — SqlSugar 可查询对象

**使用示例**:
```csharp
var query = repository.AsQueryable()
    .LeftJoin<Warehouse>((p, w) => p.WarehouseId == w.Id)
    .Select((p, w) => new { p.Name, w.Name })
    .ToListAsync();
```

---

#### 3.3.3 IUnitOfWork / UnitOfWork / UnitOfWorkExtensions

**命名空间**: `KH.WMS.Core.Database.UnitOfWorks`

**说明**: 工作单元模式，提供事务管理和仓储获取的统一入口。支持事务嵌套，内部委托给 `IDbContext` 实现。

**IUnitOfWork 继承**: `IDisposable`

**UnitOfWork 特性**: `[RegisteredService(Lifetime = Scoped, WithoutInterceptor = true)]`

**构造函数**:

| 参数名 | 类型 | 说明 |
|--------|------|------|
| `dbContext` | `IDbContext` | 数据库上下文实例 |
| `logger` | `ILogger<UnitOfWork>` | 日志记录器 |

---

##### 属性

| 属性名 | 类型 | 说明 |
|--------|------|------|
| `DbContext` | `IDbContext` | 获取数据库上下文 |
| `HasActiveTransaction` | `bool` | 是否在事务中 |
| `TransactionDepth` | `int` | 事务嵌套深度 |

---

##### 方法

###### `Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)`

**说明**: 开始事务（支持嵌套）

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `isolationLevel` | `System.Data.IsolationLevel` | 否 | 事务隔离级别，默认 `ReadCommitted` |

---

###### `Task CommitAsync()`

**说明**: 提交事务（嵌套事务仅标记完成，最外层才真正提交数据库事务）

---

###### `Task RollbackAsync()`

**说明**: 回滚事务（嵌套事务将最外层标记为需要回滚，最外层回滚时真正回滚）

---

###### `IRepository<T, TKey> GetRepository<T, TKey>() where T : class, new() where TKey : struct`

**说明**: 获取泛型仓储实例（委托给 `IDbContext.GetRepository<T, TKey>()`）

**泛型约束**:

| 参数 | 约束 |
|------|------|
| `T` | `class, new()` |
| `TKey` | `struct` |

---

###### `Task<int> ExecuteSqlAsync<TKey>(string sql, params object?[] parameters) where TKey : struct`

**说明**: 执行原始 SQL 命令

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `sql` | `string` | 是 | SQL 命令语句 |
| `parameters` | `params object?[]` | 否 | SQL 参数 |

**返回值**: `Task<int>` — 影响行数

---

###### `Task<List<T>> ExecuteSqlAsync<T, TKey>(string sql, params object?[] parameters) where T : class, new() where TKey : struct`

**说明**: 执行原始 SQL 查询并返回实体列表

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `sql` | `string` | 是 | SQL 查询语句 |
| `parameters` | `params object?[]` | 否 | SQL 参数 |

**返回值**: `Task<List<T>>`

---

##### UnitOfWorkExtensions（扩展方法）

**说明**: 为 `IUnitOfWork` 提供事务作用域和便捷的事务执行扩展方法。

###### `Task<ITransactionScope> BeginTransactionScopeAsync(this IUnitOfWork unitOfWork, ILogger logger, System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)`

**说明**: 创建事务作用域，自动管理事务生命周期（配合 `using` 使用）

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `logger` | `ILogger` | 是 | 日志记录器 |
| `isolationLevel` | `System.Data.IsolationLevel` | 否 | 事务隔离级别，默认 `ReadCommitted` |

**返回值**: `Task<ITransactionScope>` — 事务作用域对象

**ITransactionScope 接口**:

| 成员 | 说明 |
|------|------|
| `Task CommitAsync()` | 提交事务 |
| `Task RollbackAsync()` | 回滚事务 |
| `bool IsCommitted` | 事务是否已提交 |

> **注意**: `ITransactionScope` 继承 `IAsyncDisposable`。若释放时未提交，自动回滚事务。

---

###### `Task ExecuteInTransactionAsync(this IUnitOfWork unitOfWork, ILogger logger, Func<Task> action, System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)`

**说明**: 在事务作用域中执行操作（无返回值）

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `logger` | `ILogger` | 是 | 日志记录器 |
| `action` | `Func<Task>` | 是 | 要执行的操作 |
| `isolationLevel` | `System.Data.IsolationLevel` | 否 | 事务隔离级别，默认 `ReadCommitted` |

**内部逻辑**: 自动 `BeginTransactionScopeAsync` → 执行操作 → 成功时自动 `CommitAsync`，异常时自动 `RollbackAsync`

---

###### `Task<T> ExecuteInTransactionAsync<T>(this IUnitOfWork unitOfWork, ILogger logger, Func<Task<T>> action, System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)`

**说明**: 在事务作用域中执行操作（带返回值）

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `logger` | `ILogger` | 是 | 日志记录器 |
| `action` | `Func<Task<T>>` | 是 | 要执行的操作 |
| `isolationLevel` | `System.Data.IsolationLevel` | 否 | 事务隔离级别，默认 `ReadCommitted` |

**返回值**: `Task<T>` — 操作的结果

**使用示例**:
```csharp
public async Task TransferStockAsync(long fromId, long toId, int quantity)
{
    using var scope = await _unitOfWork.BeginTransactionScopeAsync(_logger);
    try
    {
        // 业务操作...
        await scope.CommitAsync();
    }
    catch
    {
        await scope.RollbackAsync();
        throw;
    }
}

// 或使用便捷方法：
await _unitOfWork.ExecuteInTransactionAsync(_logger, async () =>
{
    var repo = _unitOfWork.GetRepository<Stock, long>();
    // 业务操作...
});
```

---

#### 3.3.4 IDatabaseInitService / DatabaseInitService

**命名空间**: `KH.WMS.Core.Database`

**说明**: 数据库初始化服务，提供创建数据库、创建表结构和完整初始化的能力。

**DatabaseInitService 特性**: `[RegisteredService(Lifetime = Singleton)]`

**构造函数**:

| 参数名 | 类型 | 说明 |
|--------|------|------|
| `db` | `ISqlSugarClient` | SqlSugar 客户端实例 |

---

##### 方法

###### `void CreateDatabase(string databaseName = "")`

**说明**: 创建数据库

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `databaseName` | `string` | 否 | 数据库名称。为空时使用连接字符串中指定的数据库名 |

**内部逻辑**: 调用 `SqlSugar` 的 `DbMaintenance.CreateDatabase()`

---

###### `void CreateTables()`

**说明**: 创建所有实体对应的数据库表

**内部逻辑**:
1. 获取 `RootEntity` 基类所在的程序集
2. 扫描所有引用的程序集，查找继承自 `RootEntity` 的非抽象类
3. 使用 `SqlSugar` 的 `CodeFirst.InitTables()` 创建表结构

---

###### `void InitDatabase()`

**说明**: 完整数据库初始化（创建数据库 + 创建表结构）

**内部逻辑**:
1. 依次调用 `CreateDatabase()` 和 `CreateTables()`
2. 控制台输出初始化进度和结果

**使用示例**:
```csharp
// 通常在应用启动时调用
var initService = serviceProvider.GetRequiredService<IDatabaseInitService>();
initService.InitDatabase();
```

---

#### 3.3.5 SqlSugarDbContext

**命名空间**: `KH.WMS.Core.Database.SqlSugar`

**说明**: SqlSugar 数据库上下文实现，支持事务嵌套。通过栈结构管理事务作用域，实现嵌套事务的正确提交/回滚。

**特性**: `[RegisteredService(Lifetime = Scoped, WithoutInterceptor = true)]`

**实现接口**: `IDbContext`

**构造函数**:

| 参数名 | 类型 | 说明 |
|--------|------|------|
| `sqlSugarClient` | `ISqlSugarClient` | SqlSugar 客户端实例 |
| `logger` | `ILogger<SqlSugarDbContext>` | 日志记录器 |
| `options` | `IOptions<DatabaseOptions>` | 数据库配置选项 |

---

##### 属性

| 属性名 | 类型 | 说明 |
|--------|------|------|
| `Db` | `ISqlSugarClient` | SqlSugar 客户端（对外暴露，可直接使用 SqlSugar 全部功能） |
| `HasActiveTransaction` | `bool` | 是否在事务中（栈深度 > 0） |
| `TransactionDepth` | `int` | 事务嵌套层数 |
| `CurrentIsolationLevel` | `System.Data.IsolationLevel?` | 当前事务隔离级别 |

---

##### 事务嵌套机制

SqlSugarDbContext 使用 `Stack<TransactionScope>` 管理事务嵌套：

| 层数 | 行为 |
|------|------|
| 最外层（栈底） | 真正调用 `SqlSugar` 的 `BeginTranAsync` / `CommitTranAsync` / `RollbackTranAsync` |
| 嵌套层（栈顶） | 仅增加深度计数，不执行真实数据库事务操作 |
| 嵌套回滚 | 标记 `RequiresRollback = true`，外层提交时自动整体回滚 |

**内部 TransactionScope 类属性**:

| 属性名 | 类型 | 说明 |
|--------|------|------|
| `IsolationLevel` | `System.Data.IsolationLevel` | 事务隔离级别 |
| `IsCompleted` | `bool` | 嵌套事务是否已完成 |
| `RequiresRollback` | `bool` | 嵌套事务是否标记为需要回滚 |

---

##### 方法

###### `Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)`

**说明**: 开始事务（支持嵌套）

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `isolationLevel` | `System.Data.IsolationLevel` | 否 | 事务隔离级别，默认 `ReadCommitted` |

**内部逻辑**:
- 推入新的 `TransactionScope` 到栈中
- 仅当栈深度为 1（最外层）时调用 `_sqlSugarClient.Ado.BeginTranAsync()`

---

###### `Task CommitTransactionAsync()`

**说明**: 提交事务

**内部逻辑**:
- 检查栈顶 `TransactionScope` 的 `RequiresRollback` 标记
- 已标记回滚 → 调用 `RollbackTransactionAsync()` 整体回滚
- 最外层 → 调用 `_sqlSugarClient.Ado.CommitTranAsync()`
- 嵌套层 → 仅弹出栈、标记 `IsCompleted = true`

---

###### `Task RollbackTransactionAsync()`

**说明**: 回滚事务

**内部逻辑**:
- 最外层 → 调用 `_sqlSugarClient.Ado.RollbackTranAsync()`，清空栈
- 嵌套层 → 标记 `RequiresRollback = true`，弹出栈

---

###### `IRepository<T, TKey> GetRepository<T, TKey>() where T : class, new() where TKey : struct`

**说明**: 获取仓储实例

**内部逻辑**: 每次调用创建新的 `RepositoryBase<T, TKey>` 实例（共享同一 `ISqlSugarClient`）

---

##### Dispose 行为

释放时自动回滚未提交的事务，然后释放 `ISqlSugarClient`。

---

#### 3.3.6 SqlSugarSetup

**命名空间**: `KH.WMS.Core.Database.SqlSugar`

**说明**: SqlSugar 配置入口，注册 `ISqlSugarClient` 到 DI 容器，配置连接字符串、AOP 事件、枚举存储和审计字段自动填充。

---

##### 扩展方法

###### `IServiceCollection AddSqlSugar(this IServiceCollection services, IConfiguration configuration)`

**说明**: 添加 SqlSugar 服务到 DI 容器

**参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `services` | `IServiceCollection` | 是 | 服务集合 |
| `configuration` | `IConfiguration` | 是 | 配置对象 |

**注册内容**:

| 服务 | 生命周期 | 说明 |
|------|----------|------|
| `DatabaseOptions` | Singleton (IOptions) | 从 `appsettings.json` 的 `DbConnection` 节绑定 |
| `ISqlSugarClient` | Scoped | SqlSugar 客户端，每个请求一个实例 |

**配置详情**:

| 配置项 | 来源 | 说明 |
|--------|------|------|
| `ConnectionString` | `DatabaseOptions.ConnectionString` | 数据库连接字符串 |
| `DbType` | `DatabaseOptions.DbType` | 数据库类型（MySql / SqlServer / PostgreSQL / Oracle / Sqlite） |
| `IsAutoCloseConnection` | `true` | 自动关闭连接 |
| `InitKeyType` | `Attribute` | 从特性读取主键和自增列信息 |

**支持的数据库类型**:

| 配置文件值 | SqlSugar DbType | 说明 |
|-----------|-----------------|------|
| `MySql` | `DbType.MySql` | MySQL 数据库 |
| `SqlServer` | `DbType.SqlServer` | SQL Server 数据库（默认） |
| `PostgreSql` | `DbType.PostgreSQL` | PostgreSQL 数据库 |
| `Oracle` | `DbType.Oracle` | Oracle 数据库 |
| `Sqlite` | `DbType.Sqlite` | SQLite 数据库 |

---

##### AOP 事件

**OnLogExecuting** — 每次 SQL 执行时触发：

| 行为 | 说明 |
|------|------|
| 枚举参数值转换 | 自动将枚举值转为字符串（确保 WHERE/SET 子句中的枚举比较正确） |
| MiniProfiler 记录 | 将 SQL 和参数记录到 MiniProfiler 的 `SQL` 分类中 |
| SQL 日志输出 | 当 `EnableSqlLog = true` 时，通过 `ILogger.LogTrace` 记录 SQL 和参数 |
| 控制台输出 | 当 `EnableSqlLog = true` 时，同时在控制台输出 SQL 语句 |

**DataExecuting** — 数据写入时触发（审计字段自动填充）：

| 操作类型 | 审计字段 | 赋值来源 |
|----------|----------|----------|
| `InsertByObject` | `CreatedTime` | `DateTime.Now` |
| `InsertByObject` | `CreatedBy` | `userContext.UserId.ToString()` |
| `InsertByObject` | `CreatedByName` | `userContext.UserName` |
| `UpdateByObject` | `LastModifiedTime` | `DateTime.Now` |
| `UpdateByObject` | `LastModifiedBy` | `userContext.UserId.ToString()` |
| `UpdateByObject` | `LastModifiedByName` | `userContext.UserName` |

---

##### 外部服务配置

| 配置 | 说明 |
|------|------|
| `EntityService` | 枚举属性自动映射为 `nvarchar(20)` 数据库列类型 |

---

##### DatabaseOptions 配置类

**命名空间**: `KH.WMS.Core.Database.SqlSugar`

**属性**:

| 属性名 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `ConnectionString` | `string` | `""` | 数据库连接字符串 |
| `DbType` | `string` | `AppSettingsConstants.DbType_SqlServer` | 数据库类型标识 |
| `EnableSqlLog` | `bool` | `true` | 是否启用 SQL 日志 |
| `CommandTimeout` | `int` | `30` | 命令超时时间（秒） |

**appsettings.json 配置示例**:
```json
{
  "DbConnection": {
    "ConnectionString": "Server=localhost;Database=KHWMS;User Id=sa;Password=123456;",
    "DbType": "SqlServer",
    "EnableSqlLog": true,
    "CommandTimeout": 30
  }
}
```

### 3.4 Authentication 认证

#### 3.4.1 IJwtTokenService / JwtTokenService

**命名空间**: `KH.WMS.Core.Authentication` / `KH.WMS.Core.Authentication.JWT`

**说明**: JWT Token 服务接口及实现，负责访问令牌和刷新令牌的生成、验证、解析与刷新。`JwtTokenService` 注册为 Singleton 生命周期，依赖 `IOptions<JwtTokenOptions>` 和 `ICacheService`。

---

##### `string GenerateAccessToken(long userId, string username, long roleId)`

**说明**: 生成 JWT 访问令牌（Access Token）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `userId` | `long` | 是 | 用户ID，写入 `jti` 声明 |
| `username` | `string` | 是 | 用户名，写入 `unique_name` 声明 |
| `roleId` | `long` | 是 | 角色ID，写入 `role` 声明 |

**返回值**: `string` — JWT Token 字符串（格式：`header.payload.signature`）

**内部逻辑**: 手动构建 JWT（绕过 `JwtPayload` 的 claim 吸收问题），载荷包含 `jti`、`iat`、`nbf`、`exp`、`iss`、`aud`、`role`、`unique_name`。过期时间优先从缓存参数 `SYS_TOKEN_EXPIRE_MIN` 读取，失败时使用 `JwtTokenOptions.AccessTokenExpirationMinutes` 默认值。签名算法为 HS256。

---

##### `string GenerateRefreshToken()`

**说明**: 生成刷新令牌（Refresh Token）

**返回值**: `string` — 32 字节随机数经 Base64 编码的字符串

**内部逻辑**: 使用 `RNGCryptoServiceProvider` 生成 256 位（32 字节）加密安全随机数。

---

##### `bool ValidateToken(string token)`

**说明**: 验证 JWT Token 的有效性

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `token` | `string` | 是 | 待验证的 JWT Token |

**返回值**: `bool` — 验证通过返回 `true`，否则返回 `false`

**验证项**:
- Issuer（发行者）验证
- Audience（受众）验证
- Lifetime（有效期）验证
- IssuerSigningKey（签名密钥）验证
- ClockSkew（时钟偏移）验证（默认 5 秒）

---

##### `long? GetUserIdFromToken(string token)`

**说明**: 从 Token 中提取用户ID（`jti` 声明）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `token` | `string` | 是 | JWT Token |

**返回值**: `long?` — 用户ID，解析失败返回 `null`

---

##### `string? GetUsernameFromToken(string token)`

**说明**: 从 Token 中提取用户名（`unique_name` 声明）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `token` | `string` | 是 | JWT Token |

**返回值**: `string?` — 用户名，不存在返回 `null`

---

##### `IEnumerable<string>? GetRolesFromToken(string token)`

**说明**: 从 Token 中提取角色信息（`role` 声明）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `token` | `string` | 是 | JWT Token |

**返回值**: `IEnumerable<string>?` — 角色列表，不存在返回 `null`

---

##### `(string accessToken, string refreshToken)? RefreshToken(string accessToken, string refreshToken)`

**说明**: 刷新令牌，生成新的访问令牌和刷新令牌

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `accessToken` | `string` | 是 | 当前访问令牌（需有效） |
| `refreshToken` | `string` | 是 | 当前刷新令牌 |

**返回值**: `(string accessToken, string refreshToken)?` — 新的令牌对；令牌无效时返回 `null`

**内部逻辑**:
1. 验证 `accessToken` 是否有效
2. 从 Token 中提取 `userId` 和 `username`
3. 生成新的访问令牌和刷新令牌

**使用示例**:
```csharp
public class AuthService
{
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IJwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    public string Login(long userId, string username, long roleId)
    {
        return _jwtTokenService.GenerateAccessToken(userId, username, roleId);
    }

    public (string, string)? Refresh(string accessToken, string refreshToken)
    {
        return _jwtTokenService.RefreshToken(accessToken, refreshToken);
    }
}
```

#### 3.4.2 JwtBearerExtensions

**命名空间**: `KH.WMS.Core.Authentication.JWT`

**说明**: JWT Bearer 认证扩展方法，用于配置 ASP.NET Core 的 JWT Bearer 认证中间件。

---

##### `IServiceCollection AddJwtBearerAuthentication(this IServiceCollection services, JwtTokenOptions jwtOptions)`

**说明**: 添加 JWT Bearer 认证到服务集合中

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `services` | `IServiceCollection` | 是 | 服务集合 |
| `jwtOptions` | `JwtTokenOptions` | 是 | JWT 配置选项 |

**返回值**: `IServiceCollection` — 用于链式调用

**内部配置**:

| 配置项 | 值 | 说明 |
|--------|-----|------|
| `DefaultAuthenticateScheme` | `JwtBearerDefaults.AuthenticationScheme` | 默认认证方案 |
| `DefaultChallengeScheme` | `JwtBearerDefaults.AuthenticationScheme` | 默认挑战方案 |
| `MapInboundClaims` | `false` | 关闭 Claim 类型映射，保持原始 JWT 标准名称 |

**TokenValidationParameters 配置**:
| 参数 | 来源 |
|------|------|
| `SaveSigninToken` | `true` |
| `ValidateIssuer` | `jwtOptions.ValidateIssuer` |
| `ValidateAudience` | `jwtOptions.ValidateAudience` |
| `ValidateLifetime` | `jwtOptions.ValidateLifetime` |
| `ValidateIssuerSigningKey` | `true` |
| `ValidIssuer` | `jwtOptions.Issuer` |
| `ValidAudience` | `jwtOptions.Audience` |
| `IssuerSigningKey` | `SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))` |
| `ClockSkew` | `TimeSpan.FromSeconds(jwtOptions.ClockSkewSeconds)` |

**事件处理**:
| 事件 | 行为 |
|------|------|
| `OnMessageReceived` | 从 `Authorization` 请求头中提取 Bearer Token |
| `OnAuthenticationFailed` | 记录认证失败的异常日志 |
| `OnTokenValidated` | 无操作（预留） |
| `OnChallenge` | 返回统一 401 JSON 响应：`{ "message": "授权未通过", "status": false, "code": 401 }` |

**使用示例**:
```csharp
// 在 Program.cs 或 Startup 中配置
builder.Services.AddJwtBearerAuthentication(new JwtTokenOptions
{
    Secret = "BB3647441FFA4B5DB4E64A29B53CE525",
    Issuer = "KH.WMS",
    Audience = "KH.WMS",
    AccessTokenExpirationMinutes = 30,
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true
});
```

#### 3.4.3 JwtTokenOptions

**命名空间**: `KH.WMS.Core.Authentication.JWT`

**说明**: JWT 配置选项类，标记为 `[SelfRegisteredService]`（Singleton），从 `appsettings.json` 的 `JwtToken` 节绑定。

**属性**:
| 属性名 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `Secret` | `string` | `"BB3647441FFA4B5DB4E64A29B53CE525"` | 签名密钥，用于 HS256 签名 |
| `Issuer` | `string` | `"KH.WMS"` | 发行者（iss） |
| `Audience` | `string` | `"KH.WMS"` | 受众（aud） |
| `AccessTokenExpirationMinutes` | `int` | `30` | 访问令牌过期时间（分钟） |
| `RefreshTokenExpirationDays` | `int` | `7` | 刷新令牌过期时间（天） |
| `ClockSkewSeconds` | `int` | `5` | 时钟偏移（秒），用于容忍服务器时间差异 |
| `ValidateIssuer` | `bool` | `true` | 是否验证发行者 |
| `ValidateAudience` | `bool` | `true` | 是否验证受众 |
| `ValidateLifetime` | `bool` | `true` | 是否验证有效期 |

**使用示例**:
```csharp
// appsettings.json
{
  "JwtToken": {
    "Secret": "BB3647441FFA4B5DB4E64A29B53CE525",
    "Issuer": "KH.WMS",
    "Audience": "KH.WMS",
    "AccessTokenExpirationMinutes": 30,
    "RefreshTokenExpirationDays": 7,
    "ClockSkewSeconds": 5,
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true
  }
}
```

#### 3.4.4 TokenValidationParameters

**命名空间**: `KH.WMS.Core.Authentication.JWT`

**说明**: Token 验证参数扩展，提供静态工厂方法创建标准的 `Microsoft.IdentityModel.Tokens.TokenValidationParameters`。

---

##### `TokenValidationParameters CreateParameters(JwtTokenOptions options)`

**说明**: 从 `JwtTokenOptions` 创建 Token 验证参数

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `options` | `JwtTokenOptions` | 是 | JWT 配置选项 |

**返回值**: `Microsoft.IdentityModel.Tokens.TokenValidationParameters`

**返回的验证参数**:
| 参数 | 值 |
|------|------|
| `ValidateIssuer` | `options.ValidateIssuer` |
| `ValidateAudience` | `options.ValidateAudience` |
| `ValidateLifetime` | `options.ValidateLifetime` |
| `ValidateIssuerSigningKey` | `true` |
| `ValidIssuer` | `options.Issuer` |
| `ValidAudience` | `options.Audience` |
| `IssuerSigningKey` | `SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret))` |
| `ClockSkew` | `TimeSpan.FromSeconds(options.ClockSkewSeconds)` |

**使用示例**:
```csharp
var parameters = TokenValidationParametersExtensions.CreateParameters(jwtOptions);
var handler = new JwtSecurityTokenHandler();
var principal = handler.ValidateToken(token, parameters, out _);
```

### 3.5 Caching 缓存

#### 3.5.1 ICacheService

**命名空间**: `KH.WMS.Core.Caching`

**说明**: 缓存服务接口，定义通用的缓存操作方法（获取、设置、删除、存在性检查、GetOrCreate 模式等）。

---

##### `T? Get<T>(string key)`

**说明**: 根据键获取缓存值

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 缓存键 |

**返回值**: `T?` — 缓存值，不存在返回 `default`

---

##### `bool TryGet<T>(string key, out T? value)`

**说明**: 尝试获取缓存值

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 缓存键 |
| `value` | `out T?` | — | 输出参数，缓存值 |

**返回值**: `bool` — 是否成功获取

---

##### `void Set<T>(string key, T value, TimeSpan? expiration = null)`

**说明**: 设置缓存值

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 缓存键 |
| `value` | `T` | 是 | 缓存值 |
| `expiration` | `TimeSpan?` | 否 | 过期时间（为 null 时使用默认过期策略） |

---

##### `bool TrySet<T>(string key, T value, TimeSpan? expiration = null)`

**说明**: 尝试设置缓存值（仅当缓存不存在时设置）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 缓存键 |
| `value` | `T` | 是 | 缓存值 |
| `expiration` | `TimeSpan?` | 否 | 过期时间 |

**返回值**: `bool` — 是否成功设置（`false` 表示键已存在）

---

##### `bool SetOrCreate<T>(string key, T value, TimeSpan? expiration = null)`

**说明**: 设置或更新缓存值（键存在时更新）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 缓存键 |
| `value` | `T` | 是 | 缓存值 |
| `expiration` | `TimeSpan?` | 否 | 过期时间 |

**返回值**: `bool` — 始终返回 `true`

---

##### `bool Remove(string key)`

**说明**: 删除指定键的缓存

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 缓存键 |

**返回值**: `bool` — 始终返回 `true`

---

##### `void RemoveMultiple(IEnumerable<string> keys)`

**说明**: 批量删除多个缓存键

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `keys` | `IEnumerable<string>` | 是 | 缓存键集合 |

---

##### `bool Exists(string key)`

**说明**: 检查缓存键是否存在

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 缓存键 |

**返回值**: `bool` — 存在返回 `true`

---

##### `T? GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null)`

**说明**: 获取或创建缓存（缓存不存在时执行 factory 创建并缓存）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 缓存键 |
| `factory` | `Func<T>` | 是 | 创建缓存值的工厂函数 |
| `expiration` | `TimeSpan?` | 否 | 过期时间 |

**返回值**: `T?` — 缓存值

---

##### `Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)`

**说明**: 异步获取或创建缓存

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 缓存键 |
| `factory` | `Func<Task<T>>` | 是 | 异步工厂函数 |
| `expiration` | `TimeSpan?` | 否 | 过期时间 |

**返回值**: `Task<T?>` — 缓存值

---

##### `bool Refresh(string key, TimeSpan? expiration = null)`

**说明**: 刷新缓存，延长过期时间

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 缓存键 |
| `expiration` | `TimeSpan?` | 否 | 新的过期时间 |

**返回值**: `bool` — 刷新成功返回 `true`，缓存不存在返回 `false`

**使用示例**:
```csharp
public class ProductService
{
    private readonly ICacheService _cache;

    public ProductService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<Product> GetProductAsync(long id)
    {
        return await _cache.GetOrCreateAsync($"product:{id}", async () =>
        {
            return await _db.Queryable<Product>().FirstAsync(p => p.Id == id);
        }, TimeSpan.FromMinutes(30));
    }

    public void RemoveProductCache(long id)
    {
        _cache.Remove($"product:{id}");
    }
}
```

#### 3.5.2 IMemoryCacheService / MemoryCacheService

**命名空间**: `KH.WMS.Core.Caching.Memory`

**说明**: 内存缓存服务接口及实现，继承 `ICacheService` 并扩展了滑动过期、混合过期和缓存过期回调功能。`MemoryCacheService` 注册为 Scoped 生命周期，依赖 `IMemoryCache`（Microsoft.Extensions.Caching.Memory）和 `ILoggerService`。

**扩展方法**:

---

##### `void SetWithSlidingExpiration<T>(string key, T value, TimeSpan slidingExpiration)`

**说明**: 设置带滑动过期时间的缓存（每次访问时重置过期计时）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 缓存键 |
| `value` | `T` | 是 | 缓存值 |
| `slidingExpiration` | `TimeSpan` | 是 | 滑动过期时间 |

---

##### `void SetWithHybridExpiration<T>(string key, T value, TimeSpan absoluteExpiration, TimeSpan slidingExpiration)`

**说明**: 设置带绝对过期时间和滑动过期时间的混合过期缓存

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 缓存键 |
| `value` | `T` | 是 | 缓存值 |
| `absoluteExpiration` | `TimeSpan` | 是 | 绝对过期时间（最长生存期） |
| `slidingExpiration` | `TimeSpan` | 是 | 滑动过期时间 |

---

##### `void RegisterPostEvictionCallback(Action<object, object, EvictionReason, object> callback)`

**说明**: 注册缓存过期回调（当缓存项被逐出时触发）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `callback` | `Action<object, object, EvictionReason, object>` | 是 | 回调委托（参数：键、值、逐出原因、状态） |

**内部日志**:
- 设置缓存时记录 Debug 日志：`"缓存已设置: {Key}, 过期时间: {Expiration}s"`
- 删除缓存时记录 Debug 日志：`"缓存已删除: {Key}"`

**使用示例**:
```csharp
public class SessionManager
{
    private readonly IMemoryCacheService _cache;

    public SessionManager(IMemoryCacheService cache)
    {
        _cache = cache;
    }

    public void SetSession(string sessionId, UserSession session)
    {
        // 使用滑动过期：用户活动时自动续期
        _cache.SetWithSlidingExpiration($"session:{sessionId}", session, TimeSpan.FromMinutes(20));
    }

    public void SetTempData(string key, object data)
    {
        // 使用混合过期：最长 1 小时，但 5 分钟无访问则过期
        _cache.SetWithHybridExpiration(key, data, TimeSpan.FromHours(1), TimeSpan.FromMinutes(5));
    }
}
```

#### 3.5.3 CacheEntryOptions

**命名空间**: `KH.WMS.Core.Caching.Memory`

**说明**: 缓存策略配置静态类，提供预定义的 `MemoryCacheEntryOptions` 工厂方法和自定义创建方法。

**静态属性**:

| 属性 | 类型 | 说明 |
|------|------|------|
| `Default` | `MemoryCacheEntryOptions` | 默认策略：绝对过期 30 分钟 |
| `Short` | `MemoryCacheEntryOptions` | 短期缓存：绝对过期 5 分钟 |
| `Medium` | `MemoryCacheEntryOptions` | 中期缓存：绝对过期 30 分钟 |
| `Long` | `MemoryCacheEntryOptions` | 长期缓存：绝对过期 2 小时 |
| `Sliding` | `MemoryCacheEntryOptions` | 滑动过期缓存：滑动过期 10 分钟 |
| `Never` | `MemoryCacheEntryOptions` | 永不过期缓存：`Priority = CacheItemPriority.NeverRemove`（谨慎使用） |

**静态方法**:

##### `MemoryCacheEntryOptions Create(TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)`

**说明**: 创建自定义缓存策略

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `absoluteExpiration` | `TimeSpan?` | 否 | 绝对过期时间（相对于当前时间） |
| `slidingExpiration` | `TimeSpan?` | 否 | 滑动过期时间 |

**返回值**: `MemoryCacheEntryOptions`

**使用示例**:
```csharp
// 使用预定义策略
_cache.Set("key1", value, CacheEntryOptions.Default);
_cache.Set("key2", value, CacheEntryOptions.Short);   // 5分钟过期
_cache.Set("key3", value, CacheEntryOptions.Long);    // 2小时过期

// 使用滑动过期
_cache.Set("session", value, CacheEntryOptions.Sliding);

// 自定义策略
var customOptions = CacheEntryOptions.Create(
    absoluteExpiration: TimeSpan.FromMinutes(30),
    slidingExpiration: TimeSpan.FromMinutes(5)
);
_cache.Set("key4", value, customOptions);
```

### 3.6 Logging 日志

#### 3.6.1 ILoggerService / LoggerService

**命名空间**: `KH.WMS.Core.Logging` / `KH.WMS.Core.Logging.Serilog`

**说明**: 日志服务接口及 Serilog 实现，提供按级别记录日志、结构化日志、按模块自动分类、支持指定文件名等能力。`LoggerService` 注册为 Scoped 生命周期（`WithoutInterceptor = true`），依赖 `ILogger<LoggerService>` 和 `IHttpContextAccessor`。

**日志级别方法**（按严重程度升序）:

---

##### `void LogVerbose(string message, params object?[] args)`

**说明**: 记录详细跟踪日志 — 每一行 SQL 执行、详细流程跟踪

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `message` | `string` | 是 | 日志消息模板 |
| `args` | `params object?[]` | 否 | 模板参数 |

---

##### `void LogDebug(string message, params object?[] args)`

**说明**: 记录调试日志 — 方法参数、中间变量

---

##### `void LogInfo(string message, params object?[] args)`

**说明**: 记录信息日志 — 用户登录、单据创建

---

##### `void LogWarning(string message, params object?[] args)`

**说明**: 记录警告日志 — API 调用慢、缓存未命中，需关注

---

##### `void LogError(string message, params object?[] args)`

**说明**: 记录错误日志 — HTTP 500、业务异常，但系统可继续

---

##### `void LogError(Exception exception, string message, params object?[] args)`

**说明**: 记录错误日志（带异常对象）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `exception` | `Exception` | 是 | 异常对象 |
| `message` | `string` | 是 | 日志消息模板 |
| `args` | `params object?[]` | 否 | 模板参数 |

---

##### `void LogFatal(string message, params object?[] args)`

**说明**: 记录致命错误日志 — 数据库连接失败、磁盘满，系统无法继续

---

##### `void LogFatal(Exception exception, string message, params object?[] args)`

**说明**: 记录致命错误日志（带异常对象）

---

**业务日志方法**:

##### `void LogOperation(string operation, string? userName = null, long? userId = null, object? data = null)`

**说明**: 记录操作日志（登录、增删改等用户操作）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `operation` | `string` | 是 | 操作描述 |
| `userName` | `string?` | 否 | 用户名（默认从 HttpContext 获取） |
| `userId` | `long?` | 否 | 用户ID（默认从 HttpContext 获取） |
| `data` | `object?` | 否 | 附加数据 |

**输出格式**: `[ModuleCode] [OPERATION] {Operation} by {UserName} (UserId:{UserId}) {Data}`

---

##### `void LogBusiness(string businessType, string message, object? data = null)`

**说明**: 记录业务日志（单据创建、状态变更等）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `businessType` | `string` | 是 | 业务类型标识 |
| `message` | `string` | 是 | 日志消息 |
| `data` | `object?` | 否 | 附加数据 |

**输出格式**: `[ModuleCode] [BUSINESS:{Type}] {Message} {Data}`

---

##### `void LogPerformance(string operation, long elapsedMs, object? data = null)`

**说明**: 记录性能日志（包含耗时信息）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `operation` | `string` | 是 | 操作描述 |
| `elapsedMs` | `long` | 是 | 耗时（毫秒） |
| `data` | `object?` | 否 | 附加数据 |

**行为**: 耗时 > 3 秒时自动提升日志级别为 Warning，并设置 `LogLevelType.Warning`

**输出格式**: `[ModuleCode] [PERFORMANCE] {Operation} 耗时 {Elapsed}ms {Data}`

---

##### `void Log(LogContext context, string message, params object?[] args)`

**说明**: 使用上下文记录日志（可手动指定模块、级别、类型等）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `context` | `LogContext` | 是 | 日志上下文，包含模块、级别、用户等信息 |
| `message` | `string` | 是 | 日志消息模板 |
| `args` | `params object?[]` | 否 | 模板参数 |

---

**指定文件名的方法**（日志路由到自定义文件）:

##### `void LogInfoToFile(string fileName, string message, params object?[] args)`

**说明**: 记录信息日志到指定文件

##### `void LogErrorToFile(string fileName, string message, params object?[] args)`

**说明**: 记录错误日志到指定文件

##### `void LogErrorToFile(string fileName, Exception exception, string message, params object?[] args)`

**说明**: 记录错误日志到指定文件（带异常）

##### `void LogOperationToFile(string fileName, string operation, string? userName = null, long? userId = null, object? data = null)`

**说明**: 记录操作日志到指定文件

##### `void LogBusinessToFile(string fileName, string businessType, string message, object? data = null)`

**说明**: 记录业务日志到指定文件

**内部逻辑**:
- 所有方法自动通过 `LogModuleDetector.DetectModule()` 从调用栈识别模块
- 日志格式统一为：`[{ModuleCode}] {Message}`（模块代码格式：`"{int} {EnumName}"`，例如 `"1001 Api"`）
- 通过 `SerilogLogContext.PushProperty("FileName", fileName)` 实现日志事件路由到自定义文件（需配合 `WriteFileNameLogs` 配置）
- 创建 `LogContext` 时自动从 `HttpContext` 提取用户信息、请求 ID、租户 ID 等上下文

**使用示例**:
```csharp
public class InboundService
{
    private readonly ILoggerService _logger;

    public InboundService(ILoggerService logger)
    {
        _logger = logger;
    }

    public async Task ReceiveAsync(long inboundId)
    {
        _logger.LogInfo("开始收货处理，入库单ID: {InboundId}", inboundId);

        try
        {
            // 业务处理...
            _logger.LogOperation("收货", userName: "admin", userId: 1, data: new { InboundId = inboundId });
            _logger.LogBusiness("INBOUND_RECEIVE", "收货完成", new { InboundId = inboundId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "收货处理失败，入库单ID: {InboundId}", inboundId);
            throw;
        }
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        var sw = Stopwatch.StartNew();
        var products = await _db.Queryable<Product>().ToListAsync();
        sw.Stop();
        _logger.LogPerformance("查询产品列表", sw.ElapsedMilliseconds);
        return products;
    }
}
```

#### 3.6.2 ILogCleanupService / LogCleanupService

**命名空间**: `KH.WMS.Core.Logging.LogClean`

**说明**: 日志清理服务接口及实现，负责清理过期的文件日志和数据库日志。`LogCleanupService` 依赖 `IOptions<LogCleanupOptions>`、`ILogger<LogCleanupService>`、`ICacheService` 和可选的 `IDbContext`。

---

##### `Task CleanupFileLogsAsync()`

**说明**: 清理文件日志。根据保留天数删除过期文件，对超过最大文件大小的文件进行备份并清空。

**内部逻辑**:
1. 检查日志目录是否存在
2. 从缓存读取日志保留天数（`LOG_RETAIN_DAYS`），失败则使用 `LogCleanupOptions.RetentionDays` 默认值
3. 递归扫描所有 `*.txt` 日志文件
4. 删除最后修改时间早于截止日期的文件
5. 对超过 `MaxFileSizeMB` 的文件进行备份（重命名为 `{file}_backup_{timestamp}.txt`）后删除
6. 清理空目录

---

##### `Task CleanupDatabaseLogsAsync()`

**说明**: 清理数据库日志（当前实现为预留，代码已注释托管）

---

##### `Task CleanupAllAsync()`

**说明**: 执行全部清理（文件日志 + 数据库日志）

---

##### `Task<LogStatistics> GetStatisticsAsync()`

**说明**: 获取日志统计信息

**返回值**: `Task<LogStatistics>`

**包含的统计项**:
- 文件日志总数
- 文件日志总大小（MB）
- 数据库日志总数
- 最旧/最新日志日期

**使用示例**:
```csharp
public class LogCleanupJob : BackgroundService
{
    private readonly ILogCleanupService _logCleanup;

    public LogCleanupJob(ILogCleanupService logCleanup)
    {
        _logCleanup = logCleanup;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 每天凌晨 2 点执行清理
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            await _logCleanup.CleanupAllAsync();

            var stats = await _logCleanup.GetStatisticsAsync();
            Console.WriteLine($"日志统计: 文件数={stats.FileCount}, 大小={stats.FileSizeMB:F2}MB");
        }
    }
}
```

#### 3.6.3 LogCleanupOptions / LogStatistics

**命名空间**: `KH.WMS.Core.Logging.LogClean`

---

##### LogCleanupOptions

**说明**: 日志清理配置选项，从 `appsettings.json` 绑定。

**属性**:
| 属性名 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `RetentionDays` | `int` | `30` | 日志保留天数 |
| `MaxFileSizeMB` | `int` | `100` | 单个日志文件最大大小（MB） |
| `LogPath` | `string` | `"Logs"` | 日志目录路径 |
| `EnableAutoCleanup` | `bool` | `true` | 是否启用自动清理 |
| `CleanupTime` | `string` | `"02:00"` | 自动清理时间（24小时制） |
| `CleanupDatabaseLogs` | `bool` | `true` | 是否清理数据库日志 |
| `DatabaseLogTable` | `string` | `"SysLog"` | 数据库日志表名 |
| `DatabaseCleanupBatchSize` | `int` | `1000` | 每次清理的数据库日志数量 |

---

##### LogStatistics

**说明**: 日志统计信息。

**属性**:
| 属性名 | 类型 | 说明 |
|--------|------|------|
| `FileCount` | `int` | 文件日志总数 |
| `FileSizeMB` | `decimal` | 文件日志总大小（MB） |
| `DatabaseLogCount` | `long` | 数据库日志总数 |
| `OldestLogDate` | `DateTime?` | 最旧日志日期 |
| `NewestLogDate` | `DateTime?` | 最新日志日期 |

**使用示例**:
```csharp
// appsettings.json
{
  "LogCleanup": {
    "RetentionDays": 30,
    "MaxFileSizeMB": 100,
    "LogPath": "Logs",
    "EnableAutoCleanup": true,
    "CleanupTime": "02:00",
    "CleanupDatabaseLogs": true,
    "DatabaseLogTable": "SysLog",
    "DatabaseCleanupBatchSize": 1000
  }
}
```

#### 3.6.4 LogLevelType / LogModule / LogType

**命名空间**: `KH.WMS.Core.Logging.LogEnums`

---

##### LogLevelType

**说明**: 日志级别枚举

| 枚举值 | 值 | 说明 |
|--------|-----|------|
| `Verbose` | `0` | 详细跟踪 — 每一行 SQL 执行、详细流程跟踪 |
| `Debug` | `1` | 调试信息 — 方法参数、中间变量 |
| `Information` | `2` | 一般信息 — 用户登录、单据创建 |
| `Warning` | `3` | 警告 — API 调用慢、缓存未命中，需关注 |
| `Error` | `4` | 错误 — HTTP 500、业务异常，但系统可继续 |
| `Fatal` | `5` | 致命错误 — 数据库连接失败、磁盘满，系统无法继续 |
| `None` | `6` | 无 |

---

##### LogModule

**说明**: 日志模块枚举，按区域划分（基础模块 1000-1008，WMS 业务模块 2001-2021）

**基础模块**:
| 枚举值 | 值 | 说明 |
|--------|-----|------|
| `Core` | `1000` | 系统核心 |
| `Api` | `1001` | API 接口 |
| `Database` | `1002` | 数据库 |
| `Cache` | `1003` | 缓存 |
| `Auth` | `1004` | 认证授权 |
| `Business` | `1005` | 业务逻辑 |
| `External` | `1006` | 外部服务 |
| `Job` | `1007` | 任务调度 |
| `File` | `1008` | 文件处理 |

**WMS 业务模块**:
| 枚举值 | 值 | 说明 |
|--------|-----|------|
| `WMS_Inbound` | `2001` | 入库管理 |
| `WMS_Outbound` | `2002` | 出库管理 |
| `WMS_Inventory` | `2003` | 库存管理 |
| `WMS_Owner` | `2004` | 货主管理 |
| `WMS_Product` | `2005` | 商品管理 |
| `WMS_Warehouse` | `2006` | 仓库管理 |
| `WMS_Zone` | `2007` | 库区管理 |
| `WMS_Location` | `2008` | 库位管理 |
| `WMS_Counting` | `2009` | 盘点管理 |
| `WMS_Transfer` | `2010` | 移库管理 |
| `WMS_Wave` | `2011` | 波次管理 |
| `WMS_Picking` | `2012` | 拣货管理 |
| `WMS_Loading` | `2013` | 装车管理 |
| `WMS_Unloading` | `2014` | 卸车管理 |
| `WMS_Processing` | `2015` | 库内加工 |
| `WMS_Replenishment` | `2016` | 补货管理 |
| `WMS_Strategy` | `2017` | 策略管理 |
| `WMS_Order` | `2018` | 单据管理 |
| `WMS_Transport` | `2019` | 运输管理 |
| `WMS_Billing` | `2020` | 计费管理 |
| `WMS_Report` | `2021` | 报表管理 |

---

##### LogType

**说明**: 日志类型枚举

| 枚举值 | 值 | 说明 |
|--------|-----|------|
| `System` | `0` | 系统日志 |
| `Operation` | `1` | 操作日志 |
| `Business` | `2` | 业务日志 |
| `Exception` | `3` | 异常日志 |
| `Performance` | `4` | 性能日志 |
| `Security` | `5` | 安全日志 |
| `Audit` | `6` | 审计日志 |
| `Debug` | `7` | 调试日志 |
| `Access` | `8` | 访问日志 |
| `Job` | `9` | 任务日志 |

#### 3.6.5 LogModuleDetector

**命名空间**: `KH.WMS.Core.Logging`

**说明**: 日志模块自动识别器，通过调用栈（StackTrace）分析调用者的命名空间，自动匹配对应的 `LogModule` 枚举值，实现调用者无需手动指定日志模块。

---

##### `LogModule DetectModule(int skipFrames = 2)`

**说明**: 从调用栈中自动识别模块

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `skipFrames` | `int` | 否 | 跳过的帧数（默认跳过 LoggerService 内部调用） |

**返回值**: `LogModule` — 识别出的模块，默认返回 `LogModule.Core`

**内部逻辑**:
1. 创建 `StackTrace` 并逐帧向上遍历
2. 获取每帧的命名空间
3. 跳过日志系统本身（`KH.WMS.Core.Logging`）和 `Microsoft.Extensions.Logging` 命名空间
4. 在预定义的 `NamespaceModuleMapping` 字典中查找完整匹配或前缀匹配
5. 未匹配时检查是否包含 `Service`、`Domain`、`Application` 后缀 → 返回 `Business`
6. 最终未匹配返回 `Core`

**命名空间映射规则**（部分示例）:

| 命名空间前缀 | 映射模块 |
|-------------|---------|
| `KH.WMS.Core.Api` | `Api` |
| `KH.WMS.Core.Database` | `Database` |
| `KH.WMS.Core.Cache` | `Cache` |
| `KH.WMS.Core.Authentication` | `Auth` |
| `KH.WMS.Core.Inbound` | `WMS_Inbound` |
| `KH.WMS.Core.Outbound` | `WMS_Outbound` |
| `KH.WMS.Core.Inventory` | `WMS_Inventory` |
| `KH.WMS.Core.Picking` | `WMS_Picking` |
| `KH.WMS.Core.Warehouse` | `WMS_Warehouse` |
| `Controllers` / `Api` | `Api` |
| `External` / `ThirdParty` / `Integration` | `External` |

---

##### `LogModule DetectModuleFromType(Type type)`

**说明**: 从类型推断模块

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `type` | `Type` | 是 | 类型对象 |

**返回值**: `LogModule` — 根据类型的命名空间识别对应的模块

---

##### `void AddMapping(string namespacePrefix, LogModule module)`

**说明**: 添加自定义命名空间映射

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `namespacePrefix` | `string` | 是 | 命名空间前缀 |
| `module` | `LogModule` | 是 | 对应的模块枚举 |

---

##### `void AddMappings(Dictionary<string, LogModule> mappings)`

**说明**: 批量添加命名空间映射

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `mappings` | `Dictionary<string, LogModule>` | 是 | 命名空间到模块的映射字典 |

**使用示例**:
```csharp
// 在启动时注册自定义模块映射
LogModuleDetector.AddMapping("KH.WMS.CustomModule", LogModule.Business);
LogModuleDetector.AddMappings(new Dictionary<string, LogModule>
{
    ["KH.WMS.PluginA"] = LogModule.External,
    ["KH.WMS.PluginB"] = LogModule.Business
});
```

#### 3.6.6 LogContext

**命名空间**: `KH.WMS.Core.Logging`

**说明**: 日志上下文类，封装单条日志的完整上下文信息，包括模块、级别、类型、用户信息、请求信息等。

**属性**:

| 属性名 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `Level` | `LogLevelType` | `Information` | 日志级别 |
| `Module` | `LogModule` | — | 日志模块 |
| `Type` | `LogType` | `System` | 日志类型 |
| `UserId` | `long?` | — | 用户ID |
| `UserName` | `string?` | — | 用户名 |
| `TenantId` | `long?` | — | 租户ID |
| `RequestId` | `string?` | — | 请求ID（对应 HttpContext.TraceIdentifier） |
| `CorrelationId` | `string?` | — | 关联ID（从 `X-Correlation-ID` 请求头获取） |
| `ClientIp` | `string?` | — | 客户端IP |
| `Operation` | `string?` | — | 操作类型 |
| `Data` | `Dictionary<string, object?>` | `new()` | 业务数据字典 |
| `Properties` | `Dictionary<string, object?>` | `new()` | 扩展属性字典 |

**使用示例**:
```csharp
var context = new LogContext
{
    Level = LogLevelType.Information,
    Module = LogModule.WMS_Inbound,
    Type = LogType.Business,
    Operation = "收货确认",
    UserId = 1001,
    UserName = "admin",
    Data = { ["InboundId"] = 12345, ["Qty"] = 100 }
};

_logger.Log(context, "入库单 {InboundId} 收货完成", 12345);
```

#### 3.6.7 SerilogSetup / SerilogOptions / SerilogFileConfiguration

**命名空间**: `KH.WMS.Core.Logging.Serilog`

---

##### SerilogSetup

**说明**: Serilog 配置入口类，提供 `ConfigureSerilog` 和 `AddSerilog` 扩展方法。

---

###### `LoggerConfiguration ConfigureSerilog(string appName, string logDirectory = "Logs", LogEventLevel minimumLevel = LogEventLevel.Information, int retentionDays = 30, int maxFileSizeMB = 100, string? logFileName = null, string? errorFileName = null, string? warningFileName = null)`

**说明**: 配置 Serilog（支持文件大小限制和自定义文件名）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `appName` | `string` | 是 | 应用名称，写入 `Application` 属性 |
| `logDirectory` | `string` | 否 | 日志目录（默认 `"Logs"`） |
| `minimumLevel` | `LogEventLevel` | 否 | 最低日志级别（默认 `Information`） |
| `retentionDays` | `int` | 否 | 日志保留天数（默认 30） |
| `maxFileSizeMB` | `int` | 否 | 单个文件最大大小 MB（默认 100） |
| `logFileName` | `string?` | 否 | 自定义日志文件名（不含扩展名） |
| `errorFileName` | `string?` | 否 | 自定义错误日志文件名 |
| `warningFileName` | `string?` | 否 | 自定义警告日志文件名 |

**内部配置**:
- `MinimumLevel.Is(minimumLevel)`
- `MinimumLevel.Override("Microsoft", Warning)` / `("Microsoft.EntityFrameworkCore", Warning)` / `("System", Warning)`
- Enrichers: `FromLogContext()`, `LogEnricher`, `WithProperty("Application", appName)`
- Sinks: 控制台（模板：`[{Timestamp:HH:mm:ss} {Level:u3}] [{ModuleCode}] {Message:lj}{NewLine}{Exception}`）
- Sinks: 三个文件输出（info / error / warning），按天滚动，UTF-8 编码，文件大小限制，保留天数

---

###### `LoggerConfiguration ConfigureSerilog(string appName, SerilogOptions options)`

**说明**: 使用 `SerilogOptions` 配置 Serilog

---

###### `IHostBuilder AddSerilog(this IHostBuilder hostBuilder, string appName, string logDirectory = "Logs", int retentionDays = 30, int maxFileSizeMB = 100, string? logFileName = null, string? errorFileName = null, string? warningFileName = null)`

**说明**: 添加 Serilog 到主机（完整版，支持文件大小限制、自定义文件名、按模块分离日志）

**额外特性**:
- 从 `IConfiguration` 读取配置（`Serilog:LogPath`、`Serilog:RetentionDays`、`Serilog:MaxFileSizeMB`、`Serilog:FileNames:*` 等）
- 按模块分离日志（WMS 模块 2001-2021 自动路由到 `Logs/wms/{ModuleName}-.txt`）
- 按自定义文件名分离日志（通过 `FileName` 属性路由到 `Logs/custom/{FileName}-.txt`）
- Enrichers: `LogEnricher`, `ModuleLogEnricher`, `LogTypeEnricher`, `UserLogEnricher`, `CorrelationIdEnricher`
- `WithProperty("Application", ...)` 和 `WithProperty("Environment", ...)`

**输出模板（文件）**:
```
{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ModuleCode}] [LogType:{LogType}] [TenantId:{TenantId}] [UserId:{UserId}] [RequestId:{RequestId}] {Message:lj}{NewLine}{Exception}
```

---

##### SerilogOptions

**说明**: 日志配置选项类，从 `appsettings.json` 绑定。

**属性**:
| 属性名 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `MinimumLevel` | `string` | `"Information"` | 最低日志级别 |
| `LogDirectory` | `string` | `"Logs"` | 日志目录 |
| `RetentionDays` | `int` | `30` | 日志保留天数 |
| `MaxFileSizeMB` | `int` | `100` | 单个文件最大大小（MB） |
| `SplitByModule` | `bool` | `false` | 是否按模块分离日志 |
| `WriteToConsole` | `bool` | `true` | 是否输出到控制台 |
| `WriteToFile` | `bool` | `true` | 是否输出到文件 |
| `ModuleOverrides` | `Dictionary<string, string>` | `{ \"Microsoft\": \"Warning\", \"Microsoft.EntityFrameworkCore\": \"Warning\", \"System\": \"Warning\" }` | 模块级别覆盖 |
| `CustomLogFileName` | `string?` | `null` | 自定义日志文件名（不含路径和扩展名） |
| `CustomErrorFileName` | `string?` | `null` | 自定义错误日志文件名 |
| `CustomWarningFileName` | `string?` | `null` | 自定义警告日志文件名 |

---

##### SerilogFileConfiguration

**说明**: Serilog 日志配置扩展类，提供配置文件日志的便捷方法。

---

###### `LoggerConfiguration ConfigureFileLogging(this LoggerConfiguration loggerConfiguration, string logPath = "Logs", int retentionDays = 30, int maxFileSizeMB = 100, LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information)`

**说明**: 配置文件日志（支持文件大小限制），输出 `log-.txt`、`error-.txt`、`warning-.txt` 三个文件。

---

###### `LoggerConfiguration ConfigureFileLoggingWithCustomNames(this LoggerConfiguration loggerConfiguration, string logPath = "Logs", string? logFileName = null, string? errorFileName = null, string? warningFileName = null, int retentionDays = 30, int maxFileSizeMB = 100, LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information)`

**说明**: 配置文件日志（支持自定义文件名）

---

###### `LoggerConfiguration ConfigureFileLogging(this LoggerConfiguration loggerConfiguration, SerilogOptions options)`

**说明**: 使用 `SerilogOptions` 配置文件日志

**使用示例**:
```csharp
// 方式一：使用 SerilogSetup.ConfigureSerilog
Log.Logger = SerilogSetup.ConfigureSerilog(
    appName: "KH.WMS",
    minimumLevel: LogEventLevel.Information,
    retentionDays: 30,
    maxFileSizeMB: 100,
    logFileName: "myapp",
    errorFileName: "myapp-error",
    warningFileName: "myapp-warning"
).CreateLogger();

// 方式二：使用 SerilogSetup.AddSerilog（IHostBuilder 扩展）
builder.Host.AddSerilog("KH.WMS", new SerilogOptions
{
    MinimumLevel = "Information",
    LogDirectory = "Logs",
    RetentionDays = 30,
    MaxFileSizeMB = 100,
    WriteToConsole = true,
    CustomLogFileName = "myapp"
});

// 方式三：使用 SerilogFileConfiguration 扩展
new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .ConfigureFileLoggingWithCustomNames(
        logPath: "Logs",
        logFileName: "myapp",
        errorFileName: "myapp-error",
        retentionDays: 30,
        maxFileSizeMB: 100
    )
    .CreateLogger();
```

#### 3.6.8 Serilog Enrichers

**命名空间**: `KH.WMS.Core.Logging.Serilog.Enricher`

**说明**: Serilog 日志增强器集合，实现 `ILogEventEnricher` 接口，在日志写入时自动附加上下文属性。

---

##### LogEnricher

**说明**: 基础日志增强器，添加机器和进程信息。

| 添加的属性 | 来源 | 说明 |
|-----------|------|------|
| `MachineName` | `Environment.MachineName` | 机器名 |
| `ProcessId` | `Environment.ProcessId` | 进程ID |
| `ThreadId` | `Environment.CurrentManagedThreadId` | 当前线程ID |
| `AppDomain` | `AppDomain.CurrentDomain.FriendlyName` | 应用程序域名称 |
| `AppName` | 从 `AppContext.BaseDirectory` 推断 | 应用名称 |

---

##### ModuleLogEnricher

**说明**: 模块日志增强器，将 `Module` 属性转换为 `ModuleCode` 和 `ModuleName`。

| 添加的属性 | 来源 | 说明 |
|-----------|------|------|
| `ModuleCode` | 从 `Module` 属性值 | 模块代码（如 `"2001"`） |
| `ModuleName` | `LogModule` 枚举名 | 模块名称（如 `"WMS_Inbound"`） |

---

##### LogTypeEnricher

**说明**: 日志类型增强器，添加 `LogType` 属性（默认值为 `System`）。

| 添加的属性 | 来源 |
|-----------|------|
| `LogType` | 从日志事件的 `LogType` 属性或默认 `System` |

---

##### UserLogEnricher

**说明**: 用户信息日志增强器，从 `IHttpContextAccessor` 提取当前请求的用户和请求信息。

**构造函数**: `UserLogEnricher(IHttpContextAccessor httpContextAccessor)`

| 添加的属性 | 来源 |
|-----------|------|
| `UserId` | `ClaimTypes.NameIdentifier` |
| `UserName` | `ClaimTypes.Name` |
| `TenantId` | `TenantId` 声明 |
| `RequestId` | `HttpContext.TraceIdentifier` |
| `CorrelationId` | 请求头 `X-Correlation-ID` |
| `RequestPath` | `HttpContext.Request.Path` |
| `RequestMethod` | `HttpContext.Request.Method` |
| `ClientIp` | `HttpContext.Connection.RemoteIpAddress` |
| `UserAgent` | 请求头 `User-Agent` |

---

##### CorrelationIdEnricher

**说明**: 关联ID日志增强器，用于跨服务链路追踪。

**构造函数**: `CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)`

| 添加的属性 | 来源 |
|-----------|------|
| `CorrelationId` | 请求头 `X-Correlation-ID`（不存在时使用 `HttpContext.TraceIdentifier`） |
| `BusinessId` | 请求头 `X-Business-ID`（可选） |

---

##### PerformanceLogEnricher

**说明**: 性能日志增强器，当日志类型为 `Performance` 时额外添加内存使用信息。

| 添加的属性 | 条件 | 说明 |
|-----------|------|------|
| `MemoryUsedMB` | `LogType == "Performance"` | 当前进程工作集内存（MB） |

**使用示例**:
```csharp
// 在 SerilogSetup.AddSerilog 中自动注册了所有 Enricher：
// .Enrich.With(new LogEnricher())
// .Enrich.With(new ModuleLogEnricher())
// .Enrich.With(new LogTypeEnricher())
// .Enrich.With(new UserLogEnricher(httpContextAccessor))
// .Enrich.With(new CorrelationIdEnricher(httpContextAccessor))

// 手动注册示例（独立使用）：
new LoggerConfiguration()
    .Enrich.With(new LogEnricher())
    .Enrich.With(new LogTypeEnricher())
    .WriteTo.Console()
    .CreateLogger();
```

#### 3.6.9 WMSErrorCodes / WMSErrorMessages

**命名空间**: `KH.WMS.Core.Logging.WMSError`

---

##### WMSErrorCodes

**说明**: WMS 错误代码定义静态类，所有代码为 `string` 常量，按业务区域分组。

**通用错误 (0001-0099):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `SUCCESS` | `"0000"` | 操作成功 |
| `SYSTEM_ERROR` | `"0001"` | 系统错误 |
| `PARAM_ERROR` | `"0002"` | 参数错误 |
| `DATA_NOT_EXIST` | `"0003"` | 数据不存在 |
| `DATA_ALREADY_EXIST` | `"0004"` | 数据已存在 |
| `DATA_STATUS_ERROR` | `"0005"` | 数据状态异常 |
| `PERMISSION_DENIED` | `"0006"` | 权限不足 |
| `OPERATION_TIMEOUT` | `"0007"` | 操作超时 |
| `CONCURRENCY_CONFLICT` | `"0008"` | 并发冲突 |

**入库管理 (0101-0199):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `INBOUND_NOT_EXIST` | `"0101"` | 入库单不存在 |
| `INBOUND_STATUS_ERROR` | `"0102"` | 入库单状态不允许操作 |
| `INBOUND_DETAIL_NOT_EXIST` | `"0103"` | 入库单明细不存在 |
| `RECEIVE_QTY_MISMATCH` | `"0104"` | 收货数量与ASN不符 |
| `SKU_NOT_IN_ASN` | `"0105"` | 物料未在ASN中 |
| `RECEIVE_LOCATION_NOT_EXIST` | `"0106"` | 收货库位不存在 |
| `RECEIVE_LOCATION_TYPE_ERROR` | `"0107"` | 收货库位类型错误 |
| `PUT_AWAY_NOT_EXIST` | `"0108"` | 上架指令不存在 |
| `PUT_AWAY_LOCATION_NOT_EXIST` | `"0109"` | 上架库位不存在 |
| `PUT_AWAY_LOCATION_FULL` | `"0110"` | 上架库位容量不足 |
| `QC_FAILED` | `"0111"` | 质检不合格 |
| `QC_TASK_NOT_EXIST` | `"0112"` | 待质检任务不存在 |
| `ASN_NOT_EXIST` | `"0113"` | ASN不存在 |
| `ASN_ALREADY_EXIST` | `"0114"` | ASN已存在 |
| `ASN_STATUS_ERROR` | `"0115"` | ASN状态不允许操作 |

**出库管理 (0201-0299):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `OUTBOUND_NOT_EXIST` | `"0201"` | 出库单不存在 |
| `OUTBOUND_STATUS_ERROR` | `"0202"` | 出库单状态不允许操作 |
| `OUTBOUND_DETAIL_NOT_EXIST` | `"0203"` | 出库单明细不存在 |
| `ALLOCATION_FAILED` | `"0204"` | 分配失败 |
| `INVENTORY_SHORTAGE` | `"0205"` | 库存不足 |
| `WAVE_NOT_EXIST` | `"0206"` | 波次不存在 |
| `WAVE_STATUS_ERROR` | `"0207"` | 波次状态不允许操作 |
| `PICK_TASK_NOT_EXIST` | `"0208"` | 拣货任务不存在 |
| `PICK_TASK_STATUS_ERROR` | `"0209"` | 拣货任务状态不允许操作 |
| `PICK_LOCATION_NOT_EXIST` | `"0210"` | 拣货库位不存在 |
| `PICK_QTY_ERROR` | `"0211"` | 拣货数量异常 |
| `PACK_TASK_NOT_EXIST` | `"0212"` | 复核任务不存在 |
| `PACK_QTY_MISMATCH` | `"0213"` | 复核数量与订单不符 |
| `SHIPMENT_FAILED` | `"0214"` | 发货失败 |
| `TRACKING_NO_ALREADY_EXIST` | `"0215"` | 运单号已存在 |

**库存管理 (0301-0399):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `INVENTORY_NOT_EXIST` | `"0301"` | 库存不存在 |
| `INVENTORY_LOCKED` | `"0302"` | 库存被锁定 |
| `INVENTORY_QTY_INSUFFICIENT` | `"0303"` | 库存数量不足 |
| `BATCH_NOT_EXIST` | `"0304"` | 批次号不存在 |
| `BATCH_EXPIRED` | `"0305"` | 批次已过期 |
| `BATCH_EXPIRING_SOON` | `"0306"` | 批次即将过期 |
| `SERIAL_NO_ALREADY_EXIST` | `"0307"` | 序列号已存在 |
| `SERIAL_NO_NOT_EXIST` | `"0308"` | 序列号不存在 |
| `SERIAL_NO_ALREADY_ASSIGNED` | `"0309"` | 序列号已分配 |
| `INVENTORY_FREEZE_FAILED` | `"0310"` | 库存冻结失败 |
| `INVENTORY_UNFREEZE_FAILED` | `"0311"` | 库存解冻失败 |
| `INVENTORY_ADJUST_FAILED` | `"0312"` | 库存调整失败 |

**库位管理 (0401-0499):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `WAREHOUSE_NOT_EXIST` | `"0401"` | 仓库不存在 |
| `ZONE_NOT_EXIST` | `"0402"` | 库区不存在 |
| `LOCATION_NOT_EXIST` | `"0403"` | 库位不存在 |
| `LOCATION_ALREADY_EXIST` | `"0404"` | 库位已存在 |
| `LOCATION_OCCUPIED` | `"0405"` | 库位已占用 |
| `LOCATION_FULL` | `"0406"` | 库位已满 |
| `LOCATION_TYPE_ERROR` | `"0407"` | 库位类型错误 |
| `LOCATION_STATUS_ERROR` | `"0408"` | 库位状态错误 |
| `LOCATION_CAPACITY_INSUFFICIENT` | `"0409"` | 库位容量不足 |
| `LOCATION_WEIGHT_EXCEEDED` | `"0410"` | 库位重量超限 |
| `LOCATION_VOLUME_EXCEEDED` | `"0411"` | 库位体积超限 |
| `LOCATION_MIXING_RULE_CONFLICT` | `"0412"` | 库位混合存储规则冲突 |
| `LOCATION_SKU_LIMIT_CONFLICT` | `"0413"` | 库位物料限制冲突 |

**盘点管理 (0501-0599):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `COUNTING_NOT_EXIST` | `"0501"` | 盘点单不存在 |
| `COUNTING_STATUS_ERROR` | `"0502"` | 盘点单状态不允许操作 |
| `COUNTING_DETAIL_NOT_EXIST` | `"0503"` | 盘点明细不存在 |
| `COUNTING_DIFFERENCE_TOO_LARGE` | `"0504"` | 盘点差异过大 |
| `COUNTING_TASK_NOT_EXIST` | `"0505"` | 盘点任务不存在 |
| `COUNTING_TASK_ALREADY_COMPLETED` | `"0506"` | 盘点任务已完成 |
| `COUNTING_ADJUST_FAILED` | `"0507"` | 盘点盈亏处理失败 |
| `CYCLE_COUNT_PLAN_NOT_EXIST` | `"0508"` | 循环盘点计划不存在 |
| `COUNTING_FREEZE_FAILED` | `"0509"` | 盘点冻结失败 |

**移库管理 (0601-0699):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `TRANSFER_NOT_EXIST` | `"0601"` | 移库单不存在 |
| `TRANSFER_STATUS_ERROR` | `"0602"` | 移库单状态不允许操作 |
| `TRANSFER_TASK_NOT_EXIST` | `"0603"` | 移库指令不存在 |
| `SOURCE_LOCATION_NOT_EXIST` | `"0604"` | 源库位不存在 |
| `TARGET_LOCATION_NOT_EXIST` | `"0605"` | 目标库位不存在 |
| `SOURCE_LOCATION_EMPTY` | `"0606"` | 源库位无库存 |
| `TARGET_LOCATION_FULL` | `"0607"` | 目标库位已满 |
| `TRANSFER_QTY_ERROR` | `"0608"` | 移库数量异常 |
| `INVENTORY_MOVE_FAILED` | `"0609"` | 库存移动失败 |

**物料管理 (0701-0799):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `SKU_NOT_EXIST` | `"0701"` | 物料不存在 |
| `SKU_ALREADY_EXIST` | `"0702"` | 物料已存在 |
| `BARCODE_NOT_EXIST` | `"0703"` | 物料条码不存在 |
| `BARCODE_ALREADY_EXIST` | `"0704"` | 物料条码已存在 |
| `SKU_MASTER_NOT_EXIST` | `"0705"` | 物料主数据不存在 |
| `SKU_CATEGORY_NOT_EXIST` | `"0706"` | 物料分类不存在 |
| `SKU_ATTRIBUTE_NOT_EXIST` | `"0707"` | 物料属性不存在 |
| `SKU_PACKAGING_NOT_EXIST` | `"0708"` | 物料包装不存在 |
| `SKU_STRATEGY_NOT_EXIST` | `"0709"` | 物料策略不存在 |

**货主管理 (0801-0899):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `OWNER_NOT_EXIST` | `"0801"` | 货主不存在 |
| `OWNER_ALREADY_EXIST` | `"0802"` | 货主已存在 |
| `OWNER_STATUS_ERROR` | `"0803"` | 货主状态不允许操作 |
| `SUPPLIER_NOT_EXIST` | `"0804"` | 供应商不存在 |
| `SUPPLIER_ALREADY_EXIST` | `"0805"` | 供应商已存在 |
| `CUSTOMER_NOT_EXIST` | `"0806"` | 客户不存在 |
| `CUSTOMER_ALREADY_EXIST` | `"0807"` | 客户已存在 |

**策略管理 (901-999):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `STRATEGY_NOT_EXIST` | `"901"` | 策略不存在 |
| `STRATEGY_CONFLICT` | `"902"` | 策略冲突 |
| `PUT_AWAY_STRATEGY_NOT_EXIST` | `"903"` | 上架策略不存在 |
| `PICK_STRATEGY_NOT_EXIST` | `"904"` | 拣货策略不存在 |
| `ALLOCATION_STRATEGY_NOT_EXIST` | `"905"` | 分配策略不存在 |
| `REPLENISHMENT_STRATEGY_NOT_EXIST` | `"906"` | 补货策略不存在 |
| `WAVE_STRATEGY_NOT_EXIST` | `"907"` | 波次策略不存在 |
| `ROUTE_STRATEGY_NOT_EXIST` | `"908"` | 路径策略不存在 |
| `ABC_STRATEGY_NOT_EXIST` | `"909"` | ABC策略不存在 |

**系统集成 (1001-1099):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `ERP_API_FAILED` | `"1001"` | ERP接口调用失败 |
| `TMS_API_FAILED` | `"1002"` | TMS接口调用失败 |
| `INTERFACE_DATA_FORMAT_ERROR` | `"1003"` | 接口数据格式错误 |
| `INTERFACE_DATA_VALIDATION_FAILED` | `"1004"` | 接口数据验证失败 |
| `INTERFACE_TIMEOUT` | `"1005"` | 接口超时 |
| `INTERFACE_CONNECTION_FAILED` | `"1006"` | 接口连接失败 |

**任务管理 (1101-1199):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `TASK_NOT_EXIST` | `"1101"` | 任务不存在 |
| `TASK_STATUS_ERROR` | `"1102"` | 任务状态不允许操作 |
| `TASK_NOT_PUTAWAY` | `"1103"` | 上架任务未分配货位 |
| `TASK_CANCEL_FAILED` | `"1104"` | 任务取消失败 |
| `TASK_COMPLETE_FAILED` | `"1105"` | 任务完成处理失败 |
| `TASK_NOT_LINKED_OUTBOUND` | `"1106"` | 拣货任务未关联出库单 |
| `TASK_PICK_COMPLETE_FAILED` | `"1107"` | 拣货完成处理失败 |
| `TASK_LOCATION_NOT_ALLOCATED` | `"1108"` | 任务不是上架任务，无法分配货位 |
| `TASK_LOCATION_ALLOCATE_FAILED` | `"1109"` | 货位分配失败 |
| `TASK_LOCATION_NO_AVAILABLE` | `"1110"` | 库区无可用空闲库位 |
| `TASK_LOCATION_STRATEGY_FAILED` | `"1111"` | 货位分配策略执行失败 |
| `TASK_AT_LEAST_ONE_ID` | `"1112"` | TaskNo和WcsTaskNo至少提供一个 |
| `TASK_EXCEPTION_OCCURRED` | `"1113"` | 任务执行异常 |

**组盘管理 (1201-1299):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `CONTAINER_BIND_DATA_EMPTY` | `"1201"` | 组盘数据不能为空 |
| `CONTAINER_BIND_DETAIL_NOT_EXIST` | `"1202"` | 入库单明细不存在 |
| `CONTAINER_BIND_FAILED` | `"1203"` | 组盘失败 |
| `CONTAINER_BIND_RECORD_NOT_EXIST` | `"1204"` | 组盘记录不存在 |
| `CONTAINER_BIND_STATUS_ERROR` | `"1205"` | 容器状态不允许上架 |
| `CONTAINER_BIND_PUTAWAY_FAILED` | `"1206"` | 请求上架失败 |
| `CONTAINER_CODE_EMPTY` | `"1207"` | 容器编号不能为空 |
| `INBOUND_STATION_CODE_EMPTY` | `"1208"` | 入库口编号不能为空 |
| `CONTAINER_NO_BOUND_RECORD` | `"1209"` | 容器没有处于已组盘状态的记录 |
| `CONTAINER_BIND_WCS_PUTAWAY_FAILED` | `"1210"` | WCS申请上架失败 |
| `CONTAINER_BIND_SELECT_REQUIRED` | `"1211"` | 请选择要上架的组盘记录 |
| `CONTAINER_BIND_NO_WAREHOUSE` | `"1212"` | 容器未关联仓库 |
| `CONTAINER_BIND_PUTAWAY_TASK_FAILED` | `"1213"` | 创建上架任务失败 |
| `CONTAINER_HAS_ACTIVE_BIND` | `"1214"` | 容器已有活跃组盘记录 |
| `CONTAINER_HAS_INVENTORY` | `"1215"` | 容器已有库存记录，不允许重复组盘 |
| `CONTAINER_STATUS_ERROR` | `"1216"` | 容器状态不允许操作 |
| `CONTAINER_HAS_ACTIVE_TASK` | `"1217"` | 容器已有活跃任务 |
| `INVENTORY_FROZEN` | `"1218"` | 库存已冻结，不允许操作 |

**分配管理 (1301-1399):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `ALLOCATION_NO_DETAIL` | `"1301"` | 出库单无明细行 |
| `ALLOCATION_SINGLE_LINE_FAILED` | `"1302"` | 单行分配失败 |
| `ALLOCATION_DETAIL_NOT_EXIST` | `"1303"` | 库存明细不存在 |
| `ALLOCATION_INVENTORY_SHORTAGE_DETAIL` | `"1304"` | 物料库存不足（含明细） |
| `ALLOCATION_TASK_GENERATE_FAILED` | `"1305"` | 生成出库任务失败 |
| `ALLOCATION_HEAD_NOT_EXIST` | `"1306"` | 分配头不存在 |
| `ALLOCATION_NO_NEED_PROCESS` | `"1307"` | 任务无需处理的分配明细 |
| `ALLOCATION_PICK_COMPLETE_INFO` | `"1308"` | 拣货任务完成处理信息 |
| `ALLOCATE_FAILED` | `"1309"` | 分配失败（通用） |

**无单据操作 (1401-1499):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `ADHOC_OPERATION_FAILED` | `"1401"` | 无单据操作失败 |
| `ADHOC_FROM_LOCATION_NOT_EXIST` | `"1402"` | 起始库位不存在 |
| `ADHOC_FROM_LOCATION_STATUS_ERROR` | `"1403"` | 起始库位状态不允许操作 |
| `ADHOC_TO_LOCATION_STATUS_ERROR` | `"1404"` | 目的库位状态不允许操作 |
| `ADHOC_CONTAINER_NO_INVENTORY` | `"1405"` | 容器无库存记录 |
| `ADHOC_LOCATION_NO_INVENTORY` | `"1406"` | 货位无库存记录 |
| `ADHOC_NO_AVAILABLE_INVENTORY` | `"1407"` | 无可用库存可出库 |
| `ADHOC_INVENTORY_LOCK_FAILED` | `"1408"` | 库存锁定失败 |
| `ADHOC_FROM_LOCATION_CODE_EMPTY` | `"1409"` | 起始位置编码不能为空 |
| `ADHOC_TO_LOCATION_CODE_EMPTY` | `"1410"` | 目的位置编码不能为空 |
| `ADHOC_LOCATION_CODE_EMPTY` | `"1411"` | 货位编码不能为空 |
| `ADHOC_INBOUND_LINES_EMPTY` | `"1412"` | 无单据入库组盘物料不能为空 |
| `ADHOC_OUTBOUND_LINES_EMPTY` | `"1413"` | 无单据出库物料不能为空 |

**配置管理 (1501-1599):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `CONFIG_NOT_FOUND` | `"1501"` | 配置项不存在 |
| `CONTAINER_MIXED_BATCH` | `"1502"` | 托盘不允许绑定不同批次 |
| `CONTAINER_MIXED_MATERIAL` | `"1503"` | 托盘不允许绑定不同物料 |
| `BATCH_NO_REQUIRED` | `"1504"` | 启用批次管理时批次号必填 |
| `EXPIRY_DATE_REQUIRED` | `"1505"` | 启用有效期管理时有效期必填 |
| `OVER_PICK_NOT_ALLOWED` | `"1506"` | 不允许拣货数量大于计划数量 |

**成功消息 (9001-9099):**
| 常量名 | 代码 | 说明 |
|--------|------|------|
| `RECEIVE_SUCCESS` | `"9001"` | 接收成功 |
| `INBOUND_RECEIVE_SUCCESS` | `"9002"` | 收货成功 |
| `RECEIVE_AND_BIND_SUCCESS` | `"9003"` | 收货并组盘成功 |
| `CONTAINER_BIND_CREATED` | `"9004"` | 组盘创建成功 |
| `PUTAWAY_TASK_CREATED` | `"9005"` | 上架任务创建成功 |
| `OUTBOUND_CONFIRM_SUCCESS` | `"9006"` | 出库单确认成功 |
| `ALLOCATION_SUCCESS` | `"9007"` | 分配成功 |
| `OUTBOUND_TASK_CREATED` | `"9008"` | 出库任务生成成功 |
| `TASK_COMPLETE_SUCCESS` | `"9009"` | 任务完成成功 |
| `TASK_CANCEL_SUCCESS` | `"9010"` | 任务已取消 |
| `TASK_LOCATION_ALLOCATE_SUCCESS` | `"9011"` | 货位分配成功 |
| `PUTAWAY_TASK_CREATED_WITH_NO` | `"9012"` | 上架任务创建成功（含任务号） |

---

##### WMSErrorMessages

**说明**: WMS 错误代码说明类，提供错误码到中文描述文本的映射。

##### `string GetMessage(string errorCode)`

**说明**: 根据错误代码获取中文描述

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `errorCode` | `string` | 是 | 错误代码（如 `"0101"`） |

**返回值**: `string` — 中文描述（未匹配时返回 `"未知错误"`）

---

##### `string GetMessage(string errorCode, params object[] args)`

**说明**: 根据错误代码获取中文描述（支持格式化参数）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `errorCode` | `string` | 是 | 错误代码 |
| `args` | `params object[]` | 否 | 格式化参数 |

**返回值**: `string` — 格式化后的中文描述

**使用示例**:
```csharp
// 简单获取
string msg = WMSErrorMessages.GetMessage(WMSErrorCodes.INBOUND_NOT_EXIST);
// 返回: "入库单不存在"

// 带参数获取
string msg2 = WMSErrorMessages.GetMessage(WMSErrorCodes.TASK_NOT_EXIST, "TASK-001");
// 返回: "任务不存在: TASK-001"

string msg3 = WMSErrorMessages.GetMessage(WMSErrorCodes.CONTAINER_BIND_PUTAWAY_TASK_FAILED, "CNTR-001", "库存不足");
// 返回: "创建上架任务失败（容器 CNTR-001）: 库存不足"
```

### 3.7 Security 安全

#### 3.7.1 IEncryptionService / AesEncryptionService

**命名空间**: `KH.WMS.Core.Security.Encryption`

**说明**: AES 对称加密服务接口及实现（CBC 模式 + PKCS7 填充）。构造函数通过 SHA256 将密钥字符串散列为 256 位密钥，IV 默认取密钥前 16 字节。

**构造函数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 密钥字符串（经 SHA256 派生为 256 位） |
| `iv` | `string?` | 否 | 初始化向量（默认取密钥前 16 字节） |

---

##### `string Encrypt(string plainText)`

**说明**: 加密字符串，返回 Base64 编码密文

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `plainText` | `string` | 是 | 明文内容 |

**返回值**: `string` — Base64 编码的密文

---

##### `string Decrypt(string cipherText)`

**说明**: 解密 Base64 编码密文，返回原始明文

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `cipherText` | `string` | 是 | Base64 编码的密文 |

**返回值**: `string` — 解密后的明文

---

##### `byte[] EncryptBytes(byte[] plainBytes)`

**说明**: 加密字节数组

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `plainBytes` | `byte[]` | 是 | 明文字节数组 |

**返回值**: `byte[]` — 加密后的字节数组

---

##### `byte[] DecryptBytes(byte[] cipherBytes)`

**说明**: 解密字节数组

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `cipherBytes` | `byte[]` | 是 | 密文字节数组 |

**返回值**: `byte[]` — 解密后的明文字节数组

---

**算法细节**:
| 配置项 | 值 |
|--------|------|
| 算法 | AES (Rijndael) |
| 密钥长度 | 256 位（由 SHA256 派生） |
| 加密模式 | CBC |
| 填充模式 | PKCS7 |

**使用示例**:
```csharp
// 注入
public class MyService
{
    private readonly IEncryptionService _encryption;

    public MyService(IEncryptionService encryption)
    {
        _encryption = encryption;
    }

    public async Task ProcessAsync()
    {
        var encrypted = _encryption.Encrypt("Hello World");
        var decrypted = _encryption.Decrypt(encrypted);
        // decrypted == "Hello World"
    }
}

// 或直接实例化
var aes = new AesEncryptionService("my-secret-key");
var cipher = aes.Encrypt("sensitive data");
var plain = aes.Decrypt(cipher);
```

#### 3.7.2 IRsaCryptoService / RsaCryptoService

**命名空间**: `KH.WMS.Core.Security.Encryption`

**说明**: RSA 非对称加密服务接口及实现（登录密码加密专用）。注册为 Singleton 生命周期，内部持有 2048 位 RSA 密钥对，运行时仅暴露公钥用于前端加密，私钥解密在服务端完成。

**接口方法**:

---

##### `string GetPublicKey()`

**说明**: 获取 PEM 格式公钥（PKCS#1 格式，兼容 jsencrypt 前端库）

**返回值**: `string` — PEM 格式公钥字符串

---

##### `string Decrypt(string cipherText)`

**说明**: 使用私钥解密 Base64 编码密文

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `cipherText` | `string` | 是 | Base64 编码的 RSA 密文 |

**返回值**: `string` — 解密后的明文

---

**算法细节**:
| 配置项 | 值 |
|--------|------|
| 密钥长度 | 2048 位 |
| 填充方案 | PKCS#1 v1.5 |
| 公钥格式 | PKCS#1 PEM（`-----BEGIN RSA PUBLIC KEY-----`） |
| 生命周期 | Singleton（密钥对在应用启动时生成，运行期间不变） |

**典型用途**:
1. 前端调用 `GetPublicKey()` 获取公钥
2. 前端使用 jsencrypt 用公钥加密密码
3. 后端调用 `Decrypt()` 解密获取明文密码

**使用示例**:
```csharp
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IRsaCryptoService _rsa;

    public AuthController(IRsaCryptoService rsa)
    {
        _rsa = rsa;
    }

    [HttpGet("public-key")]
    public IActionResult GetPublicKey()
    {
        var publicKey = _rsa.GetPublicKey();
        return Ok(new { publicKey });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var password = _rsa.Decrypt(request.EncryptedPassword);
        // 使用解密后的密码进行验证...
    }
}
```

#### 3.7.3 IHashService / PasswordHasher

**命名空间**: `KH.WMS.Core.Security.Hashing`

**说明**: 哈希服务接口及 PBKDF2 密码哈希实现。`PasswordHasher` 使用 PBKDF2 + SHA256 对密码进行加盐哈希，输出格式为 `Base64(Salt + Hash)`。

**默认配置**:

| 参数 | 值 | 说明 |
|------|------|------|
| `SaltSize` | 16 字节 | 随机盐值长度 |
| `HashSize` | 32 字节 | 哈希输出长度 |
| `Iterations` | 10000 次 | PBKDF2 迭代次数 |

---

##### `string Hash(string plainText)`

**说明**: 对明文进行 PBKDF2 加盐哈希

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `plainText` | `string` | 是 | 待哈希的明文（如密码） |

**返回值**: `string` — Base64 编码的 `盐值(16B) + 哈希(32B)`

---

##### `bool Verify(string plainText, string hash)`

**说明**: 验证明文与哈希是否匹配

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `plainText` | `string` | 是 | 待验证的明文 |
| `hash` | `string` | 是 | 之前由 `Hash()` 产生的哈希值 |

**返回值**: `bool` — `true` 表示匹配

---

##### `string HashBytes(byte[] plainBytes)`

**说明**: 对字节数组进行 SHA256 哈希

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `plainBytes` | `byte[]` | 是 | 待哈希的字节数组 |

**返回值**: `string` — Base64 编码的 SHA256 哈希值

---

##### `string Hmac(string plainText, string key)`

**说明**: 使用 HMAC-SHA256 对明文签名

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `plainText` | `string` | 是 | 待签名的明文 |
| `key` | `string` | 是 | HMAC 密钥 |

**返回值**: `string` — Base64 编码的 HMAC-SHA256 签名

---

**使用示例**:
```csharp
public class UserService
{
    private readonly IHashService _hasher;

    public UserService(IHashService hasher)
    {
        _hasher = hasher;
    }

    public async Task<string> CreateUserAsync(string username, string password)
    {
        var hashedPassword = _hasher.Hash(password);
        // 存储 hashedPassword 到数据库
        return hashedPassword;
    }

    public async Task<bool> ValidatePasswordAsync(string password, string storedHash)
    {
        return _hasher.Verify(password, storedHash);
    }
}
```

---

##### 其他哈希实现

除 `PasswordHasher` 外，还提供以下 `IHashService` 实现：

| 类名 | 说明 |
|------|------|
| `Sha256Hasher` | 纯 SHA256 哈希器，适用于非密码场景（如签名校验） |
| `Md5Hasher` | MD5 哈希器（输出十六进制小写字符串），**不推荐用于安全场景**，仅用于兼容遗留系统 |

#### 3.7.4 IRateLimitService / SlidingWindowRateLimiter

**命名空间**: `KH.WMS.Core.Security.RateLimiting`

**说明**: 限流服务接口及多种限流算法实现，基于 `ICacheService`（内存缓存）存储请求计数。

**接口方法**:

---

##### `Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window)`

**说明**: 检查是否允许当前请求通过

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 限流标识（如 IP、用户ID、API 路径） |
| `limit` | `int` | 是 | 时间窗口内允许的最大请求数 |
| `window` | `TimeSpan` | 是 | 滑动时间窗口 |

**返回值**: `Task<bool>` — `true` 表示允许通过，`false` 表示已被限流

---

##### `Task<int> GetCurrentCountAsync(string key)`

**说明**: 获取当前窗口内的请求计数

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 限流标识 |

**返回值**: `Task<int>` — 当前窗口内的请求数量

---

##### `Task ResetAsync(string key)`

**说明**: 重置指定键的限流计数

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 限流标识 |

---

##### `Task<int> GetRemainingQuotaAsync(string key, int limit)`

**说明**: 获取当前剩余配额

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `key` | `string` | 是 | 限流标识 |
| `limit` | `int` | 是 | 配额上限 |

**返回值**: `Task<int>` — 剩余可用请求数

---

**限流算法实现**:

| 实现类 | 算法 | 说明 |
|--------|------|------|
| `SlidingWindowRateLimiter` | 滑动窗口 | 记录窗口内所有请求时间戳，过期自动清理。**精确限流，推荐使用** |
| `FixedWindowRateLimiter` | 固定窗口 | 按时间片分割计数。实现简单但窗口边界处可能出现突发流量 |
| `TokenBucketRateLimiter` | 令牌桶 | 以恒定速率补充令牌，支持突发流量。桶容量 = `limit`，补充速率 = `limit / window.TotalSeconds` |

**配置选项**:

```csharp
public class RateLimitOptions
{
    public int RequestLimit { get; set; } = 100;   // 时间窗口内允许的请求数
    public int WindowSeconds { get; set; } = 60;    // 时间窗口（秒）
    public RateLimitStrategy Strategy { get; set; } = RateLimitStrategy.SlidingWindow;  // 限流策略
}

public enum RateLimitStrategy
{
    FixedWindow,     // 固定窗口
    SlidingWindow,   // 滑动窗口
    TokenBucket      // 令牌桶
}
```

**使用示例**:
```csharp
public class RateLimitedController : ControllerBase
{
    private readonly IRateLimitService _rateLimiter;

    public RateLimitedController(IRateLimitService rateLimiter)
    {
        _rateLimiter = rateLimiter;
    }

    [HttpGet("api/data")]
    public async Task<IActionResult> GetData()
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var isAllowed = await _rateLimiter.IsAllowedAsync(clientIp, 100, TimeSpan.FromMinutes(1));

        if (!isAllowed)
            return StatusCode(429, "请求过于频繁，请稍后再试");

        return Ok(new { data = "..." });
    }
}
```

### 3.8 Serialization 序列化

#### 3.8.1 IJsonSerializer

**命名空间**: `KH.WMS.Core.Serialization`

**说明**: JSON 序列化统一接口，提供两个实现：`SystemJsonSerializer`（基于 System.Text.Json）和 `NewtonsoftJsonSerializer`（基于 Newtonsoft.Json），默认使用 System.Text.Json。

---

##### `string Serialize<T>(T obj, bool indented = false)`

**说明**: 将对象序列化为 JSON 字符串

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `obj` | `T` | 是 | 待序列化的对象 |
| `indented` | `bool` | 否 | 是否格式化缩进（默认 `false`） |

**返回值**: `string` — JSON 字符串

---

##### `void Serialize<T>(T obj, Stream stream, bool indented = false)`

**说明**: 将对象序列化为 JSON 并写入流

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `obj` | `T` | 是 | 待序列化的对象 |
| `stream` | `Stream` | 是 | 输出流 |
| `indented` | `bool` | 否 | 是否格式化缩进 |

---

##### `T? Deserialize<T>(string json)`

**说明**: 从 JSON 字符串反序列化为指定类型

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `json` | `string` | 是 | JSON 字符串 |

**返回值**: `T?` — 反序列化后的对象，失败返回 `default`

---

##### `T? Deserialize<T>(Stream stream)`

**说明**: 从流反序列化为指定类型

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `stream` | `Stream` | 是 | 包含 JSON 的流 |

**返回值**: `T?` — 反序列化后的对象

---

##### `object? Deserialize(string json, Type type)`

**说明**: 从 JSON 字符串反序列化为指定运行时类型

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `json` | `string` | 是 | JSON 字符串 |
| `type` | `Type` | 是 | 目标类型 |

**返回值**: `object?` — 反序列化后的对象

---

**使用示例**:
```csharp
public class JsonExample
{
    private readonly IJsonSerializer _json;

    public JsonExample(IJsonSerializer json)
    {
        _json = json;
    }

    public void Demo()
    {
        var obj = new { Name = "张三", Age = 30 };
        var json = _json.Serialize(obj, indented: true);
        // json: {"name":"张三","age":30}

        var deserialized = _json.Deserialize<MyDto>(json);
    }
}
```

#### 3.8.2 JsonSetup / NewtonsoftSettings

**命名空间**: `KH.WMS.Core.Serialization.Json`

**说明**: JSON 序列化配置入口。`JsonSetup` 配置 System.Text.Json，`NewtonsoftSettings` 提供 Newtonsoft.Json 配置选项。

---

##### `JsonSetup.AddJsonConfiguration(this IServiceCollection services, IConfiguration configuration)`

**说明**: 从 `appsettings.json` 的 `"Json"` 节读取配置，注册 `JsonSerializerOptions` 单例并注册 `SystemJsonSerializer` 为 `IJsonSerializer` 实现。

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `services` | `IServiceCollection` | 是 | 服务集合 |
| `configuration` | `IConfiguration` | 是 | 配置对象 |

**返回值**: `IServiceCollection` — 链式调用

---

##### `JsonSetup.GetDefaultOptions()`

**说明**: 获取默认 `JsonSerializerOptions` 配置

**默认配置**:
| 配置项 | 值 | 说明 |
|--------|------|------|
| `PropertyNamingPolicy` | `CamelCase` | 属性命名驼峰转换 |
| `DefaultIgnoreCondition` | `WhenWritingNull` | 序列化时忽略 null 值 |
| `WriteIndented` | `false` | 不格式化缩进 |
| `Encoder` | `UnsafeRelaxedJsonEscaping` | 宽松转义（减少转义字符） |
| 默认转换器 | `DateTimeConverter`、`LongConverter`、`EnumConverter` | 内置自定义转换器 |

---

##### `NewtonsoftSettings.GetSettings()`

**说明**: 获取 Newtonsoft.Json 默认设置（开发/调试环境，格式化输出）

| 配置项 | 值 | 说明 |
|--------|------|------|
| `Formatting` | `Indented` | 缩进格式化 |
| `ContractResolver` | `CamelCasePropertyNamesContractResolver` | 驼峰命名 |
| `NullValueHandling` | `Ignore` | 忽略 null 值 |
| `DefaultValueHandling` | `Include` | 包含默认值 |
| `DateFormatString` | `"yyyy-MM-dd HH:mm:ss"` | 日期格式 |
| `ReferenceLoopHandling` | `Ignore` | 忽略循环引用 |
| `MissingMemberHandling` | `Ignore` | 忽略缺失成员 |
| 默认转换器 | `StringEnumConverter` | 枚举序列化为字符串 |

---

##### `NewtonsoftSettings.GetCompactSettings()`

**说明**: 获取紧凑模式设置（生产环境，无缩进）

与 `GetSettings()` 的区别仅在于 `Formatting = Formatting.None`。

---

**使用示例**:

**System.Text.Json 方式（推荐）**:
```csharp
// Program.cs
builder.Services.AddJsonConfiguration(builder.Configuration);

// appsettings.json
{
  "Json": {
    "PropertyNamingPolicy": "CamelCase",
    "DefaultIgnoreCondition": "WhenWritingNull"
  }
}
```

**Newtonsoft.Json 方式**:
```csharp
var settings = NewtonsoftSettings.GetSettings();
var serializer = new NewtonsoftJsonSerializer(settings);
var json = serializer.Serialize(data);
```

#### 3.8.3 BoolToIntConverter

**命名空间**: `KH.WMS.Core.Serialization.JsonConverters`

**说明**: 将 `int` 类型的序列化/反序列化与前端 `boolean` 值兼容。解决前端 switch/checkbox 组件传出 `true`/`false` 导致 int 字段绑定失败的问题。

**转换规则**:

| 输入类型 | 输入值 | 输出值 |
|----------|--------|--------|
| `True` | `true` | `1` (int) |
| `False` | `false` | `0` (int) |
| `Number` | `0` / `1` | 按原值读取 |
| `String` | `"0"` / `"1"` | 经 `int.TryParse` 转换 |

**序列化**: 始终输出整数值（`WriteNumberValue`）

**适用类型**:
| 转换器 | 目标类型 |
|--------|----------|
| `BoolToIntConverter` | `int` |
| `NullableBoolToIntConverter` | `int?` |

**使用示例**:
```csharp
var options = new JsonSerializerOptions();
options.Converters.Add(new BoolToIntConverter());

var json = @"{\"status\": true}";
var result = JsonSerializer.Deserialize<MyDto>(json, options);
// result.Status == 1
```

#### 3.8.4 DateTimeConverter

**命名空间**: `KH.WMS.Core.Serialization.JsonConverters`

**说明**: 自定义 DateTime 格式化转换器，默认格式为 `"yyyy-MM-dd HH:mm:ss"`。支持构造时指定自定义格式。

**构造函数**:

| 构造方法 | 说明 |
|----------|------|
| `DateTimeConverter()` | 默认格式 `"yyyy-MM-dd HH:mm:ss"` |
| `DateTimeConverter(string format)` | 指定自定义格式 |

**反序列化**: 接受任意标准日期时间字符串，自动 `DateTime.TryParse`

**序列化**: 按指定格式输出字符串，默认 `"yyyy-MM-dd HH:mm:ss"`

**适用类型**:
| 转换器 | 目标类型 |
|--------|----------|
| `DateTimeConverter` | `DateTime` |
| `DateTimeOffsetConverter` | `DateTimeOffset` |
| `NullableDateTimeConverter` | `DateTime?` |

**使用示例**:
```csharp
var options = new JsonSerializerOptions();
options.Converters.Add(new DateTimeConverter("yyyy/MM/dd"));

var obj = new { Time = new DateTime(2025, 1, 15, 10, 30, 0) };
var json = JsonSerializer.Serialize(obj, options);
// json: {\"time\":\"2025/01/15\"}
```

#### 3.8.5 EnumConverter

**命名空间**: `KH.WMS.Core.Serialization.JsonConverters`

**说明**: 将枚举类型序列化为字符串，反序列化时从字符串解析为枚举值。通过 `JsonConverterFactory` 模式实现，自动适配所有枚举类型。

**转换规则**:
| 方向 | 转换 |
|------|------|
| 序列化 | 枚举值 → 枚举名（字符串） |
| 反序列化 | 字符串 → 枚举值（忽略大小写） |

**适用类型**:
| 转换器 | 目标类型 |
|--------|----------|
| `EnumConverter` | 所有 `Enum` 类型（工厂类） |
| `EnumConverter<T>` | 指定枚举类型 `T` |
| `NullableEnumConverter<T>` | `T?` 可空枚举 |

**使用示例**:
```csharp
public enum OrderStatus { Pending, Processing, Completed }

var options = new JsonSerializerOptions();
options.Converters.Add(new EnumConverter());

// 序列化
var json = JsonSerializer.Serialize(OrderStatus.Processing, options);
// json: \"Processing\"

// 反序列化
var status = JsonSerializer.Deserialize<OrderStatus>(@\"\"processing\"\", options);
// status == OrderStatus.Processing
```

#### 3.8.6 LongConverter

**命名空间**: `KH.WMS.Core.Serialization.JsonConverters`

**说明**: 将 `long` 类型序列化为字符串，解决 JavaScript 无法精确处理超过 2^53 的 64 位整数的问题。前端 JS 环境中，超过 `Number.MAX_SAFE_INTEGER`（9007199254740991）的数值会丢失精度，此转换器确保长整型在前后端传输中保持精确。

**转换规则**:
| 方向 | 转换 |
|------|------|
| 序列化 | `long` 值 → 字符串（如 `12345678901234567890` → `"12345678901234567890"`） |
| 反序列化 | 字符串/数字 → `long` 值（自动 `long.TryParse`） |

**适用类型**:
| 转换器 | 目标类型 |
|--------|----------|
| `LongConverter` | `long` |
| `NullableLongConverter` | `long?` |

**使用示例**:
```csharp
var options = new JsonSerializerOptions();
options.Converters.Add(new LongConverter());

var obj = new { Id = 1234567890123456789L };
var json = JsonSerializer.Serialize(obj, options);
// json: {\"id\":\"1234567890123456789\"}  — 字符串格式，JS 安全

var deserialized = JsonSerializer.Deserialize<MyDto>(json, options);
// deserialized.Id == 1234567890123456789L
```

### 3.9 Mapping 对象映射

#### 3.9.1 IMappingService / MappingService

**命名空间**: `KH.WMS.Core.Mapping`

**说明**: 对象映射服务接口及实现（基于 AutoMapper）。`MappingService` 通过 `IServiceProvider` 获取 `IMapper` 实例，支持实体与 DTO 之间的自动映射。注册时跳过拦截器（`WithoutInterceptor = true`）。

---

##### `TDestination Map<TDestination>(object source)`

**说明**: 将源对象映射到目标类型（按名称自动匹配属性）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `source` | `object` | 是 | 源对象 |

**返回值**: `TDestination` — 映射后的目标对象

---

##### `TDestination Map<TSource, TDestination>(TSource source)`

**说明**: 将源对象映射到目标类型（显式指定源类型）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `source` | `TSource` | 是 | 源对象 |

**返回值**: `TDestination` — 映射后的目标对象

---

##### `TDestination Map<TSource, TDestination>(TSource source, TDestination destination)`

**说明**: 将源对象属性合并到现有目标对象实例

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `source` | `TSource` | 是 | 源对象 |
| `destination` | `TDestination` | 是 | 已有的目标实例（会被修改） |

**返回值**: `TDestination` — 更新后的目标对象

---

##### `List<TDestination> MapList<TSource, TDestination>(IEnumerable<TSource> source)`

**说明**: 映射集合，将源集合中的每个元素映射到目标类型

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `source` | `IEnumerable<TSource>` | 是 | 源集合 |

**返回值**: `List<TDestination>` — 映射后的目标集合

---

##### `List<TDestination> MapList<TDestination>(IEnumerable<object> source)`

**说明**: 映射集合（不指定源类型，通过运行时类型推断）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `source` | `IEnumerable<object>` | 是 | 源集合（`object` 类型） |

**返回值**: `List<TDestination>` — 映射后的目标集合

---

**使用示例**:
```csharp
public class MyService
{
    private readonly IMappingService _mapper;

    public MyService(IMappingService mapper)
    {
        _mapper = mapper;
    }

    public OrderDto GetOrder(Order entity)
    {
        // 实体 → DTO
        return _mapper.Map<OrderDto>(entity);
    }

    public Order MergeOrder(OrderDto dto, Order existing)
    {
        // 将 DTO 合并到现有实体
        return _mapper.Map(dto, existing);
    }

    public List<OrderDto> GetOrderList(List<Order> orders)
    {
        // 集合映射
        return _mapper.MapList<Order, OrderDto>(orders);
    }
}
```

#### 3.9.2 AutoMapperSetup

**命名空间**: `KH.WMS.Core.Mapping.AutoMapper`

**说明**: AutoMapper 配置入口，提供两个扩展方法用于注册 AutoMapper 到 DI 容器。自动扫描指定程序集中的所有 `Profile` 子类，在创建 `IMapper` 实例时会调用 `AssertConfigurationIsValid()` 校验映射配置的合法性。

---

##### `IServiceCollection AddAutoMapper(this IServiceCollection services, params Assembly[] assemblies)`

**说明**: 添加 AutoMapper 服务，从指定程序集扫描并注册所有 Profile

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `services` | `IServiceCollection` | 是 | 服务集合 |
| `assemblies` | `params Assembly[]` | 否 | 要扫描的程序集数组（默认为调用方程序集） |

**返回值**: `IServiceCollection` — 链式调用

**内部配置**:
| 配置项 | 值 | 说明 |
|--------|------|------|
| `AllowNullCollections` | `true` | 允许目标集合为 null |
| `AllowNullDestinationValues` | `true` | 允许目标值为 null |
| `ShouldMapField` | `false`（排除字段） | 不映射字段，仅映射属性 |
| `ShouldMapProperty` | 仅映射公开 get 属性 | 排除私有属性 |

---

##### `IServiceCollection AddAutoMapperProfiles(this IServiceCollection services, params Type[] profileTypes)`

**说明**: 添加 AutoMapper 服务，显式指定 Profile 类型（不扫描程序集）

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `services` | `IServiceCollection` | 是 | 服务集合 |
| `profileTypes` | `params Type[]` | 是 | 要注册的 Profile 类型数组 |

**返回值**: `IServiceCollection` — 链式调用

---

**使用示例**:
```csharp
// 方式一：扫描程序集（推荐）
// Program.cs
builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(OrderProfile).Assembly);

// 方式二：指定 Profile 类型
builder.Services.AddAutoMapperProfiles(typeof(OrderProfile), typeof(WarehouseProfile));

// 定义 Profile
public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<OrderDto, Order>()
            .ForMember(dest => dest.CreatedTime, opt => opt.Ignore());
    }
}

// 使用 IMappingService（AutoMapper 注册后自动生效）
public class OrderAppService
{
    private readonly IMappingService _mapping;

    public OrderAppService(IMappingService mapping)
    {
        _mapping = mapping;
    }

    public async Task<OrderDto> GetAsync(long id)
    {
        var entity = await _orderRepository.GetByIdAsync(id);
        return _mapping.Map<OrderDto>(entity);
    }
}
```

> **注意**: `AddAutoMapper` 注册的是 `IMapper` 单例，`IMappingService` 已通过 `[RegisteredService]` 自动注册到 DI 容器，无需手动添加。

### 3.10 License 许可证

#### 3.10.1 ILicenseService / LicenseService

**命名空间**: `KH.WMS.Core.License.Services`

**说明**: License 核心服务，底层实现 License 的验证、导入、机器码生成、首次启动默认授权。注册为 **Singleton** 生命周期，跳过 AOP 拦截器（`WithoutInterceptor = true`）。License 文件默认存储在 `{BaseDirectory}/System.Private.Core.dll`，公钥文件存储在 `System.Private.Security.dll`，私钥文件存储在 `System.Private.Crypto.dll`。

##### `string GetMachineCode()`

**说明**: 获取当前服务器的机器码（基于硬件指纹，缓存后不再重复采集）

**返回值**: `string` — 64 位大写十六进制机器码（SHA256）

---

##### `LicenseData? ValidateLicense()`

**说明**: 验证当前 License 是否有效，包含签名验证、机器码匹配、有效期检查。结果缓存 5 分钟（可通过 `License:ValidateIntervalMinutes` 配置）

**返回值**: `LicenseData?` — 有效时返回 License 数据，无效时返回 null

---

##### `bool ImportLicense(string licenseContent)`

**说明**: 导入 License（Base64 编码字符串）。先验证再写入文件，验证不通过则拒绝导入

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `licenseContent` | `string` | 是 | Base64 编码的 License 文件内容 |

**返回值**: `bool` — 导入成功返回 true

---

##### `LicenseData? GetCurrentLicense()`

**说明**: 获取当前 License 信息（仅解析文件内容，不验证签名和有效期）

**返回值**: `LicenseData?` — 未找到 License 文件时返回 null

---

##### `string? GetValidationErrorMessage()`

**说明**: 获取最近一次 License 验证失败的详细信息

**返回值**: `string?` — 错误描述，没有错误时返回 null

---

##### `void EnsureDefaultLicense()`

**说明**: 首次启动初始化：检查 License 文件是否存在，如不存在则自动生成 RSA 密钥对并签发 180 天默认 License（有效期可通过 `License:DefaultValidDays` 配置）

---

#### 3.10.2 ILicenseAppService / LicenseAppService

**命名空间**: `KH.WMS.Core.License.Services`

**说明**: License 应用服务，封装了面向表现层的业务逻辑（异常捕获、结果包装）。注册为 **Scoped** 生命周期。

##### `Result<string> GetMachineCode()`

**说明**: 获取机器码，异常时返回失败 Result

**返回值**: `Result<string>` — 成功时包含机器码

---

##### `Result<LicenseInfoDto> GetLicenseInfo()`

**说明**: 获取当前 License 详细信息（含有效期、剩余天数、是否过期等）

**返回值**: `Result<LicenseInfoDto>` — License 无效时返回失败 Result

---

##### `Result<string> GenerateLicenseFile(GenerateLicenseRequest request)`

**说明**: 生成 License 文件内容（Base64 编码）。需要读取服务器上的私钥 PEM 文件

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `request` | `GenerateLicenseRequest` | 是 | 生成请求，含 MachineCode、ValidDays、LicenseType |

**返回值**: `Result<string>` — 成功时包含 Base64 编码的 License 内容

---

##### `Result ImportLicense(ImportLicenseRequest request)`

**说明**: 导入 License

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `request` | `ImportLicenseRequest` | 是 | 导入请求，含 LicenseContent |

**返回值**: `Result` — 操作结果

---

#### 3.10.3 LicenseController

**命名空间**: `KH.WMS.Core.License.Controllers`

**路由**: `api/license`

**说明**: License 管理控制器，提供机器码查询、License 信息获取、生成和导入接口。所有接口均为异步方法。

##### `GET api/license/machine-code`

**说明**: 获取当前服务器的机器码

**响应示例**:
```json
{ "machineCode": "3E7A2B1C8F9D0E4F..." }
```

---

##### `GET api/license/info`

**说明**: 获取当前 License 信息

**响应示例**:
```json
{
  "machineCode": "3E7A2B1C...",
  "productName": "WMS-V2",
  "issuedAt": "2025-01-01T00:00:00",
  "expiresAt": "2025-06-29T00:00:00",
  "validDays": 180,
  "licenseType": "Standard",
  "isExpired": false,
  "remainingDays": 175,
  "licenseId": "a1b2c3d4-...",
  "isValid": true
}
```

---

##### `POST api/license/generate`

**说明**: 生成 License 文件（需要私钥，仅管理员使用）

**请求体** (`GenerateLicenseRequest`):
```json
{
  "machineCode": "3E7A2B1C...",
  "validDays": 365,
  "licenseType": "Standard"
}
```

**响应**:
```json
{
  "licenseContent": "eyJkYXRhIjp7...",
  "fileName": "license_3E7A2B1C_365days.lic"
}
```

---

##### `POST api/license/import`

**说明**: 导入 License 文件（JSON 体方式）

**请求体** (`ImportLicenseRequest`):
```json
{
  "licenseContent": "eyJkYXRhIjp7..."
}
```

**响应**:
```json
{ "message": "License 导入成功" }
```

---

##### `POST api/license/upload`

**说明**: 上传 License 文件（`multipart/form-data` 文件上传方式）。限制最大 10KB

**请求**: `IFormFile` — 上传的 .lic 文件

**响应**:
```json
{ "message": "License 导入成功" }
```

---

#### 3.10.4 MachineCodeGenerator

**命名空间**: `KH.WMS.Core.License.Crypto`

**说明**: 静态工具类，基于服务器硬件指纹（CPU ID + 主板序列号 + 硬盘序列号）生成唯一机器码。通过 WMI 查询采集硬件信息，拼接后计算 SHA256 哈希，输出大写十六进制字符串。

**依赖**: `System.Management`（Windows WMI）

**方法**:

##### `static string GetMachineCode()`

**说明**: 采集 CPU ProcessorId、主板 SerialNumber、第一块硬盘 SerialNumber，拼接后 SHA256 哈希

**返回值**: `string` — 64 位大写十六进制哈希值

**异常**: `InvalidOperationException` — 无法获取任何硬件信息时抛出

---

#### 3.10.5 RsaKeyHelper

**命名空间**: `KH.WMS.Core.License.Crypto`

**说明**: RSA 密钥工具类，提供 RSA 2048 位密钥对的生成、PEM 格式导入导出、数据签名与验证能力。

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `GenerateKeyPair()` | 生成 RSA 2048 密钥对 | `(string PublicKeyPem, string PrivateKeyPem)` |
| `CreateFromPublicKey(string pem)` | 从 PEM 公钥创建 RSA 实例 | `RSA` |
| `CreateFromPrivateKey(string pem)` | 从 PEM 私钥创建 RSA 实例 | `RSA` |
| `SignData(string data, RSA privateKey)` | 使用私钥对数据进行 SHA256+RSA 签名 | `string` (Base64) |
| `VerifyData(string data, string signatureBase64, RSA publicKey)` | 使用公钥验证签名 | `bool` |

**签名算法**: `SHA256` + `RSASignaturePadding.Pkcs1`

**密钥格式**:
- 公钥：`SubjectPublicKeyInfo` (PEM)
- 私钥：`PKCS#8` (PEM)

---

#### 3.10.6 RsaLicenseSigner

**命名空间**: `KH.WMS.Core.License.Crypto`

**说明**: License 签名器，使用 RSA 私钥生成签名的 License 文件。构造时需传入 `RSA` 私钥实例。

##### `RsaLicenseSigner(RSA privateKey)`

**构造参数**: `RSA privateKey` — RSA 私钥

---

##### `string GenerateLicense(string machineCode, int validDays, string licenseType = "Standard", string productName = "WMS-V2")`

**说明**: 生成 License 文件内容

| 参数 | 类型 | 必填 | 默认值 | 说明 |
|------|------|------|--------|------|
| `machineCode` | `string` | 是 | — | 目标机器码 |
| `validDays` | `int` | 是 | — | 有效天数 |
| `licenseType` | `string` | 否 | `Standard` | License 类型（Standard/Professional/Enterprise） |
| `productName` | `string` | 否 | `WMS-V2` | 产品名称 |

**返回值**: `string` — Base64 编码的 License 文件内容

**流程**: 构造 LicenseData → 序列化为 JSON → RSA 签名 → 组装 LicenseFile → JSON 序列化 → Base64 编码

---

##### `string GenerateLicense(LicenseData licenseData)`

**说明**: 从已构建的 LicenseData 生成 License 文件

**返回值**: `string` — Base64 编码的 License 文件内容

---

##### `void GenerateLicenseToFile(...)`

**说明**: 生成 License 并直接写入文件

| 参数 | 说明 |
|------|------|
| `machineCode` | 目标机器码 |
| `validDays` | 有效天数 |
| `outputPath` | 输出文件路径 |
| `licenseType` | License 类型（默认 Standard） |
| `productName` | 产品名称（默认 WMS-V2） |

---

#### 3.10.7 RsaLicenseVerifier

**命名空间**: `KH.WMS.Core.License.Crypto`

**说明**: License 验证器，使用 RSA 公钥验证 License 的签名、机器码和有效期。构造时需传入 `RSA` 公钥实例。

##### `RsaLicenseVerifier(RSA publicKey)`

**构造参数**: `RSA publicKey` — RSA 公钥

---

##### `LicenseData? Verify(string licenseContent, string currentMachineCode, out string? errorMessage)`

**说明**: 验证并解析 License 文件内容。依次执行：Base64 解码 → JSON 反序列化 → RSA 签名验证 → 机器码匹配 → 有效期检查

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `licenseContent` | `string` | 是 | Base64 编码的 License 内容 |
| `currentMachineCode` | `string` | 是 | 当前服务器的机器码，用于比对 |
| `errorMessage` | `string?` | out | 验证失败时的详细错误信息 |

**返回值**: `LicenseData?` — 验证成功返回 License 数据，失败返回 null

**错误场景**:
| 错误信息 | 原因 |
|----------|------|
| License 文件格式无效 | JSON 反序列化失败或无 Data 节点 |
| License 签名验证失败，文件可能已被篡改 | RSA 签名验证不通过 |
| License 机器码不匹配 | 文件中的机器码与当前服务器不一致 |
| License 已过期 | 当前时间超过 ExpiresAt |
| License 文件内容不是有效的 Base64 编码 | Base64 解码异常 |
| License 文件内容不是有效的 JSON 格式 | JSON 解析异常 |

---

#### 3.10.8 LicenseData / LicenseFile

**命名空间**: `KH.WMS.Core.License.Models`

**LicenseData** — License 数据模型，包含所有授权信息字段

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `MachineCode` | `string` | `""` | 绑定的服务器机器码 |
| `ProductName` | `string` | `"WMS-V2"` | 产品名称 |
| `IssuedAt` | `DateTime` | — | 签发时间 |
| `ExpiresAt` | `DateTime` | — | 过期时间 |
| `ValidDays` | `int` | — | 有效天数 |
| `LicenseType` | `string` | `"Standard"` | License 类型（Standard / Professional / Enterprise） |
| `LicenseId` | `Guid` | — | 唯一标识 |

**LicenseFile** — License 文件结构（最终序列化为 JSON 后 Base64 编码存储为 .lic 文件）

| 属性 | 类型 | 说明 |
|------|------|------|
| `Data` | `LicenseData` | License 数据 |
| `Signature` | `string` | RSA 签名（Base64 编码） |

**.lic 文件结构**:
```
Base64(
  JSON({
    "data": { "machineCode": "...", ... },
    "signature": "Base64Signature..."
  })
)
```

---

#### 3.10.9 GenerateLicenseRequest / ImportLicenseRequest / LicenseInfoDto

**命名空间**: `KH.WMS.Core.License.DTOs`

**GenerateLicenseRequest** — 生成 License 请求 DTO

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `MachineCode` | `string` | `""` | 目标机器码（必填） |
| `ValidDays` | `int` | — | 有效天数（必须大于 0） |
| `LicenseType` | `string` | `"Standard"` | License 类型 |

---

**ImportLicenseRequest** — 导入 License 请求 DTO

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `LicenseContent` | `string` | `""` | License 文件内容（Base64 编码字符串） |

---

**LicenseInfoDto** — License 信息响应 DTO

| 属性 | 类型 | 说明 |
|------|------|------|
| `MachineCode` | `string` | 绑定的机器码 |
| `ProductName` | `string` | 产品名称 |
| `IssuedAt` | `DateTime` | 签发时间 |
| `ExpiresAt` | `DateTime` | 过期时间 |
| `ValidDays` | `int` | 有效天数 |
| `LicenseType` | `string` | License 类型 |
| `IsExpired` | `bool` | 是否已过期 |
| `RemainingDays` | `int` | 剩余有效天数 |
| `LicenseId` | `Guid` | License 唯一标识 |
| `IsValid` | `bool` | 是否有效 |

---

#### 3.10.10 Result / Result\<T\>

**命名空间**: `KH.WMS.Core.License.Results`

**说明**: 统一的 API 操作结果封装，提供 Success / Failure 静态工厂方法。

**Result** — 无返回值操作结果

| 属性 | 类型 | 说明 |
|------|------|------|
| `IsSuccess` | `bool` | 是否成功 |
| `IsFailure` | `bool` | 是否失败（`!IsSuccess`） |
| `Error` | `string?` | 失败时的错误信息 |
| `Message` | `string?` | 成功时的提示消息 |

**静态工厂方法**:
| 方法 | 说明 |
|------|------|
| `Result.Success(string? message = null)` | 创建成功结果 |
| `Result.Failure(string error)` | 创建失败结果 |
| `Result<T>.Success(T value, string? message = null)` | 创建带值的成功结果 |
| `Result<T>.Failure(string error)` | 创建带值的失败结果 |

**使用示例**:
```csharp
// 成功
var result = Result.Success("操作完成");

// 失败
var error = Result.Failure("机器码不能为空");

// 带值
var data = Result<string>.Success(machineCode);
if (data.IsSuccess) {
    var code = data.Value;
}
```

### 3.11 AOP 与特性

#### 3.11.1 CacheAttribute

**命名空间**: `KH.WMS.Core.Attributes`

**说明**: 缓存特性，标记在方法或类上，由 `CachingInterceptor` 拦截处理。支持方法级别和类级别，类级别对该类所有方法生效。

**目标**: `Method` | `Class` (AllowMultiple = false)

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Duration` | `int` | `300` (5 分钟) | 缓存时长（秒） |
| `KeyPrefix` | `string?` | `null` | 缓存键前缀，为空时使用 `{ClassName}.{MethodName}` |
| `Enable` | `bool` | `true` | 是否启用缓存 |
| `IncludeUser` | `bool` | `false` | 缓存键中是否包含用户信息 |

**使用示例**:
```csharp
[Cache(Duration = 600, KeyPrefix = "wms")]
public async Task<List<WarehouseDto>> GetAllWarehouses()
{
    // 返回结果将被缓存 600 秒
}

// 类级别应用，对该类所有方法生效
[Cache(Duration = 60)]
public class LookupService : ILookupService { }
```

---

#### 3.11.2 TransactionAttribute

**命名空间**: `KH.WMS.Core.Attributes`

**说明**: 事务特性，标记在控制器或 Action 上，实现 `IFilterFactory`，由 MVC 管道自动创建 `TransactionActionFilter`。自动管理请求级别的事务（Begin / Commit / Rollback）。

**目标**: `Method` | `Class` (AllowMultiple = false)

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `IsolationLevel` | `IsolationLevel` | `ReadCommitted` | 事务隔离级别 |
| `Timeout` | `int` | `30` | 事务超时时间（秒） |

**使用示例**:
```csharp
[Transaction(IsolationLevel = IsolationLevel.Serializable)]
public async Task<IActionResult> CreateOrder(OrderDto order)
{
    // 此 Action 将自动在事务中执行
    // 异常时自动回滚，成功时自动提交
}
```

---

#### 3.11.3 LogInterceptorAttribute

**命名空间**: `KH.WMS.Core.Attributes`

**说明**: 日志拦截器特性，标记在类或方法上，由 `LoggingInterceptor` 处理。支持调用链追踪，在 Debug 级别记录方法进入/退出日志，包含调用深度缩进、TraceId、用户信息。

**目标**: `Class` | `Method` (AllowMultiple = true)

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `LogParameters` | `bool` | `true` | 是否记录方法参数 |
| `LogReturnValue` | `bool` | `false` | 是否记录返回值 |
| `LogExecutionTime` | `bool` | `true` | 是否记录执行时间 |
| `LogLevel` | `LogLevel` | `Information` | 日志级别 |

**使用示例**:
```csharp
[LogInterceptor(LogParameters = true, LogReturnValue = true)]
public async Task<UserDto> GetUser(long userId)
{
    // 调用时自动记录 "-> UserService.GetUser | User: admin | 参数: P0: 1001"
    // 返回时自动记录 "<- UserService.GetUser | 返回值: { ... }"
}
```

---

#### 3.11.4 RateLimitAttribute

**命名空间**: `KH.WMS.Core.Attributes`

**说明**: 限流特性，标记在方法或类上，用于接口限流。支持固定窗口、滑动窗口、令牌桶三种策略，可按 IP 或按用户限流。

**目标**: `Method` | `Class` (AllowMultiple = false)

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `RequestLimit` | `int` | `100` | 时间窗口内允许的最大请求数 |
| `TimeWindowSeconds` | `int` | `60` | 时间窗口（秒） |
| `KeyPrefix` | `string?` | `null` | 限流键前缀 |
| `ByIp` | `bool` | `true` | 是否按 IP 限流 |
| `ByUser` | `bool` | `false` | 是否按用户限流 |
| `Strategy` | `RateLimitStrategy` | `SlidingWindow` | 限流策略 |

**RateLimitStrategy 枚举**:

| 值 | 说明 |
|----|------|
| `FixedWindow` | 固定窗口，在每个时间窗口内计数，窗口翻转时重置 |
| `SlidingWindow` | 滑动窗口，基于时间戳精确计数，避免窗口边界突发 |
| `TokenBucket` | 令牌桶算法，支持突发流量 |

**使用示例**:
```csharp
[RateLimit(RequestLimit = 10, TimeWindowSeconds = 1, Strategy = RateLimitStrategy.TokenBucket)]
public async Task<IActionResult> SearchProduct(string keyword)
{
    // 每秒最多 10 次请求
}
```

---

#### 3.11.5 InterceptorBase

**命名空间**: `KH.WMS.Core.AOP`

**说明**: AOP 拦截器基类，实现 `Castle.DynamicProxy.IInterceptor`。封装了同步/异步方法拦截模板，自动判断方法是否为异步（`Task` / `Task<T>`），提供统一的 `BeforeExecute` / `AfterExecute` / `OnException` / `OnFinally` 扩展点。

**属性**:

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `EnableAsync` | `bool` | `true` | 是否启用异步支持 |

**生命周期钩子**（可重写）：

| 方法 | 说明 |
|------|------|
| `BeforeExecute(IInvocation, string)` | 执行前逻辑（同步） |
| `AfterExecute(IInvocation, string)` | 执行后逻辑（同步） |
| `OnException(IInvocation, string, Exception)` | 异常处理（同步） |
| `OnFinally(IInvocation, string)` | 清理逻辑（同步） |
| `BeforeExecuteAsync(IInvocation, string)` | 执行前逻辑（异步） |
| `AfterExecuteAsync(IInvocation, string)` | 执行后逻辑（异步） |
| `AfterExecuteAsyncWithResult<TResult>(IInvocation, string, TResult)` | 执行后逻辑（异步，带返回值） |
| `OnExceptionAsync(IInvocation, string, Exception)` | 异常处理（异步） |
| `OnFinallyAsync(IInvocation, string)` | 清理逻辑（异步） |

**说明**: 基类自动处理同步/异步方法差异：
- 同步方法：通过 `CallTraceContext.EnterScope()` 管理调用深度
- 异步方法：通过 `CallTraceContext.EnterScope()` + `await` 管理调用深度
- 方法链层级通过 `GetIndent(depth)` 返回缩进字符串

---

#### 3.11.6 CachingInterceptor

**命名空间**: `KH.WMS.Core.AOP.Interceptors`

**依赖**: `ILoggerService`, `ICacheService`

**说明**: 缓存拦截器，实现 `IInterceptor`。识别带有 `[Cache]` 特性的方法，自动缓存返回结果。缓存命中时直接返回缓存值，不执行原方法；仅对返回 `ApiResponse` 且 `Data != null` 的结果进行缓存。

**工作流程**:
1. 检查方法/类是否标注了 `[Cache]` 且 `Enable = true`
2. 生成缓存键：`{KeyPrefix}:{参数1}_{参数2}_...`（默认前缀为 `{ClassName}.{MethodName}`）
3. 查询缓存，命中则直接返回
4. 未命中则执行原方法，将结果写入缓存（仅缓存 `ApiResponse.Data != null` 的结果）

**缓存写入条件**:
- 同步方法：`invocation.ReturnValue` 不为 null 且是 `ApiResponse` 且 `Data != null`
- 异步方法：等待 Task 完成后获取 `Result`，同上的条件

---

#### 3.11.7 LoggingInterceptor

**命名空间**: `KH.WMS.Core.AOP.Interceptors`

**依赖**: `ILoggerService`, `IHttpContextAccessor`

**继承**: `InterceptorBase`

**说明**: 日志拦截器，继承 `InterceptorBase`，识别带有 `[LogInterceptor]` 特性的方法。在方法调用链的入口和出口输出结构化日志，支持调用链缩进、TraceId 追踪、参数格式化、返回值记录。集成 MiniProfiler 性能追踪。

**日志输出示例**:
```
[方法进入] [方法链层级:1] [a1b2c3d4] -> UserService.GetUser | User: 1001 (admin) | 参数: P0: 1001
[方法进入] [方法链层级:2] [a1b2c3d4] -> UserRepository.FindById | User: 1001 (admin) | 参数: P0: 1001
[方法退出] [方法链层级:2] [a1b2c3d4] <- UserRepository.FindById | 返回值: {"Id":1001,"Name":"admin"}
[方法退出] [方法链层级:1] [a1b2c3d4] <- UserService.GetUser | 返回值: {"Id":1001,"Name":"admin"}
```

**格式化规则**:
- 字符串参数输出 `"value"`
- 基本值类型（int、bool、DateTime 等）直接输出
- 复杂对象通过 `JsonConvert.SerializeObject` 序列化
- 无参数时输出 `无参数`

---

#### 3.11.8 ExceptionInterceptor

**命名空间**: `KH.WMS.Core.AOP.Interceptors`

**依赖**: `ILoggerService`

**说明**: 异常拦截器，实现 `IInterceptor`。在方法执行过程中捕获各类异常并记录日志，然后重新抛出。不改变异常类型或阻断执行。

**异常分类记录**:

| 异常类型 | 日志消息 |
|----------|----------|
| `BusinessException` | 业务异常 |
| `ValidationException` | 验证异常 |
| `NotFoundException` | 未找到异常 |
| `UnauthorizedAccessException` | 未授权访问 |
| `SqlException` | 数据库异常 |
| `Exception` (其他) | 系统异常 |

**注意**: 此拦截器仅记录日志并重新抛出异常，不处理异常。异常由全局异常过滤器或中间件统一处理。

---

#### 3.11.9 PerformanceInterceptor

**命名空间**: `KH.WMS.Core.AOP.Interceptors`

**依赖**: `ILoggerService`

**说明**: 性能拦截器，实现 `IInterceptor`。统计方法执行时间，当超过阈值时记录 Warning 日志。适用于监控慢调用和性能瓶颈。

##### `PerformanceInterceptor(ILoggerService logger, long thresholdMs = 1000)`

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `logger` | `ILoggerService` | — | 日志服务 |
| `thresholdMs` | `long` | `1000` | 执行时间阈值（毫秒），超过此值记录警告 |

**日志示例**:
```
性能警告: UserService.GetUser 执行时间: 2345ms (超过阈值: 1000ms)
```

---

#### 3.11.10 ConfigValidationInterceptor

**命名空间**: `KH.WMS.Core.AOP.Interceptors`

**依赖**: `ILifetimeScope` (Autofac)

**说明**: 配置校验拦截器，实现 `IInterceptor`。拦截标注了 `[ConfigValidation]` 特性的业务方法，在方法执行前运行声明的校验器链。任一校验失败则短路返回 `ServiceResult.Fail`，不执行原方法。

**工作流程**:
1. 检查方法上是否有 `[ConfigValidation]` 特性
2. 仅对返回 `Task<ServiceResult>` 或 `Task<ServiceResult<T>>` 的方法生效
3. 从 Autofac LifetimeScope 解析所有 `IValidator` 实现
4. 按 `ValidatorCode` 匹配校验器并同步执行
5. 任一校验失败 → 短路返回 `ServiceResult.Fail(errorMessage)`
6. 全部通过 → 执行原方法

---

#### 3.11.11 CallTraceContext / CallTraceState

**命名空间**: `KH.WMS.Core.AOP.CallTrace`

**说明**: 调用链追踪上下文，使用 `AsyncLocal<T>` 在异步调用链中传递 TraceId 和调用层级。支持在跨线程/异步场景下保持调用链的完整性。

##### CallTraceContext（静态类）

| 成员 | 说明 |
|------|------|
| `Current` | 获取当前调用链状态（`CallTraceState`），首次访问时自动初始化新 TraceId |
| `StartNewTrace()` | 手动开始一个新的调用链（重置 TraceId 和 Depth） |
| `EnterScope()` | 进入子调用，Depth +1，返回 `IDisposable`，Dispose 时 Depth -1 |

**使用示例**:
```csharp
using (CallTraceContext.EnterScope())
{
    // Depth 自动 +1
    var traceId = CallTraceContext.Current.TraceId; // 8位短ID
    var depth = CallTraceContext.Current.Depth;
    // ... 业务逻辑
}
// 离开 using 后 Depth 自动 -1
```

##### CallTraceState

| 属性 | 类型 | 说明 |
|------|------|------|
| `TraceId` | `string` | 追踪 ID（8 位短 ID，Guid 前 8 位） |
| `Depth` | `int` | 调用层级深度（EnterScope +1，Dispose -1） |
| `IsRecord` | `bool` | 是否为最外层入口调用（由 LoggingInterceptor 控制） |

### 3.12 Filters 过滤器

**说明**: 过滤器（Filters）在 ASP.NET Core MVC 管道的不同阶段执行，用于横切关注点的处理。KH.WMS.Core 包含以下几类过滤器：

| 过滤器类型 | 执行阶段 | 主要用途 |
|------------|----------|----------|
| 异常过滤器 | 资源/Action/Result 任意阶段抛出异常后 | 统一异常处理与响应格式化 |
| 授权过滤器 | 管道最早期（Action 之前） | JWT 令牌验证、角色/策略检查 |
| 动作过滤器 | Action 执行前后 | 事务管理、日志记录 |
| 结果过滤器 | ActionResult 执行前后 | 响应数据注入（如 TraceId） |
| 资源过滤器 | 管道最早期（在 Model Binding 之前） | 请求级缓存 |

---

#### 3.12.1 GlobalExceptionFilter / CustomExceptionFilter

**命名空间**: `KH.WMS.Core.Filters.Exception`

**GlobalExceptionFilter** — 全局异常过滤器，实现 `IAsyncExceptionFilter`。使用 `ApiResponse` 统一响应格式，根据异常类型返回不同的 HTTP 状态码和错误信息。

| 异常类型 | HTTP 状态码 | 响应码 | 说明 |
|----------|-------------|--------|------|
| `BusinessException` | 400 | `ResponseCode.BAD_REQUEST` | 业务异常，含 ErrorCode 和 Details |
| `NotFoundException` | 404 | `ResponseCode.NOT_FOUND` | 资源未找到，含 resourceType/resourceId |
| `ValidationException` | 400 | `ResponseCode.VALIDATION_ERROR` | 数据验证失败，含 Errors 详情 |
| `UnauthorizedAccessException` | 401 | `ResponseCode.UNAUTHORIZED` | 无权访问 |
| `ArgumentException` | 400 | `ResponseCode.BAD_REQUEST` | 参数异常 |
| 其他异常 | 500 | `ResponseCode.INTERNAL_SERVER_ERROR` | 系统内部错误（开发环境显示详情） |

**响应格式**:
```json
{
  "code": 40000,
  "message": "业务异常描述",
  "data": null,
  "traceId": "0HN...",
  "success": false
}
```

**CustomExceptionFilter** — 实现 `IExceptionFilter` 和 `IAsyncExceptionFilter`，提供类似的异常处理能力，通过 `HandleExceptionAttribute` (IFilterFactory) 启用。

可使用 `[HandleException]` 特性标记控制器或方法：
```csharp
[HandleException]
public class OrderController : ControllerBase { }
```

---

#### 3.12.2 ApiAuthorizeFilter

**命名空间**: `KH.WMS.Core.Filters.Authorization`

**说明**: API 授权过滤器，实现 `IAuthorizationFilter`。处理 JWT 令牌验证、用户身份解析、单点登录（互踢）检测。

**工作流程**:
1. 如果 JWT 中间件未设置用户信息，手动从 Token 解析 Claims 并设置到 `HttpContext.User`
2. 检查是否标注了 `[AllowAnonymous]`，是则跳过授权
3. 从请求头提取 Bearer Token
4. 从 JWT 中提取 `userId`（jti 声明）
5. 从缓存中获取已存储的 Token 与当前 Token 比对
6. 若 `SYS_ALLOW_MULTI_LOGIN = false` 且缓存 Token 与当前 Token 不一致，返回"账号已被踢下线"

**依赖**: `ICacheService` — 用于获取缓存中的用户 Token 和多设备登录配置

**响应**:
```json
{
  "message": "账号已在其它设备登录，您已被踢下线",
  "status": false,
  "code": 401
}
```

---

#### 3.12.3 TransactionActionFilter

**命名空间**: `KH.WMS.Core.Filters.Action`

**说明**: 事务 Action Filter，实现 `IAsyncActionFilter`。由 `[Transaction]` 特性通过 `IFilterFactory` 创建，自动管理请求级别的事务。

**工作流程**:
1. 检查 `IUnitOfWork` 是否已存在活跃事务（Service 层手动开启时跳过）
2. 调用 `_unitOfWork.BeginTransactionAsync(isolationLevel)` 开启事务
3. 执行 Action
4. Action 执行成功 → `CommitAsync()` 提交事务
5. Action 抛出异常 → `RollbackAsync()` 回滚事务

**隔离级别**: 默认 `ReadCommitted`，可通过 `[Transaction(IsolationLevel = IsolationLevel.Serializable)]` 修改

**注意**: 如果 Service 层已经手动开启了事务（`HasActiveTransaction = true`），过滤器不会重复开启事务。

---

#### 3.12.4 TraceIdResultFilter

**命名空间**: `KH.WMS.Core.Filters.Result`

**说明**: TraceId 结果过滤器，实现 `IResultFilter`。自动为 `ApiResponse` 响应注入 `TraceId`（用于请求链路追踪）。

**工作方式**:
1. 在 `OnResultExecuting` 阶段检查 `context.Result`
2. 如果结果是 `ObjectResult` 且 `Value` 为 `ApiResponse`
3. 且 `response.TraceId` 为空，则设置为 `context.HttpContext.TraceIdentifier`

**注册方式**（在启动代码中添加）:
```csharp
services.AddControllers(options =>
{
    options.Filters.Add<TraceIdResultFilter>();
});
```

---

#### 3.12.5 CustomResourceFilter / CacheResourceAttribute

**命名空间**: `KH.WMS.Core.Filters.Resource`

**CustomResourceFilter** — 资源缓存过滤器，实现 `IResourceFilter` 和 `IAsyncResourceFilter`。在资源阶段（Model Binding 之前）缓存请求结果。

**工作流程**:
1. `OnResourceExecuting`：检查路径缓存是否命中，命中则设置 `context.Result` 并短路管道
2. `OnResourceExecuted`：将执行结果写入缓存字典

**注意**: 使用静态内存字典 `Dictionary<string, object>` 做缓存，适合轻量级场景。

**CacheResourceAttribute** — 资源缓存特性，实现 `IFilterFactory`，用于标记控制器或方法：

```csharp
[CacheResource]
public class LookupController : ControllerBase
{
    [CacheResource]
    public IActionResult GetConstants()
    {
        return Ok(constants);
    }
}
```

---

#### 3.12.6 CustomActionFilter / LogActionAttribute

**命名空间**: `KH.WMS.Core.Filters.Action`

**CustomActionFilter** — 自定义动作过滤器，实现 `IActionFilter` 和 `IAsyncActionFilter`。记录 Action 的执行开始、完成、耗时和异常信息。

**日志输出**:
```
开始执行: Order/Create
执行成功: Order/Create, 耗时: 45ms
```

**LogActionAttribute** — 动作日志特性，实现 `IFilterFactory`，用于标记控制器或方法：

```csharp
[LogAction]
public class OrderController : ControllerBase
{
    [LogAction]
    public async Task<IActionResult> CreateOrder(OrderDto order)
    {
        // 自动记录 Action 执行开始/完成日志
    }
}
```

---

#### 3.12.7 CustomAuthorizationFilter / CustomAuthorizeAttribute

**命名空间**: `KH.WMS.Core.Filters.Authorization`

**CustomAuthorizationFilter** — 自定义授权过滤器，实现 `IAuthorizationFilter`。支持角色和策略检查。

**工作流程**:
1. 检查用户是否已认证（`User.Identity?.IsAuthenticated`）
2. 未认证 → 返回 `401 Unauthorized`
3. 已认证但无指定角色 → 返回 `403 Forbid`
4. 检查策略（预留扩展点）

**CustomAuthorizeAttribute** — 授权特性，实现 `IFilterFactory`：

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Policy` | `string` | `""` | 授权策略名称 |
| `Roles` | `string[]` | `[]` | 允许访问的角色数组 |

**使用示例**:
```csharp
[CustomAuthorize(Roles = new[] { "Admin", "SuperAdmin" })]
public IActionResult AdminDashboard()
{
    return Ok();
}

[CustomAuthorize("RequireManagerPolicy")]
public IActionResult ManagerSection()
{
    return Ok();
}
```

### 3.13 Middlewares 中间件

> 本节介绍 `KH.WMS.Core` 提供的内置 ASP.NET Core 中间件。所有中间件均已集成到 `MiddlewareSetup.UseCustomMiddleware()` 方法中，按固定顺序自动注册。如需手动注册，请确保顺序正确。

#### 3.13.1 ExceptionHandlingMiddleware

**命名空间**: `KH.WMS.Core.Middlewares`

**说明**: 全局异常处理中间件，捕获管道中所有未处理的异常，统一转换为标准 JSON 错误响应。按异常类型分发处理逻辑，开发环境下返回详细错误信息（类型、消息、堆栈），生产环境隐藏内部细节。

**注册到管道**: 应置于中间件管道最顶层（序号 ①），确保任何下游异常均能被捕获。

**扩展方法**: `UseExceptionHandling()`

**管道位置**: `app.UseExceptionHandling()` — 必须在所有其他中间件之前调用。

**异常处理映射**:

| 异常类型 | HTTP 状态码 | Code | Message |
|---------|------------|------|---------|
| `BusinessException` | 400 BadRequest | `businessException.ErrorCode` | 业务异常消息 |
| `ValidationException` | 400 BadRequest | `ErrorCodes.VALIDATION_ERROR` | "数据验证失败" |
| `NotFoundException` | 404 NotFound | `ErrorCodes.NOT_FOUND` | 未找到异常消息 |
| `UnauthorizedAccessException` | 401 Unauthorized | `ErrorCodes.UNAUTHORIZED` | "未授权访问" |
| 其他异常 | 500 InternalServerError | `ErrorCodes.SYSTEM_ERROR` | 生产环境："系统发生错误，请稍后重试"；开发环境：exception.Message |

**响应格式**:
```json
{
  "code": "SYSTEM_ERROR",
  "message": "系统发生错误，请稍后重试",
  "data": null,
  "traceId": "0HN6G9FQ1K9CD:00000001"
}
```

**使用示例**:
```csharp
// 在 Program.cs 中注册（通常在 UseCustomMiddleware 内部自动调用）
app.UseExceptionHandling();
```

#### 3.13.2 RequestLoggingMiddleware

**命名空间**: `KH.WMS.Core.Middlewares`

**说明**: 请求日志中间件，仅拦截 `/api` 路径的 HTTP 请求，记录请求的 Method、Path、StatusCode 和执行耗时（毫秒）。当前日志代码已注释，开发者可根据需要启用。

**注册到管道**: 应在 CORS 之后、路由之前（序号 ⑥），确保仅记录已通过 CORS 的请求。

**扩展方法**: `UseRequestLogging()`

**管道位置**: `app.UseRequestLogging()` — 在 `UseRouting()` 之前调用。

**行为**:
- 仅拦截路径以 `/api` 开头的请求
- 使用 `Stopwatch` 测量请求执行耗时
- 非 `/api` 路径直接放行，不做日志记录
- 当前日志语句已注释（预留），启用后格式为：
  - 请求开始：`"请求开始: {Method} {Path} from {RemoteIp}"`
  - 请求完成：`"请求完成: {Method} {Path} - {StatusCode} - 耗时: {Elapsed}ms"`

**使用示例**:
```csharp
// 在 Program.cs 中注册
app.UseRequestLogging();
```

#### 3.13.3 RateLimitMiddleware

**命名空间**: `KH.WMS.Core.Middlewares`

**说明**: 基于内存的滑动窗口限流中间件，按客户端（用户 ID → API Key → IP 地址优先顺序）统计请求次数，超出阈值时返回 429 Too Many Requests。

**注册到管道**: 推荐在 CORS 之后、身份认证之前，确保限流在认证前生效。

**扩展方法**:
- `UseRateLimiting(RateLimitOptions? options = null)` — 使用中间件
- `AddRateLimiting(IConfiguration configuration)` — 从 `appsettings.json` 的 `RateLimit` 节绑定配置

**依赖**: 需要注册 `ICacheService`（内存缓存实现）。

**客户端标识识别优先级**:
1. 用户 ID（JWT Claim `NameIdentifier`）
2. API Key（请求头 `X-API-Key`）
3. 客户端 IP 地址

**RateLimitOptions 属性**:

| 属性名 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `RequestLimit` | `int` | `500` | 时间窗口内允许的最大请求数 |
| `WindowSeconds` | `int` | `60` | 时间窗口大小（秒） |
| `KeyPrefix` | `string` | `"ratelimit"` | 缓存键前缀 |

**超出限制时的响应**:
```json
{
  "code": "RATE_LIMIT_EXCEEDED",
  "message": "请求过于频繁，请在 60 秒后重试",
  "retryAfter": 60
}
```

**使用示例**:
```csharp
// 方式一：在 Program.cs 中使用默认配置
app.UseRateLimiting();

// 方式二：自定义配置
app.UseRateLimiting(new RateLimitOptions
{
    RequestLimit = 100,
    WindowSeconds = 30
});

// 方式三：从配置文件绑定
builder.Services.AddRateLimiting(builder.Configuration);
// appsettings.json:
// {
//   "RateLimit": {
//     "RequestLimit": 200,
//     "WindowSeconds": 60,
//     "KeyPrefix": "ratelimit"
//   }
// }
```

#### 3.13.4 CorsMiddleware

**命名空间**: `KH.WMS.Core.Middlewares`

**说明**: CORS（跨域资源共享）配置扩展，提供三种预定义策略（Default / Development / Production），支持从 `appsettings.json` 的 `Cors` 节灵活配置允许的来源、方法、请求头等。

**注册到管道**: 应置于静态文件之后、请求日志之前（序号 ⑤）。

**扩展方法**:
- `AddCustomCors(IConfiguration)` — 将 CORS 服务注册到 DI 容器
- `UseCustomCors(string policyName = "DefaultPolicy")` — 启用指定的 CORS 策略中间件

**预定义策略**:

| 策略名 | 说明 |
|--------|------|
| `"DefaultPolicy"` | 按配置自动选择 AllowAnyOrigin 或指定来源（支持凭证） |
| `"DevelopmentPolicy"` | 允许所有来源/方法/请求头，不支持凭证，预检缓存 1 小时 |
| `"ProductionPolicy"` | 仅允许指定来源，支持凭证 |

**CorsOptions 属性**（从 `appsettings.json` 的 `Cors` 节绑定）:

| 属性名 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `AllowAnyOrigin` | `bool` | `false` | 是否允许任何来源（与 AllowCredentials 冲突，二选一） |
| `AllowedOrigins` | `string[]` | `[]` | 允许的来源列表（AllowAnyOrigin 为 false 时使用） |
| `AllowAnyMethod` | `bool` | `true` | 是否允许任何 HTTP 方法 |
| `AllowAnyHeader` | `bool` | `true` | 是否允许任何请求头 |
| `ExposedHeaders` | `string[]?` | `null` | 暴露给客户端的响应头 |
| `SetPreflightMaxAge` | `bool` | `false` | 是否设置预检请求缓存时间 |
| `PreflightMaxAgeSeconds` | `int` | `600` | 预检请求缓存时间（秒） |

**使用示例**:
```csharp
// Program.cs
// 注册 CORS 服务
builder.Services.AddCustomCors(builder.Configuration);

// 使用 CORS 中间件（默认使用 DefaultPolicy）
app.UseCustomCors();

// 或指定使用生产环境策略
app.UseCustomCors("ProductionPolicy");

// appsettings.json 配置示例
// {
//   "Cors": {
//     "AllowAnyOrigin": false,
//     "AllowedOrigins": ["https://admin.example.com", "https://app.example.com"],
//     "AllowAnyMethod": true,
//     "AllowAnyHeader": true,
//     "ExposedHeaders": ["Content-Disposition", "X-Pagination"],
//     "SetPreflightMaxAge": true,
//     "PreflightMaxAgeSeconds": 3600
//   }
// }
```

#### 3.13.5 StaticFileMiddleware

**命名空间**: `KH.WMS.Core.Middlewares`

**说明**: 静态文件中间件扩展，提供默认静态文件服务 + 自定义静态文件目录 + 目录浏览功能。自动配置缓存控制头和安全响应头。

**注册到管道**: 应置于 HTTPS 重定向之后、CORS 之前（序号 ④）。

**扩展方法**:
- `UseCustomStaticFiles(IWebHostEnvironment, string wwwRoot = "wwwroot")` — 配置静态文件服务
- `UseDirectoryBrowsing(string directoryPath)` — 启用目录浏览（`/files` 路径）

**UseCustomStaticFiles 行为**:
1. 调用 `UseStaticFiles()` 启用默认静态文件服务
2. 添加缓存控制头：`Cache-Control: public, max-age=86400`（1 天）
3. 配置自定义静态文件目录（基于 `WebRootPath`），添加安全响应头：
   - `X-Content-Type-Options: nosniff`
   - `X-Frame-Options: DENY`

**UseDirectoryBrowsing 参数**:

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `directoryPath` | `string` | 是 | 要浏览的物理目录路径 |

**使用示例**:
```csharp
// 基本用法
app.UseCustomStaticFiles(app.Environment);

// 自定义 wwwroot 目录名
app.UseCustomStaticFiles(app.Environment, "public");

// 启用目录浏览
app.UseDirectoryBrowsing(@"C:\uploads");
// 可通过 /files 路径浏览该目录
```

#### 3.13.6 LicenseValidationMiddleware

**命名空间**: `KH.WMS.Core.License.Middleware`

**说明**: License 验证中间件，拦截所有非白名单路径的 HTTP 请求，验证系统许可证是否有效。许可证验证失败时返回 403 Forbidden 响应。

**注册到管道**: 应在异常处理之后、HTTPS 重定向之前调用（序号 ②），确保 License 验证在授权之前执行。

**扩展方法**: `UseLicenseValidation()`

**白名单路径**（不验证 License 的路径）:

| 路径 | 说明 |
|------|------|
| `/api/license/machine-code` | 获取机器码 |
| `/api/license/import` | 导入许可证 |
| `/api/license/info` | 获取许可证信息 |
| `/swagger` | Swagger API 文档 |
| `/health` | 健康检查 |
| `/healthchecks` | 健康检查（完整版） |

**验证失败时的响应**（HTTP 403）:
```json
{
  "type": "https://httpstatuses.com/403",
  "title": "Forbidden",
  "status": 403,
  "detail": "系统配置校验未通过",
  "instance": "/api/orders"
}
```

**依赖**: `ILicenseService` — 许可证验证服务，通过中间件的 `InvokeAsync` 参数注入。

**使用示例**:
```csharp
// 在 Program.cs 中注册（应在 UseExceptionHandling 之后、UseHttpsRedirection 之前）
app.UseLicenseValidation();
```

### 3.14 Factories 工厂

> 本节介绍业务处理器工厂模式，用于按类型动态查找和调用业务处理器，实现业务逻辑的解耦和可扩展性。

#### 3.14.1 IBusinessProcessor / BusinessProcessorBase

**命名空间**: `KH.WMS.Core.Factories`

##### IBusinessProcessor

**说明**: 业务处理器接口，所有业务处理器必须实现此接口。通过 `ProcessorType` 标识处理器类型，通过 `ProcessAsync` 执行业务逻辑。

**接口定义**:

```csharp
public interface IBusinessProcessor
{
    string ProcessorType { get; }
    Task<(bool Success, object? Data, string? ErrorMessage)> ProcessAsync(
        JToken jsonData,
        IServiceProvider serviceProvider);
}
```

**属性**:

| 属性名 | 类型 | 说明 |
|--------|------|------|
| `ProcessorType` | `string` | 处理器类型标识，用于工厂查找匹配的处理器 |

**方法**:

###### `Task<(bool Success, object? Data, string? ErrorMessage)> ProcessAsync(JToken jsonData, IServiceProvider serviceProvider)`

**说明**: 执行处理器逻辑

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `jsonData` | `JToken` | 是 | 传递给处理器的 JSON 业务数据 |
| `serviceProvider` | `IServiceProvider` | 是 | 服务提供者，用于按需解析依赖服务 |

**返回值**: `(bool Success, object? Data, string? ErrorMessage)` — 三元组，`Success` 表示处理是否成功，`Data` 为返回数据，`ErrorMessage` 为失败时的错误消息。

---

##### BusinessProcessorBase

**说明**: 业务处理器抽象基类，实现了 `IBusinessProcessor` 接口，并提供 JSON 属性安全读取的辅助方法。子类继承并实现 `ProcessorType` 和 `ProcessAsync`。

**抽象成员**:

| 成员 | 说明 |
|------|------|
| `abstract string ProcessorType { get; }` | 处理器类型，子类必须实现 |
| `abstract Task<(bool, object?, string?)> ProcessAsync(JToken, IServiceProvider)` | 处理逻辑，子类必须实现 |

**受保护的辅助方法**:

###### `string GetStringProperty(JToken element, string propertyName, string defaultValue = "")`

**说明**: 从 JSON 元素中安全获取字符串属性值

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `element` | `JToken` | 是 | JSON 元素 |
| `propertyName` | `string` | 是 | 属性名 |
| `defaultValue` | `string` | 否 | 默认值（默认 `""`） |

**返回值**: `string` — 属性值或默认值

---

###### `decimal GetDecimalProperty(JToken element, string propertyName, decimal defaultValue = 0)`

**说明**: 从 JSON 元素中安全获取数值属性

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `element` | `JToken` | 是 | JSON 元素 |
| `propertyName` | `string` | 是 | 属性名 |
| `defaultValue` | `decimal` | 否 | 默认值（默认 0） |

**返回值**: `decimal` — 属性值或默认值

---

###### `DateTime? GetDateTimeProperty(JToken element, string propertyName)`

**说明**: 从 JSON 元素中安全获取日期属性

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `element` | `JToken` | 是 | JSON 元素 |
| `propertyName` | `string` | 是 | 属性名 |

**返回值**: `DateTime?` — 日期值，解析失败或属性不存在返回 `null`

**使用示例**:
```csharp
public class InboundProcessor : BusinessProcessorBase
{
    public override string ProcessorType => "INBOUND";

    public override Task<(bool Success, object? Data, string? ErrorMessage)> ProcessAsync(
        JToken jsonData, IServiceProvider serviceProvider)
    {
        var orderNo = GetStringProperty(jsonData, "OrderNo");
        var quantity = GetDecimalProperty(jsonData, "Quantity");
        var expiryDate = GetDateTimeProperty(jsonData, "ExpiryDate");

        // 业务处理...
        return Task.FromResult<(bool, object?, string?)>((true, null, null));
    }
}
```

#### 3.14.2 BusinessProcessorFactory

**命名空间**: `KH.WMS.Core.Factories`

**说明**: 业务处理器工厂类，注册为 `Singleton` 生命周期（标记 `[SelfRegisteredService]`，跳过拦截器）。在构造时自动扫描所有引用程序集中实现 `IBusinessProcessor` 的非抽象类并注册。使用者通过 `GetProcessor(processorType)` 按类型字符串查找处理器。

**生命周期**: Singleton（全局唯一实例）

**特性**: `[SelfRegisteredService(Lifetime = ServiceLifetime.Singleton, WithoutInterceptor = true)]`

**构造函数**:

| 参数名 | 类型 | 说明 |
|--------|------|------|
| `serviceProvider` | `IServiceProvider` | 服务提供者，用于后续按需解析依赖 |

**方法**:

###### `void RegisterProcessor(IBusinessProcessor processor)`

**说明**: 注册单个处理器实例

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `processor` | `IBusinessProcessor` | 是 | 处理器实例 |

---

###### `void RegisterProcessors(IEnumerable<IBusinessProcessor> processors)`

**说明**: 批量注册多个处理器

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `processors` | `IEnumerable<IBusinessProcessor>` | 是 | 处理器集合 |

---

###### `IBusinessProcessor? GetProcessor(string processorType)`

**说明**: 根据处理器类型获取对应的处理器实例

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `processorType` | `string` | 是 | 处理器类型标识（与 `IBusinessProcessor.ProcessorType` 匹配） |

**返回值**: `IBusinessProcessor?` — 找到返回处理器实例，未找到或 `processorType` 为空返回 `null`

---

###### `IEnumerable<IBusinessProcessor> GetAllProcessors()`

**说明**: 获取所有已注册的处理器

**返回值**: `IEnumerable<IBusinessProcessor>` — 全部已注册处理器

---

###### `void AutoRegisterProcessors()`

**说明**: 自动扫描所有引用程序集，查找实现 `IBusinessProcessor` 的非抽象类，使用 `Activator.CreateInstance` 创建实例并注册。在构造函数中自动调用。

**内部逻辑**:
1. 通过 `AssemblyService.GetReferencedAssemblies()` 获取所有引用的程序集
2. 遍历每个程序集，查找实现 `IBusinessProcessor` 且非接口、非抽象类的类型
3. 使用 `Activator.CreateInstance` 创建实例并注册到内部 `ConcurrentDictionary`
4. 注册失败时记录错误日志，但不中断整体扫描

**使用示例**:
```csharp
public class OrderProcessingService
{
    private readonly BusinessProcessorFactory _factory;

    public OrderProcessingService(BusinessProcessorFactory factory)
    {
        _factory = factory;
    }

    public async Task<object?> ProcessOrderAsync(string orderType, JToken orderData)
    {
        var processor = _factory.GetProcessor(orderType);
        if (processor == null)
            throw new BusinessException($"未找到处理器: {orderType}");

        var (success, data, error) = await processor.ProcessAsync(orderData, null!);
        if (!success)
            throw new BusinessException(error ?? "处理失败");

        return data;
    }

    public IEnumerable<string> GetSupportedOrderTypes()
    {
        return _factory.GetAllProcessors().Select(p => p.ProcessorType);
    }
}
```

### 3.15 User Context 用户上下文

> 提供当前 HTTP 请求的用户上下文信息，包括用户 ID、用户名、角色、Token、权限列表等。通过 JWT Token 自动解析并缓存，同一请求内仅解析一次。

#### 3.15.1 IUserContext / UserContext

**命名空间**: `KH.WMS.Core.UserProvide`

**说明**: 用户上下文接口及实现，注册为 Scoped 生命周期。通过 `IHttpContextAccessor` 获取当前 HTTP 上下文，解析 JWT Token 提取用户信息，并使用 `ICacheService` 缓存 Token。

**注册方式**: `[RegisteredService(WithoutInterceptor = true)]`

**构造函数依赖**:

| 参数名 | 类型 | 说明 |
|--------|------|------|
| `httpContextAccessor` | `IHttpContextAccessor` | HTTP 上下文访问器 |
| `cacheService` | `ICacheService` | 缓存服务（用于缓存 Token） |
| `jwtTokenService` | `IJwtTokenService` | JWT Token 服务（用于解析 Token） |

**属性**:

| 属性名 | 类型 | 说明 |
|--------|------|------|
| `UserName` | `string?` | 当前用户名，从 JWT Token 的 `unique_name` 声明解析 |
| `UserId` | `long?` | 当前用户 ID，从 JWT Token 的 `jti` 声明解析 |
| `RoleId` | `long?` | 当前角色 ID，从 JWT Token 的 `role` 声明解析（取第一个角色） |
| `Token` | `string?` | 当前请求的 Bearer Token |
| `Permissions` | `List<string>` | 当前用户权限列表 |
| `MenuType` | `int?` | 菜单类型：`1`（请求头包含 `uniapp`）或 `0`（PC 端） |
| `IsAuthenticated` | `bool` | 当前用户是否已通过身份认证 |
| `IsSuperAdmin` | `bool` | 是否为超级管理员（RoleId == 1），绕过权限检查 |
| `HttpContext` | `HttpContext?` | 当前 HTTP 上下文 |

**方法**:

###### `string? GetToken()`

**说明**: 获取当前用户的认证令牌

**返回值**: `string?` — JWT Token 字符串

**内部解析优先级**:
1. 从 `Authorization: Bearer {token}` 请求头提取
2. 从 HttpContext 的 Claims 中查找 `jti` 声明
3. 尝试从请求头 Token 解析 Claims 并写入 HttpContext
4. 从缓存中获取（键：`CacheConstants.Token.PREFIX + userId`）

> **注意**: `EnsureToken()` 方法确保同一请求内只解析一次 Token，后续访问直接从缓存字段 `_token` 返回。

---

###### `IEnumerable<Claim> GetClaims()`

**说明**: 获取当前用户的所有 Claims

**返回值**: `IEnumerable<Claim>` — 用户 Claims 集合，HttpContext.User 为 null 时返回空集合

**使用示例**:
```csharp
public class OrderService
{
    private readonly IUserContext _userContext;

    public OrderService(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public async Task<ApiResponse> CreateOrderAsync(OrderDto dto)
    {
        // 检查用户认证状态
        if (!_userContext.IsAuthenticated)
            return ApiResponse.Unauthorized("请先登录");

        // 获取当前用户信息
        var userId = _userContext.UserId;
        var userName = _userContext.UserName;

        // 超级管理员跳过权限检查
        if (_userContext.IsSuperAdmin)
        {
            // 直接执行业务逻辑
        }

        // 检查用户权限
        if (!_userContext.Permissions.Contains("order:create"))
            return ApiResponse.Forbidden("无创建订单权限");

        // 创建订单并记录创建人
        var order = new Order
        {
            CreatedBy = userId,
            CreatorName = userName
        };

        return ApiResponse.Ok(order);
    }
}
```

### 3.16 Import/Export 导入导出

> 基于 MiniExcel 的高性能 Excel 导入导出服务，支持泛型数据导出、从文件流导入数据、以及生成导入模板（仅表头）。

#### 3.16.1 IImportExportService / ImportExportService

**命名空间**: `KH.WMS.Core.ImportExport`

**说明**: Excel 导入导出服务接口及实现，底层使用 MiniExcel 库。`ImportExportService` 注册为 Scoped 生命周期，依赖 `ILogger<ImportExportService>`。

**注册方式**: `[RegisteredService]`

**接口方法**:

###### `Task<byte[]> ExportAsync<T>(IEnumerable<T> data, string? sheetName = null)`

**说明**: 将数据导出为 Excel 字节数组

**泛型约束**: 无

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `data` | `IEnumerable<T>` | 是 | 要导出的数据集合 |
| `sheetName` | `string?` | 否 | 工作表名称，不指定时使用默认名称 |

**返回值**: `Task<byte[]>` — Excel 文件的字节数组，可直接写入 HTTP 响应或保存到磁盘

**内部逻辑**:
- `sheetName` 为 null 时，直接保存数据到单工作表
- `sheetName` 不为 null 时，创建指定名称的工作表保存数据（支持多工作表）

---

###### `Task<List<T>> ImportAsync<T>(Stream stream, string fileName) where T : class, new()`

**说明**: 从 Excel 文件流导入数据

**泛型约束**: `where T : class, new()`

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `stream` | `Stream` | 是 | Excel 文件流 |
| `fileName` | `string` | 是 | 文件名（用于日志记录） |

**返回值**: `Task<List<T>>` — 反序列化后的数据列表

**异常**:
- 导入失败时记录错误日志并重新抛出异常

---

###### `Task<byte[]> GenerateTemplateAsync<T>(string? sheetName = null) where T : class, new()`

**说明**: 生成导入模板（仅包含列标题的表头行，无数据行）

**泛型约束**: `where T : class, new()`

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `sheetName` | `string?` | 否 | 工作表名称，不指定时使用默认名称 |

**返回值**: `Task<byte[]>` — 仅含表头的 Excel 模板字节数组

**内部逻辑**: MiniExcel 会根据类型 `T` 的属性自动生成对应的列标题。

**使用示例**:
```csharp
public class ProductController
{
    private readonly IImportExportService _importExport;

    public ProductController(IImportExportService importExport)
    {
        _importExport = importExport;
    }

    /// <summary>
    /// 导出产品数据到 Excel
    /// </summary>
    public async Task<IActionResult> ExportProducts()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "商品A", Code = "P001", Price = 10.5m },
            new() { Id = 2, Name = "商品B", Code = "P002", Price = 20.0m }
        };

        var bytes = await _importExport.ExportAsync(products, "产品列表");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "products.xlsx");
    }

    /// <summary>
    /// 从 Excel 导入产品数据
    /// </summary>
    public async Task<List<Product>> ImportProducts(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var products = await _importExport.ImportAsync<Product>(stream, file.FileName);
        return products;
    }

    /// <summary>
    /// 下载导入模板
    /// </summary>
    public async Task<IActionResult> DownloadTemplate()
    {
        var bytes = await _importExport.GenerateTemplateAsync<Product>("产品导入模板");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "product-template.xlsx");
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public decimal Price { get; set; }
}
```

### 3.17 Validation 验证

> 本节提供统一校验器框架，支持通过特性标注声明业务方法的验证规则，由 AOP 拦截器自动调度执行。校验器按编码查找并顺序执行，任一校验失败则短路返回。

#### 3.17.1 IValidator

**命名空间**: `KH.WMS.Core.Validation`

**说明**: 统一校验器接口。所有校验器（配置驱动校验、参数校验等）实现此接口。通过 `[ConfigValidation]` 属性标注到业务方法上，由 `ConfigValidationInterceptor` 自动调度。

**接口定义**:

```csharp
public interface IValidator
{
    string Code { get; }
    Task<string?> ValidateAsync(object?[] args, Dictionary<string, object>? services = null);
}
```

**属性**:

| 属性名 | 类型 | 说明 |
|--------|------|------|
| `Code` | `string` | 校验器唯一标识，对应 `[ConfigValidation("code")]` 属性中的 code |

**方法**:

###### `Task<string?> ValidateAsync(object?[] args, Dictionary<string, object>? services = null)`

**说明**: 执行校验逻辑

**参数**:
| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `args` | `object?[]` | 是 | 被拦截方法的参数数组 |
| `services` | `Dictionary<string, object>?` | 否 | 可选的依赖服务字典，由拦截器注入（如 `"ConfigService"` → `ICfgGlobalConfigService` 实例） |

**返回值**: `Task<string?>` — `null` 表示校验通过；返回非空字符串表示校验失败，字符串内容为错误消息

**使用示例**:
```csharp
public class BindDataNotEmptyValidator : IValidator
{
    public string Code => ValidatorCodes.BIND_DATA_NOT_EMPTY;

    public Task<string?> ValidateAsync(object?[] args, Dictionary<string, object>? services = null)
    {
        // 检查绑定数据是否为空
        var data = args.FirstOrDefault() as IEnumerable<object>;
        if (data == null || !data.Any())
        {
            return Task.FromResult<string?>("绑定数据不能为空");
        }
        return Task.FromResult<string?>(null); // 通过
    }
}
```

#### 3.17.2 ConfigValidationAttribute

**命名空间**: `KH.WMS.Core.Validation`

**说明**: 标注在业务方法上的特性，声明该方法需要执行的校验器列表。支持在同一方法上多次使用以声明多个校验器。拦截器按声明顺序依次执行，任一校验失败则短路返回。

**特性用法**: `[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]`

**构造函数**:

| 参数名 | 类型 | 说明 |
|--------|------|------|
| `validatorCode` | `string` | 校验器编码，对应 `IValidator.Code` |

**属性**:

| 属性名 | 类型 | 说明 |
|--------|------|------|
| `ValidatorCode` | `string` | 校验器编码 |

**生效条件**: 仅对返回 `Task<ServiceResult>` 或 `Task<ServiceResult<T>>` 的方法生效。

**使用示例**:
```csharp
public class PalletizingService
{
    /// <summary>
    /// 执行组盘操作
    /// </summary>
    [ConfigValidation(ValidatorCodes.BIND_DATA_NOT_EMPTY)]  // 第1步：数据不能为空
    [ConfigValidation(ValidatorCodes.BIND_QUANTITY)]        // 第2步：校验数量
    [ConfigValidation(ValidatorCodes.MIXED_MATERIAL)]       // 第3步：校验物料混放
    [ConfigValidation(ValidatorCodes.MIXED_BATCH)]          // 第4步：校验批次混放
    public async Task<ServiceResult> BindPalletAsync(List<BindItem> items)
    {
        // 业务逻辑...
        return ServiceResult.Success();
    }
}
```

#### 3.17.3 ValidatorCodes

**命名空间**: `KH.WMS.Core.Validation`

**说明**: 校验器编码常量类，集中管理所有校验器编码。用于 `[ConfigValidation(ValidatorCodes.XXX)]` 特性标注和 `IValidator.Code` 实现中的编码匹配。

**常量定义**:

| 常量名 | 值 | 说明 |
|--------|-----|------|
| `BIND_DATA_NOT_EMPTY` | `"BIND_DATA_NOT_EMPTY"` | 组盘数据不能为空校验 |
| `BIND_QUANTITY` | `"BIND_QUANTITY"` | 组盘数量校验 |
| `BATCH_NO_REQUIRED` | `"BATCH_NO_REQUIRED"` | 批次号必填校验 |
| `EXPIRY_DATE_REQUIRED` | `"EXPIRY_DATE_REQUIRED"` | 到期日期必填校验 |
| `MIXED_MATERIAL` | `"MIXED_MATERIAL"` | 物料混放校验（校验同一托盘是否混放不同物料） |
| `MIXED_BATCH` | `"MIXED_BATCH"` | 批次混放校验（校验同一托盘是否混放不同批次） |

**使用示例**:
```csharp
// 在特性中使用
[ConfigValidation(ValidatorCodes.BIND_DATA_NOT_EMPTY)]
[ConfigValidation(ValidatorCodes.BIND_QUANTITY)]
public async Task<ServiceResult> BindPalletAsync(List<BindItem> items)

// 在校验器实现中匹配编码
public string Code => ValidatorCodes.BIND_DATA_NOT_EMPTY;
```

### 3.18 Modularity 模块化

#### 3.18.1 IModule / IModuleContext

**命名空间**: `KH.WMS.Core.Modularity`

##### IModule 接口

WMS 模块接口，所有业务模块必须实现此接口。

| 成员 | 类型 | 说明 |
|------|------|------|
| `ModuleId` | `Guid` | 模块唯一标识 |
| `Name` | `string` | 模块名称 |
| `Version` | `Version` | 模块版本 |
| `Description` | `string` | 模块描述 |
| `Author` | `string` | 模块作者 |
| `Dependencies` | `IEnumerable<ModuleDependency>` | 依赖的其他模块列表 |
| `InitializeAsync(IModuleContext)` | `Task` | 模块初始化（按依赖顺序调用） |
| `StartAsync(CancellationToken)` | `Task` | 模块启动 |
| `StopAsync(CancellationToken)` | `Task` | 模块停止 |
| `ShutdownAsync()` | `Task` | 模块卸载 |

**方法说明**:

| 方法 | 参数 | 说明 |
|------|------|------|
| `InitializeAsync(context)` | `IModuleContext context` | 在模块依赖排序后执行，用于注册服务、初始化数据库等 |
| `StartAsync(cancellationToken)` | `CancellationToken`（默认 `default`） | 初始化完成后依次启动 |
| `StopAsync(cancellationToken)` | `CancellationToken`（默认 `default`） | 按依赖逆序停止 |
| `ShutdownAsync()` | 无 | 按依赖逆序卸载，释放资源 |

**使用示例**:
```csharp
public class OrderModule : IModule
{
    public Guid ModuleId => Guid.NewGuid();
    public string Name => "OrderModule";
    public Version Version => new(1, 0, 0);
    public string Description => "订单管理模块";
    public string Author => "KH.WMS Team";
    public IEnumerable<ModuleDependency> Dependencies => Array.Empty<ModuleDependency>();

    public async Task InitializeAsync(IModuleContext context)
    {
        // 注册服务到容器
        context.RegisterScoped<IOrderService, OrderService>();
        await Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task ShutdownAsync() => Task.CompletedTask;
}
```

##### IModuleContext 接口

模块上下文接口，提供模块生命周期内的服务注册和基础设施访问能力。

| 成员 | 类型 | 说明 |
|------|------|------|
| `ServiceProvider` | `IServiceProvider` | 服务提供器 |
| `Configuration` | `IConfiguration` | 配置对象 |
| `LoggerFactory` | `ILoggerFactory` | 日志工厂 |
| `Services` | `IServiceCollection` | 服务集合 |

**注册方法**:

| 方法 | 说明 |
|------|------|
| `RegisterService<TService, TImplementation>(lifetime)` | 注册服务，默认 `Transient` |
| `RegisterService<TService>(instance, lifetime)` | 注册服务实例，默认 `Singleton` |
| `RegisterSingleton<TService, TImplementation>()` | 注册 Singleton 服务 |
| `RegisterScoped<TService, TImplementation>()` | 注册 Scoped 服务 |
| `RegisterTransient<TService, TImplementation>()` | 注册 Transient 服务 |

**参数说明**:

| 参数 | 类型 | 说明 |
|------|------|------|
| `lifetime` | `ServiceLifetime` | 生命周期（`Singleton` / `Scoped` / `Transient`），默认值视方法而定 |

**使用示例**:
```csharp
public async Task InitializeAsync(IModuleContext context)
{
    context.RegisterSingleton<IConfigService, ConfigService>();
    context.RegisterScoped<IRepository, Repository>();
    context.RegisterTransient<ITransientService, TransientService>();
}
```

---

#### 3.18.2 ModuleBase / ModuleContext

**命名空间**: `KH.WMS.Core.Modularity`

##### ModuleBase

模块基类，提供了 `IModule` 的便捷抽象实现。业务模块应继承此类而非直接实现 `IModule`。

| 抽象成员 | 说明 |
|---------|------|
| `ModuleId` | 模块唯一标识（必须实现） |
| `Name` | 模块名称（必须实现） |
| `Version` | 模块版本（必须实现） |
| `Description` | 模块描述（必须实现） |
| `Author` | 模块作者（必须实现） |

| 虚成员 | 默认值 | 说明 |
|-------|--------|------|
| `Dependencies` | `Array.Empty<ModuleDependency>()` | 依赖列表（可选重写） |
| `InitializeAsync(context)` | 必须重写 | 模块初始化 |
| `StartAsync(cancellationToken)` | `Task.CompletedTask` | 模块启动（可选重写） |
| `StopAsync(cancellationToken)` | `Task.CompletedTask` | 模块停止（可选重写） |
| `ShutdownAsync()` | `Task.CompletedTask` | 模块卸载（可选重写） |

**使用示例**:
```csharp
public class WarehouseModule : ModuleBase
{
    public override Guid ModuleId => Guid.NewGuid();
    public override string Name => "WarehouseModule";
    public override Version Version => new(1, 0, 0);
    public override string Description => "仓库管理模块";
    public override string Author => "KH.WMS Team";

    public override IEnumerable<ModuleDependency> Dependencies =>
        new[] { new ModuleDependency { ModuleName = "OrderModule" } };

    public override async Task InitializeAsync(IModuleContext context)
    {
        context.RegisterScoped<IWarehouseService, WarehouseService>();
        await Task.CompletedTask;
    }
}
```

##### ModuleContext

模块上下文实现，实现 `IModuleContext` 接口。通过 `IServiceCollection` 的 `Add` 方法完成服务注册。

**构造函数参数**:

| 参数 | 类型 | 说明 |
|------|------|------|
| `serviceProvider` | `IServiceProvider` | 服务提供器 |
| `configuration` | `IConfiguration` | 配置对象 |
| `loggerFactory` | `ILoggerFactory` | 日志工厂 |
| `services` | `IServiceCollection` | 服务集合 |

---

#### 3.18.3 ModuleLoader

**命名空间**: `KH.WMS.Core.Modularity`

模块加载器，负责从程序集发现、加载、初始化、启动和停止模块。支持基于依赖关系的拓扑排序。

**构造函数参数**:

| 参数 | 类型 | 说明 |
|------|------|------|
| `logger` | `ILogger<ModuleLoader>` | 日志记录器 |

**属性**:

| 属性 | 类型 | 说明 |
|------|------|------|
| `Modules` | `IReadOnlyList<IModule>` | 获取所有已加载的模块 |

**方法**:

| 方法 | 说明 |
|------|------|
| `LoadModulesFromAssembly(assembly, context?)` | 从单个程序集加载模块 |
| `LoadModulesFromAssemblies(assemblies, context?)` | 从多个程序集加载模块 |
| `InitializeModulesAsync(context)` | 按依赖（拓扑排序）初始化所有模块 |
| `StartModulesAsync(cancellationToken?)` | 启动所有模块 |
| `StopModulesAsync(cancellationToken?)` | 按依赖逆序停止所有模块 |
| `ShutdownModulesAsync()` | 按依赖逆序卸载所有模块 |

**加载流程说明**:

1. `LoadModulesFromAssembly` 扫描程序集中所有实现了 `IModule` 的非抽象类，通过 `Activator.CreateInstance` 实例化并加入模块列表。
2. `InitializeModulesAsync` 内部执行拓扑排序（深度优先搜索 + 循环依赖检测），确保依赖模块先初始化。
3. `StopModulesAsync` 和 `ShutdownModulesAsync` 均按拓扑排序结果的逆序执行。

**使用示例**:
```csharp
var loader = new ModuleLoader(logger);
loader.LoadModulesFromAssemblies(assemblies);
await loader.InitializeModulesAsync(context);
await loader.StartModulesAsync();
// ... 应用运行 ...
await loader.StopModulesAsync();
await loader.ShutdownModulesAsync();
```

---

#### 3.18.4 ModuleDependency

**命名空间**: `KH.WMS.Core.Modularity`

模块依赖描述，定义当前模块所依赖的另一个模块名称及版本范围。

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `ModuleName` | `string` | `""` | 依赖的模块名称 |
| `MinVersion` | `Version` | `1.0.0.0` | 最低版本要求 |
| `MaxVersion` | `Version` | `int.MaxValue` | 最高版本限制 |

**使用示例**:
```csharp
public override IEnumerable<ModuleDependency> Dependencies =>
    new[]
    {
        new ModuleDependency
        {
            ModuleName = "BaseModule",
            MinVersion = new Version(1, 0, 0),
            MaxVersion = new Version(2, 0, 0)
        }
    };
```

---

#### 3.18.5 ModuleExtensions

**命名空间**: `KH.WMS.Core.Modularity`

模块自动发现和注册扩展方法，提供便捷的模块扫描和初始化入口。

##### `ContainerBuilder RegisterModules(this ContainerBuilder builder, IServiceCollection services)`

自动发现所有引用程序集中的模块类型、按依赖排序、注册共享基础设施并将模块服务注册到 Autofac 容器。

| 参数 | 类型 | 说明 |
|------|------|------|
| `builder` | `ContainerBuilder` | Autofac 容器构建器 |
| `services` | `IServiceCollection` | 服务集合（用于获取日志和基础设施） |

**返回值**: `ContainerBuilder`

**内部流程**:
1. `GetReferencedAssemblies()` — 获取所有引用程序集（排除系统程序集）
2. `FindModuleTypes(assemblies, logger)` — 查找所有继承 `ModuleBase` 的非抽象类型
3. `SortModulesByDependencies(moduleTypes, logger)` — 按依赖关系拓扑排序
4. `RegisterSharedInfrastructure(builder, services)` — 注册共享基础设施
5. `RegisterModules(builder, sortedModules, logger)` — 通过反射调用模块的 `RegisterServices(ContainerBuilder)` 方法

##### `Task InitializeModulesAsync(this IServiceProvider serviceProvider)`

通过服务提供器自动发现、按依赖排序、初始化并启动所有模块。

| 参数 | 类型 | 说明 |
|------|------|------|
| `serviceProvider` | `IServiceProvider` | 服务提供器 |

**返回值**: `Task`

**内部流程**:
1. 获取程序集 → 查找模块类型 → 按依赖排序（与 `RegisterModules` 相同流程）
2. 创建 `ModuleContext` 实例
3. 依次调用每个模块的 `InitializeAsync(context)` 和 `StartAsync()`

**使用示例**:
```csharp
// 在 Program.cs 或 Startup 中
builder.Host.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
{
    containerBuilder.RegisterModules(builder.Services);
});

var app = builder.Build();

// 初始化模块（在 UseRouting 之前）
await app.Services.InitializeModulesAsync();
```

---

#### 3.18.6 PluginServiceExtensions

**命名空间**: `KH.WMS.Core.Modularity`

> ⚠️ **注意**: 当前版本中 `PluginServiceExtensions` 的所有方法均已被注释，插件系统暂未启用。以下文档仅作为 API 参考保留，实际调用无效。

插件系统服务注册扩展，提供从指定目录加载外部插件模块的能力。

| 方法 | 说明 | 状态 |
|------|------|------|
| `AddPluginSystem(services, pluginDirectory?)` | 注册插件管理器（单例），默认插件目录 `./plugins` | 已注释 |
| `AddPluginSystemAsync(services, pluginDirectory?, autoLoadPlugins?)` | 注册插件管理器并自动加载所有插件（默认启用自动加载） | 已注释 |

**预计流程**（待启用后生效）:
1. `AddPluginSystem` 注册 `IPluginManager` 为单例
2. `AddPluginSystemAsync` 额外执行：构建服务提供器 → 自动加载所有插件 → 初始化并启动每个插件

---

#### 3.18.7 模块开发指南

##### 创建新模块

```csharp
public class MyBusinessModule : ModuleBase
{
    public override Guid ModuleId => Guid.NewGuid();
    public override string Name => "MyBusinessModule";
    public override Version Version => new(1, 0, 0);
    public override string Description => "我的业务模块";
    public override string Author => "Developer";

    public override IEnumerable<ModuleDependency> Dependencies =>
        new[] { new ModuleDependency { ModuleName = "BaseModule" } };

    public override async Task InitializeAsync(IModuleContext context)
    {
        // 注册此模块的服务
        context.RegisterScoped<IMyService, MyService>();
        await Task.CompletedTask;
    }
}
```

##### 模块注册服务（通过 `RegisterServices` 方法）

`ModuleExtensions.RegisterModules` 通过反射调用模块的 `RegisterServices(ContainerBuilder)` 方法，可用于直接使用 Autofac API 注册服务：

```csharp
public class MyModule : ModuleBase
{
    // ... 其他成员 ...

    public void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterType<MyService>().As<IMyService>().InstancePerLifetimeScope();
    }
}
```

---

#### 3.18.8 依赖解析与排序规则

- 使用**深度优先搜索（DFS）**进行拓扑排序
- 检测循环依赖：若在遍历中再次访问到正在访问中的模块，则抛出 `InvalidOperationException("Circular dependency detected: {ModuleName}")`
- 初始化、启动按拓扑排序正序执行
- 停止、卸载按拓扑排序逆序执行

### 3.19 DependencyInjection 依赖注入

#### 3.19.1 IServiceRegistrar / ServiceRegistrar

**命名空间**: `KH.WMS.Core.DependencyInjection`

##### IServiceRegistrar 接口

服务注册器接口，定义服务注册契约。

| 方法 | 说明 |
|------|------|
| `Register(ContainerBuilder builder, params Assembly[] assemblies)` | 将指定程序集中标记的服务注册到 Autofac 容器 |

##### ServiceRegistrar 类

自动服务注册器实现，扫描程序集中的 `[RegisteredService]` 和 `[SelfRegisteredService]` 特性标记的类，自动注册到 Autofac 容器并附加 AOP 拦截器。

**方法**:

| 方法 | 说明 |
|------|------|
| `Register(builder, assemblies)` | 注册所有标记的服务到容器 |

**拦截器列表**（自动附加到注册的服务上）：

| 拦截器 | 类名 | 说明 |
|--------|------|------|
| 日志拦截器 | `LoggingInterceptor` | 方法调用日志记录 |
| 缓存拦截器 | `CachingInterceptor` | 缓存方法返回值 |
| 配置验证拦截器 | `ConfigValidationInterceptor` | 配置参数验证 |
| 异常拦截器 | `ExceptionInterceptor` | 异常统一处理 |
| 性能拦截器 | `PerformanceInterceptor` | 方法执行耗时监控 |

**内部注册逻辑**:

1. 扫描每个程序集中的所有类，查找标记 `RegisteredServiceAttribute` 或 `SelfRegisteredServiceAttribute` 的类型。
2. 根据特性类型和参数决定注册方式：
   - **`RegisteredServiceAttribute`**（有接口）：注册为 `As(serviceType)`，附加**接口拦截器**（`EnableInterfaceInterceptors`）。
   - **`SelfRegisteredServiceAttribute`**（无接口/自注册）：注册为自身类型，附加**类拦截器**（`EnableClassInterceptors`）。
   - **`WithoutInterceptor = true`**：注册服务但不附加任何拦截器（适用于 LoggerService 等基础服务）。
   - **泛型类型定义**：使用 `RegisterGeneric` 注册开放泛型。
3. 根据 `Lifetime` 属性映射生命周期：
   - `ServiceLifetime.Singleton` → `.SingleInstance()`
   - `ServiceLifetime.Scoped` → `.InstancePerLifetimeScope()`
   - `ServiceLifetime.Transient` → `.InstancePerDependency()`

**使用示例**:
```csharp
// 方式一：通过 ServiceExtensions（推荐）
builder.RegisterModule<ServiceExtensions>();

// 方式二：手动调用
var registrar = new ServiceRegistrar();
registrar.Register(builder, Assembly.GetExecutingAssembly());
```

---

#### 3.19.2 IInterceptorQueryService / InterceptorQueryService

**命名空间**: `KH.WMS.Core.DependencyInjection`

##### IInterceptorQueryService 接口

拦截器查询服务接口，提供运行时查询已注册的拦截器信息的能力。

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `GetRegisteredServices()` | `List<InterceptorServiceInfo>` | 获取所有已注册拦截器的服务列表 |
| `GetServiceInfo(string serviceName)` | `InterceptorServiceInfo?` | 根据服务名称获取拦截器信息 |
| `GetServicesByInterceptor(string interceptorName)` | `List<InterceptorServiceInfo>` | 获取指定拦截器应用的所有服务 |
| `GetAllInterceptorTypes()` | `List<string>` | 获取所有拦截器类型名称 |
| `GetStats()` | `InterceptorStats` | 获取统计信息（总数、按生命周期/拦截器分组） |

##### InterceptorServiceInfo 类

拦截器服务信息。

| 属性 | 类型 | 说明 |
|------|------|------|
| `ServiceName` | `string` | 服务名称 |
| `ServiceType` | `string` | 服务类型（接口或类）的完整名称 |
| `ImplementationType` | `string` | 实现类型的完整名称 |
| `Lifetime` | `string` | 生命周期（Singleton / Scoped / Transient） |
| `HasInterfaceInterceptors` | `bool` | 是否启用了接口拦截器 |
| `HasClassInterceptors` | `bool` | 是否启用了类拦截器 |
| `Interceptors` | `List<string>` | 应用的拦截器列表 |

##### InterceptorStats 类

拦截器统计信息。

| 属性 | 类型 | 说明 |
|------|------|------|
| `TotalServices` | `int` | 服务总数 |
| `ByLifetime` | `Dictionary<string, int>` | 按生命周期分组的服务数量 |
| `ByInterceptor` | `Dictionary<string, int>` | 按拦截器分组的服务数量 |
| `Services` | `List<InterceptorServiceInfo>` | 服务详细信息列表 |

##### InterceptorQueryService 类

拦截器查询服务实现，通过 Autofac 的 `IComponentRegistry` API 查询。

**构造函数参数**:

| 参数 | 类型 | 说明 |
|------|------|------|
| `lifetimeScope` | `ILifetimeScope` | Autofac 生命周期作用域 |

**过滤规则**:

- 仅返回 `KH.WMS.` 命名空间下的项目自定义服务
- 过滤掉 `Microsoft.*`、`Autofac.*`、`System.*`、`Castle.*`、`NetCore.*` 等框架服务
- 仅返回启用了拦截器的服务

**使用示例**:
```csharp
public class DiagnosticsController : ControllerBase
{
    private readonly IInterceptorQueryService _queryService;

    public DiagnosticsController(IInterceptorQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet("interceptors")]
    public IActionResult GetInterceptorInfo()
    {
        var stats = _queryService.GetStats();
        return Ok(new
        {
            total = stats.TotalServices,
            byLifetime = stats.ByLifetime,
            byInterceptor = stats.ByInterceptor,
            services = stats.Services
        });
    }
}
```

---

#### 3.19.3 RegisteredServiceAttribute / SelfRegisteredServiceAttribute

**命名空间**: `KH.WMS.Core.DependencyInjection.ServiceLifetimes`

##### RegisteredServiceAttribute

带接口层自注册服务标记。标记在类上，`ServiceRegistrar` 扫描到后将自动注册该类及其实现的接口。

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Lifetime` | `ServiceLifetime` | `Scoped` | 服务生命周期 |
| `WithoutInterceptor` | `bool` | `false` | 是否不附加 AOP 拦截器 |
| `ServiceType` | `Type?` | `null` | 指定注册的服务类型（接口），为 null 时取第一个接口 |

**使用示例**:
```csharp
// 自动注册为 Scoped，附加拦截器
[RegisteredService(Lifetime = ServiceLifetime.Scoped)]
public class OrderService : IOrderService
{
    // ...
}

// 注册为 Singleton，无拦截器
[RegisteredService(Lifetime = ServiceLifetime.Singleton, WithoutInterceptor = true)]
public class LoggerService : ILoggerService
{
    // ...
}

// 指定服务类型
[RegisteredService(ServiceType = typeof(ICustomService))]
public class CustomService : ICustomService, IDisposable
{
    // ...
}
```

##### SelfRegisteredServiceAttribute

不带接口的自注册服务标记。标记在类上，服务自身作为实现类型注册（不注册接口），适用于无接口的服务类。

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Lifetime` | `ServiceLifetime` | `Scoped` | 服务生命周期 |
| `WithoutInterceptor` | `bool` | `false` | 是否不附加 AOP 拦截器 |

**使用示例**:
```csharp
[SelfRegisteredService(Lifetime = ServiceLifetime.Singleton)]
public class ConfigProvider
{
    // 无接口，自注册
}

[SelfRegisteredService(Lifetime = ServiceLifetime.Transient, WithoutInterceptor = true)]
public class SimpleHelper
{
    // 无拦截器的基础工具类
}
```

---

#### 3.19.4 AssemblyService

**命名空间**: `KH.WMS.Core.DependencyInjection`

程序集服务，提供获取项目内所有引用程序集的能力。

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `GetReferencedAssemblies()` | `List<Assembly>` | 获取项目内的所有程序集列表 |

**内部逻辑**:

1. 优先通过 `DependencyContext.Default.RuntimeLibraries` 获取所有类型为 `project` 的库。
2. 从 `AppContext.BaseDirectory` 加载对应的 `.dll` 文件。
3. 如果无法获取 `DependencyContext`，则直接扫描 `bin` 目录下以 `KH.WMS.` 开头的所有 DLL。

**使用示例**:
```csharp
var assemblies = AssemblyService.GetReferencedAssemblies();
var registrar = new ServiceRegistrar();
registrar.Register(builder, assemblies.ToArray());
```

---

#### 3.19.5 ServiceExtensions

**命名空间**: `KH.WMS.Core.DependencyInjection`

Autofac 模块，作为统一的服务注册入口。继承 `Autofac.Module`，重写 `Load` 方法以自动完成所有程序集的服务注册。

| 方法 | 说明 |
|------|------|
| `Load(ContainerBuilder builder)` | 获取所有引用程序集 → 通过 `ServiceRegistrar` 注册所有标记的服务 |

**使用示例**:
```csharp
// 在 Program.cs 中
builder.Host.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
{
    containerBuilder.RegisterModule<ServiceExtensions>();
});

// 或者在使用 Autofac 作为容器时
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule<ServiceExtensions>();
});
```

---

#### 3.19.6 服务注册最佳实践

| 场景 | 推荐特性 | 说明 |
|------|---------|------|
| 服务有接口 | `[RegisteredService]` | 通过接口注册，支持接口拦截器 |
| 服务无接口 | `[SelfRegisteredService]` | 自注册，支持类拦截器 |
| 基础/工具服务 | 设置 `WithoutInterceptor = true` | 避免拦截器造成的性能开销或递归问题 |
| 泛型服务 | 在泛型类上标记特性 | `ServiceRegistrar` 使用 `RegisterGeneric` 注册开放泛型 |
| 默认生命周期 | — | 默认 `Scoped`，可按需指定 `Singleton` 或 `Transient` |

### 3.20 Helpers 工具

#### 3.20.1 ExpressionHelper

**命名空间**: `KH.WMS.Core.Helpers`

表达式树组合工具类，提供静态方法用于构建和组合 LINQ 表达式。

##### 方法一览

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `True<T>()` | `Expression<Func<T, bool>>` | 创建恒为 `true` 的表达式 |
| `False<T>()` | `Expression<Func<T, bool>>` | 创建恒为 `false` 的表达式 |
| `And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)` | `Expression<Func<T, bool>>` | 组合两个表达式为 **AND** 关系 |
| `Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)` | `Expression<Func<T, bool>>` | 组合两个表达式为 **OR** 关系 |
| `BuildFilter<T>(List<FilterCondition>? filters)` | `Expression<Func<T, bool>>` | 根据过滤条件列表动态构建表达式（自动过滤无效字段） |
| `GetValidPropertyNames(Type type)` | `HashSet<string>` | 获取类型所有可公开读写（`CanRead && CanWrite`）的属性名集合（忽略大小写） |

##### True\<T\>() / False\<T\>()

表达式种子值，用于表达式组合的起始条件。

```csharp
// 示例
var alwaysTrue = ExpressionHelper.True<Order>();          // x => true
var alwaysFalse = ExpressionHelper.False<Order>();        // x => false
```

##### And\<T\>() / Or\<T\>()

将两个表达式树的参数统一后组合（内部使用 `ReplaceExpressionVisitor` 替换参数）。

```csharp
// 示例
Expression<Func<Order, bool>> expr1 = o => o.Status == "Active";
Expression<Func<Order, bool>> expr2 = o => o.Amount > 100;

var combinedAnd = expr1.And(expr2);  // o => o.Status == "Active" AND o.Amount > 100
var combinedOr  = expr1.Or(expr2);   // o => o.Status == "Active" OR o.Amount > 100
```

##### BuildFilter\<T\>(List\<FilterCondition\>? filters)

根据 FilterCondition 列表动态构建查询表达式。内部逻辑：

1. 过滤条件为 null 或空时返回 `True<T>()`。
2. 通过 `GetValidPropertyNames` 获取实体有效属性集，自动过滤无效字段。
3. 对每个有效条件调用 `BuildSingleFilter<T>`，使用 `.And()` 组合。
4. 零个有效条件时返回 `True<T>()`。

**FilterCondition 结构**:

| 属性 | 类型 | 说明 |
|------|------|------|
| `Field` | `string` | 字段名 |
| `Operator` | `string` | 操作符（见下方支持列表） |
| `Value` | `object?` | 过滤值 |

**支持的操作符**:

| 操作符 | 说明 | 示例 |
|--------|------|------|
| `equals` | 相等 | `field == value` |
| `contains` | 字符串包含 | `field.Contains(value)` |
| `startswith` | 字符串以...开头 | `field.StartsWith(value)` |
| `endwith` | 字符串以...结尾 | `field.EndsWith(value)` |
| `gt` | 大于 | `field > value` |
| `lt` | 小于 | `field < value` |
| `gte` | 大于等于 | `field >= value` |
| `lte` | 小于等于 | `field <= value` |
| `notnull` | 不为 null | `field != null` |
| `isnull` | 为 null | `field == null` |
| `in` | 包含于列表 | `field == v1 OR field == v2 OR ...` |

```csharp
// 示例
var filters = new List<FilterCondition>
{
    new() { Field = "Status", Operator = "equals", Value = "Active" },
    new() { Field = "Amount", Operator = "gte", Value = 1000 },
    new() { Field = "Name", Operator = "contains", Value = "test" }
};

var expression = ExpressionHelper.BuildFilter<Order>(filters);
// 结果: o => o.Status == "Active" AND o.Amount >= 1000 AND o.Name.Contains("test")
```

##### GetValidPropertyNames(Type type)

```csharp
var validProps = ExpressionHelper.GetValidPropertyNames(typeof(Order));
// 返回: HashSet<string> 包含所有可读写的属性名（如 "Id", "Name", "Status", "Amount" 等）
```

---

#### 3.20.2 UtilConvert

**命名空间**: `KH.WMS.Core.Helpers`

类型转换工具扩展类，提供丰富的对象类型转换和安全检查方法。

##### 序列化与反序列化

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `Serialize(this object obj, JsonSerializerSettings? formatDate)` | `string?` | 将对象序列化为 JSON 字符串。默认日期格式 `"yyyy-MM-dd HH:mm:ss"`。对象为 null 时返回 null |
| `DeserializeObject<T>(this string json)` | `T` | 将 JSON 字符串反序列化为指定类型。输入 `"{}"` 时自动转为 `"[]"` 处理。输入为空时返回 `default(T)` |
| `ToJson(this object value)` | `string` | 将对象序列化为 JSON 字符串（简易版，无日期格式化） |

```csharp
// 示例
var order = new Order { Name = "测试", CreatedTime = DateTime.Now };
string json = order.Serialize();           // {"Name":"测试","CreatedTime":"2025-01-15 10:30:00"}
string simpleJson = order.ToJson();        // 无日期格式化

var deserialized = json.DeserializeObject<Order>();
```

##### 类型转换方法

**ObjToXxx 系列** — 将对象安全转换为目标类型，转换失败返回默认值（不抛异常）：

| 方法 | 返回类型 | 失败默认值 | 说明 |
|------|---------|-----------|------|
| `ObjToInt(this object)` | `int` | `0` | 支持枚举类型转换 |
| `ObjToInt(this object, int errorValue)` | `int` | `errorValue` | 指定失败默认值 |
| `ObjToLong(this object)` | `long` | `0` | 处理 DBNull |
| `ObjToDecimal(this object)` | `decimal` | `0` | — |
| `ObjToDecimal(this object, decimal errorValue)` | `decimal` | `errorValue` | 指定失败默认值 |
| `ObjToMoney(this object)` | `double` | `0` | 转换为金额（double） |
| `ObjToMoney(this object, double errorValue)` | `double` | `errorValue` | 指定失败默认值 |
| `ObjToBool(this object)` | `bool` | `false` | — |
| `ObjToDate(this object)` | `DateTime` | `DateTime.MinValue` | — |
| `ObjToDate(this object, DateTime errorValue)` | `DateTime` | `errorValue` | 指定失败默认值 |
| `ObjToString(this object)` | `string` | `""` | null → 空字符串，结果自动 Trim |
| `ObjToString(this object, string errorValue)` | `string` | `errorValue` | 指定失败默认值 |
| `DoubleToInt(this double)` | `int` | — | 将 double 转换为 int |

```csharp
// 示例
object val = "42";
int i = val.ObjToInt();               // 42
int i2 = "abc".ObjToInt(-1);          // -1（指定默认值）
long l = val.ObjToLong();             // 42L
decimal d = "3.14".ObjToDecimal();    // 3.14m
double money = "99.99".ObjToMoney();  // 99.99
bool b = "true".ObjToBool();          // true
DateTime dt = "2025-01-15".ObjToDate(); // 2025-01-15 00:00:00
string s = null.ObjToString();        // ""
string s2 = null.ObjToString("N/A");  // "N/A"
```

##### 类型判断方法

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `IsInt(this object obj)` | `bool` | 判断对象是否可以转换为整数（`Int32.TryParse`） |
| `IsDate(this object str)` | `bool` | 判断对象是否为有效日期格式 |
| `IsDate(this object str, out DateTime dateTime)` | `bool` | 判断并输出转换后的日期值 |
| `IsNumber(this string str, string formatString)` | `bool` | 判断字符串是否为数字格式（正则 `^[+-]?\d*[.]?\d*$` 验证） |
| `IsGuid(this string guid)` | `bool` | 判断字符串是否为有效的 Guid 格式 |
| `GetGuid(this string guid, out Guid outId)` | `bool` | 将字符串转换为 Guid 并输出 |
| `IsNotEmptyOrNull(this object thisValue)` | `bool` | 判断对象是否不为空或 null（排除 `"undefined"` 和 `"null"` 字符串） |
| `IsNullOrEmpty(this object thisValue)` | `bool` | 判断对象是否为 null、DBNull 或空字符串 |

```csharp
// 示例
bool isInt = "123".IsInt();            // true
bool isDate = "2025-01-15".IsDate();   // true
bool isNum = "3.14".IsNumber("");      // true
bool isGuid = "..." .IsGuid();         // false or true
bool notEmpty = "hello".IsNotEmptyOrNull(); // true
bool isEmpty = "".IsNullOrEmpty();     // true
```

##### 字符串处理

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `FirstLetterToLower(this string)` | `string` | 将字符串首字母转换为小写，输入为空则返回空字符串 |
| `FirstLetterToUpper(this string)` | `string` | 将字符串首字母转换为大写，输入为空则返回空字符串 |

```csharp
// 示例
string lower = "Hello".FirstLetterToLower();  // "hello"
string upper = "world".FirstLetterToUpper();  // "World"
```

##### 通用类型转换

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `ChangeType(this object value, Type type)` | `object` | 将对象值转换为指定类型。支持枚举、泛型类型、Guid、Version 等特殊类型 |
| `ChangeTypeList(this object value, Type type)` | `object` | 将对象值转换为指定类型的泛型列表。支持括号包裹的字符串格式如 `"(1,2,3)"` 或 `("a","b")` |

**ChangeType 支持的类型**:

- 枚举：字符串或数字转枚举
- 泛型类型：自动提取泛型参数并递归转换
- `Guid`：字符串转 Guid
- `Version`：字符串转 Version
- `IConvertible` 实现类型：通过 `Convert.ChangeType` 转换

```csharp
// 示例
object result = "42".ChangeType(typeof(int));          // 42
object enumVal = "Active".ChangeType(typeof(OrderStatus)); // OrderStatus.Active
object guid = "..." .ChangeType(typeof(Guid));          // Guid 实例

// ChangeTypeList
object list = "(1,2,3)".ChangeType(typeof(int));
// 返回 List<int> { 1, 2, 3 }

object strList = "(\"a\",\"b\")".ChangeType(typeof(string));
// 返回 List<string> { "a", "b" }
```

##### 时间戳转换

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `GetTimeSpmpToDate(this object timeStamp)` | `DateTime` | 将 Unix 时间戳（秒级）转换为本地 DateTime。参数为 null 时返回默认起始日期 `1970-01-01 08:00:00` |
| `DateToTimeStamp(this DateTime thisValue)` | `string` | 将 DateTime 转换为 Unix 时间戳（秒级）的字符串表示 |

```csharp
// 示例
object ts = 1705300000;
DateTime date = ts.GetTimeSpmpToDate();   // 时间戳 → 本地时间

DateTime now = DateTime.Now;
string stamp = now.DateToTimeStamp();     // 当前时间 → Unix 时间戳字符串
```

### 3.21 Constants 常量

#### 3.21.1 CacheConstants

**命名空间**: `KH.WMS.Core.Constants`

缓存键常量定义，按模块分类组织缓存键前缀和动态键生成方法。全局键前缀为 `App:`。

##### 静态键常量

| 类别 | 常量 | 值 | 说明 |
|------|------|-----|------|
| **(全局)** | `KEY_PREFIX` | `"App:"` | 全局缓存键前缀 |
| **User** | `PREFIX` | `"App:User:"` | 用户缓存前缀 |
| | `INFO` | `"App:User:Info:"` | 用户信息 |
| | `PERMISSIONS` | `"App:User:Permissions:"` | 用户权限 |
| | `ROLES` | `"App:User:Roles:"` | 用户角色 |
| | `MENUS` | `"App:User:Menus:"` | 用户菜单 |
| **Config** | `PREFIX` | `"App:Config:"` | 配置缓存前缀 |
| | `SYSTEM` | `"App:Config:System"` | 系统配置 |
| | `DICT` | `"App:Config:Dict:"` | 字典配置前缀 |
| | `SETTING` | `"App:Config:Setting:"` | 设置项前缀 |
| **Data** | `PREFIX` | `"App:Data:"` | 数据缓存前缀 |
| | `ENTITY` | `"App:Data:Entity:"` | 实体缓存前缀 |
| | `LIST` | `"App:Data:List:"` | 列表缓存前缀 |
| **Token** | `PREFIX` | `"App:Token:"` | 令牌缓存前缀 |
| | `ACCESS` | `"App:Token:Access:"` | 访问令牌 |
| | `REFRESH` | `"App:Token:Refresh:"` | 刷新令牌 |
| | `BLACKLIST` | `"App:Token:Blacklist:"` | 令牌黑名单 |
| **RateLimit** | `PREFIX` | `"App:RateLimit:"` | 限流缓存前缀 |
| | `IP` | `"App:RateLimit:IP:"` | IP 级别限流 |
| | `USER` | `"App:RateLimit:User:"` | 用户级别限流 |
| | `API` | `"App:RateLimit:API:"` | API 级别限流 |
| **Captcha** | `PREFIX` | `"App:Captcha:"` | 验证码缓存前缀 |
| | `SMS` | `"App:Captcha:SMS:"` | 短信验证码 |
| | `EMAIL` | `"App:Captcha:Email:"` | 邮箱验证码 |
| **SysParam** | `PREFIX` | `"App:SysParam:"` | 系统参数缓存前缀 |
| **File** | `PREFIX` | `"App:File:"` | 文件缓存前缀 |
| | `INFO` | `"App:File:Info:"` | 文件信息前缀 |

##### 动态键生成方法

| 类别 | 方法 | 返回键示例 | 说明 |
|------|------|-----------|------|
| **User** | `GetUserInfoKey(long userId)` | `"App:User:Info:123"` | 用户信息缓存键 |
| | `GetUserPermissionsKey(long userId)` | `"App:User:Permissions:123"` | 用户权限缓存键 |
| | `GetUserRolesKey(long userId)` | `"App:User:Roles:123"` | 用户角色缓存键 |
| | `GetUserMenusKey(long userId)` | `"App:User:Menus:123"` | 用户菜单缓存键 |
| **Config** | `GetDictKey(string dictType)` | `"App:Config:Dict:Gender"` | 字典项缓存键 |
| | `GetSettingKey(string settingKey)` | `"App:Config:Setting:Key1"` | 设置项缓存键 |
| **Data** | `GetEntityKey<T>(long id)` | `"App:Data:Entity:Order:42"` | 实体缓存键（含类型名） |
| | `GetListKey<T>()` | `"App:Data:List:Order"` | 实体列表缓存键 |
| **Token** | `GetAccessTokenKey(string token)` | `"App:Token:Access:xxx"` | 访问令牌缓存键 |
| | `GetRefreshTokenKey(string token)` | `"App:Token:Refresh:xxx"` | 刷新令牌缓存键 |
| | `GetBlacklistKey(string token)` | `"App:Token:Blacklist:xxx"` | 令牌黑名单缓存键 |
| **RateLimit** | `GetIpKey(string ip)` | `"App:RateLimit:IP:192.168.1.1"` | IP 限流计数键 |
| | `GetUserKey(long userId)` | `"App:RateLimit:User:123"` | 用户限流计数键 |
| | `GetApiKey(string apiPath)` | `"App:RateLimit:API:/api/order"` | API 限流计数键 |
| **Captcha** | `GetSmsKey(string phone)` | `"App:Captcha:SMS:138xxxx"` | 短信验证码缓存键 |
| | `GetEmailKey(string email)` | `"App:Captcha:Email:test@x.com"` | 邮箱验证码缓存键 |
| **SysParam** | `GetKey(string paramCode)` | `"App:SysParam:SYS_TOKEN_EXPIRE_MIN"` | 系统参数缓存键 |
| **File** | `GetFileInfoKey(string fileId)` | `"App:File:Info:uuid"` | 文件信息缓存键 |

##### 系统参数默认值

`SysParam.Defaults` 字典定义了系统参数的默认值，数据丢失或误删时的兜底值：

| 参数编码 | 默认值 | 说明 |
|---------|--------|------|
| `SYS_DEFAULT_PASSWORD` | `"123456"` | 系统默认密码 |
| `SYS_ALLOW_MULTI_LOGIN` | `"1"` | 是否允许多点登录 |
| `SYS_TOKEN_EXPIRE_MIN` | `"30"` | Token 过期时间（分钟） |
| `WH_LOCK_ON_INVENTORY` | `"1"` | 盘点时是否锁定仓库 |
| `LOG_RETAIN_DAYS` | `"90"` | 日志保留天数 |

```csharp
// 示例
string userKey = CacheConstants.User.GetUserInfoKey(123);
// => "App:User:Info:123"

string entityKey = CacheConstants.Data.GetEntityKey<Order>(42);
// => "App:Data:Entity:Order:42"

string sysParamKey = CacheConstants.SysParam.GetKey("SYS_TOKEN_EXPIRE_MIN");
// => "App:SysParam:SYS_TOKEN_EXPIRE_MIN"

// 获取系统参数兜底值
string defaultPassword = CacheConstants.SysParam.Defaults["SYS_DEFAULT_PASSWORD"];
// => "123456"
```

---

#### 3.21.2 ErrorConstants

**命名空间**: `KH.WMS.Core.Constants`

错误消息常量，按类别组织。所有消息为中文描述。

##### Common（通用错误）

| 常量 | 值 | 说明 |
|------|-----|------|
| `OPERATION_FAILED` | `"操作失败"` | 操作失败 |
| `SYSTEM_ERROR` | `"系统错误"` | 系统内部错误 |
| `NETWORK_ERROR` | `"网络错误"` | 网络异常 |
| `TIMEOUT_ERROR` | `"请求超时"` | 请求超时 |
| `UNKNOWN_ERROR` | `"未知错误"` | 未知错误 |

##### Validation（验证错误）

| 常量 | 值 | 说明 |
|------|-----|------|
| `REQUIRED_FIELD_MISSING` | `"必填字段缺失"` | 必填字段缺失 |
| `INVALID_FORMAT` | `"格式不正确"` | 格式错误 |
| `INVALID_VALUE` | `"值不正确"` | 值无效 |
| `OUT_OF_RANGE` | `"值超出范围"` | 值越界 |
| `DUPLICATE_VALUE` | `"值已存在"` | 值重复 |

##### Authentication（认证错误）

| 常量 | 值 | 说明 |
|------|-----|------|
| `UNAUTHORIZED` | `"未授权访问"` | 未授权 |
| `AUTHENTICATION_FAILED` | `"认证失败"` | 认证失败 |
| `LOGIN_FAILED` | `"登录失败"` | 登录失败 |
| `INVALID_CREDENTIALS` | `"用户名或密码错误"` | 凭据无效 |
| `TOKEN_EXPIRED` | `"登录已过期"` | Token 过期 |
| `TOKEN_INVALID` | `"令牌无效"` | Token 无效 |
| `PASSWORD_EXPIRED` | `"密码已过期"` | 密码过期 |
| `ACCOUNT_LOCKED` | `"账户已锁定"` | 账户锁定 |
| `ACCOUNT_DISABLED` | `"账户已禁用"` | 账户禁用 |
| `ACCOUNT_KICKED` | `"账号已在其他设备登录，请重新登录"` | 账号被踢 |

##### Authorization（授权错误）

| 常量 | 值 | 说明 |
|------|-----|------|
| `FORBIDDEN` | `"无权限访问"` | 禁止访问 |
| `INSUFFICIENT_PERMISSIONS` | `"权限不足"` | 权限不足 |
| `ROLE_REQUIRED` | `"需要指定角色"` | 需要角色 |

##### Resource（资源错误）

| 常量 | 值 | 说明 |
|------|-----|------|
| `NOT_FOUND` | `"资源未找到"` | 资源不存在 |
| `ALREADY_EXISTS` | `"资源已存在"` | 资源已存在 |
| `RESOURCE_LOCKED` | `"资源已锁定"` | 资源锁定 |
| `VERSION_CONFLICT` | `"版本冲突"` | 版本冲突 |
| `RESOURCE_DELETED` | `"资源已删除"` | 资源已删除 |

##### Data（数据错误）

| 常量 | 值 | 说明 |
|------|-----|------|
| `CONFLICT_ERROR` | `"数据冲突"` | 数据冲突 |
| `DUPLICATE_ERROR` | `"数据重复"` | 数据重复 |
| `REFERENCE_ERROR` | `"引用数据不存在"` | 引用数据不存在 |
| `CONSTRAINT_ERROR` | `"约束违反"` | 约束违反 |

##### Business（业务错误）

| 常量 | 值 | 说明 |
|------|-----|------|
| `OPERATION_NOT_ALLOWED` | `"不允许的操作"` | 不允许的操作 |
| `INVALID_STATE` | `"状态不正确"` | 状态不正确 |
| `BUSINESS_RULE_VIOLATION` | `"违反业务规则"` | 违反业务规则 |

##### File（文件错误）

| 常量 | 值 | 说明 |
|------|-----|------|
| `FILE_NOT_FOUND` | `"文件不存在"` | 文件不存在 |
| `FILE_TYPE_NOT_ALLOWED` | `"不允许的文件类型"` | 文件类型不允许 |
| `FILE_SIZE_EXCEEDED` | `"文件大小超限"` | 文件超限 |
| `FILE_UPLOAD_FAILED` | `"文件上传失败"` | 上传失败 |
| `FILE_DOWNLOAD_FAILED` | `"文件下载失败"` | 下载失败 |

```csharp
// 示例
throw new BusinessException(ErrorCodes.VALIDATION_ERROR, ErrorConstants.Validation.REQUIRED_FIELD_MISSING);
```

---

#### 3.21.3 HeaderConstants

**命名空间**: `KH.WMS.Core.Constants`

HTTP 请求头常量定义，按功能分组。

##### Authentication（认证相关）

| 常量 | 值 |
|------|-----|
| `AUTHORIZATION` | `"Authorization"` |
| `BEARER_PREFIX` | `"Bearer "` |

##### Tracing（追踪相关）

| 常量 | 值 |
|------|-----|
| `X_REQUEST_ID` | `"X-Request-ID"` |
| `X_CORRELATION_ID` | `"X-Correlation-ID"` |
| `X_TRACE_ID` | `"X-Trace-ID"` |

##### Client（客户端相关）

| 常量 | 值 |
|------|-----|
| `USER_AGENT` | `"User-Agent"` |
| `X_CLIENT_VERSION` | `"X-Client-Version"` |
| `X_CLIENT_ID` | `"X-Client-Id"` |
| `X_DEVICE_ID` | `"X-Device-ID"` |
| `X_DEVICE_TYPE` | `"X-Device-Type"` |
| `X_OS` | `"X-OS"` |
| `X_OS_VERSION` | `"X-OS-Version"` |

##### Context（请求上下文）

| 常量 | 值 |
|------|-----|
| `X_TENANT_ID` | `"X-Tenant-Id"` |
| `X_ORGANIZATION_ID` | `"X-Organization-Id"` |
| `X_LANGUAGE` | `"X-Language"` |
| `X_TIMEZONE` | `"X-Timezone"` |

##### Pagination（分页相关）

| 常量 | 值 |
|------|-----|
| `X_PAGE_INDEX` | `"X-Page-Index"` |
| `X_PAGE_SIZE` | `"X-Page-Size"` |
| `X_TOTAL_COUNT` | `"X-Total-Count"` |

##### Content（内容协商）

| 常量 | 值 |
|------|-----|
| `CONTENT_TYPE` | `"Content-Type"` |
| `ACCEPT` | `"Accept"` |
| `ACCEPT_ENCODING` | `"Accept-Encoding"` |
| `ACCEPT_LANGUAGE` | `"Accept-Language"` |

##### ContentTypes（数据格式）

| 常量 | 值 |
|------|-----|
| `APPLICATION_JSON` | `"application/json"` |
| `APPLICATION_XML` | `"application/xml"` |
| `TEXT_PLAIN` | `"text/plain"` |
| `MULTIPART_FORM_DATA` | `"multipart/form-data"` |

##### Cache（缓存控制）

| 常量 | 值 |
|------|-----|
| `CACHE_CONTROL` | `"Cache-Control"` |
| `ETAG` | `"ETag"` |
| `IF_NONE_MATCH` | `"If-None-Match"` |
| `IF_MODIFIED_SINCE` | `"If-Modified-Since"` |

##### Security（安全相关）

| 常量 | 值 |
|------|-----|
| `X_CSRF_TOKEN` | `"X-CSRF-Token"` |
| `X_REQUESTED_WITH` | `"X-Requested-With"` |
| `X_FORWARDED_FOR` | `"X-Forwarded-For"` |
| `X_REAL_IP` | `"X-Real-IP"` |

##### Api（API 相关）

| 常量 | 值 |
|------|-----|
| `X_API_KEY` | `"X-API-Key"` |
| `X_API_VERSION` | `"X-API-Version"` |
| `X_SIGNATURE` | `"X-Signature"` |
| `X_TIMESTAMP` | `"X-Timestamp"` |

```csharp
// 示例
// 设置请求头
httpClient.DefaultRequestHeaders.Add(HeaderConstants.Authentication.AUTHORIZATION, "Bearer " + token);
httpClient.DefaultRequestHeaders.Add(HeaderConstants.Tracing.X_REQUEST_ID, requestId);
httpClient.DefaultRequestHeaders.Add(HeaderConstants.Context.X_TENANT_ID, tenantId);
```

---

#### 3.21.4 AppSettingsConstants

**命名空间**: `KH.WMS.Core.Constants`

`appsettings.json` 配置键常量定义。

##### 数据库配置

| 常量 | 值 | 说明 |
|------|-----|------|
| `DbConnection` | `"DbConnection"` | 数据库连接字符串配置节点 |
| `DbType_MySql` | `"mysql"` | MySQL 数据库类型标识 |
| `DbType_SqlServer` | `"sqlserver"` | SQL Server 数据库类型标识 |
| `DbType_PostgreSql` | `"postgresql"` | PostgreSQL 数据库类型标识 |
| `DbType_Oracle` | `"oracle"` | Oracle 数据库类型标识 |
| `DbType_Sqlite` | `"sqlite"` | SQLite 数据库类型标识 |

##### 其他配置节点

| 常量 | 值 | 说明 |
|------|-----|------|
| `MiniProfiler` | `"MiniProfiler"` | MiniProfiler 监控配置节点 |
| `Swagger` | `"Swagger"` | Swagger 文档配置节点 |

```csharp
// 示例
string connStr = configuration.GetConnectionString(AppSettingsConstants.DbConnection);
string dbType = configuration["DbType"];  // 例如 "mysql", "sqlserver"

if (configuration.GetSection(AppSettingsConstants.MiniProfiler).Exists())
{
    // MiniProfiler 已配置
}
```

### 3.22 Exceptions 异常

#### 3.22.1 BusinessException

**命名空间**: `KH.WMS.Core.Exceptions`

业务异常，表示业务规则被违反时的异常。继承自 `Exception`。

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `ErrorCode` | `int` | `ErrorCodes.BUSINESS_ERROR (2000)` | 错误码 |
| `Details` | `Dictionary<string, object>` | `new()` | 错误详情（键值对，可用于携带上下文数据） |

**构造函数**:

| 构造函数 | 说明 |
|----------|------|
| `BusinessException()` | 无参数异常 |
| `BusinessException(string message)` | 仅指定错误消息 |
| `BusinessException(string message, Exception innerException)` | 指定错误消息和内部异常 |
| `BusinessException(int errorCode, string message)` | 指定错误码和错误消息 |
| `BusinessException(int errorCode, string message, Dictionary<string, object> details)` | 指定错误码、错误消息和错误详情 |

```csharp
// 示例
throw new BusinessException("库存不足");
throw new BusinessException(ErrorCodes.BUSINESS_ERROR, "操作不允许");
throw new BusinessException(ErrorCodes.STATE_ERROR, "订单状态不正确", new Dictionary<string, object>
{
    { "orderId", 123 },
    { "currentStatus", "已发货" },
    { "expectedStatus", "待发货" }
});
```

---

#### 3.22.2 NotFoundException

**命名空间**: `KH.WMS.Core.Exceptions`

未找到异常，表示请求的资源不存在。继承自 `Exception`。

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `ResourceType` | `string` | `""` | 资源类型名称 |
| `ResourceId` | `object?` | `null` | 资源 ID 或字段值 |

**构造函数**:

| 构造函数 | 说明 |
|----------|------|
| `NotFoundException()` | 无参数异常 |
| `NotFoundException(string message)` | 仅指定错误消息 |
| `NotFoundException(string message, Exception innerException)` | 指定错误消息和内部异常 |
| `NotFoundException(string resourceType, object resourceId)` | 按资源类型 + 资源 ID 查找。消息格式：`"{resourceType} (ID: {resourceId}) 未找到"` |
| `NotFoundException(string resourceType, string fieldName, object fieldValue)` | 按资源类型 + 字段名 + 字段值查找。消息格式：`"{resourceType} ({fieldName}: {fieldValue}) 未找到"` |

```csharp
// 示例
throw new NotFoundException("Order", 42);
// 消息: "Order (ID: 42) 未找到"

throw new NotFoundException("User", "Email", "test@example.com");
// 消息: "User (Email: test@example.com) 未找到"

// 在 CRUD 服务中使用
var entity = await _repository.GetByIdAsync(id);
if (entity == null)
    throw new NotFoundException(typeof(TEntity).Name, id);
```

---

#### 3.22.3 ValidationException / ValidationError

**命名空间**: `KH.WMS.Core.Exceptions`

##### ValidationException

验证异常，表示数据验证失败。继承自 `Exception`。

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Errors` | `List<ValidationError>` | `new()` | 验证错误详情列表 |

**构造函数**:

| 构造函数 | 说明 |
|----------|------|
| `ValidationException()` | 无参数异常 |
| `ValidationException(string message)` | 仅指定错误消息 |
| `ValidationException(string message, Exception innerException)` | 指定错误消息和内部异常 |
| `ValidationException(List<ValidationError> errors)` | 指定验证错误列表。Message 默认为 `"数据验证失败"` |
| `ValidationException(string field, string message)` | 指定单个字段的验证错误。Message 默认为 `"数据验证失败"`，自动添加到 Errors 列表 |

##### ValidationError

验证错误详情。

| 属性 | 类型 | 说明 |
|------|------|------|
| `Field` | `string` | 字段名 |
| `Message` | `string` | 错误消息 |
| `Value` | `object?` | 错误值（用户输入的值） |

```csharp
// 示例
// 方式一：单字段验证失败
throw new ValidationException("Email", "邮箱格式不正确");

// 方式二：多字段验证
var errors = new List<ValidationError>
{
    new() { Field = "Name", Message = "名称不能为空", Value = null },
    new() { Field = "Age", Message = "年龄必须在 0-150 之间", Value = 200 },
    new() { Field = "Email", Message = "邮箱格式不正确", Value = "invalid" }
};
throw new ValidationException(errors);

// 捕获处理
try
{
    // 业务逻辑
}
catch (ValidationException ex)
{
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"字段: {error.Field}, 错误: {error.Message}");
    }
}
```

---

#### 3.22.4 ErrorCodes

**命名空间**: `KH.WMS.Core.Exceptions`

错误码常量定义，按范围分类。同时提供 `GetMessage` 方法根据错误码获取描述。

##### 错误码一览

| 分类 | 错误码 | 常量 | 说明 |
|------|--------|------|------|
| **成功** | `0` | `SUCCESS` | 操作成功 |
| **通用错误 (1xxx)** | `1000` | `SYSTEM_ERROR` | 系统错误 |
| | `1001` | `NETWORK_ERROR` | 网络错误 |
| | `1002` | `TIMEOUT_ERROR` | 请求超时 |
| | `1999` | `UNKNOWN_ERROR` | 未知错误 |
| **业务错误 (2xxx)** | `2000` | `BUSINESS_ERROR` | 业务处理失败 |
| | `2001` | `OPERATION_FAILED` | 操作失败 |
| | `2002` | `STATE_ERROR` | 状态错误 |
| **验证错误 (3xxx)** | `3000` | `VALIDATION_ERROR` | 数据验证失败 |
| | `3001` | `REQUIRED_FIELD_MISSING` | 必填字段缺失 |
| | `3002` | `INVALID_FORMAT` | 格式不正确 |
| | `3003` | `INVALID_VALUE` | 值不正确 |
| | `3004` | `OUT_OF_RANGE` | 值超出范围 |
| **认证授权错误 (4xxx)** | `4001` | `UNAUTHORIZED` | 未授权 |
| | `4002` | `TOKEN_EXPIRED` | 令牌已过期 |
| | `4003` | `TOKEN_INVALID` | 令牌无效 |
| | `4004` | `FORBIDDEN` | 无权访问 |
| | `4005` | `LOGIN_FAILED` | 登录失败 |
| | `4006` | `PASSWORD_EXPIRED` | 密码已过期 |
| **资源错误 (5xxx)** | `5001` | `NOT_FOUND` | 资源未找到 |
| | `5002` | `ALREADY_EXISTS` | 资源已存在 |
| | `5003` | `RESOURCE_LOCKED` | 资源已锁定 |
| | `5004` | `VERSION_CONFLICT` | 版本冲突 |
| **并发错误 (6xxx)** | `6001` | `CONFLICT_ERROR` | 数据冲突 |
| | `6002` | `DUPLICATE_ERROR` | 数据重复 |
| **外部服务错误 (7xxx)** | `7001` | `EXTERNAL_SERVICE_ERROR` | 外部服务错误 |
| | `7002` | `EXTERNAL_SERVICE_UNAVAILABLE` | 外部服务不可用 |

##### GetMessage(int errorCode)

根据错误码返回中文描述。

```csharp
// 示例
string msg = ErrorCodes.GetMessage(ErrorCodes.NOT_FOUND);
// => "资源未找到"

string msg2 = ErrorCodes.GetMessage(ErrorCodes.TOKEN_EXPIRED);
// => "令牌已过期"

string unknown = ErrorCodes.GetMessage(9999);
// => "未知错误"
```

##### 使用建议

| 场景 | 推荐异常类型 | 推荐错误码 |
|------|------------|-----------|
| 通用参数校验失败 | `ValidationException` | `VALIDATION_ERROR (3000)` |
| 必填字段缺失 | `ValidationException` | `REQUIRED_FIELD_MISSING (3001)` |
| 资源不存在 | `NotFoundException` | `NOT_FOUND (5001)` |
| 无权限操作 | `BusinessException` | `FORBIDDEN (4004)` |
| Token 过期 | `BusinessException` | `TOKEN_EXPIRED (4002)` |
| 业务规则违反 | `BusinessException` | `BUSINESS_ERROR (2000)` |
| 数据重复 | `BusinessException` | `DUPLICATE_ERROR (6002)` |
| 外部服务不可用 | `BusinessException` | `EXTERNAL_SERVICE_UNAVAILABLE (7002)` |

### 3.23 Api Responses

#### 3.23.1 ApiResponse / ApiResponse\<T\>

**命名空间**: `KH.WMS.Core.Api.Responses`

**说明**: 统一的 API 响应格式，所有控制器方法统一返回此类型。包含三个变体：
- `ApiResponse` — 基础响应（无泛型 Data）
- `ApiResponse<T>` — 泛型响应，提供强类型的 `Data` 属性
- `ApiResponseExtensions` — 扩展方法，可将响应转换为 MVC `ActionResult`

---

##### ApiResponse（基础响应）

```csharp
public class ApiResponse
{
    public int Code { get; set; } = ResponseCode.SUCCESS;          // 响应码
    public string Message { get; set; } = "操作成功";               // 响应消息
    public long Timestamp { get; set; }                             // UTC 时间戳（毫秒）
    public object? Data { get; set; }                               // 响应数据
    public string? TraceId { get; set; }                            // 请求跟踪 ID
}
```

**属性说明**:

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Code` | `int` | `200` | 响应码，参考 `ResponseCode` 常量 |
| `Message` | `string` | `"操作成功"` | 响应消息 |
| `Timestamp` | `long` | 当前 UTC 时间戳 | 以毫秒为单位的 Unix 时间戳 |
| `Data` | `object?` | `null` | 响应负载数据 |
| `TraceId` | `string?` | `null` | 请求跟踪 ID，用于链路追踪 |

**静态工厂方法**:

| 方法 | 说明 |
|------|------|
| `Ok(object? data = null, string message = "操作成功")` | 创建成功响应（Code=200） |
| `Fail(int code, string message)` | 创建失败响应 |
| `NotFound(string message = "资源未找到")` | 创建 404 未找到响应 |
| `ValidationError(string message, object? errors = null)` | 创建 422 验证错误响应（Data 可携带详细错误） |
| `Unauthorized(string message = "未授权访问")` | 创建 401 未授权响应 |
| `Error(string message = "服务器内部错误")` | 创建 500 服务器错误响应 |

**使用示例**:
```csharp
// 成功
return ApiResponse.Ok(data, "查询成功");

// 失败
return ApiResponse.Fail(400, "参数错误");

// 未找到
return ApiResponse.NotFound("订单不存在");
```

---

##### ApiResponse\<T\>（泛型响应）

```csharp
public class ApiResponse<T> : ApiResponse
{
    public new T? Data { get; set; }
}
```

**工厂方法**:

| 方法 | 说明 |
|------|------|
| `Ok(T? data, string message = "操作成功")` | 创建强类型成功响应 |
| `Fail(int code, string message)` | 创建强类型失败响应 |

**使用示例**:
```csharp
// 返回强类型数据
return ApiResponse<UserDto>.Ok(user, "查询成功");

// 或直接使用
return ApiResponse<UserDto>.Ok(null, "暂无数据");
```

---

##### ApiResponseExtensions（ActionResult 扩展）

| 方法 | 说明 |
|------|------|
| `ToActionResult(this ApiResponse response)` | 将响应转换为带 HTTP 状态码的 `ActionResult` |
| `ToActionResult<T>(this ApiResponse<T> response)` | 泛型版本的 ActionResult 转换 |

**使用示例**:
```csharp
return ApiResponse.NotFound("资源未找到").ToActionResult();
// Result: HTTP 404, Body: { "code": 404, "message": "资源未找到", ... }
```

#### 3.23.2 PagedResponse\<T\>

**命名空间**: `KH.WMS.Core.Api.Responses`

**说明**: 分页响应包装，继承自 `ApiResponse<PagedResult<T>>`，用于统一分页查询的返回格式。

```csharp
public class PagedResponse<T> : ApiResponse<PagedResult<T>>
```

**静态工厂方法**:

| 方法 | 说明 |
|------|------|
| `Ok(List<T> items, int total, int pageIndex, int pageSize, string message = "查询成功")` | 创建成功分页响应，内部调用 `PagedResult<T>.Create()` |
| `Empty(int pageIndex = 1, int pageSize = 20, string message = "暂无数据")` | 创建空分页响应，内部调用 `PagedResult<T>.Empty()` |

**响应 JSON 结构**:
```json
{
  "code": 200,
  "message": "查询成功",
  "timestamp": 1704067200000,
  "traceId": null,
  "data": {
    "items": [ ... ],
    "total": 100,
    "pageIndex": 1,
    "pageSize": 20,
    "pageCount": 5,
    "hasPrevious": false,
    "hasNext": true
  }
}
```

**使用示例**:
```csharp
// 控制器中返回分页结果
var (items, total) = await _service.GetPagedAsync(query);
return PagedResponse<OrderDto>.Ok(items, total, query.PageIndex, query.PageSize);
```

#### 3.23.3 Pagination / PagedResult\<T\>

**命名空间**: `KH.WMS.Core.Api.Responses`

---

##### Pagination（分页请求参数）

**说明**: 分页查询的请求参数基类，提供页码和每页数量的定义。`QueryRequestDto` 和 `AdvancedQueryRequestDto` 均继承此类。

```csharp
public class Pagination
{
    public int PageIndex { get; set; }    // 页码（从1开始，默认1）
    public int PageSize { get; set; }     // 每页数量（默认20）
    public int Skip => (PageIndex - 1) * PageSize;  // 计算跳过数量
    public int Take => PageSize;                     // 获取数量
}
```

**属性说明**:

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `PageIndex` | `int` | `1` | 当前页码（从 1 开始，自动限制最小值为 1） |
| `PageSize` | `int` | `20` | 每页记录数（自动限制最小值为 1） |
| `Skip` | `int` | 计算值 | 分页跳过的记录数 `(PageIndex - 1) * PageSize` |
| `Take` | `int` | `PageSize` | 返回的记录数，等价于 `PageSize` |

**静态方法**:

| 方法 | 说明 |
|------|------|
| `Create(int pageIndex = 1, int pageSize = 20)` | 创建分页参数实例 |

**使用示例**:
```csharp
var paging = Pagination.Create(pageIndex: 1, pageSize: 10);
// 等价于 new Pagination { PageIndex = 1, PageSize = 10 }
```

---

##### PagedResult\<T\>（分页结果）

**说明**: 分页数据的封装结果，包含分页信息和数据列表。

```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; }         // 数据列表
    public int Total { get; set; }             // 总记录数
    public int PageIndex { get; set; }         // 当前页码
    public int PageSize { get; set; }          // 每页数量
    public int PageCount { get; }              // 总页数（计算属性）
    public bool HasPrevious { get; }           // 是否有上一页
    public bool HasNext { get; }               // 是否有下一页
}
```

**属性说明**:

| 属性 | 类型 | 说明 |
|------|------|------|
| `Items` | `List<T>` | 当前页的数据列表 |
| `Total` | `int` | 总记录数 |
| `PageIndex` | `int` | 当前页码 |
| `PageSize` | `int` | 每页数量 |
| `PageCount` | `int` | 总页数（`Ceiling(Total / PageSize)`） |
| `HasPrevious` | `bool` | 是否有上一页（`PageIndex > 1`） |
| `HasNext` | `bool` | 是否有下一页（`PageIndex < PageCount`） |

**静态方法**:

| 方法 | 说明 |
|------|------|
| `Create(List<T> items, int total, int pageIndex, int pageSize)` | 创建分页结果 |
| `Empty(int pageIndex = 1, int pageSize = 20)` | 创建空分页结果（Items 为空列表） |

**使用示例**:
```csharp
// 创建分页结果
var pagedResult = PagedResult<Order>.Create(items, total, pageIndex, pageSize);

// 空分页
var emptyResult = PagedResult<Order>.Empty();
```

#### 3.23.4 ResponseCode

**命名空间**: `KH.WMS.Core.Api.Responses`

**说明**: 响应码常量定义类，涵盖 HTTP 协议风格的 2xx/4xx/5xx 状态码。该类用于 `ApiResponse.Code` 属性，指示一次 API 调用的结果状态。

**常量定义**:

| 分类 | 常量名 | 值 | 说明 |
|------|--------|-----|------|
| **成功 2xx** | `SUCCESS` | `200` | 操作成功 |
| | `CREATED` | `201` | 创建成功 |
| | `ACCEPTED` | `202` | 请求已接受 |
| | `NO_CONTENT` | `204` | 无内容返回 |
| **客户端错误 4xx** | `BAD_REQUEST` | `400` | 请求参数错误 |
| | `UNAUTHORIZED` | `401` | 未授权访问 |
| | `FORBIDDEN` | `403` | 无权限访问 |
| | `NOT_FOUND` | `404` | 资源未找到 |
| | `METHOD_NOT_ALLOWED` | `405` | 请求方法不允许 |
| | `REQUEST_TIMEOUT` | `408` | 请求超时 |
| | `CONFLICT` | `409` | 数据冲突 |
| | `VALIDATION_ERROR` | `422` | 数据验证失败 |
| | `RATE_LIMIT_EXCEEDED` | `429` | 请求过于频繁 |
| **服务器错误 5xx** | `INTERNAL_SERVER_ERROR` | `500` | 服务器内部错误 |
| | `NOT_IMPLEMENTED` | `501` | 功能未实现 |
| | `BAD_GATEWAY` | `502` | 网关错误 |
| | `SERVICE_UNAVAILABLE` | `503` | 服务不可用 |
| | `GATEWAY_TIMEOUT` | `504` | 网关超时 |

**静态方法**:

| 方法 | 说明 |
|------|------|
| `GetMessage(int code)` | 获取响应码对应的默认消息文本（如 200 → "操作成功"） |
| `GetHttpStatusCode(int code)` | 将业务响应码映射为 HTTP 状态码（如 422 → 422，200/201/202/204 → 200） |

**使用示例**:
```csharp
var message = ResponseCode.GetMessage(404);     // "资源未找到"
var httpCode = ResponseCode.GetHttpStatusCode(422);  // 422
```

### 3.24 Models / DTOs

#### 3.24.1 BaseEntity\<T\> / RootEntity

**命名空间**: `KH.WMS.Core.Models.Entities`

---

##### RootEntity（审计字段基类）

**说明**: 实体审计字段基类，包含创建和修改的审计信息。`BaseEntity<T>` 继承自此类。

```csharp
public class RootEntity
{
    public string? CreatedBy { get; set; }          // 创建人
    public string? CreatedByName { get; set; }      // 创建人名称
    public DateTime CreatedTime { get; set; }        // 创建时间
    public string? LastModifiedBy { get; set; }      // 最后修改人
    public string? LastModifiedByName { get; set; }  // 最后修改人名称
    public DateTime? LastModifiedTime { get; set; }  // 最后修改时间
}
```

**属性说明**:

| 属性 | 类型 | 数据库映射 | 说明 |
|------|------|-----------|------|
| `CreatedBy` | `string?` | `VARCHAR(50)`, Nullable | 创建人 ID |
| `CreatedByName` | `string?` | `VARCHAR(50)`, Nullable | 创建人名称 |
| `CreatedTime` | `DateTime` | `DATETIME`, NotNull | 创建时间 |
| `LastModifiedBy` | `string?` | `VARCHAR(50)`, Nullable | 最后修改人 ID |
| `LastModifiedByName` | `string?` | `VARCHAR(50)`, Nullable | 最后修改人名称 |
| `LastModifiedTime` | `DateTime?` | `DATETIME`, Nullable | 最后修改时间 |

> 审计字段由 `CrudService` 的 `FillAuditFields()` 方法自动填充，开发者通常无需手动设置。

---

##### BaseEntity\<T\>（实体基类）

**说明**: 所有数据库实体应继承此类，提供泛型主键。

```csharp
public abstract class BaseEntity<T> : RootEntity where T : struct
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "主键ID")]
    public T Id { get; set; }
}
```

**属性说明**:

| 属性 | 类型 | SqlSugar 特性 | 说明 |
|------|------|--------------|------|
| `Id` | `T` | `IsPrimaryKey = true, IsIdentity = true` | 主键，自增 |

**继承关系**:
```
RootEntity
  └── BaseEntity<T>        (通用基类，如 BaseEntity<long>)
```

**使用示例**:
```csharp
// 实体定义
[SugarTable("wms_order")]
public class Order : BaseEntity<long>
{
    public string OrderNo { get; set; }
    public string Status { get; set; }
    // 审计字段从 RootEntity 继承
}
```

#### 3.24.2 BaseDto / CreateRequestDto / UpdateRequestDto / DeleteRequestDto / BatchDeleteRequestDto

**命名空间**: `KH.WMS.Core.Models.Dtos`

---

##### BaseDto（DTO 基类）

**说明**: 所有数据传输对象应继承此类，提供 `Id` 属性。

```csharp
public abstract class BaseDto
{
    public long Id { get; set; }
}
```

---

##### CreateRequestDto（创建请求基类）

**说明**: 新增操作的请求 DTO 基类。本身为空基类，供子类扩展业务字段。

```csharp
public abstract class CreateRequestDto
{
    // 子类在此定义新增所需的业务字段
}
```

---

##### UpdateRequestDto（更新请求基类）

**说明**: 更新操作的请求 DTO 基类，包含 `Id` 属性标识要更新的记录。

```csharp
public abstract class UpdateRequestDto
{
    public long Id { get; set; }
}
```

---

##### DeleteRequestDto（删除请求 DTO）

**说明**: 单条删除的请求 DTO。

```csharp
public class DeleteRequestDto
{
    public long Id { get; set; }
}
```

---

##### BatchDeleteRequestDto（批量删除请求 DTO）

**说明**: 批量删除的请求 DTO，支持物理删除选项。

```csharp
public class BatchDeleteRequestDto
{
    public List<long> Ids { get; set; } = new();    // 要删除的 ID 列表
    public bool IsPhysicalDelete { get; set; } = false;  // 是否物理删除（true=物理删除，false=逻辑删除）
}
```

**属性说明**:

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Ids` | `List<long>` | `new()` | 待删除记录的 ID 列表 |
| `IsPhysicalDelete` | `bool` | `false` | 是否物理删除（`false` 时为逻辑删除，更新 `IsDeleted` 字段） |

#### 3.24.3 QueryRequestDto / AdvancedQueryRequestDto

**命名空间**: `KH.WMS.Core.Models.Dtos`

---

##### QueryRequestDto（基础查询请求 DTO）

**说明**: 基础分页查询请求 DTO，继承自 `Pagination`，提供关键字搜索和时间范围过滤。适用于列表查询等简单查询场景。

```csharp
public abstract class QueryRequestDto : Pagination
{
    public string? Keyword { get; set; }       // 搜索关键字
    public DateTime? StartTime { get; set; }   // 开始时间
    public DateTime? EndTime { get; set; }     // 结束时间
}
```

---

##### AdvancedQueryRequestDto（高级查询请求 DTO）

**说明**: 高级分页查询请求 DTO，继承自 `Pagination`，在基础分页之上支持关键字搜索、多字段排序和动态过滤条件。适用于 `CrudService` 的通用查询方法。

```csharp
public class AdvancedQueryRequestDto : Pagination
{
    public string? Keyword { get; set; }                         // 搜索关键字
    public List<SortCondition>? SortConditions { get; set; }     // 多字段排序条件
    public List<FilterCondition>? Filters { get; set; }          // 动态过滤条件
}
```

**使用示例**:
```csharp
// 前端传入 JSON:
{
  "pageIndex": 1,
  "pageSize": 20,
  "keyword": "ABC",
  "sortConditions": [
    { "field": "CreatedTime", "direction": "desc" }
  ],
  "filters": [
    { "field": "Status", "operator": "equals", "value": "Active" }
  ]
}
```

#### 3.24.4 FilterCondition / SortCondition

**命名空间**: `KH.WMS.Core.Models.Dtos`

---

##### SortCondition（排序条件）

**说明**: 多字段排序条件，用于 `AdvancedQueryRequestDto.SortConditions`。

```csharp
public class SortCondition
{
    public string Field { get; set; } = string.Empty;     // 排序字段（对应实体属性名）
    public string Direction { get; set; } = "asc";         // 排序方向: "asc" 或 "desc"
}
```

**属性说明**:

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Field` | `string` | `""` | 排序字段名，对应实体属性名（如 `CreatedTime`） |
| `Direction` | `string` | `"asc"` | 排序方向，`"asc"`（升序）或 `"desc"`（降序） |

---

##### FilterCondition（过滤条件）

**说明**: 动态过滤条件，用于 `AdvancedQueryRequestDto.Filters`。

```csharp
public class FilterCondition
{
    public string Field { get; set; } = string.Empty;     // 字段名
    public string Operator { get; set; } = "contains";    // 操作符
    public object? Value { get; set; }                     // 过滤值
}
```

**支持的操作符**:

| 操作符 | 说明 | 示例 |
|--------|------|------|
| `equals` | 等于 | `{ field: "Status", op: "equals", value: "Active" }` |
| `contains` | 包含（模糊匹配） | `{ field: "Name", op: "contains", value: "ABC" }` |
| `gt` | 大于 | `{ field: "Age", op: "gt", value: 18 }` |
| `lt` | 小于 | `{ field: "Price", op: "lt", value: 100 }` |
| `gte` | 大于等于 | `{ field: "Quantity", op: "gte", value: 10 }` |
| `lte` | 小于等于 | `{ field: "Quantity", op: "lte", value: 100 }` |
| `in` | 在列表中 | `{ field: "Status", op: "in", value: ["Active", "Pending"] }` |
| `notnull` | 不为空 | `{ field: "Email", op: "notnull" }` |
| `isnull` | 为空 | `{ field: "DeletedBy", op: "isnull" }` |
| `startswith` | 以...开始 | `{ field: "OrderNo", op: "startswith", value: "PO-" }` |
| `endwith` | 以...结束 | `{ field: "OrderNo", op: "endwith", value: "-001" }` |

#### 3.24.5 ExportColumnDto / ExportRequestDto

**命名空间**: `KH.WMS.Core.Models.Dtos`

---

##### ExportColumnDto（导出列配置）

**说明**: 导出 Excel 时的列配置，用于生成中文表头、展开 ExtData 字段和翻译字典值。

```csharp
public class ExportColumnDto
{
    public string Prop { get; set; } = "";                              // 字段名（对应实体属性）
    public string Label { get; set; } = "";                             // 中文列标题
    public Dictionary<string, string>? DictMap { get; set; }            // 字典映射（value → label）
}
```

**属性说明**:

| 属性 | 类型 | 说明 |
|------|------|------|
| `Prop` | `string` | 字段名，对应实体属性名，如 `"Status"`、`"ExtData.AttrName"` |
| `Label` | `string` | 中文表头，如 `"状态"` |
| `DictMap` | `Dictionary<string, string>?` | 字典值翻译映射，如 `{ "0": "禁用", "1": "启用" }` |

---

##### ExportRequestDto（导出请求参数）

**说明**: 导出操作的请求 DTO，继承自 `AdvancedQueryRequestDto`，支持按查询条件导出。

```csharp
public class ExportRequestDto : AdvancedQueryRequestDto
{
    public List<ExportColumnDto>? Columns { get; set; }    // 导出列配置，为空时直接导出实体
    public bool ExportAll { get; set; } = true;             // 是否导出全部数据
}
```

**属性说明**:

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Columns` | `List<ExportColumnDto>?` | `null` | 导出列配置（含中文表头、字典映射），`null` 时直接导出实体全部字段 |
| `ExportAll` | `bool` | `true` | 是否导出全部数据（`true`=跳过分页导出全部，`false`=仅导出当前页） |

#### 3.24.6 ServiceResult / ServiceResult\<T\>

**命名空间**: `KH.WMS.Core.Models`

**说明**: 服务层操作结果类型，用于替代 `(bool Success, string? Message)` 和 `(bool Success, string? Message, T? Data)` 元组模式，提供更语义化的结果表达。

---

##### ServiceResult（无数据结果）

```csharp
public class ServiceResult
{
    public bool Success { get; init; }       // 是否成功
    public string? Message { get; init; }    // 消息
}
```

**静态工厂方法**:

| 方法 | 说明 |
|------|------|
| `Ok(string? message = null)` | 创建成功结果，`Success = true` |
| `Fail(string message)` | 创建失败结果，`Success = false` |

**隐式转换**:

```csharp
// 支持从 (bool, string?) 元组隐式转换
public static implicit operator ServiceResult((bool Success, string? Message) tuple)
```

**使用示例**:
```csharp
// 方式一：工厂方法
return ServiceResult.Ok("操作成功");
return ServiceResult.Fail("操作失败");

// 方式二：隐式转换（兼容旧代码）
return (true, "操作成功");
return (false, "参数错误");
```

---

##### ServiceResult\<T\>（带数据结果）

```csharp
public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }    // 返回数据
}
```

**静态工厂方法**:

| 方法 | 说明 |
|------|------|
| `Ok(T data, string? message = null)` | 创建带数据的成功结果 |
| `Fail(string message)` | 创建失败结果（无数据） |

**使用示例**:
```csharp
// 成功带数据
return ServiceResult<UserDto>.Ok(user, "查询成功");

// 失败
return ServiceResult<UserDto>.Fail("用户不存在");
```

### 3.25 Configuration 配置

**命名空间**: `KH.WMS.Core.Configuration`

**说明**: 配置提供者是一个对 `IConfiguration` 的轻量封装，提供类型绑定、配置节访问和存在性检查等便捷方法。通过 DI 容器注入，可在任意服务中获取配置。

---

#### 3.25.1 IConfigurationProvider / ConfigurationProvider

**接口定义**:

```csharp
public interface IConfigurationProvider
{
    string? GetValue(string key);                          // 获取配置值
    IConfigurationSection GetSection(string key);           // 获取配置节
    T Bind<T>(string sectionName) where T : class, new();   // 绑定到新对象
    void Bind<T>(string sectionName, T instance) where T : class;  // 绑定到已有对象
    string? GetConnectionString(string name);               // 获取连接字符串
    bool Exists(string key);                                // 检查配置是否存在
}
```

**方法说明**:

| 方法 | 说明 | 示例 |
|------|------|------|
| `GetValue(string key)` | 获取指定键的原始字符串值 | `provider.GetValue("App:AppName")` |
| `GetSection(string key)` | 获取指定路径的 `IConfigurationSection` | `provider.GetSection("Jwt")` |
| `Bind<T>(string sectionName)` | 将配置节绑定到指定类型的强类型对象（自动 `new()`） | `provider.Bind<JwtOptions>("Jwt")` |
| `Bind<T>(string sectionName, T instance)` | 将配置节绑定到已有实例（合并/覆盖） | `provider.Bind("Jwt", options)` |
| `GetConnectionString(string name)` | 从 `ConnectionStrings` 节获取连接字符串 | `provider.GetConnectionString("DefaultConnection")` |
| `Exists(string key)` | 检查指定键在配置中是否存在 | `provider.Exists("Jwt:Secret")` |

**注册方式**: `ConfigurationProvider` 通过 DI 容器自动注册。使用 `AddInfrastructure()` 或直接调用扩展方法后，可通过构造函数注入使用。

```csharp
// 依赖注入自动注册（由 IConfiguration 单例驱动）
services.AddSingleton<IConfigurationProvider, ConfigurationProvider>();
```

**使用示例**:
```csharp
public class MyService
{
    private readonly IConfigurationProvider _config;

    public MyService(IConfigurationProvider config)
    {
        _config = config;
    }

    public void DoSomething()
    {
        // 获取单值
        var appName = _config.GetValue("App:AppName");

        // 强类型绑定
        var jwtOptions = _config.Bind<JwtTokenOptions>("Jwt");

        // 获取连接字符串
        var connStr = _config.GetConnectionString("DefaultConnection");

        // 检查配置是否存在
        if (_config.Exists("MiniProfiler"))
        {
            // 存在 MiniProfiler 配置节
        }
    }
}
```

### 3.26 Setup 配置入口

**说明**: Setup 层提供了所有核心模块的集中式配置入口。开发者可通过 `AddInfrastructure()` 一键注册所有基础设施，也可按需调用各子模块的独立 Setup 方法。

---

#### 3.26.1 ServiceCollectionSetup

**命名空间**: `KH.WMS.Core.Setup`

**说明**: 服务集合配置的统一入口。`AddInfrastructure()` 方法内部按顺序调用各子模块的 Setup 方法，完成所有基础设施服务的注册。

**扩展方法**:

| 方法 | 说明 |
|------|------|
| `AddInfrastructure(this IServiceCollection, IConfiguration, IWebHostEnvironment)` | 一键添加所有基础设施服务 |

**内部注册顺序**:

| 序号 | 模块 | 调用方法 | 说明 |
|------|------|---------|------|
| 1 | 数据库 | `AddSqlSugarSetup(configuration)` | 注册 SqlSugar ORM 和仓储 |
| 2 | 缓存 | `AddCacheSetup(configuration)` | 注册内存缓存和 `ICacheService` |
| 3 | 认证 | `AddAuthenticationSetup(configuration)` | 注册 JWT Bearer 认证 |
| 4 | 日志 | `AddLoggingSetup(configuration)` | 注册 `ILoggerService` |
| 5 | 性能监控 | `AddMonitoringSetup(configuration, environment)` | 注册 MiniProfiler |
| 6 | API 文档 | `AddApiDocumentationSetup(configuration)` | 注册 Swagger |
| 7 | CORS | `AddCustomCors(configuration)` | 注册跨域策略 |
| 8 | 限流 | `AddRateLimiting(configuration)` | 注册滑动窗口限流 |
| 9 | HTTP 客户端 | `AddHttpClient()` | 注册 `IHttpClientFactory` |
| 10 | MVC 过滤器 | `options.Filters.Add<...>()` | 注册 `ApiAuthorizeFilter` 和 `TraceIdResultFilter` |

**使用示例**:
```csharp
// Program.cs 中一键注册
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// 或按需单独注册
builder.Services.AddSqlSugarSetup(builder.Configuration);
builder.Services.AddCacheSetup(builder.Configuration);
builder.Services.AddAuthenticationSetup(builder.Configuration);
```

#### 3.26.2 MiddlewareSetup

**命名空间**: `KH.WMS.Core.Setup`

**说明**: 中间件管道的统一配置入口。`UseCustomMiddleware()` 方法按预定义顺序配置完整的中间件管道，确保各中间件按正确的依赖顺序执行。

**扩展方法**:

| 方法 | 说明 |
|------|------|
| `UseCustomMiddleware(this IApplicationBuilder, IWebHostEnvironment)` | 按推荐顺序使用所有自定义中间件 |

**中间件注册顺序**:

| 序号 | 中间件 | 方法 | 说明 |
|------|--------|------|------|
| ① | 异常处理 | `UseExceptionHandling()` | 最外层异常捕获，统一错误格式 |
| ② | License 验证 | `UseLicenseValidation()` | 验证许可证有效性，自动初始化默认授权 |
| ③ | HTTPS 重定向 | `UseHttpsRedirection()` | 仅生产环境启用 |
| ④ | 静态文件 | `UseCustomStaticFiles(env)` | 提供静态文件服务 |
| ⑤ | CORS | `UseCustomCors()` | 跨域资源共享 |
| ⑥ | 请求日志 | `UseRequestLogging()` | 记录 HTTP 请求日志 |
| ⑦ | 路由 | `UseRouting()` | MVC 路由匹配 |
| ⑧ | 性能监控 | `UseMiniProfilerCustom()` | MiniProfiler 性能分析 |
| ⑨ | 认证 | `UseAuthentication()` | JWT Bearer 认证 |
| ⑩ | 授权 | `UseAuthorization()` | 授权中间件 |
| ⑪ | 端点映射 | `UseEndpoints()` | 映射 Controller 和 RazorPages |

**使用示例**:
```csharp
var app = builder.Build();
app.UseCustomMiddleware(app.Environment);
app.Run();
```

#### 3.26.3 HostSetup

**命名空间**: `KH.WMS.Core.Setup`

**说明**: 主机（`IHostBuilder`）的配置入口，提供配置文件加载和 Serilog 日志的初始化配置。

**扩展方法**:

| 方法 | 说明 |
|------|------|
| `ConfigureDefaults(this IHostBuilder)` | 配置默认服务：加载 `appsettings.json`、`appsettings.{Environment}.json` 和环境变量，注册 `IHttpContextAccessor` |
| `AddCustomConfiguration(this IHostBuilder)` | 配置 Serilog：从配置文件读取日志设置，添加控制台和文件输出（按天滚动），日志文件路径为 `Logs/log-.txt`，错误日志路径为 `Logs/error-.txt` |

**使用示例**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// 配置默认服务
builder.Host.ConfigureDefaults();

// 配置 Serilog 日志
builder.Host.AddCustomConfiguration();
```

#### 3.26.4 CacheSetup

**命名空间**: `KH.WMS.Core.Setup`

**说明**: 缓存服务的配置入口。

**扩展方法**:

| 方法 | 说明 |
|------|------|
| `AddCacheSetup(this IServiceCollection, IConfiguration)` | 注册内存缓存服务和缓存抽象层 |

**注册的服务**:

| 服务接口 | 实现类 | 生命周期 | 说明 |
|---------|--------|---------|------|
| `ICacheService` | `MemoryCacheService` | Singleton | 缓存服务统一接口 |
| `IMemoryCacheService` | `MemoryCacheService` | Singleton | 内存缓存专用接口 |

**内部调用**: 自动调用 `services.AddMemoryCache()` 注册 ASP.NET Core 内置的 `IMemoryCache`。

**使用示例**:
```csharp
// 通过 AddInfrastructure 自动注册
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// 或单独注册
builder.Services.AddCacheSetup(builder.Configuration);
```

#### 3.26.5 DatabaseSetup

**命名空间**: `KH.WMS.Core.Setup`

**说明**: 数据库 ORM 的配置入口，委托给 `KH.WMS.Core.Database.SqlSugar.SqlSugarSetup` 完成实际注册。

**扩展方法**:

| 方法 | 说明 |
|------|------|
| `AddSqlSugarSetup(this IServiceCollection, IConfiguration)` | 注册 SqlSugar 数据库访问层 |

**注册的服务**（由 `SqlSugarSetup.AddSqlSugar` 内部注册）:

| 服务接口 | 实现类 | 生命周期 | 说明 |
|---------|--------|---------|------|
| `ISqlSugarClient` | `SqlSugarClient` | Scoped | SqlSugar ORM 客户端 |
| `IUnitOfWork` | `UnitOfWork` | Scoped | 工作单元 |
| `IRepository<T>` | `Repository<T>` | Scoped | 通用仓储（每个实体类型） |

**配置项**: 从 `appsettings.json` 的 `DbConnection` 节点读取数据库配置（连接字符串、数据库类型、表前缀等）。

**使用示例**:
```csharp
builder.Services.AddSqlSugarSetup(builder.Configuration);
```

#### 3.26.6 AuthenticationSetup

**命名空间**: `KH.WMS.Core.Setup`

**说明**: JWT 认证的配置入口。绑定 `JwtTokenOptions` 配置项，并注册 JWT Bearer 认证方案。

**扩展方法**:

| 方法 | 说明 |
|------|------|
| `AddAuthenticationSetup(this IServiceCollection, IConfiguration)` | 注册 JWT Bearer 认证服务 |
| `AddAuthorizationPolicies(this IServiceCollection)` | 注册授权策略（预留扩展点） |

**注册的服务**:

| 服务/组件 | 说明 |
|----------|------|
| `JwtTokenOptions` | 从 `appsettings.json` 的 `Jwt` 节点绑定 JWT 配置（Secret、Issuer、Audience 等） |
| `JwtBearer` 认证方案 | 配置 Token 验证参数：ValidateIssuer、ValidateAudience、ValidateLifetime、IssuerSigningKey |
| `JwtBearerEvents` | 自定义认证事件：失败日志记录、401 统一 JSON 响应 |

**配置项**（`appsettings.json` 的 `Jwt` 节点）:

| 配置项 | 类型 | 说明 |
|--------|------|------|
| `Jwt:Secret` | `string` | 签名密钥（必填，建议 32 位以上） |
| `Jwt:Issuer` | `string` | 签发者 |
| `Jwt:Audience` | `string` | 接收者 |
| `Jwt:ExpireMinutes` | `int` | Token 过期时间（分钟） |
| `Jwt:ValidateIssuer` | `bool` | 是否验证签发者 |
| `Jwt:ValidateAudience` | `bool` | 是否验证接收者 |
| `Jwt:ValidateLifetime` | `bool` | 是否验证有效期 |
| `Jwt:ClockSkewSeconds` | `int` | 时钟偏差容错（秒） |

**使用示例**:
```csharp
builder.Services.AddAuthenticationSetup(builder.Configuration);
```

#### 3.26.7 LoggingSetup

**命名空间**: `KH.WMS.Core.Setup`

**说明**: 结构化日志的配置入口。

**扩展方法**:

| 方法 | 说明 |
|------|------|
| `AddSerilog(this IHostBuilder)` | 为 `IHostBuilder` 配置 Serilog（委托给 `SerilogSetup.AddSerilog`） |
| `AddLoggingSetup(this IServiceCollection, IConfiguration)` | 注册 `ILoggerService` 日志服务 |

**注册的服务**:

| 服务接口 | 实现类 | 生命周期 | 说明 |
|---------|--------|---------|------|
| `ILoggerService` | `LoggerService` | Singleton | 统一日志服务接口 |

**使用示例**:
```csharp
// Host 配置
builder.Host.AddSerilog();

// 或通过基础设施入口
builder.Services.AddLoggingSetup(builder.Configuration);
```

#### 3.26.8 MonitoringSetup / MiniProfilerSetup / MiniProfilerStorage

**命名空间**: `KH.WMS.Core.Setup` / `KH.WMS.Core.Monitoring.MiniProfiler`

**说明**: 性能监控（MiniProfiler）的配置入口，包含监控配置扩展、MiniProfiler 核心配置和内存存储实现。

---

##### MonitoringSetup（统一扩展入口）

**命名空间**: `KH.WMS.Core.Setup`

| 方法 | 说明 |
|------|------|
| `AddMonitoringSetup(this IServiceCollection, IConfiguration, IWebHostEnvironment)` | 注册 MiniProfiler 服务，委托给 `MiniProfilerSetup.AddMiniProfilerCustom` |
| `UseMiniProfiler(this IApplicationBuilder)` | 使用 MiniProfiler 中间件，委托给 `MiniProfilerSetup.UseMiniProfilerCustom` |

---

##### MiniProfilerSetup（核心配置）

**命名空间**: `KH.WMS.Core.Monitoring.MiniProfiler`

**说明**: MiniProfiler 的核心配置类，配置路由、权限、存储和分析开关。

| 方法 | 说明 |
|------|------|
| `AddMiniProfilerCustom(IServiceCollection, IConfiguration, IWebHostEnvironment)` | 注册 MiniProfiler 服务，配置存储和分析选项 |
| `UseMiniProfilerCustom(IApplicationBuilder)` | 使用 MiniProfiler 中间件及脚本注入中间件 |

**注册的服务**:

| 服务接口 | 实现类 | 生命周期 | 说明 |
|---------|--------|---------|------|
| `IAsyncStorage` | `MiniProfilerMemoryStorage` | Singleton | MiniProfiler 内存存储 |
| `MiniProfiler` (由 `AddMiniProfiler()` 注册) | — | — | MiniProfiler 核心服务 |

**配置项**（`appsettings.json` 的 `MiniProfiler` 节点）:

| 配置项 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `RouteBasePath` | `string` | `"/profiler"` | MiniProfiler 路由基础路径 |
| `EnableInProduction` | `bool` | `false` | 是否在生产环境启用 |
| `TrackConnectionOpenClose` | `bool` | `true` | 是否跟踪数据库连接 |
| `StackTraceLength` | `int` | `5` | 堆栈跟踪长度 |

---

##### MiniProfilerMemoryStorage（内存存储）

**命名空间**: `KH.WMS.Core.Monitoring.MiniProfiler`

**说明**: MiniProfiler 的自定义内存存储实现（`IAsyncStorage`），使用 `ConcurrentDictionary` 在内存中存储 profiling 数据，支持自动过期清理（默认保留 5 分钟）。

**核心特性**:

- 内存中存储，无需外部数据库
- 支持已查看/未查看标记
- 自动过期清理（每 100 次保存或每分钟清理一次）
- 线程安全（`ConcurrentDictionary` + 锁）

#### 3.26.9 ApiDocumentationSetup / SwaggerSetup / SwaggerDefaultValues / SwaggerHeaderFilter

**命名空间**: `KH.WMS.Core.Setup` / `KH.WMS.Core.Api.Documentation.Swagger`

**说明**: API 文档（Swagger/OpenAPI）的配置入口，包含统一扩展入口、Swagger 核心配置、操作过滤器和文档过滤器。

---

##### ApiDocumentationSetup（统一扩展入口）

**命名空间**: `KH.WMS.Core.Setup`

| 方法 | 说明 |
|------|------|
| `AddApiDocumentationSetup(this IServiceCollection, IConfiguration)` | 注册 Swagger 服务，委托给 `SwaggerSetup.AddSwaggerDocumentation` |
| `UseSwaggerDocumentation(this IApplicationBuilder)` | 使用 Swagger 中间件（Swagger UI + JSON 端点） |

---

##### SwaggerSetup（核心配置）

**命名空间**: `KH.WMS.Core.Api.Documentation.Swagger`

**说明**: Swagger 的核心配置类，配置 API 文档信息和 JWT 认证支持。

| 方法 | 说明 |
|------|------|
| `AddSwaggerDocumentation(IServiceCollection, IConfiguration)` | 注册 Swagger 服务：添加 Swagger 文档、JWT 认证定义、XML 注释加载 |
| `UseSwaggerDocumentation(IApplicationBuilder, IConfiguration)` | 启用 Swagger 中间件和 SwaggerUI |

**配置项**（`appsettings.json` 的 `Swagger` 节点）:

| 配置项 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| `Title` | `string` | `"API Documentation"` | API 文档标题 |
| `Version` | `string` | `"v1"` | API 版本号 |
| `Description` | `string` | `"API Documentation"` | API 描述 |
| `ContactName` | `string` | `""` | 联系人名称 |
| `ContactEmail` | `string` | `""` | 联系人邮箱 |
| `ContactUrl` | `string` | `""` | 联系人 URL |
| `LicenseName` | `string` | `""` | 许可证名称 |
| `LicenseUrl` | `string` | `""` | 许可证 URL |
| `RoutePrefix` | `string` | `"swagger"` | SwaggerUI 路由前缀 |
| `EnableJwt` | `bool` | `true` | 是否启用 JWT 认证输入框 |

---

##### SwaggerDefaultValues（操作过滤器）

**命名空间**: `KH.WMS.Core.Api.Documentation.Swagger`

**说明**: `IOperationFilter` 实现，为每个 API 操作自动设置 `OperationId`（基于路由名称或路径）并添加路径信息到描述中。

| 方法 | 说明 |
|------|------|
| `Apply(OpenApiOperation, OperationFilterContext)` | 设置操作 ID 和路径描述 |

---

##### SwaggerHeaderFilter（文档过滤器）

**命名空间**: `KH.WMS.Core.Api.Documentation.Swagger`

**说明**: `IDocumentFilter` 实现，为所有 API 操作自动添加通用请求头参数。

**添加的请求头**:

| 请求头 | 说明 |
|--------|------|
| `X-Correlation-ID` | 关联 ID，用于请求追踪 |
| `X-Request-ID` | 请求 ID，用于请求追踪 |
| `X-Client-Version` | 客户端版本号 |
| `X-Device-ID` | 设备 ID |

---

## 四、附录

### 4.1 错误码速查表

`KH.WMS.Core` 中定义了两套错误码体系，分别用于不同场景：

| 错误码类 | 命名空间 | 用途 | 使用场景 |
|---------|---------|------|---------|
| `ResponseCode` | `KH.WMS.Core.Api.Responses` | HTTP 风格状态码（整型） | 用于 `ApiResponse.Code`，表示一次 API 调用的结果 |
| `ErrorCodes` | `KH.WMS.Core.Exceptions` | 业务风格错误码（整型，按模块分段） | 用于 `BusinessException.ErrorCode`，标识具体的业务错误类型 |

---

#### ResponseCode（API 响应码）

**说明**: HTTP 协议风格的响应状态码，用于 `ApiResponse.Code` 属性。覆盖 2xx（成功）、4xx（客户端错误）和 5xx（服务器错误）。

| 常量名 | 值 | 默认消息 | 说明 |
|--------|-----|---------|------|
| `SUCCESS` | `200` | 操作成功 | 请求处理成功 |
| `CREATED` | `201` | 创建成功 | 资源创建成功 |
| `ACCEPTED` | `202` | 请求已接受 | 请求已接受，异步处理 |
| `NO_CONTENT` | `204` | 无内容返回 | 请求成功，无返回内容 |
| `BAD_REQUEST` | `400` | 请求参数错误 | 请求参数格式错误 |
| `UNAUTHORIZED` | `401` | 未授权访问 | 未登录或 Token 无效 |
| `FORBIDDEN` | `403` | 无权限访问 | 已认证但无权限 |
| `NOT_FOUND` | `404` | 资源未找到 | 请求的资源不存在 |
| `METHOD_NOT_ALLOWED` | `405` | 请求方法不允许 | HTTP 方法不允许 |
| `REQUEST_TIMEOUT` | `408` | 请求超时 | 请求超时 |
| `CONFLICT` | `409` | 数据冲突 | 数据冲突（如乐观锁） |
| `VALIDATION_ERROR` | `422` | 数据验证失败 | 数据验证不通过 |
| `RATE_LIMIT_EXCEEDED` | `429` | 请求过于频繁 | 超出限流阈值 |
| `INTERNAL_SERVER_ERROR` | `500` | 服务器内部错误 | 服务器未处理异常 |
| `NOT_IMPLEMENTED` | `501` | 功能未实现 | 功能尚未实现 |
| `BAD_GATEWAY` | `502` | 网关错误 | 上游服务返回错误 |
| `SERVICE_UNAVAILABLE` | `503` | 服务不可用 | 服务暂不可用 |
| `GATEWAY_TIMEOUT` | `504` | 网关超时 | 上游服务超时 |

---

#### ErrorCodes（业务错误码）

**说明**: 业务层面的细分错误码，用于 `BusinessException` 的 `ErrorCode` 属性。按模块分段编码，便于问题排查。

| 分类 | 常量名 | 值 | 说明 |
|------|--------|-----|------|
| **通用错误** | `SUCCESS` | `0` | 操作成功 |
| | `SYSTEM_ERROR` | `1000` | 系统错误 |
| | `NETWORK_ERROR` | `1001` | 网络错误 |
| | `TIMEOUT_ERROR` | `1002` | 请求超时 |
| | `UNKNOWN_ERROR` | `1999` | 未知错误 |
| **业务错误** | `BUSINESS_ERROR` | `2000` | 业务处理失败 |
| | `OPERATION_FAILED` | `2001` | 操作失败 |
| | `STATE_ERROR` | `2002` | 状态错误 |
| **验证错误** | `VALIDATION_ERROR` | `3000` | 数据验证失败 |
| | `REQUIRED_FIELD_MISSING` | `3001` | 必填字段缺失 |
| | `INVALID_FORMAT` | `3002` | 格式不正确 |
| | `INVALID_VALUE` | `3003` | 值不正确 |
| | `OUT_OF_RANGE` | `3004` | 值超出范围 |
| **认证授权错误** | `UNAUTHORIZED` | `4001` | 未授权 |
| | `TOKEN_EXPIRED` | `4002` | 令牌已过期 |
| | `TOKEN_INVALID` | `4003` | 令牌无效 |
| | `FORBIDDEN` | `4004` | 无权访问 |
| | `LOGIN_FAILED` | `4005` | 登录失败 |
| | `PASSWORD_EXPIRED` | `4006` | 密码已过期 |
| **资源错误** | `NOT_FOUND` | `5001` | 资源未找到 |
| | `ALREADY_EXISTS` | `5002` | 资源已存在 |
| | `RESOURCE_LOCKED` | `5003` | 资源已锁定 |
| | `VERSION_CONFLICT` | `5004` | 版本冲突 |
| **并发错误** | `CONFLICT_ERROR` | `6001` | 数据冲突 |
| | `DUPLICATE_ERROR` | `6002` | 数据重复 |
| **外部服务错误** | `EXTERNAL_SERVICE_ERROR` | `7001` | 外部服务错误 |
| | `EXTERNAL_SERVICE_UNAVAILABLE` | `7002` | 外部服务不可用 |

---

#### 使用场景说明

| 场景 | 使用哪个错误码 | 代码示例 |
|------|--------------|---------|
| API 控制器返回统一响应 | `ResponseCode` | `return ApiResponse.Fail(ResponseCode.NOT_FOUND, "订单不存在");` |
| 业务层抛出业务异常 | `ErrorCodes` | `throw new BusinessException(ErrorCodes.NOT_FOUND, "订单不存在");` |
| 全局异常过滤器捕获异常 | 自动转换 | `BusinessException` 的 `ErrorCode` 自动映射到 `ApiResponse.Code` |

### 4.2 常用常量速查

#### CacheConstants（缓存键常量）

**命名空间**: `KH.WMS.Core.Constants`

**说明**: 所有缓存键以 `"App:"` 为统一前缀，按用途分组。

| 分组 | 常量 | 值/方法 | 说明 |
|------|------|---------|------|
| **通用** | `KEY_PREFIX` | `"App:"` | 缓存键统一前缀 |
| **用户** | `User.INFO` | `"App:User:Info:"` | 用户信息缓存前缀 |
| | `User.PERMISSIONS` | `"App:User:Permissions:"` | 用户权限缓存前缀 |
| | `User.ROLES` | `"App:User:Roles:"` | 用户角色缓存前缀 |
| | `User.MENUS` | `"App:User:Menus:"` | 用户菜单缓存前缀 |
| | `User.GetUserInfoKey(userId)` | — | 获取指定用户的缓存键 |
| **配置** | `Config.SYSTEM` | `"App:Config:System"` | 系统配置缓存键 |
| | `Config.DICT` | `"App:Config:Dict:"` | 字典缓存前缀 |
| | `Config.GetDictKey(dictType)` | — | 获取指定字典类型的缓存键 |
| **数据** | `Data.ENTITY` | `"App:Data:Entity:"` | 实体缓存前缀 |
| | `Data.LIST` | `"App:Data:List:"` | 列表缓存前缀 |
| | `Data.GetEntityKey<T>(id)` | — | 获取指定实体的缓存键 |
| **令牌** | `Token.ACCESS` | `"App:Token:Access:"` | 访问令牌缓存前缀 |
| | `Token.REFRESH` | `"App:Token:Refresh:"` | 刷新令牌缓存前缀 |
| | `Token.BLACKLIST` | `"App:Token:Blacklist:"` | 令牌黑名单缓存前缀 |
| **限流** | `RateLimit.IP` | `"App:RateLimit:IP:"` | IP 限流缓存前缀 |
| | `RateLimit.USER` | `"App:RateLimit:User:"` | 用户限流缓存前缀 |
| | `RateLimit.API` | `"App:RateLimit:API:"` | API 限流缓存前缀 |
| **验证码** | `Captcha.SMS` | `"App:Captcha:SMS:"` | 短信验证码缓存前缀 |
| | `Captcha.EMAIL` | `"App:Captcha:Email:"` | 邮箱验证码缓存前缀 |
| **系统参数** | `SysParam.Defaults` | 字典兜底值 | 系统参数默认值，包括默认密码、Token 过期时间等 |
| **文件** | `File.INFO` | `"App:File:Info:"` | 文件信息缓存前缀 |

---

#### HeaderConstants（HTTP 请求头常量）

**命名空间**: `KH.WMS.Core.Constants`

| 分组 | 常量名 | 值 | 说明 |
|------|--------|-----|------|
| **认证** | `Authentication.AUTHORIZATION` | `"Authorization"` | 认证头 |
| | `Authentication.BEARER_PREFIX` | `"Bearer "` | Bearer Token 前缀 |
| **追踪** | `Tracing.X_REQUEST_ID` | `"X-Request-ID"` | 请求 ID |
| | `Tracing.X_CORRELATION_ID` | `"X-Correlation-ID"` | 关联 ID |
| | `Tracing.X_TRACE_ID` | `"X-Trace-ID"` | 链路追踪 ID |
| **客户端** | `Client.USER_AGENT` | `"User-Agent"` | 用户代理 |
| | `Client.X_CLIENT_VERSION` | `"X-Client-Version"` | 客户端版本 |
| | `Client.X_CLIENT_ID` | `"X-Client-Id"` | 客户端 ID |
| | `Client.X_DEVICE_ID` | `"X-Device-ID"` | 设备 ID |
| | `Client.X_DEVICE_TYPE` | `"X-Device-Type"` | 设备类型 |
| | `Client.X_OS` | `"X-OS"` | 操作系统 |
| **请求上下文** | `Context.X_TENANT_ID` | `"X-Tenant-Id"` | 租户 ID |
| | `Context.X_LANGUAGE` | `"X-Language"` | 语言 |
| **分页** | `Pagination.X_PAGE_INDEX` | `"X-Page-Index"` | 当前页码 |
| | `Pagination.X_PAGE_SIZE` | `"X-Page-Size"` | 每页数量 |
| | `Pagination.X_TOTAL_COUNT` | `"X-Total-Count"` | 总记录数 |
| **安全** | `Security.X_CSRF_TOKEN` | `"X-CSRF-Token"` | CSRF 令牌 |
| | `Security.X_FORWARDED_FOR` | `"X-Forwarded-For"` | 原始客户端 IP |
| | `Security.X_REAL_IP` | `"X-Real-IP"` | 真实 IP |
| **API** | `Api.X_API_KEY` | `"X-API-Key"` | API 密钥 |
| | `Api.X_API_VERSION` | `"X-API-Version"` | API 版本 |
| | `Api.X_SIGNATURE` | `"X-Signature"` | 请求签名 |
| | `Api.X_TIMESTAMP` | `"X-Timestamp"` | 请求时间戳 |
| **内容类型** | `ContentTypes.APPLICATION_JSON` | `"application/json"` | JSON 内容类型 |
| | `ContentTypes.APPLICATION_XML` | `"application/xml"` | XML 内容类型 |
| | `ContentTypes.MULTIPART_FORM_DATA` | `"multipart/form-data"` | 表单文件上传 |

---

#### AppSettingsConstants（appsettings 配置节常量）

**命名空间**: `KH.WMS.Core.Constants`

| 常量名 | 值 | 说明 |
|--------|-----|------|
| `DbConnection` | `"DbConnection"` | 数据库连接字符串配置节 |
| `MiniProfiler` | `"MiniProfiler"` | MiniProfiler 配置节 |
| `Swagger` | `"Swagger"` | Swagger 配置节 |
| `DbType_MySql` | `"mysql"` | MySQL 数据库类型标识 |
| `DbType_SqlServer` | `"sqlserver"` | SQL Server 数据库类型标识 |
| `DbType_PostgreSql` | `"postgresql"` | PostgreSQL 数据库类型标识 |
| `DbType_Oracle` | `"oracle"` | Oracle 数据库类型标识 |
| `DbType_Sqlite` | `"sqlite"` | SQLite 数据库类型标识 |

---

> 本文档由 KH.WMS 团队维护。如有问题或建议，请联系项目维护者。

## 继续阅读

- [学习路径](/learning-path)
- [培训资料下载](/training-materials)
- [架构总览](/backend/架构设计/KH.WMS架构总览)
