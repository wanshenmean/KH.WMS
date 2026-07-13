<template>
  <KhPage
    ref="pageRef"
    module="task-confirm"
    :search-columns="searchColumns"
    :search-model="searchModel"
    :columns="tableColumns"
    :form-columns="formColumns"
    :show-stat-cards="false"
    :show-toolbar="true"
    :show-index="true"
    :show-selection="true"
    :show-header-filter="true"
    :search-col-count="3"
    :crud-operations="crudOperations"
    :permission-prefix="'task:confirm'"
    :action-buttons="extraActionButtons"
  />
</template>

<script setup>
const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'taskNo', label: '任务号', type: 'input', clearable: true, placeholder: '请输入任务号' },
  {
    prop: 'taskType',
    label: '任务类型',
    type: 'select',
    clearable: true,
    options: 'dict:task_type',
  },
  {
    prop: 'status',
    label: '状态',
    type: 'select',
    clearable: true,
    options: 'dict:confirm_status',
  },
]

const searchModel = reactive({
  taskNo: '',
  taskType: '',
  status: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'taskNo', label: '任务号', width: 170, fixed: 'left' },
  {
    prop: 'taskType', label: '任务类型', width: 90, align: 'center',
    type: 'tag', tagMap: 'dict:task_type',
  },
  { prop: 'sourceLocation', label: '源位置', width: 130 },
  { prop: 'targetLocation', label: '目标位置', width: 130 },
  { prop: 'materialCode', label: '物料编码', width: 130 },
  { prop: 'materialName', label: '物料名称', minWidth: 180 },
  { prop: 'qty', label: '数量', width: 80, align: 'right' },
  { prop: 'executor', label: '执行人', width: 90 },
  {
    prop: 'status', label: '状态', width: 90, align: 'center',
    type: 'tag', tagMap: 'dict:confirm_status',
  },
]

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: false,
  update: false,
  delete: false,
  view: true,
  export: false,
}

// ==================== 表单配置（新增/编辑弹窗） ====================
const formColumns = [
  { prop: 'taskNo', label: '任务号', type: 'input', disabled: true },
  { prop: 'confirmQty', label: '确认数量', type: 'number', required: true, min: 0 },
  { prop: 'remark', label: '确认备注', type: 'textarea', maxlength: 200, rows: 3 },
]

// ==================== 额外操作按钮 ====================
const extraActionButtons = [
  {
    label: '确认',
    type: 'primary',
    permission: 'task:confirm:confirm',
    show: (row) => row.status === '待确认',
    onClick: (row) => {
      KhMessageFn.success(`任务 ${row.taskNo} 已确认，确认数量: ${row.qty}`)
      pageRef.value?.reload()
    },
  },
]
</script>
