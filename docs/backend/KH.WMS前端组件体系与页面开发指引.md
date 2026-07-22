---
title: "KH.WMS 前端组件体系与页面开发指引"
description: "KH.WMS 前端组件体系与页面开发指引：说明适用场景、当前实现、设计边界与开发或排障入口。"
status: current
audience: "前端开发人员与联调负责人"
reviewed: "2026-07-14"
sourcePaths:
  - "KH.WMS.Client/src"
---



# KH.WMS 前端组件体系与页面开发指引

> 本文用于培训通用组件、页面组件和典型页面结构。
> 配套主文档：[KH.WMS 前端开发指引](KH.WMS前端开发指引.md) — 体系化入口；本文是该专题的深入展开。

## 目录

- [1. 组件目录](#1-组件目录)
- [2. 常用通用组件](#2-常用通用组件)
- [3. 组件提升规则](#3-组件提升规则)
- [4. 典型页面结构](#4-典型页面结构)
- [5. 页面局部组件命名](#5-页面局部组件命名)
- [6. 与 API 的关系](#6-与-api-的关系)
- [7. 培训案例：拆分基础资料页面](#7-培训案例拆分基础资料页面)
- [8. 组件评审清单](#8-组件评审清单)
- [9. 常见错误](#9-常见错误)
- [10. 课后检查](#10-课后检查)

## 1. 组件目录

通用组件位置：

```text
KH.WMS.Client/src/components
```

组织方式：

```text
KhXxx/index.vue
```

页面局部组件位置：

```text
KH.WMS.Client/src/views/{业务域}/components
```

## 2. 常用通用组件

| 组件 | 定位 |
| --- | --- |
| `KhTable` | 表格、分页、搜索、列表 |
| `KhForm` | 通用表单 |
| `KhDialog` | 通用弹窗 |
| `KhDetailDialog` | 详情弹窗 |
| `KhPage` | 页面容器 |
| `KhPageHeader` | 页面标题/操作区 |
| `KhUpload` | 上传 |
| `KhMessage` | 消息提示 |
| `KhNotify` | 通知 |
| `KhStatCard` | 统计卡片 |
| `KhDashboard` | 看板布局 |
| `KhEditableTable` | 可编辑表格 |
| `KhTransfer` | 穿梭选择 |

通用组件培训时要讲“定位”，不要只讲名字。成员需要知道什么时候应该复用 `KhTable`，什么时候只是普通 `el-table` 更合适；什么时候用 `KhDialog`，什么时候页面内区域就足够。

主从页面不能只靠 `KhEditableTable` 完成。第一次配置主列表、主表表单、可编辑明细和只读详情时，请使用[KH.WMS 主从表页面配置与开发实战指引](/backend/KH.WMS主从表页面配置与开发实战指引)。

## 3. 组件提升规则

先局部，后通用：

1. 页面第一次使用，放页面同级或 `views/{业务域}/components`。
2. 同业务域多个页面复用，保留在业务域 components。
3. 多个业务域复用，再提升到 `src/components/KhXxx`。

不建议：

- 一开始就把所有弹窗放到全局组件。
- 业务强绑定组件命名为 `KhXxx`。
- 通用组件内部写死业务接口。

提升为通用组件前至少满足：

| 条件 | 说明 |
| --- | --- |
| 两个以上业务域需要 | 只在一个业务域内复用先放局部 |
| Props 和事件稳定 | 频繁变更说明抽象还没成型 |
| 不依赖具体 API | 数据由外部传入或通过事件请求 |
| 样式能融入全局 | 不绑定某个页面特殊布局 |
| 有清楚边界 | 组件只解决一个明确问题 |

## 4. 典型页面结构

基础资料页面通常包含：

- `KhPage` / 页面容器。
- 搜索条件。
- `KhTable`。
- 新增/编辑弹窗。
- 删除确认。
- 导入导出。

业务流程页面通常包含：

- 状态筛选。
- 列表。
- 业务操作按钮。
- 详情或确认弹窗。
- 操作前状态校验。

PDA 页面通常包含：

- 大按钮。
- 扫码输入。
- 当前任务信息。
- 确认动作。
- 简洁错误提示。

报表页面通常包含：

- 查询条件。
- 图表。
- 汇总指标。
- 明细表格。
- 空数据状态。

## 5. 页面局部组件命名

推荐命名：

- `MaterialFormDialog.vue`
- `ContainerBindDialog.vue`
- `AssignTaskDialog.vue`
- `TransferDetailDialog.vue`
- `OrderFormDialog.vue`

命名原则：

- 业务对象 + 用途 + 组件类型。
- 弹窗用 `Dialog` 后缀。
- 表单用 `Form` 或 `FormDialog`。
- 详情用 `DetailDialog`。

## 6. 与 API 的关系

页面组件可以调用 API，但建议：

- 页面主组件负责加载列表和总体状态。
- 弹窗组件负责表单提交。
- 通用组件不直接调用业务 API。
- API 方法从 `src/api/{module}.js` 引入。

组件交互建议：

| 场景 | 建议方式 |
| --- | --- |
| 父页面控制弹窗开关 | `v-model` 或显式 props |
| 弹窗提交成功 | `emit('success')` 通知父页面刷新 |
| 表格选择行 | `emit('selection-change')` |
| 通用组件需要数据 | 通过 props 传入，不直接请求业务接口 |
| 复杂表单校验 | 组件内部校验，业务规则由父页面传入 |

## 7. 培训案例：拆分基础资料页面

以物料资料页面为例，可以拆成：

```text
src/views/basedata/material.vue
src/views/basedata/components/MaterialFormDialog.vue
src/views/basedata/components/MaterialDetailDialog.vue
src/views/basedata/components/MaterialImportDialog.vue
```

职责建议：

| 文件 | 职责 |
| --- | --- |
| `material.vue` | 查询条件、列表、分页、按钮权限、弹窗开关 |
| `MaterialFormDialog.vue` | 新增/编辑表单、表单校验、提交 |
| `MaterialDetailDialog.vue` | 只读详情展示 |
| `MaterialImportDialog.vue` | 导入上传和导入结果 |

培训演练时让成员先拆组件职责，再写代码。能拆清楚职责，后面的 API、权限、校验才不会全部塞进一个大页面。

## 8. 组件评审清单

评审一个页面组件时检查：

- 组件是否有单一职责。
- 业务组件是否放在正确业务域目录。
- 通用组件是否没有写死业务接口。
- 组件通信是否通过 props/events，而不是互相改内部状态。
- 弹窗关闭、提交成功、提交失败是否都有明确状态。
- Loading、空数据、错误提示是否完整。
- 文案是否溢出，按钮是否在小屏下错位。

## 9. 常见错误

| 错误 | 后果 | 正确做法 |
| --- | --- | --- |
| 通用组件写业务接口 | 复用困难 | 通过 props/events 交互 |
| 弹窗组件直接操作父页面大量状态 | 耦合严重 | emit 事件通知父组件 |
| 所有组件都放全局 | components 混乱 | 局部优先 |
| 页面标题、按钮、表格风格不统一 | 用户体验割裂 | 复用 KhPage/KhTable |
| 表格空数据无处理 | 页面像异常 | 提供空状态 |

## 10. 课后检查

- 能判断组件该放全局还是局部。
- 能说出基础资料页面的典型结构。
- 能按命名规则新增弹窗组件。
- 能说明通用组件为什么不应直接调用业务 API。
- 能把一个复杂页面拆成主页面、表单弹窗、详情弹窗。



## 继续阅读

- [前端开发指引 V3.0](/backend/KH.WMS前端开发指引%20V3.0)
- [前端架构设计思路](/backend/架构设计/KH.WMS前端架构设计思路)
- [前后端联调与接口契约](/backend/KH.WMS前后端联调与接口契约指引)
