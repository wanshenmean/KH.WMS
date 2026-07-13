<template>
  <KhPage
    ref="pageRef"
    module="dispatch"
    :search-columns="searchColumns"
    :search-model="searchModel"
    :columns="tableColumns"
    :show-stat-cards="true"
    :show-toolbar="true"
    :show-index="true"
    :show-header-filter="true"
    :search-col-count="4"
    :crud-operations="crudOperations"
    :permission-prefix="''"
    :before-delete="beforeDelete"
    :action-buttons="extraActionButtons"
  />
</template>

<script setup>
const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'dispatchNo', label: '分拣单号', type: 'input', clearable: true },
  { prop: 'waveNo', label: '波次号', type: 'input', clearable: true },
  { prop: 'dispatcher', label: '分拣员', type: 'input', clearable: true },
  {
    prop: 'status',
    label: '状态',
    type: 'select',
    clearable: true,
    options: 'dict:dispatch_status',
  },
]

const searchModel = reactive({
  dispatchNo: '',
  waveNo: '',
  dispatcher: '',
  status: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'dispatchNo', label: '分拣单号', width: 150, fixed: 'left' },
  { prop: 'waveNo', label: '波次号', width: 140 },
  { prop: 'outboundNo', label: '出库单号', width: 150 },
  { prop: 'materialCode', label: '物料编码', width: 140 },
  { prop: 'materialName', label: '物料名称', minWidth: 160 },
  { prop: 'dispatchQty', label: '分拣数量', width: 100, align: 'right' },
  { prop: 'targetChannel', label: '目标通道', width: 110, align: 'center' },
  { prop: 'dispatcher', label: '分拣员', width: 100 },
  { prop: 'status', label: '状态', width: 100, type: 'slot' },
  { prop: 'createTime', label: '创建时间', width: 170 },
]

// ==================== CRUD 配置 ====================
const crudOperations = { create: false, update: false, delete: false, view: true, export: true }

const beforeDelete = (row) => {
  if (row.status === '已完成') {
    KhMessageFn.warning('已完成的分拣单不允许删除')
    return false
  }
}

// ==================== 额外操作按钮 ====================
const extraActionButtons = [
  {
    label: '开始分拣',
    type: 'primary',
    show: (row) => row.status === '待分拣',
    confirm: '确认开始分拣？',
    onClick: (row) => {
      KhMessageFn.success(`已开始分拣: ${row.dispatchNo}`)
    },
  },
  {
    label: '完成',
    type: 'success',
    show: (row) => row.status === '分拣中',
    confirm: '确认完成该分拣作业？',
    onClick: (row) => {
      KhMessageFn.success(`分拣已完成: ${row.dispatchNo}`)
    },
  },
]
</script>