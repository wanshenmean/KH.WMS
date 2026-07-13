<template>
  <div class="kh-page">
    <!-- 统计卡片区域 -->
    <div v-if="showStatCards && statCards.length" class="kh-page__stats">
      <div class="kh-page__stats-grid">
        <div v-for="card in statCards" :key="card.label || card.prop" class="kh-page__stats-item">
          <KhStatCard :value="card.value" :label="card.label" :icon="card.icon" :theme="card.theme || 'primary'"
            :clickable="card.clickable || false" :formatter="card.formatter"
            @click="(val) => emit('stat-click', card, val)">
            <template v-if="$slots['stat-extra']" #extra>
              <slot name="stat-extra" :card="card" />
            </template>
          </KhStatCard>
        </div>
      </div>
      <slot name="stat-extra-row" />
    </div>

    <!-- 查询表单区域 -->
    <div v-if="showSearch && searchColumns.length" class="kh-page__search">
      <KhForm ref="searchFormRef" :columns="searchFormColumns" :model-value="searchModel"
        @update:model-value="onSearchModelUpdate" :col-count="searchColCount"
        :collapsible="collapsible" :default-collapsed="defaultCollapsed"
        :quick-search-placeholder="quickSearchPlaceholder" @search="handleSearch" @reset="handleReset">
        <template #extra-buttons>
          <slot name="search-extra-buttons" />
        </template>
      </KhForm>
    </div>

    <!-- 表格区域 -->
    <div ref="tableWrapRef" class="kh-page__table">
      <KhTable ref="tableRef" :columns="columns" :load="effectiveLoad" :extra-params="searchModel"
        :show-toolbar="showToolbar" :show-pagination="showPagination" :show-index="showIndex"
        :show-selection="showSelection" :border="border" :stripe="stripe" :height="tableHeight"
        :toolbar-buttons="mergedToolbarButtons" :action-buttons="mergedActionButtons" v-bind="tableAttrs" :title="title">
        <template #toolbar-left>
          <slot name="toolbar-left" />
        </template>
        <template #toolbar-right>
          <slot name="toolbar-right" />
        </template>
        <template v-if="hasActionContent" #action="scope">
          <slot name="action" v-bind="scope" />
        </template>
        <template v-for="name in extraSlotNames" :key="name" #[name]="scope">
          <slot :name="name" v-bind="scope || {}" />
        </template>
      </KhTable>
    </div>

    <!-- 内置新增/编辑弹窗 -->
    <KhDialog v-if="module" v-model="dialogVisible"
      :title="title + ' - ' + (dialogMode === 'create' ? createLabel : '编辑')" :width="dialogWidth"
      :form-columns="activeFormColumns" :form-model="formData" :form-col-count="dialogColCount"
      :confirm-loading="dialogLoading" @confirm="handleDialogConfirm"/>

    <!-- 内置查看详情弹窗 -->
    <KhDetailDialog v-if="module && crudOperations.view" v-model="detailVisible" :title="title + ' 详情'" :width="detailWidth"
      :data="detailData" :items="detailItems" :line-configs="detailLinesConfig" :column="2" />
  </div>
</template>

<script setup>
import { useCrudApi, buildPageQuery } from '@/utils/crud'
import { useDictStore } from '@/stores/dict'
import { resolveColumn, collectDictTypes } from '@/utils/dict-resolve'
import KhDialog from '@/components/KhDialog/index.vue'
import KhDetailDialog from '@/components/KhDetailDialog/index.vue'

const dictStore = useDictStore()

const props = defineProps({
  /** 页面标题，用于弹窗标题前缀（如 "用户管理" → 弹窗显示 "用户管理 - 新增"） */
  title: { type: String, default: '' },
  /** 统计卡片配置数组 */
  statCards: { type: Array, default: () => [] },
  /** 查询表单列配置（传给 KhForm） */
  searchColumns: { type: Array, default: () => [] },
  /** 查询表单数据模型 */
  searchModel: { type: Object, default: () => ({}) },
  /** 表格列配置（传给 KhTable） */
  columns: { type: Array, required: true, default: () => [] },
  /** 数据加载函数（传了 module 时可不传，内部自动生成） */
  load: { type: Function, default: null },
  /** 是否显示统计卡片，默认 true */
  showStatCards: { type: Boolean, default: true },
  /** 是否显示查询表单，默认 true */
  showSearch: { type: Boolean, default: true },
  /** 查询表单是否可折叠，默认 true */
  collapsible: { type: Boolean, default: true },
  /** 查询表单默认是否收起，仅在 collapsible 为 true 时生效，默认 true */
  defaultCollapsed: { type: Boolean, default: true },
  /** 查询表单每行列数，默认 4 */
  searchColCount: { type: Number, default: 4 },
  /** 收起态搜索框占位文字，不传时自动使用第一个查询字段 */
  quickSearchPlaceholder: { type: String, default: '' },
  /** 统计卡片栅格数，默认 6（一行 4 个） */
  statSpan: { type: Number, default: 6 },
  /** 是否显示工具栏，默认 true */
  showToolbar: { type: Boolean, default: true },
  /** 是否显示分页，默认 true */
  showPagination: { type: Boolean, default: true },
  /** 是否显示序号列，默认 true */
  showIndex: { type: Boolean, default: true },
  /** 是否显示多选列，默认 true */
  showSelection: { type: Boolean, default: true },
  /** 表格边框，默认 true */
  border: { type: Boolean, default: true },
  /** 斑马纹，默认 false */
  stripe: { type: Boolean, default: true },

  // ==================== CRUD 相关 ====================
  /** 模块名，传入后自动启用内置 CRUD（如 'user' → /api/user/pagelist） */
  module: { type: String, default: '' },
  /** 表单列配置，用于新增/编辑弹窗。不传则尝试从后端 form-config 接口获取 */
  formColumns: { type: Array, default: () => [] },
  /** 自定义表单数据模型 */
  customFormData: { type: Object, default: undefined },
  /** 弹窗宽度，默认 '800px' */
  dialogWidth: { type: [String, Number], default: '800px' },
  /** 弹窗每行列数，默认 2 */
  dialogColCount: { type: Number, default: 2 },
  /** 权限码前缀，自动拼接操作名（如 :add/:edit/:delete），默认使用 module */
  permissionPrefix: { type: String, default: '' },
  /** 权限码操作名映射，默认 { create: 'add', update: 'edit', delete: 'delete', view: 'query', export: 'export' } */
  permissionMap: {
    type: Object,
    default: () => ({
      create: 'add',
      update: 'edit',
      delete: 'delete',
      view: 'view',
      export: 'export',
    }),
  },
  /** 各操作开关 */
  crudOperations: {
    type: Object,
    default: () => ({
      create: true,
      update: true,
      delete: true,
      view: false,
      export: false,
    }),
  },
  /** 新增按钮文字，默认 '新增' */
  createLabel: { type: String, default: '新增' },
  /** 新增按钮图标 */
  createIcon: { type: [Object, null], default: null },
  /** 新增前的回调，返回 false 可阻止打开弹窗 */
  beforeCreate: { type: Function, default: null },
  /** 编辑按钮是否显示的回调，接收 row，返回 false 则该行不显示编辑按钮 */
  updateShow: { type: Function, default: null },
  /** 编辑前的回调，接收 row，返回 false 可阻止 */
  beforeUpdate: { type: Function, default: null },
  /** 删除按钮是否显示的回调，接收 row，返回 false 则该行不显示删除按钮 */
  deleteShow: { type: Function, default: null },
  /** 删除前的回调，接收 row，返回 false 可阻止 */
  beforeDelete: { type: Function, default: null },
  /** 删除确认文案，默认 '确定删除该数据?' */
  deleteConfirmText: { type: String, default: '确定删除该数据?' },
  /** 提交前的回调，接收 (data, mode)，mode='create'|'update'，返回 false 可阻止提交 */
  beforeSubmit: { type: Function, default: null },
  /** 提交后的回调，接收 (response, mode) */
  afterSubmit: { type: Function, default: null },
  /** 详情弹窗宽度，默认 '500px' */
  detailWidth: { type: [String, Number], default: '900px' },
  /**
   * 从表配置数组，用于详情弹窗中展示主从关系的明细行
   * [{ prop, title?, columns, maxHeight? }]
   * - prop: 后端返回数据中对应的数组字段名（如 'lines'）
   * - title: 从表区域标题，默认 '明细行'
   * - columns: el-table 列配置 [{ prop, label, width?, ... }]
   */
  detailLines: { type: Array, default: () => [] },
  /** 外部工具栏按钮（与内置按钮合并，内置在前） */
  toolbarButtons: { type: Array, default: () => [] },
  /** 外部操作列按钮（与内置按钮合并，内置在前） */
  actionButtons: { type: Array, default: () => [] },
})

const emit = defineEmits([
  'search',
  'reset',
  'stat-click',
])

const attrs = useAttrs()
const slots = useSlots()

// 需要透传给 KhTable 的额外 slot 名（排除已显式处理的）
const reservedSlotNames = new Set(['toolbar-left', 'toolbar-right', 'action', 'stat-extra', 'stat-extra-row', 'search-extra-buttons'])
const extraSlotNames = computed(() => Object.keys(slots).filter(name => !reservedSlotNames.has(name)))

// 排除已声明的 props 和 slot，剩余透传给 KhTable
const tableAttrs = computed(() => {
  const reserved = new Set([
    'class', 'style', 'modelValue', 'statCards', 'searchColumns', 'searchModel',
    'columns', 'load', 'showStatCards', 'showSearch', 'collapsible', 'defaultCollapsed',
    'searchColCount', 'quickSearchPlaceholder', 'statSpan', 'showToolbar', 'showPagination',
    'showIndex', 'showSelection', 'border', 'stripe',
    'module', 'formColumns', 'dialogWidth', 'dialogColCount', 'permissionPrefix',
    'crudOperations', 'createLabel', 'createIcon', 'beforeCreate', 'updateShow', 'beforeUpdate',
    'deleteShow', 'beforeDelete', 'deleteConfirmText', 'beforeSubmit', 'afterSubmit', 'detailWidth',
    'detailLines', 'toolbarButtons', 'actionButtons',
  ])
  const result = {}
  for (const [key, value] of Object.entries(attrs)) {
    if (!reserved.has(key) && !key.startsWith('on')) {
      result[key] = value
    }
  }
  return result
})

const searchFormRef = ref(null)
const tableRef = ref(null)
const tableWrapRef = ref(null)
const tableHeight = ref(undefined)

// ==================== ResizeObserver ====================

let resizeObserver = null
const updateTableHeight = () => {
  if (!tableWrapRef.value) return
  const wrap = tableWrapRef.value
  const style = getComputedStyle(wrap)
  const padTop = parseFloat(style.paddingTop) || 0
  const padBottom = parseFloat(style.paddingBottom) || 0
  const available = wrap.clientHeight - padTop - padBottom
  const toolbar = wrap.querySelector('.kh-table__toolbar')
  const pagination = wrap.querySelector('.kh-table__pagination')
  const toolbarH = toolbar ? toolbar.offsetHeight + 12 : 0
  const paginationH = pagination ? pagination.offsetHeight : 0
  const h = available - toolbarH - paginationH
  if (h > 100) {
    tableHeight.value = h
  }
}

onMounted(() => {
  // 预加载所有字典数据（含从表列配置）
  const detailLineColumns = props.detailLines.flatMap(l => l.columns || [])
  const allColumns = [...props.searchColumns, ...(props.formColumns || []), ...props.columns, ...detailLineColumns]
  const dictTypes = collectDictTypes(allColumns)
  dictTypes.forEach(type => dictStore.getDict(type))
  nextTick(updateTableHeight)
  if (tableWrapRef.value) {
    resizeObserver = new ResizeObserver(updateTableHeight)
    resizeObserver.observe(tableWrapRef.value)
  }
})

onBeforeUnmount(() => {
  resizeObserver?.disconnect()
  resizeObserver = null
})

// ==================== 搜索表单 ====================

/** 实际传给 KhForm 的列配置（末尾追加查询/重置按钮） */
const searchFormColumns = computed(() => {
  if (!props.searchColumns.length) return []
  return [
    ...props.searchColumns,
    { type: 'buttons', label: '', prop: '_buttons' },
  ]
})

const handleSearch = () => {
  tableRef.value?.reload()
  emit('search', props.searchModel)
}

/**
 * KhForm 内部 formData 变化时，同步回外部 searchModel（改属性、不替换引用），
 * 使 KhTable 的 extraParams 能拿到最新查询条件。
 * 修复：原先用 :model 传给 KhForm（KhForm prop 是 modelValue），导致输入不同步、搜索参数丢失。
 */
const onSearchModelUpdate = (val) => {
  if (!val || typeof val !== 'object') return
  Object.keys(val).forEach((k) => {
    props.searchModel[k] = val[k]
  })
}

const handleReset = () => {
  Object.keys(props.searchModel).forEach(key => {
    props.searchModel[key] = ''
  })
  tableRef.value?.reload()
  emit('reset')
}

// ==================== CRUD 内置逻辑 ====================

/** 权限码前缀 */
const permPrefix = computed(() => props.permissionPrefix || props.module)

/** CRUD API 实例 */
const crudApi = computed(() => props.module ? useCrudApi(props.module) : null)

/** 内置分页加载函数 */
const internalLoadFn = async (params) => {
  const res = await crudApi.value.pageList({ ...params, searchColumns: props.searchColumns })
  return {
    data: res.data?.items ?? [],
    total: res.data?.total ?? 0,
  }
}

/** 传给 KhTable 的 load：外部 load 优先，其次用内置 */
const effectiveLoad = computed(() => props.load || (crudApi.value ? internalLoadFn : null))

// ==================== 内置按钮 ====================

/** 新增/编辑弹窗状态 */
const dialogVisible = ref(false)
const dialogMode = ref('create')
const dialogLoading = ref(false)
const formData = ref({})
const activeFormColumns = ref([])

/** 查看详情弹窗状态 */
const detailVisible = ref(false)
const detailData = ref({})

/** 缓存后端表单配置（避免每次打开弹窗都请求） */
let cachedFormConfig = null

/** 获取表单列配置：优先用页面传入的，否则从后端接口获取 */
const fetchFormColumns = async () => {
  if (props.formColumns.length) {
    activeFormColumns.value = props.formColumns
    return
  }
  if (cachedFormConfig) {
    activeFormColumns.value = cachedFormConfig
    return
  }
  try {
    const res = await crudApi.value.formConfig()
    cachedFormConfig = res.data || []
    activeFormColumns.value = cachedFormConfig
  } catch {
    activeFormColumns.value = []
  }
}

/** 格式化详情值（tag 类型映射显示） */
const formatDetailValue = (val, col) => {
  if (val === null || val === undefined) return '-'
  // 解析 dict 引用的 tagMap
  const resolvedCol = resolveColumn(col, dictStore.cache)
  if (resolvedCol.type === 'tag' && resolvedCol.tagMap && resolvedCol.tagMap[val] !== undefined) {
    const mapped = resolvedCol.tagMap[val]
    return ['success', 'warning', 'danger', 'info', 'primary'].includes(mapped) ? val : mapped
  }
  if (resolvedCol.type === 'switch') return val ? '是' : '否'
  return val
}

/** 详情弹窗中展示的项（排除 slot/expand 等非数据列，转为 KhDetailDialog items 格式） */
const detailItems = computed(() =>
  props.columns
    .filter(c => c.prop && c.type !== 'slot' && c.type !== 'expand')
    .map(col => {
      const resolved = resolveColumn(col, dictStore.cache)
      const item = {
        prop: resolved.prop,
        label: resolved.label,
        span: resolved.span || 1,
      }
      // tag 类型：传递 tagMap 和 tagTypeMap
      if (resolved.type === 'tag' && resolved.tagMap) {
        item.type = 'tag'
        item.tagMap = resolved.tagMap
        // 根据值自动推断颜色：数字 1/0 对应 success/danger，字符串做简单映射
        item.tagTypeMap = resolved.tagTypeMap || buildDefaultTagTypeMap(resolved.tagMap)
      } else if (resolved.type === 'switch') {
        item.type = 'tag'
        item.tagMap = { 1: '是', 0: '否', true: '是', false: '否' }
        item.tagTypeMap = { 1: 'success', 0: 'info', true: 'success', false: 'info' }
      }
      // 自定义格式化
      if (resolved.formatter) {
        item.formatter = resolved.formatter
      }
      return item
    })
)

/** 从表配置：解析 detailLines 中的字典引用 */
const detailLinesConfig = computed(() =>
  props.detailLines.map(line => ({
    ...line,
    columns: (line.columns || []).map(col => {
      const resolved = resolveColumn(col, dictStore.cache)
      const result = {
        prop: resolved.prop,
        label: resolved.label,
        width: resolved.width,
        minWidth: resolved.minWidth,
        align: resolved.align,
        showOverflowTooltip: resolved.showOverflowTooltip,
      }
      if (resolved.type === 'tag' && resolved.tagMap) {
        result.type = 'tag'
        result.tagMap = resolved.tagMap
        result.tagTypeMap = resolved.tagTypeMap || buildDefaultTagTypeMap(resolved.tagMap)
      }
      if (resolved.formatter) {
        result.formatter = resolved.formatter
      }
      return result
    }),
  }))
)

/** 根据 tagMap 的值自动生成 tagTypeMap */
function buildDefaultTagTypeMap(tagMap) {
  if (typeof tagMap !== 'object') return {}
  const typeMap = {}
  const entries = Object.entries(tagMap)
  entries.forEach(([key, val]) => {
    if (typeMap[key]) return
    const valStr = String(val).toLowerCase()
    if (valStr.includes('启用') || valStr.includes('正常') || valStr.includes('成功') || valStr.includes('是') || valStr.includes('完成'))
      typeMap[key] = 'success'
    else if (valStr.includes('禁用') || valStr.includes('删除') || valStr.includes('否') || valStr.includes('取消') || valStr.includes('冻结'))
      typeMap[key] = 'danger'
    else if (valStr.includes('警告') || valStr.includes('预警') || valStr.includes('锁定') || valStr.includes('维护'))
      typeMap[key] = 'warning'
    else if (valStr.includes('草稿') || valStr.includes('待'))
      typeMap[key] = 'info'
    else
      typeMap[key] = 'primary'
  })
  return typeMap
}

// ==================== 内置操作处理函数 ====================

const handleCreate = async () => {
  if (props.beforeCreate?.() === false) return
  dialogMode.value = 'create'
  if (props.customFormData) {
    formData.value = { ...props.customFormData }
  } else {
    formData.value = {}
  }
  await fetchFormColumns()
  dialogVisible.value = true
}

const handleUpdate = async (row) => {
  if (props.beforeUpdate?.(row) === false) return
  dialogMode.value = 'update'
  await fetchFormColumns()
  formData.value = { ...row }
  dialogVisible.value = true
}

const handleView = async (row) => {
  try {
    const res = await crudApi.value.detail(row.id)
    detailData.value = res.data || row
  } catch {
    detailData.value = row
  }
  detailVisible.value = true
}

const handleDelete = async (row) => {
  if (props.beforeDelete?.(row) === false) return
  try {
    await crudApi.value.delete(row.id)
    KhMessageFn.success('删除成功')
    tableRef.value?.reload()
  } catch {
    // request.js 已处理错误提示
  }
}

const exportLoading = ref(false)

/** 构建导出列配置（prop/label/字典映射），供后端生成中文表头 */
const buildExportColumns = () => {
  return props.columns.map(col => {
    const item = { prop: col.prop, label: col.label }
    // 如果列使用了字典 tagMap，将已解析的 tagMap 作为字典映射发给后端
    const resolved = resolveColumn(col, dictStore.cache)
    if (resolved.tagMap && typeof resolved.tagMap === 'object' && !Array.isArray(resolved.tagMap)) {
      item.dictMap = { ...resolved.tagMap }
    }
    return item
  })
}

/** 导出为 Excel 并触发下载 */
const handleExport = async (exportAll = true) => {
  if (!crudApi.value) return
  exportLoading.value = true
  try {
    // 合并搜索条件与表格当前查询参数
    const tableParams = tableRef.value?.getQueryParams?.() || {}
    const query = buildPageQuery({ ...props.searchModel, ...tableParams })
    const exportColumns = buildExportColumns()
    const res = await crudApi.value.export(query, exportAll, exportColumns)
    const base64 = res.data
    if (!base64) {
      KhMessageFn.warning('导出数据为空')
      return
    }
    const bytes = atob(base64)
    const buf = new Uint8Array(bytes.length)
    for (let i = 0; i < bytes.length; i++) buf[i] = bytes.charCodeAt(i)
    const blob = new Blob([buf], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `${props.title || props.module || 'export'}.xlsx`
    a.click()
    URL.revokeObjectURL(url)
    KhMessageFn.success('导出成功')
  } catch (error) {
    console.error('[Export]', error)
    if (error?.message) KhMessageFn.error('导出失败: ' + error.message)
  } finally {
    exportLoading.value = false
  }
}

const handleDialogConfirm = async (data) => {
  if (props.beforeSubmit?.(data, dialogMode.value) === false) return
  dialogLoading.value = true
  try {
    const res = dialogMode.value === 'create'
      ? await crudApi.value.create(data)
      : await crudApi.value.update(data)
    props.afterSubmit?.(res, dialogMode.value)
    KhMessageFn.success(dialogMode.value === 'create' ? '新增成功' : '修改成功')
    dialogVisible.value = false
    tableRef.value?.reload()
  } catch {
    // request.js 已处理错误提示
  } finally {
    dialogLoading.value = false
  }
}

// ==================== 合并按钮 ====================

/** 获取权限码 */
const getPerm = (action) => `${permPrefix.value}:${props.permissionMap[action] || action}`

/** 内置工具栏按钮 */
const internalToolbarButtons = computed(() => {
  if (!props.module) return []
  const btns = []
  const ops = props.crudOperations
  if (ops.create) {
    btns.push({
      label: props.createLabel,
      type: 'primary',
      icon: props.createIcon,
      permission: getPerm('create'),
      onClick: handleCreate,
    })
  }
  if (ops.export) {
    btns.push({
      label: '导出',
      type: 'success',
      loading: exportLoading.value,
      permission: getPerm('export'),
      onClick: () => handleExport(true),
      dropdown: [
        { label: '导出当前页', onClick: () => handleExport(false) },
        { label: '导出所有数据', onClick: () => handleExport(true) },
      ],
    })
  }
  return btns
})

/** 内置操作列按钮 */
const internalActionButtons = computed(() => {
  if (!props.module) return []
  const btns = []
  const ops = props.crudOperations
  if (ops.view) {
    btns.push({
      label: '查看',
      permission: getPerm('view'),
      onClick: (row) => handleView(row),
    })
  }
  if (ops.update) {
    const btn = {
      label: '编辑',
      permission: getPerm('update'),
      onClick: (row) => handleUpdate(row),
    }
    if (props.updateShow) {
      btn.show = (row) => props.updateShow(row)
    }
    btns.push(btn)
  }
  if (ops.delete) {
    const btn = {
      label: '删除',
      type: 'danger',
      permission: getPerm('delete'),
      confirm: props.deleteConfirmText,
      onClick: (row) => handleDelete(row),
    }
    if (props.deleteShow) {
      btn.show = (row) => props.deleteShow(row)
    }
    btns.push(btn)
  }
  return btns
})

/**
 * 校验按钮权限码是否属于当前页面前缀
 * - 无 permission 属性的按钮不受前缀限制（如纯展示按钮）
 * - 有 permission 的按钮必须以 permPrefix + ':' 开头，防止其他页面的按钮在当前页面显示
 */
const isButtonPermissionMatched = (btn) => {
  if (!btn.permission) return true
  const prefix = permPrefix.value
  if (!prefix) return true
  return btn.permission === prefix || btn.permission.startsWith(`${prefix}:`)
}

/** 合并后的工具栏按钮（内置在前，外部在后，过滤掉不属于当前页面前缀的按钮） */
const mergedToolbarButtons = computed(() => [
  ...internalToolbarButtons.value,
  ...(props.toolbarButtons || []).filter(isButtonPermissionMatched),
])

/** 合并后的操作列按钮（内置在前，外部在后，过滤掉不属于当前页面前缀的按钮） */
const mergedActionButtons = computed(() => [
  ...internalActionButtons.value,
  ...(props.actionButtons || []).filter(isButtonPermissionMatched),
])

/** 是否存在操作列内容（仅当使用者自定义了 action slot 时才传递，内置按钮通过 actionButtons prop 走权限判断） */
const hasActionContent = computed(() => {
  return !!slots.action
})

// ==================== Expose ====================

defineExpose({
  tableRef,
  searchFormRef,
  reload: () => tableRef.value?.reload(),
  refresh: () => tableRef.value?.refresh(),
  getSelectionRows: () => tableRef.value?.getSelectionRows() || [],
  clearSelection: () => tableRef.value?.clearSelection(),
  /** 手动打开新增弹窗 */
  openCreateDialog: handleCreate,
  /** 手动打开编辑弹窗 */
  openUpdateDialog: (row) => handleUpdate(row),
  /** 手动打开详情弹窗 */
  openDetailDialog: (row) => handleView(row),
})
</script>

<style scoped>
.kh-page {
  display: flex;
  flex-direction: column;
  gap: 12px;
  flex: 1;
  min-height: 0;
  overflow: hidden;
}

.kh-page__stats {
  margin-bottom: 0;
  flex-shrink: 0;
}

.kh-page__stats-grid {
  display: flex;
  gap: 16px;
}

.kh-page__stats-item {
  flex: 1;
  min-width: 0;
}

.kh-page__stats :deep(.el-card) {
  border: none;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}

.kh-page__search {
  margin-bottom: 0;
  padding: 16px 20px 4px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
  flex-shrink: 0;
}

.kh-page__table {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
  flex: 1;
  min-height: 0;
  min-width: 0;
  overflow: hidden;
}

/* 详情弹窗 */
.kh-page__detail {
  padding: 8px 0;
}

.kh-page__detail-row {
  display: flex;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid #f0f0f0;
}

.kh-page__detail-row:last-child {
  border-bottom: none;
}

.kh-page__detail-label {
  width: 100px;
  flex-shrink: 0;
  color: #909399;
  font-size: 14px;
}
</style>
