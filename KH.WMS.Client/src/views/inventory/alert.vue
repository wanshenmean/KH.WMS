<template>
  <KhPage
    ref="pageRef"
    module="inventory-alert-record"
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
    :permission-prefix="'inv:alert'"
    :before-delete="beforeDelete"
    :action-buttons="extraActionButtons"
  />
</template>

<script setup>
const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'materialCode', label: '物料编码', type: 'input', clearable: true },
  {
    prop: 'alertType',
    label: '预警类型',
    type: 'select',
    clearable: true,
    options: 'dict:alert_type',
  },
  {
    prop: 'processStatus',
    label: '处理状态',
    type: 'select',
    clearable: true,
    options: [
      { label: '未处理', value: '未处理' },
      { label: '已处理', value: '已处理' },
      { label: '已忽略', value: '已忽略' },
    ],
  },
]

const searchModel = reactive({
  materialCode: '',
  alertType: '',
  processStatus: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'materialCode', label: '物料编码', width: 140, fixed: 'left' },
  { prop: 'materialName', label: '物料名称', minWidth: 160 },
  { prop: 'currentStock', label: '当前库存', width: 110, align: 'right' },
  { prop: 'threshold', label: '预警阈值', width: 110, align: 'right' },
  { prop: 'alertType', label: '预警类型', width: 110, type: 'tag', tagMap: 'dict:alert_type'},
  { prop: 'alertTime', label: '预警时间', width: 170 },
  { prop: 'processStatus', label: '处理状态', width: 100, type: 'tag', tagMap: { '未处理': '未处理', '已处理': '已处理', '已忽略': '已忽略' }, tagTypeMap: { '未处理': 'danger', '已处理': 'success', '已忽略': 'info' } },
]

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: false,
  update: false,
  delete: false,
  view: true,
  export: true,
}

// ==================== 表单配置（新增/编辑弹窗） ====================
const formColumns = []

// ==================== 额外操作按钮 ====================
const extraActionButtons = [
  {
    label: '处理',
    type: 'warning',
    permission: 'inv:alert:process',
    show: (row) => row.processStatus === '未处理',
    onClick: (row) => {
      KhMessageFn.success(`预警已处理: ${row.materialCode}`)
      pageRef.value?.reload()
    },
  },
]
</script>