---
title: "KH.WMS 后端开发指引 V3 教程目录"
description: "KH.WMS 后端开发指引 V3 教程目录：说明适用场景、当前实现、设计边界与开发或排障入口。"
status: current
audience: "后端开发人员与代码评审人员"
reviewed: "2026-07-14"
sourcePaths:
  - "KH.WMS/KH.WMS.Server"
  - "KH.WMS/KH.WMS.Core"
  - "KH.WMS/Modules"
---



# KH.WMS 后端开发指引 V3 教程目录

> 本目录由 KH.WMS后端开发指引 V3.0.md 拆分而来。每篇教程都先说明“这一章是干什么的、什么时候需要看、怎么执行、执行后怎么验证”，然后保留原章节正文。

## 怎么使用

- 第一次培训:按第 0 章到第 3 章读，先建立地图、启动机制和请求链路概念。
- 写普通 CRUD:读第 6 章、第 7 章、第 8 章，再按附录 B.1 检查。
- 写动态字段维护:读第 7.6 节所在教程、第 9 章，再按附录 B.2 检查。
- 写跨模块流程:读第 10 章、第 11 章，再按附录 B.3/B.5 检查。
- 排查联调问题:先看附录 C，再回到第 2、3、5、7 章定位。
- 查运行时底座:看 [后端底层概念专题索引](../后端底层概念/README.md),尤其是 Swagger、JSON、MiniProfiler、License、限流、后台服务和登录加密专题。

## 教程列表

- [第 0 章 这份文档怎么读 教程](00-这份文档怎么读.md)
- [第 1 章 KH.WMS 后端整体地图 教程](01-KH.WMS后端整体地图.md)
- [第 2 章 后端基础配置与启动机制 教程](02-后端基础配置与启动机制.md)
- [第 3 章 请求链路、事务、异常、TraceId、AOP 教程](03-请求链路事务异常TraceIdAOP.md)
- [第 4 章 Controller / Service / Entity / DTO / Contract 的职责边界 教程](04-职责边界.md)
- [第 5 章 服务自动注册: `[RegisteredService]` 教程](05-服务自动注册RegisteredService.md)
- [第 6 章 一个完整 CRUD 的底层执行链路 教程](06-完整CRUD底层执行链路.md)
- [第 7 章 CRUD 基类能力详解 教程](07-CRUD基类能力详解.md)
- [第 8 章 后端开发标准流程 教程](08-后端开发标准流程.md)
- [第 9 章 `CrudController<TEntity>` 与 `ExtDataCrudController<TEntity>` 怎么选 教程](09-CrudController与ExtDataCrudController怎么选.md)
- [第 10 章 跨模块 Contract 契约 教程](10-跨模块Contract契约.md)
- [第 11 章 业务流程、事务和校验扩展 教程](11-业务流程事务和校验扩展.md)
- [第 12 章 结合源码的后端深度走读](12-结合源码的后端深度走读.md)
- [附录 A 常用命令 教程](A-常用命令.md)
- [附录 B 开发检查清单 教程](B-开发检查清单.md)
- [附录 C 常见坑 教程](C-常见坑.md)

## 原始总文档

- [KH.WMS后端开发指引 V3.0.md](../KH.WMS后端开发指引%20V3.0.md)

## 底层概念专题

- [KH.WMS 后端底层概念专题索引](../后端底层概念/README.md)
- [Swagger 与 OpenAPI 接口文档底座](../后端底层概念/16-Swagger与OpenAPI接口文档底座.md)
- [MiniProfiler 性能观测底座](../后端底层概念/18-MiniProfiler性能观测底座.md)
- [License 授权许可与运行时拦截](../后端底层概念/19-License授权许可与运行时拦截.md)
- [限流 RateLimit 底座](../后端底层概念/20-限流RateLimit底座.md)



## 继续阅读

- [后端架构设计思路](/backend/架构设计/KH.WMS后端架构设计思路)
- [底层机制索引](/backend/后端底层概念/README)
