<template>
  <KhPage ref="pageRef" module="aisle" title="巷道管理" :search-columns="searchColumns" :search-model="searchModel"
    :columns="tableColumns" :form-columns="formColumns" :show-stat-cards="false" :show-toolbar="true" :show-index="true"
    :show-selection="true" :show-header-filter="true" :search-col-count="3" :crud-operations="crudOperations"
    :custom-form-data="formDialogData" :permission-prefix="'wh:aisle'" :action-buttons="extraActionButtons" />
</template>

<script setup>
import { getZonesByWarehouse } from '@/api/warehouse'
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('aisle')

const extraActionButtons = [
  {
    label: (row) => row.status === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'wh:aisle:toggle',
    show: (row) => true,
    confirm: (row) => `确定要${row.status === 1 ? '禁用' : '启用'}吗？`,
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
const zoneOptions = ref([])

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'aisleCode', label: '巷道编码', type: 'input', clearable: true },
  { prop: 'aisleName', label: '巷道名称', type: 'input', clearable: true },
  {
    prop: 'warehouseName', label: '所属仓库', type: 'select', clearable: true,
    options: 'dict:warehouse_list',
  },
  {
    prop: 'status', label: '状态', type: 'select', clearable: true,
    options: 'dict:status_flag',
  },
]

const searchModel = reactive({ aisleCode: '', aisleName: '', warehouseName: '', status: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'aisleCode', label: '巷道编码', width: 120 },
  { prop: 'aisleName', label: '巷道名称', width: 150 },
  { prop: 'warehouseId', label: '仓库ID', width: 120 },
  { prop: 'zoneId', label: '库区ID', width: 110 },
  { prop: 'equipmentCode', label: '关联堆垛机设备编码', width: 120 },
  { prop: 'status', label: '状态', width: 80 },
  { prop: 'sortNo', label: '排序', width: 80 },
  { prop: 'remark', label: '备注', minWidth: 150 },
]

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: false,
  export: false,
}

// ==================== 表单配置（新增/编辑弹窗） ====================
const formColumns = [
  { prop: 'aisleCode', label: '巷道编码', type: 'input', required: true, maxlength: 30 },
  { prop: 'aisleName', label: '巷道名称', type: 'input', required: true, maxlength: 50 },

  {
    prop: 'warehouseId', label: '仓库ID', width: 120, options: 'dict:warehouse_list', required: true, type: 'select', onChange: async (value) => {
      if (value) {
        const res = await getZonesByWarehouse(value)
        zoneOptions.value = res.data.map(zone => ({ label: zone.zoneName, value: zone.zoneId }))
      } else {
        zoneOptions.value = []
      }
    }
  },
  { prop: 'zoneId', label: '库区ID', width: 120, options: zoneOptions, type: 'select' },
  { prop: 'equipmentCode', label: '关联堆垛机设备编码', type: 'input', maxlength: 30, labelWidth: 150, span: 24 },
  { prop: 'sortNo', label: '排序', type: 'number', required: true },
  { prop: 'status', label: '状态', type: 'select', required: true, options: 'dict:status_flag' },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]

const formDialogData = reactive({
  aisleCode: '',
  aisleName: '',
  warehouseId: '',
  zoneId: 0,
  equipmentCode: '',
  sortNo: 0,
  status: '1',
  remark: ''
})
</script>
