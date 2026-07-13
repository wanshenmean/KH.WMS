<template>
  <KhPage
    ref="pageRef"
    module="putaway"
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
    :delete-show="(row) => row.status === '待上架'"
    :action-buttons="extraActionButtons"
  />
</template>

<script setup>
const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'taskNo', label: '任务编号', type: 'input', clearable: true },
  { prop: 'materialCode', label: '物料编码', type: 'input', clearable: true },
  {
    prop: 'status',
    label: '状态',
    type: 'select',
    clearable: true,
    options: 'dict:putaway_status',
  },
]

const searchModel = reactive({
  taskNo: '',
  materialCode: '',
  status: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'taskNo', label: '任务编号', width: 160, fixed: 'left' },
  { prop: 'rcvNo', label: '收货单号', width: 180 },
  { prop: 'materialCode', label: '物料编码', width: 140 },
  { prop: 'materialName', label: '物料名称', minWidth: 150 },
  { prop: 'quantity', label: '数量', width: 90, align: 'right' },
  { prop: 'targetLocation', label: '目标库位', width: 130 },
  { prop: 'asrsStatus', label: 'ASRS状态', width: 100, type: 'slot' },
  { prop: 'operator', label: '操作人', width: 100 },
  { prop: 'createTime', label: '创建时间', width: 170 },
  { prop: 'status', label: '状态', width: 100, type: 'slot' },
]

// ==================== CRUD 配置 ====================
const crudOperations = { create: false, update: false, delete: false, view: true, export: true }

// ==================== 额外操作按钮 ====================
const extraActionButtons = [
  {
    label: '分配库位',
    type: 'primary',
    show: (row) => row.status === '待上架',
    onClick: (row) => {
      KhMessageFn.info(`分配库位: ${row.taskNo}`)
    },
  },
  {
    label: '触发ASRS',
    type: 'success',
    show: (row) => row.asrsStatus !== '已完成' && row.status !== '已完成',
    confirm: '确认触发ASRS自动上架？',
    onClick: (row) => {
      KhMessageFn.success(`ASRS任务已下发: ${row.taskNo}`)
    },
  },
]
</script>