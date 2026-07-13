<template>
  <KhPage ref="pageRef" module="port" title="站台管理" :search-columns="searchColumns" :search-model="searchModel"
    :columns="tableColumns" :form-columns="formColumns" :show-stat-cards="false" :show-toolbar="true" :show-index="true"
    :show-selection="true" :show-header-filter="true" :search-col-count="3" :crud-operations="crudOperations"
    :permission-prefix="'wh:port'" :custom-form-data="formDialogData" :action-buttons="extraActionButtons" />
</template>

<script setup>
import { getZonesByWarehouse } from '@/api/warehouse'
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('port')

const extraActionButtons = [
  {
    label: (row) => row.status === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'wh:port:toggle',
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
const selectWarehouseId = ref('')

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'portCode', label: '站台编码', type: 'input', clearable: true },
  { prop: 'portName', label: '站台名称', type: 'input', clearable: true },
  {
    prop: 'warehouseId', label: '所属仓库', type: 'select', clearable: true,
    options: 'dict:warehouse_list',
  },
  {
    prop: 'portType', label: '站台类型', type: 'select', clearable: true,
    options: 'dict:port_type',
  },
  {
    prop: 'status', label: '状态', type: 'select', clearable: true,
    options: 'dict:status_flag',
  },
]

const searchModel = reactive({ portCode: '', portName: '', warehouseId: '', portType: '', status: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'portCode', label: '站台编码', width: 120 },
  { prop: 'portName', label: '站台名称', width: 150 },
  { prop: 'warehouseId', label: '所属仓库', width: 140, type: 'tag', tagMap: 'dict:warehouse_list' },
  { prop: 'zoneId', label: '所属库区', width: 140, type: 'tag', tagMap: 'dict:zone_list' },
  { prop: 'conveyorLineId', label: '所属输送线', width: 140, type: 'tag', tagMap: 'dict:conveyor_line_list'},
  {
    prop: 'portType', label: '站台类型', width: 110, align: 'center',
    type: 'tag', tagMap: 'dict:port_type',
  },
  { prop: 'equipmentCode', label: '关联设备编码', width: 130 },
  {
    prop: 'status', label: '状态', width: 80, align: 'center',
    type: 'tag', tagMap: 'dict:status_flag',
  },
  { prop: 'remark', label: '备注', minWidth: 160, showOverflowTooltip: true },
  { prop: 'createdTime', label: '创建时间', width: 170 },
]

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: false,
  export: false,
}

const computedZoneDisabled = computed(() => {
  return !selectWarehouseId.value || zoneOptions.value.length === 0
})

// ==================== 表单配置（新增/编辑弹窗） ====================
const formColumns = [
  { prop: 'portCode', label: '站台编码', type: 'input', required: true, maxlength: 20 },
  { prop: 'portName', label: '站台名称', type: 'input', required: true, maxlength: 50 },
  {
    prop: 'warehouseId', label: '所属仓库', type: 'select', required: true,
    options: 'dict:warehouse_list', onChange: async (value, formData) => {
      selectWarehouseId.value = value
      formData.zoneId = ''
      if (value) {
        const res = await getZonesByWarehouse(value)
        zoneOptions.value = res.data.map(item => ({
          label: item.zoneName,
          value: item.id,
        }))
      } else {
        zoneOptions.value = []
      }
    }
  },
  {
    prop: 'zoneId', label: '所属库区', type: 'select',
    options: zoneOptions, disabled: computedZoneDisabled,
  },
  {
    prop: 'conveyorLineId', label: '所属输送线', type: 'select',
    options: 'dict:conveyor_line_list',
  },
  {
    prop: 'portType', label: '站台类型', type: 'select', required: true,
    options: 'dict:port_type',
  },
  { prop: 'equipmentCode', label: '关联设备编码', type: 'input', maxlength: 50 },
  { prop: 'status', label: '状态', type: 'select', required: true, options: 'dict:status_flag' },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]

const formDialogData = reactive({
  portCode: '',
  portName: '',
  warehouseId: '',
  zoneId: '',
  conveyorLineId: 0,
  portType: '',
  equipmentCode: '',
  status: '1',
  remark: '',
})

</script>
