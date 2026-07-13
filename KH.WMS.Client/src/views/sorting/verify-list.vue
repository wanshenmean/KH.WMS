<template>
  <KhPage
    ref="pageRef"
    module="verify"
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
    :before-delete="beforeDelete"
    :action-buttons="extraActionButtons"
  />
</template>

<script setup>
const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'dispatchNo', label: '分拣单号', type: 'input', clearable: true },
  { prop: 'materialCode', label: '物料编码', type: 'input', clearable: true },
  {
    prop: 'status',
    label: '验证状态',
    type: 'select',
    clearable: true,
    options: 'dict:verify_status',
  },
]

const searchModel = reactive({
  dispatchNo: '',
  materialCode: '',
  status: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'dispatchNo', label: '分拣单号', width: 150, fixed: 'left' },
  { prop: 'materialCode', label: '物料编码', width: 140 },
  { prop: 'materialName', label: '物料名称', minWidth: 160 },
  { prop: 'dispatchQty', label: '分拣数量', width: 100, align: 'right' },
  { prop: 'verifyQty', label: '验证数量', width: 100, align: 'right' },
  { prop: 'diff', label: '差异', width: 90, align: 'center', type: 'slot' },
  { prop: 'verifier', label: '验证人', width: 100 },
  { prop: 'verifyTime', label: '验证时间', width: 170 },
  { prop: 'status', label: '状态', width: 100, type: 'slot' },
]

// ==================== CRUD 配置 ====================
const crudOperations = { create: false, update: false, delete: false, view: true, export: true }

const beforeDelete = (row) => {
  if (row.status !== '待验证') {
    KhMessageFn.warning('仅待验证状态的记录允许删除')
    return false
  }
}

// ==================== 额外操作按钮 ====================
const extraActionButtons = [
  {
    label: '验证通过',
    type: 'success',
    show: (row) => row.status === '待验证',
    confirm: '确认验证通过该分拣单？',
    onClick: (row) => {
      KhMessageFn.success(`验证通过: ${row.dispatchNo}`)
    },
  },
  {
    label: '标记异常',
    type: 'danger',
    show: (row) => row.status === '待验证',
    confirm: '确认标记该分拣单为异常？将触发异常处理流程。',
    onClick: (row) => {
      KhMessageFn.warning(`已标记异常: ${row.dispatchNo}`)
    },
  },
]
</script>