---
title: "KH.WMS 前端路由菜单与权限开发指引"
description: "KH.WMS 前端路由菜单与权限开发指引：说明适用场景、当前实现、设计边界与开发或排障入口。"
status: current
audience: "前端开发人员与联调负责人"
reviewed: "2026-07-14"
sourcePaths:
  - "KH.WMS.Client/src"
---



# KH.WMS 前端路由菜单与权限开发指引

> 本文用于培训前端动态路由、菜单、登录态和按钮权限。
> 配套主文档：[KH.WMS 前端开发指引](KH.WMS前端开发指引.md) — 体系化入口；本文是该专题的深入展开。

## 目录

- [1. 阅读路径](#1-阅读路径)
- [2. 关键代码入口](#2-关键代码入口)
- [3. 静态路由](#3-静态路由)
- [4. 动态路由匹配](#4-动态路由匹配)
- [5. 权限树转换](#5-权限树转换)
- [6. 菜单生成](#6-菜单生成)
- [7. 路由守卫](#7-路由守卫)
- [8. 按钮权限](#8-按钮权限)
- [9. 培训案例：新增菜单和按钮权限](#9-培训案例新增菜单和按钮权限)
- [10. 路由权限排查流程](#10-路由权限排查流程)
- [11. 常见错误](#11-常见错误)
- [12. 课后检查](#12-课后检查)

## 1. 阅读路径

1. 先读 `KH.WMS前端开发指引.md`。
2. 再读本文。
3. 涉及后端菜单配置时同步查 `KH.WMS前后端联调与接口契约指引.md`。

## 2. 关键代码入口

| 文件 | 用途 |
| --- | --- |
| `src/router/index.js` | 静态路由、动态路由、路由守卫 |
| `src/stores/permission.js` | 菜单、路由、按钮权限 |
| `src/stores/user.js` | 登录态、用户信息 |
| `src/directives/permission.js` | 按钮权限指令 |
| `src/layouts/PcLayout.vue` | PC 布局 |
| `src/layouts/PdaLayout.vue` | PDA 布局 |

## 3. 静态路由

静态路由包括：

- `/login`
- `/`
- `/home`
- `/pda/receiving`
- `/pda/putaway`
- `/pda/picking`
- `/pda/sorting`
- `/pda/count`

PC 页面挂在 `Layout` 子路由下。

PDA 页面挂在 `/pda` 布局下。

静态路由适合放登录、首页、PDA 固定入口等不依赖后端菜单生成的页面。业务菜单页面一般不要直接写成静态路由，否则会绕过菜单权限训练路径，也容易出现“页面能访问但菜单没有”的不一致。

## 4. 动态路由匹配

动态路由来自后端权限树。

后端菜单字段：

```text
component = "system/user"
```

前端匹配文件：

```text
/src/views/system/user.vue
```

匹配逻辑在 `resolveComponent(backendComponent)`。

注意：

- `src/views/**/components` 被排除，不会作为页面路由。
- 如果找不到真实页面，会使用 `PlaceholderView.vue`。
- 后端 `component` 字段不要带 `.vue`。

动态路由匹配时要同时检查三件事：

| 检查项 | 正确示例 |
| --- | --- |
| 后端 `component` | `system/user` |
| 前端页面文件 | `src/views/system/user.vue` |
| 前端访问路径 | 由后端 `path` 决定，例如 `/system/user` |

`component` 决定加载哪个 Vue 文件，`path` 决定浏览器地址，`permissionCode` 决定是否有路由权限。培训时要避免把三者混成一个字段。

## 5. 权限树转换

`permissionStore.fetchPermissions(roleId)` 完成：

- 保存原始权限树。
- 提取路由权限码。
- 提取按钮权限码。
- 构建动态菜单。
- 构建动态路由。
- 标记权限已加载。

关键字段：

| 字段 | 说明 |
| --- | --- |
| `permissionCode` | 菜单/路由权限码 |
| `permKey` | 按钮权限码 |
| `menuType` | 目录或菜单项 |
| `path` | 前端路由路径 |
| `component` | 前端页面组件路径 |
| `isVisible` | 是否显示 |
| `status` | 是否启用 |

## 6. 菜单生成

`buildMenuList(tree)` 将后端权限树转成前端菜单。

规则：

- 只显示 `isVisible === 1 && status === 1` 的节点。
- `menuType === 0` 作为目录。
- `menuType === 1` 作为页面菜单。
- 目录必须有子项。
- 菜单项必须有路径。

图标通过 `iconMap` 将后端 icon 名称转换为 Element Plus 图标名。

目录节点、菜单节点和按钮节点不要混用：

| 类型 | 职责 | 前端行为 |
| --- | --- | --- |
| 目录 | 组织菜单层级 | 生成菜单分组，一般不直接打开页面 |
| 菜单 | 打开业务页面 | 生成菜单项和动态路由 |
| 按钮 | 控制页面动作 | 控制新增、编辑、删除、审核等按钮 |

## 7. 路由守卫

路由守卫做以下事情：

1. 设置页面标题。
2. 判断是否公开页面。
3. 检查 token。
4. 刷新页面时重新获取用户信息。
5. 首次加载权限并注册动态路由。
6. 校验路由权限。
7. 无权限跳转 `/home`。

登录过期：

- 没有 token 访问受保护页面会跳 `/login`。
- Token 失效由请求拦截器处理强制登出。

## 8. 按钮权限

按钮权限来自后端权限树的 `buttons`。

前端通过：

- `permissionStore.hasButtonPermission(btnCode)`
- `src/directives/permission.js`

开发约定：

- 前端隐藏按钮只是体验优化。
- 后端仍必须校验关键操作权限。
- 按钮权限命名应和后端保持一致。

## 9. 培训案例：新增菜单和按钮权限

以“品牌资料”页面为例，完整配置链路如下：

| 项 | 示例 |
| --- | --- |
| 页面文件 | `src/views/basedata/brand.vue` |
| 菜单路径 | `/basedata/brand` |
| 后端 component | `basedata/brand` |
| 菜单权限码 | `basedata:brand:view` |
| 新增按钮 | `basedata:brand:create` |
| 编辑按钮 | `basedata:brand:update` |
| 删除按钮 | `basedata:brand:delete` |

前端页面开发时：

1. 页面操作按钮绑定权限码。
2. 菜单刷新后检查是否出现。
3. 直接访问 URL 检查路由守卫。
4. 删除某个按钮权限后刷新页面，确认按钮消失。
5. 使用接口直接调用关键操作，确认后端也会校验权限。

培训重点不是“按钮能不能隐藏”，而是成员要理解权限链路：后端权限树返回权限，前端 Store 保存权限，页面使用权限，后端接口再次兜底。

## 10. 路由权限排查流程

页面打不开时按以下顺序查：

1. 用户是否登录，`localStorage` 是否有 token。
2. 当前用户是否有角色和权限数据。
3. `permissionStore` 是否完成 `fetchPermissions`。
4. 后端权限树中菜单节点是否 `status === 1`。
5. 菜单节点是否 `isVisible === 1`。
6. 后端 `component` 是否能匹配 `src/views` 下的页面文件。
7. 页面是否被误放到 `components` 目录。
8. 动态路由是否已注册完成后再跳转。

常见现象和定位：

| 现象 | 优先检查 |
| --- | --- |
| 菜单没有 | 后端权限树、`isVisible`、目录 children |
| 菜单有但打开占位页 | `component` 和页面文件路径 |
| 刷新后空白 | 权限重新加载和动态路由注册 |
| 按钮不显示 | `permKey`、按钮权限集合、指令参数 |
| 直接 URL 能进但菜单没有 | 静态路由或权限判断漏掉 |

## 11. 常见错误

| 错误 | 现象 | 正确做法 |
| --- | --- | --- |
| 后端 component 写错 | 页面显示占位页 | 填 `业务域/页面名` |
| 页面放在 components 下 | 动态路由匹配不到 | 页面放业务域根目录 |
| 菜单目录没有子项 | 菜单不显示 | 确认 children |
| 只前端隐藏按钮 | 可通过接口绕过 | 后端也校验权限 |
| 刷新后路由丢失 | 页面 404 或空白 | 重新加载权限并注册动态路由 |

## 12. 课后检查

- 能解释后端 `component` 如何匹配前端页面。
- 能说明菜单和路由如何从权限树生成。
- 能判断按钮权限应该放在哪里配置。
- 能排查动态路由找不到页面的问题。
- 能独立配置一个菜单节点和三个按钮权限。



## 继续阅读

- [前端开发指引 V3.0](/backend/KH.WMS前端开发指引%20V3.0)
- [前端架构设计思路](/backend/架构设计/KH.WMS前端架构设计思路)
- [前后端联调与接口契约](/backend/KH.WMS前后端联调与接口契约指引)
