<template>
  <KhPage
    ref="pageRef"
    module="container"
    title="容器管理"
    :search-columns="searchColumns"
    :search-model="searchModel"
    :columns="tableColumns"
    :form-columns="formColumns"
    :show-stat-cards="false"
    :show-toolbar="true"
    :show-index="true"
    :show-selection="true"
    :show-header-filter="true"
    :search-col-count="3"
    :crud-operations="crudOperations"
    :permission-prefix="'bd:container'"
  />
</template>

<script setup>

const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'containerNo', label: '容器编号', type: 'input', clearable: true, placeholder: '请输入容器编号' },
  { prop: 'containerTypeId', label: '容器类型', type: 'select', clearable: true, options: 'dict:container_type' },
  {
    prop: 'status', label: '状态', type: 'select', clearable: true,
    options: 'dict:status_flag',
  },
]

const searchModel = reactive({ containerNo: '', containerTypeId: '', status: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'containerNo', label: '容器编号', width: 140 },
  { prop: 'containerTypeId', label: '容器类型', width: 140, type: 'tag', tagMap: 'dict:container_type' },
  { prop: 'warehouseId', label: '所在仓库', width: 140 },
  { prop: 'currentLocationId', label: '当前库位', minWidth: 150, showOverflowTooltip: true },
  {
    prop: 'status', label: '状态', width: 90, align: 'center',
    type: 'tag', tagMap: 'dict:status_flag',
  },
]

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: false,
  export: true,
}

// ==================== 表单配置（新增/编辑弹窗） ====================
const formColumns = [
  { prop: 'containerNo', label: '容器编号', type: 'input', required: true, placeholder: '请输入容器编号' },
  { prop: 'containerTypeId', label: '容器类型', type: 'select', required: true, options: 'dict:container_type', placeholder: '请选择容器类型' },
  { prop: 'warehouseId', label: '所在仓库', type: 'select', options: 'dict:warehouse', placeholder: '请选择仓库' },
  { prop: 'status', label: '状态', type: 'switch', activeValue: 1, inactiveValue: 0 },
]
</script>
