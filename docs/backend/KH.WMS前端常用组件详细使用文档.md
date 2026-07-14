---
title: "KH.WMS 前端常用组件详细使用文档"
description: "KH.WMS 前端常用组件详细使用文档：说明适用场景、当前实现、设计边界与开发或排障入口。"
status: current
audience: "前端开发人员与联调负责人"
reviewed: "2026-07-14"
sourcePaths:
  - "KH.WMS.Client/src"
---



# KH.WMS 前端常用组件详细使用文档

> 本文面向 `KH.WMS.Client` 前端开发，说明 `src/components/KhXxx` 公共组件的定位、常用 props、事件、插槽、暴露方法、典型写法和页面开发场景。  
> 代码依据当前仓库实现整理，重点覆盖页面中使用最多的 `KhPage`。

## 目录

- [1. 组件总览](#1-组件总览)
- [2. KhPage 页面级 CRUD 外壳](#2-khpage-页面级-crud-外壳)
- [3. KhTable 通用业务表格](#3-khtable-通用业务表格)
- [4. KhForm 配置式表单](#4-khform-配置式表单)
- [5. KhDialog 通用弹窗](#5-khdialog-通用弹窗)
- [6. KhDetailDialog 详情弹窗](#6-khdetaildialog-详情弹窗)
- [7. KhStatCard 指标卡片](#7-khstatcard-指标卡片)
- [8. KhUpload 上传组件](#8-khupload-上传组件)
- [9. KhMessage / KhMsgBox / KhNotify 反馈组件](#9-khmessage--khmsgbox--khnotify-反馈组件)
- [10. KhLayout / KhMenu / KhFullscreen / KhNotification 页面外壳组件](#10-khlayout--khmenu--khfullscreen--khnotification-页面外壳组件)
- [11. KhDashboard 看板组件](#11-khdashboard-看板组件)
- [12. KhEditableTable 可编辑表格](#12-kheditabletable-可编辑表格)
- [13. 低频但可复用组件速查](#13-低频但可复用组件速查)
- [14. 按开发场景组织组件](#14-按开发场景组织组件)
- [15. 新页面开发检查清单](#15-新页面开发检查清单)
- [16. 推荐真实页面参考](#16-推荐真实页面参考)
- [17. 全量组件深度说明与问题清单](#17-全量组件深度说明与问题清单)

## 1. 组件总览

公共组件目录：

```text
KH.WMS.Client/src/components
```

统一组织方式：

```text
KhXxx/index.vue
```

统一导出文件：

```text
KH.WMS.Client/src/components/index.js
```

当前页面使用频率大致如下，说明 `KhPage` 是列表页的主入口，`KhDialog`、`KhForm`、`KhTable` 多数时候由 `KhPage` 内部组合使用：

| 组件 | 页面/组件中使用次数 | 优先掌握程度 |
| --- | ---: | --- |
| `KhPage` | 76 | 必须掌握 |
| `KhDialog` | 45 | 必须掌握 |
| `KhStatCard` | 18 | 高频 |
| `KhForm` | 14 | 必须掌握 |
| `KhTable` | 5 | 必须掌握 |
| `KhEditableTable` | 5 | 高频 |
| `KhLayout` | 3 | 布局维护必备 |
| `KhUpload` | 1 | 附件/导入场景 |
| `KhDashboard` | 1 | 看板场景 |

### 组件选择原则

| 场景 | 首选组件 | 说明 |
| --- | --- | --- |
| 标准增删改查列表页 | `KhPage` | 搜索、表格、分页、工具栏、新增编辑弹窗、详情弹窗一体化 |
| 只有表格，无内置 CRUD | `KhTable` | 自己控制数据加载、按钮和弹窗 |
| 弹窗内配置式表单 | `KhDialog + formColumns` | 最少模板完成新增/编辑 |
| 独立表单区域 | `KhForm` | 适合搜索区、配置表单、流程表单 |
| 只读详情 | `KhDetailDialog` | 字段详情、从表明细 |
| 页面指标区 | `KhStatCard` 或 `KhPage.statCards` | 单卡或 `KhPage` 顶部统计 |
| 上传附件/导入文件 | `KhUpload` | 统一大小、数量、类型校验 |
| 操作反馈 | `KhMessageFn` / `KhMsgBoxFn` / `KhNotifyFn` | 不直接调用 Element Plus 原始 API |
| PC 后台整体外壳 | `KhLayout` | 菜单、头部、标签页、主内容区域 |
| 看板 | `KhDashboard` | 监控/统计类页面 |
| 明细行可编辑 | `KhEditableTable` | 入库、盘点、组盘等行编辑场景 |

## 2. KhPage 页面级 CRUD 外壳

`KhPage` 是当前后台页面最重要的公共组件。它不是简单容器，而是一个页面级组合组件：

```text
KhPage
├─ KhStatCard      顶部统计卡片，可选
├─ KhForm          查询表单，可折叠
├─ KhTable         表格、分页、工具栏、操作列
├─ KhDialog        内置新增/编辑弹窗，传 module 后启用
└─ KhDetailDialog  内置详情弹窗，crudOperations.view=true 时启用
```

### 2.1 什么时候使用 KhPage

优先使用 `KhPage`：

- 页面是列表 + 查询 + 分页 + 操作列。
- 页面能抽象成 `searchColumns`、`columns`、`formColumns` 三组配置。
- 后端接口符合 `useCrudApi(module)` 约定。
- 页面只是在标准 CRUD 上追加少量按钮、钩子、插槽或自定义加载。

谨慎使用 `KhPage`：

- 页面是重交互工作台，比如拖拽、图形化库位图、实时设备监控。
- 表格不是主内容。
- 查询、表格、弹窗之间存在复杂分屏或嵌套布局。
- 表单提交流程不是一次提交，而是多步骤审批、扫码确认、临时保存等。

这类场景可以退回 `KhForm + KhTable + KhDialog` 自行组合。

### 2.2 最小可用示例

```vue
<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage
      ref="pageRef"
      module="brand"
      title="品牌管理"
      :search-columns="searchColumns"
      :search-model="searchModel"
      :columns="tableColumns"
      :form-columns="formColumns"
      permission-prefix="bd:brand"
    />
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'

const pageRef = ref(null)

const searchModel = reactive({
  brandCode: '',
  brandName: '',
  status: '',
})

const searchColumns = [
  { prop: 'brandCode', label: '品牌编码', type: 'input', clearable: true },
  { prop: 'brandName', label: '品牌名称', type: 'input', clearable: true },
  { prop: 'status', label: '状态', type: 'select', clearable: true, options: 'dict:status_flag' },
]

const tableColumns = [
  { prop: 'brandCode', label: '品牌编码', width: 130 },
  { prop: 'brandName', label: '品牌名称', minWidth: 160 },
  { prop: 'status', label: '状态', width: 90, type: 'tag', tagMap: 'dict:status_flag' },
  { prop: 'remark', label: '备注', minWidth: 160, showOverflowTooltip: true },
]

const formColumns = [
  { prop: 'brandCode', label: '品牌编码', type: 'input', required: true, maxlength: 20 },
  { prop: 'brandName', label: '品牌名称', type: 'input', required: true, maxlength: 100 },
  { prop: 'status', label: '状态', type: 'switch', activeValue: 1, inactiveValue: 0 },
  { prop: 'remark', label: '备注', type: 'textarea', rows: 3, maxlength: 200 },
]
</script>
```

### 2.3 核心 props

| Prop | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `title` | `String` | `''` | 页面标题，也用于内置弹窗标题和 `KhTable` 标题 |
| `module` | `String` | `''` | 模块名。传入后启用内置 CRUD，例如 `material` 对应 `/api/material/...` |
| `searchColumns` | `Array` | `[]` | 查询表单字段配置，传给 `KhForm` |
| `searchModel` | `Object` | `{}` | 查询条件对象，会作为 `KhTable.extraParams` |
| `columns` | `Array` | 必填 | 表格列配置，传给 `KhTable` |
| `formColumns` | `Array` | `[]` | 新增/编辑弹窗字段配置，传给内置 `KhDialog` |
| `customFormData` | `Object` | `undefined` | 新增弹窗初始值 |
| `load` | `Function` | `null` | 自定义表格加载函数。优先级高于内置 `module` 加载 |
| `statCards` | `Array` | `[]` | 顶部统计卡片配置 |
| `showStatCards` | `Boolean` | `true` | 是否显示统计卡片区域 |
| `statSpan` | `Number` | `6` | 源码已声明，但当前 `KhPage` 模板未使用；不要依赖它调整统计卡宽度 |
| `showSearch` | `Boolean` | `true` | 是否显示搜索区 |
| `showToolbar` | `Boolean` | `true` | 是否显示表格工具栏 |
| `showPagination` | `Boolean` | `true` | 是否显示分页 |
| `showIndex` | `Boolean` | `true` | 是否显示序号列 |
| `showSelection` | `Boolean` | `true` | 是否显示多选列 |
| `searchColCount` | `Number` | `4` | 搜索表单每行列数 |
| `collapsible` | `Boolean` | `true` | 搜索表单是否可折叠 |
| `defaultCollapsed` | `Boolean` | `true` | 搜索表单默认是否收起 |
| `quickSearchPlaceholder` | `String` | `''` | 收起态快速搜索占位文字 |
| `dialogWidth` | `String | Number` | `'800px'` | 内置新增/编辑弹窗宽度 |
| `dialogColCount` | `Number` | `2` | 内置新增/编辑弹窗表单列数 |
| `detailWidth` | `String | Number` | `'900px'` | 内置详情弹窗宽度 |
| `detailLines` | `Array` | `[]` | 详情弹窗从表配置 |
| `permissionPrefix` | `String` | `''` | 权限码前缀，默认使用 `module` |
| `permissionMap` | `Object` | 见下文 | 内置 CRUD 按钮权限动作名映射 |
| `crudOperations` | `Object` | 见下文 | 内置新增、编辑、删除、查看、导出开关 |
| `createLabel` | `String` | `'新增'` | 内置新增按钮文案 |
| `createIcon` | `Object | null` | `null` | 内置新增按钮图标 |
| `beforeCreate` | `Function` | `null` | 新增前钩子，返回 `false` 阻止打开弹窗 |
| `updateShow` | `Function` | `null` | 控制内置编辑按钮是否显示 |
| `beforeUpdate` | `Function` | `null` | 编辑前钩子，返回 `false` 阻止打开弹窗 |
| `deleteShow` | `Function` | `null` | 控制内置删除按钮是否显示 |
| `beforeDelete` | `Function` | `null` | 删除前钩子，返回 `false` 阻止删除 |
| `deleteConfirmText` | `String` | `'确定删除该数据?'` | 内置删除确认文案 |
| `beforeSubmit` | `Function` | `null` | 新增/编辑提交前钩子，返回 `false` 阻止提交 |
| `afterSubmit` | `Function` | `null` | 新增/编辑提交成功后钩子 |
| `detailWidth` | `String | Number` | `'900px'` | 内置详情弹窗宽度 |
| `detailLines` | `Array` | `[]` | 详情弹窗从表配置 |
| `toolbarButtons` | `Array` | `[]` | 追加工具栏按钮，和内置按钮合并 |
| `actionButtons` | `Array` | `[]` | 追加操作列按钮，和内置按钮合并 |

### 2.3.1 KhPage 与 KhTable / KhForm 的配置关系

`KhPage` 内部固定渲染一个搜索 `KhForm` 和一个 `KhTable`。因此有三类配置：

| 类型 | 说明 | 例子 |
| --- | --- | --- |
| `KhPage` 直接声明 | `KhPage` 自己消费，并显式传给内部组件 | `showSelection`、`showToolbar`、`searchColCount`、`dialogWidth` |
| 透传给 `KhTable` | `KhPage` 没声明、也不在保留名单里的属性，会进入 `tableAttrs` 并传给内部 `KhTable` | `show-header-filter`、`action-width`、`row-style` |
| 不能直接传给搜索 `KhForm` | 搜索区 `KhForm` 只接收 `columns/model/colCount/collapsible/defaultCollapsed/quickSearchPlaceholder` 和 `search-extra-buttons` 插槽 | `labelWidth`、`disabled`、`showFooter` 不会被 `KhPage` 传给搜索表单 |

换句话说：`KhTable` 里一部分配置已经被 `KhPage` 包装成同名或近似同名 props；剩余表格配置可以通过透传继续用。`KhForm` 的搜索区配置没有做完整透传，只能通过 `searchColumns` 字段配置和少量 `KhPage` props 控制。

### 2.3.2 KhPage 已直接包装的 KhTable 配置

这些配置在 `KhPage` 上有明确 props，并会显式传给内部 `KhTable`：

| KhPage prop | 对应 KhTable prop | 说明 |
| --- | --- | --- |
| `columns` | `columns` | 表格列配置 |
| `load` 或 `module` | `load` | 内部最终传 `effectiveLoad` |
| `searchModel` | `extraParams` | 搜索参数对象会作为表格额外参数 |
| `showToolbar` | `showToolbar` | 是否显示工具栏 |
| `showPagination` | `showPagination` | 是否显示分页 |
| `showIndex` | `showIndex` | 是否显示序号列 |
| `showSelection` | `showSelection` | 是否显示多选列 |
| `border` | `border` | 表格边框 |
| `stripe` | `stripe` | 斑马纹 |
| `title` | `title` | 表格工具栏标题 |
| `toolbarButtons` | `toolbarButtons` | 会和内置新增/导出按钮合并 |
| `actionButtons` | `actionButtons` | 会和内置查看/编辑/删除按钮合并 |
| 内部计算 | `height` | `KhPage` 根据容器高度自动计算，不建议外部强行覆盖 |

注意：`KhPage` 会覆盖内部 `KhTable` 的 `title`、`height`、`load`、`extraParams`、`toolbarButtons`、`actionButtons`。如果你在 `KhPage` 上直接传这些同名属性，最终行为以 `KhPage` 内部计算和合并结果为准。

### 2.3.3 KhPage 可透传给 KhTable 的常用配置

未被 `KhPage` 保留的属性会透传给内部 `KhTable`。常用可透传项如下：

| 可在 KhPage 上传的属性 | 对应 KhTable prop | 用途 |
| --- | --- | --- |
| `data` | `data` | 外部数据模式。一般不推荐和 `module/load` 混用 |
| `loading` | `loading` | 外部 loading 状态 |
| `row-key` | `rowKey` | 行唯一键、树表和选择稳定性 |
| `size` | `size` | 表格尺寸 |
| `max-height` | `maxHeight` | 最大高度。注意 `KhPage` 已传 `height`，多数场景不需要 |
| `highlight-current-row` | `highlightCurrentRow` | 高亮当前行 |
| `default-expand-all` | `defaultExpandAll` | 树表默认展开 |
| `tree-props` | `treeProps` | 树数据 children 字段映射 |
| `header-cell-style` | `headerCellStyle` | 表头样式 |
| `row-style` | `rowStyle` | 行内联样式函数 |
| `row-class-name` | `rowClassName` | 行 class 函数 |
| `selection-width` | `selectionWidth` | 多选列宽 |
| `selection-fixed` | `selectionFixed` | 多选列固定 |
| `index-width` | `indexWidth` | 序号列宽 |
| `index-fixed` | `indexFixed` | 序号列固定 |
| `action-columns` | `actionColumns` | 旧式操作列按钮。不推荐新页面使用 |
| `action-label` | `actionLabel` | 操作列标题 |
| `action-width` | `actionWidth` | 操作列宽 |
| `action-min-width` | `actionMinWidth` | 操作列最小宽 |
| `action-fixed` | `actionFixed` | 操作列固定位置 |
| `total` | `total` | 外部分页总数。一般不推荐和 `load/module` 混用 |
| `page-num` | `pageNum` | 外部页码。一般不推荐和 `load/module` 混用 |
| `page-size` | `pageSize` | 每页条数 |
| `page-sizes` | `pageSizes` | 每页条数选项 |
| `show-refresh` | `showRefresh` | 工具栏刷新按钮 |
| `show-column-setting` | `showColumnSetting` | 列设置按钮 |
| `show-header-filter` | `showHeaderFilter` | 表头筛选 |
| `show-selection-info` | `showSelectionInfo` | 已选行数提示 |
| `auto-load` | `autoLoad` | 挂载时是否自动加载 |

示例：

```vue
<KhPage
  :show-header-filter="true"
  :action-width="'180'"
  :row-style="rowStyle"
/>
```

这些属性不是 `KhPage` 明确声明的 props，但会传给内部 `KhTable`。

### 2.3.4 KhPage 不建议或不能透传的 KhTable 配置

| 属性 | 原因 | 推荐做法 |
| --- | --- | --- |
| `columns` | `KhPage` 已直接声明 | 用 `:columns="tableColumns"` |
| `load` | `KhPage` 已直接声明，并会和 `module` 生成的加载函数合并为 `effectiveLoad` | 用 `:load="load"` 或 `module`，二选一 |
| `extra-params` | `KhPage` 固定传 `searchModel` | 查询条件放 `searchModel` |
| `height` | `KhPage` 自动计算表格高度 | 通过页面布局解决高度问题 |
| `toolbar-buttons` | `KhPage` 会合并内置按钮 | 用 `:toolbar-buttons="toolbarButtons"`，不要期待覆盖内置按钮 |
| `action-buttons` | `KhPage` 会合并内置按钮 | 用 `:action-buttons="actionButtons"`，不要期待覆盖内置按钮 |
| `title` | `KhPage` 固定用页面 `title` 传给 `KhTable` | 页面标题和表格标题保持一致 |
| `onSelectionChange` 等事件监听 | `KhPage` 的 `tableAttrs` 会过滤 `on*`，不会自动传给 `KhTable` | 用 `pageRef.value?.getSelectionRows()`，或通过 `tableRef` 访问内部表格 |

### 2.3.5 KhPage 内部搜索 KhForm 的可配置范围

`KhPage` 搜索区内部写法是：

```vue
<KhForm
  :columns="searchFormColumns"
  :model="searchModel"
  :col-count="searchColCount"
  :collapsible="collapsible"
  :default-collapsed="defaultCollapsed"
  :quick-search-placeholder="quickSearchPlaceholder"
  @search="handleSearch"
  @reset="handleReset"
/>
```

因此搜索 `KhForm` 的配置范围如下：

| KhPage 配置 | 对应 KhForm 能力 | 说明 |
| --- | --- | --- |
| `searchColumns` | `columns` | 会自动追加 `{ type: 'buttons', prop: '_buttons' }` |
| `searchModel` | 查询模型和表格 extraParams | 重置时 `KhPage` 会把每个 key 置为空字符串 |
| `searchColCount` | `colCount` | 搜索表单每行列数 |
| `collapsible` | `collapsible` | 是否折叠 |
| `defaultCollapsed` | `defaultCollapsed` | 默认收起 |
| `quickSearchPlaceholder` | `quickSearchPlaceholder` | 快速搜索占位 |
| `search-extra-buttons` slot | `extra-buttons` slot | 查询/重置按钮后追加内容 |

`KhForm` 的这些 props 目前不能通过 `KhPage` 直接配置到搜索区：`labelWidth`、`labelPosition`、`inline`、`gutter`、`size`、`disabled`、`showFooter`。如果搜索区需要这些能力，优先考虑：

1. 通过 `searchColumns` 的字段配置解决，例如 `span`、`labelWidth`、`placeholder`、`disabled`、`bindProps`。
2. 使用 `search-extra-buttons` 插槽补按钮。
3. 如果搜索表单布局高度定制，退回 `KhForm + KhTable` 自行组合。

当前源码还需要注意一个实现细节：`KhPage` 给搜索 `KhForm` 传的是 `:model="searchModel"`，而 `KhForm` 自身声明的是 `modelValue`。如果遇到“搜索表单输入值没有同步到 `searchModel`”的问题，优先检查这里；可在组件层修成 `v-model="searchModel"` 或 `:model-value="searchModel"`，否则页面侧不要假设 `KhForm` 内部输入一定会自动写回 `searchModel`。

### 2.4 module 与 useCrudApi 约定

传入 `module` 后，`KhPage` 会调用 `useCrudApi(module)`，自动拼出以下接口：

| 操作 | 请求 |
| --- | --- |
| 分页 | `POST /api/{module}/pagelist` |
| 详情 | `GET /api/{module}/{id}` |
| 新增 | `POST /api/{module}/create` |
| 修改 | `POST /api/{module}/update` |
| 删除 | `DELETE /api/{module}/delete/{id}` |
| 表单配置 | `GET /api/{module}/form-config` |

分页时 `KhTable` 传入的扁平参数会由 `buildPageQuery` 转成后端分页结构：

```js
{
  pageIndex: 1,
  pageSize: 30,
  sortConditions: [],
  filters: [
    { field: 'materialName', operator: 'contains', value: '螺丝' },
  ],
}
```

查询字段的默认 operator 规则：

| 字段类型 | 默认 operator |
| --- | --- |
| `input` | `contains` |
| `select` | `equals` |
| `number` | `equals` |
| 数组值 | `in` |

如果需要覆盖：

```js
const searchColumns = [
  { prop: 'createdTime', label: '创建时间', type: 'date', filterOperator: 'greaterThanOrEqual' },
]
```

### 2.5 searchColumns 查询配置

`searchColumns` 直接复用 `KhForm` 配置。常见写法：

```js
const searchColumns = [
  { prop: 'orderNo', label: '单号', type: 'input', clearable: true },
  { prop: 'orderStatus', label: '状态', type: 'select', clearable: true, options: 'dict:inbound_order_status' },
  { prop: 'createdTime', label: '创建日期', type: 'date', dateType: 'daterange', valueFormat: 'YYYY-MM-DD' },
]
```

使用建议：

- `prop` 必须与后端筛选字段一致。
- `searchModel` 要预置同名字段，否则重置和快速搜索容易出现状态不一致。
- 字典选项优先用 `options: 'dict:xxx'`，由组件自动加载。
- 查询字段不要太多。常用字段放搜索区，不常用字段用表头筛选或高级查询。

### 2.6 columns 表格配置

`columns` 直接传给 `KhTable`。常见列类型：

| 类型 | 写法 | 说明 |
| --- | --- | --- |
| 普通文本 | `{ prop, label }` | 默认文本 |
| 标签 | `{ type: 'tag', tagMap }` | 状态、类型、优先级 |
| 图片 | `{ type: 'image' }` | 图片预览 |
| 开关 | `{ type: 'switch' }` | 行内切换 |
| 链接 | `{ type: 'link', onClick }` | 可点击文本 |
| 插槽 | `{ type: 'slot', prop: 'xxx' }` | 自定义渲染 |
| 格式化 | `{ formatter(row, col, value) {} }` | 自定义显示文本 |
| 可编辑标签下拉 | `{ type: 'tag-select', options, tagTypeMap }` | 行内修改枚举值 |
| 展开行 | `{ type: 'expand' }` | 展示明细、子表格、轨迹 |

示例：

```js
const tableColumns = [
  { prop: 'taskNo', label: '任务编号', width: 160, fixed: 'left' },
  { prop: 'taskType', label: '任务类型', width: 100, type: 'tag', tagMap: 'dict:task_type' },
  { prop: 'taskStatus', label: '状态', width: 100, type: 'tag', tagMap: 'dict:task_status' },
  {
    prop: 'taskPriority',
    label: '优先级',
    width: 100,
    type: 'tag-select',
    options: [
      { label: '普通', value: 'NORMAL' },
      { label: '紧急', value: 'URGENT' },
    ],
    tagTypeMap: { NORMAL: 'info', URGENT: 'danger' },
    selectProps: { clearable: false },
  },
  {
    prop: 'planQty',
    label: '计划数量',
    width: 100,
    align: 'right',
    cellStyle: (row, col, value) => value > row.stockQty
      ? { color: '#f56c6c', fontWeight: 600 }
      : {},
  },
  { prop: 'remark', label: '备注', minWidth: 160, showOverflowTooltip: true },
]
```

`KhPage` 也可以直接使用 `KhTable` 的表格级配置。因为 `KhPage` 内部会把未声明的属性透传给 `KhTable`，所以这些写法都是有效的：

```vue
<KhPage
  ref="pageRef"
  title="波次任务"
  module="wmsWaveTask"
  :columns="tableColumns"
  :row-style="getRowStyle"
  :row-class-name="getRowClassName"
  :show-header-filter="true"
  :action-width="180"
  :page-sizes="[20, 50, 100, 200]"
  row-key="id"
/>
```

```js
const getRowStyle = (row) => {
  if (row.taskStatus === 'CANCELLED') return 'danger'
  if (row.taskStatus === 'FINISHED') return 'success'
  if (row.taskPriority === 'URGENT') return { backgroundColor: '#fff7e6' }
  return {}
}

const getRowClassName = ({ row }) => {
  return row.locked ? 'is-locked-row' : ''
}
```

注意：

- `row-style` 返回对象时会作为行内样式；返回 `'danger'`、`'success'` 等预设名称时会走项目的行样式预设。
- 一旦传了 `row-style`，`KhTable` 会自动关闭斑马纹，避免斑马纹背景覆盖行样式。
- 简单背景色优先用 `row-style`；复杂 hover、边框、字体组合优先用 `row-class-name` 加 CSS。
- `KhPage` 会透传普通属性和插槽，但不会透传 `@selection-change`、`@cell-change` 这类事件监听。需要监听这些事件时，优先直接使用 `KhTable`；在 `KhPage` 中只建议通过 `pageRef.getSelectionRows()`、`actionButtons`、自定义 slot 处理常见交互。

### 2.7 formColumns 表单配置

`formColumns` 用于内置新增/编辑弹窗。常见写法：

```js
const formColumns = [
  { prop: 'materialCode', label: '物料编码', type: 'input', required: true, maxlength: 20 },
  { prop: 'materialName', label: '物料名称', type: 'input', required: true, maxlength: 100 },
  { prop: 'categoryId', label: '物料分类', type: 'select', required: true, options: 'dict:material_category' },
  { prop: 'status', label: '状态', type: 'switch', activeValue: 1, inactiveValue: 0 },
  { prop: 'remark', label: '备注', type: 'textarea', rows: 3, maxlength: 200 },
]
```

如果不传 `formColumns`，`KhPage` 会尝试调用 `/api/{module}/form-config` 获取后端动态表单配置。

### 2.8 crudOperations 内置按钮开关

默认值：

```js
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: false,
  export: false,
}
```

示例：只读列表 + 详情 + 导出：

```vue
<KhPage
  module="task-header"
  :crud-operations="{ create: false, update: false, delete: false, view: true, export: true }"
/>
```

说明：

- `create` 生成工具栏“新增”按钮。
- `export` 生成工具栏“导出”按钮，目前内置实现只是提示“待实现”，复杂导出建议自定义 `toolbarButtons`。
- `view` 生成操作列“查看”按钮，并打开内置 `KhDetailDialog`。
- `update` 生成操作列“编辑”按钮。
- `delete` 生成操作列“删除”按钮，并使用 `el-popconfirm` 确认。

### 2.9 权限配置

`KhPage` 会用 `permissionPrefix + permissionMap` 生成按钮权限码。

默认 `permissionPrefix`：

```js
permissionPrefix || module
```

默认 `permissionMap`：

```js
{
  create: 'add',
  update: 'edit',
  delete: 'delete',
  view: 'view',
  export: 'export',
}
```

例如：

```vue
<KhPage module="material" permission-prefix="bd:material" />
```

生成权限：

| 操作 | 权限码 |
| --- | --- |
| 新增 | `bd:material:add` |
| 编辑 | `bd:material:edit` |
| 删除 | `bd:material:delete` |
| 查看 | `bd:material:view` |
| 导出 | `bd:material:export` |

常见坑：

- `permission-prefix` 必须与后端菜单/按钮权限保持一致。
- 自定义 `actionButtons` 也要显式写 `permission`。
- 权限值里是否含业务域前缀，例如 `task:header:complete`，必须按后端实际配置。

### 2.10 toolbarButtons 与 actionButtons

`toolbarButtons` 会追加到表格工具栏，内置按钮在前，自定义按钮在后。

```js
const toolbarButtons = [
  {
    label: '批量审核',
    type: 'primary',
    permission: 'inv:adjust:approve',
    onClick: () => handleBatchApprove(),
  },
]
```

`actionButtons` 会追加到操作列，适合行级业务操作：

```js
const actionButtons = [
  {
    label: '完成',
    permission: 'task:header:complete',
    type: 'success',
    onClick: (row) => handleComplete(row),
    show: (row) => row.taskStatus === 'PENDING' || row.taskStatus === 'IN_PROGRESS',
  },
  {
    label: '取消',
    permission: 'task:header:cancel',
    onClick: (row) => handleCancel(row),
    show: (row) => row.taskStatus === 'PENDING',
  },
]
```

按钮配置常用字段：

| 字段 | 说明 |
| --- | --- |
| `label` | 按钮文本 |
| `type` | Element Plus 按钮类型 |
| `permission` | 权限码 |
| `icon` | 图标组件 |
| `loading` | 加载状态 |
| `disabled` | 布尔值或函数，视组件实现而定 |
| `show(row)` | 行按钮显隐控制 |
| `confirm` | 操作前确认文案 |
| `onClick(row, index)` | 点击回调 |

### 2.11 生命周期钩子

| 钩子 | 调用时机 | 返回值 |
| --- | --- | --- |
| `beforeCreate()` | 点击新增前 | 返回 `false` 阻止打开弹窗 |
| `beforeUpdate(row)` | 点击编辑前 | 返回 `false` 阻止打开弹窗 |
| `beforeDelete(row)` | 删除前 | 返回 `false` 阻止删除 |
| `beforeSubmit(data, mode)` | 新增/编辑提交前 | 返回 `false` 阻止提交 |
| `afterSubmit(response, mode)` | 新增/编辑提交成功后 | 无 |

常见用法：提交前整理扩展字段。

```js
const beforeSubmit = (data) => {
  const raw = extractAndCleanExtData(data)
  if (raw) data.extDataRaw = raw
}
```

注意：`beforeSubmit` 当前实现只判断是否严格等于 `false`。如果没有阻止提交，不需要返回值。

### 2.12 deleteShow / updateShow 控制内置行按钮

对状态机页面，内置编辑/删除按钮可以按行控制显隐。

```vue
<KhPage
  :delete-show="(row) => row.allowedActions?.includes('delete')"
  :update-show="(row) => row.orderStatus === 'DRAFT'"
/>
```

建议：

- 简单状态判断可以写在页面。
- 复杂权限/状态机最好由后端返回 `allowedActions`，前端只负责展示。

### 2.13 自定义 load

`load` 的优先级高于 `module` 内置加载。函数接收 `KhTable` 传入的分页、排序、筛选和搜索参数，返回：

```js
{
  data: [],
  total: 0,
}
```

示例：

```js
const load = async (params) => {
  const res = await customPageApi(params)
  return {
    data: res.data?.items ?? [],
    total: res.data?.total ?? 0,
  }
}
```

扩展字段场景可使用 `useExtFields` 提供的 `withFlatExtLoad`：

```js
const crudApi = useCrudApi('material')
const { withFlatExtLoad } = useExtFields('/api/material/form-config')
const load = withFlatExtLoad(crudApi, searchColumns)
```

### 2.14 字典配置

`KhPage` 会收集 `searchColumns`、`formColumns`、`columns`、`detailLines.columns` 中的字典引用，并通过 `dictStore.getDict(type)` 预加载。

支持的字典写法：

```js
{ type: 'select', options: 'dict:status_flag' }
{ type: 'tag', tagMap: 'dict:task_status' }
{ filterOptions: 'dict:material_category' }
```

组件会把字典转换为：

- `options`：下拉选项。
- `tagMap`：值到显示文本的映射。
- `tagTypeMap`：值到标签颜色的映射。

### 2.15 插槽

`KhPage` 显式支持：

| 插槽 | 说明 |
| --- | --- |
| `toolbar-left` | 表格工具栏左侧 |
| `toolbar-right` | 表格工具栏右侧，传了会覆盖默认刷新/列设置区 |
| `action` | 完全自定义操作列 |
| `stat-extra` | 单个统计卡片扩展内容 |
| `stat-extra-row` | 统计区下方整行扩展内容 |
| `search-extra-buttons` | 搜索按钮后追加按钮 |

其他未保留插槽会透传给内部 `KhTable`，通常用于 `columns` 中的 `type: 'slot'`。

示例：自定义状态列：

```vue
<KhPage :columns="tableColumns">
  <template #orderStatus="{ row }">
    <el-tag :type="statusTagMap[row.orderStatus]">
      {{ row.orderStatusName }}
    </el-tag>
  </template>
</KhPage>
```

对应列配置：

```js
const tableColumns = [
  { prop: 'orderStatus', label: '状态', type: 'slot', width: 100 },
]
```

### 2.16 detailLines 主从详情

当 `crudOperations.view=true` 时，内置“查看”按钮会打开 `KhDetailDialog`。如果详情数据里含从表数组，可通过 `detailLines` 配置展示。

```js
const detailLines = [
  {
    prop: 'lines',
    title: '任务行',
    columns: [
      { prop: 'lineNo', label: '行号', width: 70 },
      { prop: 'materialCode', label: '物料编码', width: 130 },
      { prop: 'materialName', label: '物料名称', minWidth: 160 },
    ],
  },
]
```

```vue
<KhPage
  :crud-operations="{ create: false, update: false, delete: false, view: true }"
  :detail-lines="detailLines"
  detail-width="1200px"
/>
```

### 2.17 statCards 顶部统计

`statCards` 传给内置 `KhStatCard`。

```js
const statCards = [
  { label: '库存总量', value: 123456, icon: markRaw(Box), theme: 'primary' },
  { label: '预警数量', value: 18, icon: markRaw(Warning), theme: 'warning', clickable: true },
]
```

```vue
<KhPage :stat-cards="statCards" @stat-click="handleStatClick" />
```

注意：`KhPage` 当前源码虽然声明了 `statSpan`，但统计卡片模板没有把它绑定到布局上。需要改变顶部指标卡列数时，优先通过外层样式或修改 `KhPage` 组件实现，不要只传 `stat-span`。

### 2.18 暴露方法

通过 `ref` 可调用：

| 方法/属性 | 说明 |
| --- | --- |
| `tableRef` | 内部 `KhTable` ref |
| `searchFormRef` | 内部搜索 `KhForm` ref |
| `reload()` | 重置到第 1 页并重新加载 |
| `refresh()` | 刷新当前页 |
| `getSelectionRows()` | 获取当前选中行 |
| `clearSelection()` | 清空选中 |
| `openCreateDialog()` | 手动打开新增弹窗 |
| `openUpdateDialog(row)` | 手动打开编辑弹窗 |
| `openDetailDialog(row)` | 手动打开详情弹窗 |

常见用法：

```js
const pageRef = ref(null)

const handleSuccess = () => {
  pageRef.value?.reload()
}

const handleBatchDelete = () => {
  const rows = pageRef.value?.getSelectionRows() || []
}
```

### 2.19 KhPage 常见坑

| 问题 | 原因 | 建议 |
| --- | --- | --- |
| 页面高度异常，表格不撑满 | 外层容器未给高度和 flex | 外层使用 `height: 100%; display: flex; flex-direction: column;` |
| 新增按钮不显示 | `module` 为空或 `crudOperations.create=false` 或权限不足 | 检查 `module`、权限码、`crudOperations` |
| 删除/编辑按钮权限失效 | `permissionPrefix` 与后端按钮权限不一致 | 以系统菜单权限配置为准 |
| 搜索重置后字段残留 | `searchModel` 未定义对应 key | `searchColumns` 和 `searchModel` 保持同名字段 |
| 字典列空白 | 字典类型不正确或后端未返回 | 检查 `dict:xxx` 与字典管理配置 |
| 弹窗表单没有默认值 | 未传 `customFormData` 或字段类型不匹配 | 为 switch/number/select 设置明确初始值 |
| 复杂业务塞进 `beforeSubmit` | 钩子职责过重 | 状态推进、审批、扫码等用自定义弹窗或 actionButtons |
| 操作列按钮太多 | 行内按钮堆叠 | 重要操作保留行内，批量/低频操作放工具栏或更多菜单 |

## 3. KhTable 通用业务表格

`KhTable` 是 `KhPage` 的表格底座，也可独立使用。它封装了：

- 表格列配置。
- 内部分页加载。
- 工具栏按钮。
- 操作列按钮。
- 多选、序号。
- 刷新、列设置。
- 表头筛选。
- 行点击、排序、当前行、单元格变更事件。

### 3.1 独立使用示例

```vue
<KhTable
  ref="tableRef"
  title="任务列表"
  :columns="columns"
  :load="load"
  :show-toolbar="true"
  :show-pagination="true"
  :show-selection="true"
  :show-index="true"
  :toolbar-buttons="toolbarButtons"
  :action-buttons="actionButtons"
  :show-header-filter="true"
/>
```

```js
const load = async (params) => {
  const res = await api.page(params)
  return {
    data: res.data.items,
    total: res.data.total,
  }
}
```

### 3.2 核心 props

| Prop | 说明 |
| --- | --- |
| `data` | 外部数据模式下的表格数据 |
| `columns` | 列配置，必传 |
| `load` | 内部加载模式的异步函数 |
| `extraParams` | 加载时额外合并的查询参数 |
| `rowKey` | 行 key，默认 `id` |
| `border` / `stripe` / `size` | 表格样式 |
| `height` / `maxHeight` | 表格高度 |
| `showSelection` | 多选列 |
| `showIndex` | 序号列 |
| `showPagination` | 分页 |
| `showToolbar` | 工具栏 |
| `showRefresh` | 工具栏刷新按钮 |
| `showColumnSetting` | 列设置 |
| `showHeaderFilter` | 表头筛选 |
| `toolbarButtons` | 工具栏按钮 |
| `actionButtons` | 操作列按钮 |
| `actionWidth` / `actionMinWidth` / `actionFixed` | 操作列宽度与固定方式 |
| `rowStyle` | 行样式函数 |
| `rowClassName` | 行 class |

### 3.3 columns 配置

通用字段：

| 字段 | 说明 |
| --- | --- |
| `prop` | 数据字段 |
| `label` | 列标题 |
| `width` / `minWidth` | 宽度 |
| `fixed` | 固定列 |
| `align` / `headerAlign` | 对齐 |
| `sortable` | 排序 |
| `showOverflowTooltip` | 溢出 tooltip，默认开启 |
| `formatter` | 格式化函数 |
| `cellStyle` | 单元格样式函数 |
| `bindProps` | 透传给 `el-table-column` |

列类型：

```js
const columns = [
  { prop: 'code', label: '编码', width: 120 },
  { prop: 'status', label: '状态', type: 'tag', tagMap: 'dict:status_flag' },
  { prop: 'enabled', label: '启用', type: 'switch', activeValue: 1, inactiveValue: 0 },
  { prop: 'imageUrl', label: '图片', type: 'image', imageWidth: '40px', imageHeight: '40px' },
  { prop: 'orderNo', label: '单号', type: 'link', onClick: (row) => openDetail(row) },
  { prop: 'custom', label: '自定义', type: 'slot' },
]
```

#### 3.3.1 rowStyle 行样式

`rowStyle` 是表格级配置，不写在 `columns` 里。它用于根据整行数据控制行背景色、字体色等。

`KhTable` 写法：

```vue
<KhTable :columns="columns" :load="loadData" :row-style="getRowStyle" />
```

`KhPage` 写法：

```vue
<KhPage module="wmsTask" :columns="columns" :row-style="getRowStyle" />
```

```js
const getRowStyle = (row, rowIndex) => {
  if (row.status === 'CANCELLED') return 'danger'
  if (row.status === 'FINISHED') return 'success'
  if (row.priority === 'URGENT') return { backgroundColor: '#fff7e6' }
  if (rowIndex % 2 === 0 && row.warning) return { color: '#e6a23c' }
  return {}
}
```

常见问题：

| 问题 | 原因 | 处理办法 |
| --- | --- | --- |
| 写在 `columns` 里不生效 | `rowStyle` 是表格属性，不是列属性 | 写成 `<KhTable :row-style="getRowStyle" />` 或 `<KhPage :row-style="getRowStyle" />` |
| 传了 `stripe` 但斑马纹没了 | 组件检测到 `rowStyle` 后会自动关闭斑马纹 | 这是正常行为，避免背景互相覆盖 |
| 只想改某个单元格颜色 | `rowStyle` 会影响整行 | 改用列配置 `cellStyle` |
| 需要复杂 hover、边框、字体组合 | 行内样式不好维护 | 改用 `rowClassName` + CSS |

#### 3.3.2 rowClassName 行类名

`rowClassName` 适合做复杂样式，支持字符串或函数。函数参数沿用 Element Plus 的行信息对象，常用 `row` 和 `rowIndex`。

```vue
<KhTable :columns="columns" :load="loadData" :row-class-name="getRowClassName" />
```

```js
const getRowClassName = ({ row, rowIndex }) => {
  if (row.locked) return 'is-locked-row'
  if (rowIndex === 0) return 'is-first-row'
  return ''
}
```

```css
:deep(.is-locked-row td) {
  background-color: #f5f7fa;
  color: #909399;
}

:deep(.is-first-row td:first-child) {
  font-weight: 600;
}
```

`KhPage` 中同样可以直接写：

```vue
<KhPage module="wmsTask" :columns="columns" :row-class-name="getRowClassName" />
```

#### 3.3.3 cellStyle 单元格样式

`cellStyle` 写在某一列上，只影响该列对应的单元格。

```js
const columns = [
  {
    prop: 'availableQty',
    label: '可用库存',
    align: 'right',
    cellStyle: (row, col, value, rowIndex) => {
      if (value <= 0) return { color: '#f56c6c', fontWeight: 600 }
      if (value < row.safeQty) return { color: '#e6a23c' }
      return {}
    },
  },
]
```

常见问题：

| 问题 | 原因 | 处理办法 |
| --- | --- | --- |
| `cellStyle` 没有拿到值 | 函数第三个参数才是当前列值 | 使用 `(row, col, value) => {}` |
| 改整行背景不方便 | `cellStyle` 只影响单元格 | 改用表格级 `rowStyle` |
| 样式被自定义插槽覆盖 | slot 内部元素自带样式优先 | 在 slot 内部控制样式，或提高 CSS 选择器优先级 |

#### 3.3.4 formatter 格式化显示

`formatter` 只负责展示文本，不改变原始数据。

```js
const columns = [
  {
    prop: 'amount',
    label: '金额',
    align: 'right',
    formatter: (row, col, value) => value == null ? '-' : `¥${Number(value).toFixed(2)}`,
  },
  {
    prop: 'createdTime',
    label: '创建时间',
    minWidth: 160,
    formatter: (row, col, value) => value ? value.replace('T', ' ') : '-',
  },
]
```

常见问题：

| 问题 | 原因 | 处理办法 |
| --- | --- | --- |
| 排序按格式化后的文本排 | 前端展示和后端排序协议混用 | 后端分页表格优先使用接口排序 |
| 需要按钮、图标、复杂 DOM | `formatter` 只返回展示文本 | 改用 `type: 'slot'` |

#### 3.3.5 tag / tag-select / switch / link

`tag` 用于只读状态展示：

```js
const columns = [
  {
    prop: 'status',
    label: '状态',
    type: 'tag',
    tagMap: { ENABLED: '启用', DISABLED: '停用' },
    tagTypeMap: { ENABLED: 'success', DISABLED: 'info' },
    tagEffect: 'light',
  },
]
```

`tag-select` 用于行内修改枚举值，变更后会触发 `cell-change`：

```js
const columns = [
  {
    prop: 'priority',
    label: '优先级',
    type: 'tag-select',
    options: [
      { label: '普通', value: 'NORMAL' },
      { label: '紧急', value: 'URGENT' },
    ],
    tagTypeMap: { NORMAL: 'info', URGENT: 'danger' },
    selectProps: { clearable: false },
  },
]
```

`switch` 用于行内开关：

```js
const columns = [
  {
    prop: 'enabled',
    label: '启用',
    type: 'switch',
    activeValue: 1,
    inactiveValue: 0,
    switchProps: { activeText: '启用', inactiveText: '停用' },
  },
]
```

`link` 用于点击进入详情：

```js
const columns = [
  {
    prop: 'orderNo',
    label: '单号',
    type: 'link',
    onClick: (row) => openDetail(row),
  },
]
```

注意：`tag-select` 和 `switch` 只负责修改 UI 值并触发 `cell-change`，不会自动调用保存接口。使用 `KhTable` 时监听 `@cell-change` 保存；使用 `KhPage` 时事件不会透传，建议改成 `actionButtons` 或自定义 slot 中显式调用接口。

#### 3.3.6 slot 自定义列

列配置：

```js
const columns = [
  { prop: 'status', label: '状态', type: 'slot' },
  { prop: 'progress', label: '进度', type: 'slot', minWidth: 160 },
]
```

`KhTable` 插槽：

```vue
<KhTable :columns="columns" :load="loadData">
  <template #status="{ row }">
    <el-tag :type="row.status === 'FINISHED' ? 'success' : 'warning'">
      {{ row.statusName }}
    </el-tag>
  </template>

  <template #progress="{ row }">
    <el-progress :percentage="row.progress || 0" />
  </template>
</KhTable>
```

`KhPage` 会把非保留插槽继续转给内部 `KhTable`，所以写法相同：

```vue
<KhPage module="wmsTask" :columns="columns">
  <template #status="{ row }">
    <el-tag>{{ row.statusName }}</el-tag>
  </template>
</KhPage>
```

#### 3.3.7 expand 展开行

需要展示明细、轨迹、子表格时使用展开行。

```js
const columns = [
  { type: 'expand', prop: 'expand' },
  { prop: 'orderNo', label: '单号', width: 160 },
  { prop: 'ownerName', label: '货主', minWidth: 140 },
]
```

```vue
<KhTable :columns="columns" :load="loadData" row-key="id">
  <template #expand="{ row }">
    <div class="order-detail">
      <KhTable
        :columns="detailColumns"
        :data="row.details || []"
        :show-pagination="false"
        :show-toolbar="false"
        :show-selection="false"
      />
    </div>
  </template>
</KhTable>
```

`KhPage` 中也可以直接使用 `#expand`：

```vue
<KhPage module="wmsOrder" :columns="columns" row-key="id">
  <template #expand="{ row }">
    <OrderDetailPanel :row="row" />
  </template>
</KhPage>
```

常见问题：

| 问题 | 原因 | 处理办法 |
| --- | --- | --- |
| 展开状态错乱 | 没有稳定 `row-key` | 必须传唯一 `row-key`，通常是 `id` |
| 展开后子表格分页出现 | 子表格默认显示分页 | 子表格加 `:show-pagination="false"` |
| 展开内容太挤 | 展开 slot 没有容器样式 | 给展开内容加 padding 或独立组件 |

### 3.4 表头筛选

开启：

```vue
<KhTable :show-header-filter="true" :columns="columns" />
```

列配置：

```js
const columns = [
  { prop: 'materialName', label: '物料名称', searchable: true, filterType: 'input' },
  { prop: 'status', label: '状态', searchable: true, filterType: 'select', filterOptions: 'dict:status_flag' },
  { prop: 'qty', label: '数量', searchable: true, filterType: 'number-range' },
  { prop: 'createdTime', label: '创建时间', searchable: true, filterType: 'date-range' },
]
```

支持的 `filterType`：

- `input`
- `select`
- `multiple-select`
- `number-range`
- `date-range`
- `datetime-range`

完整示例：

```js
const columns = [
  {
    prop: 'materialCode',
    label: '物料编码',
    searchable: true,
    filterType: 'input',
    filterMatchMode: 'equals',
  },
  {
    prop: 'materialName',
    label: '物料名称',
    searchable: true,
    filterType: 'input',
    filterMatchMode: 'contains',
  },
  {
    prop: 'status',
    label: '状态',
    type: 'tag',
    tagMap: 'dict:status_flag',
    searchable: true,
    filterType: 'select',
    filterOptions: 'dict:status_flag',
  },
  {
    prop: 'warehouseType',
    label: '仓库类型',
    searchable: true,
    filterType: 'multiple-select',
    filterOptions: [
      { label: '原料仓', value: 'RAW' },
      { label: '成品仓', value: 'FINISHED' },
    ],
  },
  {
    prop: 'availableQty',
    label: '可用库存',
    searchable: true,
    filterType: 'number-range',
  },
  {
    prop: 'createdTime',
    label: '创建时间',
    searchable: true,
    filterType: 'datetime-range',
  },
]
```

`KhPage` 中开启表头筛选：

```vue
<KhPage
  module="bdMaterial"
  :columns="columns"
  :show-header-filter="true"
/>
```

筛选条件会进入 `KhTable` 的查询参数。如果使用内部 `load` 或 `KhPage module` 模式，组件会自动带上这些条件；如果使用外部数据模式，需要父组件监听 `header-filter` 或 `search` 并自行请求。

常见问题：

| 问题 | 原因 | 处理办法 |
| --- | --- | --- |
| 表头没有筛选图标 | 只开了 `show-header-filter`，列没有 `searchable: true` | 两个条件都要满足 |
| 下拉筛选没有选项 | `filterOptions` 没配，或字典未加载 | 显式配置数组或 `dict:xxx` |
| 编码列模糊匹配查出太多 | `input` 默认是 `contains` | 主键、编码列加 `filterMatchMode: 'equals'` |
| 多选筛选后后端不识别 | `multiple-select` 默认操作符是 `in` | 确认后端支持 `in`，不支持就转换查询条件 |
| 日期范围格式不对 | 前后端日期格式不一致 | 在 `load` 中统一转换开始/结束时间 |
| 在 `KhPage` 上监听不到 `@header-filter` | `KhPage` 不透传事件监听 | 使用内部查询，或改用 `KhTable` 外部模式 |

### 3.5 按钮配置

工具栏：

```js
const toolbarButtons = [
  { label: '新增', type: 'primary', icon: Plus, permission: 'bd:brand:add', onClick: openCreate },
]
```

操作列：

```js
const actionButtons = [
  { label: '编辑', permission: 'bd:brand:edit', onClick: (row) => openEdit(row) },
  { label: '删除', type: 'danger', confirm: '确定删除该数据？', onClick: (row) => remove(row) },
]
```

### 3.6 事件

| 事件 | 参数 | 说明 |
| --- | --- | --- |
| `update:pageNum` | `pageNum` | 外部数据模式下页码变化 |
| `update:pageSize` | `pageSize` | 外部数据模式下每页条数变化 |
| `selection-change` | `rows` | 多选变化 |
| `sort-change` | `{ column, prop, order }` | 排序变化 |
| `current-change` | `row` | 当前行变化 |
| `row-click` | `row` | 行点击 |
| `row-dblclick` | `row` | 行双击 |
| `cell-change` | `prop, row, val` | switch/tag-select 等行内编辑变化 |
| `header-filter` | `filters` | 表头筛选变化 |
| `refresh` | - | 外部数据模式下点击刷新 |
| `search` | - | 外部数据模式下需要父组件重新请求 |
| `before-load` | `params` | 内部加载模式请求前通知；当前实现不能通过返回 `false` 取消加载 |
| `after-load` | `{ data, total }` | 内部加载模式成功后触发 |
| `load-error` | `error` | 内部加载模式失败后触发 |

### 3.7 暴露方法

| 方法 | 说明 |
| --- | --- |
| `reload()` | 回到第 1 页并重新加载 |
| `refresh()` | 当前页重新加载 |
| `getQueryParams()` | 获取当前查询参数 |
| `getSelectionRows()` | 获取选中行 |
| `clearSelection()` | 清空选择 |
| `toggleRowSelection(row, selected)` | 切换行选中 |
| `clearSort()` | 清除排序 |
| `sort(prop, order)` | 手动排序 |
| `setCurrentRow(row)` | 设置当前行 |
| `clearFilter(columnKeys)` | 清除 Element 内置筛选 |
| `clearHeaderFilters()` | 清除表头筛选 |

## 4. KhForm 配置式表单

`KhForm` 用一组 `columns` 配置生成表单，适合查询表单、新增编辑表单和流程表单。

### 4.1 基本用法

```vue
<KhForm
  ref="formRef"
  v-model="formData"
  :columns="formColumns"
  :col-count="2"
  label-width="100px"
  show-footer
  @submit="handleSubmit"
  @cancel="handleCancel"
/>
```

```js
const formData = reactive({})

const formColumns = [
  { prop: 'code', label: '编码', type: 'input', required: true },
  { prop: 'name', label: '名称', type: 'input', required: true },
  { prop: 'type', label: '类型', type: 'select', options: 'dict:xxx' },
]
```

### 4.2 核心 props

| Prop | 说明 |
| --- | --- |
| `columns` | 表单字段配置 |
| `modelValue` | `v-model` 数据 |
| `labelWidth` | 标签宽度，默认 `100px` |
| `labelPosition` | 标签位置 |
| `inline` | 行内表单 |
| `colCount` | 每行列数 |
| `gutter` | 栅格间距 |
| `size` | 表单尺寸 |
| `disabled` | 整体禁用 |
| `showFooter` | 是否显示底部确认/取消 |
| `collapsible` | 是否启用折叠搜索模式 |
| `defaultCollapsed` | 默认收起 |
| `quickSearchPlaceholder` | 收起态搜索占位 |

### 4.3 字段类型

| 类型 | 说明 |
| --- | --- |
| `input` | 文本输入，默认类型 |
| `textarea` | 多行文本 |
| `number` | 数字输入 |
| `select` | 下拉选择 |
| `remote-select` | 远程搜索下拉 |
| `date` | 日期/时间选择 |
| `switch` | 开关 |
| `radio` | 单选 |
| `checkbox` | 多选 |
| `cascader` | Element 级联选择 |
| `cascade-select` | 多个独立下拉组成的级联 |
| `color-picker` | 颜色选择 |
| `icon-picker` | 图标选择 |
| `slot` | 自定义插槽 |
| `buttons` | 查询/重置按钮组 |

### 4.4 字段配置通用属性

```js
{
  prop: 'materialCode',
  label: '物料编码',
  type: 'input',
  required: true,
  requiredMessage: '请输入物料编码',
  rules: [],
  span: 12,
  hidden: false,
  disabled: false,
  defaultValue: '',
  placeholder: '请输入物料编码',
  clearable: true,
  bindProps: {},
  onChange: (value, formData) => {},
}
```

### 4.5 校验

简单必填：

```js
{ prop: 'materialName', label: '物料名称', required: true }
```

自定义规则：

```js
{
  prop: 'qty',
  label: '数量',
  type: 'number',
  rules: [
    { required: true, message: '请输入数量', trigger: 'blur' },
    { type: 'number', min: 1, message: '数量必须大于 0', trigger: 'blur' },
  ],
}
```

### 4.6 级联下拉

```js
{
  prop: 'locationCascade',
  label: '库位',
  type: 'cascade-select',
  cascadeItems: [
    { prop: 'warehouseId', label: '仓库', options: warehouseOptions },
    {
      prop: 'zoneId',
      label: '库区',
      parentProp: 'warehouseId',
      loadOptions: async (warehouseId) => loadZones(warehouseId),
    },
    {
      prop: 'locationId',
      label: '库位',
      parentProp: 'zoneId',
      loadOptions: async (zoneId) => loadLocations(zoneId),
    },
  ],
}
```

### 4.7 事件与暴露方法

事件：

| 事件 | 说明 |
| --- | --- |
| `update:modelValue` | 表单数据变化 |
| `search` | 点击查询或快速搜索 |
| `reset` | 点击重置 |
| `submit` | 校验通过后提交 |
| `cancel` | 点击取消 |
| `change` | 字段变化 |

暴露方法：

| 方法 | 说明 |
| --- | --- |
| `formRef` | 内部 `el-form` ref，可直接调用 Element Plus 表单方法 |
| `formData` | 内部响应式表单数据对象 |
| `validate()` | 手动校验，返回 `Promise<boolean>` |
| `resetFields()` | 重置字段 |
| `clearValidate(props)` | 清除校验 |
| `initFormData()` | 根据 columns 重新初始化数据 |

## 5. KhDialog 通用弹窗

`KhDialog` 封装了 Element Plus `el-dialog`，并内置 `KhForm` 能力。既可以作为普通弹窗，也可以通过 `formColumns` 快速生成表单弹窗。

### 5.1 表单弹窗

```vue
<KhDialog
  v-model="visible"
  title="新增物料"
  width="800px"
  :form-columns="formColumns"
  :form-model="formData"
  :form-col-count="2"
  :confirm-loading="submitLoading"
  @confirm="handleConfirm"
/>
```

```js
const handleConfirm = async (data) => {
  submitLoading.value = true
  try {
    await api.create(data)
    visible.value = false
    pageRef.value?.reload()
  } finally {
    submitLoading.value = false
  }
}
```

### 5.2 自定义内容弹窗

```vue
<KhDialog v-model="visible" title="处理异常" width="700px" @confirm="handleSubmit">
  <KhForm ref="formRef" v-model="formData" :columns="columns" />
</KhDialog>
```

### 5.3 核心 props

| Prop | 默认值 | 说明 |
| --- | --- | --- |
| `modelValue` | `false` | `v-model` 显隐 |
| `title` | `''` | 标题 |
| `description` | `''` | 标题下描述 |
| `width` | `'720px'` | 宽度 |
| `height` | `''` | 固定内容高度 |
| `top` | `'10vh'` | 距顶部距离 |
| `closeOnClickModal` | `false` | 点击遮罩关闭 |
| `closeOnPressEscape` | `true` | ESC 关闭 |
| `destroyOnClose` | `false` | 关闭销毁内容 |
| `draggable` | `false` | 可拖拽 |
| `fullscreen` | `false` | 全屏 |
| `showClose` | `true` | 右上角关闭 |
| `appendToBody` | `true` | 挂到 body |
| `showFooter` | `true` | 底部按钮 |
| `confirmText` | `'确定'` | 确认按钮文本 |
| `cancelText` | `'取消'` | 取消按钮文本 |
| `confirmLoading` | `false` | 确认按钮 loading |
| `formColumns` | `[]` | 内置表单字段 |
| `formModel` | `{}` | 内置表单初始值 |
| `formLabelWidth` | `'100px'` | 内置表单标签宽 |
| `formColCount` | `1` | 内置表单列数 |
| `formSize` | `'default'` | 内置表单尺寸 |
| `formDisabled` | `false` | 内置表单禁用 |

### 5.4 事件与方法

事件：

| 事件 | 说明 |
| --- | --- |
| `open` | 弹窗打开后 |
| `close` | 弹窗关闭后 |
| `confirm` | 确认。存在内置表单时，校验通过后传入表单数据 |
| `cancel` | 取消 |

暴露方法：

| 方法/属性 | 说明 |
| --- | --- |
| `khFormRef` | 内置 `KhForm` ref |
| `formData` | 内置表单数据 |
| `open(data)` | 编程式打开 |
| `close()` | 编程式关闭 |

### 5.5 使用建议

- 标准新增/编辑优先交给 `KhPage` 内置弹窗。
- 独立业务动作，例如“冻结库存”“异常处理”“分配货位”，用页面自定义 `KhDialog`。
- 长表单用 `width="900px"`、`form-col-count="2"`。
- 重要提交动作必须控制 `confirmLoading`，避免重复提交。
- `closeOnClickModal` 默认保持 `false`，避免误关丢数据。

## 6. KhDetailDialog 详情弹窗

`KhDetailDialog` 用于只读详情展示，常由 `KhPage` 内置查看动作打开，也可单独使用。

### 6.1 基本用法

```vue
<KhDetailDialog
  v-model="visible"
  title="任务详情"
  width="1000px"
  :data="detailData"
  :items="detailItems"
  :line-configs="lineConfigs"
  :column="2"
/>
```

### 6.2 适合场景

- 列表行详情。
- 订单头 + 明细行。
- 任务头 + 任务行。
- 只读审核信息。

### 6.3 配置建议

`items` 用于头部字段：

```js
const detailItems = [
  { prop: 'taskNo', label: '任务编号' },
  { prop: 'taskStatus', label: '状态', type: 'tag', tagMap: statusMap, tagTypeMap: statusTypeMap },
  { prop: 'createdTime', label: '创建时间' },
]
```

`lineConfigs` 用于从表：

```js
const lineConfigs = [
  {
    prop: 'lines',
    title: '明细行',
    columns: [
      { prop: 'lineNo', label: '行号', width: 80 },
      { prop: 'materialCode', label: '物料编码', width: 140 },
      { prop: 'qty', label: '数量', width: 100 },
    ],
  },
]
```

## 7. KhStatCard 指标卡片

`KhStatCard` 用于显示单个指标。可单独使用，也可通过 `KhPage.statCards` 批量显示。

### 7.1 基本用法

```vue
<KhStatCard
  label="库存总量"
  :value="123456"
  :icon="markRaw(Box)"
  theme="primary"
  clickable
  :formatter="(v) => v.toLocaleString() + ' 件'"
  @click="handleClick"
/>
```

### 7.2 props

| Prop | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `value` | `Number | String` | - | 指标值 |
| `label` | `String` | - | 指标名称 |
| `icon` | `Object | String` | - | 图标 |
| `iconSize` | `Number` | `28` | 图标尺寸 |
| `theme` | `String` | `'primary'` | `primary / success / warning / danger / info` |
| `shadow` | `String` | `'hover'` | 卡片阴影 |
| `clickable` | `Boolean` | `false` | 是否可点击 |
| `formatter` | `Function` | `null` | 格式化函数 |

建议图标用 `markRaw` 包装：

```js
import { markRaw } from 'vue'
import { Box } from '@element-plus/icons-vue'

const icon = markRaw(Box)
```

## 8. KhUpload 上传组件

`KhUpload` 封装文件上传，适合附件管理、导入、图片上传等场景。当前系统在附件页面使用。

### 8.1 使用场景

- 系统附件上传。
- 业务单据附件。
- 导入文件选择。
- 图片/文档上传。

### 8.2 使用建议

```vue
<KhUpload
  v-model:file-list="fileList"
  action="/api/attachment/upload"
  :limit="5"
  :max-size="10"
  accept=".xlsx,.xls,.pdf,.png,.jpg"
  @success="handleUploadSuccess"
  @error="handleUploadError"
/>
```

### 8.3 常见能力

从实现看，组件包含：

- 文件数量限制。
- 文件大小限制，超出会调用 `KhMessageFn.error`。
- 上传成功提示。
- 上传失败提示。
- 暴露内部关键方法给父组件调用。

使用建议：

- 导入 Excel 时明确 `accept`。
- 业务附件建议限制 `limit` 和 `maxSize`。
- 上传成功后刷新列表，不要只更新本地状态。
- 后端返回附件 ID 后，应与业务单据保存动作解耦。

## 9. KhMessage / KhMsgBox / KhNotify 反馈组件

反馈组件统一包装 Element Plus 的消息、确认框、通知能力。页面内应优先使用这些函数，而不是直接散落调用 Element Plus。

### 9.1 KhMessageFn

```js
import { KhMessageFn } from '@/components/KhMessage/index.vue'

KhMessageFn.success('保存成功')
KhMessageFn.warning('请选择至少一行数据')
KhMessageFn.error('操作失败')
KhMessageFn.info('功能待实现')
```

适合短反馈：

- 保存成功。
- 删除成功。
- 校验提醒。
- API 业务错误提示。

### 9.2 KhMsgBoxFn

```js
import { KhMsgBoxFn } from '@/components/KhMsgBox/index.vue'

KhMsgBoxFn.confirm('确认取消该任务吗？', '取消确认', {
  confirmButtonText: '确认',
  cancelButtonText: '取消',
  type: 'warning',
}).then(async () => {
  await cancelTask(row.id)
})
```

适合需要用户确认的动作：

- 删除。
- 取消任务。
- 完成任务。
- 提交审批。
- 回滚状态。

### 9.3 KhNotifyFn

```js
import { KhNotifyFn } from '@/components/KhNotify/index.vue'

KhNotifyFn.success('任务已完成')
KhNotifyFn.warning('库存低于预警值')
```

适合较长或系统级通知：

- WebSocket 通知。
- 后台任务完成。
- 预警消息。

## 10. KhLayout / KhMenu / KhFullscreen / KhNotification 页面外壳组件

这组组件主要由 `layouts/PcLayout.vue` 使用，普通业务页面一般不直接改。

### 10.1 KhLayout

`KhLayout` 负责 PC 后台主框架：

- 左侧菜单。
- 顶部栏。
- 面包屑/折叠按钮。
- 标签页。
- 主内容 `router-view`。
- `header-right` 插槽。

典型用法在 `PcLayout.vue`：

```vue
<KhLayout
  :menu-list="menuList"
  :menu-router="true"
  :show-tabs="true"
  home-key="/home"
  title="WMS"
>
  <template #header-right>
    <KhFullscreen />
    <KhNotification :messages="wsStore.notifications" />
  </template>
</KhLayout>
```

维护建议：

- 业务页面不要自己写侧边栏和顶部栏。
- 菜单数据从路由/权限 store 来。
- 顶部右侧工具通过 `header-right` 扩展。

### 10.2 KhMenu

`KhMenu` 是 `KhLayout` 内部菜单组件。普通页面无需直接使用。

适合改动：

- 菜单图标。
- 菜单折叠样式。
- 路由跳转行为。
- 权限菜单渲染。

### 10.3 KhFullscreen

全屏切换按钮，通常放在顶部右侧工具区。

### 10.4 KhNotification

通知入口，当前与 websocket store 的通知消息配合使用。

### 10.5 页面外壳组件完整用法

这组组件一般只在 `layouts/PcLayout.vue` 或顶层布局里使用，业务页面不要重复创建侧边栏、顶部栏。

```vue
<template>
  <KhLayout
    ref="layoutRef"
    title="KH.WMS"
    :menu-list="menuList"
    :menu-router="true"
    :show-tabs="true"
    home-key="/home"
    @select="handleMenuSelect"
  >
    <template #header-right>
      <KhFullscreen />
      <KhNotification
        :messages="notifications"
        :badge-props="{ max: 99 }"
        @click="handleNoticeClick"
        @read="markRead"
        @read-all="markAllRead"
      >
        <template #extra>
          <el-button link type="primary" @click="goNoticeCenter">查看全部</el-button>
        </template>
      </KhNotification>
    </template>

    <router-view />
  </KhLayout>
</template>
```

```js
const menuList = [
  {
    title: '基础资料',
    path: '/basedata',
    icon: 'Folder',
    children: [
      { title: '物料管理', path: '/basedata/material', icon: 'Box' },
      { title: '客户管理', path: '/basedata/customer', icon: 'User' },
    ],
  },
]

const notifications = ref([
  {
    id: 1,
    title: '库存预警',
    content: 'A001 可用库存低于安全库存',
    time: '09:30',
    read: false,
  },
])

function handleNoticeClick(message) {
  // 可跳转到对应业务详情。
}
```

使用注意：

- `KhLayout` 的默认插槽放页面主体，`header-right` 放全屏、通知、用户信息等顶部工具。
- `KhMenu` 通常由 `KhLayout` 内部使用；只有自定义布局时才单独使用。
- `KhFullscreen` 不需要传 props，可通过 ref 读取 `isFullscreen`。
- `KhNotification` 的 `messages` 建议由 websocket store 或通知 store 统一维护，点击时会把未读消息标记为已读并触发事件。

单独使用 `KhMenu` 的写法：

```vue
<KhMenu
  :menu-list="menuList"
  :collapse="false"
  :menu-router="false"
  active-index="/basedata/material"
  @select="handleSelect"
/>
```

常见问题：

| 现象 | 可能原因 | 处理办法 |
| --- | --- | --- |
| 菜单点击不跳转 | `menu-router=false` 或 path 不匹配路由 | 开启 `menu-router` 或在 `select` 中手动跳转 |
| 顶部通知数量不对 | `messages.read` 字段不是布尔值 | 统一消息结构，未读用 `read: false` |
| 通知点击后没有进入业务页 | 组件只 emit，不负责路由 | 在 `@click` 中根据 message 跳转 |
| 业务页面重复出现侧边栏 | 在业务页里再次使用了 `KhLayout` | `KhLayout` 只放在顶层布局 |

## 11. KhDashboard 看板组件

`KhDashboard` 用于监控/大屏/运营看板类页面。当前 `WarehouseDashboard.vue` 使用。

### 11.1 使用场景

- 仓库运行总览。
- 输送线/堆垛机监控。
- 入出库趋势。
- 库存状态总览。

### 11.2 使用建议

- 看板页面通常不用 `KhPage`，因为主内容不是列表。
- 指标用 `KhStatCard` 或看板内部卡片。
- 图表数据加载、刷新周期、异常提示要放在页面层。
- 实时监控应注意定时器和 websocket 的卸载。

### 11.3 完整用法

```vue
<template>
  <KhDashboard
    ref="dashboardRef"
    title="仓库运行看板"
    :stats="stats"
    :charts="charts"
    :stat-span="6"
  />
</template>
```

```js
import { ref, onMounted, onBeforeUnmount, nextTick } from 'vue'

const dashboardRef = ref(null)

const stats = ref([
  { label: '今日入库', value: 128, color: '#409EFF' },
  { label: '今日出库', value: 96, color: '#67C23A' },
  { label: '库存预警', value: 12, color: '#E6A23C' },
  { label: '异常任务', value: 3, color: '#F56C6C' },
])

const charts = ref([
  {
    type: 'line',
    title: '出入库趋势',
    span: 12,
    option: {
      tooltip: { trigger: 'axis' },
      xAxis: { type: 'category', data: ['08:00', '10:00', '12:00', '14:00'] },
      yAxis: { type: 'value' },
      series: [
        { name: '入库', type: 'line', data: [12, 28, 35, 42] },
        { name: '出库', type: 'line', data: [8, 20, 31, 38] },
      ],
    },
  },
  {
    type: 'pie',
    title: '库区占比',
    span: 12,
    option: {
      tooltip: { trigger: 'item' },
      series: [
        {
          type: 'pie',
          radius: '60%',
          data: [
            { name: '原料区', value: 40 },
            { name: '成品区', value: 35 },
            { name: '暂存区', value: 25 },
          ],
        },
      ],
    },
  },
])

let timer = null

onMounted(async () => {
  await loadDashboard()
  await nextTick()
  dashboardRef.value?.refresh()
  timer = window.setInterval(loadDashboard, 60_000)
})

onBeforeUnmount(() => {
  if (timer) window.clearInterval(timer)
})

async function loadDashboard() {
  const data = await queryDashboard()
  stats.value = data.stats
  charts.value = data.charts
  await nextTick()
  dashboardRef.value?.refresh()
}
```

常见问题：

| 现象 | 可能原因 | 处理办法 |
| --- | --- | --- |
| 图表空白 | 容器还没有尺寸或 option 为空 | 数据加载后 `nextTick` 再 `refresh()` |
| 切换 tab 后图表变形 | 容器尺寸变化未 resize | tab 激活后调用 `dashboardRef.value?.resize()` |
| 数据更新但图表不变 | 只改了数组内容，图表实例未重建 | 更新 charts 后调用 `refresh()` |
| 定时器导致重复请求 | 页面卸载时未清理 | `onBeforeUnmount` 中 `clearInterval` |

## 12. KhEditableTable 可编辑表格

`KhEditableTable` 适合明细行录入，比如入库组盘、盘点明细、临时任务行。

### 12.1 基本思路

```vue
<KhEditableTable
  v-model="lines"
  :columns="lineColumns"
  @cell-change="handleCellChange"
  @add="handleAddLine"
  @delete="handleDeleteLine"
/>
```

### 12.2 适合场景

- 弹窗里录入多行物料。
- 盘点实际数量填写。
- 入库收货明细。
- 手工创建临时任务行。

### 12.3 使用建议

- 行数据必须有稳定 key。
- 数量、库位、批次等字段应在行内校验。
- 提交前在父组件统一校验整张明细表。
- 如果明细行太复杂，优先拆成业务子组件，不要把所有逻辑塞进页面主文件。

### 12.4 完整可复制示例

下面是弹窗里录入“主表 + 明细行”的推荐写法。`KhEditableTable` 只负责编辑明细行，保存、校验、接口提交都放在父组件。

```vue
<template>
  <KhDialog
    v-model="visible"
    title="创建入库单"
    width="960px"
    :show-footer="true"
    @confirm="handleSubmit"
  >
    <KhForm
      ref="formRef"
      v-model="formModel"
      :columns="formColumns"
      :col-count="2"
    />

    <KhEditableTable
      v-model="detailLines"
      row-key="rowId"
      :columns="lineColumns"
      :default-row="createDefaultLine"
      :max-height="360"
      add-text="添加明细"
      @cell-change="handleLineChange"
      @add="handleAddLine"
      @delete="handleDeleteLine"
    >
      <template #materialCode="{ row, index }">
        <el-input
          v-model="row.materialCode"
          placeholder="扫描或输入物料编码"
          clearable
          @change="() => handleMaterialChange(row, index)"
        />
      </template>

      <template #action="{ row, index }">
        <el-button link type="primary" @click="copyLine(row)">复制</el-button>
        <el-button link type="danger" @click="deleteLine(index)">删除</el-button>
      </template>
    </KhEditableTable>
  </KhDialog>
</template>
```

```js
import { ref } from 'vue'
import { KhMessageFn } from '@/components/KhMessage/index.vue'

const visible = ref(false)
const formRef = ref(null)

const formModel = ref({
  supplierId: null,
  warehouseId: null,
  remark: '',
})

const detailLines = ref([])

const formColumns = [
  { prop: 'supplierId', label: '供应商', type: 'select', required: true, options: [] },
  { prop: 'warehouseId', label: '仓库', type: 'select', required: true, options: 'dict:warehouse' },
  { prop: 'remark', label: '备注', type: 'textarea', span: 2 },
]

const createDefaultLine = () => ({
  rowId: crypto.randomUUID(),
  materialCode: '',
  materialName: '',
  batchNo: '',
  qty: 1,
  unit: '',
  productionDate: '',
  qualityStatus: 1,
})

const lineColumns = [
  { prop: 'materialCode', label: '物料编码', type: 'slot', minWidth: 160 },
  { prop: 'materialName', label: '物料名称', minWidth: 160 },
  { prop: 'batchNo', label: '批次', type: 'input', minWidth: 140, placeholder: '请输入批次' },
  { prop: 'qty', label: '数量', type: 'number', width: 140, min: 1, precision: 2 },
  { prop: 'unit', label: '单位', type: 'select', width: 120, options: 'dict:unit' },
  { prop: 'productionDate', label: '生产日期', type: 'date', width: 160, valueFormat: 'YYYY-MM-DD' },
  {
    prop: 'qualityStatus',
    label: '质检状态',
    type: 'switch',
    width: 120,
    activeValue: 1,
    inactiveValue: 0,
  },
]

function handleLineChange(prop, row, index) {
  if (prop === 'qty' && row.qty <= 0) {
    KhMessageFn.warning(`第 ${index + 1} 行数量必须大于 0`)
  }
}

function handleAddLine(row) {
  // 可在这里补默认仓库、默认批次等业务数据。
}

function handleDeleteLine(row, index) {
  // 默认删除没有二次确认，关键业务可以改用 action slot 自己确认。
}

function deleteLine(index) {
  detailLines.value.splice(index, 1)
}

function copyLine(row) {
  detailLines.value.push({
    ...row,
    rowId: crypto.randomUUID(),
  })
}

async function handleSubmit() {
  await formRef.value?.validate?.()
  validateDetailLines()

  const dto = {
    ...formModel.value,
    lines: detailLines.value.map((line, index) => ({
      lineNo: index + 1,
      materialCode: line.materialCode,
      batchNo: line.batchNo,
      qty: line.qty,
      unit: line.unit,
      productionDate: line.productionDate,
      qualityStatus: line.qualityStatus,
    })),
  }

  await createInboundOrder(dto)
  KhMessageFn.success('保存成功')
  visible.value = false
}

function validateDetailLines() {
  if (detailLines.value.length === 0) {
    throw new Error('请至少添加一行明细')
  }

  detailLines.value.forEach((line, index) => {
    const rowNo = index + 1
    if (!line.materialCode) throw new Error(`第 ${rowNo} 行请选择物料`)
    if (!line.qty || line.qty <= 0) throw new Error(`第 ${rowNo} 行数量必须大于 0`)
    if (!line.unit) throw new Error(`第 ${rowNo} 行请选择单位`)
  })
}
```

### 12.5 columns 字段写法

| 字段 | 适用类型 | 说明 |
| --- | --- | --- |
| `prop` / `label` | 全部 | 数据字段和表头 |
| `width` / `minWidth` / `fixed` | 全部 | 列宽和固定列 |
| `align` / `headerAlign` | 全部 | 内容和表头对齐，默认居中 |
| `visible` | 全部 | `false` 时隐藏 |
| `showOverflowTooltip` | 文本列 | 文本超长提示 |
| `type: 'input'` | 文本输入 | 支持 `placeholder`、`maxlength`、`disabled`、`clearable` |
| `type: 'number'` | 数量/金额 | 支持 `min`、`max`、`precision`、`step`、`controls` |
| `type: 'select'` | 枚举 | 支持 `options`、`multiple`、`filterable`、`clearable` |
| `type: 'date'` | 日期 | 支持 `dateType`、`valueFormat`、`placeholder` |
| `type: 'switch'` | 是否/状态 | 支持 `activeValue`、`inactiveValue`、`disabled` |
| `type: 'slot'` | 复杂编辑 | 使用同名插槽完全自定义 |

`options` 支持数组、函数、以及 `dict:xxx` 字典引用。字典会在组件初始化时预加载并解析。

### 12.6 事件怎么用

| 事件 | 参数 | 触发时机 | 常见用途 |
| --- | --- | --- | --- |
| `update:modelValue` | `list` | 新增、删除或 v-model 更新 | 同步明细数组 |
| `cell-change` | `prop, row, index` | 某个编辑控件 change | 联动计算、行内校验 |
| `add` | `newRow` | 点击新增行 | 补业务默认值、滚动到底部 |
| `delete` | `removed, index` | 默认删除按钮删除 | 记录删除项、同步后端删除队列 |

注意：行内输入直接修改的是 `row[prop]`。如果父组件只依赖深层 watch，可能不如监听 `cell-change` 清晰；提交前仍应直接读取 `detailLines.value` 做最终校验。

### 12.7 常见业务模式

**物料选择后回填名称、单位、规格：**

```js
async function handleMaterialChange(row, index) {
  if (!row.materialCode) return
  const material = await getMaterialByCode(row.materialCode)
  if (!material) {
    KhMessageFn.warning(`第 ${index + 1} 行物料不存在`)
    row.materialName = ''
    row.unit = ''
    return
  }

  row.materialName = material.materialName
  row.unit = material.unit
  row.spec = material.spec
}
```

**数量变更后自动计算金额：**

```js
function handleLineChange(prop, row) {
  if (prop === 'qty' || prop === 'price') {
    row.amount = Number(row.qty || 0) * Number(row.price || 0)
  }
}
```

**删除前需要确认：**

```vue
<template #action="{ index }">
  <el-popconfirm title="确定删除该明细？" @confirm="deleteLine(index)">
    <template #reference>
      <el-button link type="danger">删除</el-button>
    </template>
  </el-popconfirm>
</template>
```

### 12.8 使用边界

- `KhEditableTable` 不负责表单校验规则展示；复杂校验统一在父组件提交前处理。
- 默认删除按钮没有二次确认，关键明细建议用 `action` slot 自己实现。
- 不适合几百上千行大数据编辑；大批量编辑应拆成导入、分批保存或专用页面。
- 行内控件较多时，弹窗宽度要足够，必要时用全屏弹窗或拆成“编辑明细”子弹窗。
- 需要行内远程搜索、扫码、批次弹窗选择时，优先用 `type: 'slot'` 自定义单元格。

### 12.9 常见问题

| 现象 | 可能原因 | 处理办法 |
| --- | --- | --- |
| 点击添加后字段都是空 | `defaultRow` 没有返回完整结构 | 给每个列字段准备默认值 |
| 删除后行状态错乱 | `rowKey` 不稳定或复用了 id | 新增临时行使用 `rowId/uuid` |
| select 没有选项 | `options` 为空、函数未返回数组或字典不存在 | 检查数组结构和 `dict:xxx` |
| 输入后父组件计算没触发 | 只修改了行对象内部字段 | 使用 `cell-change` 做联动 |
| 提交后后端类型错误 | number/switch/select 值类型和 DTO 不一致 | 提交前统一转换类型 |
| slot 列不显示 | 列 `type` 不是 `slot` 或插槽名和 `prop` 不一致 | `type: 'slot'` + `#propName` |
| 操作列太窄 | 默认 `actionWidth` 只有 70 | 设置 `:action-width="140"` 或自定义 action slot |
| 弹窗里表格高度撑破 | 明细行过多且未限制高度 | 设置 `max-height`，外层弹窗给合适宽高 |

## 13. 低频但可复用组件速查

以下组件不是每个页面都会用，但遇到对应场景时应优先复用。本章按组件逐个说明：先讲适用场景，再给常用写法和注意点。

### 13.1 KhAlert

用于统一警告/提示块，适合表单顶部说明、风险提示、配置影响说明。

```vue
<KhAlert
  type="warning"
  title="操作提醒"
  description="修改库位类型会影响后续上架策略，请确认后再保存。"
  show-icon
  :closable="false"
/>
```

只需要页面内静态提示时用 `KhAlert`；保存成功、接口失败、校验提醒这类短反馈用 `KhMessageFn`。

### 13.2 KhCollapse

用于折叠面板，适合配置分组、详情分段、复杂表单分区。

```vue
<KhCollapse v-model="activePanels" @change="handleCollapseChange">
  <el-collapse-item title="基础信息" name="base">
    <KhForm v-model="formModel" :columns="baseColumns" />
  </el-collapse-item>
  <el-collapse-item title="扩展配置" name="extend">
    <KhForm v-model="formModel" :columns="extendColumns" />
  </el-collapse-item>
</KhCollapse>
```

```js
const activePanels = ref(['base'])
```

普通模式 `v-model` 用数组。当前组件虽然声明了 `accordion`，但没有把它绑定到内部 `el-collapse`，所以 `accordion` 在当前实现下不会生效；强手风琴场景优先直接用原生 `el-collapse`，或先修组件模板为 `:accordion="accordion"`。

### 13.3 KhColorPicker

颜色选择器，当前也可由 `KhForm` 的 `type: 'color-picker'` 间接使用。菜单、标签、状态颜色配置优先用它。

```vue
<KhColorPicker
  v-model="tagColor"
  placeholder="请选择标签颜色"
  :predefine="['#409EFF', '#67C23A', '#E6A23C', '#F56C6C']"
  @change="handleColorChange"
/>
```

在 `KhForm` 中使用：

```js
const columns = [
  { prop: 'tagColor', label: '标签颜色', type: 'color-picker' },
]
```

颜色值推荐保存为 `#409EFF` 这类字符串。选择器面板会 Teleport 到 body，放在弹窗、抽屉、表格单元格里一般不会被 overflow 裁剪。

### 13.4 KhIconPicker

图标选择器，当前也可由 `KhForm` 的 `type: 'icon-picker'` 间接使用。菜单图标配置优先用它。

```vue
<KhIconPicker
  v-model="menuIcon"
  placeholder="请选择菜单图标"
  @change="handleIconChange"
/>
```

在 `KhForm` 中使用：

```js
const columns = [
  { prop: 'menuIcon', label: '菜单图标', type: 'icon-picker' },
]
```

图标值保存的是 Element Plus 图标名称字符串，比如 `Setting`、`Menu`。保存前不需要转成组件对象。

### 13.5 KhDragList

拖拽排序列表，适合策略步骤、菜单排序、字段排序等需要拖拽改变顺序的场景。

```vue
<KhDragList
  v-model="strategySteps"
  row-key="id"
  label-key="stepName"
  :show-remove="true"
  @change="handleSortChange"
  @remove="handleRemoveStep"
  @drag-end="saveSort"
>
  <template #item="{ element, index }">
    <div class="step-row">
      <span>{{ index + 1 }}. {{ element.stepName }}</span>
      <el-tag size="small">{{ element.strategyType }}</el-tag>
    </div>
  </template>
</KhDragList>
```

```js
const strategySteps = ref([
  { id: 1, stepName: '校验库存', strategyType: 'check' },
  { id: 2, stepName: '分配库位', strategyType: 'allocate' },
])

async function saveSort() {
  await saveStepSort(strategySteps.value.map((item, index) => ({
    id: item.id,
    sort: index + 1,
  })))
}
```

`row-key` 必须稳定。拖拽只会更新前端数组，不会自动保存后端顺序。

### 13.6 KhSortList

按钮式上下移动排序，不依赖拖拽，适合排序项较少、需要明确“上移/下移”按钮的管理页。

```vue
<KhSortList
  ref="sortListRef"
  v-model="rules"
  row-key="id"
  label-key="ruleName"
  hint="数值越靠前优先级越高"
  @sort="handleSort"
  @remove="handleRemove"
/>
```

```js
const rules = ref([
  { id: 'a', ruleName: '优先同巷道' },
  { id: 'b', ruleName: '优先近库位' },
])

function handleSort(list) {
  // 可以在这里节流保存，也可以等用户点击保存按钮时统一提交。
}
```

排序后仍需要调用保存接口。`rowKey` 重复会导致移动或删除错乱。

### 13.7 KhSideDrawer

侧边抽屉，适合不打断主列表上下文的详情、筛选、辅助配置。

```vue
<KhSideDrawer
  v-model="drawerVisible"
  title="库存详情"
  width="520px"
  placement="right"
  @open="loadDetail"
  @close="resetDetail"
>
  <KhForm v-model="detail" :columns="detailColumns" :disabled="true" />

  <template #footer>
    <el-button @click="drawerVisible = false">关闭</el-button>
    <el-button type="primary" @click="openAdjustDialog">库存调整</el-button>
  </template>
</KhSideDrawer>
```

抽屉适合轻量详情和辅助操作。不建议在抽屉里再套复杂多步骤流程；复杂流程优先独立页面或 `KhDialog`。

### 13.8 KhSteps

流程步骤组件，适合入库、出库、任务、异常处理的状态流转展示。

```vue
<KhSteps
  :active="activeStep"
  :steps="steps"
  finish-status="success"
>
  <template #icon-1>
    <el-icon><Upload /></el-icon>
  </template>
</KhSteps>
```

```js
const activeStep = ref(1)
const steps = [
  { title: '创建单据', description: '录入基础信息' },
  { title: '提交审核', description: '等待主管确认' },
  { title: '执行入库', description: '生成库存流水' },
]
```

`active` 是从 0 开始的索引。业务状态通常需要先映射成步骤索引，不要直接把后端状态码传进去。

### 13.9 KhTimeline

时间线展示组件，适合日志、审批记录、状态流转。

```vue
<KhTimeline :items="logs">
  <template #item-0="{ item }">
    <strong>{{ item.title }}</strong>
    <div>{{ item.content }}</div>
  </template>
</KhTimeline>
```

```js
const logs = [
  { time: '2026-06-25 09:00', type: 'primary', title: '创建任务', content: '系统自动创建' },
  { time: '2026-06-25 09:15', type: 'success', title: '任务完成', content: '操作员确认完成' },
]
```

每项建议至少有 `time/title/content`，否则时间线会显得信息不足。

### 13.10 KhTransfer

穿梭选择器，适合角色分配权限、用户分配角色、策略分配对象。

```vue
<KhTransfer
  v-model="selectedPermissionIds"
  :data="permissionOptions"
  filter-placeholder="搜索权限"
  @change="handlePermissionChange"
/>
```

```js
const selectedPermissionIds = ref([])
const permissionOptions = ref([
  { key: 'sys:user:add', label: '用户新增' },
  { key: 'sys:user:edit', label: '用户编辑' },
])
```

`data` 结构沿用 Element Plus Transfer，默认需要 `key` 和 `label`。

### 13.11 KhNoticeBar

顶部公告条，适合系统公告、停机提醒、预警滚动。当前首页使用这类组件。

```vue
<KhNoticeBar
  type="warning"
  text="今晚 22:00-23:00 系统维护，期间可能短暂不可用。"
  scrollable
  :speed="60"
  @close="handleNoticeClose"
/>
```

也可以用默认插槽：

```vue
<KhNoticeBar type="info" :closable="false">
  当前仓库存在 3 条库存预警，请及时处理。
</KhNoticeBar>
```

短文本不建议开启滚动；只有公告较长或顶部空间很窄时再开启 `scrollable`。

### 13.12 KhLoading

统一 Loading 容器，适合局部遮罩或长任务等待。

```vue
<KhLoading :loading="loading" text="数据加载中...">
  <KhTable :columns="columns" :data="list" />
</KhLoading>
```

```js
const loading = ref(false)

async function loadData() {
  loading.value = true
  try {
    list.value = await queryList()
  } finally {
    loading.value = false
  }
}
```

`fullscreen` 只适合页面初始化或全局切换，不建议包住普通业务请求。普通列表、弹窗、局部区域优先用非全屏 loading。

### 13.13 KhWaterfall

瀑布流布局，后台业务较少使用，适合图片、附件预览类页面。

```vue
<KhWaterfall
  :items="attachments"
  :columns="3"
  :gap="12"
  :loading="loading"
  :finished="finished"
>
  <template #default="{ item }">
    <el-card shadow="hover">
      <img :src="item.url" class="attachment-cover" />
      <div>{{ item.fileName }}</div>
    </el-card>
  </template>
</KhWaterfall>
```

当前组件只负责瀑布流展示和 loading/finished 状态展示。源码声明了 `load-more` 事件，但组件内部没有滚动监听，也不会主动触发该事件；无限滚动需要父级自己监听滚动并加载下一页。后台管理页如果卡片高度差不多，普通 grid 往往比瀑布流更稳定。

### 13.14 KhPageHeader

页面头部标题组件，适合详情页、流程页、非 `KhPage` 页面顶部。

```vue
<KhPageHeader
  title="库存调整详情"
  subtitle="查看调整单基础信息、明细和审批记录"
  :breadcrumb="breadcrumb"
  @back="router.back()"
>
  <template #extra>
    <el-button @click="router.back()">返回列表</el-button>
    <el-button type="primary" @click="handleSubmit">提交审核</el-button>
  </template>

  <template #content>
    <KhAlert type="info" title="当前单据处于草稿状态" show-icon />
  </template>
</KhPageHeader>
```

```js
const breadcrumb = [
  { title: '库存管理', to: '/inventory' },
  { title: '库存调整', to: '/inventory/adjust' },
  { title: '详情' },
]
```

返回按钮只触发 `back` 事件，不会自动调用路由返回。页面要自己决定是 `router.back()`、跳列表页，还是关闭当前 tab。
## 14. 按开发场景组织组件

前面是“组件手册”，本章按开发任务反向选择组件。

### 14.1 标准基础资料 CRUD 页面

适用模块：

- `basedata`
- `warehouse`
- `config`
- `system` 中的简单实体

推荐组合：

```text
KhPage
├─ searchColumns
├─ searchModel
├─ tableColumns
├─ formColumns
└─ module + permissionPrefix
```

开发步骤：

1. 定义 `module`，确认后端接口是否符合 `/api/{module}/pagelist` 等约定。
2. 定义 `searchModel` 和 `searchColumns`。
3. 定义 `tableColumns`。
4. 定义 `formColumns`。
5. 配置 `permissionPrefix`。
6. 如需详情，打开 `crudOperations.view`。
7. 如需扩展字段，用 `useExtFields` 合并列和表单。

模板：

```vue
<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage
      ref="pageRef"
      module="xxx"
      title="xxx管理"
      :search-columns="searchColumns"
      :search-model="searchModel"
      :columns="tableColumns"
      :form-columns="formColumns"
      :crud-operations="crudOperations"
      permission-prefix="xxx:xxx"
      :show-header-filter="true"
      :search-col-count="3"
    />
  </div>
</template>
```

### 14.2 带状态机的业务列表页

适用模块：

- 入库单。
- 出库单。
- 任务。
- 库存调整。
- 盘点。
- 异常处理。

推荐组合：

```text
KhPage
├─ 内置 CRUD 只保留适合的部分
├─ actionButtons 承载状态动作
├─ KhDialog 承载复杂动作弹窗
└─ KhMsgBoxFn / KhMessageFn 做确认和反馈
```

示例模式：

```js
const actionButtons = [
  {
    label: '提交',
    permission: 'inv:adjust:submit',
    type: 'primary',
    show: (row) => row.status === 'DRAFT',
    onClick: (row) => handleSubmit(row),
  },
  {
    label: '作废',
    permission: 'inv:adjust:cancel',
    type: 'danger',
    show: (row) => row.status !== 'APPROVED',
    onClick: (row) => handleCancel(row),
  },
]
```

建议：

- 状态推进必须由后端校验，前端只做展示和触发。
- 行按钮显隐优先使用后端返回的 `allowedActions`。
- 操作成功后调用 `pageRef.value?.reload()`。
- 复杂输入用自定义 `KhDialog`，不要把一堆输入塞进确认框。

### 14.3 只读报表列表页

适用模块：

- 日志。
- 库存查询。
- 报表明细。
- 任务查询。

推荐组合：

```text
KhPage
├─ showStatCards/statCards
├─ searchColumns
├─ tableColumns
├─ crudOperations: create/update/delete=false
└─ 自定义 load 或 module 分页
```

示例：

```vue
<KhPage
  module="inventory-header"
  title="库存查询"
  :stat-cards="statCards"
  :search-columns="searchColumns"
  :search-model="searchModel"
  :columns="tableColumns"
  :crud-operations="{ create: false, update: false, delete: false, view: true, export: true }"
/>
```

### 14.4 弹窗内业务处理

适用场景：

- 冻结库存。
- 异常处理。
- 分配货位。
- 收货确认。
- 组盘。

推荐组合：

```text
父页面 KhPage
└─ 子组件或页面内 KhDialog
   ├─ KhForm
   ├─ KhEditableTable
   └─ success 事件刷新 KhPage
```

建议：

- 弹窗内有完整业务流程时，优先抽成 `views/{domain}/components/XxxDialog.vue`。
- 弹窗成功后 `emit('success')`，父页面负责 `pageRef.value?.reload()`。
- 弹窗内部不要直接改父页面表格状态。

### 14.5 主从表/明细录入页面

推荐组合：

```text
KhPage 主表
└─ KhDialog
   ├─ KhForm 主信息
   └─ KhEditableTable 明细行
```

提交时：

1. 校验主表单。
2. 校验明细行不能为空。
3. 校验每行关键字段。
4. 组装 DTO。
5. 调接口。
6. 关闭弹窗并刷新 `KhPage`。

### 14.6 看板页面

推荐组合：

```text
KhDashboard 或自定义看板页
├─ KhStatCard
├─ ECharts/监控组件
└─ KhMessageFn 反馈异常
```

不建议使用 `KhPage`，除非看板下方有独立列表。

### 14.7 PDA 页面

PDA 页面通常不使用 `KhPage`。推荐：

- 使用 `PdaLayout`。
- 大按钮、大字号。
- 扫码输入框优先。
- 少弹窗，必要弹窗要适配小屏。
- 操作反馈用 `KhMessageFn`。

## 15. 新页面开发检查清单

开发前：

- 是否是标准 CRUD？是则优先 `KhPage`。
- 后端接口是否符合 `useCrudApi(module)` 约定？
- 权限码前缀是否已在菜单/按钮权限中配置？
- 字典项是否已经存在？

开发中：

- `searchColumns` 与 `searchModel` 字段是否一致？
- `columns` 是否给长文本设置 `showOverflowTooltip`？
- 状态列是否使用 `type: 'tag'` 和字典？
- `formColumns` 必填、长度、数值范围是否完整？
- switch 是否设置了 `activeValue` / `inactiveValue`？
- 行按钮是否配置 `permission`？
- 复杂按钮是否配置 `show(row)`？
- 操作成功后是否刷新 `pageRef`？

提交前：

- 页面外层是否有高度和 flex 布局？
- 表格是否能分页、刷新、搜索、重置？
- 新增、编辑、删除、查看权限是否符合预期？
- 空数据、加载中、接口失败是否有反馈？
- 字典是否正确显示？
- 弹窗关闭后再次打开是否没有旧数据残留？
- 批量操作是否处理“未选择数据”？
- 状态机动作是否由后端最终校验？

## 16. 推荐真实页面参考

| 文件 | 推荐学习点 |
| --- | --- |
| `KH.WMS.Client/src/views/basedata/material.vue` | `KhPage + useExtFields + 自定义 load + beforeSubmit` |
| `KH.WMS.Client/src/views/system/user.vue` | 标准用户 CRUD 页面 |
| `KH.WMS.Client/src/views/task/list.vue` | 只读任务列表 + 自定义 actionButtons + detailLines |
| `KH.WMS.Client/src/views/inbound/order.vue` | 状态机页面 + 自定义弹窗 + 成功刷新 |
| `KH.WMS.Client/src/views/inventory/stock-query.vue` | 查询页 + statCards + 自定义行操作 |
| `KH.WMS.Client/src/views/config/code-rule.vue` | `KhPage` 外加自定义 `KhDialog/KhForm` 的复杂配置页 |

## 17. 全量组件深度说明与问题清单

本章用于补齐所有 `KhXxx` 组件的详细说明。前面章节偏“常用开发路径”，本章偏“查手册和排错”。每个组件都按以下角度阅读：

- **定位**：组件解决什么问题。
- **接口**：props、events、slots、expose。
- **用法**：最小示例和组合方式。
- **问题清单**：尽可能覆盖开发、联调、运行时会遇到的现象、原因、处理办法和预防建议。

### 17.1 KhPage

`KhPage` 是页面级组件，负责把搜索、表格、分页、工具栏、CRUD 弹窗、详情弹窗整合成一个配置式页面。它也是最容易出问题的组件，因为它同时连着 API、权限、字典、表格高度、弹窗状态和业务钩子。

#### 17.1.1 核心接口补充

| 类别 | 名称 | 说明 |
| --- | --- | --- |
| props | `title` | 页面标题，同时影响表格标题和弹窗标题 |
| props | `statCards` / `showStatCards` | 顶部指标卡片 |
| props | `searchColumns` / `searchModel` | 查询表单配置与查询模型 |
| props | `columns` | 表格列配置，必传 |
| props | `load` | 自定义加载函数，优先级高于内置 `module` |
| props | `module` | CRUD 模块名，启用内置接口 |
| props | `formColumns` / `customFormData` | 新增编辑弹窗表单配置和默认值 |
| props | `crudOperations` | 内置新增、编辑、删除、查看、导出按钮开关 |
| props | `permissionPrefix` / `permissionMap` | 内置按钮权限码拼接 |
| props | `beforeCreate` / `beforeUpdate` / `beforeDelete` / `beforeSubmit` / `afterSubmit` | 业务钩子 |
| props | `updateShow` / `deleteShow` | 内置编辑/删除按钮行级显隐 |
| props | `detailLines` | 详情弹窗从表配置 |
| props | `toolbarButtons` / `actionButtons` | 追加工具栏和操作列按钮 |
| emits | `search` | 搜索后触发，参数为 `searchModel` |
| emits | `reset` | 重置后触发 |
| emits | `stat-click` | 点击指标卡片触发 |
| slots | `toolbar-left` / `toolbar-right` | 表格工具栏左右侧 |
| slots | `action` | 完全覆盖操作列 |
| slots | `search-extra-buttons` | 搜索按钮旁追加按钮 |
| slots | `stat-extra` / `stat-extra-row` | 指标卡扩展 |
| expose | `reload()` / `refresh()` | 重载第 1 页 / 刷新当前页 |
| expose | `getSelectionRows()` / `clearSelection()` | 多选行读取与清空 |
| expose | `openCreateDialog()` / `openUpdateDialog(row)` / `openDetailDialog(row)` | 手动打开内置弹窗 |

#### 17.1.2 配置覆盖关系

`KhPage` 的配置要按“声明、透传、覆盖”理解：

| 配置来源 | 能否在 KhPage 上用 | 说明 |
| --- | --- | --- |
| `KhPage` 自己声明的 props | 可以 | 例如 `module`、`searchColumns`、`formColumns`、`crudOperations` |
| `KhTable` 的部分 props | 可以透传 | 例如 `show-header-filter`、`row-style`、`action-width`、`page-sizes` |
| `KhTable` 的事件 | 不能直接透传 | `tableAttrs` 过滤 `on*`，不能直接在 `KhPage` 上监听 `selection-change` |
| `KhForm` 的搜索区 props | 只有少量被包装 | `searchColCount/collapsible/defaultCollapsed/quickSearchPlaceholder` |
| `KhDialog` 的内置弹窗 props | 只有少量被包装 | `dialogWidth/dialogColCount`，其他弹窗属性不能直接传入内置弹窗 |
| `KhDetailDialog` 的内置详情 props | 只有少量被包装 | `detailWidth/detailLines`，详情列来自 `columns` |

`KhPage` 已包装的表格配置：`columns`、`load`、`searchModel -> extraParams`、`showToolbar`、`showPagination`、`showIndex`、`showSelection`、`border`、`stripe`、`title`、`toolbarButtons`、`actionButtons`、自动计算的 `height`。

`KhPage` 可继续透传给 `KhTable` 的常用配置：`data`、`loading`、`row-key`、`size`、`max-height`、`highlight-current-row`、`default-expand-all`、`tree-props`、`header-cell-style`、`row-style`、`row-class-name`、`selection-width`、`selection-fixed`、`index-width`、`index-fixed`、`action-label`、`action-width`、`action-min-width`、`action-fixed`、`page-size`、`page-sizes`、`show-refresh`、`show-column-setting`、`show-header-filter`、`show-selection-info`、`auto-load`。

不建议在 `KhPage` 上混用的表格配置：`data/total/page-num` 与 `module/load` 混用、`height` 与自动高度混用、`actionColumns` 与 `actionButtons` 混用。能用 `KhPage` 的配置就用 `KhPage` 的配置，需要完全控制表格时再退回 `KhTable`。

搜索区 `KhForm` 只包装这些能力：`searchColumns`、`searchModel`、`searchColCount`、`collapsible`、`defaultCollapsed`、`quickSearchPlaceholder`、`search-extra-buttons`。`KhForm` 的 `labelWidth/labelPosition/inline/gutter/size/disabled/showFooter` 不会作为搜索区 props 直接透传；要么写进 `searchColumns` 字段配置，要么自组 `KhForm + KhTable`。

#### 17.1.3 标准写法

```vue
<KhPage
  ref="pageRef"
  module="material"
  title="物料管理"
  :search-columns="searchColumns"
  :search-model="searchModel"
  :columns="tableColumns"
  :form-columns="formColumns"
  :crud-operations="{ create: true, update: true, delete: true, view: true, export: true }"
  permission-prefix="bd:material"
  :show-header-filter="true"
  :search-col-count="3"
  :before-submit="beforeSubmit"
/>
```

#### 17.1.4 问题清单

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 页面空白或表格高度为 0 | 外层容器没有高度，`KhPage` flex 无法计算 | 外层加 `height: 100%; display: flex; flex-direction: column;` | 新列表页统一套固定外层模板 |
| 表格只显示一点点高度 | 父级布局没有 `min-height: 0` 或被其他容器撑坏 | 检查布局链路，父级 flex 子项加 `min-height: 0` | 列表页不要再包多层无意义 card |
| 新增按钮不显示 | `module` 为空、`crudOperations.create=false`、权限不足 | 检查 `module`、`crudOperations`、`permissionPrefix` | 新页面先用管理员账号验证权限 |
| 编辑/删除按钮不显示 | `crudOperations` 关闭、`updateShow/deleteShow` 返回 false、权限不足 | 暂时去掉 show 函数定位问题 | 状态按钮显隐逻辑写成独立函数便于调试 |
| 自定义 actionButtons 不显示 | 按钮配置没有 `permission` 或权限码不匹配 | 对照系统按钮权限配置 | 自定义按钮必须写权限码和 show 条件 |
| 点击删除没有二次确认文案正确显示 | `deleteConfirmText` 未传或文案被乱码/空值覆盖 | 显式传 `delete-confirm-text` | 删除按钮统一写业务名，例如“确定删除该物料？” |
| 搜索没有生效 | `searchColumns.prop` 与 `searchModel` 或后端字段不一致 | 确认三者字段名一致 | 新建页面时先写 searchModel 再复制 prop |
| 重置后仍残留查询条件 | `searchModel` 中字段没有被初始化，或字段值是数组/对象 | 手动重置特殊字段，或监听 reset | 数组/日期范围字段在 `handleReset` 后额外清空 |
| 收起态快速搜索查错字段 | 快速搜索默认使用第一个查询字段 | 传 `quick-search-placeholder` 并调整第一字段 | 第一查询字段放最常用关键词 |
| 分页页码不重置 | 调用了 `refresh()` 而非 `reload()` | 搜索/条件变化用 `reload()` | 操作成功可用 `refresh()`，条件变化用 `reload()` |
| 接口地址不对 | `module` 不符合后端路由 | 改 `module` 或传自定义 `load` | 新增模块前先确认后端 controller route |
| 分页参数后端收不到 | 后端不兼容 `buildPageQuery` 结构 | 自定义 `load` 转换参数 | 非标准接口不要强行走内置 `module` |
| 字典下拉为空 | `dict:xxx` 名称错误或字典 store 未加载到数据 | 检查字典类型和接口返回 | 字典类型从系统字典复制，不手敲 |
| tag 显示原始值 | `tagMap` 未配置或字典缺失 | 给列加 `type: 'tag', tagMap: 'dict:xxx'` | 状态列默认用 tag |
| tag 颜色不符合预期 | 字典没有 `tagType` 或 `tagTypeMap` | 显式传 `tagTypeMap` | 后端字典项维护颜色 |
| 新增弹窗打开旧数据 | `customFormData` 是被复用的响应式对象，或子弹窗状态没清 | 打开前重置默认值 | 默认值用工厂或在 `beforeCreate` 清理 |
| 编辑弹窗字段缺失 | `formColumns` 中缺少对应字段 | 补齐字段或使用后端 `form-config` | 表格字段和表单字段分开维护 |
| 提交后后端报字段类型错误 | switch/select/number 默认值类型不对 | 对齐 `activeValue/inactiveValue` 和 select value 类型 | 后端 DTO 是数字就别传字符串 |
| `beforeSubmit` 不阻止提交 | 只返回了空值或 Promise 未处理，组件只判断 `false` | 明确 `return false` | 钩子里不要写含糊返回值 |
| `beforeSubmit` 异步校验不可靠 | 当前实现直接调用并判断返回值，未 await | 异步校验放自定义弹窗提交逻辑 | `KhPage` 内置弹窗适合简单同步处理 |
| 操作成功不刷新 | 自定义按钮里忘记调 `pageRef.value?.reload()` | 成功后刷新 | 所有 actionButtons 成功分支最后刷新 |
| 详情弹窗数据不全 | 后端 detail 接口未返回完整数据，组件 fallback 到 row | 检查 `/api/{module}/{id}` | 详情页要约定 detail DTO |
| 从表不显示 | `detailLines.prop` 与详情数据数组字段不一致 | 改 prop 或后端返回对应数组 | 从表字段用复数，如 `lines` |
| 传了 action slot 后内置按钮不见 | `action` slot 会覆盖默认操作列内容 | 在 slot 中自己补内置动作，或用 `actionButtons` | 只追加按钮时优先用 `actionButtons` |
| 表头筛选不生效 | `show-header-filter` 透传给 KhTable，但列没有 `searchable` | 给列加 `searchable: true` | 需要表头筛选的列显式标记 |
| 批量操作拿不到选中行 | `showSelection=false` 或 ref 未绑定 | 开启 `show-selection` 并使用 `pageRef` | 批量按钮先判断空数组 |
| 多选状态在刷新后残留 | 刷新前未清空或 el-table 保留 selection | 调 `clearSelection()` | 批量操作成功后清空选择 |
| 导出按钮点击只是提示待实现 | 内置 `handleExport` 尚未实现业务导出 | 用自定义 `toolbarButtons` 写导出 | 复杂导出不要依赖内置 export |
| 页面加载时请求太多字典 | 多组 columns 重复引用字典 | 合并字典类型或按需加载 | 大页面减少重复列配置 |
| 页面内联函数过多影响可读性 | props 中直接写复杂箭头函数 | 抽成命名函数 | 状态机页面保持函数命名清楚 |
| PDA 页面用 KhPage 很难用 | `KhPage` 为 PC 列表页设计 | PDA 使用 `PdaLayout` 和大按钮表单 | 手持端不要套后台表格页 |
| 在 `KhPage` 上监听 `@selection-change` 没反应 | `tableAttrs` 过滤 `on*`，事件没有透传给内部 `KhTable` | 通过 `pageRef.value?.getSelectionRows()` 获取选择，或改组件透传事件 | 需要大量表格事件时直接使用 `KhTable` |
| 传了 `label-width` 但搜索表单没变化 | `KhPage` 不会把该属性传给搜索 `KhForm` | 在 `searchColumns` 字段上配置 `labelWidth`，或自组搜索表单 | 搜索区只依赖 `searchColumns` 做细调 |
| 传了 `height/max-height` 后表格高度不按预期 | `KhPage` 自动计算并传 `height`，和外部配置冲突 | 优先修父级布局，不强传高度 | `KhPage` 页面统一 flex 高度 |
| 传 `data/total/page-num` 后和接口加载混乱 | 同时用了外部数据模式和 `module/load` 内部模式 | 二选一；标准页用 `module/load` | 需要外部模式时直接用 `KhTable` |
| 搜索表单输入没进入 `searchModel` | 当前源码传 `:model`，而 `KhForm` 声明的是 `modelValue` | 修 `KhPage` 为 `v-model`/`:model-value`，或检查实际页面行为 | 文档和开发中明确该实现细节 |

### 17.2 KhTable

`KhTable` 是通用表格底座。它有两种模式：传 `load` 由组件内部分页加载，或传 `data/total/pageNum/pageSize` 由父组件外部控制。

#### 17.2.1 核心接口补充

| 类别 | 名称 | 说明 |
| --- | --- | --- |
| props | `data` | 外部数据 |
| props | `columns` | 列配置 |
| props | `load` | 内部异步加载函数 |
| props | `extraParams` | 加载时合并的额外查询参数 |
| props | `rowKey` | 行唯一键，默认 `id` |
| props | `showSelection` / `showIndex` | 多选列、序号列 |
| props | `showPagination` | 分页 |
| props | `showToolbar` / `toolbarButtons` | 工具栏 |
| props | `actionButtons` / `actionColumns` | 操作列 |
| props | `showHeaderFilter` | 表头筛选 |
| emits | `selection-change` | 多选变化 |
| emits | `sort-change` | 排序变化 |
| emits | `row-click` / `row-dblclick` | 行点击 |
| emits | `cell-change` | 行内编辑值变化 |
| emits | `header-filter` | 表头筛选变化 |
| emits | `update:pageNum` / `update:pageSize` | 外部数据模式分页同步 |
| emits | `search` / `refresh` | 外部数据模式触发父级重新请求 |
| emits | `before-load` / `after-load` / `load-error` | 内部加载模式生命周期；`before-load` 只是通知，不能取消加载 |
| expose | `reload()` / `refresh()` | 重载和刷新 |
| expose | `getSelectionRows()` / `clearSelection()` | 选择行处理 |
| expose | `clearHeaderFilters()` | 清空表头筛选 |

#### 17.2.2 columns 字段清单

| 字段 | 说明 |
| --- | --- |
| `prop` / `label` | 字段名和列名 |
| `width` / `minWidth` / `fixed` | 列宽和固定 |
| `align` / `headerAlign` | 内容和表头对齐 |
| `sortable` | 是否排序 |
| `showOverflowTooltip` | 文本溢出提示 |
| `type` | `tag`、`image`、`switch`、`tag-select`、`link`、`slot`、`expand` |
| `tagMap` / `tagTypeMap` / `tagEffect` | 标签文本和颜色 |
| `options` / `filterOptions` | 下拉和筛选选项 |
| `searchable` / `filterType` | 表头筛选 |
| `formatter` | 文本格式化 |
| `cellStyle` | 单元格样式 |
| `bindProps` | 透传给 `el-table-column` |

表格级样式字段不写在 `columns` 中，而是写在 `KhTable` 或 `KhPage` 上：

| 字段 | 使用位置 | 说明 |
| --- | --- | --- |
| `row-style` | `<KhTable>` / `<KhPage>` | 行内样式函数，可返回样式对象或预设名称 |
| `row-class-name` | `<KhTable>` / `<KhPage>` | 行类名，适合复杂 CSS |
| `header-cell-style` | `<KhTable>` / `<KhPage>` | 表头单元格样式 |
| `show-header-filter` | `<KhTable>` / `<KhPage>` | 是否显示表头筛选行 |
| `row-key` | `<KhTable>` / `<KhPage>` | 树表、展开行、稳定选择必须配置 |
| `tree-props` | `<KhTable>` / `<KhPage>` | 树形数据 children / hasChildren 字段 |
| `default-expand-all` | `<KhTable>` / `<KhPage>` | 树表或展开行默认展开 |

#### 17.2.3 问题清单

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 表格不加载数据 | `load` 未传，`data` 也为空 | 选择内部或外部模式之一 | 标准分页优先传 `load` |
| `load` 返回后表格仍为空 | 返回结构不是 `{ data, total }` | 调整返回格式 | 封装统一 page adapter |
| 分页 total 不对 | 返回 `total` 字段错误或路径错误 | 检查接口返回 | 后端分页 DTO 统一 |
| 切换页码不请求 | 外部模式下未监听 `search` 或 `update:pageNum` | 父组件处理分页事件 | 外部模式必须自己维护分页 |
| 搜索参数没带上 | `extraParams` 未传或对象不是响应式 | 传响应式查询对象 | 使用 `KhPage` 时由组件自动处理 |
| 排序后后端不生效 | 后端不支持 `sortConditions` | 自定义 load 转换排序 | 明确后端排序协议 |
| 多选列点击行导致误选 | `row-click` 内默认会 toggle selection | 自定义行点击或关闭多选 | 操作密集表格谨慎开启行点击选择 |
| 操作列过窄 | `actionWidth` 默认不足 | 设置 `action-width` 或 `action-min-width` | 三个以上按钮考虑更多菜单 |
| 操作列按钮重复 | 同时传了 `actionColumns` 和 `actionButtons` | 保留一种 | 新代码优先 `actionButtons` |
| 自定义 action slot 后配置按钮失效 | slot 覆盖默认内容 | 用 `actionButtons` 追加即可 | 不需要完全自定义时别用 action slot |
| 表头筛选输入后没请求 | 外部模式没监听 `header-filter/search` | 父组件处理事件 | 内部模式用 `load` 简化 |
| 表头筛选 select 无选项 | `filterOptions` 缺失或字典未解析 | 配置数组或 `dict:xxx` | 字典筛选列显式写 `filterOptions` |
| number-range 报错 | 默认筛选值不是 `{min,max}` | 使用组件默认初始化，不要外部乱改 | 不直接修改 `headerFilters` 结构 |
| date-range 值格式后端不认 | `value-format` 或转换协议不一致 | 在 load 中转换 | 日期筛选后端格式统一 |
| switch 改了 UI 但后端没保存 | 只触发 `cell-change`，未调接口 | 监听事件并提交 | 行内编辑必须写保存逻辑 |
| `rowStyle` 写了不生效 | 写进了 `columns` 或函数未返回样式 | 改为表格级 `:row-style="fn"`，返回对象或预设名称 | 行样式统一写在 `KhTable/KhPage` 属性上 |
| `rowStyle` 生效后斑马纹消失 | 组件为避免背景覆盖自动关闭 stripe | 这是预期行为 | 需要斑马效果时在 `rowStyle` 中自行处理 |
| 只想某列变色却整行变色 | 用了 `rowStyle` | 改用该列的 `cellStyle` | 行样式和单元格样式分清 |
| `rowClassName` 样式没有覆盖到单元格 | Element 表格背景在 `td` 上 | CSS 写 `:deep(.class-name td)` | 复杂行样式统一加 `td` 选择器 |
| `cellStyle` 参数用错 | 当前值是第三个参数 `value` | 使用 `(row, col, value, rowIndex)` | 新列样式按固定签名写 |
| `tag-select` 或 `switch` 在 `KhPage` 中没法监听保存 | `KhPage` 不透传 `@cell-change` | 改用 `KhTable`，或用自定义 slot/actionButtons 调接口 | 行内编辑保存优先直接用 `KhTable` |
| expand 展开状态错乱 | 没有配置稳定 `row-key` | 设置 `row-key="id"` | 展开行、树表、多选都要配置 rowKey |
| tag-select 选项为空 | `options/tagMap` 未配置 | 补配置 | 可编辑枚举列先定义 options |
| 图片列预览失败 | URL 为空或跨域 | 给默认图或处理空值 | 图片字段后端返回完整 URL |
| formatter 报错导致整列异常 | formatter 未处理 null/undefined | 做空值保护 | formatter 永远写兜底 |
| 列设置不持久 | 当前只是组件内部状态 | 父组件自行持久化 | 需要持久列设置时扩展存储 |
| 树表格不展开 | `rowKey/treeProps` 不匹配 | 设置正确 `row-key` 和 `tree-props` | 树数据统一 children 字段 |
| 表格横向溢出 | 固定宽度列过多 | 使用 `minWidth`，减少 fixed | 宽表只固定关键列 |
| 空状态太高或太低 | 容器高度不稳定 | 设置表格高度或外层 flex | 列表页统一布局 |

### 17.3 KhForm

`KhForm` 是配置式表单，既可做搜索表单，也可做新增编辑表单。它会根据 `columns` 自动初始化数据、加载字典、生成校验规则。

#### 17.3.1 字段类型补充

| 类型 | 适用场景 | 关键字段 |
| --- | --- | --- |
| `input` | 文本、编码、名称 | `maxlength`、`prefix`、`suffix`、`inputType` |
| `textarea` | 备注、说明 | `rows`、`maxlength` |
| `number` | 数量、优先级、阈值 | `min`、`max`、`step`、`precision` |
| `select` | 字典枚举 | `options`、`multiple`、`filterable` |
| `remote-select` | 远程搜索 | `remoteMethod`、`loading` |
| `date` | 日期/时间/范围 | `dateType`、`format`、`valueFormat` |
| `switch` | 启用、是否 | `activeValue`、`inactiveValue` |
| `radio` / `checkbox` | 单选/多选 | `options`、`isButton` |
| `cascader` | 树形级联 | `options`、`cascaderProps` |
| `cascade-select` | 多级独立下拉 | `cascadeItems` |
| `color-picker` | 颜色配置 | `predefine` |
| `icon-picker` | 图标配置 | `placeholder` |
| `slot` | 自定义控件 | 以 `prop` 作为 slot 名 |
| `buttons` | 查询按钮区 | 自动渲染查询/重置 |

#### 17.3.2 问题清单

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| v-model 没同步 | 父组件传的不是响应式对象或监听被覆盖 | 使用 `reactive/ref` 并正常绑定 | 表单数据统一 `reactive({})` |
| 打开弹窗后显示上次数据 | 父组件没有重置 model | 弹窗 open 时重新赋值 | 新增和编辑分开初始化 |
| 必填校验不触发 | 字段缺少 `prop` 或 `required` | 补 prop 和 required | 每个表单项必须有唯一 prop |
| select 必填校验触发时机不准 | 默认 trigger 是 blur | 给 select 设置 `trigger: 'change'` 的 rules | 选择类字段用 change 校验 |
| switch 传值类型不对 | 未设置 active/inactive value | 显式设置 `activeValue/inactiveValue` | 和后端 DTO 类型保持一致 |
| number 为空传 null/undefined | 没有默认值或允许清空 | 提交前兜底 | 数字字段 defaultValue 明确 |
| date 后端不认 | `valueFormat` 和后端格式不同 | 设置 `valueFormat` | 日期统一 `YYYY-MM-DD` 或 `YYYY-MM-DD HH:mm:ss` |
| 字典 select 为空 | `options: 'dict:xxx'` 错误 | 查字典类型 | 字典项命名集中维护 |
| 远程搜索不显示 loading | `remoteMethod` 没返回 Promise | 改成 async 函数 | remoteMethod 统一 async |
| 级联下拉子级不刷新 | `parentProp` 配错或父值为空 | 检查 cascadeItems 顺序 | 父级 prop 必须在前 |
| 级联修改后旧子级残留 | 自定义逻辑绕过了内置清空 | 使用内置 `cascade-select` | 不直接改子级 options |
| 折叠搜索只搜一个字段 | quick search 设计就是单输入 | 展开完整搜索或自定义 | 多条件搜索默认展开 |
| `buttons` 不靠右 | 自定义 span 或 colCount 导致 | 给 buttons 设置合适 span | 搜索表单最后一个字段用 buttons |
| 自定义 slot 不显示 | `type` 不是 `slot` 或 slot 名不等于 prop | 改列配置和插槽名 | slot 字段名保持一致 |
| 表单项挤在一起 | `colCount` 太大或 label 太长 | 调整 `colCount`、`labelWidth` | 复杂表单用 2 列 |
| 表单初始化覆盖用户输入 | columns/modelValue 变化触发 init | 避免频繁重建 columns | columns 用 computed 时保持稳定 |
| 自定义 rules 被 required 覆盖 | 组件逻辑 rules 优先 | rules 中自己写 required | 复杂校验全部用 rules |
| disabled 不生效 | 单字段 bindProps 覆盖或自定义 slot | 检查字段配置 | 禁用逻辑统一写在 column |

### 17.4 KhDialog

`KhDialog` 是统一弹窗。它可以内置 `KhForm`，也可以只做容器。

#### 17.4.1 问题清单

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 点击遮罩不能关闭 | 默认 `closeOnClickModal=false` | 需要时显式开启 | 表单弹窗通常保持 false |
| ESC 关闭导致数据丢失 | `closeOnPressEscape=true` | 重要表单改 false | 关键业务弹窗禁用 ESC |
| 确认按钮可重复点击 | 未传 `confirmLoading` | 提交期间置 true | 所有异步提交都绑定 loading |
| confirm 没传表单数据 | 没使用 `formColumns`，而是自定义内容 | 从自定义表单 ref 取值 | 自定义内容自己负责校验 |
| 内置表单校验不通过但无提示 | 字段 rules/prop 配错 | 检查 `formColumns` | 表单项 prop 必须对应数据字段 |
| 关闭后旧数据残留 | 外部 model 或内部子组件状态未清 | 关闭时重置父组件数据 | 复杂弹窗抽子组件并监听 close |
| 高弹窗内容溢出 | 未设置 `height` 或内容没有滚动 | 传 `height` 或拆分内容 | 长表单用固定高度 |
| 嵌套弹窗层级异常 | 多个 append-to-body/teleport 叠加 | 检查 z-index 和 appendToBody | 尽量避免深层嵌套弹窗 |
| 打开动画后 ref 为空 | 内容还没渲染 | `nextTick` 后访问 | ref 操作放 open 后 nextTick |
| 自定义 footer 后默认按钮消失 | footer slot 覆盖默认 footer | 自己渲染取消/确认按钮 | 只改按钮文本用 props，不用 slot |

### 17.5 KhDetailDialog

用于只读详情。主要风险在数据字段、字典映射和从表配置。

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 字段显示 `-` | data 中没有该 prop | 检查 detail DTO | 详情字段从后端 DTO 对齐 |
| tag 颜色不对 | `tagTypeMap` 缺失 | 补 tagTypeMap | 字典维护 tagType |
| 从表为空 | `lineConfigs.prop` 字段不匹配 | 修改 prop | 从表字段统一叫 `lines` 或业务名 |
| 从表列太窄 | columns width 不合理 | 设置 minWidth | 从表列宽单独设计 |
| 详情弹窗太窄 | 默认宽度不够 | 传 `width/detailWidth` | 主从详情建议 1000px 以上 |

### 17.6 KhStatCard

指标卡片适合少量关键数值。

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 图标不显示 | icon 传了字符串但未注册，或组件对象被响应式代理 | 使用 Element Plus 图标组件并 `markRaw` | 指标图标统一 `markRaw(Icon)` |
| 数值格式不符合 | 默认只做简单显示 | 传 `formatter` | 金额、百分比、单位都用 formatter |
| 点击无响应 | `clickable=false` 或未监听 click | 开启 clickable 并监听 | 可点击卡片视觉和行为同时配置 |
| 卡片一行挤压 | statCards 太多 | 减少数量或分组 | 顶部指标控制在 4 个左右 |

### 17.7 KhUpload

上传组件适合附件和导入。风险主要在文件限制、接口返回和业务绑定。

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 文件无法选择 | `accept` 过窄 | 放宽或修正 accept | Excel/PDF/图片分别配置 |
| 上传前就报错 | 超过 `limit` 或 `maxSize` | 调整限制或压缩文件 | 附件规则写在页面说明 |
| 上传成功但业务没关联 | 只上传附件，未保存业务单据关联 | 成功后保存附件 id | 附件上传和业务保存流程分清 |
| 上传失败无详情 | 后端错误 message 不明确 | request 层和组件提示都检查 | 后端返回标准错误 |
| 重复上传同名文件 | 未限制重复 | 上传前检查 fileList | 业务需要时按 name/hash 去重 |
| 删除文件后后端未删 | 只删本地列表 | 调删除附件接口 | 区分“临时删除”和“持久删除” |

### 17.8 KhMessage / KhMsgBox / KhNotify

这三类是反馈函数，不是普通展示组件。

| 组件 | 适合 | 不适合 |
| --- | --- | --- |
| `KhMessageFn` | 保存成功、轻量错误、表单提醒 | 长文本说明 |
| `KhMsgBoxFn` | 删除、取消、提交等确认 | 普通提示 |
| `KhNotifyFn` | 系统通知、后台任务结果 | 高频连续反馈 |

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 确认框点取消报错 | Promise catch 未处理 | 追加 `.catch(() => {})` | 所有 confirm 都处理取消 |
| 消息刷屏 | 循环里连续调用 message | 合并提示 | 批量操作只提示汇总结果 |
| 错误提示重复 | request 层和页面都提示 | 约定谁负责提示 | API 错误默认 request 层处理 |
| confirm 文案不清楚 | 只写“确定吗” | 写明对象和后果 | 危险动作说明影响 |

### 17.9 KhLayout

PC 后台整体布局。普通页面一般不直接使用。

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 菜单不显示 | `menuList` 为空或权限过滤为空 | 检查权限 store | 登录后先加载菜单 |
| 路由跳转失败 | `menuRouter` 与 path/index 不一致 | router 模式传真实 path | 菜单项 path 对齐 router |
| 标签页不更新 | key 配置不稳定 | 检查 route path/name | 标签页 key 使用稳定路由 |
| 主内容滚动异常 | 页面自己设置了全屏 overflow | 检查布局层级 | 业务页只控制自身内容区 |
| 头部右侧工具错位 | header-right 内容过宽 | 控制工具宽度 | 顶部只放高频工具 |

### 17.10 KhMenu

菜单组件由 `KhLayout` 使用，支持折叠、搜索、路由模式。

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 当前菜单不高亮 | `activeIndex` 或 route.path 不匹配 | router 模式检查 path | 菜单 path 与路由 path 一致 |
| 搜索后父菜单空白 | children 被过滤，父项结构不完整 | 检查菜单数据 title/path | 菜单数据字段统一 |
| 图标不显示 | icon 字符串未注册为组件 | 传可渲染图标名或组件 | 菜单图标从 KhIconPicker 选 |
| 折叠时搜索框还占位 | collapse 未正确传入 | 检查 `collapse` | 折叠状态由 layout 控制 |

### 17.11 KhFullscreen

全屏切换按钮。

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 浏览器拒绝全屏 | 非用户手势触发 | 只在点击事件中调用 | 不在 mounted 自动全屏 |
| 状态图标不更新 | fullscreenchange 未触发或浏览器兼容 | 检查浏览器 API | 只作为增强功能 |
| iframe/权限环境不可用 | 浏览器策略限制 | 允许 fullscreen 权限 | 内嵌场景提前验证 |

### 17.12 KhNotification

顶部通知入口，适合 websocket 消息。

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 未读数不更新 | 直接修改 props 数组但父 store 未持久 | 在父 store 更新 read 状态 | 通知状态由 store 管 |
| 点击消息后状态丢失 | 只改本地对象，刷新后恢复 | 调后端已读接口 | 已读动作持久化 |
| 消息太多卡顿 | 全量渲染 messages | 限制条数或分页 | 顶部只显示最近消息 |
| 类型颜色不对 | msg.type 非 `info/success/warning/error` | 规范 type | websocket 消息格式统一 |

### 17.13 KhAlert

`KhAlert` 是统一提示条，封装 `el-alert`。

| 接口 | 说明 |
| --- | --- |
| props | `type`、`title`、`description`、`closable`、`center`、`showIcon`、`effect` |
| emits | `close` |
| attrs | 透传给 `el-alert` |

示例：

```vue
<KhAlert
  type="warning"
  title="操作提醒"
  description="修改库位类型会影响后续上架策略，请确认后再保存。"
  show-icon
/>
```

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 关闭后无法再显示 | 父组件没有控制 v-if/key | 用父状态控制重新渲染 | 可关闭提示由父级管理显示状态 |
| 图标不显示 | `showIcon=false` | 设置 `show-icon` | 警告/错误提示默认带图标 |
| 文案换行难看 | description 过长 | 拆短句或用 slot/普通区域 | Alert 只放摘要 |

### 17.14 KhCollapse

折叠面板组件，封装 `el-collapse`。

| 接口 | 说明 |
| --- | --- |
| props | `modelValue`、`accordion`（当前声明了但未绑定到内部 `el-collapse`，实际不会生效） |
| emits | `update:modelValue`、`change` |
| expose | `activeNames` |
| slots | 默认 slot，放 `el-collapse-item` |

示例：

```vue
<KhCollapse v-model="activeNames">
  <el-collapse-item title="基础信息" name="base">...</el-collapse-item>
  <el-collapse-item title="扩展配置" name="ext">...</el-collapse-item>
</KhCollapse>
```

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| accordion 不生效 | 组件当前未显式把 `accordion` 绑定到 `el-collapse`，且该 prop 已被组件声明，不会进入 `$attrs` | 修组件模板为 `:accordion="accordion"`，或直接用原生 `el-collapse` | 使用前验证折叠行为 |
| 展开状态类型错误 | accordion 用 string，普通模式用 array | 按模式传正确类型 | v-model 初始值和模式匹配 |
| 内容区域样式被覆盖 | 内部使用深度样式 | 页面局部覆盖需更高选择器 | 折叠内容少做复杂布局 |

### 17.15 KhColorPicker

颜色选择器，常被 `KhForm` 的 `color-picker` 类型间接使用。

| 接口 | 说明 |
| --- | --- |
| props | `modelValue`、`placeholder`、`disabled`、`showAlpha`、`predefine` |
| emits | `update:modelValue`、`change` |
| 行为 | 面板 Teleport 到 body，点击外部或滚动关闭 |

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 输入颜色不生效 | 只接受 `#RGB` 或 `#RRGGBB` | 输入合法 hex | 颜色字段给预设优先 |
| 透明度不生效 | `showAlpha` 当前没有实际 alpha UI | 不依赖 alpha | 需要透明色时扩展组件 |
| 滚动后面板关闭 | 组件设计为滚动关闭防止错位 | 重新打开选择 | 放在稳定容器内 |
| 面板被遮挡 | z-index 或 body 弹层竞争 | 检查弹层层级 | 弹窗内使用时重点验证 |

### 17.16 KhIconPicker

图标选择器，常被 `KhForm` 的 `icon-picker` 类型间接使用。

| 接口 | 说明 |
| --- | --- |
| props | `modelValue`、`placeholder`、`disabled` |
| emits | `update:modelValue`、`change` |
| 行为 | 内置 Element Plus 图标名列表，支持搜索 |

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 选中图标不渲染 | 图标名不存在或 Element Plus 未注册 | 换内置列表中的图标名 | 图标只从选择器选 |
| 搜索不到图标 | 关键字和英文图标名不匹配 | 用英文关键词 | 图标命名给用户培训 |
| 弹窗里被遮挡 | Teleport 层级竞争 | 检查 z-index | 弹窗内测试图标选择 |

### 17.17 KhDragList

拖拽排序列表，依赖 `vue-draggable-plus`。

| 接口 | 说明 |
| --- | --- |
| props | `modelValue`、`rowKey`、`animation`、`ghostClass`、`chosenClass`、`disabled`、`showRemove`、`emptyText`、`emptyImageSize`、`labelKey` |
| emits | `update:modelValue`、`change`、`remove`、`drag-start`、`drag-end` |
| slots | `item`、`actions` |
| expose | `innerList` |

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 拖拽后顺序没保存 | 只更新前端列表，未调保存接口 | drag-end 后提交排序 | 排序场景都设计保存按钮 |
| 拖拽错乱 | `rowKey` 不唯一 | 使用唯一 id | 没 id 时谨慎用 `index` |
| 删除后父组件没更新 | 未使用 v-model | 绑定 `v-model` | 列表组件统一双向绑定 |
| 默认文本显示 JSON | 没有 label/name/title/filterName | 传 `labelKey` 或 item slot | 业务对象指定 labelKey |
| 禁用后还能点自定义按钮 | disabled 只影响拖拽和默认样式 | 自定义 actions 也判断 disabled | slot 内透传 disabled 或父级控制 |

### 17.18 KhSortList

按钮式上下移动排序，不依赖拖拽。

| 接口 | 说明 |
| --- | --- |
| props | `modelValue`、`rowKey`、`disabled`、`showRemove`、`showHint`、`hint`、`emptyText`、`emptyImageSize`、`labelKey` |
| emits | `update:modelValue`、`change`、`remove`、`sort` |
| slots | `item`、`actions` |
| expose | `list`、`moveUp(index)`、`moveDown(index)`、`remove(index)` |

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 上下移动无效 | index 越界或 disabled=true | 检查按钮状态 | 禁用态给明显提示 |
| 排序后后端未变 | 没调用保存 | sort/change 后保存 | 和 KhDragList 一样需要持久化 |
| key 重复警告 | rowKey 字段重复 | 使用唯一 rowKey | 数据源保证唯一 |
| 文案还是默认提示 | 未传 hint | 传业务提示 | 复杂排序给明确说明 |

### 17.19 KhSideDrawer

侧边抽屉，适合详情、辅助筛选、非阻塞配置。

| 接口 | 说明 |
| --- | --- |
| props | `modelValue`、`title`、`width`、`placement`、`showClose` |
| emits | `update:modelValue`、`open`、`close` |
| slots | `header`、默认内容、`footer` |

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 抽屉方向不对 | `placement` 不是 left/right/top/bottom | 传合法值 | 右侧详情默认 `right` |
| 没有标题区域 | `title` 为空且无 header slot | 传 title 或 header slot | 详情抽屉都要标题 |
| 关闭后父状态没变 | 未监听 v-model | 使用 `v-model` | 不直接用 `:model-value` |
| footer 不显示 | 没传 footer slot | 添加 footer slot | 需要操作按钮时放 footer |

### 17.20 KhSteps

流程步骤组件，封装 `el-steps`。

| 接口 | 说明 |
| --- | --- |
| props | `active`、`steps`、`direction`、`processStatus`、`finishStatus`、`simple` |
| slots | `icon-{index}` |

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 当前步骤不对 | active 从 0 开始 | 转换状态到索引 | 状态机写映射表 |
| 步骤状态颜色不对 | step.status 覆盖全局状态 | 检查每个 step.status | 只有异常步骤单独传 status |
| 移动端拥挤 | horizontal 步骤太多 | 改 vertical/simple | 超过 4 步用纵向 |

### 17.21 KhTimeline

时间线展示组件，适合日志、审批记录、状态流转。

| 接口 | 说明 |
| --- | --- |
| props | `items` |
| item 字段 | `time`、`type`、`hollow`、`icon`、`title`、`content` |
| slots | `item-{index}` |

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 时间不显示 | item 字段叫 createdTime 而不是 time | 转换为 time | 传给组件前做 adapter |
| 内容太长撑开 | content 未截断 | 自定义 item slot | 时间线只放摘要 |
| 类型颜色不对 | type 非 Element Plus 支持值 | 使用 `primary/success/warning/danger/info` | 状态映射统一 |

### 17.22 KhTransfer

穿梭选择器，封装 `el-transfer`。

| 接口 | 说明 |
| --- | --- |
| props | `modelValue`、`data`、`filterPlaceholder` |
| emits | `update:modelValue`、`change` |
| expose | `innerValue` |
| attrs | 透传给 `el-transfer` |

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 右侧不显示已选 | `modelValue` 中 key 与 data key 不匹配 | 对齐 key | data 使用 `{ key, label }` |
| 搜索不到 | label 字段不对或 filter-method 未传 | 按 Element Transfer 配置 | 权限/角色数据统一 label |
| 面板太窄 | 默认 280px | 外部样式覆盖 | 大数据选择用弹窗宽屏 |
| change 参数不够 | 当前 emit 只传 val | 需要更多信息时监听 el-transfer attrs 或扩展 | 复杂分配页面可自定义 |

### 17.23 KhNoticeBar

公告条/提醒条。

| 接口 | 说明 |
| --- | --- |
| props | `text`、`type`、`closable`、`scrollable`、`speed` |
| emits | `close` |
| slots | 默认内容 |

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 关闭后无法恢复 | 内部 visible=false | 通过 key 重新挂载 | 重要公告由父组件控制 |
| 滚动不动 | 文本宽度未超过容器 | 正常现象 | 只有长公告开启 scrollable |
| 滚动太快/慢 | speed 不合适 | 调 speed | 按 px/s 调整 |
| type 不生效 | 非 `info/success/warning/error` | 使用合法值 | 公告类型枚举化 |

### 17.24 KhLoading

局部 loading 容器。

| 接口 | 说明 |
| --- | --- |
| props | `loading`、`text`、`fullscreen`、`background` |
| slots | 默认内容 |

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| loading 遮罩不覆盖内容 | 内容容器没有尺寸 | 给容器高度 | 局部 loading 外层有 min-height |
| 全屏遮罩影响操作 | `fullscreen=true` | 改局部 loading | 除全局初始化外少用 fullscreen |
| 请求失败后一直 loading | finally 未关闭 | try/finally 控制 | 所有 async loading 写 finally |

### 17.25 KhWaterfall

瀑布流容器。

| 接口 | 说明 |
| --- | --- |
| props | `items`、`columns`、`gap`、`loading`、`finished` |
| emits | `load-more`（当前只声明，组件内部没有主动触发） |
| slots | 默认 slot，参数 `item/index` |

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 没触发 load-more | 当前组件只声明事件，没有内置滚动监听 | 父组件监听滚动后触发 | 需要无限滚动时补容器监听 |
| 卡片断裂 | CSS column 布局天然按列排布 | 接受瀑布流特性或改 grid | 数据卡片高度差不大时用 grid |
| 移动端列数太多 | columns 固定 | 根据屏幕计算 columns | 移动端传 1 或 2 |

### 17.26 KhPageHeader

页面头部标题组件。

| 接口 | 说明 |
| --- | --- |
| props | `title`、`subtitle`、`breadcrumb`、`showBack` |
| emits | `back` |
| slots | `extra`、`content` |

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 返回按钮没效果 | 只 emit back，不自动路由返回 | 父组件监听 `@back` | 页面自己决定返回逻辑 |
| 面包屑不跳转 | crumb.to 不符合 vue-router | 检查 to | breadcrumb 传 `{ title, to }` |
| 标题区挤压 | content slot 太宽 | 控制右侧宽度 | 右侧只放关键操作 |

### 17.27 KhEditableTable

可编辑表格适合弹窗明细行录入。

| 接口 | 说明 |
| --- | --- |
| props | `modelValue`、`columns`、`rowKey`、`border`、`stripe`、`size`、`maxHeight`、`showIndex`、`showAction`、`hideDelete`、`showAdd`、`defaultRow`、`inputSize` |
| emits | `update:modelValue`、`cell-change`、`add`、`delete` |
| slots | 列 slot、`action` |

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 新增行字段为空导致报错 | `defaultRow` 未提供完整结构 | 提供 defaultRow | 每列 prop 都有默认值 |
| 行内 select 无选项 | options 未传或字典未加载 | 使用数组或 `dict:xxx` | 明细字典字段统一配置 |
| 编辑后父组件没感知 | 只改 row 内字段，emit cell-change | 在 cell-change 中处理或提交前统一读取 | 明细提交前统一校验 |
| 删除误删 | 默认没有确认 | 自定义 action slot 加确认 | 关键明细删除加确认 |
| 数字精度错误 | precision 未配置 | 设置 precision | 数量/金额明确精度 |
| 控件太挤 | 列太多且 action 固定 | 调 maxHeight/width，拆分明细 | 超宽明细考虑弹窗全屏 |

### 17.28 KhDashboard

大屏/看板组件，内置 ECharts 初始化和暗色主题。

| 接口 | 说明 |
| --- | --- |
| props | `title`、`stats`、`charts`、`statSpan` |
| expose | `refresh()`、`resize()`、`getChartInstances()` |
| chart 字段 | `type`、`title`、`span`、`option`、`data` |

问题清单：

| 现象 | 可能原因 | 处理办法 | 预防建议 |
| --- | --- | --- | --- |
| 图表空白 | DOM 未有尺寸或 option 空 | 检查容器尺寸和 option | 看板挂载后 nextTick refresh |
| 数据变了图表不更新 | 组件没有深度 watch charts | 调 `dashboardRef.refresh()` | 数据加载完成后手动 refresh |
| 切换标签后图表变形 | 容器尺寸变化未 resize | 调 `resize()` | tab/窗口变化后 resize |
| rank 图倒序不符合预期 | 内部按 value 升序再反向视觉显示 | 自定义 option | 重要排行用自定义 option |
| 移动端拥挤 | statSpan/chart span 固定 | 响应式调整 span | 移动端单列看板 |
| 内存泄漏 | 页面销毁前图表未释放 | 组件已 dispose，确认不重复挂载 | 不在外部重复 init 同一 DOM |

### 17.29 KhFullscreen / KhNotification / KhLoading 的组合问题

这三个常出现在顶部工具或全局状态中。

| 组合问题 | 处理建议 |
| --- | --- |
| 全屏后通知 popover 位置异常 | 关闭并重新打开 popover，或全屏后触发 resize |
| 全局 loading 遮住通知 | 全局 loading 只用于初始化，业务请求用局部 loading |
| websocket 通知过多影响布局 | 通知入口只显示未读数和最近列表，历史消息进独立页面 |

### 17.30 组件选择反例清单

| 反例 | 问题 | 推荐做法 |
| --- | --- | --- |
| 用 `KhPage` 做 PDA 扫码页 | 表格和弹窗不适合手持端 | 用 `PdaLayout` + 大按钮 + 简单表单 |
| 用 `KhDialog` 做复杂多步骤流程 | 状态和校验会堆在一个弹窗里 | 拆业务子组件或独立页面 |
| 用 `KhTable` 行内按钮承载十几个动作 | 操作列拥挤且权限难维护 | 高频动作行内，低频动作放更多菜单或详情页 |
| 用 `KhForm` 动态生成所有复杂 UI | slot 和联动过多会难读 | 复杂区域拆自定义组件 |
| 所有组件都提升到 `src/components` | 公共层污染 | 只把跨业务域稳定复用的组件提升 |
| 在公共组件里直接调业务 API | 复用性下降 | 业务 API 留在页面或业务组件 |

### 17.31 全局排错索引

| 问题类别 | 优先检查 |
| --- | --- |
| 页面无数据 | 接口返回、`load` 返回结构、`module`、分页参数 |
| 按钮不显示 | `crudOperations`、`permissionPrefix`、按钮 `permission`、`show(row)` |
| 表单提交失败 | `formColumns`、字段类型、默认值、rules、`beforeSubmit` |
| 字典不显示 | `dict:xxx`、字典 store、后端字典项、tagMap/tagTypeMap |
| 弹窗状态混乱 | v-model、formModel、close/reset、destroyOnClose |
| 表格高度异常 | 父级高度、flex、`min-height: 0`、`KhPage` 外层 |
| 多选异常 | `showSelection`、rowKey、刷新后清空选择 |
| 图表空白 | 容器尺寸、ECharts option、refresh/resize |
| 拖拽排序丢失 | rowKey、v-model、保存接口 |
| 上传异常 | accept、limit、maxSize、接口返回、业务关联 |



## 继续阅读

- [前端开发指引 V3.0](/backend/KH.WMS前端开发指引%20V3.0)
- [前端架构设计思路](/backend/架构设计/KH.WMS前端架构设计思路)
- [前后端联调与接口契约](/backend/KH.WMS前后端联调与接口契约指引)
