<template>
  <KhPage
    ref="pageRef"
    module="sort-verify"
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
    :permission-prefix="''"
    :action-buttons="extraActionButtons"
  />
</template>

<script setup>
const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'orderNo', label: '出库单号', type: 'input', placeholder: '请输入出库单号' },
  { prop: 'materialCode', label: '物料编码', type: 'input', placeholder: '请输入物料编码' },
  { prop: 'verifier', label: '复核人', type: 'input', placeholder: '请输入复核人' },
  {
    prop: 'status', label: '状态', type: 'select', placeholder: '请选择状态',
    options: 'dict:sort_status',
  },
  { prop: 'dateRange', label: '复核时间', type: 'date', dateType: 'daterange', startPlaceholder: '开始日期', endPlaceholder: '结束日期' },
]

const searchModel = reactive({
  orderNo: '',
  materialCode: '',
  verifier: '',
  status: '',
  dateRange: [],
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'orderNo', label: '出库单号', width: 160 },
  { prop: 'materialCode', label: '物料编码', width: 140 },
  { prop: 'materialName', label: '物料名称', minWidth: 160 },
  { prop: 'orderQty', label: '订单数量', width: 100, align: 'right' },
  { prop: 'actualQty', label: '实际数量', width: 100, align: 'right' },
  { prop: 'diff', label: '差异', width: 80, align: 'center' },
  { prop: 'verifier', label: '复核人', width: 100 },
  { prop: 'verifyTime', label: '复核时间', width: 170 },
  { prop: 'status', label: '状态', width: 100, tagMap: 'dict:sort_status'},
]

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: false,
  update: false,
  delete: false,
  view: false,
  export: false,
}

// ==================== 额外操作按钮 ====================
const extraActionButtons = [
  {
    label: '批量复核',
    type: 'primary',
    show: () => true,
    isToolbar: true,
    onClick: () => {
      const rows = pageRef.value?.getSelectionRows() || []
      if (!rows.length) {
        KhMessageFn.warning('请先选择需要复核的记录')
        return
      }
      KhMsgBoxFn.confirm(`确认批量复核 ${rows.length} 条分拣记录？`, '批量复核', {
        confirmButtonText: '确认复核',
        cancelButtonText: '取消',
        type: 'warning',
      }).then(() => {
        KhMessageFn.success(`批量复核 ${rows.length} 条记录成功`)
        pageRef.value?.reload()
        pageRef.value?.clearSelection()
      }).catch(() => {})
    },
  },
  {
    label: '异常处理',
    type: 'warning',
    show: () => true,
    isToolbar: true,
    onClick: () => {
      KhMsgBoxFn.alert('请联系质检部门处理异常品，异常报告已自动生成。', '异常处理提示', { type: 'warning' })
    },
  },
  {
    label: '导出',
    type: 'info',
    show: () => true,
    isToolbar: true,
    onClick: () => {
      KhMessageFn.info('分拣复核数据导出中...')
    },
  },
  {
    label: '复核',
    type: 'primary',
    show: () => true,
    confirm: '确认复核出库单【{orderNo}】？',
    onClick: (row) => {
      KhMessageFn.success('复核成功')
    },
  },
  {
    label: '查看明细',
    type: 'info',
    show: () => true,
    onClick: (row) => {
      KhMessageFn.info(`查看出库单 ${row.orderNo} 明细`)
    },
  },
]
</script>
