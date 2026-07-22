<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage ref="pageRef" title="到货预约实战" module="training-arrival-appointment" :search-columns="searchColumns"
      :search-model="searchModel" :columns="tableColumns" :show-stat-cards="false" :show-toolbar="true"
      :show-index="true" :show-header-filter="true"
      :crud-operations="{ create: false, update: false, delete: true, view: true, export: false }"
      permission-prefix="training:appointment" :action-buttons="actionButtons" :toolbar-buttons="toolbarButtons"
      :detail-lines="detailLines" detail-width="1100px" />

    <KhDialog v-model="dialogVisible" :title="dialogMode === 'create' ? '新增到货预约' : '编辑到货预约'" width="1150px"
      destroy-on-close :confirm-loading="submitLoading" @confirm="handleSubmit" @close="resetForm">
      <KhForm ref="formRef" v-model="formData" :columns="formColumns" :col-count="4" label-width="100px" />
      <el-divider content-position="left">预约明细</el-divider>
      <KhEditableTable v-model="items" :columns="lineColumns" :default-row="createEmptyLine" :max-height="360"
        add-text="添加明细" :action-width="70" />
    </KhDialog>
  </div>
</template>

<script setup>
import { KhEditableTable } from '@/components'
import { useCrudApi } from '@/utils/crud'
import { getTrainingCarriers, getTrainingOwners } from '@/api/training'

const pageRef = ref(null)
const formRef = ref(null)
const crudApi = useCrudApi('training-arrival-appointment')
const dialogVisible = ref(false)
const dialogMode = ref('create')
const submitLoading = ref(false)
const carrierOptions = ref([])
const ownerOptions = ref([])
const items = ref([])

const createFormData = () => ({
  id: 0, appointmentNo: '', carrierId: null, ownerId: null, warehouseId: null,
  appointmentDate: '', appointmentTimeSlot: '', vehicleNo: '', driverName: '', driverPhone: '', remark: '',
})
const formData = reactive(createFormData())
const createEmptyLine = () => ({
  id: 0, lineNo: items.value.length + 1, materialId: null, materialCode: '', materialName: '',
  expectedQty: null, unitId: null, batchNo: '', remark: '',
})

const searchModel = reactive({ appointmentNo: '', appointmentDate: '' })
const searchColumns = [
  { prop: 'appointmentNo', label: '预约单号', type: 'input', clearable: true },
  { prop: 'appointmentDate', label: '预约日期', type: 'date', clearable: true },
]
const tableColumns = [
  { prop: 'appointmentNo', label: '预约单号', width: 190 },
  { prop: 'appointmentDate', label: '预约日期', width: 110, align: 'center' },
  { prop: 'appointmentTimeSlot', label: '预约时段', width: 120 },
  { prop: 'carrierId', label: '承运商', width: 140, type: 'tag', tagMap: computed(() => carrierTagMap.value) },
  { prop: 'ownerId', label: '货主', width: 140, type: 'tag', tagMap: computed(() => ownerTagMap.value) },
  { prop: 'warehouseId', label: '仓库', width: 130, type: 'tag', tagMap: 'dict:warehouse_list' },
  { prop: 'vehicleNo', label: '车牌号', width: 120 },
  { prop: 'driverName', label: '司机', width: 100 },
  { prop: 'driverPhone', label: '司机电话', width: 130 },
  { prop: 'remark', label: '备注', minWidth: 150, showOverflowTooltip: true },
]
const carrierTagMap = computed(() => Object.fromEntries(carrierOptions.value.map(x => [x.value, x.label])))
const ownerTagMap = computed(() => Object.fromEntries(ownerOptions.value.map(x => [x.value, x.label])))
const formColumns = computed(() => [
  { prop: 'appointmentNo', label: '预约单号', type: 'input', required: true, maxlength: 50, colSpan: 2 },
  { prop: 'appointmentDate', label: '预约日期', type: 'date', required: true, valueFormat: 'YYYY-MM-DD' },
  { prop: 'appointmentTimeSlot', label: '预约时段', type: 'input', maxlength: 20 },
  { prop: 'carrierId', label: '承运商', type: 'select', clearable: true, options: carrierOptions.value },
  { prop: 'ownerId', label: '货主', type: 'select', clearable: true, options: ownerOptions.value },
  { prop: 'warehouseId', label: '仓库', type: 'select', clearable: true, options: 'dict:warehouse_list' },
  { prop: 'vehicleNo', label: '车牌号', type: 'input', maxlength: 30 },
  { prop: 'driverName', label: '司机姓名', type: 'input', maxlength: 50 },
  { prop: 'driverPhone', label: '司机电话', type: 'input', maxlength: 20 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 500 },
])
const lineColumns = [
  { prop: 'lineNo', label: '行号', type: 'number', width: 80, min: 1, precision: 0 },
  { prop: 'materialCode', label: '物料编码', type: 'select', minWidth: 150, options: 'dict:material_list' },
  { prop: 'materialName', label: '物料名称', type: 'input', minWidth: 170 },
  { prop: 'expectedQty', label: '预计数量', type: 'number', width: 120, min: 0.001, precision: 3, controls: false },
  { prop: 'batchNo', label: '批次号', type: 'input', width: 140 },
  { prop: 'remark', label: '备注', type: 'input', minWidth: 140 },
]
const detailLines = [{
  prop: 'items', title: '预约明细', columns: [
    { prop: 'lineNo', label: '行号', width: 70 }, { prop: 'materialCode', label: '物料编码', width: 140 },
    { prop: 'materialName', label: '物料名称', minWidth: 170 }, { prop: 'expectedQty', label: '预计数量', width: 120 },
    { prop: 'batchNo', label: '批次号', width: 140 }, { prop: 'remark', label: '备注', minWidth: 140 },
  ]
}]

const openCreate = () => {
  resetForm()
  dialogMode.value = 'create'
  dialogVisible.value = true
}
const openUpdate = async (row) => {
  const res = await crudApi.detail(row.id)
  const detail = res.data || res
  resetForm()
  dialogMode.value = 'update'
  Object.assign(formData, detail)
  items.value = (detail.items || []).map(x => ({ ...x }))
  dialogVisible.value = true
}
const toolbarButtons = [{ label: '新增', type: 'primary', permission: 'training:appointment:add', onClick: openCreate }]
const actionButtons = [{ label: '编辑', permission: 'training:appointment:edit', onClick: openUpdate }]

function resetForm() {
  Object.assign(formData, createFormData())
  items.value = []
}

const validateItems = () => {
  if (!items.value.length) return '至少添加一条预约明细'
  if (items.value.some(x => !x.lineNo || x.lineNo <= 0)) return '明细行号必须大于 0'
  if (new Set(items.value.map(x => x.lineNo)).size !== items.value.length) return '明细行号不能重复'
  if (items.value.some(x => !x.materialCode || !x.materialName)) return '物料编码和名称不能为空'
  if (items.value.some(x => !x.expectedQty || x.expectedQty <= 0)) return '预计到货数量必须大于 0'
  return ''
}
const toRequestLine = (row, isCreate) => {
  const { _rowKey, materialLabel, ...line } = row
  return {
    ...line,
    id: isCreate ? 0 : (line.id || 0),
    appointmentId: isCreate ? 0 : (line.appointmentId || 0),
  }
}
const handleSubmit = async () => {
  if (submitLoading.value || !await formRef.value?.validate()) return
  const error = validateItems()
  if (error) return KhMessageFn.warning(error)
  submitLoading.value = true
  try {
    const isCreate = dialogMode.value === 'create'
    const data = { ...formData, items: items.value.map(x => toRequestLine(x, isCreate)) }
    if (isCreate) {
      data.id = 0
      await crudApi.create(data)
    } else {
      await crudApi.update(data)
    }
    KhMessageFn.success(dialogMode.value === 'create' ? '新增成功' : '更新成功')
    dialogVisible.value = false
    pageRef.value?.reload()
  } finally {
    submitLoading.value = false
  }
}

onMounted(async () => {
  const [carriers, owners] = await Promise.all([getTrainingCarriers(), getTrainingOwners()])
  carrierOptions.value = (carriers.data || []).map(x => ({ label: `${x.carrierCode} - ${x.carrierName}`, value: x.id }))
  ownerOptions.value = (owners.data || []).map(x => ({ label: `${x.ownerCode} - ${x.ownerName}`, value: x.id }))
})
</script>
