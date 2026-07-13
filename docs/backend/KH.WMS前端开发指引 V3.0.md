# KH.WMS PC 前端开发指引 V3.0

> 适用范围：`KH.WMS.Client` 的 PC 端业务页面。PDA 页面走 `PdaLayout.vue`，本文只在必要处说明差异。
> 对照代码日期：2026-07-08。
> 推荐读者：第一次写 KH.WMS 前端页面的开发者、负责前后端联调的人、维护 PC 页面的人。

这份文档只回答一个问题：**拿到一个后端业务接口后，前端怎样稳定写出一个能查、能增删改查、能联调、能维护的 PC 页面。**

最重要的结论先放前面：

- 标准列表页优先用 `KhPage`，不要从零手写查询区、表格、分页、弹窗。
- 标准 CRUD 接口优先用 `useCrudApi(module)`，它会自动适配后端分页结构。
- 页面文件必须放在 `src/views/<业务域>/xxx.vue`，不要放进 `components/`，动态路由会排除 `views/**/components`。
- 菜单 `component` 必须等于 `src/views/` 下的相对路径，不带 `.vue`。
- `module` 必须等于后端 `[Route("api/xxx")]` 去掉 `api/` 后的值。
- `permissionPrefix` 必须和后端按钮权限码前缀一致。
- 标准单表用 `KhPage` 内置新增/编辑；主从表、业务动作、复杂弹窗再手写 `KhDialog`。
- `KhPage` 可以透传很多 `KhTable` 属性，但不会自动透传 `KhTable` 事件；内联编辑、行点击这类交互要按本文的真实写法处理。
- 自定义成功操作后要 `KhMessageFn.success(...)`，然后 `pageRef.value?.reload()`。

## 目录

- [1 一张图看懂前端结构](#1-一张图看懂前端结构)
- [2 开发前先确认 7 件事](#2-开发前先确认-7-件事)
- [3 新增标准 CRUD 页面](#3-新增标准-crud-页面)
- [4 KhPage 怎么工作](#4-khpage-怎么工作)
- [5 查询、表格、表单配置怎么写](#5-查询表格表单配置怎么写)
- [6 API、请求和返回值判断](#6-api请求和返回值判断)
- [7 路由、菜单和权限](#7-路由菜单和权限)
- [8 字典和扩展字段](#8-字典和扩展字段)
- [9 主从表页面怎么写](#9-主从表页面怎么写)
- [10 手动弹窗和子弹窗](#10-手动弹窗和子弹窗)
- [11 import 规则](#11-import-规则)
- [12 常见场景速查](#12-常见场景速查)
- [13 提交前检查清单](#13-提交前检查清单)
- [14 常见坑](#14-常见坑)
- [附录 A 常用文件索引](#附录-a-常用文件索引)
- [附录 B 常用命令](#附录-b-常用命令)

## 1 一张图看懂前端结构

`KH.WMS.Client` 是一个 Vue 3 + Vite + Element Plus + Pinia 项目。

```text
KH.WMS.Client/
  public/
    config.js                 运行时 API 地址配置
  src/
    api/                      业务 API 函数
    components/               全局复用 Kh 组件
    layouts/
      PcLayout.vue            PC 主布局
      PdaLayout.vue           PDA 布局
    router/
      index.js                登录守卫、动态路由注册
      menuConfig.js           旧菜单配置或参考
    stores/
      user.js                 登录用户、token
      permission.js           菜单、路由、按钮权限
      dict.js                 字典缓存
    utils/
      request.js              axios 封装、token、错误提示
      crud.js                 标准 CRUD API 工厂和分页转换
      useExtFields.js         扩展字段加载、回显、提交处理
    views/
      basedata/
      config/
      inbound/
      outbound/
      system/
      warehouse/
```

页面开发时只需要记住这条调用链：

```text
后端菜单 component
  -> src/router/index.js 动态加载 src/views/<component>.vue
  -> 页面使用 KhPage
  -> KhPage 使用 KhForm + KhTable + KhDialog
  -> KhPage 根据 module 创建 useCrudApi(module)
  -> useCrudApi 通过 request.js 请求后端 /api/<module>/...
```

## 2 开发前先确认 7 件事

写页面前先和后端或菜单配置确认下面 7 件事。确认完，页面基本不会走偏。

| 顺序 | 要确认什么 | 示例 | 前端用在哪里 |
| --- | --- | --- | --- |
| 1 | 后端路由 | `[Route("api/material")]` | `module="material"`、`useCrudApi('material')` |
| 2 | 是否标准 CRUD | `/pagelist`、`/create`、`/update`、`/delete/{id}` | 决定能否用 `KhPage` 内置 CRUD |
| 3 | 菜单 path | `/basedata/material` | 菜单点击地址 |
| 4 | 菜单 component | `basedata/material` | 必须存在 `src/views/basedata/material.vue` |
| 5 | 页面权限前缀 | `bd:material` | `permissionPrefix="'bd:material'"` |
| 6 | 是否有扩展字段 | `/api/material/form-config` | 决定是否用 `useExtFields` |
| 7 | 是否主从表 | 请求体 `{ order, lines }` | 决定是否手写 `KhDialog + KhEditableTable` |

一个完整对应关系如下：

```text
后端 Controller: [Route("api/material")]
前端 module:     material
CRUD 工厂:       useCrudApi('material')
菜单 path:       /basedata/material
菜单 component:  basedata/material
页面文件:        src/views/basedata/material.vue
权限前缀:        bd:material
```

## 3 新增标准 CRUD 页面

本章是新人最常用路径。只要后端是标准 CRUD，就照这个步骤写。

### 3.1 第一步：创建页面文件

如果后端菜单 `component = basedata/material`，前端文件就建在：

```text
src/views/basedata/material.vue
```

不要建在：

```text
src/views/basedata/components/material.vue
```

原因：`src/router/index.js` 用 `import.meta.glob('../views/**/*.vue')` 预加载页面，但会过滤掉 `components` 子目录。放错位置时，菜单能看到，路由却加载不到真实页面。

### 3.2 第二步：放入最小 KhPage 骨架

```vue
<template>
  <!-- 页面外层必须占满高度，否则 KhPage 内部表格高度可能计算不准 -->
  <div style="height: 100%; display: flex; flex-direction: column;">
    <!--
      最小 KhPage 需要 7 类信息：
      1. ref：后续刷新列表用
      2. module：后端 api 路由名
      3. title：页面和弹窗标题
      4. search：查询区配置和值
      5. columns：表格列
      6. formColumns：新增/编辑表单
      7. permissionPrefix：按钮权限前缀
    -->
    <KhPage
      ref="pageRef"
      module="material"
      title="物料管理"
      :search-columns="searchColumns"
      :search-model="searchModel"
      :columns="tableColumns"
      :form-columns="formColumns"
      :custom-form-data="formDialogData"
      :crud-operations="crudOperations"
      :permission-prefix="'bd:material'"
    />
  </div>
</template>

<script setup>
// KhPage 组件实例。自定义按钮、子弹窗成功后通常调用 pageRef.value?.reload()
const pageRef = ref(null)
</script>
```

这里每个属性都有明确职责：

| 属性 | 作用 |
| --- | --- |
| `ref="pageRef"` | 自定义操作成功后调用 `pageRef.value?.reload()` |
| `module` | 生成 `/api/<module>/pagelist/create/update/delete/...` |
| `title` | 页面标题、内置弹窗标题、导出文件名 |
| `searchColumns` | 查询区显示哪些条件 |
| `searchModel` | 查询区绑定的数据，也是请求参数来源 |
| `columns` | 表格列 |
| `formColumns` | 新增/编辑弹窗表单字段 |
| `customFormData` | 新增时的默认表单值 |
| `crudOperations` | 控制新增、编辑、删除、查看、导出开关 |
| `permissionPrefix` | 内置按钮和自定义按钮的权限前缀 |

### 3.3 第三步：补齐一个可用页面

下面是标准单表 CRUD 模板。复制后通常只改 `module`、权限前缀、字段名和字典名。

```vue
<template>
  <!-- 标准业务页推荐用满高 flex 容器包住 KhPage -->
  <div style="height: 100%; display: flex; flex-direction: column;">
    <!--
      这个 KhPage 使用内置 CRUD：
      module="material" 会自动连接 /api/material/pagelist、create、update、delete 等接口。
      如果页面是主从表或业务动作很复杂，就不要打开内置 create/update。
    -->
    <KhPage
      ref="pageRef"
      module="material"
      title="物料管理"
      :search-columns="searchColumns"
      :search-model="searchModel"
      :columns="tableColumns"
      :form-columns="formColumns"
      :custom-form-data="formDialogData"
      :crud-operations="crudOperations"
      :permission-prefix="'bd:material'"
      :show-stat-cards="false"
      :show-header-filter="true"
      :search-col-count="3"
    />
  </div>
</template>

<script setup>
// 页面级 KhPage 引用，供自定义操作刷新列表
const pageRef = ref(null)

// 查询表单绑定对象。key 必须和 searchColumns.prop、后端过滤字段一致
const searchModel = reactive({
  materialCode: '',
  materialName: '',
  status: '',
})

// 查询区配置。KhPage 会把它交给 KhForm 渲染
const searchColumns = [
  // input 默认会用 contains 查询
  { prop: 'materialCode', label: '物料编码', type: 'input', clearable: true },
  { prop: 'materialName', label: '物料名称', type: 'input', clearable: true },
  // select 默认会用 equals 查询；dict:status_flag 会自动加载字典
  { prop: 'status', label: '状态', type: 'select', clearable: true, options: 'dict:status_flag' },
]

// 表格列配置。prop 对应后端列表接口返回字段
const tableColumns = [
  // 编码、状态、日期这类字段通常给固定宽度
  { prop: 'materialCode', label: '物料编码', width: 130 },
  // 名称、备注这类内容长短不固定，通常用 minWidth
  { prop: 'materialName', label: '物料名称', minWidth: 150 },
  // type=tag 表示按标签渲染；tagMap 指向字典
  { prop: 'status', label: '状态', width: 90, type: 'tag', tagMap: 'dict:status_flag' },
  // 长文本建议打开 tooltip，避免撑宽表格
  { prop: 'remark', label: '备注', minWidth: 160, showOverflowTooltip: true },
]

// 新增/编辑弹窗的表单字段配置。prop 对应提交给后端的字段名
const formColumns = [
  // required=true 会让 KhForm 自动生成必填校验
  { prop: 'materialCode', label: '物料编码', type: 'input', required: true, maxlength: 20 },
  { prop: 'materialName', label: '物料名称', type: 'input', required: true, maxlength: 100 },
  // switch 要确认后端需要 1/0 还是 true/false，当前项目常用 1/0
  { prop: 'status', label: '状态', type: 'switch', activeValue: 1, inactiveValue: 0 },
  { prop: 'remark', label: '备注', type: 'textarea', rows: 3, maxlength: 200 },
]

// 新增弹窗打开时的默认值。编辑时 KhPage 会用当前行数据覆盖
const formDialogData = reactive({
  materialCode: '',
  materialName: '',
  status: 1,
  remark: '',
})

// KhPage 内置操作开关。false 表示不生成该内置按钮
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: true,
  export: true,
}
</script>
```

### 3.4 标准页面的开发顺序

实际开发时建议按这个顺序填：

1. 建 `src/views/<业务域>/<页面>.vue`。
2. 写 `KhPage` 骨架，先让菜单能打开页面。
3. 根据后端分页查询 DTO 写 `searchModel` 和 `searchColumns`。
4. 根据后端列表返回 DTO 写 `tableColumns`。
5. 根据后端新增/编辑 DTO 写 `formColumns`。
6. 配 `crudOperations`。
7. 配 `permissionPrefix`。
8. 如果有状态切换、收货、组盘等业务动作，再加 `actionButtons`。
9. 如果有扩展字段，再接 `useExtFields`。
10. 最后跑页面，验证查询、新增、编辑、删除、导出、权限。

## 4 KhPage 怎么工作

`KhPage` 是 PC 业务页面的首选壳组件。它组合了：

- `KhForm`：查询区。
- `KhTable`：表格、分页、工具栏、操作列。
- `KhDialog`：内置新增/编辑弹窗。
- `KhDetailDialog`：内置查看详情弹窗。
- `useCrudApi`：标准 CRUD 请求。

### 4.1 内置 CRUD 端点

传入 `module="material"` 后，`KhPage` 会内部创建：

```js
// KhPage 根据 module 自动创建标准 CRUD API
useCrudApi('material')
```

对应请求如下：

| 操作 | 方法 | 地址 |
| --- | --- | --- |
| 分页 | `POST` | `/api/material/pagelist` |
| 详情 | `GET` | `/api/material/{id}` |
| 新增 | `POST` | `/api/material/create` |
| 编辑 | `POST` | `/api/material/update` |
| 删除 | `DELETE` | `/api/material/delete/{id}` |
| 状态 | `PUT` | `/api/material/status/{id}` |
| 表单配置 | `GET` | `/api/material/form-config` |
| 导出 | `POST` | `/api/material/export` |

### 4.2 查询参数会被转换

`KhTable` 传给加载函数的是扁平参数：

```js
{
  // KhTable 内部分页参数，pageNum 从 1 开始
  pageNum: 1,
  pageSize: 30,
  // 查询区里的业务字段，会和 searchColumns 一起转换成 filters
  materialCode: 'A',
  status: 1,
  // 表格点击排序后产生的排序条件
  sortConditions: []
}
```

`useCrudApi.pageList` 会通过 `buildPageQuery` 转成后端分页结构：

```js
{
  // 后端分页页码
  pageIndex: 1,
  // 后端每页条数
  pageSize: 30,
  // 排序条件，字段名来自表格列 prop
  sortConditions: [],
  // 过滤条件数组，后端按 field/operator/value 执行查询
  filters: [
    // input 类型默认 contains，适合模糊查询
    { field: 'materialCode', operator: 'contains', value: 'A' },
    // select 类型默认 equals，适合状态、类型这类枚举值
    { field: 'status', operator: 'equals', value: '1' }
  ]
}
```

默认 operator 规则：

| 查询列类型 | 默认 operator |
| --- | --- |
| `input` | `contains` |
| `select` | `equals` |
| `number` | `equals` |
| 数组值 | `in` |

需要指定时，在查询列上写 `filterOperator`。

表头筛选和查询区不是两套接口。当前代码里，`KhTable` 会把表头筛选整理成 `filters` 数组，`useCrudApi.pageList` 再把它和查询区过滤条件合并：

```js
// 表头筛选“状态=启用”时，KhTable 传给 load 的参数里会带 filters
const params = {
  // 查询区字段仍然是扁平字段
  materialCode: 'A',
  // 表头筛选已经提前整理成后端 filters 条目
  filters: [
    { field: 'status', operator: 'equals', value: 1 },
  ],
}

// buildPageQuery 最终会把查询区和表头筛选合到同一个 filters 数组
buildPageQuery(params)
```

所以后端只需要统一处理 `filters`，不用区分“查询区筛选”和“表头筛选”。

### 4.3 内置新增、编辑、查看的差异

`KhPage` 内置行为要分清：

- 新增：用 `customFormData` 初始化表单，提交 `create(data)`。
- 编辑：直接用当前表格行 `row` 打开表单，提交 `update(data)`。
- 查看：调用 `detail(row.id)`，用返回数据打开详情弹窗。

所以，如果编辑必须加载最新详情，或编辑需要主从表明细，就不要依赖内置编辑。关掉 `crudOperations.update`，自己写 `handleUpdate(row)`。

内置钩子也要按当前实现理解：

| 钩子 | 当前实现 | 适合做什么 |
| --- | --- | --- |
| `beforeCreate` | 同步执行，返回 `false` 才阻止打开新增弹窗 | 判断当前页面状态能不能新增 |
| `beforeUpdate(row)` | 同步执行，返回 `false` 才阻止打开编辑弹窗 | 判断当前行能不能编辑 |
| `beforeDelete(row)` | 同步执行，返回 `false` 才阻止删除 | 前端兜底限制，例如管理员账号不允许删 |
| `beforeSubmit(data, mode)` | 同步执行，返回 `false` 才阻止提交 | 整理字段、提取扩展字段、简单同步校验 |
| `afterSubmit(res, mode)` | 提交成功后同步调用 | 成功后的额外状态处理 |

不要在这些钩子里写“必须等待接口结果才能决定是否继续”的异步校验，因为 `KhPage` 当前没有 `await beforeSubmit()`。需要异步校验时，改用手写 `KhDialog` 提交流程。

### 4.4 常用暴露方法

页面通过 `ref` 调用：

```js
// 重新回到第一页并加载数据，适合新增、删除、查询条件改变后使用
pageRef.value?.reload()          // 回到第一页并重新加载

// 保持当前页码刷新，适合状态切换、行内编辑成功后使用
pageRef.value?.refresh()         // 当前页刷新

// 获取表格当前勾选行，适合批量操作
pageRef.value?.getSelectionRows()

// 清空表格勾选状态
pageRef.value?.clearSelection()

// 编程式打开 KhPage 内置新增弹窗
pageRef.value?.openCreateDialog()

// 编程式打开 KhPage 内置编辑弹窗
pageRef.value?.openUpdateDialog(row)

// 编程式打开 KhPage 内置详情弹窗
pageRef.value?.openDetailDialog(row)
```

自定义按钮成功后最常用：

```js
// 先提示用户，再刷新列表。顺序固定，用户体验最清楚
KhMessageFn.success('操作成功')
pageRef.value?.reload()
```

### 4.5 `KhPage` 和 `KhTable` 的差异边界

`KhPage` 内部包了一层 `KhTable`，但两者不是完全等价。对照 `src/components/KhPage/index.vue` 当前实现，要记住下面这张表：

| 能力 | 当前实现 | 页面写法 |
| --- | --- | --- |
| 普通表格属性 | `KhPage` 会把未声明的普通属性透传给 `KhTable` | 可以直接写 `:show-header-filter="true"`、`:action-width="'150'"`、`:page-size="50"`、`:row-style="fn"` |
| 自定义列插槽 | 除 `toolbar-left`、`toolbar-right`、`action` 等保留插槽外，其他插槽会继续传给 `KhTable` | `tableColumns` 写 `type: 'slot'`，再写 `<template #status="{ row }">...</template>` |
| `toolbar-left/right`、`action` 插槽 | `KhPage` 自己显式接收后再传给 `KhTable` | 可以正常写在 `KhPage` 里面 |
| `KhTable` 事件 | `KhPage` 当前没有继续 `emit` 出来，且 `tableAttrs` 会过滤 `onXxx` | 不要直接在 `KhPage` 上写 `@cell-change`、`@row-click`、`@header-filter` 来指望收到内部表格事件 |
| 直接控制表格 | `KhPage` 暴露了 `tableRef`，`pageRef.value.tableRef` 指向内部 `KhTable` | 适合调用 `refresh/reload/getSelectionRows`，不适合绑定模板事件 |

最容易踩坑的是内联编辑：

```vue
<!-- 当前 KhPage 实现下，这样写收不到 KhTable 的 cell-change -->
<KhPage @cell-change="handleCellChange" />
```

如果是状态启用/禁用，推荐用 `actionButtons`。如果必须在单元格里显示开关，用 `type: 'slot'` 自己渲染 `el-switch`，示例见 [5.3 表格列配置](#53-表格列配置)。

## 5 查询、表格、表单配置怎么写

### 5.1 `prop` 是整套配置的核心

`prop` 同时决定：

- 查询参数字段名。
- 表格读取 `row[prop]`。
- 表单读取和提交 `formData[prop]`。
- 表单校验字段名。
- 插槽名称。

因此 `prop` 必须和后端字段对齐，拼写不要随意改。

### 5.2 查询配置

```js
// searchModel 是查询区实际存值的对象。
// 每个 key 都要和 searchColumns.prop 一致，否则输入值不会传给后端。
const searchModel = reactive({
  orderNo: '',
  orderStatus: '',
})

// searchColumns 决定查询区渲染哪些控件。
// KhPage 会把它交给 KhForm，并在查询时一起传给 useCrudApi 推断 operator。
const searchColumns = [
  // 输入框适合单号、编码、名称，默认 contains 模糊匹配
  { prop: 'orderNo', label: '入库单号', type: 'input', clearable: true },
  // 下拉框适合状态、类型，options 写 dict:xxx 会自动加载字典
  { prop: 'orderStatus', label: '状态', type: 'select', clearable: true, options: 'dict:inbound_order_status' },
]
```

写法约定：

- `searchModel` 中要包含每个查询字段的初始值。
- 输入框用 `type: 'input'`。
- 字典下拉用 `type: 'select', options: 'dict:xxx'`。
- 查询字段必须是后端允许过滤的字段。

### 5.3 表格列配置

```js
// tableColumns 决定列表显示哪些字段以及如何渲染。
// prop 对应后端分页接口返回的 row 字段。
const tableColumns = [
  // 固定宽度列：适合单号、编码、日期、状态
  { prop: 'orderNo', label: '入库单号', width: 160 },
  // tag 类型：把枚举值渲染成标签，tagMap 使用字典映射显示文本和颜色
  { prop: 'orderStatus', label: '状态', width: 100, type: 'tag', tagMap: 'dict:inbound_order_status' },
  // 备注这类长文本用 minWidth，并打开 tooltip 防止内容撑破表格
  { prop: 'remark', label: '备注', minWidth: 160, showOverflowTooltip: true },
]
```

常用列类型：

| 类型 | 写法 |
| --- | --- |
| 普通文本 | `{ prop, label, width }` |
| 长文本 | `showOverflowTooltip: true` |
| 字典标签 | `type: 'tag', tagMap: 'dict:xxx'` |
| 自定义渲染 | `type: 'slot'`，并写同名插槽 |
| 数字 | `align: 'right'` |
| 可编辑开关 | `KhTable` 直用时可用 `type: 'switch'`；在 `KhPage` 里推荐用自定义插槽或行按钮 |
| 链接 | `type: 'link', onClick(row)` |

自定义列示例：

```vue
<KhPage :columns="tableColumns">
  <!-- 插槽名 orderStatus 必须等于 tableColumns 里对应列的 prop -->
  <template #orderStatus="{ row }">
    <!-- row 是当前行数据，这里按状态映射标签颜色和显示文本 -->
    <el-tag size="small" :type="statusTagMap[row.orderStatus]?.type || 'info'">
      {{ statusTagMap[row.orderStatus]?.label || row.orderStatus }}
    </el-tag>
  </template>
</KhPage>
```

```js
const tableColumns = [
  // type: 'slot' 表示这一列交给同名插槽渲染，不走默认文本/tag 渲染
  { prop: 'orderStatus', label: '状态', width: 100, type: 'slot' },
]
```

可编辑开关示例：

```vue
<!--
  KhPage 当前不会把 KhTable 的 cell-change 事件继续 emit 出来。
  所以在 KhPage 页面里，内联开关推荐用 slot 自己渲染并直接绑定 change。
-->
<KhPage
  ref="pageRef"
  :columns="tableColumns"
>
  <template #status="{ row }">
    <el-switch
      :model-value="row.status"
      :active-value="1"
      :inactive-value="0"
      @change="(value) => handleStatusChange(row, value)"
    />
  </template>
</KhPage>
```

```js
const tableColumns = [
  {
    // 在 KhPage 中使用 slot 渲染开关时，这里必须写 type:'slot'
    prop: 'status',
    label: '状态',
    width: 100,
    type: 'slot',
  },
]

// value 是开关切换后的目标状态，row 是当前行
const handleStatusChange = async (row, value) => {
  try {
    // 调用标准状态接口：PUT /api/<module>/status/{id}
    const res = await crudApi.setStatus(row.id, value)
    if (res.code === 200 || res.code === 0 || res.success === true) {
      KhMessageFn.success(res.message || '状态已更新')
      // 状态切换通常刷新当前页即可，不需要回到第一页
      pageRef.value?.refresh()
    }
  } catch {
    // 接口失败时刷新一次，把开关恢复成后端真实状态
    pageRef.value?.refresh()
  }
}
```

如果你直接使用的是 `KhTable` 而不是 `KhPage`，才可以用组件内置的 `type: 'switch' + @cell-change`：

```vue
<!-- KhTable 直用时才这样监听 cell-change -->
<KhTable
  :columns="tableColumns"
  :load="load"
  @cell-change="handleCellChange"
/>
```

```js
const tableColumns = [
  {
    // KhTable 内置 switch 不会自动请求后端，只负责显示和触发 cell-change
    prop: 'status',
    label: '状态',
    type: 'switch',
    activeValue: 1,
    inactiveValue: 0,
  },
]

// KhTable 的 cell-change 参数是 prop、row、value
const handleCellChange = async (prop, row, value) => {
  if (prop !== 'status') return
  await crudApi.setStatus(row.id, value)
}
```

链接列示例：

```js
const tableColumns = [
  {
    // 单号列显示 row.orderNo
    prop: 'orderNo',
    label: '入库单号',
    width: 160,
    // type=link 表示用 el-link 渲染，适合“点击单号看详情”
    type: 'link',
    // 点击链接时会拿到当前行 row
    onClick: (row) => {
      // 这里复用 KhPage 内置详情弹窗；也可以改成 handleDetail(row)
      pageRef.value?.openDetailDialog(row)
    },
  },
]
```

链接列适合单号、编码这类“点击查看详情”的字段。如果点击后要打开复杂业务弹窗，也可以在 `onClick(row)` 里调用自己的 `handleXxx(row)`。

注意：链接列的点击回调写在 `columns` 配置里，由 `KhTable` 内部直接调用，所以它不依赖 `KhPage` 事件透传，放在 `KhPage` 里可以正常使用。

### 5.4 表单配置

```js
// formColumns 决定新增/编辑弹窗里显示哪些字段。
// prop 是提交给后端的字段名，不能只按前端习惯随便起名。
const formColumns = [
  // required=true 会自动生成“请输入客户编码”的必填校验
  { prop: 'customerCode', label: '客户编码', type: 'input', required: true },
  { prop: 'customerName', label: '客户名称', type: 'input', required: true },
  // switch 的提交值由 activeValue/inactiveValue 决定
  { prop: 'status', label: '状态', type: 'switch', activeValue: 1, inactiveValue: 0 },
  // textarea 用于备注、说明这类长文本
  { prop: 'remark', label: '备注', type: 'textarea', rows: 3, maxlength: 200 },
]
```

常用字段类型：

| 类型 | 示例 |
| --- | --- |
| 输入框 | `{ prop: 'code', label: '编码', type: 'input' }` |
| 文本域 | `{ prop: 'remark', label: '备注', type: 'textarea', rows: 3 }` |
| 数字 | `{ prop: 'qty', label: '数量', type: 'number', min: 0, precision: 2 }` |
| 下拉 | `{ prop: 'status', label: '状态', type: 'select', options: 'dict:status_flag' }` |
| 日期 | `{ prop: 'orderDate', label: '日期', type: 'date', valueFormat: 'YYYY-MM-DD' }` |
| 开关 | `{ prop: 'status', label: '状态', type: 'switch', activeValue: 1, inactiveValue: 0 }` |
| 自定义 | `{ prop: 'xxx', label: '字段', type: 'slot' }` |

表单默认值建议用对象或函数，不要在多个弹窗之间复用脏数据：

```js
// KhPage 内置新增弹窗打开时，会把这个对象浅拷贝成初始表单值
const formDialogData = reactive({
  customerCode: '',
  customerName: '',
  // 新增时默认启用
  status: 1,
  remark: '',
})
```

手写弹窗时推荐函数式默认值：

```js
// 每次需要重置表单时都重新创建一份默认值，避免复用上一次弹窗的数据
const createFormData = () => ({
  id: null,
  code: '',
  name: '',
  status: 1,
  remark: '',
})

// reactive 创建后不要整体替换，只改里面的属性
const formData = reactive(createFormData())

const resetForm = () => {
  // Object.assign 会保留 reactive 引用，同时覆盖字段值
  Object.assign(formData, createFormData())
}
```

不要这样替换响应式对象：

```js
// 错误示例：这会替换 reactive 引用，导致表单响应式失效
formData = detail
```

正确写法：

```js
// 正确示例：只覆盖对象属性，保留 reactive 引用
Object.assign(formData, detail)
```

### 5.5 表单高级控件

`KhForm` 当前实现不只支持输入框和下拉框，还支持几个业务页面已经用到或适合复用的高级控件。

远程搜索下拉：

```js
const formColumns = [
  {
    // remote-select 适合物料、分类、客户这类数据量较大的字段
    prop: 'materialCategoryId',
    label: '物料分类',
    type: 'remote-select',
    clearable: true,
    // 用户输入关键字时由 KhForm 调用，结果要写回 item.options
    remoteMethod: async (query, item) => {
      const res = await searchCategory(query)
      item.options = (res.data || []).map(x => ({
        label: x.categoryName,
        value: x.id,
      }))
    },
  },
]
```

多级联动下拉：

```js
const formColumns = [
  {
    // cascade-select 是多个独立 el-select，不是 Element 的树形 cascader
    prop: '_locationCascade',
    label: '库位选择',
    type: 'cascade-select',
    span: 24,
    cascadeItems: [
      {
        // 第一级直接加载仓库字典
        prop: 'warehouseId',
        label: '仓库',
        options: 'dict:warehouse_list',
      },
      {
        // 第二级依赖 warehouseId，父级变化时会自动清空并重新加载
        prop: 'zoneId',
        label: '库区',
        parentProp: 'warehouseId',
        loadOptions: async (warehouseId) => {
          const res = await getZonesByWarehouse(warehouseId)
          return (res.data || []).map(x => ({ label: x.zoneName, value: x.id }))
        },
      },
    ],
  },
]
```

颜色和图标选择：

```js
const formColumns = [
  // 系统菜单、配置类页面可以用 icon-picker 选 Element Plus 图标
  { prop: 'icon', label: '图标', type: 'icon-picker', maxlength: 50 },
  // 类型、状态、库区配置可以用 color-picker 维护颜色
  { prop: 'color', label: '颜色', type: 'color-picker', showAlpha: false },
]
```

这些高级控件仍然遵守同一条规则：`prop` 是最终写入 `formData`、提交给后端的字段名。`cascade-select` 这种一项控件包含多个字段时，真正提交的是每个 `cascadeItems[].prop`。

## 6 API、请求和返回值判断

### 6.1 请求统一从 `request.js` 出去

`src/utils/request.js` 已经处理：

- 从 `localStorage` 读取 token，并注入 `Authorization: Bearer xxx`。
- 默认显示全局 loading，可用 `{ showLoading: false }` 关闭。
- HTTP 2xx 直接返回 `response.data`。
- 响应头 `x-access-token` / `x-refresh-token` 自动更新本地 token。
- HTTP 401 尝试 refresh token，失败后清登录状态并跳登录页。
- HTTP 402 跳 `/license`。
- 403、404、500、网络异常统一提示。

页面代码里不要重复封装 axios。

### 6.2 标准 CRUD API

```js
import { useCrudApi } from '@/utils/crud'

// module 写 material，对应后端 /api/material
const crudApi = useCrudApi('material')

// 分页查询，会把扁平查询参数转换成后端 pageIndex/pageSize/filters
await crudApi.pageList(params)
// 新增，POST /api/material/create
await crudApi.create(data)
// 修改，POST /api/material/update
await crudApi.update(data)
// 删除，DELETE /api/material/delete/{id}
await crudApi.delete(id)
// 详情，GET /api/material/{id}
await crudApi.detail(id)
// 状态切换，PUT /api/material/status/{id}
await crudApi.setStatus(id, 0)
```

### 6.3 非标准接口放到 `src/api/<domain>.js`

主从表或业务动作不要硬塞进 `useCrudApi`。例如入库单：

```js
// src/api/inbound.js
// 创建入库单：主表和明细行一起提交，不能用 useCrudApi.create
export function createInboundOrder(data) {
  return request.post('/api/inbound-order', data)
}

// 更新入库单：后端接口是 PUT /api/inbound-order/{id}
export function updateInboundOrder(id, data) {
  return request.put(`/api/inbound-order/${id}`, data)
}

// 获取入库单详情：编辑主从表时要加载明细行
export function getInboundOrderDetail(id) {
  return request.get(`/api/inbound-order/${id}/detail`)
}
```

页面中显式导入：

```js
// 从业务 API 文件导入主从表专用接口
import { createInboundOrder, updateInboundOrder, getInboundOrderDetail } from '@/api/inbound'
```

### 6.4 成功判断

项目里常见两类返回：

```js
// code=200 是当前项目最常见的成功格式
{ code: 200, data, message }
// code=0 是部分接口或兼容接口可能使用的成功格式
{ code: 0, data, message }
// success=true 是另一类业务接口可能使用的成功格式
{ success: true, data, message }
```

自定义按钮建议这样判断：

```js
// 后端新旧接口可能存在不同成功格式，统一封装一个判断函数
const isSuccess = (res) => res?.code === 200 || res?.code === 0 || res?.success === true

// row 是当前表格行，newStatus 是要提交给后端的新状态
const res = await crudApi.setStatus(row.id, newStatus)
if (isSuccess(res)) {
  KhMessageFn.success(res.message || '操作成功')
  // 成功后刷新列表，让页面以服务端数据为准
  pageRef.value?.reload()
}
```

`request.js` 已经处理 HTTP 错误提示，所以 `catch` 里通常不要重复弹同一个错误：

```js
try {
  // 执行业务接口
  await updateInboundOrder(id, data)
  // 只有成功才主动提示
  KhMessageFn.success('修改成功')
  pageRef.value?.reload()
} catch {
  // request.js 已处理错误提示
}
```

## 7 路由、菜单和权限

### 7.1 动态路由如何加载页面

`src/router/index.js` 会预加载：

```js
// 预加载 src/views 下所有 .vue 页面，供后端 component 动态匹配
const allViewModules = import.meta.glob('../views/**/*.vue')
```

再过滤：

```js
// 排除业务域 components 子目录，避免弹窗/局部组件被当成路由页面
!key.includes('/components/')
```

然后用后端菜单的 `component` 精确匹配：

```text
后端 component = system/user
前端文件       = src/views/system/user.vue
```

匹配不到时会进入占位页面。

### 7.2 菜单字段怎么填

后端菜单常见字段：

| 字段 | 示例 | 说明 |
| --- | --- | --- |
| `path` | `/system/user` | 浏览器地址 |
| `component` | `system/user` | 前端页面路径，不带 `.vue` |
| `permissionCode` | `system:user` | 路由权限 |
| `permissionName` | `用户管理` | 菜单和页面标题 |
| `buttons[].permKey` | `system:user:add` | 按钮权限 |

### 7.3 KhPage 权限码怎么拼

```vue
<!-- permission-prefix 是后端按钮 permKey 的共同前缀 -->
<KhPage
  module="user"
  :permission-prefix="'system:user'"
/>
```

默认内置按钮权限：

| 操作 | 默认权限 |
| --- | --- |
| 新增 | `system:user:add` |
| 编辑 | `system:user:edit` |
| 删除 | `system:user:delete` |
| 查看 | `system:user:view` |
| 导出 | `system:user:export` |

自定义按钮写完整权限码：

```js
// 自定义按钮不会自动知道业务含义，需要显式写 permission 和 onClick
const actionButtons = [
  {
    label: '禁用',
    // 必须和后端按钮 permKey 一致，否则无权限时无法正确隐藏
    permission: 'system:user:toggle',
    // confirm 会自动渲染确认弹框，确认后才执行 onClick
    confirm: '确定禁用该用户？',
    onClick: handleToggle,
  },
]
```

`KhPage` 会过滤掉不属于当前 `permissionPrefix` 的按钮，防止把其他页面按钮误传进来。

注意：前端权限只负责显示和隐藏，接口权限仍然必须由后端校验。

## 8 字典和扩展字段

### 8.1 字典

字典统一写成 `dict:xxx`。

```js
// 查询/表单下拉：options 指向字典，组件会自动加载选项
{ prop: 'status', label: '状态', type: 'select', options: 'dict:status_flag' }
// 表格标签：tagMap 指向字典，组件会把值映射成文本和颜色
{ prop: 'status', label: '状态', type: 'tag', tagMap: 'dict:status_flag' }
```

`KhPage`、`KhForm`、`KhTable` 会收集列配置里的字典引用，调用 `useDictStore().getDict(type)` 加载并缓存。

字典接口返回后会变成：

```js
{
  // 前端下拉框和标签显示的文字
  label: item.itemLabel,
  // 真正提交给后端或用于匹配 row 字段的值
  value: item.itemValue,
  // el-tag 的颜色类型，例如 success、warning、danger
  tagType: item.tagColor || '',
}
```

### 8.2 扩展字段适用场景

当后端提供：

```text
GET /api/<module>/form-config
```

并且业务数据里有 `extData` 或行级扩展字段时，用 `useExtFields`。

标准单表页面参考 `src/views/basedata/material.vue`、`src/views/basedata/customer.vue`。

### 8.3 单表扩展字段模板

```js
import { useCrudApi } from '@/utils/crud'
import { useExtFields } from '@/utils/useExtFields'

// 标准 CRUD 接口仍然用 useCrudApi
const crudApi = useCrudApi('material')

// useExtFields 负责扩展字段的三件事：
// 1. 加载后端 form-config
// 2. 把扩展字段合并到表格列和表单列
// 3. 提交前把扩展字段整理成 extDataRaw
const {
  // 调 /api/material/form-config 加载扩展字段配置
  loadExtConfig,
  // 合并基础表单列 + 扩展字段列
  mergedColumns,
  // 合并基础表格列 + 扩展字段列
  mergedTableColumns,
  // 提交前提取扩展字段，并从 data 中删除这些临时字段
  extractAndCleanExtData,
  // 创建带 extData 展平能力的 KhPage load 函数
  withFlatExtLoad,
} = useExtFields('/api/material/form-config')

onMounted(() => {
  // 页面挂载后先加载扩展字段配置，后续 computed 才能合并出完整列
  loadExtConfig()
})

// 基础表格列：只写固定业务字段，扩展字段由 mergedTableColumns 自动插入
const baseTableColumns = [
  { prop: 'materialCode', label: '物料编码', width: 130 },
  { prop: 'materialName', label: '物料名称', minWidth: 150 },
  { prop: 'remark', label: '备注', minWidth: 150 },
]

// 最终传给 KhPage 的表格列
const tableColumns = computed(() => mergedTableColumns(baseTableColumns))

// 基础表单列：只写固定业务字段，扩展字段由 mergedColumns 自动插入
const baseFormColumns = [
  { prop: 'materialCode', label: '物料编码', type: 'input', required: true },
  { prop: 'materialName', label: '物料名称', type: 'input', required: true },
  { prop: 'remark', label: '备注', type: 'textarea' },
]

// 最终传给 KhPage 的新增/编辑表单列
const formColumns = computed(() => mergedColumns(baseFormColumns))

// 自定义加载函数：调用分页接口后，把 row.extData 展平成 row.xxx
const load = withFlatExtLoad(crudApi, searchColumns)

// KhPage 内置新增/编辑提交前会调用 beforeSubmit
const beforeSubmit = (data) => {
  // 把表单中的扩展字段收集成 JSON 字符串
  const raw = extractAndCleanExtData(data)
  // 后端通过 extDataRaw 接收扩展字段原始 JSON
  if (raw) data.extDataRaw = raw
}
```

传给 `KhPage`：

```vue
<!--
  扩展字段页面必须把合并后的列、带展平能力的 load、
  以及提交前 beforeSubmit 都传给 KhPage。
-->
<KhPage
  module="material"
  :columns="tableColumns"
  :form-columns="formColumns"
  :load="load"
  :before-submit="beforeSubmit"
/>
```

### 8.4 扩展字段为什么需要 `load` 和 `beforeSubmit`

`load = withFlatExtLoad(...)` 做的是回显：

```text
后端 row.extData JSON
  -> flattenExtData(row)
  -> 展开成 row.xxx
  -> 表格和编辑弹窗能直接显示扩展字段
```

`beforeSubmit` 做的是提交清理：

```text
表单里的扩展字段
  -> extractAndCleanExtData(data)
  -> 删除 data.xxx
  -> 写入 data.extDataRaw
```

如果漏掉 `load`，列表和编辑表单可能看不到扩展字段值。

如果漏掉 `beforeSubmit`，后端可能收到多余字段，或扩展字段没有进入 `extDataRaw`。

## 9 主从表页面怎么写

主从表指一个主单据带多行明细，例如入库单：

```js
{
  // 主表数据，通常对应后端 Order/Header DTO
  order: { orderNo, orderType, warehouseId, ... },
  // 明细行数组，通常对应后端 Line/Item DTO
  lines: [
    { lineNo, materialCode, orderedQty, ... }
  ]
}
```

这类页面不要用 `KhPage` 内置新增/编辑，因为内置新增/编辑只适合单实体提交。

### 9.1 主从表推荐结构

```text
KhPage
  负责主列表、查询、删除、查看详情

toolbarButtons
  自定义新增按钮

actionButtons
  自定义编辑、收货、组盘等行按钮

KhDialog
  手写新增/编辑弹窗

KhForm
  主表字段

KhEditableTable
  明细行

src/api/<domain>.js
  主从表专用 API
```

### 9.2 KhPage 配置

```vue
<!--
  主从表页面仍然用 KhPage 承担列表查询、删除和详情。
  但 create/update 关闭，因为新增/编辑要提交 order + lines。
-->
<KhPage
  ref="pageRef"
  title="入库单管理"
  module="inbound-order"
  :search-columns="searchColumns"
  :search-model="searchModel"
  :columns="tableColumns"
  :crud-operations="{ create: false, update: false, delete: true, view: true, export: false }"
  :permission-prefix="'in:order'"
  :toolbar-buttons="toolbarButtons"
  :action-buttons="actionButtons"
  :detail-lines="detailLineConfigs"
/>
```

重点是关掉内置新增和编辑：

```js
// 关闭内置新增/编辑，避免 KhPage 按单实体提交 create/update
{ create: false, update: false }
```

### 9.3 自定义新增和编辑按钮

```js
// 工具栏按钮：对整个列表生效，这里用来自定义“新增”
const toolbarButtons = [
  {
    label: '新增',
    type: 'primary',
    // Plus 是 Element Plus 图标，markRaw 避免 Vue 把组件变成响应式对象
    icon: markRaw(Plus),
    // 按钮权限码来自后端 permKey
    permission: 'in:order:add',
    onClick: handleCreate,
  },
]

// 行操作按钮：对单行记录生效
const actionButtons = [
  {
    label: '编辑',
    permission: 'in:order:edit',
    // 后端返回 allowedActions 时，用它控制当前行能不能编辑
    show: (row) => row.allowedActions?.includes('EDIT'),
    // row 是当前行，编辑时通常要用 row.id 去加载详情
    onClick: (row) => handleUpdate(row),
  },
]
```

### 9.4 手写弹窗结构

```vue
<!--
  dialogVisible 控制弹窗开关。
  dialogMode 决定标题显示“新增”还是“编辑”。
  submitLoading 会让确认按钮进入 loading，防止重复提交。
  @confirm 点击确认时提交，@close 关闭时重置表单。
-->
<KhDialog
  v-model="dialogVisible"
  :title="dialogMode === 'create' ? '新增入库单' : '编辑入库单'"
  width="1100px"
  destroy-on-close
  :confirm-loading="submitLoading"
  @confirm="handleSubmit"
  @close="resetForm"
>
  <template #default>
    <!-- 主表字段，例如单据类型、仓库、供应商、日期 -->
    <KhForm
      ref="formRef"
      :columns="formColumns"
      v-model="formData"
      :label-width="'90px'"
      :col-count="4"
    />

    <el-divider content-position="left">明细行</el-divider>

    <!-- 明细行表格，v-model 绑定 lines 数组 -->
    <!-- default-row 是点击“添加行”时创建默认空行的函数 -->
    <KhEditableTable
      v-model="lines"
      :columns="lineColumns"
      :default-row="createEmptyLine"
      :max-height="360"
      add-text="添加行"
      :action-width="70"
      @cell-change="handleLineCellChange"
    />
  </template>
</KhDialog>
```

`KhEditableTable` 的 `cell-change` 参数和 `KhTable` 不一样：

```js
// KhEditableTable 参数是 prop、row、index。第三个参数是行索引，不是新值。
const handleLineCellChange = (prop, row, index) => {
  // 如果要拿最新值，从 row[prop] 读取
  const value = row[prop]

  // 例如数量变化后，立即重算当前行金额
  if (prop === 'orderedQty' || prop === 'unitPrice') {
    row.amount = (Number(row.orderedQty) || 0) * (Number(row.unitPrice) || 0)
  }
}
```

### 9.5 提交主从表

```js
const handleSubmit = async () => {
  // KhDialog 使用自定义插槽时，不会自动校验里面的 KhForm。
  // 所以手写弹窗提交前要先调用 KhForm 暴露的 validate。
  const valid = await formRef.value?.validate?.()
  if (!valid) return

  // KhForm 暴露的 formData 是当前主表表单值，先浅拷贝出来，避免直接改组件内部对象
  const submitData = { ...formRef.value.formData }

  // 明细行要整理成后端 DTO 需要的字段，不要把前端临时字段原样提交
  const submitLines = lines.value.map((line, index) => ({
    // 行号通常由前端按当前顺序生成
    lineNo: index + 1,
    materialCode: line.materialCode || '',
    materialName: line.materialName || '',
    // 数量为空时按 0 提交，避免后端收到 null 后计算异常
    orderedQty: line.orderedQty ?? 0,
    batchNo: line.batchNo || '',
    // 日期为空时提交 null，比空字符串更容易被后端识别为“未填”
    manufactureDate: line.manufactureDate || null,
    expiryDate: line.expiryDate || null,
    remark: line.remark || '',
  }))

  // 组装后端需要的主从表请求体
  const requestBody = {
    // 主表数据
    order: {
      ...submitData,
      // totalLines 由当前明细行数量计算，不信任表单里旧值
      totalLines: submitLines.length,
    },
    // 明细行数据
    lines: submitLines,
    // 主表扩展字段
    extDataRaw: extractExtData(submitData),
    // 行级扩展字段
    lineExtDataRaw: extractLineExtData(submitLines),
  }

  // 开启提交 loading，避免用户连续点击确认
  submitLoading.value = true
  try {
    if (dialogMode.value === 'create') {
      // 新增时 id 交给后端生成，避免传 null 或旧 id
      delete requestBody.order.id
      await createInboundOrder(requestBody)
      KhMessageFn.success('新增成功')
    } else {
      // 编辑时用主表 id 定位要更新的单据
      await updateInboundOrder(requestBody.order.id, requestBody)
      KhMessageFn.success('修改成功')
    }

    // 成功后关闭弹窗，并刷新主列表
    dialogVisible.value = false
    pageRef.value?.reload()
  } catch {
    // request.js 已处理错误提示
  } finally {
    // 无论成功失败都关闭 loading
    submitLoading.value = false
  }
}
```

### 9.6 编辑时加载详情

主从表编辑通常要加载详情，而不是只用列表行。

```js
const handleUpdate = async (row) => {
  // 标记当前弹窗是编辑模式，提交时走 update
  dialogMode.value = 'update'
  // 先清空上一次弹窗残留数据
  resetForm()

  try {
    // 主从表编辑必须加载详情，因为列表行通常不包含完整明细
    const res = await getInboundOrderDetail(row.id)
    const detail = res.data || res
    // 兼容后端返回 { order, lines } 或直接返回 order 字段的情况
    const order = detail.order || detail
    const detailLines = detail.lines || []

    // 回填主表。用 Object.assign 保留 formData 的 reactive 引用
    Object.assign(formData, {
      id: order.id,
      orderNo: order.orderNo || '',
      orderType: order.orderType || '',
      warehouseId: order.warehouseId || null,
      supplierId: order.supplierId || null,
      orderDate: order.orderDate || '',
      remark: order.remark || '',
    })

    // 回填明细行。这里只保留弹窗编辑需要的字段
    lines.value = detailLines.map(line => ({
      materialCode: line.materialCode || '',
      materialName: line.materialName || '',
      orderedQty: line.orderedQty ?? null,
      batchNo: line.batchNo || '',
      remark: line.remark || '',
    }))

    // 数据准备完成后再打开弹窗，避免用户看到空表单闪一下
    dialogVisible.value = true
  } catch {
    KhMessageFn.error('加载详情失败')
  }
}
```

## 10 手动弹窗和子弹窗

### 10.1 什么时候拆子组件

拆到 `src/views/<业务域>/components/XxxDialog.vue`：

- 弹窗内部超过 150 行。
- 弹窗自己加载详情或有多次请求。
- 弹窗包含表格、步骤、分栏、较复杂状态。
- 多个页面复用同一个弹窗。

留在当前页面：

- 只是一个简单输入框。
- 逻辑很少，只服务当前页面。
- `KhPage` 内置弹窗已经满足。

### 10.2 父页面打开子弹窗

```vue
<!--
  父页面只负责三件事：
  1. 用 v-model 控制子弹窗显隐
  2. 把当前行的必要信息作为 props 传进去
  3. 子弹窗 success 后刷新 KhPage
-->
<ReceiveDialog
  v-model="receiveDialogVisible"
  :order-id="currentOrder.id"
  :order-no="currentOrder.orderNo"
  :order-status="currentOrder.orderStatus"
  @success="pageRef?.reload()"
/>
```

```js
// 子弹窗开关
const receiveDialogVisible = ref(false)

// 当前正在操作的单据。打开弹窗前从表格行 row 里赋值
const currentOrder = ref({ id: null, orderNo: '', orderStatus: '' })

const handleReceive = (row) => {
  // 保存当前行信息，供 ReceiveDialog props 使用
  currentOrder.value = {
    id: row.id,
    orderNo: row.orderNo,
    orderStatus: row.orderStatus,
  }
  // 打开子弹窗
  receiveDialogVisible.value = true
}
```

### 10.3 子弹窗标准写法

```js
// 子弹窗通过 props 接收父页面传入的数据
const props = defineProps({
  // v-model 对应的值，父页面控制弹窗是否打开
  modelValue: { type: Boolean, default: false },
  // 当前业务对象 id，用来加载详情或提交操作
  orderId: { type: [Number, String], default: null },
})

// update:modelValue 用于 v-model 双向绑定；success 用于通知父页面刷新
const emit = defineEmits(['update:modelValue', 'success'])

// 把 props.modelValue 包成可写 computed，给 KhDialog 的 v-model 使用
const visible = computed({
  // 父页面打开弹窗时，这里读到 true
  get: () => props.modelValue,
  // 子组件关闭弹窗时，通过 emit 写回父页面
  set: (value) => emit('update:modelValue', value),
})
```

提交成功：

```js
try {
  // 子弹窗内部自己调用业务接口
  await receiveInboundOrder(props.orderId, receiveLines)
  KhMessageFn.success('收货成功')
  // 关闭子弹窗，同时同步父页面的 receiveDialogVisible=false
  visible.value = false
  // 通知父页面刷新列表
  emit('success')
} catch {
  // request.js 已处理错误提示
}
```

这套写法的好处是：

- 父组件用 `v-model` 控制开关。
- 子组件自己负责请求、校验、loading。
- 子组件成功后 `emit('success')`，父组件只刷新列表。

### 10.4 `KhDialog` 内置表单和自定义内容的差异

`KhDialog` 有两种用法，确认按钮行为不一样：

| 用法 | 写法 | 点击确认时 |
| --- | --- | --- |
| 内置表单 | `<KhDialog :form-columns="formColumns" :form-model="formData" @confirm="submit" />` | `KhDialog` 自动校验内部 `KhForm`，校验通过后把表单数据作为参数传给 `submit(data)` |
| 自定义内容 | `<KhDialog @confirm="submit"><KhForm ref="formRef" ... /></KhDialog>` | `KhDialog` 不知道你插槽里放了什么，`confirm` 不带参数，也不会自动校验 |

内置表单适合简单单表弹窗：

```vue
<!-- form-columns 非空时，KhDialog 自己创建 KhForm 并负责校验 -->
<KhDialog
  v-model="dialogVisible"
  :form-columns="formColumns"
  :form-model="formData"
  @confirm="handleConfirm"
/>
```

```js
// data 是 KhDialog 内部 KhForm 校验通过后的表单数据
const handleConfirm = async (data) => {
  await crudApi.create(data)
}
```

自定义内容适合主从表、复杂布局，但要自己校验：

```vue
<!-- 自定义内容模式：KhDialog 只负责弹窗和确认按钮，不会自动校验插槽里的 KhForm -->
<KhDialog v-model="dialogVisible" @confirm="handleSubmit">
  <KhForm ref="formRef" v-model="formData" :columns="formColumns" />
  <KhEditableTable v-model="lines" :columns="lineColumns" />
</KhDialog>
```

```js
const handleSubmit = async () => {
  // 自定义插槽里的 KhForm 必须自己校验
  const valid = await formRef.value?.validate?.()
  if (!valid) return

  // 自定义插槽里的表单数据也要自己取
  const submitData = { ...formRef.value.formData }
  await submitOrder({ order: submitData, lines: lines.value })
}
```

## 11 import 规则

`vite.config.js` 已配置自动导入：

- Vue 常用 API：`ref`、`reactive`、`computed`、`watch`、`onMounted` 等。
- Vue Router、Pinia 常用 API。
- `src/components` 下的组件。
- `KhMessageFn`、`KhMsgBoxFn`、`KhNotifyFn`。
- Element Plus 图标，常见于 `markRaw(Plus)`。

所以页面里通常不需要写：

```js
// 自动导入已经覆盖这些常用品，不需要手动写
import { ref, reactive, computed, onMounted } from 'vue'
import KhPage from '@/components/KhPage/index.vue'
import { KhMessageFn } from '@/components/KhMessage/index.vue'
```

需要手动 import 的情况：

| 类型 | 示例 |
| --- | --- |
| 业务 API | `import { createInboundOrder } from '@/api/inbound'` |
| 工具函数 | `import { useCrudApi } from '@/utils/crud'` |
| 扩展字段 | `import { useExtFields } from '@/utils/useExtFields'` |
| 当前目录子组件 | `import ReceiveDialog from './components/ReceiveDialog.vue'` |
| 从组件索引导出的组件 | `import { KhEditableTable } from '@/components'` |

## 12 常见场景速查

### 12.1 新增标准 CRUD 页面

1. 后端提供 `/api/xxx/pagelist/create/update/delete/{id}`。
2. 新建 `src/views/<domain>/<page>.vue`。
3. 后端菜单 `component` 填 `<domain>/<page>`。
4. 页面写 `<KhPage module="xxx" ... />`。
5. 定义 `searchModel/searchColumns/tableColumns/formColumns`。
6. 配 `crudOperations` 和 `permissionPrefix`。
7. 页面验证查询、新增、编辑、删除。

### 12.2 新增状态切换按钮

```js
import { useCrudApi } from '@/utils/crud'

// 创建标准 CRUD API，module=material 对应 /api/material
const crudApi = useCrudApi('material')

// 自定义行按钮：点击后切换当前行状态
const actionButtons = [
  {
    label: (row) => row.status === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'bd:material:toggle',
    confirm: (row) => `确定要${row.status === 1 ? '禁用' : '启用'}吗？`,
    onClick: async (row) => {
      // 根据当前状态计算目标状态
      const newStatus = row.status === 1 ? 0 : 1
      // 调用标准状态接口
      const res = await crudApi.setStatus(row.id, newStatus)
      if (res.code === 200 || res.code === 0 || res.success === true) {
        KhMessageFn.success(res.message || '操作成功')
        // 状态变化后刷新列表
        pageRef.value?.reload()
      }
    },
  },
]
```

### 12.3 添加工具栏按钮

工具栏按钮用于对整个列表生效的动作，例如新增、导入、批量处理。

```js
// 工具栏按钮用于“整页动作”，不依赖某一行
const toolbarButtons = [
  {
    label: '新增',
    type: 'primary',
    // 图标来自 Element Plus，已由 vite.config.js 自动导入
    icon: markRaw(Plus),
    permission: 'in:order:add',
    onClick: handleCreate,
  },
]
```

```vue
<!-- 把工具栏按钮交给 KhPage，KhPage 会转交给 KhTable 渲染 -->
<KhPage :toolbar-buttons="toolbarButtons" />
```

### 12.4 添加行操作按钮

行操作按钮用于单行动作，例如编辑、收货、组盘、作废。

```js
// 行操作按钮用于“单行动作”，onClick 会拿到当前行 row
const actionButtons = [
  {
    label: '收货',
    permission: 'in:order:receive',
    // 只有后端允许 RECEIVE 时才显示按钮
    show: (row) => row.allowedActions?.includes('RECEIVE'),
    onClick: (row) => handleReceive(row),
  },
]
```

```vue
<!-- 把行操作按钮交给 KhPage，最终渲染在表格操作列 -->
<KhPage :action-buttons="actionButtons" />
```

### 12.5 添加详情从表

```js
// detailLines 用于 KhPage 内置详情弹窗里的从表区域
const detailLineConfigs = [
  {
    // prop 对应详情接口返回数据里的数组字段，例如 detail.items
    prop: 'items',
    title: '入库明细行',
    // columns 是从表自己的列配置
    columns: [
      { prop: 'lineNo', label: '行号', width: 70 },
      { prop: 'materialCode', label: '物料编码', minWidth: 130 },
      { prop: 'orderedQty', label: '订单数量', width: 120, align: 'right' },
    ],
  },
]
```

```vue
<!-- view=true 才会生成内置“查看”按钮并打开详情弹窗 -->
<KhPage :detail-lines="detailLineConfigs" :crud-operations="{ view: true }" />
```

### 12.6 添加导出

```js
// 开启 export 后，KhPage 会生成导出按钮和导出下拉菜单
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: true,
  export: true,
}
```

`KhPage` 会生成导出下拉：

- 导出当前页。
- 导出所有数据。

导出列会根据 `columns` 生成，字典列会把映射传给后端。

### 12.7 添加表头筛选

页面传给 `KhPage`：

```vue
<!-- 只打开 show-header-filter 还不够，具体列还要配置 searchable -->
<KhPage :show-header-filter="true" />
```

需要筛选的列加：

```js
// searchable=true 表示这一列表头显示筛选入口
{ prop: 'materialCode', label: '物料编码', searchable: true, filterType: 'input' }
// select 筛选通常配 filterOptions；这里使用字典
{ prop: 'status', label: '状态', searchable: true, filterType: 'select', filterOptions: 'dict:status_flag' }
```

表头筛选会和查询区参数一起进入分页请求。

## 13 提交前检查清单

提交前逐项看一遍：

- 页面文件在 `src/views/<业务域>/<页面>.vue`，不在 `components/`。
- 后端菜单 `component` 等于页面路径，不带 `.vue`。
- `module` 等于后端 `api/xxx` 的 `xxx`。
- `permissionPrefix` 和后端按钮权限码前缀一致。
- `searchModel` 包含所有查询字段初始值。
- `searchColumns.prop` 和后端查询字段一致。
- `tableColumns.prop` 和后端列表返回字段一致。
- `formColumns.prop` 和新增/编辑 DTO 字段一致。
- 字典编码真实存在，写法是 `dict:xxx`。
- 自定义按钮都配置了 `permission`。
- 自定义成功操作后有 `KhMessageFn.success(...)` 和 `pageRef.value?.reload()`。
- HTTP 错误不要在 `catch` 里重复提示。
- `KhPage` 页面没有把 `@cell-change`、`@row-click` 这类 `KhTable` 事件直接写在 `KhPage` 上。
- 主从表没有使用 `KhPage` 内置新增/编辑。
- 扩展字段页面已经配置 `loadExtConfig()`、`load`、`beforeSubmit`。
- 手写弹窗提交时有 `submitLoading`，防止重复提交。
- 手写 `KhDialog` 里如果放了自定义 `KhForm`，提交前已经手动 `validate()`。
- 编辑回填 `reactive` 对象时使用 `Object.assign`。
- 表格字典列使用 `type: 'tag'`，不要把表单里的 `type: 'select'` 直接照搬到表格列。

## 14 常见坑

| 坑 | 现象 | 正确做法 |
| --- | --- | --- |
| 页面放到 `views/**/components` | 菜单打开占位页 | 菜单页面放在业务域根目录 |
| 菜单 `component` 写错 | 动态路由找不到页面 | 必须等于 `src/views/<component>.vue` |
| `module` 写错 | 列表、新增、编辑、删除 404 | 和后端 `[Route("api/xxx")]` 对齐 |
| `permissionPrefix` 写错 | 按钮不显示或权限混乱 | 和后端 `permKey` 前缀一致 |
| `searchModel` 没有字段 | 输入后查询参数丢失 | 给每个查询字段设置初始值 |
| `@cell-change` 写在 `KhPage` 上 | 内联开关切换后没有进入处理函数 | 用 `actionButtons`，或用 `type:'slot'` 自己渲染开关 |
| 把 `KhTable` 的 `cell-change` 参数照搬到 `KhEditableTable` | 第三个参数被误当成新值 | `KhEditableTable` 第三个参数是行索引，新值从 `row[prop]` 读 |
| 表格列写 `type:'select', tagMap:'dict:xxx'` | 表格只显示原始值，不显示标签 | 表格字典展示用 `type:'tag'` |
| 手写弹窗以为 `KhDialog` 会校验插槽里的 `KhForm` | 必填没填也提交了 | 自定义插槽提交前手动 `await formRef.value.validate()` |
| 在 `beforeSubmit` 里做异步校验 | 返回 Promise 后仍继续提交 | `beforeSubmit` 只做同步整理；异步校验用手写弹窗 |
| 把主从表交给内置编辑 | 请求体不符合后端 DTO | 关掉内置新增/编辑，手写弹窗 |
| 编辑必须详情却用了内置编辑 | 弹窗只有列表行字段 | 自己写 `handleUpdate` 调详情 |
| extData 漏掉 `load` | 列表和编辑不显示扩展字段 | 用 `withFlatExtLoad` |
| extData 漏掉 `beforeSubmit` | 扩展字段没进 `extDataRaw` | 用 `extractAndCleanExtData` |
| 替换 `reactive` 对象 | 表单不刷新或失去响应式 | 使用 `Object.assign` |
| 成功后不刷新 | 列表仍显示旧数据 | `pageRef.value?.reload()` |
| catch 里重复提示 | 用户看到两次错误 | HTTP 错误交给 `request.js` |
| 只做前端权限 | 直接调接口仍可操作 | 后端也必须校验权限 |

## 附录 A 常用文件索引

| 文件 | 用途 |
| --- | --- |
| `KH.WMS.Client/src/views/basedata/material.vue` | 标准 CRUD + 扩展字段样板 |
| `KH.WMS.Client/src/views/basedata/customer.vue` | 简洁 CRUD + 扩展字段样板 |
| `KH.WMS.Client/src/views/inbound/order.vue` | 主从表、手写弹窗、行级扩展字段样板 |
| `KH.WMS.Client/src/views/inbound/components/ReceiveDialog.vue` | 子弹窗 `v-model + success` 样板 |
| `KH.WMS.Client/src/components/KhPage/index.vue` | 页面壳、内置 CRUD、按钮合并、详情 |
| `KH.WMS.Client/src/components/KhForm/index.vue` | 表单配置、校验、折叠查询 |
| `KH.WMS.Client/src/components/KhTable/index.vue` | 表格、分页、表头筛选、操作列 |
| `KH.WMS.Client/src/components/KhEditableTable/index.vue` | 主从表明细行编辑 |
| `KH.WMS.Client/src/components/KhDialog/index.vue` | 弹窗、内置表单、确认事件 |
| `KH.WMS.Client/src/utils/dict-resolve.js` | `dict:xxx` 解析、字典颜色映射 |
| `KH.WMS.Client/src/utils/crud.js` | `useCrudApi` 和分页查询转换 |
| `KH.WMS.Client/src/utils/request.js` | axios、token、loading、错误提示 |
| `KH.WMS.Client/src/utils/useExtFields.js` | 扩展字段加载、回显、提交 |
| `KH.WMS.Client/src/router/index.js` | 动态路由和登录守卫 |
| `KH.WMS.Client/src/stores/permission.js` | 菜单、路由、按钮权限 |
| `KH.WMS.Client/src/stores/dict.js` | 字典缓存 |
| `KH.WMS.Client/src/api/inbound.js` | 非标准业务 API 示例 |

## 附录 B 常用命令

```bash
cd KH.WMS.Client
npm install
npm run dev
npm run build
npm run preview
npm run test:e2e
```

开发默认访问：

```text
http://localhost:3000
```

Vite 代理默认后端：

```text
http://localhost:9291
```

生产环境 API 地址由 `public/config.js` 控制：

```js
// 部署后可直接改 dist/config.js，不需要重新 npm run build
window.__APP_CONFIG__ = {
  // 同源部署或由 Nginx/IIS 反代 /api 时留空；分离部署时填后端网关地址
  API_BASE_URL: '',
}
```
