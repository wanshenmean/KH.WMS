<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage
      ref="pageRef"
      module="document-status"
      title="单据状态配置"
      :search-columns="searchColumns"
      :search-model="searchModel"
      :columns="tableColumns"
      :show-stat-cards="false"
      :show-toolbar="true"
      :show-index="true"
      :show-selection="true"
      :show-header-filter="true"
      :search-col-count="3"
      :crud-operations="crudOperations"
      :permission-prefix="'cfg:doc_status'"
      :form-columns="formColumns"
      :custom-form-data="formData"
      :before-submit="handleBeforeSubmit"
      :before-update="handleBeforeUpdate"
      :before-create="handleBeforeCreate"
      :action-buttons="extraActionButtons"
    />
  </div>
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('document-status')
const docTypeCrudApi = useCrudApi('document-type')

// ==================== 单据类型选项 ====================
const docTypeOptions = ref([])

async function loadDocTypeOptions() {
  try {
    const res = await docTypeCrudApi.pageList({ pageNum: 1, pageSize: 999, isActive: 1 })
    const list = res.data?.items || res.data || res.rows || []
    const activeList = list.filter(d => d.isActive === 1)
    const options = activeList.map(d => ({ label: d.typeName, value: d.id }))
    // 更新搜索和表单中单据类型下拉选项
    const searchCol = searchColumns.find(c => c.prop === 'docTypeId')
    if (searchCol) searchCol.options = options
    const formCol = formColumns.find(c => c.prop === 'docTypeId')
    if (formCol) formCol.options = options
  } catch {
    // 加载失败时保持空列表
  }
}

// ==================== 可流转状态选项（根据 docTypeId 动态加载） ====================
// 说明：可流转状态选项的加载由「单据类型」列的 onChange 回调驱动。
// KhPage/KhDialog/KhForm 内部对表单数据都做了浅拷贝，弹窗内选择单据类型时
// 修改的是 KhForm 内部模型，外部 watch(formData.docTypeId) 无法感知，故必须用列级 onChange。
// formColumns 为 reactive，直接修改列的 options 即可触发 KhForm 重新渲染下拉项。
function getNextStatusColumn() {
  return formColumns.find(c => c.prop === 'nextStatuses')
}

// 异步加载序号：用于丢弃过期请求结果（切换类型 / 先编辑后新增时，旧请求晚返回不应覆盖新状态）
let nextStatusLoadSeq = 0

// excludeStatusCode：编辑时排除自身状态，避免状态流转到自身（新增时为 undefined，不排除）
async function loadNextStatusOptions(docTypeId, excludeStatusCode) {
  const col = getNextStatusColumn()
  const seq = ++nextStatusLoadSeq
  if (!docTypeId) {
    if (col) col.options = []
    return
  }
  try {
    const res = await crudApi.pageList({ pageNum: 1, pageSize: 999, docTypeId, isActive: 1 })
    if (seq !== nextStatusLoadSeq) return // 已被后续加载取代，丢弃过期结果
    const list = res.data?.items || res.data || res.rows || []
    const options = list
      .filter(d => d.isActive === 1 && d.statusCode !== excludeStatusCode)
      .map(d => ({ label: `${d.statusCode}（${d.statusName}）`, value: d.statusCode }))
    if (seq !== nextStatusLoadSeq) return
    if (col) col.options = options
  } catch {
    if (seq === nextStatusLoadSeq && col) col.options = []
  }
}

// ==================== 状态码→名称映射（表格「可流转状态」列显示中文） ====================
// 按单据类型分组，避免不同单据类型下相同状态码（如 DRAFT）名称相互覆盖
const statusCodeNameMap = ref({}) // { [docTypeId]: { [statusCode]: statusName } }

async function loadStatusCodeNameMap() {
  try {
    const res = await crudApi.pageList({ pageNum: 1, pageSize: 999 })
    const list = res.data?.items || res.data || res.rows || []
    const map = {}
    list.forEach((d) => {
      if (!map[d.docTypeId]) map[d.docTypeId] = {}
      map[d.docTypeId][d.statusCode] = d.statusName
    })
    statusCodeNameMap.value = map
  } catch {
    statusCodeNameMap.value = {}
  }
}

// 表格列 formatter：将 nextStatuses（JSON 字符串/数组）转为中文状态名，用「、」连接
function formatNextStatuses(row) {
  let codes = row.nextStatuses
  if (typeof codes === 'string') {
    try { codes = JSON.parse(codes) } catch { codes = [] }
  }
  if (!Array.isArray(codes) || codes.length === 0) return '-'
  const typeMap = statusCodeNameMap.value[row.docTypeId] || {}
  return codes.map((c) => typeMap[c] || c).join('、')
}

onMounted(() => {
  loadDocTypeOptions()
  loadStatusCodeNameMap()
})

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'docTypeId', label: '单据类型', type: 'select', clearable: true, options: [] },
  { prop: 'statusCode', label: '状态编码', type: 'input', clearable: true },
  { prop: 'statusName', label: '状态名称', type: 'input', clearable: true },
]

const searchModel = reactive({ docTypeId: '', statusCode: '', statusName: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'docTypeId', label: '单据类型', width: 120, type: 'tag', tagMap: 'dict:doc_type_list' },
  { prop: 'statusCode', label: '状态编码', width: 120 },
  { prop: 'statusName', label: '状态名称', width: 120 },
  {
    prop: 'statusCategory', label: '状态分类', width: 100, align: 'center',
    type: 'tag',
    tagMap: { INITIAL: '初始态', PROCESS: '流程中', TERMINAL: '终态', CANCEL: '取消' },
    tagTypeMap: { INITIAL: 'primary', PROCESS: '', TERMINAL: 'success', CANCEL: 'info' },
  },
  {
    prop: 'isInitial', label: '初始状态', width: 80, align: 'center',
    type: 'tag', tagMap: { 1: '是', 0: '否' }, tagTypeMap: { 1: 'success', 0: 'info' },
  },
  {
    prop: 'isFinal', label: '终态', width: 70, align: 'center',
    type: 'tag', tagMap: { 1: '是', 0: '否' }, tagTypeMap: { 1: 'warning', 0: 'info' },
  },
  {
    prop: 'isCancel', label: '取消', width: 70, align: 'center',
    type: 'tag', tagMap: { 1: '是', 0: '否' }, tagTypeMap: { 1: 'danger', 0: 'info' },
  },
  { prop: 'allowEdit', label: '可编辑', width: 70, align: 'center', type: 'tag', tagMap: { 1: '是', 0: '否' }, tagTypeMap: { 1: 'success', 0: 'info' } },
  { prop: 'allowDelete', label: '可删除', width: 70, align: 'center', type: 'tag', tagMap: { 1: '是', 0: '否' }, tagTypeMap: { 1: 'success', 0: 'info' } },
  {
    prop: 'nextStatuses', label: '可流转状态', minWidth: 200, showOverflowTooltip: true,
    formatter: (row) => formatNextStatuses(row),
  },
  { prop: 'sortNo', label: '排序', width: 70, align: 'center' },
  { prop: 'color', label: '颜色', width: 70, align: 'center' },
  {
    prop: 'isActive', label: '启用', width: 70, align: 'center',
    type: 'tag', tagMap: { 0: '禁用', 1: '启用' }, tagTypeMap: { 1: 'success', 0: 'danger' },
  },
]

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: true,
  export: false,
}

// ==================== 表单配置 ====================
const formColumns = reactive([
  {
    prop: 'docTypeId', label: '单据类型', type: 'select', required: true,
    options: [],
    // 切换单据类型时：1) 重新加载该类型下的可流转状态选项；2) 清空可能不再适用的已选状态码。
    // 回调第二参数 fd 为 KhForm 内部表单模型，可直接修改以联动清空 nextStatuses。
    onChange: (val, fd) => {
      loadNextStatusOptions(val, fd?.statusCode)
      if (fd && Array.isArray(fd.nextStatuses) && fd.nextStatuses.length) fd.nextStatuses = []
    },
  },
  { prop: 'statusCode', label: '状态编码', type: 'input', required: true, maxlength: 30 },
  { prop: 'statusName', label: '状态名称', type: 'input', required: true, maxlength: 100 },
  { prop: 'isInitial', label: '是否初始状态', type: 'switch', defaultValue: 0, activeValue: 1, inactiveValue: 0 },
  { prop: 'isFinal', label: '是否终态', type: 'switch', defaultValue: 0, activeValue: 1, inactiveValue: 0 },
  { prop: 'isCancel', label: '是否取消状态', type: 'switch', defaultValue: 0, activeValue: 1, inactiveValue: 0 },
  { prop: 'allowEdit', label: '允许编辑', type: 'switch', defaultValue: 1, activeValue: 1, inactiveValue: 0 },
  { prop: 'allowDelete', label: '允许删除', type: 'switch', defaultValue: 1, activeValue: 1, inactiveValue: 0 },
  {
    prop: 'nextStatuses', label: '可流转状态', type: 'select', multiple: true, filterable: true,
    clearable: true, span: 24, placeholder: '请先选择单据类型，再配置可流转状态',
    options: [],
  },
  { prop: 'color', label: '颜色', type: 'input', maxlength: 10, placeholder: '如 #409EFF' },
  { prop: 'sortNo', label: '排序', type: 'number', min: 0 },
  { prop: 'isActive', label: '是否启用', type: 'switch', defaultValue: 1, activeValue: 1, inactiveValue: 0 },
  { prop: 'description', label: '描述', type: 'textarea', span: 24, maxlength: 500 },
])

const formData = reactive({
  docTypeId: null,
  statusCode: '',
  statusName: '',
  isInitial: 0,
  isFinal: 0,
  isCancel: 0,
  allowEdit: 1,
  allowDelete: 1,
  nextStatuses: [],
  color: '',
  sortNo: 0,
  isActive: 1,
  description: '',
})

// ==================== 数据转换 ====================

// 新增前：清空可流转状态选项，避免残留上次编辑/查看的单据类型选项
// （loadNextStatusOptions(null) 走清空分支，同步执行，无需 await）
function handleBeforeCreate() {
  loadNextStatusOptions(null)
}

// 编辑前：将 nextStatuses 从 JSON 字符串转为数组
function handleBeforeUpdate(row) {
  if (typeof row.nextStatuses === 'string' && row.nextStatuses) {
    try {
      row.nextStatuses = JSON.parse(row.nextStatuses)
    } catch {
      row.nextStatuses = []
    }
  } else if (!row.nextStatuses) {
    row.nextStatuses = []
  }
  // 编辑时加载该单据类型的可流转状态选项（排除自身状态，避免流转到自身）
  if (row.docTypeId) {
    loadNextStatusOptions(row.docTypeId, row.statusCode)
  }
}

// 提交前：将 nextStatuses 从数组转为 JSON 字符串
function handleBeforeSubmit(data) {
  if (Array.isArray(data.nextStatuses)) {
    data.nextStatuses = data.nextStatuses.length > 0
      ? JSON.stringify(data.nextStatuses)
      : null
  }
}

// ==================== 额外操作按钮 ====================
const extraActionButtons = [
  {
    label: (row) => row.isActive === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'cfg:doc_status:toggle',
    confirm: (row) => `确定要${row.isActive === 1 ? '禁用' : '启用'}吗？`,
    onClick: async (row) => {
      const currentStatus = row.isActive
      const newStatus = currentStatus === 1 ? 0 : 1
      const res = await crudApi.setStatus(row.id, newStatus)
      if (res.code === 200) {
        KhMessageFn.success(res.message)
        pageRef.value?.reload()
      }
    },
  },
]
</script>
