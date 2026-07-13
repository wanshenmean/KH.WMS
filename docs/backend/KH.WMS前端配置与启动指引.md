# KH.WMS 前端配置与启动指引

> 本文用于培训 KH.WMS.Client 的本地启动、构建、代理和基础配置。
> 配套主文档：[KH.WMS 前端开发指引](KH.WMS前端开发指引.md) — 体系化入口；本文是该专题的深入展开。

## 目录

- [1. 阅读路径](#1-阅读路径)
- [2. 项目入口](#2-项目入口)
- [3. 常用命令](#3-常用命令)
- [4. 本地环境检查流程](#4-本地环境检查流程)
- [5. Vite 配置](#5-vite-配置)
- [6. 后端代理](#6-后端代理)
- [7. Element Plus 自动导入](#7-element-plus-自动导入)
- [8. 自定义元素配置](#8-自定义元素配置)
- [9. 包管理约定](#9-包管理约定)
- [10. 常见错误](#10-常见错误)
- [11. 课后检查](#11-课后检查)

## 1. 阅读路径

1. 先读 `KH.WMS项目技术栈与目录指引.md` 的前端技术栈和快速启动。
2. 再读本文掌握 `vite.config.js`、`package.json` 和代理规则。

## 2. 项目入口

前端工程路径：

```text
KH.WMS.Client
```

关键文件：

| 文件 | 用途 |
| --- | --- |
| `package.json` | 依赖和脚本 |
| `vite.config.js` | Vite 插件、别名、代理、端口 |
| `src/main.js` | Vue 应用入口 |
| `src/App.vue` | 根组件 |
| `src/router/index.js` | 路由与权限守卫 |

## 3. 常用命令

```powershell
cd D:\Git\0.KH.WMS\KH.WMS.Client
npm install
npm run dev
npm run build
npm run preview
npm run test:e2e
```

| 命令 | 用途 |
| --- | --- |
| `npm install` | 安装依赖 |
| `npm run dev` | 启动 Vite 开发服务 |
| `npm run build` | 构建生产包 |
| `npm run preview` | 本地预览构建结果 |
| `npm run test:e2e` | 执行 Playwright 测试 |
| `npm run test:e2e:ui` | UI 模式执行 E2E |
| `npm run test:e2e:headed` | 有界面浏览器执行 E2E |

## 4. 本地环境检查流程

培训前先让成员完成以下检查，避免把环境问题误判为代码问题：

1. 确认 Node.js 和 npm 可用。
2. 确认位于 `KH.WMS.Client` 目录再执行 npm 命令。
3. 确认使用项目锁文件对应的 npm 安装依赖。
4. 确认后端 API 端口 `9291` 已启动。
5. 确认前端端口 `3000` 未被占用。
6. 启动后访问前端页面并打开浏览器开发者工具。
7. 查看 Network 中 `/api` 请求是否被代理到后端。
8. 登录后刷新页面，确认路由和权限可恢复。

建议培训时要求成员记录：

| 检查项 | 期望结果 |
| --- | --- |
| `npm install` | 无依赖安装错误 |
| `npm run dev` | Vite 正常启动并监听 `3000` |
| `/api` 请求 | 不是直接请求写死后端完整地址 |
| 登录后刷新 | 页面不丢菜单、不空白 |
| 控制台 | 无持续性红色错误 |

## 5. Vite 配置

配置文件：

```text
KH.WMS.Client/vite.config.js
```

当前关键配置：

| 配置 | 值 | 说明 |
| --- | --- | --- |
| `server.host` | `0.0.0.0` | 允许局域网访问 |
| `server.port` | `3000` | 前端开发端口 |
| `server.open` | `true` | 启动后自动打开浏览器 |
| `resolve.alias.@` | `src` | `@` 指向 `src` |

## 6. 后端代理

开发环境代理：

| 前端路径 | 后端目标 | 用途 |
| --- | --- | --- |
| `/api` | `http://localhost:9291` | HTTP API |
| `/ws` | `ws://localhost:9291` | WebSocket |

注意：

- 前端请求写 `/api/...`，不要写死 `http://localhost:9291`。
- WebSocket 路径走 `/ws`，代理会转到后端。
- 后端未启动时，前端页面会出现网络错误或 404/500。

代理排错顺序：

1. Network 中确认请求路径是否以 `/api` 开头。
2. 确认 Vite dev server 是否使用当前 `vite.config.js`。
3. 确认后端 `http://localhost:9291` 是否能打开 Swagger。
4. 确认后端 CORS、认证、接口路由是否正常。
5. WebSocket 问题单独看 `/ws` 握手是否成功。

## 7. Element Plus 自动导入

项目使用：

- `unplugin-auto-import`
- `unplugin-vue-components`
- `ElementPlusResolver`

作用：

- 常用 Element Plus API 自动导入。
- Element Plus 组件自动按需引入。
- 减少页面手写 import。

## 8. 自定义元素配置

`vite.config.js` 中把 `ElPopper`、`ElPopperContent` 标记为自定义元素，用于避免特定虚拟组件的指令挂载警告。

开发时不要随意删除该配置，除非确认 Element Plus 版本和相关组件行为已变化。

## 9. 包管理约定

当前项目有：

- `package.json`
- `package-lock.json`

培训约定：

- 默认使用 npm。
- 新增依赖前先确认团队包管理器。
- 不要混用 npm、pnpm、yarn 生成多个锁文件。
- 提交依赖变更时同步提交锁文件。

新增依赖前要说明三个问题：

| 问题 | 要求 |
| --- | --- |
| 为什么项目现有依赖不能满足 | 避免重复轮子 |
| 新依赖会进入哪些页面 | 评估影响范围 |
| 是否影响打包体积和浏览器兼容 | 评估上线风险 |

## 10. 常见错误

| 错误 | 现象 | 处理 |
| --- | --- | --- |
| 后端没启动 | 接口请求失败 | 先启动后端 `9291` |
| 端口 3000 被占用 | Vite 启动失败 | 关闭占用进程或改端口 |
| 请求写死后端地址 | 不同环境不可用 | 使用 `/api` 代理 |
| 依赖安装异常 | 启动失败 | 删除 `node_modules` 后重新 `npm install` |
| 混用包管理器 | 依赖树不一致 | 统一 npm 和 lock 文件 |

## 11. 课后检查

- 能启动前端开发服务。
- 能解释 `/api` 和 `/ws` 代理。
- 能知道在哪里改前端端口和别名。
- 能说明为什么不要写死后端地址。
- 能按顺序排查“页面能打开但接口不通”的问题。
