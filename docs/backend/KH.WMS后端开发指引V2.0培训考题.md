---
title: "KH.WMS 后端开发指引 V2.0 培训考题（历史）"
description: "KH.WMS 后端开发指引 V2.0 培训考题（历史）：说明适用场景、当前实现、设计边界与开发或排障入口。"
status: archived
audience: "维护历史版本或执行迁移的开发人员"
reviewed: "2026-07-14"
search: false
replacement: "/backend/后端开发指引V3教程/README"
---

# KH.WMS 后端开发指引 V2.0 培训考题

## 考题说明

- 题型组成:单选题 20 题、多选题 15 题、判断题 15 题、填空题 6 题、场景分析题 4 题。
- 考核重点:后端开发流程、模块职责、职责边界、Contract、服务注册、CRUD/ExtData、事务校验、AOP、TraceId 与联调排查。
- 命题范围:仅限 `KH.WMS后端开发指引 V2.0.md` 以及当前后端实际代码中可验证的类、接口、目录和属性。

## 第一部分:单选题(20 题)

1. KH.WMS 后端开发的中心思想是?
   A. 所有业务逻辑都写在 Controller 中
   B. 业务代码落在正确业务模块里,模块之间通过稳定契约协作,通用技术能力交给底座
   C. 所有模块都直接引用对方 Services 目录
   D. 新功能优先复制相似 Controller

2. 后端启动项目位于哪个目录?
   A. `KH.WMS.Client/`
   B. `KH.WMS/KH.WMS.Server/`
   C. `KH.WMS/Modules/`
   D. `docs/backend/`

3. 后端核心入口文件是?
   A. `KH.WMS/KH.WMS.Server/Program.cs`
   B. `KH.WMS.Client/src/App.vue`
   C. `KH.WMS/Entities/KH.WMS.Entities.csproj`
   D. `KH.WMS/Modules/BaseDataModule/README.md`

4. 普通单模块 CRUD 的典型做法是?
   A. Entity + Interface + Service + `CrudController<TEntity>`
   B. 只新增 Controller
   C. 只新增 Contract
   D. Entity 放到模块 Services 目录

5. 实体有 `ExtData`,并且前端需要保存和回显动态字段时,Controller 应优先继承?
   A. `ControllerBase`
   B. `CrudController<TEntity>`
   C. `ExtDataCrudController<TEntity>`
   D. `BackgroundService`

6. 业务实体应放在哪个目录体系下?
   A. `KH.WMS/Modules/{Module}Module/.../Services/`
   B. `KH.WMS/Entities/KH.WMS.Entities/{Domain}/`
   C. `KH.WMS/KH.WMS.Server/Controllers/`
   D. `KH.WMS/Config/KH.WMS.Config/Controllers/`

7. 模块内 Service 接口一般放在哪里?
   A. 模块项目的 `Interfaces/`
   B. 模块项目的 `Controllers/`
   C. `KH.WMS.Server/Properties/`
   D. `KH.WMS.Client/src/api/`

8. 新增标准 CRUD Service 时,文档推荐继承哪个服务基类?
   A. `CrudService<TEntity>`
   B. `ControllerBase`
   C. `ModuleBase`
   D. `Exception`

9. Controller 的主要职责是?
   A. 直接写复杂业务和事务
   B. 做 API 入口,调用 Service
   C. 暴露跨模块能力给其他模块
   D. 保存所有状态机配置

10. Contract 的主要职责是?
    A. 做跨模块门面
    B. 替代 Controller 面向前端
    C. 暴露整套 CRUD 给所有模块
    D. 存放数据库实体

11. 判断业务归属模块时,文档强调优先看什么?
    A. 页面挂在哪个菜单
    B. 谁拥有这条数据和规则
    C. 哪个 Controller 名字更短
    D. 哪个模块文件少

12. 入库单、收货、组盘、上架请求主要属于哪个模块?
    A. `BaseDataModule`
    B. `InboundModule`
    C. `InventoryModule`
    D. `SystemModule`

13. 库存生成、库存扣减、库存锁定、库存移动主要属于哪个模块?
    A. `InventoryModule`
    B. `TaskModule`
    C. `WarehouseModule`
    D. `DashboardModule`

14. 用户、角色、权限、字典、参数主要属于哪个模块?
    A. `OutboundModule`
    B. `SystemModule`
    C. `InboundModule`
    D. `BaseDataModule`

15. Service 需要按接口注入和调用的一个重要原因是?
    A. AOP 拦截器基于接口代理工作
    B. Controller 无法调用类方法
    C. 实体必须实现接口
    D. Swagger 只能扫描接口

16. `[RegisteredService(ServiceType = typeof(IXxxService))]` 的作用是?
    A. 明确服务注册到哪个接口
    B. 指定 Controller 路由
    C. 指定数据库表名
    D. 开启前端菜单权限

17. 可插拔校验器应实现哪个接口?
    A. `IValidator`
    B. `ICrudService<TEntity>`
    C. `IRepository<TEntity, long>`
    D. `IUnitOfWork`

18. Validator 校验通过时应返回什么?
    A. `null`
    B. `false`
    C. 空对象
    D. 抛出所有异常

19. 多表写入时需要重点处理什么?
    A. 事务边界
    B. 前端路由名称
    C. CSS 样式
    D. 图片资源

20. Swagger 里看不到新 Controller 时,优先检查项不包括哪一项?
    A. Controller 是否在 `.Modules.` 程序集中
    B. Controller 是否有 `[Route("api/xxx")]`
    C. Controller 是否继承 `ControllerBase` 或 CRUD 基类
    D. 前端页面颜色是否正确

## 第二部分:多选题(15 题)

1. 文档中提到一条需求从理解到落代码,需要考虑哪些事项?
   A. 判断业务归属模块
   B. 判断需求类型
   C. 建 Entity / Interface / Service / Controller
   D. 只看菜单位置决定模块

2. `Program.cs` 负责的事项包括?
   A. 使用 Autofac 作为 DI 容器
   B. 注册带 `[RegisteredService]` / `[SelfRegisteredService]` 的服务
   C. 扫描业务模块 Controller
   D. 托管 Swagger 和前端 SPA

3. 以下哪些属于技术底座而不是业务模块教程模板?
   A. `KH.WMS.Core`
   B. `KH.WMS.Config`
   C. `KH.WMS.Algorithms`
   D. `BaseDataModule`

4. 一个业务模块通常可能包含哪些目录?
   A. `Controllers/`
   B. `Interfaces/`
   C. `Services/`
   D. `Validation/`

5. 实体的基本要求包括?
   A. 继承 `BaseEntity<long>`
   B. 表字段用实体属性表达
   C. 需要扩展字段时有 `string? ExtData`
   D. 在实体中查库或调用 Service

6. 以下哪些属于 DTO 适合承担的内容?
   A. 前端请求模型
   B. 前端响应模型
   C. 批量动作参数
   D. 动态字段保存格式中的 `extDataRaw`

7. 判断是否需要 Contract 时,文档提出的判断问题包括?
   A. 别的模块是否需要调用本模块能力
   B. 本模块是否需要调用别的模块能力
   C. 是否会写入别的模块拥有的数据
   D. 是否只是本模块 Controller 调本模块 Service

8. Contract 应该暴露什么?
   A. 跨模块必须使用的业务动作
   B. 对方模块需要的最小查询能力
   C. 稳定的请求/结果模型
   D. 整套内部 CRUD 和仓储对象

9. 以下哪些场景通常不要写 Contract?
   A. 只给前端调用的接口
   B. 只在本模块内部复用的逻辑
   C. 为了少写 Service 接口
   D. 别的模块需要调用本模块的核心业务能力

10. `[RegisteredService]` 推荐明确写 `ServiceType` 的场景包括?
    A. 普通业务 Service
    B. Contract 实现
    C. Validator 实现
    D. 多接口实现类

11. `WithoutInterceptor = true` 适合谨慎使用在哪些服务上?
    A. 基础设施服务
    B. Validator
    C. 会被拦截器内部调用、容易循环依赖的服务
    D. 需要 `[ConfigValidation]` 生效的业务 Service

12. `ExtDataCrudController<TEntity>` 相比 `CrudController<TEntity>` 主要额外处理哪些能力?
    A. 保存时接收 `extDataRaw`
    B. 将动态字段序列化到 `ExtData`
    C. 详情回显时展开动态字段
    D. 自动替代所有正式数据库列

13. 新增可插拔校验器的检查项包括?
    A. 在 `ValidatorCodes` 增加唯一编码
    B. 实现 `IValidator`
    C. 标记 `[RegisteredService(WithoutInterceptor = true, ServiceType = typeof(IValidator))]`
    D. 目标方法增加 `[ConfigValidation(ValidatorCodes.XXX)]`

14. AOP 拦截器链中,文档提到的拦截器能力包括?
    A. 日志
    B. 缓存
    C. 配置校验
    D. 异常和性能

15. 联调排查时,前端报错应要求提供哪些信息?
    A. 接口
    B. 时间
    C. TraceId
    D. 页面主题色

## 第三部分:判断题(15 题)

1. 新增业务时,推荐先复制相似 Controller,再慢慢调整业务归属。  
2. `KH.WMS.Config` 有 Controller、Service、Contract,因此可以作为新业务模块的模板。  
3. 业务模块 Controller 必须能被启动项目扫描到,程序集命名需要符合扫描规则。  
4. 普通表维护在不需要动态字段时,不应该使用 `ExtDataCrudController`。  
5. 实体集中放在 `KH.WMS.Entities`,有利于仓储、Contract 和多个业务模块稳定复用。  
6. 模块内 `Interfaces/` 就是跨模块 Contract,其他模块应该直接引用它。  
7. Controller 应保持轻量,复杂业务、事务和跨模块编排应放到 Service。  
8. Contract 直接面向前端,前端应绕过 Controller 调用 Contract。  
9. 跨模块流程由调用方控制事务,被调 Contract 不应随意自己开启大事务。  
10. Validator 如果依赖事务内锁定后的状态,更适合放在 Service 事务内部。  
11. Service 关闭拦截器后,`[ConfigValidation]` 仍然一定会执行。  
12. `ValidatorCodes`、`IValidator.Code`、`ConfigValidation` 三处编码应保持一致。  
13. WCS/PDA/批量动作要考虑重复提交、状态复核和锁。  
14. 库存数量变化应考虑并发和事务,并通过库存模块能力暴露。  
15. DashboardModule 是轻量聚合查询模块,不建议暴露 Contract。  

## 第四部分:填空题(6 题)

1. 文档将核心原则浓缩为一句话:Controller 做入口,Service 做业务,Contract 做跨模块门面,`________` 做技术底座。

2. 后端常用本地 Swagger 地址是 `________`。

3. 普通 CRUD Controller 推荐继承 `________`。

4. 动态扩展字段实体必须有 `________` 属性。

5. Contract 接口推荐放在 `________/{Domain}/` 下定义。

6. 校验器编码常量文件是 `KH.WMS.Core/Validation/________`。

## 第五部分:场景分析题(4 题)

1. 新增“物料分类维护”接口。请说明它属于哪类需求、归属哪个模块、需要新增或复用哪些后端对象,Controller 应继承哪个基类。

2. 新增“供应商动态字段维护”。实体已有 `ExtData`,前端需要保存和回显动态字段。请说明为什么不能只用普通 CRUD,并写出后端和前端联调时需要关注的关键点。

3. 入库组盘完成后需要申请创建上架任务。请说明该流程涉及哪些模块,调用方应该注入哪个 Contract,为什么不能在入库 Service 里直接插任务表,事务边界由谁控制。

4. `ContainerBindAsync` 上的可配置校验没有执行。请按文档给出排查步骤,并说明目标 Service 和 Validator 在注册及拦截器设置上分别应注意什么。

## 参考答案与解析

### 单选题答案

1. B。来源:第 0.1 章。
2. B。来源:第 1.1 章。
3. A。来源:第 1.1 章。
4. A。来源:第 2.1 章。
5. C。来源:第 2.1、6.2、6.3 章。
6. B。来源:第 2.3 章。
7. A。来源:第 2.4 章。
8. A。来源:第 2.5 章。
9. B。来源:第 3.4 章。
10. A。来源:第 3.5、4 章。
11. B。来源:第 2.2 章。
12. B。来源:第 2.2、8.2 章。
13. A。来源:第 2.2、8.3 章。
14. B。来源:第 2.2、8.7 章。
15. A。来源:第 2.4、7.6 章。
16. A。来源:第 5.3 章。
17. A。来源:第 2.9 章。
18. A。来源:第 2.9、附录 B.4。
19. A。来源:第 2.8、7.2 章。
20. D。来源:第 1.1、附录 B.6。

### 多选题答案

1. A、B、C。来源:第 0.1 章。
2. A、B、C、D。来源:第 1.1 章。
3. A、B、C。来源:第 0.3 章。
4. A、B、C、D。来源:第 1.3 章。
5. A、B、C。来源:第 2.3 章。
6. A、B、C、D。来源:第 3.2、6.2 章。
7. A、B、C。来源:第 2.7 章。
8. A、B、C。来源:第 4.3 章。
9. A、B、C。来源:第 4.6 章。
10. A、B、C、D。来源:第 5.3、5.5、5.7 章。
11. A、B、C。来源:第 5.6、7.6、附录 B.4。
12. A、B、C。来源:第 6.2、6.4 章。
13. A、B、C、D。来源:第 2.9、附录 B.4。
14. A、B、C、D。来源:第 7.6 章。
15. A、B、C。来源:附录 B.6。

### 判断题答案

1. 错。文档明确不推荐先搜 Controller 复制。
2. 错。`KH.WMS.Config` 是技术底座,不是业务模块教程模板。
3. 对。来源:第 1.1 章。
4. 对。来源:第 6.3、附录 B.1。
5. 对。来源:第 2.3 章。
6. 错。模块内接口不是跨模块 Contract,其他模块不应引用模块内 `Interfaces/`。
7. 对。来源:第 3.3、3.4 章。
8. 错。Contract 不直接面向前端,前端仍调 Controller。
9. 对。来源:第 4.5 章。
10. 对。来源:第 2.9、附录 C。
11. 错。关闭拦截器会导致 `ConfigValidationInterceptor` 没机会执行。
12. 对。来源:第 7.6、附录 B.4。
13. 对。来源:第 0.1、7.2 章。
14. 对。来源:第 8.3 章。库存模块对外通过 `IInventoryContract` 暴露库存相关能力。
15. 对。来源:第 8.8 章。

### 填空题答案

1. `Core/Config/Algorithms`
2. `http://localhost:9291/swagger`
3. `CrudController<TEntity>`
4. `string? ExtData`
5. `KH.WMS.Contracts`
6. `ValidatorCodes.cs`

### 场景分析题评分要点

1. 物料分类维护:
   - 判断为普通单模块 CRUD。
   - 归属 `BaseDataModule`。
   - Entity 放 `KH.WMS.Entities/{Domain}/`,继承 `BaseEntity<long>`。
   - 模块内新增 Interface 继承 `ICrudService<TEntity>`。
   - Service 继承 `CrudService<TEntity>`,标记 `[RegisteredService(ServiceType = typeof(IXxxService))]`。
   - Controller 继承 `CrudController<TEntity>`,使用 `api/xxx` 风格路由。

2. 供应商动态字段维护:
   - 因实体有 `ExtData`,且前端需要保存/回显动态字段,应使用 `ExtDataCrudController<TEntity>`。
   - Service 仍按普通 `ICrudService<TEntity>` / `CrudService<TEntity>` 注册。
   - 前端保存时关注 `extDataRaw`。
   - 详情接口应能把 `ExtData` 展开回显。
   - 分页是否展开由前端 load 函数处理,不能误以为后端自动展开所有分页字段。
   - 动态字段只适合客户化扩展信息,会查询、排序、统计或参与规则的字段应优先建正式列。

3. 入库组盘后申请上架任务:
   - 入库组盘流程由 `InboundModule` 编排。
   - 创建上架任务能力归属 `TaskModule`。
   - 入库模块调用 `ITaskContract.CreatePutawayTaskAsync`。
   - 不应在入库 Service 里直接插任务表,否则会把任务模块内部规则搬到入库模块,破坏模块边界。
   - 如涉及容器注册/状态变更,走 `IContainerContract`。
   - 跨模块流程事务由调用方控制,失败路径要能回滚。

4. `ContainerBindAsync` 校验未执行:
   - 确认目标方法是否通过接口注入的 Service 调用。
   - 确认目标 Service 没有设置 `WithoutInterceptor = true`。
   - 确认目标方法标记了 `[ConfigValidation(ValidatorCodes.XXX)]`。
   - 确认 Validator 实现 `IValidator`,并注册为 `[RegisteredService(WithoutInterceptor = true, ServiceType = typeof(IValidator))]`。
   - 确认 `ValidatorCodes`、`IValidator.Code`、`ConfigValidation` 三处编码一致。
   - 确认目标方法返回类型适合 `ConfigValidationInterceptor`,例如 `Task<ServiceResult>` 或 `Task<ServiceResult<T>>`。
   - Validator 类只实现 `IValidator`,避免注册到非预期接口。

## 继续阅读

- [当前维护版本](/backend/后端开发指引V3教程/README)
- [学习路径](/learning-path)
- [培训资料下载](/training-materials)
