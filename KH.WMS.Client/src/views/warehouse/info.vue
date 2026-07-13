<template>
  <KhPage ref="pageRef" module="warehouse" title="仓库管理" :search-columns="searchColumns" :search-model="searchModel"
    :columns="tableColumns" :form-columns="formColumns" :show-stat-cards="false" :show-toolbar="true" :show-index="true"
    :show-selection="true" :show-header-filter="true" :search-col-count="3" :crud-operations="crudOperations"
    :permission-prefix="'wh:warehouse'" :action-buttons="extraActionButtons" />
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('warehouse')

const extraActionButtons = [
  {
    label: (row) => row.status === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'wh:warehouse:toggle',
    show: (row) => true,
    confirm: (row) => `确定要${row.status === 1 ? '禁用' : '启用'}该仓库吗？`,
    onClick: async (row) => {
      const newStatus = row.status === 1 ? 0 : 1
      const res = await crudApi.setStatus(row.id, newStatus)
      if (res.code === 200) {
        KhMessageFn.success(res.message)
        pageRef.value?.reload()
      }
    },
  },
]

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'warehouseCode', label: '仓库编码', type: 'input', clearable: true },
  { prop: 'warehouseName', label: '仓库名称', type: 'input', clearable: true },
  {
    prop: 'type', label: '类型', type: 'select', clearable: true,
    options: 'dict:warehouse_type',
  },
]

const searchModel = reactive({
  warehouseCode: '',
  warehouseName: '',
  type: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'id', label: '主键ID', visible: false },
  { prop: 'warehouseCode', label: '仓库编码', width: 120 },
  { prop: 'warehouseName', label: '仓库名称', minWidth: 140 },
  { prop: 'warehouseType', label: '类型', width: 100, type: 'tag', tagMap: 'dict:warehouse_type' },
  { prop: 'totalBins', label: '库位数', width: 90, align: 'right' },
  { prop: 'usedBins', label: '已用库位', width: 100, align: 'right' },
  {
    prop: 'utilization', label: '利用率(%)', width: 110, align: 'center',
    type: 'slot',
  },
  {
    prop: 'status', label: '状态', width: 90, align: 'center',
    type: 'tag', tagMap: 'dict:status_flag',
  },
  { prop: 'createTime', label: '创建时间', width: 170 },
]

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: true,
  export: true,
}

// ==================== 表单配置（新增/编辑弹窗） ====================
const formColumns = [
  { prop: 'warehouseCode', label: '仓库编码', type: 'input', required: true, maxlength: 20 },
  { prop: 'warehouseName', label: '仓库名称', type: 'input', required: true, maxlength: 50 },
  {
    prop: 'warehouseType', label: '仓库类型', type: 'select', required: true,
    options: 'dict:warehouse_type',
  },
  { prop: 'status', label: '状态', type: 'select', required: true, options: 'dict:status_flag', },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]
</script>
