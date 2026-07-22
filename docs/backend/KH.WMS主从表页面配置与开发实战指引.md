---
title: "KH.WMS 主从表页面配置与开发实战指引"
description: "以到货预约和预约明细为案例，从建表、实体、Navigate、Service、Controller、菜单到 KhPage 主从页面逐步完成新增、编辑、查看、删除和验收。"
status: current
audience: "第一次开发 KH.WMS 主从页面的前后端开发人员和培训学员"
reviewed: "2026-07-22"
sourcePaths:
  - "KH.WMS.Client/src/views/training/arrival-appointment.vue"
  - "KH.WMS.Client/src/components/KhEditableTable/index.vue"
  - "KH.WMS/KH.WMS.Core/Services/DetailSaveService.cs"
  - "KH.WMS/Entities/KH.WMS.Entities/Training/TrnArrivalAppointment.cs"
  - "KH.WMS/Modules/TrainingModule/KH.WMS.Modules.TrainingModule/Services/TrainingArrivalAppointmentService.cs"
---

# KH.WMS 主从表页面配置与开发实战指引

这是一份可以从第一步照着做到最后一步的主从表教程。贯穿案例是“到货预约主表 + 预约明细从表”，最终页面支持查询、新增、编辑、查看、删除，以及一次更新中同时修改旧明细、新增明细和删除明细。

本文讲的是当前仓库已经实现并可运行的方式，不把规划能力写成现有能力。完整示例位于：

- 后端实体：`KH.WMS/Entities/KH.WMS.Entities/Training`。
- 后端模块：`KH.WMS/Modules/TrainingModule`。
- 前端页面：`KH.WMS.Client/src/views/training/arrival-appointment.vue`。
- SQL 和菜单：`docs/backend/实战培训`。
- 自动化验收：`KH.WMS.Client/e2e/training.spec.js`。

## 开始前必须先理解的四类配置

当前项目不存在“写一个配置项就自动生成完整主从编辑页”的能力。四类配置各有明确边界：

| 配置 | 负责什么 | 不负责什么 |
| --- | --- | --- |
| `tableColumns` | 主列表展示 | 不生成主表编辑表单，不保存明细 |
| `formColumns` | 新增/编辑弹窗里的主表字段 | 不生成明细表格 |
| `lineColumns` | `KhEditableTable` 中可编辑的明细列 | 不负责主表字段，不自动提交接口 |
| `detailLines` | “查看”弹窗里的只读明细 | 不参与新增、编辑、校验和保存 |

因此，主从编辑页必须手动组合：

```text
KhPage：主列表、查询、删除、只读详情
  + KhDialog：新增/编辑弹窗
    + KhForm：主表字段
    + KhEditableTable：明细字段
  + handleSubmit：校验并组装完整主从请求体
```

本文示例直接提交主实体及其 `items`，不要和入库单专用的 `{ order, lines }` 请求体混用。

## 第 1 步：判断数据是否真正属于主从生命周期

### 本步目标

先判断“预约明细”是否应该由“到货预约”统一创建、修改和删除，避免把两个可独立维护的对象误建成主从表。

### 为什么要做

`DetailSaveService` 使用完整集合语义。更新时，已存在但未再次提交的明细会被删除。只有从表确实依附主表生命周期时，这种语义才安全。

### 修改位置

本步先不改代码，在需求评审或设计记录中确认以下判断。

### 完整判断清单

```text
[x] 明细脱离主表后没有独立业务意义
[x] 明细必须和主表在同一个事务中保存
[x] 删除主表时允许删除所属全部明细
[x] 编辑页面每次提交完整明细集合
[x] 明细不提供独立维护页面
[x] 明细 ID 不能从一个主表迁移到另一个主表
```

到货预约符合这些条件：预约明细不能独立存在，必须归属于一张预约单。

### 执行方法

将需求中的主对象、从对象、外键、级联删除规则和更新语义逐项写清楚，再进入建表。

### 预期结果

团队对以下语义达成一致：更新请求提交完整 `items`；已有行保留 ID；新行 ID 为 `0`；页面删掉的旧行不再提交，并由后端删除。

### 常见错误

- 把可独立审批、独立查询的对象做成从表。
- 前端只提交“发生变化的行”，却使用完整集合保存服务。
- 允许用户把其他主表的明细 ID 填入当前请求。

### 验证方法

如果产品明确要求“只提交变化行”或“明细可以独立流转”，停止使用本教程的通用保存方式，改用专用 DTO 和显式增删改接口。

## 第 2 步：创建主表、从表、索引和样例数据

### 本步目标

建立隔离的培训表，并通过索引保证预约单号和同一预约单内行号唯一。

### 为什么要做

后端校验用于提供清晰提示，数据库唯一索引用于兜住并发写入。两层约束缺一不可。

### 修改位置

`docs/backend/实战培训/01-training-tables.sql`

### 完整核心 SQL

```sql
IF OBJECT_ID(N'dbo.trn_arrival_appointment', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.trn_arrival_appointment
    (
        Id                  bigint IDENTITY(1,1) NOT NULL,
        AppointmentNo       nvarchar(50) NOT NULL,
        CarrierId           bigint NULL,
        OwnerId             bigint NULL,
        WarehouseId         bigint NULL,
        AppointmentDate     date NOT NULL,
        AppointmentTimeSlot nvarchar(20) NULL,
        VehicleNo           nvarchar(30) NULL,
        DriverName          nvarchar(50) NULL,
        DriverPhone         nvarchar(20) NULL,
        Remark              nvarchar(500) NULL,
        CreatedBy           nvarchar(50) NULL,
        CreatedByName       nvarchar(50) NULL,
        CreatedTime         datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
        LastModifiedBy      nvarchar(50) NULL,
        LastModifiedByName  nvarchar(50) NULL,
        LastModifiedTime    datetime2(3) NULL,
        CONSTRAINT PK_trn_arrival_appointment PRIMARY KEY CLUSTERED (Id)
    );
END;

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.trn_arrival_appointment')
      AND name = N'uk_trn_arrival_appointment_no'
)
    CREATE UNIQUE INDEX uk_trn_arrival_appointment_no
        ON dbo.trn_arrival_appointment(AppointmentNo);

IF OBJECT_ID(N'dbo.trn_arrival_appointment_line', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.trn_arrival_appointment_line
    (
        Id                 bigint IDENTITY(1,1) NOT NULL,
        AppointmentId      bigint NOT NULL,
        LineNo             int NOT NULL,
        MaterialId         bigint NULL,
        MaterialCode       nvarchar(50) NOT NULL,
        MaterialName       nvarchar(200) NOT NULL,
        ExpectedQty        decimal(12,3) NOT NULL,
        UnitId             bigint NULL,
        BatchNo            nvarchar(50) NULL,
        Remark             nvarchar(500) NULL,
        CreatedBy          nvarchar(50) NULL,
        CreatedByName      nvarchar(50) NULL,
        CreatedTime        datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
        LastModifiedBy     nvarchar(50) NULL,
        LastModifiedByName nvarchar(50) NULL,
        LastModifiedTime   datetime2(3) NULL,
        CONSTRAINT PK_trn_arrival_appointment_line PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_trn_arrival_line_no CHECK (LineNo > 0),
        CONSTRAINT CK_trn_arrival_expected_qty CHECK (ExpectedQty > 0)
    );
END;

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.trn_arrival_appointment_line')
      AND name = N'idx_trn_arrival_appointment_line_header'
)
    CREATE INDEX idx_trn_arrival_appointment_line_header
        ON dbo.trn_arrival_appointment_line(AppointmentId);

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.trn_arrival_appointment_line')
      AND name = N'uk_trn_arrival_appointment_line_no'
)
    CREATE UNIQUE INDEX uk_trn_arrival_appointment_line_no
        ON dbo.trn_arrival_appointment_line(AppointmentId, LineNo);
```

仓库脚本还包含承运商、货主和幂等样例数据，请实际执行完整文件，不要只复制上面的建表片段。

### 执行方法

```powershell
sqlcmd -S "<server>" -d "<training-database>" -E -b -f 65001 -i "docs\backend\实战培训\01-training-tables.sql"
```

连续执行两次。

### 预期结果

四张培训表存在，样例数据计数为 `3/3/3/5`；第二次执行后计数不增加。

### 常见错误

- 在错误的业务库执行脚本。
- 用不支持 UTF-8 的旧版 `sqlcmd` 执行，却漏掉 `-f 65001`。
- 只建普通索引，漏掉 `(AppointmentId, LineNo)` 唯一索引。

### 验证方法

```sql
SELECT COUNT_BIG(1) FROM dbo.trn_arrival_appointment;
SELECT COUNT_BIG(1) FROM dbo.trn_arrival_appointment_line;
SELECT AppointmentId, LineNo, COUNT_BIG(1)
FROM dbo.trn_arrival_appointment_line
GROUP BY AppointmentId, LineNo
HAVING COUNT_BIG(1) > 1;
```

最后一条查询应返回零行。

## 第 3 步：创建主实体与从实体

### 本步目标

让 C# 属性、SQL 列名、长度、可空性和精度一一对应。

### 为什么要做

实体是请求绑定、仓储映射和导航保存共同使用的结构。字段类型不一致会在序列化或写库时才暴露问题。

### 修改位置

- `KH.WMS/Entities/KH.WMS.Entities/Training/TrnArrivalAppointment.cs`
- `KH.WMS/Entities/KH.WMS.Entities/Training/TrnArrivalAppointmentLine.cs`

### 完整代码

```csharp
using KH.WMS.Core.Models.Entities;
using SqlSugar;

namespace KH.WMS.Entities.Training;

[SugarTable("trn_arrival_appointment")]
[SugarIndex("uk_trn_arrival_appointment_no", nameof(AppointmentNo), OrderByType.Asc, true)]
public class TrnArrivalAppointment : BaseEntity<long>
{
    [SugarColumn(Length = 50, IsNullable = false)]
    public string AppointmentNo { get; set; } = string.Empty;

    [SugarColumn(IsNullable = true)]
    public long? CarrierId { get; set; }

    [SugarColumn(IsNullable = true)]
    public long? OwnerId { get; set; }

    [SugarColumn(IsNullable = true)]
    public long? WarehouseId { get; set; }

    [SugarColumn(IsNullable = false)]
    public DateOnly AppointmentDate { get; set; }

    [SugarColumn(Length = 20, IsNullable = true)]
    public string? AppointmentTimeSlot { get; set; }

    [SugarColumn(Length = 30, IsNullable = true)]
    public string? VehicleNo { get; set; }

    [SugarColumn(Length = 50, IsNullable = true)]
    public string? DriverName { get; set; }

    [SugarColumn(Length = 20, IsNullable = true)]
    public string? DriverPhone { get; set; }

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Remark { get; set; }

    [Navigate(NavigateType.OneToMany, nameof(TrnArrivalAppointmentLine.AppointmentId), nameof(Id))]
    public List<TrnArrivalAppointmentLine>? Items { get; set; }
}
```

```csharp
using KH.WMS.Core.Models.Entities;
using SqlSugar;

namespace KH.WMS.Entities.Training;

[SugarTable("trn_arrival_appointment_line")]
[SugarIndex("uk_trn_arrival_appointment_line_no", nameof(AppointmentId), OrderByType.Asc, nameof(LineNo), OrderByType.Asc, true)]
public class TrnArrivalAppointmentLine : BaseEntity<long>
{
    [SugarColumn(IsNullable = false)]
    public long AppointmentId { get; set; }

    [SugarColumn(IsNullable = false)]
    public int LineNo { get; set; }

    [SugarColumn(IsNullable = true)]
    public long? MaterialId { get; set; }

    [SugarColumn(Length = 50, IsNullable = false)]
    public string MaterialCode { get; set; } = string.Empty;

    [SugarColumn(Length = 200, IsNullable = false)]
    public string MaterialName { get; set; } = string.Empty;

    [SugarColumn(ColumnDataType = "decimal(12,3)", IsNullable = false)]
    public decimal ExpectedQty { get; set; }

    [SugarColumn(IsNullable = true)]
    public long? UnitId { get; set; }

    [SugarColumn(Length = 50, IsNullable = true)]
    public string? BatchNo { get; set; }

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Remark { get; set; }
}
```

### 执行方法

创建文件后保存为 UTF-8，并确保实体项目自动包含这两个 `.cs` 文件。

### 预期结果

解决方案能找到 `KH.WMS.Entities.Training` 命名空间，主从实体都继承 `BaseEntity<long>`。

### 常见错误

- 在子类重复声明 `Id`、`CreatedTime` 等公共字段。
- `ExpectedQty` 使用 `double`，与数据库 decimal 精度不一致。
- 从表外键写成可空 `long?`，掩盖孤儿明细问题。

### 验证方法

```powershell
dotnet build "KH.WMS\Entities\KH.WMS.Entities\KH.WMS.Entities.csproj" --no-restore
```

## 第 4 步：配置 OneToMany Navigate

### 本步目标

告诉仓储和 `DetailSaveService`：主实体的 `Items` 是一对多明细，外键是 `AppointmentId`。

### 为什么要做

没有 `Navigate` 时，详情查询不会自动带出明细，保存服务也会跳过该集合。

### 修改位置

`TrnArrivalAppointment.Items`

### 完整代码

```csharp
[Navigate(
    NavigateType.OneToMany,
    nameof(TrnArrivalAppointmentLine.AppointmentId),
    nameof(TrnArrivalAppointment.Id))]
public List<TrnArrivalAppointmentLine>? Items { get; set; }
```

三个参数的对应关系：

| 参数 | 当前值 | 含义 |
| --- | --- | --- |
| 导航类型 | `NavigateType.OneToMany` | 一个预约对应多条明细 |
| 从表外键 | `AppointmentId` | 写入从表时要回填的字段 |
| 主表关联键 | `Id` | 外键值来源 |

### 执行方法

把导航特性放在主表集合属性上，不要放到从表普通字段上。

### 预期结果

`GetByIdWithNavAsync(id)` 返回主表时，`Items` 包含所属明细；保存服务能扫描到该集合。

### 常见错误

- 把第二、第三个参数写反。
- 属性类型使用 `IEnumerable<T>`，而当前保存服务只扫描 `List<T>`。
- 只声明 `Items`，忘记 `[Navigate]`。

### 验证方法

启动后调用 `GET /api/training-arrival-appointment/{id}`，响应中的 `items` 应为数组而不是始终为空。

## 第 5 步：创建 Service 接口和实现，并注入 IDetailSaveService

### 本步目标

复用标准 CRUD，同时启用主从事务保存。

### 为什么要做

`CrudService<TEntity>` 只有收到 `IDetailSaveService` 才会在主表保存后处理导航明细。漏注入时接口可能返回成功，但从表没有数据。

### 修改位置

- `TrainingModule/Interfaces/ITrainingArrivalAppointmentService.cs`
- `TrainingModule/Services/TrainingArrivalAppointmentService.cs`

### 完整代码

```csharp
using KH.WMS.Core.Services;
using KH.WMS.Entities.Training;

namespace KH.WMS.Modules.TrainingModule.Interfaces;

public interface ITrainingArrivalAppointmentService
    : ICrudService<TrnArrivalAppointment>;
```

```csharp
[RegisteredService(ServiceType = typeof(ITrainingArrivalAppointmentService))]
public class TrainingArrivalAppointmentService(
    IRepository<TrnArrivalAppointment, long> repository,
    IUnitOfWork unitOfWork,
    IDetailSaveService detailSaveService)
    : CrudService<TrnArrivalAppointment>(repository, unitOfWork, detailSaveService),
      ITrainingArrivalAppointmentService
{
    protected override async Task BeforeCreateAsync(TrnArrivalAppointment entity)
    {
        await Validate(entity);
    }

    protected override async Task BeforeUpdateAsync(TrnArrivalAppointment entity)
    {
        await Validate(entity);
    }

    private async Task Validate(TrnArrivalAppointment entity)
    {
        // 第 6 步补齐
        await Task.CompletedTask;
    }
}
```

### 执行方法

保持仓库现有 `[RegisteredService]` 自动注册方式，不在 `Program.cs` 重复注册。

### 预期结果

运行时可以解析 `ITrainingArrivalAppointmentService`、`IDetailSaveService` 和主实体仓储。

### 常见错误

- 构造函数有 `IDetailSaveService`，但基类调用漏传该参数。
- Service 忘记 `[RegisteredService]`。
- 接口没有继承 `ICrudService<TrnArrivalAppointment>`。

### 验证方法

启动 Server；如果依赖缺失，应在启动或首次访问接口时立即暴露解析错误。

## 第 6 步：添加主表、明细和跨行校验

### 本步目标

在进入写库前拒绝无效整单，并给出能直接定位的错误信息。

### 为什么要做

数据库约束只能告诉调用方“写入失败”，不能替代预约单号唯一、至少一条明细、行号不重复等业务规则。

### 修改位置

`TrainingArrivalAppointmentService.Validate`

### 完整代码

```csharp
private async Task Validate(TrnArrivalAppointment entity)
{
    if (string.IsNullOrWhiteSpace(entity.AppointmentNo))
        throw new InvalidOperationException("预约单号不能为空");

    if (await _repository.ExistsAsync(x =>
        x.AppointmentNo == entity.AppointmentNo.Trim() && x.Id != entity.Id))
        throw new InvalidOperationException($"预约单号 {entity.AppointmentNo.Trim()} 已存在");

    if (entity.Items is not { Count: > 0 })
        throw new InvalidOperationException("到货预约至少需要一条明细");

    if (entity.Items.Any(x => x.LineNo <= 0))
        throw new InvalidOperationException("明细行号必须大于 0");

    if (entity.Items.GroupBy(x => x.LineNo).Any(x => x.Count() > 1))
        throw new InvalidOperationException("同一预约单的明细行号不能重复");

    if (entity.Items.Any(x =>
        string.IsNullOrWhiteSpace(x.MaterialCode) ||
        string.IsNullOrWhiteSpace(x.MaterialName)))
        throw new InvalidOperationException("明细物料编码和名称不能为空");

    if (entity.Items.Any(x => x.ExpectedQty <= 0))
        throw new InvalidOperationException("明细预计到货数量必须大于 0");
}
```

### 执行方法

创建和更新都调用同一验证方法，避免两个入口规则漂移。

### 预期结果

任何一条明细不合法时，主表和所有明细一起回滚。

### 常见错误

- 只在前端校验，绕过页面直接调 API 就能写入脏数据。
- 只检查明细数量，不检查重复行号。
- 更新唯一性校验漏掉 `x.Id != entity.Id`，导致自己和自己冲突。

### 验证方法

分别提交空 `items`、重复 `lineNo`、空物料编码和 `expectedQty = 0`，再按预约单号分页查询，均不应留下主表。

> 通用 `DetailSaveService` 支持空数组代表“删除全部明细”；但本到货预约案例有“至少一条明细”的业务规则，所以 Service 会先拒绝空数组。其他允许零明细的业务可以不加这条规则。

## 第 7 步：使用 CrudController 暴露标准接口

### 本步目标

用最少代码提供分页、详情、新增、更新和删除接口。

### 为什么要做

这些接口已经由 `CrudController<TEntity>` 实现，重复写 Action 容易造成路由和返回契约不一致。

### 修改位置

`TrainingModule/Controllers/TrainingArrivalAppointmentController.cs`

### 完整代码

```csharp
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Training;
using KH.WMS.Modules.TrainingModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.TrainingModule.Controllers;

[Route("api/training-arrival-appointment")]
public class TrainingArrivalAppointmentController(
    ITrainingArrivalAppointmentService service)
    : CrudController<TrnArrivalAppointment>(service);
```

固定接口为：

```text
POST   /api/training-arrival-appointment/pagelist
GET    /api/training-arrival-appointment/{id}
POST   /api/training-arrival-appointment/create
POST   /api/training-arrival-appointment/update
DELETE /api/training-arrival-appointment/delete/{id}
```

### 执行方法

路由中的 `training-arrival-appointment` 必须与前端 `useCrudApi(...)` 和 `KhPage module` 完全一致。

### 预期结果

Swagger 中出现这组标准接口，详情接口返回 `items`。

### 常见错误

- 把更新写成 `PUT`，但前端通用 CRUD 当前使用 `POST /update`。
- Controller 继承 `ControllerBase` 后手写不完整接口。
- 路由名和前端 module 多一个或少一个单词。

### 验证方法

打开 Swagger，按上面的五个地址逐项核对 HTTP 方法和路径。

## 第 8 步：注册 TrainingModule、菜单、页面路由和按钮权限

### 本步目标

让 Server 发现模块，让前端动态路由发现页面，并让新增、编辑、删除、查看按钮受权限控制。

### 为什么要做

只创建 Controller 或 Vue 文件都不够。Server 必须引用模块程序集，`sys_permission` 还必须提供正确的 `Path`、`Component` 和按钮权限。

### 修改位置

- `KH.WMS.Server.csproj`
- `KH.WMS.sln`
- `docs/backend/实战培训/03-training-menu.sql`

### 完整配置

```xml
<ProjectReference Include="..\Modules\TrainingModule\KH.WMS.Modules.TrainingModule\KH.WMS.Modules.TrainingModule.csproj" />
```

```sql
IF NOT EXISTS
(
    SELECT 1 FROM dbo.sys_permission
    WHERE PermissionCode = N'training_appointment'
)
INSERT dbo.sys_permission
(
    PermissionCode, PermissionName, ParentId, MenuType,
    Path, Component, Icon, SortNo, IsVisible, Status,
    Buttons, IsExternal, IsCache, CreatedTime
)
VALUES
(
    N'training_appointment', N'实战三：到货预约', @ParentId, 1,
    N'/training/arrival-appointment', N'training/arrival-appointment',
    N'schedule', 3, 1, 1,
    N'[
      {"buttonCode":"btn_add","buttonName":"新增","permKey":"training:appointment:add","sortNo":1,"status":1},
      {"buttonCode":"btn_edit","buttonName":"编辑","permKey":"training:appointment:edit","sortNo":2,"status":1},
      {"buttonCode":"btn_delete","buttonName":"删除","permKey":"training:appointment:delete","sortNo":3,"status":1},
      {"buttonCode":"btn_view","buttonName":"查看","permKey":"training:appointment:view","sortNo":4,"status":1}
    ]',
    0, 0, SYSDATETIME()
);
```

### 执行方法

先构建解决方案，再在培训业务库执行完整的 `03-training-menu.sql`，然后重新登录以刷新菜单和按钮权限。

### 预期结果

菜单出现“实战培训 → 实战三：到货预约”，访问地址为 `/training/arrival-appointment`。

### 常见错误

- `Component` 写成 `/training/arrival-appointment.vue`。正确值不带开头斜杠和 `.vue`。
- 菜单已经存在，但旧数据的 `Component` 值错误；幂等脚本只判断不存在，不会自动修旧值。
- 改完菜单不重新登录，仍使用旧权限缓存。

### 验证方法

```sql
SELECT PermissionCode, Path, Component, Buttons
FROM dbo.sys_permission
WHERE PermissionCode = N'training_appointment';
```

## 第 9 步：先用 Swagger 验证主从接口

### 本步目标

在写页面前先证明后端契约正确，避免把 API 问题误判成页面配置问题。

### 为什么要做

主从页面同时涉及路由、绑定、事务、导航和明细 ID。先从 HTTP 层分段验证，定位成本最低。

### 修改位置

本步不改代码，通过 Swagger 调用第 7 步的接口。

### 完整创建请求

```json
{
  "id": 0,
  "appointmentNo": "TRN-APT-20260803-001",
  "carrierId": 1,
  "ownerId": 1,
  "warehouseId": null,
  "appointmentDate": "2026-08-03",
  "appointmentTimeSlot": "09:00-11:00",
  "vehicleNo": "沪A·STUDY",
  "driverName": "培训司机",
  "driverPhone": "13800000000",
  "remark": "Swagger 主从测试",
  "items": [
    {
      "id": 0,
      "lineNo": 1,
      "materialId": null,
      "materialCode": "TRN-MAT-010",
      "materialName": "学员物料",
      "expectedQty": 10.000,
      "unitId": null,
      "batchNo": "TRN-BATCH-01",
      "remark": "第一行"
    }
  ]
}
```

### 执行方法

1. 调 `create`，记录返回的主表 ID。
2. 调 `GET /{id}`，记录明细 ID。
3. 调 `update`，已有明细保留 ID，再增加一条 `id = 0` 的新明细。
4. 再次调详情，确认两条明细都有非零 ID。
5. 更新时只提交其中一条，确认另一条被删除。
6. 调删除接口，再查详情。

### 预期结果

创建返回有效主键；详情带 `items`；更新支持新增、修改和遗漏删除；删除后详情返回业务码 `404`。

### 常见错误

- 创建请求给明细填历史 ID。
- 更新请求把已有明细 ID 改成 `0`，导致重复插入。
- 把请求体包成 `{ "order": ..., "lines": ... }`。

### 验证方法

同时查数据库，确认主表 ID、明细 `AppointmentId` 和 API 返回值一致。

## 第 10 步：创建 KhPage 主列表并关闭内置新增、编辑

### 本步目标

让 `KhPage` 负责查询、删除和只读详情，把新增、编辑交给自定义弹窗。

### 为什么要做

`KhPage` 内置新增/编辑适合单实体表单，不会自动生成 `KhEditableTable`、逐行校验和完整 `items` 请求体。

### 修改位置

`KH.WMS.Client/src/views/training/arrival-appointment.vue`

### 完整模板骨架

```vue
<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage
      ref="pageRef"
      title="到货预约实战"
      module="training-arrival-appointment"
      :search-columns="searchColumns"
      :search-model="searchModel"
      :columns="tableColumns"
      :show-stat-cards="false"
      :show-toolbar="true"
      :show-index="true"
      :show-header-filter="true"
      :crud-operations="{
        create: false,
        update: false,
        delete: true,
        view: true,
        export: false
      }"
      permission-prefix="training:appointment"
      :action-buttons="actionButtons"
      :toolbar-buttons="toolbarButtons"
      :detail-lines="detailLines"
      detail-width="1100px"
    />
  </div>
</template>
```

### 执行方法

确认 `module` 对应后端路由，`permission-prefix` 对应菜单按钮的权限前缀。

### 预期结果

页面可加载主列表；内置新增和编辑按钮不出现；删除和查看仍由 `KhPage` 工作。

### 常见错误

- `crudOperations` 仍启用 `create/update`，页面出现两套按钮。
- `permission-prefix` 写成 `training-arrival-appointment`，与菜单权限不一致。
- 以为 `detailLines` 会生成编辑表格。

### 验证方法

打开页面，确认只有自定义“新增”“编辑”和内置“查看”“删除”四类业务入口。

## 第 11 步：配置查询、列表、主表、明细编辑和详情字段

### 本步目标

把五组字段配置放到正确位置，避免“配置写了但页面没反应”。

### 为什么要做

查询、主列表、主表表单、可编辑明细和只读明细由不同组件消费，不能互相替代。

### 修改位置

`arrival-appointment.vue` 的 `<script setup>`。

### 完整配置

```js
const searchModel = reactive({ appointmentNo: '', appointmentDate: '' })

const searchColumns = [
  { prop: 'appointmentNo', label: '预约单号', type: 'input', clearable: true },
  { prop: 'appointmentDate', label: '预约日期', type: 'date', clearable: true },
]

const tableColumns = [
  { prop: 'appointmentNo', label: '预约单号', width: 190 },
  { prop: 'appointmentDate', label: '预约日期', width: 110, align: 'center' },
  { prop: 'appointmentTimeSlot', label: '预约时段', width: 120 },
  { prop: 'carrierId', label: '承运商', width: 140, type: 'tag', tagMap: computed(() => carrierTagMap.value) },
  { prop: 'ownerId', label: '货主', width: 140, type: 'tag', tagMap: computed(() => ownerTagMap.value) },
  { prop: 'warehouseId', label: '仓库', width: 130, type: 'tag', tagMap: 'dict:warehouse_list' },
  { prop: 'vehicleNo', label: '车牌号', width: 120 },
  { prop: 'driverName', label: '司机', width: 100 },
  { prop: 'driverPhone', label: '司机电话', width: 130 },
  { prop: 'remark', label: '备注', minWidth: 150, showOverflowTooltip: true },
]

const formColumns = computed(() => [
  { prop: 'appointmentNo', label: '预约单号', type: 'input', required: true, maxlength: 50, colSpan: 2 },
  { prop: 'appointmentDate', label: '预约日期', type: 'date', required: true, valueFormat: 'YYYY-MM-DD' },
  { prop: 'appointmentTimeSlot', label: '预约时段', type: 'input', maxlength: 20 },
  { prop: 'carrierId', label: '承运商', type: 'select', clearable: true, options: carrierOptions.value },
  { prop: 'ownerId', label: '货主', type: 'select', clearable: true, options: ownerOptions.value },
  { prop: 'warehouseId', label: '仓库', type: 'select', clearable: true, options: 'dict:warehouse_list' },
  { prop: 'vehicleNo', label: '车牌号', type: 'input', maxlength: 30 },
  { prop: 'driverName', label: '司机姓名', type: 'input', maxlength: 50 },
  { prop: 'driverPhone', label: '司机电话', type: 'input', maxlength: 20 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 500 },
])

const lineColumns = [
  { prop: 'lineNo', label: '行号', type: 'number', width: 80, min: 1, precision: 0 },
  { prop: 'materialCode', label: '物料编码', type: 'select', minWidth: 150, options: 'dict:material_list' },
  { prop: 'materialName', label: '物料名称', type: 'input', minWidth: 170 },
  { prop: 'expectedQty', label: '预计数量', type: 'number', width: 120, min: 0.001, precision: 3, controls: false },
  { prop: 'batchNo', label: '批次号', type: 'input', width: 140 },
  { prop: 'remark', label: '备注', type: 'input', minWidth: 140 },
]

const detailLines = [{
  prop: 'items',
  title: '预约明细',
  columns: [
    { prop: 'lineNo', label: '行号', width: 70 },
    { prop: 'materialCode', label: '物料编码', width: 140 },
    { prop: 'materialName', label: '物料名称', minWidth: 170 },
    { prop: 'expectedQty', label: '预计数量', width: 120 },
    { prop: 'batchNo', label: '批次号', width: 140 },
    { prop: 'remark', label: '备注', minWidth: 140 },
  ]
}]
```

### 执行方法

按后端 JSON 属性名填写 `prop`。当前序列化输出为小驼峰，所以集合属性是 `items`。

### 预期结果

列表、表单、编辑明细和只读详情各自显示对应字段。

### 常见错误

- `detailLines.prop` 写成 `lines`，而接口返回 `items`。
- 表格展示列使用 `type: 'select'`，导致只读列表不按 tag 展示。
- `formColumns` 不是 computed，异步加载的下拉选项不更新。

### 验证方法

逐一检查五组配置中的每个 `prop` 是否能在请求或响应中找到同名字段。

## 第 12 步：创建新增、编辑弹窗和明细默认行

### 本步目标

组合 `KhDialog + KhForm + KhEditableTable`，建立可控的主从编辑状态。

### 为什么要做

自定义弹窗让页面能够手动加载详情、保留明细 ID、执行逐行校验并组装请求。

### 修改位置

`arrival-appointment.vue`

### 完整代码

```vue
<KhDialog
  v-model="dialogVisible"
  :title="dialogMode === 'create' ? '新增到货预约' : '编辑到货预约'"
  width="1150px"
  destroy-on-close
  :confirm-loading="submitLoading"
  @confirm="handleSubmit"
  @close="resetForm"
>
  <KhForm
    ref="formRef"
    v-model="formData"
    :columns="formColumns"
    :col-count="4"
    label-width="100px"
  />
  <el-divider content-position="left">预约明细</el-divider>
  <KhEditableTable
    v-model="items"
    :columns="lineColumns"
    :default-row="createEmptyLine"
    :max-height="360"
    add-text="添加明细"
    :action-width="70"
  />
</KhDialog>
```

```js
const dialogVisible = ref(false)
const dialogMode = ref('create')
const submitLoading = ref(false)
const items = ref([])

const createFormData = () => ({
  id: 0,
  appointmentNo: '',
  carrierId: null,
  ownerId: null,
  warehouseId: null,
  appointmentDate: '',
  appointmentTimeSlot: '',
  vehicleNo: '',
  driverName: '',
  driverPhone: '',
  remark: '',
})

const formData = reactive(createFormData())

const createEmptyLine = () => ({
  id: 0,
  lineNo: items.value.length + 1,
  materialId: null,
  materialCode: '',
  materialName: '',
  expectedQty: null,
  unitId: null,
  batchNo: '',
  remark: '',
})

function resetForm() {
  Object.assign(formData, createFormData())
  items.value = []
}
```

### 执行方法

新增前调用 `resetForm()`；关闭时也重置，防止上一次编辑数据残留。

### 预期结果

点击新增打开空主表；点击“添加明细”依次生成行号 1、2、3。

### 常见错误

- `default-row` 传对象而不是函数，导致多行共享同一个对象。
- 只清空主表，不清空 `items`。
- 认为 `KhDialog` 会自动校验插槽中的自定义 `KhForm`。

### 验证方法

连续打开、关闭新增弹窗两次，第二次不应残留第一次输入的数据。

## 第 13 步：加载承运商、货主、仓库和物料选项

### 本步目标

把外键字段转成用户可读的下拉选项，同时保留实际提交的 ID 或编码。

### 为什么要做

主表存的是承运商、货主、仓库 ID，用户需要看到编码和名称；明细物料则使用现有字典选项。

### 修改位置

- `KH.WMS.Client/src/api/training.js`
- `arrival-appointment.vue`

### 完整代码

```js
import request from '@/utils/request'

export const getTrainingCarriers = () =>
  request.get('/api/training-carrier/all')

export const getTrainingOwners = () =>
  request.get('/api/training-owner-profile/all')
```

```js
import { getTrainingCarriers, getTrainingOwners } from '@/api/training'

const carrierOptions = ref([])
const ownerOptions = ref([])

const carrierTagMap = computed(() =>
  Object.fromEntries(carrierOptions.value.map(x => [x.value, x.label])))

const ownerTagMap = computed(() =>
  Object.fromEntries(ownerOptions.value.map(x => [x.value, x.label])))

onMounted(async () => {
  const [carriers, owners] = await Promise.all([
    getTrainingCarriers(),
    getTrainingOwners(),
  ])

  carrierOptions.value = (carriers.data || []).map(x => ({
    label: `${x.carrierCode} - ${x.carrierName}`,
    value: x.id,
  }))

  ownerOptions.value = (owners.data || []).map(x => ({
    label: `${x.ownerCode} - ${x.ownerName}`,
    value: x.id,
  }))
})
```

仓库和物料沿用现有字典：`dict:warehouse_list`、`dict:material_list`。

### 执行方法

并行加载互不依赖的选项；API 返回后映射为 `{ label, value }`。

### 预期结果

表单下拉显示编码和名称，提交值仍是 ID；列表 tag 显示可读名称。

### 常见错误

- 把整个对象提交给 `carrierId`。
- 下拉配置写成字符串，但对应字典不存在。
- 忽略 API 失败，页面打开后下拉始终为空。

### 验证方法

在浏览器网络面板检查两个 `/all` 请求；选择后查看 `formData.carrierId` 和 `ownerId`，应为数字或空值。

## 第 14 步：实现主表校验、逐行校验、重复行号校验和提交防抖

### 本步目标

在发送请求前给用户及时反馈，并防止重复点击产生两张预约单。

### 为什么要做

前端校验改善操作体验，后端校验保证安全；两者职责不同，必须同时存在。

### 修改位置

`arrival-appointment.vue`

### 完整代码

```js
const validateItems = () => {
  if (!items.value.length) return '至少添加一条预约明细'
  if (items.value.some(x => !x.lineNo || x.lineNo <= 0))
    return '明细行号必须大于 0'
  if (new Set(items.value.map(x => x.lineNo)).size !== items.value.length)
    return '明细行号不能重复'
  if (items.value.some(x => !x.materialCode || !x.materialName))
    return '物料编码和名称不能为空'
  if (items.value.some(x => !x.expectedQty || x.expectedQty <= 0))
    return '预计到货数量必须大于 0'
  return ''
}

const handleSubmit = async () => {
  if (submitLoading.value || !await formRef.value?.validate()) return

  const error = validateItems()
  if (error) return KhMessageFn.warning(error)

  submitLoading.value = true
  try {
    // 第 15、17 步组装请求
  } finally {
    submitLoading.value = false
  }
}
```

### 执行方法

先判断 `submitLoading`，再校验主表，最后校验明细；任何一步失败都不发送请求。

### 预期结果

必填项、空明细、重复行号、空物料和无效数量会在页面直接提示；提交期间确认按钮进入 loading。

### 常见错误

- 忘记 `await formRef.value.validate()`。
- 只在按钮上设置 loading，函数没有防重入判断。
- `finally` 中不恢复 loading，接口报错后按钮永久不可用。

### 验证方法

连续快速点击两次确认，网络面板只能出现一个 create 或 update 请求。

## 第 15 步：组装新增请求，并移除前端临时字段

### 本步目标

创建时强制主表和全部明细 ID 为 `0`，只提交后端实体认识的字段。

### 为什么要做

创建请求携带非零明细 ID 可能造成越权引用。前端行键、显示文本等临时字段也不应进入实体请求。

### 修改位置

`handleSubmit`

### 完整代码

```js
const toRequestLine = (row, mode) => {
  const { _rowKey, materialLabel, ...line } = row
  return {
    ...line,
    id: mode === 'create' ? 0 : (line.id || 0),
    appointmentId: mode === 'create' ? 0 : (line.appointmentId || 0),
  }
}

const buildCreateRequest = () => ({
  ...formData,
  id: 0,
  items: items.value.map(row => toRequestLine(row, 'create')),
})

if (dialogMode.value === 'create') {
  await crudApi.create(buildCreateRequest())
}
```

当前实际页面没有 `_rowKey` 和 `materialLabel`，示例仍显式展示清理方式，便于以后增加临时字段时不污染请求。

### 执行方法

把请求组装集中到纯函数，不要直接修改页面正在编辑的 `items`。

### 预期结果

创建请求的主表 `id = 0`，每条明细 `id = 0`；后端插入后回填真实主键和外键。

### 常见错误

- 把编辑过的旧明细复制到新增弹窗，仍携带非零 ID。
- 直接发送 `{ ...row }`，把 `_rowKey` 等临时字段带给后端。
- 前端自己猜 `appointmentId`，而不是让后端用主表生成 ID 回填。

### 验证方法

在网络面板查看 create 请求；再调详情，主表 ID、明细 ID 和明细 `appointmentId` 都应为有效值。

## 第 16 步：加载编辑详情并保留已有明细 ID

### 本步目标

编辑时使用详情接口返回的完整主从数据，而不是只复制列表行。

### 为什么要做

列表通常不包含 `items`。如果编辑时丢失明细 ID，保存服务会把所有行当新行插入，造成重复数据。

### 修改位置

`openUpdate`

### 完整代码

```js
const openUpdate = async (row) => {
  const res = await crudApi.detail(row.id)
  const detail = res.data || res

  resetForm()
  dialogMode.value = 'update'
  Object.assign(formData, detail)
  items.value = (detail.items || []).map(x => ({ ...x }))
  dialogVisible.value = true
}

const actionButtons = [{
  label: '编辑',
  permission: 'training:appointment:edit',
  onClick: openUpdate,
}]
```

### 执行方法

先等待详情接口，再填充表单；对每条明细浅复制，避免意外改写缓存对象。

### 预期结果

编辑弹窗能回显主表和全部明细，每条已有明细仍带原来的非零 ID。

### 常见错误

- `items.value = row.items || []`，但列表行没有明细。
- 映射明细时只保留显示字段，漏掉 `id`。
- 先打开弹窗再异步回填，用户短暂看到上一张单的数据。

### 验证方法

打开浏览器开发工具，在编辑状态检查 `items.value.map(x => x.id)`，已有行都应大于 `0`。

## 第 17 步：组装完整更新集合

### 本步目标

一次请求同时表达：修改已有行、新增行和删除行。

### 为什么要做

当前通用保存服务不是差量补丁接口，而是完整集合接口。后端用提交 ID 集合与数据库已有 ID 集合做差集。

### 修改位置

`handleSubmit`

### 完整代码

```js
const buildUpdateRequest = () => ({
  ...formData,
  items: items.value.map(row => toRequestLine(row, 'update')),
})

const handleSubmit = async () => {
  if (submitLoading.value || !await formRef.value?.validate()) return

  const error = validateItems()
  if (error) return KhMessageFn.warning(error)

  submitLoading.value = true
  try {
    if (dialogMode.value === 'create') {
      await crudApi.create(buildCreateRequest())
    } else {
      await crudApi.update(buildUpdateRequest())
    }

    KhMessageFn.success(
      dialogMode.value === 'create' ? '新增成功' : '更新成功'
    )
    dialogVisible.value = false
    pageRef.value?.reload()
  } finally {
    submitLoading.value = false
  }
}
```

完整集合规则必须牢记：

```text
已有行：保留原非零 ID -> 后端更新
新增行：ID = 0 -> 后端插入并回填 ID
删除行：从 items 中移除 -> 后端删除遗漏的已有 ID
其他主表的明细 ID：后端拒绝
```

### 执行方法

编辑一张有两条明细的预约单：修改第一条，删除第二条，再增加第三条，然后只提交界面当前两条。

### 预期结果

数据库最终只有界面当前两条：第一条 ID 不变且内容更新；新行获得新 ID；被删行不存在。

### 常见错误

- 只提交变化的两条，误删其他未提交旧行。
- 新行沿用复制来源的旧 ID。
- 为了“删除全部”提交 `items: null`，没有明确区分字段缺失和完整集合语义。

### 验证方法

保存后重新调详情，不依赖页面本地状态；对比保存前后的 ID 集合。

## 第 18 步：配置只读详情中的 detailLines

### 本步目标

让用户点击“查看”时同时看到主表字段和只读明细表格。

### 为什么要做

`KhPage` 的详情弹窗只知道主列表列。明细数组必须通过 `detailLines` 明确告诉 `KhDetailDialog` 如何展示。

### 修改位置

`arrival-appointment.vue` 的 `detailLines` 和 `KhPage` 属性。

### 完整代码

```js
const detailLines = [{
  prop: 'items',
  title: '预约明细',
  columns: [
    { prop: 'lineNo', label: '行号', width: 70 },
    { prop: 'materialCode', label: '物料编码', width: 140 },
    { prop: 'materialName', label: '物料名称', minWidth: 170 },
    { prop: 'expectedQty', label: '预计数量', width: 120 },
    { prop: 'batchNo', label: '批次号', width: 140 },
    { prop: 'remark', label: '备注', minWidth: 140 },
  ],
}]
```

```vue
<KhPage
  :detail-lines="detailLines"
  detail-width="1100px"
/>
```

### 执行方法

`prop` 必须等于详情响应中的数组字段 `items`；列配置只放只读展示信息。

### 预期结果

点击查看后，详情弹窗下方出现“预约明细”表格。

### 常见错误

- 把 `detailLines` 当成编辑配置，期望出现输入框。
- `detail-width` 太小，明细表横向挤压。
- 详情接口未加载 Navigate，配置正确但数组为空。

### 验证方法

直接比较网络响应 `data.items.length` 和详情弹窗明细行数，两者应一致。

## 第 19 步：验证新增、修改、明细增删和级联删除

### 本步目标

用一套可重复的验收流程覆盖主从生命周期，而不是只看“接口返回成功”。

### 为什么要做

主表成功不代表明细成功；更新成功也不代表遗漏行正确删除。必须检查返回、数据库和页面刷新后的真实状态。

### 修改位置

自动化用例：`KH.WMS.Client/e2e/training.spec.js`

### 完整验收矩阵

| 场景 | 操作 | 必须验证 |
| --- | --- | --- |
| 新增一主多从 | 创建两条明细 | 主 ID、明细 ID、外键均有效 |
| 无明细 | `items: []` | 整单失败且主表未残留 |
| 重复行号 | 两行 `lineNo = 1` | 整单回滚 |
| 数量无效 | `expectedQty = 0` | 整单回滚 |
| 混合更新 | 改旧行、加新行、漏交一行 | 旧 ID 保留、新 ID 回填、漏交行删除 |
| 跨主表 ID | A 单提交 B 单明细 ID | 请求失败，A/B 都不被串改 |
| 删除主表 | 删除预约单 | 主表和所属明细全部删除 |

### 执行方法

```powershell
Set-Location "KH.WMS.Client"
npx playwright test e2e/training.spec.js
```

需要先启动后端和前端，并准备培训数据库、菜单及测试账号。

### 预期结果

自动化用例通过；测试创建的数据在 `finally` 中清理；不存在孤儿明细。

### 常见错误

- 只断言 HTTP 200，不检查 `data.items`。
- 测试失败后不清理数据，下一次运行遇到唯一索引冲突。
- 级联删除只检查主表 404，不查孤儿明细。

### 验证方法

```sql
SELECT l.Id, l.AppointmentId
FROM dbo.trn_arrival_appointment_line l
LEFT JOIN dbo.trn_arrival_appointment h ON h.Id = l.AppointmentId
WHERE h.Id IS NULL;
```

查询必须返回零行。

## 第 20 步：按现象排查主从问题

### 本步目标

建立从现象到代码位置的排查顺序，减少在前后端之间来回猜测。

### 为什么要做

主从故障通常跨越 Navigate、详情加载、请求组装、ID 语义、事务和菜单路由。按链路排查比随机改代码更快。

### 修改位置

根据下表定位，不先做无关重构。

### 完整排错表

| 现象 | 首查位置 | 常见原因 | 修复方向 |
| --- | --- | --- | --- |
| 页面 404 | `sys_permission.Component` | 路径多斜杠或带 `.vue` | 改成 `training/arrival-appointment` |
| 创建主表成功但从表为空 | Service 构造函数 | 未把 `IDetailSaveService` 传给基类 | 注入并传入基类 |
| 详情 `items` 为空 | 主实体 `Navigate` | 外键名写错或没配置导航 | 核对三个参数 |
| 编辑弹窗没有明细 | `openUpdate` | 直接使用列表行 | 先调详情接口 |
| 编辑后重复插入明细 | 更新请求 | 已有行 ID 被清零 | 回显和提交时保留 ID |
| 删除界面一行后数据库仍存在 | 请求体 | 被删行仍在 `items` 中 | 提交界面当前完整集合 |
| 正常行被误删 | 更新请求 | 只提交变化行 | 改成提交全部保留行 |
| A 单能修改 B 单明细 | `DetailSaveService` | 未校验 ID 归属 | 更新前查询当前主表已有 ID 集合 |
| 创建后明细外键为 0 | `CrudService.CreateAsync` | 主表插入后未回填 `entity.Id` | 保存明细前设置主实体 ID |
| 新增明细响应后 ID 仍为 0 | `InsertByDynamic` | 插入结果未回填对象 | 将 `AddAsync` 返回 ID 写回明细 |
| 删除主表留下孤儿明细 | 删除链路 | 未加载导航或未走级联删除 | 核对 `GetByIdWithNavAsync` 和 `DeleteWithNavAsync` |
| 任一明细失败但主表残留 | 事务链路 | 主表和明细不在同一 UnitOfWork | 统一在 `CrudService` 事务内保存 |
| 提示“至少一条明细” | Service 规则 | 到货预约不允许空集合 | 至少保留一行；其他业务按规则决定 |

### 执行方法

固定按以下顺序收集证据：

```text
1. 浏览器当前 URL 和菜单 Component
2. 请求 URL、方法、请求体
3. 响应 code、message、traceId
4. 详情响应是否含 items 和非零 ID
5. 后端日志中的同一 traceId
6. 主表、从表和孤儿明细 SQL 查询
7. Navigate、Service 注入和事务代码
```

### 预期结果

每个问题都能定位到一个明确层级，并用请求、日志或 SQL 证明修复有效。

### 常见错误

- 页面没显示明细就先改数据库。
- 接口报错后只看 toast，不记录 `traceId`。
- 为修一个主从问题顺手改通用请求封装、Vite 或公共组件配置。

### 验证方法

修复后重跑第 19 步的同一场景，并检查 `git diff` 只包含直接相关改动。

## 最终完整提交示例

当前页面实际采用下面的核心提交方式：

```js
const handleSubmit = async () => {
  if (submitLoading.value || !await formRef.value?.validate()) return

  const error = validateItems()
  if (error) return KhMessageFn.warning(error)

  submitLoading.value = true
  try {
    const isCreate = dialogMode.value === 'create'
    const data = {
      ...formData,
      items: items.value.map(x => toRequestLine(x, isCreate)),
    }

    if (isCreate) {
      data.id = 0
      await crudApi.create(data)
    } else {
      await crudApi.update(data)
    }

    KhMessageFn.success(
      dialogMode.value === 'create' ? '新增成功' : '更新成功'
    )
    dialogVisible.value = false
    pageRef.value?.reload()
  } finally {
    submitLoading.value = false
  }
}
```

后端还会再次校验创建明细 ID 和更新明细归属，不能只依赖前端。

## 交付前检查清单

- [ ] SQL 连续执行两次，培训样例仍为 `3/3/3/5`。
- [ ] 主实体 `Items` 的 Navigate 三个参数正确。
- [ ] Service 注入并传递 `IDetailSaveService`。
- [ ] Swagger 五个标准接口可用。
- [ ] 菜单 `Component` 与 Vue 相对路径一致。
- [ ] `KhPage` 内置 create/update 已关闭。
- [ ] `tableColumns`、`formColumns`、`lineColumns`、`detailLines` 职责没有混用。
- [ ] 新增时所有明细 ID 为 `0`。
- [ ] 编辑回显和提交保留已有明细 ID。
- [ ] 更新提交当前完整明细集合。
- [ ] 后端拒绝跨主表明细 ID。
- [ ] 页面和后端都有校验与防重复提交。
- [ ] 删除主表后孤儿明细查询为零行。
- [ ] 后端、前端、文档构建和主从 E2E 全部通过。

## 继续阅读

- [KH.WMS 前端组件体系与页面开发指引](/backend/KH.WMS前端组件体系与页面开发指引)
- [KH.WMS 前端常用组件详细使用文档](/backend/KH.WMS前端常用组件详细使用文档)
- [CRUD 基类能力详解](/backend/后端开发指引V3教程/07-CRUD基类能力详解)
- [通用开发实战培训](/backend/实战培训/README)
