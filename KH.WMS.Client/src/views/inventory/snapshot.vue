<template>
  <KhPage ref="pageRef" module="inventory-snapshot" title="库存快照" :search-columns="searchColumns" :search-model="searchModel"
    :columns="tableColumns" :form-columns="formColumns" :show-stat-cards="false" :show-toolbar="true"
    :show-index="true" :show-header-filter="true" :search-col-count="3" :crud-operations="crudOperations"
    :permission-prefix="'inv:snapshot'" :before-submit="handleBeforeSubmit" :detail-lines="detailLineConfigs"
    :detail-width="'1100px'" create-label="创建快照" />
</template>

<script setup>
import { createSnapshot } from '@/api/inventory'

const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'snapshotNo', label: '快照编号', type: 'input', clearable: true },
  {
    prop: 'snapshotType',
    label: '快照类型',
    type: 'select',
    clearable: true,
    options: [
      { label: '手动创建', value: 'MANUAL' },
      { label: '每日定时', value: 'DAILY' },
      { label: '盘点触发', value: 'STOCKTAKE' },
    ],
  },
]

const searchModel = reactive({
  snapshotNo: '',
  snapshotType: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'snapshotNo', label: '快照编号', width: 160, fixed: 'left' },
  { prop: 'snapshotName', label: '快照名称', minWidth: 180 },
  {
    prop: 'snapshotType', label: '快照类型', width: 120, type: 'tag',
    tagMap: { 'MANUAL': '手动创建', 'DAILY': '每日定时', 'STOCKTAKE': '盘点触发' },
    tagTypeMap: { 'MANUAL': 'primary', 'DAILY': 'success', 'STOCKTAKE': 'warning' },
  },
  { prop: 'materialCount', label: '物料总数', width: 110, align: 'right' },
  { prop: 'totalStock', label: '库存总量', width: 110, align: 'right' },
  { prop: 'description', label: '说明', minWidth: 150, showOverflowTooltip: true },
  { prop: 'createdTime', label: '创建时间', width: 170 },
]

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: true,
  update: false,
  delete: true,
  view: true,
  export: true,
}

// ==================== 表单配置 ====================
const formColumns = [
  { prop: 'snapshotName', label: '快照名称', type: 'input', required: true, maxlength: 100 },
  { prop: 'description', label: '快照说明', type: 'textarea', maxlength: 500 },
]

// ==================== 详情子表格配置 ====================
const detailLineConfigs = [
  {
    prop: 'details',
    title: '快照明细',
    columns: [
      { prop: 'warehouseId', label: '仓库ID', width: 90, align: 'center' },
      { prop: 'locationId', label: '库位ID', width: 90, align: 'center' },
      { prop: 'materialId', label: '物料ID', width: 90, align: 'center' },
      { prop: 'batchNo', label: '批次号', minWidth: 120, showOverflowTooltip: true },
      { prop: 'containerId', label: '容器ID', width: 90, align: 'center' },
      { prop: 'qty', label: '数量', width: 110, align: 'right' },
    ],
  },
]

// ==================== 拦截表单提交，改调自定义 API ====================
// 注意：KhPage 的 beforeSubmit 是同步调用（无 await），async 函数返回 Promise !== false，拦截不住
const handleBeforeSubmit = (data, mode) => {
  if (mode !== 'create') return true

  createSnapshot({
    snapshotName: data.snapshotName,
    snapshotType: 'MANUAL',
    description: data.description || null,
  }).then(res => {
    if (res.code === 200) {
      KhMessageFn.success(`快照 ${res.data?.snapshotNo} 创建成功`)
      pageRef.value?.reload()
    } else {
      KhMessageFn.error(res.message || '创建失败')
    }
  }).catch(error => {
    KhMessageFn.error('创建失败: ' + error.message)
  })

  return false // 同步返回 false，阻止默认 CRUD 提交
}
</script>
