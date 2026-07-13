# KH.WMS 前端请求封装与接口开发指引

> 本文用于培训前端如何统一调用后端 API。
> 配套主文档：[KH.WMS 前端开发指引](KH.WMS前端开发指引.md) — 体系化入口；本文是该专题的深入展开。

## 目录

- [1. 阅读路径](#1-阅读路径)
- [2. 关键代码入口](#2-关键代码入口)
- [3. Axios 实例](#3-axios-实例)
- [4. 请求拦截器](#4-请求拦截器)
- [5. 响应拦截器](#5-响应拦截器)
- [6. Token 刷新](#6-token-刷新)
- [7. API 文件组织](#7-api-文件组织)
- [8. CRUD API 工具](#8-crud-api-工具)
- [9. 分页查询转换](#9-分页查询转换)
- [10. 培训案例：封装品牌资料接口](#10-培训案例封装品牌资料接口)
- [11. 接口排错流程](#11-接口排错流程)
- [12. 常见错误](#12-常见错误)
- [13. 课后检查](#13-课后检查)

## 1. 阅读路径

1. 先读 `KH.WMS前端配置与启动指引.md`。
2. 再读本文。
3. 涉及后端响应结构时查 `KH.WMS.Core-API-参考文档.md`。

## 2. 关键代码入口

| 文件 | 用途 |
| --- | --- |
| `src/utils/request.js` | Axios 实例、拦截器、Token 刷新 |
| `src/utils/crud.js` | CRUD API 工具和分页查询转换 |
| `src/api/*.js` | 各业务域 API 封装 |
| `src/stores/user.js` | token、refreshToken、用户信息 |

## 3. Axios 实例

请求实例配置：

- `baseURL: ''`
- `timeout: 15000`

开发约定：

- 请求路径写 `/api/...`。
- 不要在页面中直接创建新的 axios 实例。
- 不要绕过 `src/utils/request.js`。

统一实例的价值：

| 能力 | 如果绕过会怎样 |
| --- | --- |
| Token 请求头 | 接口 401 |
| 全局 Loading | 页面体验不一致 |
| 错误提示 | 每个页面重复处理 |
| Token 刷新 | 登录态无法恢复 |
| 响应结构 | 页面判断混乱 |

## 4. 请求拦截器

请求拦截器负责：

- 从 `localStorage` 读取 token。
- 添加 `Authorization: Bearer <token>`。
- 根据 `config.showLoading !== false` 控制全局 Loading。

`showLoading: false` 适合：

- 字典请求。
- 权限加载。
- 页面后台静默刷新。
- 高频轮询。

## 5. 响应拦截器

成功响应：

- Axios HTTP 2xx 时返回 `response.data`。

失败响应：

- 401：尝试刷新 Token。
- 403：提示无权限。
- 404：提示资源不存在。
- 500：提示服务器错误。
- 其他：提示后端 `message` 或默认失败文案。

页面中不要再重复写一套错误提示，除非是明确的业务交互，例如“库存不足时打开替代库位建议”。通用 HTTP 错误、登录过期、权限错误应交给拦截器。

## 6. Token 刷新

401 时流程：

```text
收到 401
  -> 检查 refreshToken
  -> 如果正在刷新，将请求加入 pendingRequests
  -> 调用 refreshToken 接口
  -> 更新 token 和 refreshToken
  -> 重放等待中的请求
  -> 刷新失败则强制登出
```

注意：

- `isRefreshing` 防止多个请求同时刷新 Token。
- `pendingRequests` 保存等待队列。
- 刷新 Token 使用动态 import，避免循环依赖。

培训时可以用两个并发请求模拟 401，观察只有一次 refresh 请求，其他请求等待刷新完成后重放。成员要理解这里解决的是“多个接口同时过期导致重复刷新”的问题。

## 7. API 文件组织

| 文件 | 业务域 |
| --- | --- |
| `src/api/auth.js` | 登录、刷新 Token |
| `src/api/user.js` | 用户信息、权限 |
| `src/api/system.js` | 系统管理、字典、参数 |
| `src/api/basedata.js` | 基础资料 |
| `src/api/warehouse.js` | 仓储基础 |
| `src/api/inbound.js` | 入库 |
| `src/api/inventory.js` | 库存 |
| `src/api/outbound.js` | 出库 |
| `src/api/task.js` | 任务 |
| `src/api/strategy.js` | 策略 |
| `src/api/adhoc.js` | 临时任务 |

新增接口时：

- 优先放到对应业务域 API 文件。
- 接口方法命名要能表达业务动作。
- 页面只调用 API 方法，不拼接口路径。

建议命名：

| 类型 | 命名示例 |
| --- | --- |
| 分页 | `getMaterialPage`、`getBrandPage` |
| 详情 | `getMaterialDetail` |
| 创建 | `createMaterial` |
| 更新 | `updateMaterial` |
| 删除 | `deleteMaterial` |
| 自定义动作 | `enableMaterial`、`submitInboundOrder`、`assignTask` |

## 8. CRUD API 工具

`useCrudApi(module)` 生成：

- `pageList(params)`
- `detail(id)`
- `create(data)`
- `update(data)`
- `delete(id)`
- `formConfig()`

对应后端：

- `/api/{module}/pagelist`
- `/api/{module}/{id}`
- `/api/{module}/create`
- `/api/{module}/update`
- `/api/{module}/delete/{id}`
- `/api/{module}/form-config`

CRUD 工具适合标准资料维护页面；业务动作复杂、需要提交/审核/分配/取消等动作时，应在 API 文件中显式封装方法，避免把业务语义藏在页面里。

## 9. 分页查询转换

`buildPageQuery(params)` 将前端扁平参数转为后端分页结构：

```js
{
  pageIndex,
  pageSize,
  sortConditions,
  filters
}
```

规则：

- 空值不传。
- select 默认 `equals`。
- input 默认 `contains`。
- 数组默认 `in`。
- 表头筛选合并到 filters。

分页查询培训重点：

| 前端输入 | 后端含义 |
| --- | --- |
| `pageIndex`、`pageSize` | 分页 |
| `sortConditions` | 排序字段和方向 |
| `filters` | 字段、操作符、值 |
| 空字符串 | 不应进入过滤条件 |
| 多选数组 | 通常转换为 `in` |

## 10. 培训案例：封装品牌资料接口

假设后端品牌路由为 `/api/basedata/brand`，前端建议这样落地：

```js
import request from '@/utils/request'
import { useCrudApi } from '@/utils/crud'

const brandCrud = useCrudApi('basedata/brand')

export const getBrandPage = brandCrud.pageList
export const getBrandDetail = brandCrud.detail
export const createBrand = brandCrud.create
export const updateBrand = brandCrud.update
export const deleteBrand = brandCrud.delete
export const getBrandFormConfig = brandCrud.formConfig

export function enableBrand(id) {
  return request.post(`/api/basedata/brand/${id}/enable`)
}
```

页面只 import 这些方法，不出现 `/api/basedata/brand` 字符串。这样路由变更、鉴权变更、Loading 策略变更都集中在 API 层和 request 层处理。

## 11. 接口排错流程

接口异常时按下面顺序看：

1. Network 请求路径是否正确，是否以 `/api` 开头。
2. 请求方法是否和后端 Controller 一致。
3. 请求头是否带 `Authorization`。
4. 请求体结构是否符合 DTO。
5. 分页查询是否使用 `buildPageQuery`。
6. 响应是否为 `ApiResponse` 结构。
7. 失败响应是否有 TraceId 或后端业务错误信息。
8. 是否触发了 Token 刷新和请求重放。

常见状态码：

| 状态码 | 前端处理方向 |
| --- | --- |
| 401 | 看 Token、refreshToken、登录态 |
| 403 | 看菜单/按钮/接口权限 |
| 404 | 看代理、路由、Controller 路径 |
| 422 | 看 DTO 校验、必填字段、字段类型 |
| 500 | 拿 TraceId 查后端日志 |

## 12. 常见错误

| 错误 | 后果 | 正确做法 |
| --- | --- | --- |
| 页面直接写 axios | 拦截器失效 | 走 API 文件 |
| 忘记 `showLoading: false` | 字典/权限加载闪屏 | 静默请求关闭 Loading |
| 401 时重复刷新 | 请求风暴 | 使用刷新队列 |
| 分页参数手写 | 后端 filters 不一致 | 使用 `buildPageQuery` |
| 接口路径硬编码到页面 | 难维护 | API 文件统一封装 |

## 13. 课后检查

- 能解释 Token 刷新队列。
- 能用 `useCrudApi` 对接一个 CRUD 页面。
- 能说明 `buildPageQuery` 的输出结构。
- 能判断哪些请求需要 `showLoading: false`。
- 能根据 Network 请求定位 401、403、404、422、500 的责任范围。
