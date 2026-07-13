# KH.WMS 前端 E2E 测试与质量检查指引

> 本文用于培训前端 E2E 测试、构建验证和页面质量检查。
> 配套主文档：[KH.WMS 前端开发指引](KH.WMS前端开发指引.md) — 体系化入口；本文是该专题的深入展开。

## 目录

- [1. 关键入口](#1-关键入口)
- [2. 测试命令](#2-测试命令)
- [3. 写测试前的准备](#3-写测试前的准备)
- [4. 建议测试场景](#4-建议测试场景)
- [5. 新增测试文件命名](#5-新增测试文件命名)
- [6. 测试数据策略](#6-测试数据策略)
- [7. E2E 编写模板](#7-e2e-编写模板)
- [8. 手工质量检查](#8-手工质量检查)
- [9. 常见错误](#9-常见错误)
- [10. 课后检查](#10-课后检查)

## 1. 关键入口

| 文件/目录 | 用途 |
| --- | --- |
| `KH.WMS.Client/playwright.config.js` | Playwright 配置 |
| `KH.WMS.Client/e2e` | E2E 测试目录 |
| `e2e/helpers.js` | 测试辅助方法 |
| `e2e/login.spec.js` | 登录测试 |
| `e2e/task.spec.js` | 任务测试 |
| `e2e/container-bind.spec.js` | 容器绑定测试 |

## 2. 测试命令

```powershell
cd D:\Git\0.KH.WMS\KH.WMS.Client
npm run test:e2e
npm run test:e2e:ui
npm run test:e2e:headed
```

| 命令 | 用途 |
| --- | --- |
| `npm run test:e2e` | 无界面执行全部测试 |
| `npm run test:e2e:ui` | UI 模式调试测试 |
| `npm run test:e2e:headed` | 有浏览器界面执行 |

## 3. 写测试前的准备

- 后端服务可用。
- 前端服务可用。
- 测试账号可登录。
- 测试数据稳定或可在测试中创建。
- 页面路由和权限配置完成。

培训时先确认测试目标：E2E 不是替代所有接口测试，它主要验证用户在浏览器中的关键路径是否能完成，包括路由、权限、请求、页面状态和提示是否串起来。

## 4. 建议测试场景

基础页面：

- 页面能打开。
- 列表能加载。
- 搜索能返回结果。
- 新增、编辑、删除能完成。
- 表单校验有效。

业务流程：

- 登录。
- 创建单据。
- 执行业务动作。
- 状态变化正确。
- 刷新后状态仍正确。

权限：

- 无权限菜单不显示。
- 无权限按钮不显示。
- 直接访问无权限页面会跳转或拒绝。

异常：

- 后端失败时前端提示正确。
- 空数据状态正确。
- Token 过期后能跳转登录。

每个新页面至少覆盖：

| 页面类型 | 最低覆盖 |
| --- | --- |
| 基础资料 | 打开页面、查询、新增、编辑、删除、表单校验 |
| 流程页面 | 打开页面、执行业务动作、状态变化、失败提示 |
| PDA 页面 | 扫码输入、确认动作、异常提示、返回路径 |
| 报表页面 | 查询、空数据、图表或表格渲染 |
| 权限页面 | 无权限菜单/按钮不可见 |

## 5. 新增测试文件命名

推荐：

- `material.spec.js`
- `warehouse-location.spec.js`
- `inbound-order.spec.js`
- `outbound-order.spec.js`
- `inventory-stock.spec.js`

命名原则：

- 业务域 + 页面/流程。
- 不要把大量无关流程塞进一个 spec。
- 可复用逻辑放 `helpers.js`。

## 6. 测试数据策略

稳定测试数据优先级：

1. 测试环境预置数据。
2. 测试开始时通过 API 创建数据。
3. 测试结束时清理数据。
4. 必须依赖人工数据时，在文档中写清楚数据要求。

不要依赖：

- 某个人本机数据库里刚好存在的数据。
- 会被业务人员随时修改的生产样例数据。
- 只按名称查找但名称可能重复的数据。

建议测试数据命名带前缀，例如 `E2E_品牌_时间戳`，便于排查和清理。

## 7. E2E 编写模板

新增页面测试可以按这个结构写：

```js
import { test, expect } from '@playwright/test'
import { login } from './helpers'

test.describe('品牌资料', () => {
  test.beforeEach(async ({ page }) => {
    await login(page)
    await page.goto('/basedata/brand')
  })

  test('can search and open create dialog', async ({ page }) => {
    await expect(page.getByText('品牌')).toBeVisible()
    await page.getByRole('button', { name: '新增' }).click()
    await expect(page.getByRole('dialog')).toBeVisible()
  })
})
```

编写时优先使用可访问选择器：

- `getByRole`
- `getByLabel`
- `getByText`
- 稳定的测试标识

少用脆弱选择器，例如深层 CSS 路径和自动生成 class。

## 8. 手工质量检查

即使没有 E2E，也应检查：

- `npm run build` 成功。
- 页面无控制台报错。
- 文案不溢出。
- 表格、弹窗、按钮状态正常。
- Loading、空数据、错误状态正常。
- 权限按钮控制正常。
- 刷新页面动态路由正常恢复。

手工检查顺序建议：

1. `npm run build`。
2. 登录进入页面。
3. 主流程操作。
4. 异常输入和后端失败提示。
5. 刷新页面。
6. 切换无权限账号。
7. 浏览器控制台和 Network 检查。

## 9. 常见错误

| 错误 | 现象 | 正确做法 |
| --- | --- | --- |
| 测试依赖本地偶然数据 | CI 或他人环境失败 | 准备稳定测试数据 |
| 所有流程写一个测试 | 难定位失败 | 按业务拆分 spec |
| 只测成功路径 | 异常时页面崩 | 补失败和空数据场景 |
| 不复用登录 helper | 测试重复 | 抽到 `helpers.js` |
| 不跑 build | 运行时才发现构建错误 | 完成前跑 `npm run build` |

## 10. 课后检查

- 能运行 Playwright 测试。
- 能新增一个页面基础 E2E。
- 能说明 E2E 和手工检查各自覆盖什么。
- 能判断新增页面至少需要哪些验收场景。
- 能设计稳定测试数据，避免测试依赖偶然环境。
