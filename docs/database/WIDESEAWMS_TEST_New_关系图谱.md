# WIDESEAWMS_TEST_New 关系图谱

> 这是从数据字典中拆出的图谱版，只保留分业务域关系图和 UML 类图，方便阅读和评审。字段、索引、主外键详情请看 WIDESEAWMS_TEST_New_数据字典.md。

## 5 数据库关系图（按业务域拆分）

> 全库 89 张表不再合并成一张图；下面按业务域拆分，每张图只保留表与外键关系，字段详情请看上方表结构明细。

### 5.1 核心业务链路

```mermaid
flowchart LR
    T1["供应商<br/>md_supplier"]
    T2["客户<br/>md_customer"]
    T3["物料<br/>md_material"]
    T4["物料单位<br/>md_material_unit"]
    T5["仓库<br/>md_warehouse"]
    T6["仓库库区<br/>md_warehouse_zone"]
    T7["库位<br/>md_location"]
    T8["容器<br/>md_container"]
    T9["入库单<br/>bd_inbound_order"]
    T10["入库单明细<br/>bd_inbound_order_line"]
    T11["出库单<br/>bd_outbound_order"]
    T12["出库单明细<br/>bd_outbound_order_line"]
    T13["库存主表<br/>inv_inventory_header"]
    T14["库存明细<br/>inv_inventory_detail"]
    T15["作业任务单头<br/>bd_task_header"]
    T16["作业任务明细<br/>bd_task_line"]
    T4 -->|BaseUnitId| T3
    T4 -->|BaseUnitId| T4
    T5 -->|WarehouseId| T6
    T6 -->|ParentZoneId| T6
    T5 -->|WarehouseId| T7
    T6 -->|ZoneId| T7
    T5 -->|WarehouseId| T8
    T5 -->|WarehouseId| T9
    T1 -->|SupplierId| T9
    T9 -->|OrderId| T10
    T3 -->|MaterialId| T10
    T4 -->|UnitId| T10
    T5 -->|WarehouseId| T11
    T2 -->|CustomerId| T11
    T11 -->|OrderId| T12
    T3 -->|MaterialId| T12
    T4 -->|UnitId| T12
    T5 -->|WarehouseId| T13
    T7 -->|LocationId| T13
    T13 -->|HeaderId| T14
    T3 -->|MaterialId| T14
    T1 -->|SupplierId| T14
    T5 -->|WarehouseId| T15
    T6 -->|ZoneId| T15
    T8 -->|ContainerId| T15
    T7 -->|FromLocationId| T15
    T7 -->|ToLocationId| T15
    T15 -->|TaskId| T16
    T3 -->|MaterialId| T16
    T13 -->|InventoryHeaderId| T16
```

### 5.2 基础资料

```mermaid
flowchart LR
    T1["物料<br/>md_material"]
    T2["物料分类<br/>md_material_category"]
    T3["物料单位<br/>md_material_unit"]
    T4["物料周转分析<br/>md_material_turnover"]
    T5["周转等级配置<br/>cfg_turnover_class"]
    T6["供应商<br/>md_supplier"]
    T7["客户<br/>md_customer"]
    T8["容器<br/>md_container"]
    T9["容器类型<br/>md_container_type"]
    T2 -->|CategoryId| T1
    T3 -->|BaseUnitId| T1
    T2 -->|ParentId| T2
    T3 -->|BaseUnitId| T3
    T1 -->|MaterialId| T4
    T9 -->|ContainerTypeId| T8
```

### 5.3 仓库建模

```mermaid
flowchart LR
    T1["仓库<br/>md_warehouse"]
    T2["仓库库区<br/>md_warehouse_zone"]
    T3["巷道<br/>md_aisle"]
    T4["库位<br/>md_location"]
    T5["逻辑库区<br/>md_logical_zone"]
    T6["逻辑库区映射<br/>md_logical_zone_mapping"]
    T7["站台<br/>md_port"]
    T8["接驳点<br/>md_transfer_point"]
    T9["输送线<br/>md_conveyor_line"]
    T1 -->|WarehouseId| T2
    T2 -->|ParentZoneId| T2
    T1 -->|WarehouseId| T3
    T2 -->|ZoneId| T3
    T1 -->|WarehouseId| T4
    T2 -->|ZoneId| T4
    T1 -->|WarehouseId| T5
    T5 -->|LogicalZoneId| T6
    T1 -->|WarehouseId| T7
    T2 -->|ZoneId| T7
    T1 -->|WarehouseId| T8
    T2 -->|ZoneId| T8
    T3 -->|AisleId| T8
    T1 -->|WarehouseId| T9
    T2 -->|ZoneId| T9
```

### 5.4 入库业务

```mermaid
flowchart LR
    T1["入库单<br/>bd_inbound_order"]
    T2["入库单明细<br/>bd_inbound_order_line"]
    T3["入库容器绑定单头<br/>bd_inbound_container_bind_header"]
    T4["入库容器绑定明细<br/>bd_inbound_container_bind_detail"]
    T5["供应商<br/>md_supplier"]
    T6["物料<br/>md_material"]
    T7["物料单位<br/>md_material_unit"]
    T8["仓库<br/>md_warehouse"]
    T9["容器<br/>md_container"]
    T8 -->|WarehouseId| T1
    T5 -->|SupplierId| T1
    T1 -->|OrderId| T2
    T6 -->|MaterialId| T2
    T7 -->|UnitId| T2
    T8 -->|WarehouseId| T3
    T1 -->|InboundOrderId| T3
    T3 -->|HeaderId| T4
    T2 -->|InboundOrderLineId| T4
    T6 -->|MaterialId| T4
    T7 -->|UnitId| T4
    T7 -->|BaseUnitId| T6
    T7 -->|BaseUnitId| T7
    T8 -->|WarehouseId| T9
```

### 5.5 出库业务

```mermaid
flowchart LR
    T1["出库单<br/>bd_outbound_order"]
    T2["出库单明细<br/>bd_outbound_order_line"]
    T3["出库分配单头<br/>bd_outbound_allocation_header"]
    T4["出库分配单明细<br/>bd_outbound_allocation_detail"]
    T5["波次<br/>out_wave"]
    T6["波次明细<br/>out_wave_line"]
    T7["客户<br/>md_customer"]
    T8["物料<br/>md_material"]
    T9["仓库<br/>md_warehouse"]
    T9 -->|WarehouseId| T1
    T7 -->|CustomerId| T1
    T1 -->|OrderId| T2
    T8 -->|MaterialId| T2
    T1 -->|OutboundOrderId| T3
    T9 -->|WarehouseId| T3
    T3 -->|HeaderId| T4
    T8 -->|MaterialId| T4
    T9 -->|WarehouseId| T5
```

### 5.6 库存业务

```mermaid
flowchart LR
    T1["库存主表<br/>inv_inventory_header"]
    T2["库存明细<br/>inv_inventory_detail"]
    T3["库存移动记录<br/>inv_movement"]
    T4["库存调整单<br/>inv_adjust"]
    T5["库存调整明细<br/>inv_adjust_line"]
    T6["库存冻结记录<br/>inv_freeze_record"]
    T7["库存盘点单<br/>inv_stocktake"]
    T8["库存盘点明细<br/>inv_stocktake_detail"]
    T9["库存快照头<br/>inv_snapshot_header"]
    T10["库存快照<br/>inv_snapshot"]
    T11["库存预警记录<br/>inv_alert_record"]
    T12["库存预警配置<br/>cfg_inventory_alert"]
    T1 -->|HeaderId| T2
    T7 -->|StocktakeId| T8
    T9 -->|SnapshotHeaderId| T10
    T7 -->|StocktakeId| T10
```

### 5.7 任务与搬运

```mermaid
flowchart LR
    T1["作业任务单头<br/>bd_task_header"]
    T2["作业任务明细<br/>bd_task_line"]
    T3["任务确认<br/>bd_task_confirm"]
    T4["搬运计划<br/>bd_transfer_plan"]
    T5["搬运计划明细<br/>bd_transfer_plan_line"]
    T6["WCS任务日志<br/>bd_wcs_task_log"]
    T7["库位<br/>md_location"]
    T8["站台<br/>md_port"]
    T9["接驳点<br/>md_transfer_point"]
    T7 -->|FromLocationId| T1
    T7 -->|ToLocationId| T1
    T1 -->|TaskId| T2
    T1 -->|TaskHeaderId| T3
    T4 -->|PlanId| T5
    T7 -->|FromLocationId| T5
    T7 -->|ToLocationId| T5
    T1 -->|TaskHeaderId| T5
    T1 -->|TaskId| T6
```

### 5.8 系统权限

```mermaid
flowchart LR
    T1["用户<br/>sys_user"]
    T2["角色<br/>sys_role"]
    T3["用户角色<br/>sys_user_role"]
    T4["权限<br/>sys_permission"]
    T5["角色权限<br/>sys_role_permission"]
    T6["字典类型<br/>sys_dict_type"]
    T7["字典项<br/>sys_dict_item"]
    T8["系统参数<br/>sys_parameter"]
    T9["附件<br/>sys_attachment"]
    T10["操作日志<br/>sys_operate_log"]
    T11["系统关系<br/>sys_relation"]
    T2 -->|ParentId| T2
    T1 -->|UserId| T3
    T2 -->|RoleId| T3
    T4 -->|ParentId| T4
    T2 -->|RoleId| T5
    T4 -->|PermissionId| T5
```

### 5.9 配置与策略

```mermaid
flowchart LR
    T1["单据类型配置<br/>cfg_document_type"]
    T2["单据状态配置<br/>cfg_document_status"]
    T3["单据字段配置<br/>cfg_document_field"]
    T4["单据类型规则<br/>cfg_document_type_rule"]
    T5["单据类型流程配置<br/>cfg_document_type_process"]
    T6["单据类型站台配置<br/>cfg_doc_type_port"]
    T7["扩展字段类型配置<br/>cfg_ext_field_type"]
    T8["扩展字段配置<br/>cfg_ext_field"]
    T9["编码规则<br/>cfg_code_rule"]
    T10["编码流水<br/>cfg_code_sequence"]
    T11["策略配置<br/>cfg_strategy_config"]
    T12["策略链配置<br/>cfg_strategy_chain_config"]
    T13["策略链步骤<br/>cfg_strategy_chain_step"]
    T14["全局配置<br/>cfg_global_config"]
    T1 -->|DocTypeId| T2
    T1 -->|DocTypeId| T3
    T1 -->|DocTypeId| T4
    T1 -->|DocTypeId| T5
    T1 -->|DocTypeId| T6
    T7 -->|EntityTypeId| T8
    T9 -->|RuleId| T10
    T12 -->|ChainId| T13
    T11 -->|StrategyConfigId| T13
```

### 5.10 接口与日志

```mermaid
flowchart LR
    T1["接口配置<br/>int_interface_config"]
    T2["接口日志<br/>int_interface_log"]
    T3["接口数据映射配置<br/>int_data_mapping_config"]
    T4["同步日志<br/>int_sync_log"]
    T5["同步日志明细<br/>int_sync_log_detail"]
    T6["定时任务执行日志<br/>log_job_execution"]
    T7["编码生成记录<br/>log_code_record"]
    T8["异常单<br/>exc_exception"]
    T9["异常处理日志<br/>exc_exception_log"]
```

## 6 UML 类图（按业务域拆分）

> UML 图按业务域拆分，并只展示 Id 与外键字段，避免把普通业务字段全部画进图里导致缩放后不可读。

### 6.1 核心业务链路

```mermaid
classDiagram
    class MdSupplier {
        +bigint Id
    }
    class MdCustomer {
        +bigint Id
    }
    class MdMaterial {
        +bigint Id
        +bigint CategoryId
        +bigint BaseUnitId
    }
    class MdMaterialUnit {
        +bigint Id
        +bigint BaseUnitId
    }
    class MdWarehouse {
        +bigint Id
    }
    class MdWarehouseZoneDTO {
        +bigint Id
        +bigint WarehouseId
        +bigint ParentZoneId
    }
    class MdLocationDTO {
        +bigint Id
        +bigint WarehouseId
        +bigint ZoneId
    }
    class MdContainer {
        +bigint Id
        +bigint ContainerTypeId
        +bigint WarehouseId
    }
    class InboundOrder {
        +bigint Id
        +bigint WarehouseId
        +bigint SupplierId
    }
    class InboundOrderLine {
        +bigint Id
        +bigint OrderId
        +bigint MaterialId
        +bigint UnitId
    }
    class OutboundOrder {
        +bigint Id
        +bigint WarehouseId
        +bigint CustomerId
    }
    class OutboundOrderLine {
        +bigint Id
        +bigint OrderId
        +bigint MaterialId
        +bigint UnitId
    }
    class InvInventoryHeaderDTO {
        +bigint Id
        +bigint WarehouseId
        +bigint LocationId
    }
    class InvInventoryDetailDTO {
        +bigint Id
        +bigint HeaderId
        +bigint MaterialId
        +bigint SupplierId
    }
    class TaskHeader {
        +bigint Id
        +bigint WarehouseId
        +bigint ZoneId
        +bigint ContainerId
        +bigint FromLocationId
        +bigint ToLocationId
    }
    class TaskLine {
        +bigint Id
        +bigint TaskId
        +bigint MaterialId
        +bigint InventoryHeaderId
    }
    MdMaterialUnit "1" --> "*" MdMaterial : BaseUnitId
    MdMaterialUnit "1" --> "*" MdMaterialUnit : BaseUnitId
    MdWarehouse "1" --> "*" MdWarehouseZoneDTO : WarehouseId
    MdWarehouseZoneDTO "1" --> "*" MdWarehouseZoneDTO : ParentZoneId
    MdWarehouse "1" --> "*" MdLocationDTO : WarehouseId
    MdWarehouseZoneDTO "1" --> "*" MdLocationDTO : ZoneId
    MdWarehouse "1" --> "*" MdContainer : WarehouseId
    MdWarehouse "1" --> "*" InboundOrder : WarehouseId
    MdSupplier "1" --> "*" InboundOrder : SupplierId
    InboundOrder "1" --> "*" InboundOrderLine : OrderId
    MdMaterial "1" --> "*" InboundOrderLine : MaterialId
    MdMaterialUnit "1" --> "*" InboundOrderLine : UnitId
    MdWarehouse "1" --> "*" OutboundOrder : WarehouseId
    MdCustomer "1" --> "*" OutboundOrder : CustomerId
    OutboundOrder "1" --> "*" OutboundOrderLine : OrderId
    MdMaterial "1" --> "*" OutboundOrderLine : MaterialId
    MdMaterialUnit "1" --> "*" OutboundOrderLine : UnitId
    MdWarehouse "1" --> "*" InvInventoryHeaderDTO : WarehouseId
    MdLocationDTO "1" --> "*" InvInventoryHeaderDTO : LocationId
    InvInventoryHeaderDTO "1" --> "*" InvInventoryDetailDTO : HeaderId
    MdMaterial "1" --> "*" InvInventoryDetailDTO : MaterialId
    MdSupplier "1" --> "*" InvInventoryDetailDTO : SupplierId
    MdWarehouse "1" --> "*" TaskHeader : WarehouseId
    MdWarehouseZoneDTO "1" --> "*" TaskHeader : ZoneId
    MdContainer "1" --> "*" TaskHeader : ContainerId
    MdLocationDTO "1" --> "*" TaskHeader : FromLocationId
    MdLocationDTO "1" --> "*" TaskHeader : ToLocationId
    TaskHeader "1" --> "*" TaskLine : TaskId
    MdMaterial "1" --> "*" TaskLine : MaterialId
    InvInventoryHeaderDTO "1" --> "*" TaskLine : InventoryHeaderId
```

### 6.2 基础资料

```mermaid
classDiagram
    class MdMaterial {
        +bigint Id
        +bigint CategoryId
        +bigint BaseUnitId
    }
    class MdMaterialCategory {
        +bigint Id
        +bigint ParentId
    }
    class MdMaterialUnit {
        +bigint Id
        +bigint BaseUnitId
    }
    class MdMaterialTurnoverDTO {
        +bigint Id
        +bigint MaterialId
    }
    class CfgTurnoverClass {
        +bigint Id
    }
    class MdSupplier {
        +bigint Id
    }
    class MdCustomer {
        +bigint Id
    }
    class MdContainer {
        +bigint Id
        +bigint ContainerTypeId
        +bigint WarehouseId
    }
    class MdContainerType {
        +bigint Id
    }
    MdMaterialCategory "1" --> "*" MdMaterial : CategoryId
    MdMaterialUnit "1" --> "*" MdMaterial : BaseUnitId
    MdMaterialCategory "1" --> "*" MdMaterialCategory : ParentId
    MdMaterialUnit "1" --> "*" MdMaterialUnit : BaseUnitId
    MdMaterial "1" --> "*" MdMaterialTurnoverDTO : MaterialId
    MdContainerType "1" --> "*" MdContainer : ContainerTypeId
```

### 6.3 仓库建模

```mermaid
classDiagram
    class MdWarehouse {
        +bigint Id
    }
    class MdWarehouseZoneDTO {
        +bigint Id
        +bigint WarehouseId
        +bigint ParentZoneId
    }
    class MdAisleDTO {
        +bigint Id
        +bigint WarehouseId
        +bigint ZoneId
    }
    class MdLocationDTO {
        +bigint Id
        +bigint WarehouseId
        +bigint ZoneId
    }
    class MdLogicalZone {
        +bigint Id
        +bigint WarehouseId
    }
    class MdLogicalZoneMapping {
        +bigint Id
        +bigint LogicalZoneId
    }
    class MdPortDTO {
        +bigint Id
        +bigint WarehouseId
        +bigint ZoneId
    }
    class MdTransferPointDTO {
        +bigint Id
        +bigint WarehouseId
        +bigint ZoneId
        +bigint AisleId
    }
    class MdConveyorLine {
        +bigint Id
        +bigint WarehouseId
        +bigint ZoneId
    }
    MdWarehouse "1" --> "*" MdWarehouseZoneDTO : WarehouseId
    MdWarehouseZoneDTO "1" --> "*" MdWarehouseZoneDTO : ParentZoneId
    MdWarehouse "1" --> "*" MdAisleDTO : WarehouseId
    MdWarehouseZoneDTO "1" --> "*" MdAisleDTO : ZoneId
    MdWarehouse "1" --> "*" MdLocationDTO : WarehouseId
    MdWarehouseZoneDTO "1" --> "*" MdLocationDTO : ZoneId
    MdWarehouse "1" --> "*" MdLogicalZone : WarehouseId
    MdLogicalZone "1" --> "*" MdLogicalZoneMapping : LogicalZoneId
    MdWarehouse "1" --> "*" MdPortDTO : WarehouseId
    MdWarehouseZoneDTO "1" --> "*" MdPortDTO : ZoneId
    MdWarehouse "1" --> "*" MdTransferPointDTO : WarehouseId
    MdWarehouseZoneDTO "1" --> "*" MdTransferPointDTO : ZoneId
    MdAisleDTO "1" --> "*" MdTransferPointDTO : AisleId
    MdWarehouse "1" --> "*" MdConveyorLine : WarehouseId
    MdWarehouseZoneDTO "1" --> "*" MdConveyorLine : ZoneId
```

### 6.4 入库业务

```mermaid
classDiagram
    class InboundOrder {
        +bigint Id
        +bigint WarehouseId
        +bigint SupplierId
    }
    class InboundOrderLine {
        +bigint Id
        +bigint OrderId
        +bigint MaterialId
        +bigint UnitId
    }
    class InboundContainerBindHeader {
        +bigint Id
        +bigint WarehouseId
        +bigint InboundOrderId
    }
    class InboundContainerBindDetail {
        +bigint Id
        +bigint HeaderId
        +bigint InboundOrderLineId
        +bigint MaterialId
        +bigint UnitId
    }
    class MdSupplier {
        +bigint Id
    }
    class MdMaterial {
        +bigint Id
        +bigint CategoryId
        +bigint BaseUnitId
    }
    class MdMaterialUnit {
        +bigint Id
        +bigint BaseUnitId
    }
    class MdWarehouse {
        +bigint Id
    }
    class MdContainer {
        +bigint Id
        +bigint ContainerTypeId
        +bigint WarehouseId
    }
    MdWarehouse "1" --> "*" InboundOrder : WarehouseId
    MdSupplier "1" --> "*" InboundOrder : SupplierId
    InboundOrder "1" --> "*" InboundOrderLine : OrderId
    MdMaterial "1" --> "*" InboundOrderLine : MaterialId
    MdMaterialUnit "1" --> "*" InboundOrderLine : UnitId
    MdWarehouse "1" --> "*" InboundContainerBindHeader : WarehouseId
    InboundOrder "1" --> "*" InboundContainerBindHeader : InboundOrderId
    InboundContainerBindHeader "1" --> "*" InboundContainerBindDetail : HeaderId
    InboundOrderLine "1" --> "*" InboundContainerBindDetail : InboundOrderLineId
    MdMaterial "1" --> "*" InboundContainerBindDetail : MaterialId
    MdMaterialUnit "1" --> "*" InboundContainerBindDetail : UnitId
    MdMaterialUnit "1" --> "*" MdMaterial : BaseUnitId
    MdMaterialUnit "1" --> "*" MdMaterialUnit : BaseUnitId
    MdWarehouse "1" --> "*" MdContainer : WarehouseId
```

### 6.5 出库业务

```mermaid
classDiagram
    class OutboundOrder {
        +bigint Id
        +bigint WarehouseId
        +bigint CustomerId
    }
    class OutboundOrderLine {
        +bigint Id
        +bigint OrderId
        +bigint MaterialId
        +bigint UnitId
    }
    class OutboundAllocationHeader {
        +bigint Id
        +bigint OutboundOrderId
        +bigint WarehouseId
    }
    class OutboundAllocationDetail {
        +bigint Id
        +bigint HeaderId
        +bigint MaterialId
        +bigint InventoryHeaderId
        +bigint ContainerId
        +bigint LocationId
        +bigint TaskHeaderId
    }
    class OutWave {
        +bigint Id
        +bigint WarehouseId
    }
    class OutWaveLine {
        +bigint Id
    }
    class MdCustomer {
        +bigint Id
    }
    class MdMaterial {
        +bigint Id
        +bigint CategoryId
        +bigint BaseUnitId
    }
    class MdWarehouse {
        +bigint Id
    }
    MdWarehouse "1" --> "*" OutboundOrder : WarehouseId
    MdCustomer "1" --> "*" OutboundOrder : CustomerId
    OutboundOrder "1" --> "*" OutboundOrderLine : OrderId
    MdMaterial "1" --> "*" OutboundOrderLine : MaterialId
    OutboundOrder "1" --> "*" OutboundAllocationHeader : OutboundOrderId
    MdWarehouse "1" --> "*" OutboundAllocationHeader : WarehouseId
    OutboundAllocationHeader "1" --> "*" OutboundAllocationDetail : HeaderId
    MdMaterial "1" --> "*" OutboundAllocationDetail : MaterialId
    MdWarehouse "1" --> "*" OutWave : WarehouseId
```

### 6.6 库存业务

```mermaid
classDiagram
    class InvInventoryHeaderDTO {
        +bigint Id
        +bigint WarehouseId
        +bigint LocationId
    }
    class InvInventoryDetailDTO {
        +bigint Id
        +bigint HeaderId
        +bigint MaterialId
        +bigint SupplierId
    }
    class InvMovement {
        +bigint Id
        +bigint WarehouseId
        +bigint MaterialId
        +bigint LocationId
    }
    class InvAdjust {
        +bigint Id
        +bigint WarehouseId
    }
    class InvAdjustLine {
        +bigint Id
        +bigint LocationId
        +bigint MaterialId
        +bigint ContainerId
    }
    class InvFreezeRecord {
        +bigint Id
        +bigint WarehouseId
        +bigint MaterialId
        +bigint LocationId
    }
    class InvStocktake {
        +bigint Id
        +bigint WarehouseId
        +bigint ZoneId
    }
    class InvStocktakeDetail {
        +bigint Id
        +bigint StocktakeId
        +bigint LocationId
        +bigint MaterialId
        +bigint ContainerId
    }
    class InvSnapshotHeader {
        +bigint Id
    }
    class InvSnapshot {
        +bigint Id
        +bigint StocktakeId
        +bigint WarehouseId
        +bigint LocationId
        +bigint MaterialId
        +bigint ContainerId
        +bigint SnapshotHeaderId
    }
    class InvAlertRecord {
        +bigint Id
        +bigint WarehouseId
        +bigint ZoneId
        +bigint LocationId
        +bigint MaterialId
    }
    class CfgInventoryAlert {
        +bigint Id
        +bigint WarehouseId
        +bigint ZoneId
        +bigint MaterialId
    }
    InvInventoryHeaderDTO "1" --> "*" InvInventoryDetailDTO : HeaderId
    InvStocktake "1" --> "*" InvStocktakeDetail : StocktakeId
    InvSnapshotHeader "1" --> "*" InvSnapshot : SnapshotHeaderId
    InvStocktake "1" --> "*" InvSnapshot : StocktakeId
```

### 6.7 任务与搬运

```mermaid
classDiagram
    class TaskHeader {
        +bigint Id
        +bigint WarehouseId
        +bigint ZoneId
        +bigint ContainerId
        +bigint FromLocationId
        +bigint ToLocationId
    }
    class TaskLine {
        +bigint Id
        +bigint TaskId
        +bigint MaterialId
        +bigint InventoryHeaderId
    }
    class TaskConfirm {
        +bigint Id
        +bigint TaskHeaderId
    }
    class TransferPlan {
        +bigint Id
        +bigint WarehouseId
    }
    class TransferPlanLine {
        +bigint Id
        +bigint PlanId
        +bigint MaterialId
        +bigint ContainerId
        +bigint FromLocationId
        +bigint ToLocationId
        +bigint TaskHeaderId
    }
    class WcsTaskLog {
        +bigint Id
        +bigint TaskId
    }
    class MdLocationDTO {
        +bigint Id
        +bigint WarehouseId
        +bigint ZoneId
    }
    class MdPortDTO {
        +bigint Id
        +bigint WarehouseId
        +bigint ZoneId
    }
    class MdTransferPointDTO {
        +bigint Id
        +bigint WarehouseId
        +bigint ZoneId
        +bigint AisleId
    }
    MdLocationDTO "1" --> "*" TaskHeader : FromLocationId
    MdLocationDTO "1" --> "*" TaskHeader : ToLocationId
    TaskHeader "1" --> "*" TaskLine : TaskId
    TaskHeader "1" --> "*" TaskConfirm : TaskHeaderId
    TransferPlan "1" --> "*" TransferPlanLine : PlanId
    MdLocationDTO "1" --> "*" TransferPlanLine : FromLocationId
    MdLocationDTO "1" --> "*" TransferPlanLine : ToLocationId
    TaskHeader "1" --> "*" TransferPlanLine : TaskHeaderId
    TaskHeader "1" --> "*" WcsTaskLog : TaskId
```

### 6.8 系统权限

```mermaid
classDiagram
    class SysUser {
        +bigint Id
    }
    class SysRole {
        +bigint Id
        +bigint ParentId
    }
    class SysUserRole {
        +bigint Id
        +bigint UserId
        +bigint RoleId
    }
    class SysPermission {
        +bigint Id
        +bigint ParentId
    }
    class SysRolePermission {
        +bigint Id
        +bigint RoleId
        +bigint PermissionId
    }
    class SysDictType {
        +bigint Id
    }
    class SysDictItem {
        +bigint Id
    }
    class SysParameter {
        +bigint Id
    }
    class SysAttachment {
        +bigint Id
    }
    class SysOperateLog {
        +bigint Id
    }
    class SysRelation {
        +bigint Id
    }
    SysRole "1" --> "*" SysRole : ParentId
    SysUser "1" --> "*" SysUserRole : UserId
    SysRole "1" --> "*" SysUserRole : RoleId
    SysPermission "1" --> "*" SysPermission : ParentId
    SysRole "1" --> "*" SysRolePermission : RoleId
    SysPermission "1" --> "*" SysRolePermission : PermissionId
```

### 6.9 配置与策略

```mermaid
classDiagram
    class CfgDocumentType {
        +bigint Id
    }
    class CfgDocumentStatus {
        +bigint Id
        +bigint DocTypeId
    }
    class CfgDocumentField {
        +bigint Id
        +bigint DocTypeId
    }
    class CfgDocumentTypeRule {
        +bigint Id
        +bigint DocTypeId
    }
    class CfgDocumentTypeProcess {
        +bigint Id
        +bigint DocTypeId
    }
    class CfgDocTypePortDTO {
        +bigint Id
        +bigint DocTypeId
        +bigint PortId
        +bigint ZoneId
    }
    class CfgExtFieldType {
        +bigint Id
    }
    class CfgExtField {
        +bigint Id
        +bigint EntityTypeId
    }
    class CfgCodeRule {
        +bigint Id
    }
    class CfgCodeSequence {
        +bigint Id
        +bigint RuleId
    }
    class CfgStrategyConfig {
        +bigint Id
        +bigint WarehouseId
        +bigint ZoneId
        +bigint MaterialId
        +bigint MaterialCategoryId
    }
    class CfgStrategyChainConfig {
        +bigint Id
        +bigint WarehouseId
        +bigint ZoneId
    }
    class CfgStrategyChainStep {
        +bigint Id
        +bigint ChainId
        +bigint StrategyConfigId
    }
    class CfgGlobalConfig {
        +bigint Id
    }
    CfgDocumentType "1" --> "*" CfgDocumentStatus : DocTypeId
    CfgDocumentType "1" --> "*" CfgDocumentField : DocTypeId
    CfgDocumentType "1" --> "*" CfgDocumentTypeRule : DocTypeId
    CfgDocumentType "1" --> "*" CfgDocumentTypeProcess : DocTypeId
    CfgDocumentType "1" --> "*" CfgDocTypePortDTO : DocTypeId
    CfgExtFieldType "1" --> "*" CfgExtField : EntityTypeId
    CfgCodeRule "1" --> "*" CfgCodeSequence : RuleId
    CfgStrategyChainConfig "1" --> "*" CfgStrategyChainStep : ChainId
    CfgStrategyConfig "1" --> "*" CfgStrategyChainStep : StrategyConfigId
```

### 6.10 接口与日志

```mermaid
classDiagram
    class int_interface_config {
        +bigint Id
    }
    class int_interface_log {
        +bigint Id
    }
    class int_data_mapping_config {
        +bigint Id
    }
    class int_sync_log {
        +bigint Id
    }
    class int_sync_log_detail {
        +bigint Id
    }
    class LogJobExecution {
        +bigint Id
        +bigint JobId
    }
    class LogCodeRecord {
        +bigint Id
        +bigint RuleId
    }
    class ExcException {
        +bigint Id
        +bigint WarehouseId
        +bigint LocationId
    }
    class ExcExceptionLog {
        +bigint Id
    }
```
