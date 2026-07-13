<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage ref="pageRef" module="transfer-point" title="接驳口管理" :search-columns="searchColumns"
      :search-model="searchModel" :columns="tableColumns" :show-stat-cards="false" :show-toolbar="true"
      :show-index="true" :show-selection="true" :show-header-filter="true" :search-col-count="3"
      :crud-operations="crudOperations" :permission-prefix="'wh:transfer_point'" :form-columns="formColumns"
      :custom-form-data="formDialogData" :action-width="'120'" :action-buttons="extraActionButtons" />
  </div>
</template>

<script setup>
import { getZonesByWarehouse } from '@/api/warehouse'
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('transfer-point')

const extraActionButtons = [
  {
    label: (row) => row.status === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'wh:transfer_point:toggle',
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

const computedZoneDisabled = computed(() => {
  return !selectWarehouseId.value || zoneOptions.value.length === 0
})

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'pointCode', label: '接驳口编码', type: 'input', clearable: true },
  { prop: 'pointName', label: '接驳口名称', type: 'input', clearable: true },
  {
    prop: 'warehouseId', label: '所属仓库', type: 'select', clearable: true,
    options: 'dict:warehouse_list',
  },
  {
    prop: 'pointType', label: '接驳口类型', type: 'select', clearable: true,
    options: [
      { label: '入库接驳', value: 'INBOUND' },
      { label: '出库接驳', value: 'OUTBOUND' },
      { label: '混合', value: 'MIXED' },
    ],
  },
]

const searchModel = reactive({
  pointCode: '',
  pointName: '',
  warehouseId: '',
  pointType: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'id', label: '主键ID', visible: false },
  { prop: 'pointCode', label: '接驳口编码', width: 140 },
  { prop: 'pointName', label: '接驳口名称', width: 150 },
  { prop: 'warehouseId', label: '所属仓库', width: 130, type: 'tag', tagMap: 'dict:warehouse_list' },
  { prop: 'conveyorLineId', label: '输送线', width: 120, type: 'tag', tagMap: 'dict:conveyor_line_list' },
  { prop: 'aisleId', label: '巷道', width: 100 },
  { prop: 'zoneId', label: '库区', width: 100, type: 'tag', tagMap: 'dict:zone_list' },
  {
    prop: 'pointType', label: '接驳口类型', width: 110, align: 'center',
    type: 'tag',
    tagMap: { INBOUND: '入库接驳', OUTBOUND: '出库接驳', MIXED: '混合' },
    tagTypeMap: { INBOUND: 'success', OUTBOUND: 'warning', MIXED: 'primary' },
  },
  {
    prop: 'status', label: '状态', width: 90, align: 'center',
    type: 'tag', tagMap: 'dict:status_flag',
  },
  { prop: 'remark', label: '备注', minWidth: 160, showOverflowTooltip: true },
  { prop: 'createdBy', label: '创建人', width: 120 },
  { prop: 'createdByName', label: '创建人名称', width: 130 },
  { prop: 'createdTime', label: '创建时间', minWidth: 170 },
  { prop: 'lastModifiedBy', label: '最后修改人', minWidth: 130 },
  { prop: 'lastModifiedByName', label: '最后修改人名称', minWidth: 140 },
  { prop: 'lastModifiedTime', label: '最后修改时间', minWidth: 170 },
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
  { prop: 'pointCode', label: '接驳口编码', type: 'input', required: true, maxlength: 20 },
  { prop: 'pointName', label: '接驳口名称', type: 'input', required: true, maxlength: 50 },
  {
    prop: 'warehouseId', label: '所属仓库', type: 'select', required: true,
    options: 'dict:warehouse_list',
    onChange: async (value, formData) => {
      selectWarehouseId.value = value
      formData.zoneId = null
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
    prop: 'conveyorLineId', label: '输送线', type: 'select', required: true,
    options: 'dict:conveyor_line_list',
  },
  { prop: 'aisleId', label: '巷道', type: 'input' },
  {
    prop: 'zoneId', label: '库区', type: 'select',
    options: zoneOptions, disabled: computedZoneDisabled,
  },
  {
    prop: 'pointType', label: '接驳口类型', type: 'select', required: true,
    options: [
      { label: '入库接驳', value: 'INBOUND' },
      { label: '出库接驳', value: 'OUTBOUND' },
      { label: '混合', value: 'MIXED' },
    ],
  },
  { prop: 'status', label: '状态', type: 'select', required: true, options: 'dict:status_flag' },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]

const formDialogData = reactive({
  pointCode: '',
  pointName: '',
  warehouseId: null,
  conveyorLineId: null,
  aisleId: null,
  zoneId: null,
  pointType: 'MIXED',
  status: '1',
  remark: '',
})

</script>
