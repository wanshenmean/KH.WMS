<template>
  <KhDialog
    v-model="visible"
    :title="dialogTitle"
    :form-columns="formColumns"
    :form-model="innerFormData"
    :form-col-count="2"
    :form-disabled="disabled"
    :confirm-loading="loading"
    @confirm="handleConfirm"
  />
</template>

<script setup>
import KhDialog from '@/components/KhDialog/index.vue'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  mode: { type: String, default: 'create' },
  data: { type: Object, default: () => ({}) },
})

const emit = defineEmits(['update:modelValue', 'success'])

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const disabled = computed(() => props.mode === 'view')
const loading = ref(false)

const dialogTitle = computed(() => props.mode === 'create' ? '新增库位' : props.mode === 'edit' ? '编辑库位' : '库位详情')

const formColumns = [
  { prop: 'binNo', label: '库位编号', type: 'input', required: true, span: 12 },
  { prop: 'binName', label: '库位名称', type: 'input', required: true, span: 12 },
  { prop: 'warehouseNo', label: '所属仓库', type: 'select', required: true, span: 12, options: [
    { label: 'A区-原材料仓', value: 'WH-A' },
    { label: 'B区-成品仓', value: 'WH-B' },
    { label: 'C区-半成品仓', value: 'WH-C' },
    { label: 'D区-备件仓', value: 'WH-D' },
  ]},
  { prop: 'zoneNo', label: '所属库区', type: 'select', required: true, span: 12, options: [
    { label: 'A1区', value: 'Z-A1' },
    { label: 'A2区', value: 'Z-A2' },
    { label: 'B1区', value: 'Z-B1' },
    { label: 'B2区', value: 'Z-B2' },
  ]},
  { prop: 'binType', label: '库位类型', type: 'select', required: true, span: 12, options: [
    { label: '标准位', value: '标准位' },
    { label: '重型位', value: '重型位' },
    { label: '轻型位', value: '轻型位' },
    { label: '冷链位', value: '冷链位' },
  ]},
  { prop: 'capacity', label: '容量(kg)', type: 'number', span: 12, min: 0 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]

const innerFormData = ref({})

watch(() => props.modelValue, (val) => {
  if (val) {
    if (props.mode === 'create') {
      innerFormData.value = { binNo: '', binName: '', warehouseNo: '', zoneNo: '', binType: '标准位', capacity: 1000, remark: '' }
    } else {
      innerFormData.value = { ...props.data }
    }
  }
})

const handleConfirm = () => {
  loading.value = true
  setTimeout(() => {
    loading.value = false
    visible.value = false
    KhMessageFn.success(props.mode === 'create' ? '新增库位成功' : '保存成功')
    emit('success')
  }, 500)
}
</script>
