<template>
  <KhPage
    ref="pageRef"
    module="inventory-freeze"
    :search-columns="searchColumns"
    :search-model="searchModel"
    :columns="tableColumns"
    :show-stat-cards="false"
    :show-toolbar="true"
    :show-index="true"
    :show-header-filter="true"
    :search-col-count="3"
    :crud-operations="crudOperations"
    :permission-prefix="'inv:freeze'"
  />
</template>

<script setup>

const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'containerCode', label: '容器编号', type: 'input', clearable: true },
  { prop: 'freezeReason', label: '冻结原因', type: 'input', clearable: true },
  {
    prop: 'status',
    label: '状态',
    type: 'select',
    clearable: true,
    options: 'dict:freeze_status',
  },
]

const searchModel = reactive({
  containerCode: '',
  freezeReason: '',
  status: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'freezeNo', label: '冻结单号', width: 160, fixed: 'left' },
  { prop: 'containerCode', label: '容器编号', width: 140 },
  { prop: 'locationCode', label: '库位编码', width: 140 },
  { prop: 'freezeQty', label: '冻结数量', width: 100, align: 'right' },
  { prop: 'freezeReason', label: '冻结原因', minWidth: 200, showOverflowTooltip: true },
  { prop: 'createdByName', label: '操作人', width: 100 },
  { prop: 'freezeTime', label: '冻结时间', width: 170 },
  { prop: 'unfreezeTime', label: '解冻时间', width: 170 },
  { prop: 'status', label: '状态', width: 100, type: 'tag', tagMap: 'dict:freeze_status' },
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
