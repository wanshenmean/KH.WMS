# KH.WMS 前端开发指引（重写版）— 设计文档

| 项 | 值 |
| --- | --- |
| 主题 | 重写 `docs/backend/KH.WMS前端开发指引.md`，使其独立成体系 |
| 撰写日期 | 2026-06-23 |
| 交付路径 | 原地覆盖 `docs/backend/KH.WMS前端开发指引.md`（方案 α） |
| 目标读者 | 前端新人入职（首要）、前后端联调工程师、有经验前端/架构师 |
| 配套文档 | 6 份前端专项文档（详见 §2） |

---

## 1. 背景与目标

### 1.1 现状问题

当前 `docs/backend/KH.WMS前端开发指引.md` 仅 219 行，11 节，更像"目录页"而非"教科书"。读者必须配合 6 份专项文档才能拼出前端全貌。

对比《`KH.WMS后端开发指引.md`》（1755 行，4 章+附录，独立成体系），前端入口文档**自包含程度**明显不足。

### 1.2 设计目标

重写后的 `KH.WMS前端开发指引.md` 应满足：

1. **独立成体系**：单读即可建立对 `KH.WMS.Client` 的完整认知并开始业务开发。
2. **深度对等后端**：预估 1700 行左右，与后端文档 1755 行相当。
3. **保留 6 份专项**：专项文档降级为"深入专题"，主文档不重复其深度细节。

### 1.3 不在范围内

- 不修改 `KH.WMS.Client/` 下任何源代码
- 不修改后端任何代码或文档
- 不删除任何现有专项文档
- 不创建 CI / 文档自动化脚本
- 不做 TS 迁移、性能优化、重构建议
- 不写 Vite 选型、Element Plus 二次封装设计哲学、Kh 组件设计动机

---

## 2. 与现有文档的边界

按"主文档自包含 70%，专项文档补 30% 深度"划分：

| 文档 | 新定位 |
| --- | --- |
| **`KH.WMS前端开发指引.md`**（重写） | **入口主文档**，自包含，覆盖架构+业务+约定+模块+附录 |
| `KH.WMS前端组件体系与页面开发指引.md` | **深入专题**：KhXxx 组件的 props/events/案例全集 |
| `KH.WMS前端状态管理与公共工具指引.md` | **深入专题**：Pinia store 设计模式 + utils 详解 |
| `KH.WMS前端请求封装与接口开发指引.md` | **深入专题**：axios/request 拦截器、token 刷新、Loading、错误码 |
| `KH.WMS前端路由菜单与权限开发指引.md` | **深入专题**：动态路由生成算法、菜单与权限码映射 |
| `KH.WMS前端配置与启动指引.md` | **深入专题**：vite.config.js、代理、环境变量 |
| `KH.WMS前端E2E测试与质量检查指引.md` | **深入专题**：Playwright 用法 + 用例设计 |
| `KH.WMS前后端联调与接口契约指引.md` | **前后端共有**，主文档附录 C 仅索引，不重复 |
| `KH.WMS项目技术栈与目录指引.md` | **总览**，主文档"阅读路径"指引读者先读它 |

**调整 6 份专项文档**：在各自的 `> 本文...` 头部加一行"配套主文档：`KH.WMS前端开发指引`"。不重写专项内容。

---

## 3. 章节骨架与篇幅

```
0  阅读路径与读者地图                  （~80 行）
1  从点击到渲染：一个用户操作的完整旅程  （~250 行，架构线 A）
2  前端项目分层与依赖                  （~200 行，8 层依赖图）
3  开发模板与约定                      （~450 行，最厚一章，A+B 双线交织）
4  业务域页面开发实战                  （~300 行，14 域速查 + brand + PdaReceiving 双案例）
附录 A 命令、构建、部署、环境变量        （~120 行）
附录 B E2E 测试与质量检查              （~80 行，仅索引）
附录 C 联调、接口契约、错误码还原        （~120 行）
附录 D 与后端架构对照表                （~60 行）
附录 E 常见坑点速查                    （~100 行）
```

预估总长 **~1700 行**。

---

## 4. 双线贯穿案例

### 4.1 双线定义

| 线 | 起点 | 终点 | 主要覆盖主题 |
| --- | --- | --- | --- |
| **架构线 A** | 点击登录按钮 | 看到首页（带动态菜单） | request 拦截器、JWT/Pinia、动态路由生成、菜单/权限码、Layout、dict 缓存、Loading |
| **业务线 B** | 在 `basedata` 下新增"品牌"页面 | 完成 CRUD（查询、新增、编辑、删除、权限） | KhTable/KhForm/KhDialog、API 文件、`useCrudApi`、`buildPageQuery`、字典、按钮权限、extData 字段 |
| **辅线 P（PDA）** | 仅在第 4 章出现 | — | PcLayout 与 PdaLayout 差异、扫码、`/api/inbound/receipt/confirm`、与后端 Contract 对齐 |

### 4.2 各章双线展开

#### 第 1 章｜从点击到渲染（架构线 A）

> 主题：从 `LoginView.vue` 的 `@click` 开始，跟踪"一个请求到一次渲染"走过的所有节点。

| 节点 | 真实文件 | 关键内容 |
| --- | --- | --- |
| 1.1 浏览器 → Vite 代理 → Kestrel | `vite.config.js`、`Program.cs` | 代理配置、CORS |
| 1.2 全局 axios 实例与拦截器 | `src/utils/request.js` | request/response 拦截、token 注入、401 刷新队列、Loading |
| 1.3 `/api/auth/login` 调用 | `src/api/auth.js` | 入参 RSA 加密 |
| 1.4 后端鉴权与 JWT 签发 | 后端 `SysUserService.LoginAsync` | 仅简注、不展开 |
| 1.5 token 写入 Pinia + localStorage | `src/stores/user.js` | setToken、解析 claims、router.replace |
| 1.6 `App.vue` + `<router-view>` | `App.vue`、`src/main.js` | 全局注册、Kh 组件、指令 |
| 1.7 路由守卫：`generateDynamicRoutes` | `src/router/index.js` | 调 `/api/permission/menu`、动态 `addRoute` |
| 1.8 后端菜单 → 前端 component 映射 | `src/router/menuConfig.js` + 后端菜单 `meta.component` | 路径映射规则、动态 import |
| 1.9 `PcLayout.vue` 渲染菜单、面包屑、字典缓存 | `src/layouts/PcLayout.vue` + `src/stores/dict.js` | 调 `/api/dict/all`、按业务类型缓存 |
| 1.10 首页仪表盘渲染 | `src/views/HomeView.vue` / `src/views/dashboard/WarehouseDashboard.vue` | KhStatCard、ECharts |
| 1.11 错误回到前端（含 TraceId） | `GlobalExceptionFilter` + 前端拦截 | TraceId 落日志、贴回排错 |

附一张 mermaid 序列图（与后端 Ch1 全景图对照）。

#### 第 2 章｜前端项目分层与依赖

> 主题：`src/` 的 8 个子目录各自的角色与依赖方向。

| 层 | 路径 | 角色 | 关键约定 |
| --- | --- | --- | --- |
| 入口层 | `main.js`、`App.vue` | 启动入口 | 手动注册 Kh 组件、`globalProperties` |
| 布局层 | `src/layouts/` | PcLayout / PdaLayout | PC 与 PDA 共用一组路由，仅外壳不同 |
| 路由层 | `src/router/` | 静态 + 动态路由 | 动态路由由后端菜单生成 |
| 状态层 | `src/stores/` | Pinia：user、permission、dict、app、websocket | 跨页面状态唯一落点 |
| API 层 | `src/api/` | 按业务域 | 每个域一个文件，`useCrudApi(module)` 通用 CRUD |
| 工具层 | `src/utils/` | request / crud / dict-resolve / useExtFields / websocket / mockData | 无业务依赖 |
| 视图层 | `src/views/{业务域}/` | 页面放业务域根目录，`components` 子目录放局部组件 | 路由扫描会排除 `**/components` |
| 通用组件层 | `src/components/KhXxx/` | 跨业务域复用 | Kh 全局组件命名约定 |

附 mermaid 依赖方向图；强调反向依赖禁止（视图不能反向 import 通用组件的内部文件）。

#### 第 3 章｜开发模板与约定（最厚一章，A+B 双线交织）

| 节 | 主题 | 架构线 A 举例 | 业务线 B 举例 |
| --- | --- | --- | --- |
| 3.1 | Vue 3 `<script setup>` 约定 | `LoginView.vue` | `basedata/brand.vue` |
| 3.2 | Kh 组件体系速查 | `KhLoading`（全局 Loading） | `KhTable` / `KhForm` / `KhDialog` |
| 3.3 | API 文件组织 | `src/api/auth.js` | `src/api/basedata.js` 中 `useCrudApi('material')` / `useCrudApi('brand')` |
| 3.4 | Pinia store 写法 | `useUserStore` / `useDictStore` | `usePermissionStore`（动态路由） |
| 3.5 | 路由与菜单 | `router/index.js` 守卫 + `addRoute` | 后端菜单 `meta.component: 'basedata/brand'` 的映射 |
| 3.6 | 权限控制 | `v-permission` 指令 | 品牌页按钮权限 `basedata:brand:create` |
| 3.7 | 字典与扩展字段 | 登录后 `dict.load()` 一次性拉取 | 品牌页"启用状态"字段从字典取 |
| 3.8 | 错误处理 | axios 拦截 + `$khMessage` / `$khNotify` | 业务错误（422 字段级）显示在表单字段下 |
| 3.9 | 调试与日志 | Vue DevTools + Pinia 插件 | `console.log` 与 `request.js` 的统一 Loading 控制 |
| 3.10 | 完整骨架（实战样例） | 一个完整的 `LoginView.vue` 骨架 | 一个完整的 `basedata/brand.vue` 骨架 |

#### 第 4 章｜业务域页面开发实战

| 节 | 主题 |
| --- | --- |
| 4.1 | 14 个业务域速查表（basedata / config / inbound / outbound / inventory / task / system / strategy / warehouse / report / dashboard / sorting / exception / pda） |
| 4.2 | 深度案例 B：**新增物料品牌页面** —— CRUD + 权限 + 字典 + 导入导出 |
| 4.3 | 深度案例 P：**PDA 收货上架** —— PdaLayout、扫码、`/api/inbound/receipt/confirm`、与后端 `IContainerContract` / `ITaskContract` 契约对齐 |
| 4.4 | 业务域间差异速查（基础资料 vs 业务流程 vs PDA vs 报表/大屏） |
| 4.5 | 新增页面标准落地清单 |

#### 附录（4 个附录）

- **附录 A**：常用命令（`npm install/dev/build/test:e2e`）、vite 代理配置、环境变量、生产构建
- **附录 B**：E2E 测试（仅索引到专项文档，列 4 个现有用例 `login.spec.js` / `task.spec.js` / `container-bind.spec.js` / `helpers.js` 的位置）
- **附录 C**：联调与错误码还原 —— 响应包 `{code, message, traceId}`；非 200 统一处理；字段级 422；TraceId 贴回后端日志的 `CorrelationId` 过滤
- **附录 D**：与后端架构对照表 —— 一张表对照"前端 X 概念 = 后端 Y 概念"
- **附录 E**：常见坑点速查表（10 条左右）

### 4.3 双线交织的 4 条硬规则

1. 第 1 章只走架构线 A，让新人先建立"一个请求怎么走通"的全局认知
2. 第 2 章是分层总览，不举具体例子，只画依赖图
3. 第 3 章每节都用 A+B 双线，每节末尾一句"完整骨架"指向 3.10 节的实战样例
4. 第 4 章是业务实战，B 线（brand）+ P 线（PdaReceiving）双案例深入，不在每个业务域都铺一遍

---

## 5. 写作约定

### 5.1 图表风格

- **mermaid**：序列图（Ch1）、flowchart 依赖图（Ch2）、状态图（Ch4）
- **表格**：响应码表、模块对照表、易错点表（与后端一致风格）
- **代码块**：所有 Vue 示例用 `<script setup>` + `<template>` 完整片段；JS 用 ES 模块；不用 TypeScript

### 5.2 代码示例来源

**只用真实文件**，不写假想的样板代码。每个代码片段顶部标 `文件路径:行号` 或 `文件路径`。代码过长时截关键片段 + 标注完整路径，让读者跳到 IDE 查完整代码。

### 5.3 术语与命名

| 用语 | 不允许 |
| --- | --- |
| "路由组件"、"动态路由菜单" | "菜单项"、"导航" |
| `useXxxStore` | "Pinia store"（首次出现时括注） |
| "业务域"、"视图模块" | 混用"模块"、"子系统"（与后端模块混） |
| `meta.component` | 含糊的"菜单配置字段" |

### 5.4 引用风格

- 代码文件：`src/views/basedata/brand.vue`
- 后端实体/接口：`MdMaterial` / `POST /api/material/create`
- 专项文档：`KH.WMS前端组件体系与页面开发指引.md §3`
- 后端文档：`《KH.WMS后端开发指引》第 1.6 节`

---

## 6. 文档头部元数据

```markdown
# KH.WMS 前端开发指引

> 适用版本：KH.WMS.Client 与后端对齐至 [最近的提交 hash]
> 目标读者：前端新人入职（首要）、前后端联调工程师、有经验前端/架构师
> 阅读路径：建议通读；按附录速查；深入专题请翻对应专项文档
> 与后端文档对照：见附录 D
> 配套专项文档：见 §2
```

---

## 7. 自检清单（写完主文档后过一遍）

| 检查项 | 怎么过 | 通过标准 |
| --- | --- | --- |
| 占位符扫描 | grep `TBD\|TODO\|XXX\|待补\|<占位>` | 0 命中 |
| 内部一致性 | 通读一遍：4 章之间引用是否对得上、A/B 线引用一致 | 0 矛盾 |
| 范围检查 | 与 §1.3 不在范围清单逐条比对 | 全部不越界 |
| 歧义检查 | 让一个新人读某段，让他复述"这一段在讲什么" | 复述与意图一致 |
| 路径有效 | 列出的所有 `src/...` 路径都用 Glob 验证存在 | 所有引用真实存在 |
| 接口路径 | 列出的所有 `/api/...` 路径与后端 Controller 对照 | 至少抽 5 个核对 |
| 与后端对照 | 附录 D 每一行都有后端文档对应引用 | 每行都能在《KH.WMS后端开发指引》中找到对应小节 |

发现问题直接改文档，不另起修订。

---

## 8. 执行级别（落地颗粒度）

### 8.1 落地阶段

| 阶段 | 交付物 | 自检点 |
| --- | --- | --- |
| 1 | 整体骨架（章节标题、章前导读、目录） | 章节顺序与 §3 一致 |
| 2 | 第 1 章（架构线 A，11 节点 + mermaid 全景图） | 11 节点齐全、mermaid 渲染正常 |
| 3 | 第 2 章（8 层依赖图 + 速查表） | mermaid 依赖方向不出现反向箭头 |
| 4 | 第 3 章（最厚，10 节，A+B 双线，10 个代码片段） | 每节都引用真实文件路径 |
| 5 | 第 4 章（14 域速查 + brand + PdaReceiving 深入） | 双案例走通 CRUD/扫码 + Contract 对齐 |
| 6 | 附录 A-E | 附录 D 至少 8 行对照 |
| 7 | 自检（§7 全部 7 项） | 全部通过 |
| 8 | 调整 6 份专项文档头部"配套主文档"标识 | 6 份文件都有该行 |

### 8.2 单章节内执行顺序

1. 章前导读（这一章解决什么问题，预期读者读完后能做什么）
2. 章节目录
3. 各节内容
4. 章末易错点（与后端风格一致）

### 8.3 实施边界

- ✅ 仅产出一份新版的 `KH.WMS前端开发指引.md`，按需 git commit
- ✅ 调整 6 份专项文档的"定位描述"（在 `> 本文...` 头部加一行"配套主文档：KH.WMS前端开发指引"），不重写
- ❌ 不修改源代码、不修改其他文档、不创建 CI 脚本

---

## 9. 收尾时间线

```
[当前] 设计文档已写入并自检
   ↓
[本会话] 把路径告诉用户，等用户审阅
   ↓
[用户批准后] 调用 superpowers 的 writing-plans 技能 → 实施计划
   ↓
[实施计划批准后] 按 §8.1 阶段逐章写新文档
   ↓
最终 git commit
```

---

## 10. 与后端文档的差异（明确写作风格不同之处）

| 项 | 后端文档 | 前端文档（重写版） |
| --- | --- | --- |
| 语言 | 中文 | 中文（一致） |
| 实体类名 | 直接用 `MdMaterial` | 不用（前端不直接接触实体） |
| 接口路径 | `/api/material/create` | 同样写法 |
| 代码语言 | C# | Vue + JS |
| 图表 | mermaid | mermaid（一致） |