# KH.WMS 后端架构设计思路

KH.WMS 后端采用 ASP.NET Core 模块化单体：一个启动和部署单元内保留统一事务与运维方式，同时用业务模块、分层职责和 Contract 约束代码边界。

本文解释为什么选择这种形态、各项目应该承担什么职责，以及复用底座和编写专用业务流程之间怎样取舍。具体开发步骤请继续阅读[KH.WMS 后端整体地图](/backend/后端开发指引V3教程/01-KH.WMS后端整体地图)。

## 1. 后端架构要解决的问题

| 问题 | 架构目标 |
| --- | --- |
| 一个业务动作跨单据、库存、任务和容器 | 在明确的 Service 事务边界内保持一致性 |
| 业务模块协作频繁 | 通过 Contract 调用能力，不直接引用对方实现 |
| 大量标准维护接口重复 | 用 CRUD Controller、Service 和仓储底座减少样板代码 |
| 仓库规则和策略不同 | 把配置、状态和算法从业务流程中分离 |
| 鉴权、异常、日志和响应必须一致 | 由 Server 与 Core 提供统一横切能力 |
| Controller 和 Service 自动发现 | 用程序集约定与注册特性接入应用 |
| 现场问题难复现 | 用统一异常、TraceId、结构化日志和性能观测保留证据 |

后端的核心原则是：业务规则按领域归属，跨模块能力按契约暴露，技术共性进入底座，事务由业务动作的编排者负责。

## 2. 为什么选择模块化单体

当前系统由 `KH.WMS.Server` 启动，各业务模块以项目引用方式进入同一 ASP.NET Core 进程。

```text
KH.WMS.Server
  -> Core / Config
  -> BaseDataModule
  -> InboundModule
  -> InventoryModule
  -> OutboundModule
  -> SystemModule
  -> TaskModule
  -> WarehouseModule
  -> DashboardModule
```

### 解决什么问题

入库、库存、出库和任务不是互相独立的简单服务。一次业务动作可能需要读取配置、执行策略、写多个模块拥有的数据，并保证全部成功或全部失败。

### 为什么不直接拆成微服务

微服务会同时引入网络失败、接口版本、服务发现、分布式追踪、消息重复、最终一致性和多套部署监控。当前业务首先需要的是可靠事务、快速联调和简单现场运维，这些需求由单进程和单部署单元更直接地满足。

### 获得的收益

- 跨模块调用仍是进程内接口调用，性能和排错路径直接。
- 复杂流程可以使用统一 UnitOfWork 和事务机制。
- 一个部署包更适合现场交付和 Windows 服务运行。
- Core、Config、Algorithms 等底座可以被所有模块一致使用。

### 付出的代价

- 任一模块发布通常需要发布整个后端。
- 进程级故障可能影响所有模块。
- 如果缺少边界约束，模块化单体容易退化成互相引用的大单体。

### 什么时候才值得评估拆分

只有出现明确的独立扩缩容、故障隔离、数据所有权、团队发布节奏或安全隔离需求时，才应评估服务拆分。拆分依据应是稳定业务边界和 Contract，而不是文件数量或“微服务更先进”。

## 3. 项目与职责地图

```text
Server                 组合根、HTTP 管道、托管与后台服务
Core                   DI、AOP、数据库、仓储、事务、响应、鉴权、缓存、日志
Entities               业务持久化实体、枚举和常量
Contracts              跨业务模块使用的接口与请求模型
Config                 配置库、扩展字段、编码、状态与配置解析
Algorithms             可选择、可替换的策略算法
Modules.*              入库、库存、出库、任务等具体业务能力
QuartzJob              独立作业项目，目前不在 Server 主引用链中
```

### 3.1 Server 为什么是组合根

`KH.WMS.Server/Program.cs` 负责选择和组装能力，而不是实现业务规则。当前入口完成：

- Autofac 容器和策略注册。
- `AddInfrastructure` 基础设施注册。
- Controller、全局异常过滤器和 TraceId 结果过滤器注册。
- `Modules.*` 与 Config Controller 的程序集发现。
- 数据与配置预热、后台服务和中间件管道。
- Swagger、上传静态文件、前端 SPA 托管与 history 回退。

组合根集中后，应用实际启用了哪些模块和横切能力可以从一个入口判断。Program 不应新增物料、入库或库存业务判断。

### 3.2 Core 为什么不能变成业务杂物箱

Core 存放所有模块都必须遵守的技术能力，例如仓储、事务、统一响应、注册特性、AOP、日志和缓存。它的价值是形成一致基础设施。

如果只有入库模块使用的规则被放入 Core，其他模块会被迫依赖无关业务，最终任何改动都影响全局。进入 Core 的判断标准不是“很多地方能调用”，而是“它是否与具体业务领域无关，并且所有调用者需要相同语义”。

### 3.3 Entities 与 DTO 为什么分开

Entity 描述持久化结构和 ORM 映射；DTO 描述某个接口或页面需要的数据形状。两者变化原因不同：数据库结构变化不应该自动扩大外部接口，页面聚合字段也不应该污染持久化实体。

简单 CRUD 基类可能直接使用 Entity，这是当前标准路径的一部分。流程型接口、聚合查询和对外集成仍应优先使用专用 DTO，避免把内部表结构变成长期契约。

### 3.4 Config 与 Algorithms 为什么单独存在

Config 处理扩展字段、编码、状态、系统参数和配置解析；Algorithms 处理上架、分配等可选择策略。它们解决的是“规则因仓库或场景变化”，不是某张业务表的普通 CRUD。

独立后，业务 Service 可以表达“解析配置”或“执行策略”，而不是散落大量仓库编号和条件分支。代价是执行链更长，必须能记录采用了什么配置或策略。

## 4. 当前真实项目引用

当前 `.csproj` 体现的主要关系包括：

```text
Server -> 各 Modules.* + Config
Entities -> Core
Contracts -> Entities + Config
Config -> Core
Algorithms -> Core

BaseDataModule -> Contracts
InboundModule -> Algorithms + Contracts
InventoryModule -> Contracts
OutboundModule -> Algorithms + Contracts
SystemModule -> Contracts + Entities
TaskModule -> Algorithms + Contracts
WarehouseModule -> Contracts
DashboardModule -> Entities
```

这张图描述当前事实，不代表可以随意增加引用。新的业务模块不应直接引用另一个 `Modules.*` 项目。需要跨模块能力时，优先把最小接口放入 Contracts，由数据所属模块实现。

`Contracts -> Config` 是当前已有依赖，新增契约时仍应避免把 Config 的具体实现模型扩散到所有模块；优先依赖稳定抽象和最小请求模型。

## 5. 一个业务模块内部为什么分层

典型模块结构：

```text
KH.WMS.Modules.SampleModule/
  Controllers/       HTTP 入口
  Interfaces/        模块内 Service 接口
  Services/          业务规则与流程编排
  DTOs/              接口入参与出参
  Contracts/         对其他模块暴露能力的实现
  Validation/        可插拔业务校验
```

不是每个模块都必须机械创建全部目录。目录服务于职责，有实际能力时再创建。

### 5.1 Controller：保持薄的 HTTP 边界

Controller 负责路由、模型接收、调用 Service 和返回响应。标准接口优先继承 `CrudController<TEntity>` 或 `ExtDataCrudController<TEntity>`。

这样设计能让 HTTP 协议与业务规则分开，Service 可以被 Contract、后台服务或测试复用。代价是一个请求要跨文件阅读，但职责清楚后排错范围更小。

Controller 不应直接注入多个 Repository、开启复杂事务或调用其他模块内部 Service。

### 5.2 Service：业务流程的主入口

Service 负责本模块规则、仓储访问、状态流转、跨模块 Contract 调用和流程事务。谁理解完整业务动作，谁就应该编排事务。

Service 不等于“把 Controller 代码搬一层”。它要表达业务意图，例如申请上架、分配库存、确认任务，而不仅是拼装数据库调用。

### 5.3 DTO：保护接口契约

创建、更新、流程动作和聚合查询应根据需要定义 DTO。DTO 可以包含页面需要但数据库没有的字段，也可以隐藏 Entity 内部字段。

复用 DTO 的前提是语义相同，而不是字段刚好一样。跨模块共享可变 DTO 会让多个模块互相牵制。

### 5.4 Contract：跨模块边界

Contract 接口放在 `KH.WMS.Contracts` 或稳定抽象项目，实现放在能力所属模块的 `Contracts/` 下。调用方只注入接口。

```text
Inbound Service
  -> ITaskContract
      -> TaskModule/Contracts/TaskContract
          -> Task 模块自己的 Service / Repository
```

调用方不需要知道任务模块怎样存表，也不应直接引用 `TaskHeaderService`。这样可以避免循环项目引用，并让被调用模块保护自己的数据规则。

Contract 的代价是要维护稳定接口。它应暴露业务能力和最小数据，不应成为“为了绕过边界而公开所有 Service 方法”的镜像。

### 5.5 Validation：可组合业务约束

当校验规则需要按流程、仓库或配置组合时，可插拔 Validator 比在 Service 中堆叠条件更容易扩展和测试。稳定且只有一个位置使用的简单校验直接写清楚即可，不必过度抽象。

## 6. Controller 和 Service 为什么自动发现

### Controller 发现

Program 通过程序集名包含 `.Modules.` 的约定，把业务模块加入 MVC `ApplicationPart`；`KH.WMS.Config` 是明确的特殊项。

收益是新增模块 Controller 不需要在启动入口逐个注册类型。代价是程序集命名、启动项目引用和扫描规则成为运行前提。Swagger 看不到 Controller 时，应优先检查这些条件。

### Service 发现

带 `[RegisteredService]` 或 `[SelfRegisteredService]` 的类型由 Autofac 注册模块发现，并按配置挂接拦截器。接口调用可以获得 AOP 能力。

收益是减少重复 DI 注册并统一横切行为。代价是注册关系不完全显示在 Program 中；多接口、Contract、Validator 和不需要拦截的服务必须显式声明意图。

自动发现只适合遵守稳定约定的类型。特殊生命周期、多个实现选择或外部客户端仍应使用清晰的显式注册。

## 7. AOP 和横切底座为什么集中

事务、日志、缓存、审计或其他横切行为如果复制到每个 Service，容易出现遗漏和执行顺序差异。Core 通过注册特性、拦截器、过滤器和中间件集中这些能力。

```text
HTTP 请求
  -> Middleware
  -> Authentication / Authorization
  -> Controller
  -> Service interface proxy
  -> AOP interceptors
  -> Service implementation
  -> Repository / UnitOfWork
  -> Result filter + TraceId
  -> 统一响应
```

### 获得的收益

- 业务 Service 更聚焦于规则。
- 事务和日志行为一致。
- 统一异常可以保留失败语义并生成可追踪响应。

### 代价与边界

- 自调用或直接 new 实现可能绕过代理。
- 拦截器顺序和异常处理必须有明确约定。
- 把业务流程塞进 AOP 会让执行链不可理解。

AOP 适合“所有符合条件的调用都应该发生”的技术行为，不适合决定某个单据下一步状态。

## 8. 为什么提供 CRUD 基类和扩展钩子

`CrudController<TEntity>`、`CrudService<TEntity>` 和仓储底座统一分页、过滤、增删改查、状态、导入导出等常见能力。`ExtDataCrudController<TEntity>` 在标准 CRUD 上增加动态扩展字段处理。

### 解决什么问题

资料维护接口数量多。如果每个模块手写相同查询与写入流程，会产生不同分页结构、错误处理和扩展行为。

### 获得的收益

- 与前端 `useCrudApi(module)` 形成稳定契约。
- 标准接口代码少，质量修复可以集中完成。
- 业务差异可以通过钩子覆盖，而不复制完整流程。

### 付出的代价

- 基类行为属于隐式执行链，开发者必须知道钩子时机。
- 基类改动影响多个实体，需要高回归强度。
- 过度继承会让流程型接口语义变得模糊。

### 什么时候不适用

跨聚合写入、复杂状态流转、主从表事务、外部系统交互和需要幂等控制的命令，应使用专用 DTO 与明确 Service 方法。它们可以复用仓储、事务和响应底座，但不必继承通用 CRUD 入口。

## 9. 事务边界为什么放在业务编排层

事务回答的是“哪些写入必须一起成功”。这个问题只有理解完整业务动作的 Service 才能回答。

以申请上架为例：

```text
Inbound Controller
  -> Inbound Service 开始业务动作
      -> 校验入库与容器状态
      -> Algorithms 选择上架策略
      -> ITaskContract 创建任务
      -> Contract 更新相关状态
      -> 提交事务
```

如果 Controller、每个 Repository 或每个被调用 Contract 都各自决定事务，可能出现前半段已提交、后半段失败。推荐由最外层业务编排者定义边界，内部能力参与同一 UnitOfWork，除非某个外部系统客观上无法加入本地事务。

遇到外部设备、消息或第三方接口时，应明确重试、幂等和补偿策略，不能假装网络调用属于数据库原子事务。

## 10. Config 和 Algorithms 的使用边界

### 适合进入 Config

- 仓库或客户可配置字段。
- 编码规则、状态定义和系统参数。
- 扩展字段表单配置。
- 多模块都要读取的稳定配置语义。

### 适合进入 Algorithms

- 上架、拣选、库存分配等可替换决策。
- 需要按配置选择不同实现的策略链。
- 相同输入上下文需要输出决策结果的算法能力。

### 不适合抽离

- 只属于单个业务模块的固定状态流转。
- 只有一处使用且不会变化的简单判断。
- 为了避免写几行清晰代码而创建的空泛“策略”。

配置化和策略化的收益是可替换，代价是行为不再只由代码决定。因此日志应记录关键配置、策略名称和结果，避免现场只能看到“系统自动选择”。

## 11. 统一响应、异常和 TraceId 为什么重要

后端通过全局异常过滤器、统一响应类型和 TraceId 结果过滤器建立一致的失败协议。前端可以统一判断成功与错误，日志可以用 TraceId 串起一次请求。

### 设计原则

- 业务校验失败保留明确错误信息，不返回伪成功。
- 未知异常由统一入口记录，不在每层重复捕获并丢失堆栈。
- 响应中的 TraceId 与日志上下文一致。
- Controller 不为每个接口重复包装完全相同的异常结构。

### 收益

- 前端错误处理简单一致。
- 现场人员提供 TraceId 后能快速定位日志。
- 事务可以根据真实异常正确回滚。

### 风险

如果 Service 捕获所有异常后返回成功对象，统一异常、事务回滚和 TraceId 都会失去价值。只捕获能够恢复、转换或补充上下文的异常。

## 12. ModuleBase 的当前真实状态

`KH.WMS.Core.Modularity` 中存在 `ModuleBase`、`ModuleLoader`、`RegisterModules` 和 `InitializeModulesAsync` 等能力，但当前 `Program.cs` 没有调用这套模块生命周期入口。

当前业务主链路是：

```text
Server 项目引用模块
  -> AssemblyService 找到被引用程序集
  -> MVC ApplicationPart 发现 Controller
  -> Autofac 注册模块发现带特性的 Service
```

因此新增普通业务模块时，不需要为了“模块化”补一个 `ModuleBase`。只有团队决定正式启用模块生命周期并完成启动、顺序、失败处理和迁移设计后，才能把预留代码描述为现行架构。

## 13. 一张表决定代码放哪里

| 需求 | 推荐位置 | 不推荐位置 | 判断原因 |
| --- | --- | --- | --- |
| HTTP 路由和参数接收 | 模块 `Controllers` | Core、Repository | 属于传输边界 |
| 本模块业务规则和流程 | 模块 `Services` | Controller | 需要复用、事务和测试 |
| 数据库持久化结构 | Entities 或 Config 实体目录 | DTO、Controller | 由数据存储方式驱动变化 |
| 页面或接口专用形状 | 模块 `DTOs` | 在 Entity 无限加字段 | 由契约和用例驱动变化 |
| 跨模块能力接口 | Contracts 或稳定 Abstractions | 调用方模块内部接口 | 多模块需要稳定依赖 |
| 跨模块能力实现 | 数据所属模块的 `Contracts` | 调用方模块 | 所有者保护自己的规则 |
| 技术横切能力 | Core | 任一业务模块复制 | 所有模块语义一致 |
| 配置、编码、动态字段 | Config | 业务模块复制配置表逻辑 | 属于共享配置底座 |
| 可替换决策算法 | Algorithms | Service 大量仓库分支 | 需要按上下文选择策略 |

## 14. 后端架构失控信号

- Controller 注入多个 Repository 并直接编排事务。
- 一个 `Modules.*` 项目引用另一个 `Modules.*` 项目。
- Contract 直接暴露对方 Service、Repository 或内部 Entity 图。
- 为消除循环依赖，把业务代码移动到 Core。
- 一个 Service 同时承担多个领域的数据所有权。
- 所有流程都继承 CRUD 基类，并依靠大量 override 修补语义。
- 仓库差异散落为硬编码编号和长条件分支。
- 捕获异常后返回成功，或日志没有 TraceId 和业务上下文。
- 新模块创建 `ModuleBase`，但启动入口实际上不会执行它。

## 15. 后端架构评审清单

- 需求的数据所有者和业务模块是否明确？
- Controller 是否只承担 HTTP 边界？
- 是标准 CRUD 还是需要专用 DTO 和流程方法？
- 事务覆盖了哪些写入，最外层编排者是谁？
- 是否直接引用其他模块实现；能否改为最小 Contract？
- 新增能力属于业务模块、Config、Algorithms 还是 Core，变化原因是什么？
- 自动注册的接口、生命周期和拦截行为是否明确？
- 异常能否触发正确回滚，并通过 TraceId 定位？
- 外部调用是否考虑超时、重试、幂等和补偿？
- `.csproj` 引用方向是否保持无模块互引？

## 16. 继续阅读

- [KH.WMS 架构总览](./KH.WMS架构总览.md)
- [KH.WMS 后端整体地图](/backend/后端开发指引V3教程/01-KH.WMS后端整体地图)
- [Controller / Service / Entity / DTO / Contract 职责边界](/backend/后端开发指引V3教程/04-职责边界)
- [完整 CRUD 底层执行链路](/backend/后端开发指引V3教程/06-完整CRUD底层执行链路)
- [跨模块 Contract 契约](/backend/后端开发指引V3教程/10-跨模块Contract契约)
- [业务流程、事务和校验扩展](/backend/后端开发指引V3教程/11-业务流程事务和校验扩展)
- [模块边界与分层职责](/backend/后端底层概念/02-模块边界与分层职责)
- [依赖注入自动注册与 AOP 代理](/backend/后端底层概念/03-依赖注入自动注册与AOP代理)
