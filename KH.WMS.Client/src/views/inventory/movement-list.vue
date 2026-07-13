<template>
  <KhPage ref="pageRef" module="inventory-movement" :search-columns="searchColumns" :search-model="searchModel"
    :columns="tableColumns" :show-stat-cards="false" :show-toolbar="true" :show-index="true"
    :show-header-filter="true" :search-col-count="3" :crud-operations="crudOperations"
    :permission-prefix="'inv:movement_log'" />
</template>

<script setup>
const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'containerCode', label: '容器编号', type: 'input', clearable: true },
  { prop: 'batchNo', label: '批次号', type: 'input', clearable: true },
  {
    prop: 'movementType',
    label: '变动类型',
    type: 'select',
    clearable: true,
    options: 'dict:movement_type',
  },
  {
    prop: 'direction',
    label: '变动方向',
    type: 'select',
    clearable: true,
    options: [
      { label: '增加', value: 'INCREASE' },
      { label: '减少', value: 'DECREASE' },
      { label: '不变', value: 'UNCHANGED' },
    ],
  },
  { prop: 'docNo', label: '关联单号', type: 'input', clearable: true },
]

const searchModel = reactive({
  containerCode: '',
  batchNo: '',
  movementType: '',
  direction: '',
  docNo: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'containerCode', label: '容器编号', width: 130, fixed: 'left' },
  { prop: 'batchNo', label: '批次号', width: 130, showOverflowTooltip: true },
  { prop: 'serialNo', label: '序列号', width: 130, showOverflowTooltip: true },
  {
    prop: 'movementType', label: '变动类型', width: 100, type: 'tag',
    tagMap: 'dict:movement_type',
  },
  {
    prop: 'direction', label: '变动方向', width: 100, type: 'tag', align: 'center',
    tagMap: { 'INCREASE': '增加', 'DECREASE': '减少', 'UNCHANGED': '不变' },
    tagTypeMap: { 'INCREASE': 'success', 'DECREASE': 'danger', 'UNCHANGED': 'info' },
  },
  { prop: 'movementQty', label: '变动数量', width: 100, align: 'right' },
  { prop: 'unit', label: '单位', width: 80, align: 'center' },
  { prop: 'qtyBefore', label: '变动前', width: 90, align: 'right' },
  { prop: 'qtyAfter', label: '变动后', width: 90, align: 'right' },
  { prop: 'docType', label: '单据类型', width: 100 },
  { prop: 'docNo', label: '关联单号', width: 160, showOverflowTooltip: true },
  { prop: 'operatorName', label: '操作人', width: 100 },
  { prop: 'movementTime', label: '变动时间', width: 170 },
]

// ==================== CRUD 配置（只读） ====================
const crudOperations = {
  create: false,
  update: false,
  delete: false,
  view: true,
  export: true,
}
</script>
