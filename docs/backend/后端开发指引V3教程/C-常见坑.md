# 附录 C 常见坑 教程

> 来源: KH.WMS后端开发指引 V3.0.md。本文把原章节单独抽出来，并补充“干什么、什么时候看、怎么执行”，用于新人培训和日常开发查阅。

## 这一章是干什么的

集中记录新人最容易踩的坑，用来快速排查 Swagger、DI、AOP、ExtData、Contract、事务、校验等问题。

## 什么时候需要看

现象已经出现，但不知道该回哪一章查原因时。

## 怎么执行

- 先按错误现象在常见坑里找关键词。
- 根据提示回到对应章节或配置点核对。
- 修复后重新执行构建、Swagger 或接口联调验证。

## 执行后怎么验证

问题能被复现、定位、修复，并用同一接口重新验证通过。

## 下一步看哪里

修复后把经验沉淀到对应章节或检查清单。

---

## 原章节内容

# 附录 C 常见坑

| 坑 | 现象 | 正确做法 |
| --- | --- | --- |
| Service 没写 `ServiceType` | Controller 注入接口失败或注册到非预期接口 | 显式写 `[RegisteredService(ServiceType = typeof(IXxxService))]` |
| 跨模块直接引用对方 `Services/` | 依赖越来越乱,改一个模块影响一片 | 通过 `KH.WMS.Contracts` 的 Contract 调用 |
| 把 `KH.WMS.Config` 当业务模块模仿 | 新模块扫描、引用边界混乱 | 业务模块使用 `.Modules.` 命名,配置层只当技术底座 |
| 实体放到模块项目里 | 其他模块和仓储层难复用 | 实体放 `KH.WMS.Entities` |
| 没有 `ExtData` 却继承 `ExtDataCrudController` | 动态字段语义混乱 | 普通表用 `CrudController` |
| 有 `ExtData` 却继承 `CrudController` | `extDataRaw` 保存/回显不符合前端预期 | 动态字段表用 `ExtDataCrudController` |
| 以为 `ExtDataCrudController` 会展开分页 | 分页字段和详情字段表现不一致 | 当前后端只覆盖详情展开,分页由前端 load 处理 |
| Controller 写复杂业务 | 难测试、难复用、事务散乱 | Controller 只调 Service |
| Contract 暴露整套 CRUD | 模块边界失控 | 只暴露跨模块必要业务能力 |
| 被调 Contract 自己开大事务 | 调用方无法保证整条链路一致 | 跨模块流程由调用方控制事务 |
| Validator 规则依赖事务内锁状态 | 校验和写入之间仍可能竞态 | 这类规则放 Service 事务内部 |
| Validator 实现多个接口 | 无拦截器注册分支可能注册到非预期接口 | Validator 类只实现 `IValidator` |
| 目标 Service 关闭拦截器 | `[ConfigValidation]` 不执行 | 需要配置校验的业务 Service 保持 AOP 默认开启 |
| 业务失败返回匿名对象 | TraceId 和统一响应不稳定 | 返回 `ApiResponse` / `ServiceResult` |
| 库存数量被业务模块直接改表 | 库存虚高/虚低、流水缺失 | 走 `IInventoryContract` |
| WCS/PDA 完成入口不做并发保护 | 重复完成、重复扣减、重复生成库存 | 使用锁、状态复核或幂等机制 |
| 字典里塞业务专属配置 | 字典膨胀,规则难维护 | 业务配置走配置底座能力 |
| 把启动配置和业务配置混用 | 改了配置但要重启,或运行期规则无法维护 | 启动配置放 `appsettings`,业务规则放 Config 配置库或系统参数 |
| Swagger 能看到但接口调用失败 | 误以为 Swagger 问题 | 区分文档发现和请求管道,继续查 License、认证、授权、业务校验 |
| Swagger 看不到新模块接口 | Controller 写好了但文档无接口 | 检查模块项目引用、程序集名 `.Modules.`、`ApplicationPartManager` 和构造函数依赖 |
| 手写 JSON 响应格式不统一 | 前端按 `data.code` / `data.message` 解析失败 | 中间件也保持 `ApiResponse`、camelCase 和正确 content-type |
| MiniProfiler 看不到方法链 | 只有 SQL,没有 Service 进入退出 | 确认 Service 通过 DI 代理调用,没有 `WithoutInterceptor=true` |
| 402 当成用户权限问题处理 | 换角色、换 token 都没用 | 402 是 License,按机器码、License 文件、有效期和开关排查 |
| 配了限流但没有 429 | `RateLimit` 配置存在但不生效 | 当前 `UseRateLimiting()` 默认注释,启用前还要确认配置被中间件读取 |
| 后台服务里直接依赖当前用户 | 定时任务审计字段为空或报错 | 后台任务没有 HTTP 用户上下文,用系统操作语义并允许用户为空 |
| 登录公钥长期缓存 | 服务重启后登录解密失败 | 登录前重新获取 `/api/user/public-key` |
| 密码明文或 RSA 密文入库 | 登录校验异常且存在安全风险 | 数据库只保存 `PasswordHasher.Hash` 生成的哈希 |
