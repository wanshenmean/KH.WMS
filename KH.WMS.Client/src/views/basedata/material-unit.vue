<template>
  <KhPage ref="pageRef" module="material-unit" title="物料单位" :search-columns="searchColumns" :search-model="searchModel"
    :columns="tableColumns" :form-columns="formColumns" :show-stat-cards="false" :show-toolbar="true" :show-index="true"
    :show-selection="true" :show-header-filter="true" :search-col-count="3" :crud-operations="crudOperations"
    :custom-form-data="formDialogData" :permission-prefix="'bd:material_unit'" :action-buttons="extraActionButtons" />
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('material-unit')

const extraActionButtons = [
  {
    label: (row) => row.status === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'bd:material_unit:toggle',
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

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'unitCode', label: '单位编码', type: 'input', clearable: true, placeholder: '请输入单位编码' },
  { prop: 'unitName', label: '单位名称', type: 'input', clearable: true, placeholder: '请输入单位名称' },
  {
    prop: 'status', label: '状态', type: 'select', clearable: true,
    options: 'dict:status_flag',
  },
]

const searchModel = reactive({ unitCode: '', unitName: '', status: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'unitCode', label: '单位编码', width: 150 },
  { prop: 'unitName', label: '单位名称', minWidth: 150 },
  { prop: 'baseUnitId', label: '基本单位', width: 120 },
  { prop: 'conversionRate', label: '换算率', width: 100, align: 'right' },
  {
    prop: 'status', label: '状态', width: 90, align: 'center',
    type: 'tag', tagMap: 'dict:status_flag',
  },
  { prop: 'remark', label: '备注', minWidth: 180, showOverflowTooltip: true },
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
  { prop: 'unitCode', label: '单位编码', type: 'input', required: true, placeholder: '请输入单位编码' },
  { prop: 'unitName', label: '单位名称', type: 'input', required: true, placeholder: '请输入单位名称' },
  // { prop: 'baseUnitId', label: '基本单位', type: 'input', required: true, placeholder: '请输入基本单位' },
  { prop: 'baseUnitId', label: '基本单位', type: 'select', options: 'dict:material_unit', placeholder: '请选择基本单位' },
  { prop: 'conversionRate', label: '换算率', type: 'number', min: 0, precision: 4, placeholder: '与基本单位的换算比例' },
  { prop: 'status', label: '状态', type: 'switch', activeValue: 1, inactiveValue: 0 },
  { prop: 'remark', label: '备注', type: 'textarea', maxlength: 200, rows: 3 },
]

const formDialogData = reactive({
  unitCode: '',
  unitName: '',
  baseUnitId: 0,
  conversionRate: '',
  status: '1',
  remark: ''
})
</script>
