# ConfigModule 物理拆分独立项目任务清单

> 创建时间：2026-06-29
> 前置条件：阶段一（接口解耦）、阶段二（Abstractions 项目拆分）已完成
> 当前状态：`KH.WMS.Config.Abstractions` 项目已建立，7个抽象文件已迁移，编译通过

---

## 目标

将 `KH.WMS.Modules.ConfigModule` 的实现代码迁移到独立项目 `KH.WMS.Config`，与 `KH.WMS.Config.Abstractions` 一起作为可独立打包的配置层，实现"固化配置层、防止业务方随意修改"的架构治理目标。

---

## 任务拆解

### 步骤 1：创建 KH.WMS.Config 实现项目

- [ ] 在 `Config/` 下创建 `KH.WMS.Config` 类库项目（net8.0）
- [ ] 配置 csproj：
  - 引用 `KH.WMS.Config.Abstractions`
  - 引用 `KH.WMS.Core`、`KH.WMS.Entities`
  - 引用 `KH.WMS.Contracts`（实现契约）
  - 配置 NuGet 打包元数据（PackageId=KH.WMS.Config）
- [ ] 加入解决方案 `KH.WMS.sln`
- [ ] 加入解决方案文件夹"Config"虚拟目录

### 步骤 2：迁移源码文件

从 `Modules/ConfigModule/KH.WMS.Modules.ConfigModule/` 迁移以下目录到 `Config/KH.WMS.Config/`：

- [ ] `Contracts/` —— 4个契约实现（ConfigResolverContract、DefaultConfigScopeResolver、DocumentStatusValidatorContract、CfgExtFieldContract、CfgDocumentFieldExtContract）
- [ ] `Controllers/` —— 15个 Controller
- [ ] `Services/` —— 18对 Service 实现
- [ ] `Interfaces/` —— 18个 IService 接口
- [ ] `DTOs/` —— GlobalConfigDto 等
- [ ] `Docs/` —— 模块文档（如有）

### 步骤 3：调整项目引用

- [ ] `KH.WMS.Server` 项目：将 `KH.WMS.Modules.ConfigModule` 引用改为 `KH.WMS.Config`
- [ ] 移除原 `KH.WMS.Modules.ConfigModule` 项目引用（保留文件但不再被解决方案引用，或直接删除）
- [ ] 检查其他模块（InboundModule/OutboundModule 等）是否直接引用了 ConfigModule，如有则改为引用 `KH.WMS.Config.Abstractions`（仅用接口）或 `KH.WMS.Contracts`

### 步骤 4：调整命名空间

- [ ] 全局替换 `namespace KH.WMS.Modules.ConfigModule` → `namespace KH.WMS.Config`
- [ ] 更新 `using KH.WMS.Modules.ConfigModule.xxx` → `using KH.WMS.Config.xxx`
- [ ] 检查 DI 自动注册：`AssemblyService.GetReferencedAssemblies()` 是否需要显式加入新程序集

### 步骤 5：调整 DI 注册

- [ ] 检查 `ServiceRegistrar` 的程序集扫描逻辑，确认 `KH.WMS.Config.dll` 能被自动扫描到
- [ ] 检查 `Program.cs` 中的模块程序集加载（`moduleAssemblies`），替换 ConfigModule 程序集
- [ ] 检查 Controller 自动发现（`ApplicationPart`），确认新程序集的 Controller 能被发现

### 步骤 6：编译验证

- [ ] `dotnet build KH.WMS.sln` —— 0 Error
- [ ] 检查所有 warning，确认无新增引用问题

### 步骤 7：运行验证

- [ ] 启动后端服务，确认配置库初始化正常
- [ ] 访问配置相关 API（`/api/global-config` 等），确认响应正常
- [ ] 访问依赖配置的业务功能（入库校验等），确认 `IConfigResolverContract` 注入正常

### 步骤 8：清理

- [ ] 删除原 `Modules/ConfigModule` 目录（确认无残留引用）
- [ ] 更新 `KH.WMS.sln`，移除旧项目节点
- [ ] 提交代码

---

## 风险点

| 风险 | 影响 | 应对 |
|------|------|------|
| 程序集扫描遗漏 | DI 注册失败，配置服务无法注入 | 步骤5 重点验证 `AssemblyService` 扫描逻辑 |
| Controller 发现遗漏 | API 404 | 步骤5 验证 `ApplicationPart` 配置 |
| 命名空间替换遗漏 | 编译失败 | 步骤4 全局搜索，逐个确认 |
| 其他模块直接引用了 ConfigModule 的具体类 | 编译失败或运行时类型加载失败 | 步骤3 检查所有 csproj 引用 |

---

## 验收标准

1. `KH.WMS.Config` 和 `KH.WMS.Config.Abstractions` 是两个独立项目
2. 解决方案编译 0 Error
3. 后端服务正常启动，配置 API 正常响应
4. 业务功能（入库/出库/库存）正常，配置校验生效
5. 原 `KH.WMS.Modules.ConfigModule` 已删除或不再被任何项目引用
