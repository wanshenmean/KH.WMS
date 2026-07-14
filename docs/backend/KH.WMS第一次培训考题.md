---
title: "KH.WMS 第一次培训考题"
description: "KH.WMS 第一次培训考题：说明适用场景、当前实现、设计边界与开发或排障入口。"
status: training
audience: "新成员、培训讲师与参与考核的开发人员"
reviewed: "2026-07-14"
---

# KH.WMS 第一次培训考题

## 考题说明

- 题型组成：单选题 36 题、多选题 25 题、判断题 22 题、填空题 13 题、场景分析题 4 题。
- 考核重点：项目调用关系、技术栈、后端类库与模块职责、前端目录与组件、文件落点、依赖边界、新增功能落地方式。

## 第一部分：单选题（36 题）

1. KH.WMS 后端解决方案默认位于哪个路径？
   A. `KH.WMS.Client`
   B. `KH.WMS/KH.WMS.sln`
   C. `docs/backend`
   D. `KH.WMS/Modules`

2. KH.WMS 前端工程默认位于哪个路径？
   A. `KH.WMS.Client`
   B. `KH.WMS.Server`
   C. `KH.WMS.Core`
   D. `KH.WMS.Contracts`

3. 后端主要类库目标框架是哪个？
   A. `net6.0`
   B. `net7.0`
   C. `net8.0`
   D. `netstandard2.1`

4. `KH.WMS.Server` 使用的项目 SDK 类型是？
   A. `Microsoft.NET.Sdk`
   B. `Microsoft.NET.Sdk.Web`
   C. `Microsoft.NET.Sdk.Worker`
   D. `Microsoft.NET.Sdk.Razor`

5. 后端 ORM 使用的是？
   A. EF Core
   B. Dapper
   C. SqlSugarCore
   D. NHibernate

6. 当前后端 DI 主要使用哪组依赖？
   A. Unity / Castle Windsor
   B. Autofac / Autofac.Extensions.DependencyInjection
   C. Ninject / StructureMap
   D. SimpleInjector / DryIoc

7. 当前后端 AOP 主要使用哪组依赖？
   A. MediatR / FluentValidation
   B. Autofac.Extras.DynamicProxy / Castle.Core.AsyncInterceptor
   C. Polly / Scrutor
   D. Quartz / Hangfire

8. 后端日志体系主要基于哪个日志库？
   A. NLog
   B. log4net
   C. Serilog
   D. Microsoft TraceSource

9. API 文档使用的是？
   A. NSwag
   B. Swashbuckle.AspNetCore
   C. DocFX
   D. Scalar

10. 性能分析入口对应的依赖是？
      A. MiniProfiler.AspNetCore.Mvc
      B. BenchmarkDotNet
      C. OpenTelemetry
      D. AppMetrics

11. 前端主框架是？
      A. React
      B. Angular
      C. Vue
      D. Svelte

12. 前端构建工具是？
      A. Webpack
      B. Vite
      C. Rollup CLI
      D. Parcel

13. 前端 UI 组件库是？
      A. Ant Design Vue
      B. Naive UI
      C. Element Plus
      D. Vuetify

14. 前端状态管理使用的是？
      A. Vuex
      B. Redux
      C. Pinia
      D. MobX

15. 前端 HTTP 请求库是？
      A. fetch 封装
      B. axios
      C. superagent
      D. ky

16. Vite 开发服务默认端口是？
      A. 5173
      B. 3000
      C. 8080
      D. 9291

17. 后端常用本地端口是？
      A. 3000
      B. 5173
      C. 9291
      D. 1433

18. Swagger API 文档访问地址是？
      A. `http://localhost:9291/swagger/index.html`
      B. `http://localhost:3000/swagger`
      C. `http://localhost:9291/api/swagger`
      D. `http://localhost:3000/api-docs`

19. 前端 `/api` 代理目标是？
      A. `http://localhost:3000`
      B. `http://localhost:9291`
      C. `ws://localhost:9291`
      D. `http://localhost:8080`

20. `KH.WMS.Core` 的主要职责是？
      A. 只存放页面组件
      B. 存放基础设施能力
      C. 存放所有业务页面
      D. 存放数据库脚本

21. `KH.WMS.Entities` 的主要职责是？
      A. 存放业务实体
      B. 存放 API 请求封装
      C. 存放路由配置
      D. 存放前端布局

22. `KH.WMS.Contracts` 的主要职责是？
      A. 存放跨模块接口、请求对象和事件
      B. 存放运行日志
      C. 存放上传文件
      D. 存放 Vue 组件

23. `BaseDataModule` 主要负责？
      A. 用户、角色、权限
      B. 物料、客户、供应商、容器等基础资料
      C. 库存快照
      D. 出库分配

24. `SystemModule` 主要负责？
      A. 仓储库位
      B. 前端构建配置
      C. 用户、角色、权限、菜单、参数、字典等
      D. 入库单收货

25. `WarehouseModule` 主要负责？
      A. 仓库、库区、巷道、库位等仓储基础
      B. 前端消息通知
      C. Swagger 配置
      D. E2E 测试

26. 前端 API 请求封装目录是？
      A. `src/api`
      B. `src/views`
      C. `src/stores`
      D. `src/layouts`

27. 前端页面视图目录是？
      A. `src/utils`
      B. `src/views`
      C. `src/components`
      D. `src/directives`

28. 页面专属弹窗组件应优先放在哪里？
      A. `src/components`
      B. `src/views/{业务域}/components`
      C. `src/stores`
      D. `src/router`

29. 多页面复用组件应提升到哪里？
      A. `src/components/KhXxx/index.vue`
      B. `src/views/{业务域}/api`
      C. `src/typings`
      D. `public`

30. 后端菜单的 `component` 字段需要匹配什么？
      A. `/src/views/{component}.vue`
      B. `/src/api/{component}.js`
      C. `/src/stores/{component}.js`
      D. `/src/components/{component}.vue`

31. 新增普通 CRUD 服务时，当前推荐继承哪个服务基类？
      A. `ControllerBase`
      B. `CrudService<TEntity>`
      C. `BackgroundService`
      D. `DbContext`

32. 新增普通 CRUD 控制器时，当前推荐继承哪个控制器基类？
      A. `CrudController<TEntity>`
      B. `RazorPage`
      C. `Hub`
      D. `Middleware`

33. `KH.WMS.Server` 中用于支持 Windows 服务部署的配置是？
      A. `UseWindowsService`
      B. `UseSwaggerUI`
      C. `UseStaticFiles`
      D. `UseRouting`

34. `DailySnapshotBackgroundService` 属于哪类能力？
      A. 后台服务
      B. 前端组件
      C. 路由守卫
      D. 请求 DTO

35. `KH.WMS.Client/public` 目录更适合放哪类内容？
      A. 前端静态资源或运行时文件
      B. 后端实体类
      C. 数据库迁移文件
      D. 控制器基类

36. `src/utils/request.js` 的主要用途是？
      A. Axios 请求封装
      B. Pinia 状态注册
      C. 路由组件扫描
      D. E2E 测试入口

## 第二部分：多选题（25 题）

1. KH.WMS 当前包含哪些主要部分？
   A. `.NET 8 / ASP.NET Core Web API` 后端
   B. `Vue 3 / Vite / Element Plus` 前端
   C. Android 原生客户端
   D. iOS 原生客户端

2. `KH.WMS.Core` 提供的基础设施包括哪些？
   A. 数据库
   B. 认证
   C. 日志
   D. 具体入库上架业务流程

3. 后端日志体系相关依赖包括哪些？
   A. Serilog.AspNetCore
   B. log4net
   C. Serilog.Sinks.Seq
   D. Serilog.Sinks.File

4. 前端自动化相关依赖包括哪些？
   A. unplugin-auto-import
   B. unplugin-vue-components
   C. jQuery UI
   D. @playwright/test

5. `KH.WMS.Core` 中与数据和中间件能力相关的目录包括哪些？
   A. `Database`
   B. `Views`
   C. `Pages`
   D. `Middlewares`

6. 当前文档列出的业务模块包括哪些？
   A. `InboundModule`
   B. `PaymentModule`
   C. `InventoryModule`
   D. `MobileModule`

7. `src` 下当前文档提到的目录包括哪些？
   A. `Controllers`
   B. `api`
   C. `components`
   D. `backend`

8. 适合放在 `src/components` 的组件包括哪些？
   A. `MaterialBrandFormDialog`
   B. `KhTable`
   C. `KhDialog`
   D. `KhPage`

9. 新增物料品牌示例中，后端可能涉及哪些文件？
   A. `material-brand.vue`
   B. `MdMaterialBrand.cs`
   C. `MaterialBrandFormDialog.vue`
   D. `MaterialBrandService.cs`

10. `KH.WMS.Core` 中与 Web API 请求处理相关的目录包括哪些？
      A. `src/views`
      B. `components`
      C. `Filters`
      D. `Middlewares`

11. `KH.WMS.Contracts` 中适合放哪些内容？
      A. 跨模块接口
      B. 请求对象
      C. 事件
      D. 页面样式

12. 前端核心技术栈包括哪些？
      A. Vue
      B. Angular
      C. Vite
      D. Element Plus

13. `Program.cs` 当前涉及哪些启动行为？
      A. 使用 Windows Service 支持
      B. 注册 DailySnapshotBackgroundService
      C. 注册前端 Pinia Store
      D. 启用 SPA 回退

14. 业务模块中适合放控制器和接口定义的目录包括哪些？
      A. `Controllers`
      B. `wwwroot`
      C. `public`
      D. `Interfaces`

15. 前端根目录包含哪些？
      A. `src`
      B. `pom.xml`
      C. `public`
      D. `backend.sln`

16. `src/api` 中包含哪些业务接口文件？
      A. `Program.cs`
      B. `basedata.js`
      C. `warehouse.js`
      D. `BaseController.cs`

17. 新增后端普通业务功能时，常见落点包括哪些？
      A. 前端弹窗放后端 `Controllers`
      B. 实体放 `Entities`
      C. 服务接口放模块 `Interfaces`
      D. 服务实现放模块 `Services`

18. `KH.WMS.Server` 启动项目目录中可能包含哪些内容？
      A. `src/views`
      B. `Program.cs`
      C. `src/router`
      D. `Profiles`

19. 前端 `src/views` 中和 WMS 仓储作业相关的业务页面目录包括哪些？
      A. `android`
      B. `dashboard`
      C. `inventory`
      D. `inbound`

20. `src/stores` 中包含哪些状态文件？
      A. `app.js`
      B. `user.js`
      C. `warehouse.cs`
      D. `permission.js`

21. 新增前端业务功能时，常见落点包括哪些？
      A. API 放 `src/api`
      B. 控制器放 `src/stores`
      C. 页面放 `src/views/{业务域}`
      D. 后端实体放 `src/components`

22. 前端依赖原则中推荐的做法包括哪些？
      A. 页面调用后端先封装到 `src/api`
      B. 页面内部表单放 `src/views/{业务域}/components`
      C. 多页面复用组件提升到 `src/components`
      D. 复杂业务流程全部塞进 `utils`

23. 新增页面后，通常需要优先检查哪些页面路由关联点？
      A. 页面文件是否放在 `src/views/{业务域}`
      B. 菜单 `component` 是否匹配页面路径
      C. 是否新增后端实体到前端 `components`
      D. 是否把控制器放到 `src/stores`

24. `KH.WMS.Server` 的关键职责包括哪些？
      A. DI 注册
      B. Android 安装包打包
      C. Swagger
      D. 中间件

25. `src/views` 中包含哪些业务域目录？
      A. `Controllers`
      B. `basedata`
      C. `inbound`
      D. `BaseController.cs`

## 第三部分：判断题（22 题）

1. 后端主要类库目标框架为 `net8.0`。

2. `KH.WMS.Server` 可以托管前端 `dist` 产物。

3. `KH.WMS.Core` 适合放通用 CRUD、仓储、认证、日志、缓存等能力。

4. 具体入库、出库、库存、任务业务流程应放到对应业务模块中。

5. 一个业务模块可以直接引用另一个模块的 `Services` 实现类，这是推荐做法。

6. 前端页面应优先直接散写 `axios` 调用。

7. 前端页面调用后端时，推荐先在 `src/api/{module}.js` 封装。

8. 页面专属弹窗一开始就应该放进全局 `src/components`。

9. 多页面复用组件适合放到 `src/components/KhXxx/index.vue`。

10. `src/views/**/components` 不作为页面路由组件。

11. 后端菜单 `component` 字段需要匹配 `/src/views/{component}.vue`。

12. `src/stores` 适合放登录态、权限、字典、WebSocket 等跨页面状态。

13. 页面内部临时状态也必须放到 Pinia Store。

14. `src/utils` 适合放纯工具函数。

15. 复杂业务流程应尽量塞进 `utils` 以便复用。

16. 新增普通 CRUD 服务推荐使用 `IRepository<TEntity, long>`、`IUnitOfWork`、`IDetailSaveService`。

17. 新增普通 CRUD 服务推荐继承 `CrudService<TEntity>`。

18. 新增普通 CRUD 控制器推荐继承 `CrudController<TEntity>`。

19. 前端构建后只能由 Vite 预览，不能由后端托管。

20. `KH.WMS.Server/BackgroundServices` 适合放后台服务类。

21. `src/utils/request.js` 适合承载前端请求封装能力。

22. 后端业务控制器通常应优先放到对应业务模块的 `Controllers` 目录。

## 第四部分：填空题（13 题）

1. 后端解决方案路径是 `__________`。

2. 前端工程路径是 `__________`。

3. 后端主要目标框架是 `__________`。

4. 后端 ORM 依赖是 `__________`。

5. Vite 开发端口是 `__________`。

6. 后端常用本地端口是 `__________`。

7. 前端 `/api` 代理目标是 `__________`。

8. `KH.WMS.Core` 中数据库相关能力主要放在 `__________` 目录。

9. 业务模块 API 控制器通常放在模块的 `__________` 目录。

10. 业务模块服务实现通常放在模块的 `__________` 目录。

11. 前端后端接口请求封装目录是 `__________`。

12. 页面专属组件推荐放在 `__________`。

13. 多页面复用组件推荐放在 `__________`。

## 第五部分：场景分析题（4 题）

1. 你要新增“供应商等级”基础资料，包含实体、CRUD 接口、前端列表页和编辑弹窗。请说明后端和前端文件应分别放到哪些目录，并说明哪些内容不应放到 `KH.WMS.Core`。

2. 你要给 `src/views/basedata/material-brand.vue` 配置菜单。后端菜单的 `component` 字段应如何填写？如果把弹窗组件放到 `src/views/basedata/components`，它会不会被动态路由当成页面？为什么？

3. 你要做一个多页面复用的高级表格组件，并且一个只给入库收货页使用的收货弹窗。请分别说明它们的前端目录落点。

4. 你要排查一个模块控制器没有出现在 Swagger 中的问题。结合文档说明 `Program.cs` 中控制器发现机制和你会优先检查哪些点。

## 参考答案与解析

### 单选题答案

1. B。后端解决方案位于 `KH.WMS/KH.WMS.sln`。

2. A。前端工程位于 `KH.WMS.Client`。

3. C。后端主要目标框架为 `net8.0`。

4. B。`KH.WMS.Server` 使用 `Microsoft.NET.Sdk.Web`。

5. C。ORM 为 SqlSugarCore。

6. B。DI 使用 Autofac 及其 ASP.NET Core 集成包。

7. B。AOP 使用 DynamicProxy 和 AsyncInterceptor。

8. C。日志体系基于 Serilog。

9. B。API 文档使用 Swashbuckle.AspNetCore。

10. A。性能分析入口为 MiniProfiler.AspNetCore.Mvc。

11. C。前端主框架为 Vue。

12. B。构建工具为 Vite。

13. C。UI 组件库为 Element Plus。

14. C。状态管理为 Pinia。

15. B。HTTP 请求使用 axios。

16. B。Vite 开发端口为 3000。

17. C。后端常用本地端口为 9291。

18. A。Swagger 地址为 `http://localhost:9291/swagger/index.html`。

19. B。`/api` 代理到 `http://localhost:9291`。

20. B。`KH.WMS.Core` 存放基础设施能力。

21. A。`KH.WMS.Entities` 存放业务实体。

22. A。`KH.WMS.Contracts` 存放跨模块契约。

23. B。`BaseDataModule` 负责基础资料。

24. C。`SystemModule` 负责系统管理能力。

25. A。`WarehouseModule` 负责仓储基础。

26. A。API 请求封装在 `src/api`。

27. B。页面视图在 `src/views`。

28. B。页面专属组件放 `src/views/{业务域}/components`。

29. A。多页面复用组件放 `src/components/KhXxx/index.vue`。

30. A。动态路由匹配 `/src/views/{component}.vue`。

31. B。普通 CRUD 服务推荐继承 `CrudService<TEntity>`。

32. A。普通 CRUD 控制器推荐继承 `CrudController<TEntity>`。

33. A。`UseWindowsService` 用于支持 Windows Service 部署。

34. A。`DailySnapshotBackgroundService` 是后台服务。

35. A。`public` 目录适合放前端静态资源或运行时文件。

36. A。`request.js` 用于 Axios 请求封装。

### 多选题答案

1. A、B。项目包含后端和前端两部分。

2. A、B、C。Core 承载通用基础能力，具体入库上架流程应放对应业务模块。

3. A、C、D。日志体系基于 Serilog，log4net 不属于当前文档列出的日志依赖。

4. A、B、D。jQuery UI 不在文档技术栈中。

5. A、D。`Database`、`Middlewares` 属于 Core 中的数据和中间件能力目录，`Views`、`Pages` 不属于这些能力目录。

6. A、C。`InboundModule`、`InventoryModule` 属于当前文档列出的业务模块，`PaymentModule` 和 `MobileModule` 不是。

7. B、C。`api`、`components` 属于前端 `src` 下目录，`Controllers` 和 `backend` 不是前端 `src` 目录。

8. B、C、D。`KhTable`、`KhDialog`、`KhPage` 适合全局复用，页面专属弹窗不应直接放全局组件目录。

9. B、D。实体和服务实现属于后端文件，`material-brand.vue` 与 `MaterialBrandFormDialog.vue` 是前端文件。

10. C、D。`Filters`、`Middlewares` 与 Web API 请求处理相关，`src/views` 和 `components` 是前端目录概念。

11. A、B、C。页面样式不属于 Contracts。

12. A、C、D。前端核心技术栈为 Vue、Vite、Element Plus，Angular 不属于当前技术栈。

13. A、B、D。`Program.cs` 涉及 Windows Service、后台服务和 SPA 回退，不负责注册前端 Pinia Store。

14. A、D。业务模块中控制器放 `Controllers`，接口定义放 `Interfaces`，`wwwroot` 和 `public` 不是业务模块接口或控制器目录。

15. A、C。`src`、`public` 属于前端工程根目录内容，`pom.xml` 和 `backend.sln` 不是。

16. B、C。`basedata.js`、`warehouse.js` 属于前端 API 文件，`Program.cs` 和 `BaseController.cs` 是后端文件。

17. B、C、D。实体、服务接口、服务实现都有明确后端落点，前端弹窗不应放后端 `Controllers`。

18. B、D。`Program.cs` 和 `Profiles` 属于启动项目相关内容，`src/views`、`src/router` 属于前端目录。

19. C、D。`inventory`、`inbound` 分别对应库存和入库页面，`android` 和 `dashboard` 不属于仓储作业页面目录。

20. A、B、D。`app.js`、`user.js`、`permission.js` 属于 Store 文件，`warehouse.cs` 不是前端 Store 文件。

21. A、C。前端新增功能通常涉及 API 和页面，控制器不放 `src/stores`，后端实体也不放 `src/components`。

22. A、B、C。复杂业务流程不应全部塞进 `utils`。

23. A、B。新增页面的页面路由关联点主要是页面文件路径和菜单 `component`，后端实体和控制器不放前端目录。

24. A、C、D。Server 负责 DI、Swagger、中间件等启动能力，不负责 Android 安装包打包。

25. B、C。`basedata`、`inbound` 属于前端业务域目录，`Controllers` 和 `BaseController.cs` 是后端目录或文件概念。

### 判断题答案

1. 对。目标框架为 `net8.0`。

2. 对。后端可托管前端 `dist`。

3. 对。Core 存放通用技术能力。

4. 对。业务流程放对应模块。

5. 错。模块间调用应通过 Contracts。

6. 错。应先封装到 `src/api`。

7. 对。页面通过 API 封装调用后端。

8. 错。页面专属组件放页面业务域下。

9. 对。多页面复用组件提升到 `src/components`。

10. 对。`views/**/components` 不作为路由页面。

11. 对。`component` 要匹配 views 文件。

12. 对。Store 适合放跨页面状态。

13. 错。页面内部临时状态不必放 Store。

14. 对。`utils` 放纯工具函数。

15. 错。复杂业务流程不应塞进 `utils`。

16. 对。这是当前 CRUD 推荐依赖。

17. 对。推荐继承 `CrudService<TEntity>`。

18. 对。推荐继承 `CrudController<TEntity>`。

19. 错。后端也可以托管 SPA。

20. 对。后台服务类适合放 `BackgroundServices`。

21. 对。`request.js` 用于前端请求封装。

22. 对。业务控制器通常放对应模块的 `Controllers` 目录。

### 填空题答案

1. `KH.WMS/KH.WMS.sln`

2. `KH.WMS.Client`

3. `net8.0`

4. `SqlSugarCore`

5. `3000`

6. `9291`

7. `http://localhost:9291`

8. `Database`

9. `Controllers`

10. `Services`

11. `src/api`

12. `src/views/{业务域}/components`

13. `src/components/KhXxx/index.vue`

### 场景分析题评分要点

1. 后端实体放 `Entities/KH.WMS.Entities/BaseData`，服务接口/实现/控制器/DTO 放 `BaseDataModule` 对应目录；前端 API 放 `src/api/basedata.js`，页面放 `src/views/basedata`，弹窗放 `src/views/basedata/components`；具体供应商等级业务规则不应放 `KH.WMS.Core`。

2. `component` 填 `basedata/material-brand`；`src/views/basedata/components` 下弹窗不会被动态路由匹配，因为该目录被作为局部组件目录使用。

3. 多页面高级表格组件放 `src/components/KhXxx/index.vue`；收货页专属弹窗放 `src/views/inbound/components`。

4. 检查控制器所在程序集是否属于 `.Modules.`，项目是否被 Server 引用，路由和控制器类型是否正确，`ApplicationPartManager` 是否能加载程序集。

## 继续阅读

- [学习路径](/learning-path)
- [培训资料下载](/training-materials)
- [架构总览](/backend/架构设计/KH.WMS架构总览)
