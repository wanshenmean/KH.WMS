---
title: "附录 A 常用命令 教程"
description: "附录 A 常用命令 教程：说明适用场景、当前实现、设计边界与开发或排障入口。"
status: reference
audience: "后端开发人员与代码评审人员"
reviewed: "2026-07-14"
sourcePaths:
  - "KH.WMS/KH.WMS.Server"
  - "KH.WMS/KH.WMS.Core"
  - "KH.WMS/Modules"
---



# 附录 A 常用命令 教程

> 来源: KH.WMS后端开发指引 V3.0.md。本文把原章节单独抽出来，并补充“干什么、什么时候看、怎么执行”，用于新人培训和日常开发查阅。

## 这一章是干什么的

汇总后端开发、构建、启动、测试、联调常用命令，方便新人直接执行。

## 什么时候需要看

不知道在终端里运行什么命令，或培训时需要统一本地操作步骤。

## 怎么执行

- 进入对应项目目录。
- 按附录命令执行构建、启动或验证。
- 执行失败时记录完整命令、当前目录和错误输出。

## 执行后怎么验证

命令能在正确目录执行，并得到预期的构建、启动或测试结果。

## 下一步看哪里

命令执行后按附录 B 做开发检查，遇到问题看附录 C。

---


| 场景 | 命令 / 地址 |
| --- | --- |
| 进入后端目录 | `cd D:\Git\0.KH.WMS\KH.WMS` |
| 还原依赖 | `dotnet restore KH.WMS.sln` |
| 编译 | `dotnet build KH.WMS.sln` |
| 启动后端 | `dotnet run --project KH.WMS.Server\KH.WMS.Server.csproj` |
| Swagger | `http://localhost:9291/swagger` |
| MiniProfiler | `http://localhost:9291/profiler` |

排查命令:

| 场景 | 命令 |
| --- | --- |
| 查找注册特性 | `rg -n "RegisteredService|SelfRegisteredService" KH.WMS` |
| 查找 Contract | `rg -n "interface I.*Contract|class .*Contract" KH.WMS` |
| 查找校验器 | `rg -n "IValidator|ConfigValidation|ValidatorCodes" KH.WMS` |
| 查找 Controller 基类 | `rg -n "CrudController|ExtDataCrudController" KH.WMS/Modules` |

---



## 继续阅读

- [后端 V3 教程目录](/backend/后端开发指引V3教程/README)
- [后端架构设计思路](/backend/架构设计/KH.WMS后端架构设计思路)
- [底层机制索引](/backend/后端底层概念/README)
