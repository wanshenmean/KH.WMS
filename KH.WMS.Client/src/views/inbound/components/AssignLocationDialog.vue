<template>
  <KhDialog
    v-model="visible"
    :title="dialogTitle"
    width="800px"
    :form-columns="formColumns"
    :form-model="innerFormData"
    :form-col-count="2"
    :confirm-loading="loading"
    @confirm="handleConfirm"
  />
</template>

<script setup>
import KhDialog from '@/components/KhDialog/index.vue'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  data: { type: Object, default: () => ({}) },
  pageTitle: { type: String, default: '入库管理' },
})

const emit = defineEmits(['update:modelValue', 'success'])

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const loading = ref(false)

const dialogTitle = computed(() => pageTitle + ' - 分配库位')

const formColumns = [
  { prop: 'materialCode', label: '物料编码', type: 'input', span: 12, disabled: true },
  { prop: 'materialName', label: '物料名称', type: 'input', span: 12, disabled: true },
  { prop: 'warehouse', label: '仓库', type: 'input', span: 12, disabled: true },
  { prop: 'zone', label: '库区', type: 'select', required: true, span: 12, options: [
    { label: 'A1区', value: 'A1区' },
    { label: 'A2区', value: 'A2区' },
    { label: 'B1区', value: 'B1区' },
    { label: 'B2区', value: 'B2区' },
    { label: 'C1区', value: 'C1区' },
  ]},
  { prop: 'bin', label: '库位', type: 'select', required: true, span: 12, options: [
    { label: 'A1-001-A', value: 'A1-001-A' },
    { label: 'A1-001-B', value: 'A1-001-B' },
    { label: 'A1-002-A', value: 'A1-002-A' },
    { label: 'A2-001-A', value: 'A2-001-A' },
    { label: 'B1-001-A', value: 'B1-001-A' },
  ]},
  { prop: 'quantity', label: '上架数量', type: 'number', required: true, span: 12, min: 1 },
]

const innerFormData = ref({})

watch(() => props.modelValue, (val) => {
  if (val) {
    innerFormData.value = {
      materialCode: props.data.materialCode || '',
      materialName: props.data.materialName || '',
      warehouse: props.data.warehouse || '',
      zone: '',
      bin: '',
      quantity: props.data.quantity || 0,
    }
  }
})

const handleConfirm = () => {
  loading.value = true
  setTimeout(() => {
    loading.value = false
    visible.value = false
    KhMessageFn.success('库位分配成功，已触发ASRS任务')
    emit('success')
  }, 500)
}
</script>
