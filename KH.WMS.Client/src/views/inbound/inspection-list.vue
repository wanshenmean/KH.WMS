<template>
  <KhPage
    ref="pageRef"
    module="inspection"
    :search-columns="searchColumns"
    :search-model="searchModel"
    :columns="tableColumns"
    :show-stat-cards="true"
    :show-toolbar="true"
    :show-index="true"
    :show-selection="true"
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
  { prop: 'rcvNo', label: '收货单号', type: 'input', clearable: true },
  { prop: 'materialCode', label: '物料编码', type: 'input', clearable: true },
  {
    prop: 'status',
    label: '验收状态',
    type: 'select',
    clearable: true,
    options: 'dict:inspection_status',
  },
]

const searchModel = reactive({
  rcvNo: '',
  materialCode: '',
  status: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'rcvNo', label: '收货单号', width: 180, fixed: 'left' },
  { prop: 'materialCode', label: '物料编码', width: 140 },
  { prop: 'materialName', label: '物料名称', minWidth: 160 },
  { prop: 'batchNo', label: '批次号', width: 140 },
  { prop: 'inspectQty', label: '送检数量', width: 100, align: 'right' },
  { prop: 'qualifiedQty', label: '合格数量', width: 100, align: 'right' },
  { prop: 'unqualifiedQty', label: '不合格数量', width: 110, align: 'right' },
  { prop: 'inspector', label: '验收人', width: 100 },
  { prop: 'inspectTime', label: '验收时间', width: 170 },
  { prop: 'status', label: '状态', width: 100, type: 'slot' },
]

// ==================== CRUD 配置 ====================
const crudOperations = { create: false, update: false, delete: false, view: true, export: true }

const beforeDelete = (row) => {
  if (row.status !== '待验收') {
    KhMessageFn.warning('仅待验收状态的记录允许删除')
    return false
  }
}

// ==================== 额外操作按钮 ====================
const extraActionButtons = [
  {
    label: '通过',
    type: 'success',
    show: (row) => row.status === '待验收',
    confirm: '确认通过该物料的验收？',
    onClick: (row) => {
      KhMessageFn.success(`已通过: ${row.rcvNo}`)
    },
  },
  {
    label: '驳回',
    type: 'danger',
    show: (row) => row.status === '待验收',
    confirm: '确认驳回该物料的验收？',
    onClick: (row) => {
      KhMessageFn.warning(`已驳回: ${row.rcvNo}`)
    },
  },
  {
    label: '查看明细',
    type: 'primary',
    show: () => true,
    onClick: (row) => {
      KhMsgBoxFn.alert(
        `<div style="line-height: 2">
          <p><b>收货单号：</b>${row.rcvNo}</p>
          <p><b>物料编码：</b>${row.materialCode}</p>
          <p><b>物料名称：</b>${row.materialName}</p>
          <p><b>批次号：</b>${row.batchNo}</p>
          <p><b>验收数量：</b>${row.inspectQty}</p>
          <p><b>合格数量：</b>${row.qualifiedQty}</p>
          <p><b>不合格数量：</b>${row.unqualifiedQty}</p>
          <p><b>验收人：</b>${row.inspector}</p>
          <p><b>验收时间：</b>${row.inspectTime}</p>
        </div>`,
        '验收明细',
        { dangerouslyUseHTMLString: true, customStyle: 'max-width: 500px' }
      )
    },
  },
]
</script>