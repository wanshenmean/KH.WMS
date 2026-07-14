---
title: "三个程序集公开类型索引"
description: "三个程序集公开类型索引：说明适用场景、当前实现、设计边界与开发或排障入口。"
status: reference
audience: "接口调用方、扩展开发人员与模块维护者"
reviewed: "2026-07-14"
sourcePaths:
  - "KH.WMS"
---

# 三个程序集公开类型索引

本索引从文档基线提交成功编译的程序集提取，覆盖所有公开顶层类型。它用于发现类型和核对命名空间；成员签名及使用语义请查看各模块 API 文档或源码。

> 说明：编译器生成的嵌套类型不在本索引中；基类为 `System.Object` 时省略。泛型类型参数保持程序集元数据中的完整名称。

## KH.WMS.Algorithms

程序集版本：`0.1.0.0`；公开顶层类型：84。

### `KH.WMS.Algorithms.Strategy`

- `PolicyContext` — 类
- `PolicyResult` — 类

### `KH.WMS.Algorithms.Strategy.Configuration`

- `CfgStrategyChainConfig` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgStrategyChainStep` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgStrategyConfig` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`

### `KH.WMS.Algorithms.Strategy.Constants`

- `AlgoConstants` — 静态类
- `StrategyParams` — 静态类

### `KH.WMS.Algorithms.Strategy.Controllers`

- `StrategyChainController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Algorithms.Strategy.Configuration.CfgStrategyChainConfig>`
- `StrategyConfigController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Algorithms.Strategy.Configuration.CfgStrategyConfig>`
- `StrategyQueryController` — 类；基类：`Microsoft.AspNetCore.Mvc.ControllerBase`

### `KH.WMS.Algorithms.Strategy.DTOs`

- `CfgDocTypePortDTO` — 类
- `InventoryInfoDTO` — 类
- `InvInventoryDetailDTO` — 类
- `InvInventoryHeaderDTO` — 类
- `LogicalZoneDTO` — 类
- `LogicalZonePhysicalMapping` — 类
- `MdAisleDTO` — 类
- `MdLocationDTO` — 类
- `MdLogicalZoneDTO` — 类
- `MdLogicalZoneMappingDTO` — 类
- `MdMaterialTurnoverDTO` — 类
- `MdPortDTO` — 类
- `MdTransferPointDTO` — 类
- `MdWarehouseZoneDTO` — 类

### `KH.WMS.Algorithms.Strategy.Enums`

- `PolicyType` — 枚举；基类：`System.Enum`

### `KH.WMS.Algorithms.Strategy.Implementations.InventoryAllocation`

- `BatchStrategy` — 类；基类：`KH.WMS.Algorithms.Strategy.Strategies.InventoryAllocationStrategyBase`
- `FefoStrategy` — 类；基类：`KH.WMS.Algorithms.Strategy.Strategies.InventoryAllocationStrategyBase`
- `FifoStrategy` — 类；基类：`KH.WMS.Algorithms.Strategy.Strategies.InventoryAllocationStrategyBase`
- `UtilizationPriorityStrategy` — 类；基类：`KH.WMS.Algorithms.Strategy.Strategies.InventoryAllocationStrategyBase`

### `KH.WMS.Algorithms.Strategy.Implementations.LocationAllocation`

- `AbcClassAllocationStrategy` — 类；基类：`KH.WMS.Algorithms.Strategy.Strategies.LocationAllocationStrategyBase`
- `CategoryZoneAllocationStrategy` — 类；基类：`KH.WMS.Algorithms.Strategy.Strategies.LocationAllocationStrategyBase`
- `CentralizedStorageStrategy` — 类；基类：`KH.WMS.Algorithms.Strategy.Strategies.LocationAllocationStrategyBase`
- `DoubleDeepAllocationStrategy` — 类；基类：`KH.WMS.Algorithms.Strategy.Strategies.LocationAllocationStrategyBase`

### `KH.WMS.Algorithms.Strategy.Implementations.Picking`

- `DefaultPickingStrategy` — 类；基类：`KH.WMS.Algorithms.Strategy.Strategies.PickingStrategyBase`

### `KH.WMS.Algorithms.Strategy.Implementations.Putaway`

- `DefaultPutawayStrategy` — 类；基类：`KH.WMS.Algorithms.Strategy.Strategies.PutawayStrategyBase`

### `KH.WMS.Algorithms.Strategy.Interfaces`

- `IPolicyChain` — 接口
- `IPolicyContext` — 接口
- `IPolicyEngine` — 接口
- `IPolicyFilter` — 接口
- `IPolicyRegistry` — 接口
- `IPolicyResult` — 接口
- `IPolicyStrategy` — 接口

### `KH.WMS.Algorithms.Strategy.Models`

- `PolicyExecutionLog` — 类

### `KH.WMS.Algorithms.Strategy.Optimizers`

- `CranePathOptimizer` — 静态类

### `KH.WMS.Algorithms.Strategy.QueryServices`

- `IInventoryQueryService` — 接口
- `ILocationQueryService` — 接口
- `InventoryQueryService` — 类
- `IWarehouseQueryService` — 接口
- `LocationQueryService` — 类
- `WarehouseQueryService` — 类

### `KH.WMS.Algorithms.Strategy.Services`

- `IStrategyChainService` — 接口
- `IStrategyConfigService` — 接口
- `IStrategyQueryService` — 接口
- `ParamFieldDef` — 类
- `PolicyChain` — 类
- `PolicyEngine` — 类
- `PolicyFilterBase` — 抽象类
- `PolicyRegistry` — 类
- `StrategyAutofacModule` — 类；基类：`Autofac.Module`
- `StrategyChainCreateRequest` — 类
- `StrategyChainDetail` — 类
- `StrategyChainRuntimeInfo` — 类
- `StrategyChainService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Algorithms.Strategy.Configuration.CfgStrategyChainConfig>`
- `StrategyChainUpdateRequest` — 类
- `StrategyConfigService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Algorithms.Strategy.Configuration.CfgStrategyConfig>`
- `StrategyOptionItem` — 类
- `StrategyOptionsResult` — 类
- `StrategyQueryService` — 类
- `StrategyRuntimeInfo` — 类
- `StrategyTypeInfo` — 类

### `KH.WMS.Algorithms.Strategy.Strategies`

- `InventoryAllocationItem` — 类
- `InventoryAllocationResult` — 类
- `InventoryAllocationStrategyBase` — 抽象类；基类：`KH.WMS.Algorithms.Strategy.Strategies.PolicyStrategyBase`
- `LocationAllocationResult` — 类
- `LocationAllocationStrategyBase` — 抽象类；基类：`KH.WMS.Algorithms.Strategy.Strategies.PolicyStrategyBase`
- `LocationRecommendation` — 类
- `PickingResult` — 类
- `PickingStrategyBase` — 抽象类；基类：`KH.WMS.Algorithms.Strategy.Strategies.PolicyStrategyBase`
- `PickingTaskItem` — 类
- `PolicyStrategyBase` — 抽象类
- `PutawayResult` — 类
- `PutawayStrategyBase` — 抽象类；基类：`KH.WMS.Algorithms.Strategy.Strategies.PolicyStrategyBase`
- `SortField` — 类
- `SyncPolicyStrategyBase` — 抽象类；基类：`KH.WMS.Algorithms.Strategy.Strategies.PolicyStrategyBase`

## KH.WMS.Config

程序集版本：`1.0.0.0`；公开顶层类型：88。

### `KH.WMS.Config.Abstractions`

- `CfgCodeRule` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgCodeSequence` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgDocTypePort` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgDocumentField` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgDocumentStatus` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgDocumentType` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgDocumentTypeProcess` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgDocumentTypeRule` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgExtField` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgExtFieldType` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgGlobalConfig` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgJobDefinition` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgJobTrigger` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgLocationStatus` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgLocationType` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgPortType` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgTransferPointType` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgWarehouseType` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `CfgWarehouseZoneType` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `ConfigScopeContext` — 类
- `ConfigScopeLevels` — 静态类
- `ICfgDocumentFieldExtContract` — 接口
- `ICfgExtFieldContract` — 接口
- `IConfigResolverContract` — 接口
- `IConfigScopeResolver` — 接口
- `IDocumentStatusValidatorContract` — 接口
- `LogCodeRecord` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`
- `LogJobExecution` — 类；基类：`KH.WMS.Core.Models.Entities.BaseEntity<System.Int64>`

### `KH.WMS.Config.Contracts`

- `CfgDocumentFieldExtContract` — 类
- `CfgExtFieldContract` — 类
- `ConfigResolverContract` — 类
- `DefaultConfigScopeResolver` — 类
- `DocumentStatusValidatorContract` — 类

### `KH.WMS.Config.Controllers`

- `CfgCodeRuleController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgCodeRule>`
- `CfgCodeSequenceController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgCodeSequence>`
- `CfgDocTypePortController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgDocTypePort>`
- `CfgDocumentFieldController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgDocumentField>`
- `CfgDocumentStatusController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgDocumentStatus>`
- `CfgDocumentTypeController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgDocumentType>`
- `CfgDocumentTypeProcessController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgDocumentTypeProcess>`
- `CfgDocumentTypeRuleController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgDocumentTypeRule>`
- `CfgExtFieldController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgExtField>`
- `CfgExtFieldTypeController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgExtFieldType>`
- `CfgGlobalConfigController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgGlobalConfig>`
- `CfgLocationStatusController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgLocationStatus>`
- `CfgLocationTypeController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgLocationType>`
- `CfgPortTypeController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgPortType>`
- `CfgTransferPointTypeController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgTransferPointType>`
- `CfgWarehouseTypeController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgWarehouseType>`
- `CfgWarehouseZoneTypeController` — 类；基类：`KH.WMS.Core.Controllers.CrudController<KH.WMS.Config.Abstractions.CfgWarehouseZoneType>`

### `KH.WMS.Config.DTOs`

- `BatchUpdateConfigRequest` — 类
- `ConfigGroupDto` — 类
- `ConfigItemDto` — 类
- `ConfigUpdateItem` — 类

### `KH.WMS.Config.Interfaces`

- `ICfgCodeRuleService` — 接口
- `ICfgCodeSequenceService` — 接口
- `ICfgDocTypePortService` — 接口
- `ICfgDocumentFieldService` — 接口
- `ICfgDocumentStatusService` — 接口
- `ICfgDocumentTypeProcessService` — 接口
- `ICfgDocumentTypeRuleService` — 接口
- `ICfgDocumentTypeService` — 接口
- `ICfgExtFieldConfigService` — 接口
- `ICfgExtFieldTypeConfigService` — 接口
- `ICfgGlobalConfigService` — 接口
- `ICfgLocationStatusService` — 接口
- `ICfgLocationTypeService` — 接口
- `ICfgPortTypeService` — 接口
- `ICfgTransferPointTypeService` — 接口
- `ICfgWarehouseTypeService` — 接口
- `ICfgWarehouseZoneTypeService` — 接口

### `KH.WMS.Config.Services`

- `CfgCodeRuleService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgCodeRule>`
- `CfgCodeSequenceService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgCodeSequence>`
- `CfgDocTypePortService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgDocTypePort>`
- `CfgDocumentFieldService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgDocumentField>`
- `CfgDocumentStatusService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgDocumentStatus>`
- `CfgDocumentTypeProcessService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgDocumentTypeProcess>`
- `CfgDocumentTypeRuleService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgDocumentTypeRule>`
- `CfgDocumentTypeService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgDocumentType>`
- `CfgExtFieldConfigService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgExtField>`
- `CfgExtFieldTypeConfigService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgExtFieldType>`
- `CfgGlobalConfigService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgGlobalConfig>`
- `CfgLocationStatusService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgLocationStatus>`
- `CfgLocationTypeService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgLocationType>`
- `CfgPortTypeService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgPortType>`
- `CfgTransferPointTypeService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgTransferPointType>`
- `CfgWarehouseTypeService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgWarehouseType>`
- `CfgWarehouseZoneTypeService` — 类；基类：`KH.WMS.Core.Services.CrudService<KH.WMS.Config.Abstractions.CfgWarehouseZoneType>`

## KH.WMS.Core

程序集版本：`0.1.0.0`；公开顶层类型：236。

### `KH.WMS.Core.AOP`

- `InterceptorBase` — 抽象类

### `KH.WMS.Core.AOP.CallTrace`

- `CallTraceContext` — 静态类
- `CallTraceState` — 类

### `KH.WMS.Core.AOP.Interceptors`

- `CachingInterceptor` — 类
- `ConfigValidationInterceptor` — 类
- `ExceptionInterceptor` — 类
- `LoggingInterceptor` — 类；基类：`KH.WMS.Core.AOP.InterceptorBase`
- `PerformanceInterceptor` — 类

### `KH.WMS.Core.Api.Documentation.Swagger`

- `SwaggerDefaultValues` — 类
- `SwaggerHeaderFilter` — 类
- `SwaggerOptions` — 类
- `SwaggerSetup` — 静态类

### `KH.WMS.Core.Api.Responses`

- `ApiResponse` — 类
- `ApiResponse<T>` — 类；基类：`KH.WMS.Core.Api.Responses.ApiResponse`
- `ApiResponseExtensions` — 静态类
- `PagedResponse<T>` — 类；基类：`KH.WMS.Core.Api.Responses.ApiResponse<KH.WMS.Core.Api.Responses.PagedResult<T>>`
- `PagedResult<T>` — 类
- `Pagination` — 类
- `ResponseCode` — 静态类

### `KH.WMS.Core.Attributes`

- `CacheAttribute` — 类；基类：`System.Attribute`
- `LogInterceptorAttribute` — 类；基类：`System.Attribute`
- `RateLimitAttribute` — 类；基类：`System.Attribute`
- `RateLimitStrategy` — 枚举；基类：`System.Enum`
- `StatusFieldNameAttribute` — 类；基类：`System.Attribute`
- `TransactionAttribute` — 类；基类：`System.Attribute`

### `KH.WMS.Core.Authentication`

- `IJwtTokenService` — 接口

### `KH.WMS.Core.Authentication.JWT`

- `JwtBearerExtensions` — 静态类
- `JwtTokenOptions` — 类
- `JwtTokenService` — 类
- `TokenValidationParametersExtensions` — 静态类

### `KH.WMS.Core.Caching`

- `ICacheService` — 接口

### `KH.WMS.Core.Caching.Memory`

- `CacheEntryOptions` — 类
- `IMemoryCacheService` — 接口
- `MemoryCacheService` — 类

### `KH.WMS.Core.Configuration`

- `ConfigurationProvider` — 类
- `IConfigurationProvider` — 接口

### `KH.WMS.Core.Constants`

- `AppSettingsConstants` — 类
- `BoolFlag` — 静态类
- `CacheConstants` — 静态类
- `ErrorConstants` — 静态类
- `HeaderConstants` — 静态类
- `RoleConstants` — 静态类
- `SysParamConstants` — 静态类

### `KH.WMS.Core.Controllers`

- `CrudController<TEntity>` — 抽象类；基类：`Microsoft.AspNetCore.Mvc.ControllerBase`
- `ExtDataCrudController<TEntity>` — 抽象类；基类：`KH.WMS.Core.Controllers.CrudController<TEntity>`

### `KH.WMS.Core.Database`

- `DatabaseInitService` — 类
- `IDatabaseInitService` — 接口
- `IDbContext` — 接口

### `KH.WMS.Core.Database.Repositories`

- `IRepository<T, TKey>` — 接口
- `RepositoryBase<T, TKey>` — 类

### `KH.WMS.Core.Database.SqlSugar`

- `ConfigDbAttribute` — 类；基类：`System.Attribute`
- `DatabaseOptions` — 类
- `SqlSugarDbContext` — 类
- `SqlSugarSetup` — 静态类

### `KH.WMS.Core.Database.UnitOfWorks`

- `ITransactionScope` — 接口
- `IUnitOfWork` — 接口
- `TransactionScopeWrapper` — 类
- `UnitOfWork` — 类
- `UnitOfWorkExtensions` — 静态类

### `KH.WMS.Core.DependencyInjection`

- `AssemblyService` — 类
- `IInterceptorQueryService` — 接口
- `InterceptorQueryService` — 类
- `InterceptorServiceInfo` — 类
- `InterceptorStats` — 类
- `IServiceRegistrar` — 接口
- `ServiceExtensions` — 类；基类：`Autofac.Module`
- `ServiceRegistrar` — 类

### `KH.WMS.Core.DependencyInjection.ServiceLifetimes`

- `RegisteredServiceAttribute` — 类；基类：`System.Attribute`
- `SelfRegisteredServiceAttribute` — 类；基类：`System.Attribute`

### `KH.WMS.Core.Exceptions`

- `BusinessException` — 类；基类：`System.Exception`
- `ErrorCodes` — 静态类
- `NotFoundException` — 类；基类：`System.Exception`
- `ValidationError` — 类
- `ValidationException` — 类；基类：`System.Exception`

### `KH.WMS.Core.Factories`

- `BusinessProcessorBase` — 抽象类
- `BusinessProcessorFactory` — 类
- `IBusinessProcessor` — 接口

### `KH.WMS.Core.Filters.Action`

- `CustomActionFilter` — 类
- `LogActionAttribute` — 类；基类：`System.Attribute`
- `TransactionActionFilter` — 类

### `KH.WMS.Core.Filters.Authorization`

- `ApiAuthorizeFilter` — 类
- `CustomAuthorizationFilter` — 类
- `CustomAuthorizeAttribute` — 类；基类：`System.Attribute`

### `KH.WMS.Core.Filters.Exception`

- `CustomExceptionFilter` — 类
- `GlobalExceptionFilter` — 类
- `HandleExceptionAttribute` — 类；基类：`System.Attribute`

### `KH.WMS.Core.Filters.Resource`

- `CacheResourceAttribute` — 类；基类：`System.Attribute`
- `CustomResourceFilter` — 类

### `KH.WMS.Core.Filters.Result`

- `TraceIdResultFilter` — 类

### `KH.WMS.Core.Helpers`

- `ExpressionHelper` — 静态类
- `UtilConvert` — 静态类

### `KH.WMS.Core.ImportExport`

- `ExcelHelper` — 静态类

### `KH.WMS.Core.License.Controllers`

- `LicenseController` — 类；基类：`Microsoft.AspNetCore.Mvc.ControllerBase`

### `KH.WMS.Core.License.Crypto`

- `MachineCodeGenerator` — 静态类
- `RsaKeyHelper` — 静态类
- `RsaLicenseSigner` — 类
- `RsaLicenseVerifier` — 类

### `KH.WMS.Core.License.DTOs`

- `GenerateLicenseRequest` — 类
- `ImportLicenseRequest` — 类
- `LicenseInfoDto` — 类

### `KH.WMS.Core.License.Interfaces`

- `ILicenseAppService` — 接口
- `ILicenseService` — 接口

### `KH.WMS.Core.License.Middleware`

- `LicenseMiddlewareExtensions` — 静态类
- `LicenseValidationMiddleware` — 类

### `KH.WMS.Core.License.Models`

- `LicenseData` — 类
- `LicenseFile` — 类

### `KH.WMS.Core.License.Results`

- `Result` — 类
- `Result<T>` — 类；基类：`KH.WMS.Core.License.Results.Result`

### `KH.WMS.Core.License.Services`

- `LicenseAppService` — 类
- `LicenseService` — 类

### `KH.WMS.Core.Logging`

- `ErrorLogScope` — 静态类
- `ILoggerService` — 接口
- `LogContext` — 类
- `LogModuleDetector` — 静态类
- `WmsLogModules` — 静态类

### `KH.WMS.Core.Logging.LogClean`

- `ILogCleanupService` — 接口
- `LogCleanupOptions` — 类
- `LogCleanupService` — 类
- `LogStatistics` — 类

### `KH.WMS.Core.Logging.LogEnums`

- `LogLevelType` — 枚举；基类：`System.Enum`
- `LogModule` — 枚举；基类：`System.Enum`
- `LogType` — 枚举；基类：`System.Enum`

### `KH.WMS.Core.Logging.Serilog`

- `LoggerService` — 类
- `NullSink` — 类
- `SerilogFileConfiguration` — 静态类
- `SerilogOptions` — 类
- `SerilogSetup` — 静态类

### `KH.WMS.Core.Logging.Serilog.Enricher`

- `CorrelationIdEnricher` — 类
- `LogEnricher` — 类
- `LogTypeEnricher` — 类
- `ModuleLogEnricher` — 类
- `PerformanceLogEnricher` — 类
- `UserLogEnricher` — 类

### `KH.WMS.Core.Logging.WMSError`

- `WMSErrorCodes` — 静态类
- `WMSErrorMessages` — 静态类

### `KH.WMS.Core.Mapping`

- `IMappingService` — 接口
- `MappingService` — 类

### `KH.WMS.Core.Mapping.AutoMapper`

- `AutoMapperSetup` — 静态类

### `KH.WMS.Core.Middlewares`

- `CorsMiddlewareExtensions` — 静态类
- `CorsOptions` — 类
- `ExceptionHandlingMiddleware` — 类
- `ExceptionHandlingMiddlewareExtensions` — 静态类
- `RateLimitCounter` — 类
- `RateLimitMiddleware` — 类
- `RateLimitMiddlewareExtensions` — 静态类
- `RateLimitOptions` — 类
- `RequestLoggingMiddleware` — 类
- `RequestLoggingMiddlewareExtensions` — 静态类
- `StaticFileMiddlewareExtensions` — 静态类

### `KH.WMS.Core.Models`

- `ServiceResult` — 类
- `ServiceResult<T>` — 类；基类：`KH.WMS.Core.Models.ServiceResult`

### `KH.WMS.Core.Models.Dtos`

- `AdvancedQueryRequestDto` — 类；基类：`KH.WMS.Core.Api.Responses.Pagination`
- `BaseDto` — 抽象类
- `BatchDeleteRequestDto` — 类
- `CreateRequestDto` — 抽象类
- `DeleteRequestDto` — 类
- `ExportColumnDto` — 类
- `ExportRequestDto` — 类；基类：`KH.WMS.Core.Models.Dtos.AdvancedQueryRequestDto`
- `FilterCondition` — 类
- `QueryRequestDto` — 抽象类；基类：`KH.WMS.Core.Api.Responses.Pagination`
- `SetStatusDto` — 类
- `SortCondition` — 类
- `UpdateRequestDto` — 抽象类

### `KH.WMS.Core.Models.Entities`

- `BaseEntity<T>` — 抽象类；基类：`KH.WMS.Core.Models.Entities.RootEntity`
- `IEnableDisableEntity` — 接口
- `RootEntity` — 类
- `StatusFieldNames` — 静态类

### `KH.WMS.Core.Modularity`

- `IModule` — 接口
- `IModuleContext` — 接口
- `ModuleBase` — 抽象类
- `ModuleContext` — 类
- `ModuleDependency` — 类
- `ModuleExtensions` — 静态类
- `ModuleLoader` — 类

### `KH.WMS.Core.Monitoring.MiniProfiler`

- `MiniProfilerInjectorMiddleware` — 类
- `MiniProfilerMemoryStorage` — 类
- `MiniProfilerSettings` — 类
- `MiniProfilerSetup` — 静态类

### `KH.WMS.Core.Security.Encryption`

- `AesEncryptionService` — 类
- `IEncryptionService` — 接口
- `IRsaCryptoService` — 接口
- `RsaCryptoService` — 类
- `RsaEncryptionService` — 类

### `KH.WMS.Core.Security.Hashing`

- `IHashService` — 接口
- `Md5Hasher` — 类
- `PasswordHasher` — 类
- `Sha256Hasher` — 类

### `KH.WMS.Core.Security.RateLimiting`

- `FixedWindowCounter` — 类
- `FixedWindowRateLimiter` — 类
- `IRateLimitService` — 接口
- `RateLimitOptions` — 类
- `RateLimitStrategy` — 枚举；基类：`System.Enum`
- `SlidingWindowCounter` — 类
- `SlidingWindowRateLimiter` — 类
- `TokenBucket` — 类
- `TokenBucketRateLimiter` — 类

### `KH.WMS.Core.Serialization`

- `IJsonSerializer` — 接口

### `KH.WMS.Core.Serialization.Json`

- `JsonSetup` — 静态类
- `NewtonsoftJsonSerializer` — 类
- `NewtonsoftSettings` — 静态类
- `SystemJsonSerializer` — 类

### `KH.WMS.Core.Serialization.JsonConverters`

- `BoolToIntConverter` — 类；基类：`System.Text.Json.Serialization.JsonConverter<System.Int32>`
- `DateTimeConverter` — 类；基类：`System.Text.Json.Serialization.JsonConverter<System.DateTime>`
- `DateTimeOffsetConverter` — 类；基类：`System.Text.Json.Serialization.JsonConverter<System.DateTimeOffset>`
- `EnumConverter` — 类；基类：`System.Text.Json.Serialization.JsonConverterFactory`
- `EnumConverter<T>` — 类；基类：`System.Text.Json.Serialization.JsonConverter<T>`
- `LongConverter` — 类；基类：`System.Text.Json.Serialization.JsonConverter<System.Int64>`
- `NullableBoolToIntConverter` — 类；基类：`System.Text.Json.Serialization.JsonConverter<System.Nullable<System.Int32>>`
- `NullableDateTimeConverter` — 类；基类：`System.Text.Json.Serialization.JsonConverter<System.Nullable<System.DateTime>>`
- `NullableEnumConverter<T>` — 类；基类：`System.Text.Json.Serialization.JsonConverter<System.Nullable<T>>`
- `NullableLongConverter` — 类；基类：`System.Text.Json.Serialization.JsonConverter<System.Nullable<System.Int64>>`

### `KH.WMS.Core.Services`

- `ApplicationService` — 类
- `CodeGeneratorService` — 类
- `CrudService<TEntity>` — 类；基类：`KH.WMS.Core.Services.ApplicationService`
- `DetailSaveService` — 类
- `DomainService` — 抽象类
- `IApplicationService` — 接口
- `ICodeGeneratorService` — 接口
- `ICrudService<TEntity>` — 接口
- `IDetailSaveService` — 接口
- `IDomainService` — 接口
- `ValidationResult` — 类

### `KH.WMS.Core.Setup`

- `ApiDocumentationSetup` — 静态类
- `AuthenticationSetup` — 静态类
- `CacheSetup` — 静态类
- `DatabaseSetup` — 静态类
- `HostSetup` — 静态类
- `LoggingSetup` — 静态类
- `MiddlewareSetup` — 静态类
- `MonitoringSetup` — 静态类
- `ServiceCollectionSetup` — 静态类

### `KH.WMS.Core.UserProvide`

- `IUserContext` — 接口
- `UserContext` — 类

### `KH.WMS.Core.Validation`

- `ConfigValidationAttribute` — 类；基类：`System.Attribute`
- `IValidator` — 接口
- `ValidatorCodes` — 静态类

## 继续阅读

- [API 参考首页](/api/README)
- [跨模块 Contract](/backend/KH.WMS后端Contract与模块协作指引)
