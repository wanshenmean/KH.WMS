# KH.WMS 前端开发指引 · 加深实施计划

> 实施日期：2026-06-24
> 配套 spec：`docs/superpowers/specs/2026-06-24-frontend-guide-deepening-design.md`
> 目标文件：`docs/backend/KH.WMS前端开发指引.md`（主文档）
> 当前行数：1105 → 目标 1500-1800
> 13 批次串行执行，每批一 commit

## 全局约束

1. **唯一可修改的文件**：`docs/backend/KH.WMS前端开发指引.md`
2. **6 个专业文档绝对不修改**：
   - `docs/backend/KH.WMS前端组件体系与页面开发指引.md`
   - `docs/backend/KH.WMS前端状态管理与公共工具指引.md`
   - `docs/backend/KH.WMS前端请求封装与接口开发指引.md`
   - `docs/backend/KH.WMS前端路由菜单与权限开发指引.md`
   - `docs/backend/KH.WMS前端配置与启动指引.md`
   - `docs/backend/KH.WMS前端E2E测试与质量检查指引.md`
3. **章节标题不动**：4 章 + 5 附录的标题层级保持原样，仅 §0 在原目录下追加 §0.4-§0.5
4. **每节顶部加「本节以 X 线为例」**（X ∈ {A, B, P}）
5. **每节每个代码块后紧跟 3-5 行解读**（为什么 / 怎么工作 / 什么时候用 / 踩坑）
6. **现有「指回专业文档」全部保留**作为延伸阅读
7. **每批一个 commit**，commit 消息格式：`docs(frontend-guide): 加深 D{N} xxx（行数 X→Y）`
8. **不自动 push**

## Task 0：基线（已完成）

- 备份当前主文档行数：1105
- 备份专业文档 SHA：`git rev-parse HEAD:docs/backend/KH.WMS前端*.md`
- 设置验证脚本：每批完成后 `wc -l` 主文档

## Task 1：批次 D1 — §0 加 A/B/P 总谱

**输入**：
- 当前 §0（line 11-42）含 0.1 配套专项文档、0.2 读者地图、0.3 整体目录

**输出**：
- 在 §0.3 后追加：
  - **§0.4 贯穿案例总谱**（A/B/P 线定义 + 表格）
  - **§0.5 深入阅读路径**（基于 3 类读者 × 5 种工作场景的推荐）

**关键内容**（subagent 必写）：

```markdown
### 0.4 贯穿案例总谱（A/B/P 三线）

主文档有 3 条贯穿案例主线，标记在每节顶部：

| 线 | 全称 | 关注点 | 起点 | 终点 | 首次出现 |
| --- | --- | --- | --- | --- | --- |
| A 线 | 架构纵线 | 从点击到渲染 | 浏览器点击登录 | 首页渲染 | §1.1 |
| B 线 | 业务纵线 | 基于业务域开发 | 后端就绪 | 新页面 250 行可运行 | §3.1 |
| P 线 | PDA 纵线 | 手持端业务 | 扫描容器号 | 上架任务回写 | §4.3 |

A 线关心：axios、路由守卫、Pinia、Layout、Kh 组件、错误处理等横切基础设施。
B 线关心：API 封装、CRUD 范式、字典、权限、useCrudApi、新页面标准落地。
P 线关心：紧凑布局、扫码、实时反馈、错误重试、PdaLayout 特殊性。
```

**自检**：
- §0.4 表格存在
- §0.5 段落存在
- 主文档行数 +30~50

---

## Task 2：批次 D2 — §1.2 axios 拦截器深化

**输入**：
- 当前 §1.2（line 67-98）含简化版 axios 实例与拦截器

**输出**：
- 替换简化代码为真实代码（从 `src/utils/request.js` 读取）
- 每个代码块后加 3-5 行解读
- 关键解读点：
  - `baseURL: '/api'` 的代理机制解读
  - `axios.create` vs `axios.defaults` 选型
  - `loadingCount` 引用计数 vs 布尔值的优势
  - `pendingRequests` 队列的并发安全解读
  - 401 触发刷新的简化版（详细版在 D7）

**自检**：
- 引用代码与 `src/utils/request.js` 真实代码一致
- 解读段有"为什么 / 怎么工作 / 什么时候用 / 踩坑"四要素中的至少 3 个
- 主文档行数 +40~60

---

## Task 3：批次 D3 — §1.3-§1.4 登录 + 后端鉴权深化

**输入**：
- 当前 §1.3（line 100-118）含简化 login
- 当前 §1.4（line 120-139）含简化 LoginAsync 5 步

**输出**：
- §1.3：真实 RSA 加密流程（公钥从 `/api/auth/public-key` 拉，登录前先拉公钥）
- §1.4：后端 LoginAsync 5 步详细分解（查用户→校验密码哈希→查角色权限→签发 JWT→返回 token+user）
- §1.4.1：JWT 三段结构解读（header.payload.signature）
- §1.4.2：refreshToken 与 accessToken 双 token 机制

**自检**：
- 引用代码与 `src/api/auth.js` 和 `src/utils/rsa.js` 一致
- JWT 三段结构段落存在
- 主文档行数 +50~70

---

## Task 4：批次 D4 — §1.5-§1.6 token 写入 + main.js 深化

**输入**：
- 当前 §1.5（line 141-161）含简化 userStore.login
- 当前 §1.6（line 163-179）含简化 main.js

**输出**：
- §1.5：真实 userStore 完整字段表（state: token, refreshToken, userInfo, roles, permissions, loginTime）+ 完整 action 列表
- §1.6：main.js 启动顺序 7 步详细解读（创建 app → 注册 Pinia → 注册 Router → 注册 ElementPlus → 注册 Kh 组件 → 注册全局指令 → 挂载命令式方法）

**自检**：
- 引用代码与 `src/stores/user.js` 和 `src/main.js` 一致
- 启动顺序每步有 1-2 行解读
- 主文档行数 +50~70

---

## Task 5：批次 D5 — §1.7-§1.8 动态路由深化

**输入**：
- 当前 §1.7（line 181-198）含简化守卫
- 当前 §1.8（line 200-223）含简化 component 映射

**输出**：
- §1.7：真实 router.beforeEach 完整版（公开页判断、token 检查、刷新时重新获取用户信息、首次加载权限、动态路由注册、路由权限校验）
- §1.7.1：动态路由生成算法伪代码（`menuToRoute` 完整版）
- §1.8：`import.meta.glob('@/views/**/*.vue')` 机制解读
- §1.8.1：404 占位页（PlaceholderView.vue）逻辑
- §1.8.2：菜单三态（目录/菜单/按钮）规则

**自检**：
- 引用代码与 `src/router/index.js` 和 `src/router/menuConfig.js` 一致
- `menuToRoute` 伪代码完整
- 主文档行数 +60~80

---

## Task 6：批次 D6 — §1.9-§1.10 PcLayout + 首页深化

**输入**：
- 当前 §1.9（line 225-259）含简化 PcLayout + dictStore
- 当前 §1.10（line 261-271）含简化首页

**输出**：
- §1.9：真实 PcLayout 三栏布局（侧边栏菜单 + 顶栏面包屑 + 主区 + 标签页可选）
- §1.9.1：字典懒加载 vs 全量加载的取舍（dictStore.load() 一次性 vs dictStore.load(type) 按需）
- §1.10：KhStatCard 真实 props（title, value, icon, color, suffix, prefix）
- §1.10.1：ECharts 接入（`import * as echarts from 'echarts'` + `echarts.init(dom)` + `setOption`）

**自检**：
- 引用代码与 `src/layouts/PcLayout.vue` 和 `src/stores/dict.js` 一致
- 字典加载策略对比表存在
- 主文档行数 +50~70

---

## Task 7：批次 D7 — §1.11-§1.12 traceId + 错误处理矩阵

**输入**：
- 当前 §1.11（line 273-292）含简化 traceId
- 当前 §1.12（line 294-321）含时序图

**输出**：
- §1.11.1：完整 401 刷新队列伪代码（isRefreshing 标志 + pendingRequests 队列 + refreshToken 动态 import）
- §1.11.2：错误响应处理矩阵（401/403/404/422/429/500 六状态）
- §1.11.3：422 注入 KhForm 流程（data.fields → 表单字段错误）
- §1.12：时序图保持

**自检**：
- 401 刷新队列伪代码完整
- 错误处理矩阵 6 状态齐全
- 主文档行数 +60~80

---

## Task 8：批次 D8 — §3.1-§3.3 骨架 + API 深化

**输入**：
- 当前 §3.1（line 497-565）含简化骨架
- 当前 §3.2（line 567-586）含 5 组件速查
- 当前 §3.3（line 588-622）含简化 API 文件

**输出**：
- §3.1：真实 LoginView.vue 引用 + 真实 brand.vue 完整骨架（Vue 3 `<script setup>` 写法）
- §3.2：KhTable/KhForm/KhDialog 真实 props 速查（每个组件 props 列全列）
- §3.3：useCrudApi 内部实现（生成 pageList/detail/create/update/delete/formConfig 6 个方法 + 路径模板）
- §3.3.1：buildPageQuery 完整规则（空值不传、select equals、input contains、数组 in、表头筛选合并到 filters）

**自检**：
- 引用代码与 `src/views/LoginView.vue` 和 `src/utils/crud.js` 一致
- useCrudApi 内部实现伪代码完整
- buildPageQuery 规则表存在
- 主文档行数 +80~100

---

## Task 9：批次 D9 — §3.4-§3.5 5 个 store + 动态路由详解

**输入**：
- 当前 §3.4（line 624-663）含简化 store
- 当前 §3.5（line 665-685）含简化动态路由

**输出**：
- §3.4.1：5 个 store 完整字段表（user: token, refreshToken, userInfo, roles, permissions / permission: routesGenerated, menuTree, permissions / dict: items, loadedTypes / app: loading, loadingCount, sidebarCollapsed / websocket: connected, lastMessage）
- §3.4.2：Pinia 最佳实践（setup style vs options style / 持久化策略 / 重置策略）
- §3.5：动态路由生成算法详细版（fetchPermissions 流程：保存原始树 → 提取路由权限 → 提取按钮权限 → 构建菜单 → 构建动态路由 → 标记权限已加载）
- §3.5.1：菜单三态规则（menuType=0 目录 / menuType=1 菜单 / 按钮 permKey）

**自检**：
- 5 个 store 字段表完整
- 动态路由算法详细版存在
- 主文档行数 +60~80

---

## Task 10：批次 D10 — §3.6-§3.8 权限 + 字典 + 错误深化

**输入**：
- 当前 §3.6（line 687-710）含简化权限
- 当前 §3.7（line 712-742）含简化字典
- 当前 §3.8（line 744-760）含简化错误处理

**输出**：
- §3.6：v-permission 指令完整源码（接收字符串或数组、匹配 userStore.permissions、`*` 通配）
- §3.6.1：后端 ApiAuthorize 对照表（前/后端 6 项对照）
- §3.7.1：字典懒加载/全量/页面级三种策略对比
- §3.7.2：扩展字段（extData）完整流程（页面加载基础配置 → /form-config → 合并表单配置 → 提交时放 extData → 详情页同配置解析）
- §3.8：错误处理矩阵（6 状态 + 兜底）
- §3.8.1：422 字段级错误注入 KhForm 完整流程（data.fields 解析为 { fieldName: 'msg' } → KhForm 渲染）

**自检**：
- v-permission 源码完整
- 字典三种策略对比表存在
- 422 注入流程图存在
- 主文档行数 +70~90

---

## Task 11：批次 D11 — §3.9-§3.10 调试 + 完整骨架深化

**输入**：
- 当前 §3.9（line 762-776）含简化调试
- 当前 §3.10（line 778-806）含简化完整骨架

**输出**：
- §3.9.1：ECharts 接入（5 步）
- §3.9.2：浏览器调试技巧（Vue DevTools 组件树 + Pinia 标签 + 时间旅行 / Network 抓 traceId / Console 配合 console.log / console.table）
- §3.10.1：完整 brand.vue 250 行骨架（按真实结构占位，subagent 引用 §4.2 真实步骤）

**自检**：
- ECharts 5 步完整
- 调试技巧 4 维齐全
- 主文档行数 +30~50

---

## Task 12：批次 D12 — §4 业务域深解 + 附录 D 合并

**输入**：
- 当前 §4（line 808-973）含 14 域速查 + brand 案例 + PDA 案例 + 业务域差异 + 落地清单
- 当前附录 D（line 1069-1085）含 13 行后端对照

**输出**：
- §4.1 14 域速查保留，**追加 §4.1.1 业务域差异深解**：
  - inbound：状态机 DRAFT→RECEIVING→RECEIVED→BOUND，前端关键页面 order.vue / receiving.vue
  - outbound：状态机 + 波次，前端关键页面 order.vue / wave.vue
  - inventory：状态机 + 调整 / 盘点，前端关键页面 stock-query / adjust / stocktake
  - task：状态机 + 确认，前端关键页面 list.vue / assign.vue
  - system：CRUD 范式 + 权限配置
  - warehouse：CRUD 范式 + 树形结构
- §4.2 / §4.3 / §4.4 / §4.5 保留并小幅增强
- **附录 D 删除整节**，其内容合并到正文：
  - axios 中间件 → §1.2
  - Jwt 中间件 → §1.4
  - 动态路由 → §1.7-§1.8
  - v-permission / ApiAuthorize → §3.6
  - useCrudApi / ExtDataCrudController → §3.3
  - KhForm / ValidationException → §3.8
  - KhTable / ICrudService → §3.4 + §3.3
  - useDictStore / SysDictService → §3.7
  - $khNotify / ApiResponse.Fail → §1.11
  - dict-resolve / 字典翻译 → §3.7
  - WebSocket / KH.WMS.Engines → §1.9
  - PdaLayout / KhPage → §2.3.2

**自检**：
- 4 个业务域深解（inbound/outbound/inventory/task）每域 1 段
- 附录 D 已删除
- 附录 E 保留
- 主文档行数 +60~80

---

## Task 13：批次 D13 — 最终自检

**输入**：D1-D12 全部完成后的主文档

**输出**：自检报告 `D:\Git\0.KH.WMS\.superpowers\sdd\deepen-task-13-report.md`

**自检 7 项**：
1. 全文行数 `wc -l docs/backend/KH.WMS前端开发指引.md` ≥ 1500 且 ≤ 1800
2. 占位符扫描 `grep -nE 'TBD|TODO|XXX|待补|<占位>' docs/backend/KH.WMS前端开发指引.md` 无输出
3. 引用文件存在（21 个核心文件 Read 校验）
4. 接口路径抽样 5 个（/api/auth/login /api/permission/menu /api/dict/all /api/inbound/receipt）
5. 附录 D 已删除（grep 验证），附录 E 保留
6. §0.4 A/B/P 总谱存在
7. §1 §3 各节"代码 + 3-5 行解读"覆盖率抽样验证（随机抽 5 节确认）

**同时验证 6 个专业文档无修改**：
- `git diff HEAD~13 HEAD -- docs/backend/KH.WMS前端组件体系与页面开发指引.md` 应为空
- 同上 5 个专业文档全部为空

**最终交付**：
- 13 个 commit（每个 D 批次 1 个）
- 主文档行数 1500-1800
- 6 个专业文档 0 改动

---

## 提交协议

每批 subagent 完成后：
1. self-review
2. git add 主文档
3. git commit -m "docs(frontend-guide): 加深 D{N} xxx（行数 {X}→{Y}）"
4. 上报简短状态（status + commit SHA + 行数变化 + 自检结果）

每 2 批后由 reviewer 抽样验证真实性：
- 抽 1 个批次读 git diff
- 验证引用代码与 src/xxx/xxx.js 一致
- 验证解读段存在且符合"为什么 / 怎么工作 / 什么时候用 / 踩坑"四要素
- 验证章节标题未被改动

## 风险与回退

| 风险 | 回退策略 |
| --- | --- |
| 某批代码引用与真实不符 | reviewer 标记，subagent 重做该批 |
| 行数超 1800 | 精简解读段，删除冗余代码引用 |
| 行数不足 1500 | 追加 D1 提到的"关键决策点解读" |
| 改坏专业文档 | reviewer 立即停止，回退该批 |
| 6/24 spec 与 6/23 plan 冲突 | 6/23 是基线，6/24 在其上叠加，无冲突 |

## 进度跟踪

在 `D:\Git\0.KH.WMS\.superpowers\sdd\deepen-progress.md` 维护进度：
- 13 个批次的 commit SHA 和行数变化
- reviewer 抽样验证记录
- 任何 blocker
