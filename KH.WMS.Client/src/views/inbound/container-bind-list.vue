<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage
      ref="pageRef"
      title="组盘管理"
      module="inbound-container-bind"
      :stat-cards="statCards"
      :search-columns="searchColumns"
      :search-model="searchModel"
      :columns="tableColumns"
      :show-stat-cards="true"
      :show-search="true"
      :show-toolbar="true"
      :show-index="true"
      :show-header-filter="true"
      :search-col-count="3"
      :stat-span="6"
      :crud-operations="crudOperations"
      :permission-prefix="'con:bind'"
      :action-buttons="actionButtons"
      :detail-lines="detailLineConfigs"
      :detail-width="'1000px'"
      :toolbar-buttons="toolbarButtons"
      @stat-click="handleStatClick"
    >
    </KhPage>
  </div>
</template>

<script setup>
import { cancelContainerBind, putawayContainerBind } from '@/api/inbound'

const pageRef = ref(null)

// ==================== 统计卡片 ====================
const statCards = ref([
  { label: '已绑定', value: 0, icon: markRaw(Box), theme: 'warning', clickable: true },
  { label: '已上架', value: 0, icon: markRaw(CircleCheck), theme: 'success', clickable: true },
  { label: '已取消', value: 0, icon: markRaw(Clock), theme: 'info', clickable: true },
])

const handleStatClick = (card) => {
  if (card.label === '已绑定') {
    searchModel.bindStatus = 'BOUND'
  } else if (card.label === '已上架') {
    searchModel.bindStatus = 'PUT_AWAY'
  } else if (card.label === '已取消') {
    searchModel.bindStatus = 'CANCELLED'
  }
  pageRef.value?.reload()
}

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'containerCode', label: '容器编号', type: 'input', clearable: true },
  { prop: 'sourceDocNo', label: '来源单号', type: 'input', clearable: true },
  {
    prop: 'bindStatus', label: '组盘状态', type: 'select', clearable: true,
    options: 'dict:bind_status',
  },
  {
    prop: 'qualityStatus', label: '质量状态', type: 'select', clearable: true,
    options: 'dict:quality_status',
  },
]

const searchModel = reactive({
  containerCode: '',
  sourceDocNo: '',
  bindStatus: '',
})

// ==================== CRUD 配置 ====================
const crudOperations = { create: false, update: false, delete: false, view: true, export: true }

// ==================== 操作列按钮 ====================
const handleCancel = async (row) => {
  try {
    const res = await cancelContainerBind(row.id)
    if (res.data?.success !== false) {
      KhMessageFn.success(res.data?.message || res.message || '取消组盘成功')
      pageRef.value?.reload()
    }
  } catch {
    // request.js 已处理错误提示
  }
}

// ==================== 请求上架 ====================
const handlePutAway = async () => {
  const rows = pageRef.value?.getSelectionRows() || []
  if (rows.length === 0) {
    KhMessageFn.warning('请选择要上架的组盘记录')
    return
  }
  // 校验选中行的状态
  const invalidRows = rows.filter(r => r.bindStatus !== 'BOUND')
  if (invalidRows.length > 0) {
    KhMessageFn.warning('仅"已绑定"状态的记录可以请求上架')
    return
  }
  const ids = rows.map(r => r.id)
  try {
    const res = await putawayContainerBind(ids)
    if (res.data?.success !== false) {
      KhMessageFn.success(res.data?.message || res.message || '请求上架成功')
      pageRef.value?.reload()
    }
  } catch {
    // request.js 已处理错误提示
  }
}

const toolbarButtons = [
  {
    label: '请求上架',
    type: 'primary',
    icon: markRaw(Upload),
    permission: 'con:bind:putaway',
    onClick: handlePutAway,
  },
]

const actionButtons = [
  {
    label: '取消组盘',
    type: 'danger',
    permission: 'con:bind:delete',
    onClick: (row) => handleCancel(row),
    show: (row) => row.bindStatus === 'BOUND',
  },
]

// ==================== 表格列（头表字段） ====================
const tableColumns = [
  { prop: 'containerCode', label: '容器编号', width: 130, fixed: 'left' },
  {
    prop: 'bindStatus', label: '组盘状态', width: 120, align: 'center',
    type: 'tag', tagMap: 'dict:bind_status',
  },
  {
    prop: 'qualityStatus', label: '质量状态', width: 120, align: 'center',
    type: 'tag', tagMap: 'dict:quality_status',
  },
  {
    prop: 'sourceType', label: '来源类型', width: 120, align: 'center',
    type: 'tag', tagMap: 'dict:source_type',
  },
  { prop: 'sourceDocNo', label: '来源单号', minWidth: 150, showOverflowTooltip: true },
  { prop: 'detailCount', label: '明细数', width: 80, align: 'center' },
  { prop: 'warehouseId', label: '仓库', width: 100 },
  { prop: 'bindTime', label: '组盘时间', width: 170, align: 'center' },
  { prop: 'remark', label: '备注', minWidth: 120, showOverflowTooltip: true },
]

// ==================== 详情弹窗从表配置 ====================
const detailLineConfigs = [
  {
    prop: 'details',
    title: '组盘明细',
    columns: [
      { prop: 'materialCode', label: '物料编码', minWidth: 130 },
      { prop: 'materialName', label: '物料名称', minWidth: 160, showOverflowTooltip: true },
      { prop: 'qty', label: '数量', width: 100, align: 'right' },
      { prop: 'batchNo', label: '批次号', width: 120 },
      { prop: 'productionDate', label: '生产日期', width: 110, align: 'center' },
      { prop: 'expiryDate', label: '有效期', width: 110, align: 'center' },
      { prop: 'remark', label: '备注', minWidth: 120 },
    ],
  },
]
</script>
