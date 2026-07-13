<template>
  <KhPage
    ref="pageRef"
    module="shipping-list"
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
  { prop: 'customerName', label: '客户名称', type: 'input', placeholder: '请输入客户名称' },
  { prop: 'logisticsNo', label: '物流单号', type: 'input', placeholder: '请输入物流单号' },
  {
    prop: 'logisticsCompany', label: '物流公司', type: 'select', placeholder: '请选择物流公司',
    options: [
      { label: '顺丰速运', value: '顺丰速运' },
      { label: '京东物流', value: '京东物流' },
      { label: '中通快递', value: '中通快递' },
      { label: '圆通速递', value: '圆通速递' },
      { label: '韵达快递', value: '韵达快递' },
      { label: '德邦物流', value: '德邦物流' },
    ],
  },
  {
    prop: 'status', label: '状态', type: 'select', placeholder: '请选择状态',
    options: 'dict:shipping_status',
  },
]

const searchModel = reactive({
  orderNo: '',
  customerName: '',
  logisticsNo: '',
  logisticsCompany: '',
  status: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'orderNo', label: '出库单号', width: 160 },
  { prop: 'customerName', label: '客户名称', minWidth: 200 },
  { prop: 'logisticsCompany', label: '物流公司', width: 110 },
  { prop: 'logisticsNo', label: '物流单号', width: 180 },
  { prop: 'receiver', label: '收货人', width: 100 },
  { prop: 'receiverPhone', label: '收货电话', width: 130 },
  { prop: 'address', label: '收货地址', minWidth: 220 },
  { prop: 'shipper', label: '发货人', width: 100 },
  { prop: 'shipTime', label: '发货时间', width: 170 },
  { prop: 'status', label: '状态', width: 100, tagMap: 'dict:shipping_status'},
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
    label: '批量发货',
    type: 'primary',
    show: () => true,
    isToolbar: true,
    onClick: () => {
      const rows = pageRef.value?.getSelectionRows() || []
      if (!rows.length) {
        KhMessageFn.warning('请先选择需要发货的记录')
        return
      }
      KhMsgBoxFn.confirm(`确认批量发货 ${rows.length} 条记录？`, '批量发货', {
        confirmButtonText: '确认发货',
        cancelButtonText: '取消',
        type: 'warning',
      }).then(() => {
        KhMessageFn.success(`批量发货 ${rows.length} 条记录成功`)
        pageRef.value?.reload()
        pageRef.value?.clearSelection()
      }).catch(() => {})
    },
  },
  {
    label: '打印面单',
    type: 'success',
    show: () => true,
    isToolbar: true,
    onClick: () => {
      KhMessageFn.info('面单打印功能开发中')
    },
  },
  {
    label: '导出',
    type: 'info',
    show: () => true,
    isToolbar: true,
    onClick: () => {
      KhMessageFn.info('发货数据导出中...')
    },
  },
  {
    label: '发货',
    type: 'primary',
    show: (row) => row.status === '待发货',
    onClick: (row) => {
      KhMessageFn.info(`发货: ${row.orderNo}`)
    },
  },
  {
    label: '查看物流',
    type: 'info',
    show: () => true,
    onClick: (row) => {
      KhMessageFn.info(`查看物流: ${row.orderNo}`)
    },
  },
  {
    label: '确认签收',
    type: 'success',
    show: (row) => row.status === '已发货',
    confirm: '确认签收出库单【{orderNo}】？',
    onClick: (row) => {
      KhMessageFn.success('签收确认成功')
    },
  },
]
</script>
