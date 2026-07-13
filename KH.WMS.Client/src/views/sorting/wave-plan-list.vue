<template>
  <KhPage
    ref="pageRef"
    module="wave-plan"
    :search-columns="searchColumns"
    :search-model="searchModel"
    :columns="tableColumns"
    :show-stat-cards="true"
    :show-toolbar="true"
    :show-index="true"
    :show-header-filter="true"
    :search-col-count="3"
    :crud-operations="crudOperations"
    :permission-prefix="''"
    :delete-show="(row) => row.status === '待执行'"
    :action-buttons="extraActionButtons"
  />
</template>

<script setup>
const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'waveNo', label: '波次号', type: 'input', clearable: true },
  { prop: 'customer', label: '客户', type: 'input', clearable: true },
  {
    prop: 'status',
    label: '状态',
    type: 'select',
    clearable: true,
    options: 'dict:wave_status',
  },
]

const searchModel = reactive({
  waveNo: '',
  customer: '',
  status: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'waveNo', label: '波次号', width: 150, fixed: 'left' },
  { prop: 'waveType', label: '波次类型', width: 120, align: 'center' },
  { prop: 'customer', label: '客户名称', minWidth: 180 },
  { prop: 'orderCount', label: '订单数', width: 90, align: 'right' },
  { prop: 'totalLines', label: '总物料行', width: 100, align: 'right' },
  { prop: 'planTime', label: '计划时间', width: 170 },
  { prop: 'executor', label: '执行人', width: 100 },
  { prop: 'status', label: '状态', width: 100, type: 'slot' },
]

// ==================== CRUD 配置 ====================
const crudOperations = { create: true, update: false, delete: false, view: true, export: true }

// ==================== 额外操作按钮 ====================
const extraActionButtons = [
  {
    label: '执行',
    type: 'warning',
    show: (row) => row.status === '待执行',
    confirm: '确认执行该波次？',
    onClick: (row) => {
      KhMessageFn.success(`已开始执行波次: ${row.waveNo}`)
    },
  },
  {
    label: '查看订单',
    type: 'primary',
    show: () => true,
    onClick: (row) => {
      KhMessageFn.info(`查看波次 ${row.waveNo} 的订单`)
    },
  },
]
</script>