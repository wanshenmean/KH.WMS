<template>
  <div style="height: 100%; ">
    <KhPage ref="pageRef" title="承运商实战" module="training-carrier"
      :search-columns="searchColumns" :search-model="searchModel" :columns="tableColumns"
      :form-columns="formColumns" :show-stat-cards="false" :show-toolbar="true"
      :show-index="true" :show-selection="true" :show-header-filter="true" :search-col-count="3"
      :crud-operations="{ create: true, update: true, delete: true, view: true, export: true }"
      permission-prefix="training:carrier" :action-buttons="statusButtons" :toolbar-buttons="toolbarButtons" />
    <input ref="fileInput" type="file" accept=".xlsx,.xls" hidden @change="handleImport" />
  </div>
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'
import { importTrainingCarriers } from '@/api/training'

const pageRef = ref(null)
const fileInput = ref(null)
const crudApi = useCrudApi('training-carrier')

const searchModel = reactive({ carrierCode: '', carrierName: '', status: '' })
const searchColumns = [
  { prop: 'carrierCode', label: '承运商编码', type: 'input', clearable: true },
  { prop: 'carrierName', label: '承运商名称', type: 'input', clearable: true },
  { prop: 'status', label: '状态', type: 'select', clearable: true, options: 'dict:status_flag' },
]

const tableColumns = [
  { prop: 'carrierCode', label: '承运商编码', width: 140 },
  { prop: 'carrierName', label: '承运商名称', minWidth: 160 },
  { prop: 'contactName', label: '联系人', width: 100 },
  { prop: 'contactPhone', label: '联系电话', width: 130 },
  { prop: 'transportMode', label: '运输方式', width: 120 },
  { prop: 'status', label: '状态', width: 90, align: 'center', type: 'tag', tagMap: 'dict:status_flag' },
  { prop: 'remark', label: '备注', minWidth: 160, showOverflowTooltip: true },
  { prop: 'createdTime', label: '创建时间', width: 170 },
]

const formColumns = [
  { prop: 'carrierCode', label: '承运商编码', type: 'input', required: true, maxlength: 30 },
  { prop: 'carrierName', label: '承运商名称', type: 'input', required: true, maxlength: 100 },
  { prop: 'contactName', label: '联系人', type: 'input', maxlength: 50 },
  { prop: 'contactPhone', label: '联系电话', type: 'input', maxlength: 20 },
  { prop: 'transportMode', label: '运输方式', type: 'select', clearable: true, options: [
    { label: '公路运输', value: 'ROAD' }, { label: '冷链运输', value: 'COLD_CHAIN' }, { label: '多式联运', value: 'MULTIMODAL' },
  ] },
  { prop: 'status', label: '状态', type: 'switch', activeValue: 1, inactiveValue: 0, defaultValue: 1 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 500 },
]

const statusButtons = [{
  label: (row) => row.status === 1 ? '禁用' : '启用',
  type: 'warning', permission: 'training:carrier:toggle',
  confirm: (row) => `确定${row.status === 1 ? '禁用' : '启用'}该承运商吗？`,
  onClick: async (row) => {
    const res = await crudApi.setStatus(row.id, row.status === 1 ? 0 : 1)
    KhMessageFn.success(res.message)
    pageRef.value?.reload()
  },
}]

const toolbarButtons = [{ label: '导入', permission: 'training:carrier:import', onClick: () => fileInput.value?.click() }]

const handleImport = async (event) => {
  const file = event.target.files?.[0]
  if (!file) return
  try {
    const res = await importTrainingCarriers(file)
    KhMessageFn.success(res.message || '导入完成')
    pageRef.value?.reload()
  } finally {
    event.target.value = ''
  }
}
</script>
