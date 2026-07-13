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
  pageTitle: { type: String, default: '任务管理' },
})

const emit = defineEmits(['update:modelValue', 'success'])

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const loading = ref(false)

const dialogTitle = computed(() => pageTitle + ' - 新建')

const warehouses = [
  { label: 'A区-原材料仓', value: 'A区-原材料仓' },
  { label: 'B区-成品仓', value: 'B区-成品仓' },
  { label: 'C区-半成品仓', value: 'C区-半成品仓' },
  { label: 'D区-备件仓', value: 'D区-备件仓' },
]

const formColumns = [
  { prop: 'transferNo', label: '调拨单号', type: 'input', required: true, span: 12, disabled: true },
  { prop: 'sourceWarehouse', label: '源仓库', type: 'select', required: true, span: 12, options: warehouses },
  { prop: 'targetWarehouse', label: '目标仓库', type: 'select', required: true, span: 12, options: warehouses },
  { prop: 'materialCode', label: '物料编码', type: 'input', required: true, span: 12 },
  { prop: 'quantity', label: '调拨数量', type: 'number', required: true, span: 12, min: 1 },
  { prop: 'transferDate', label: '调拨日期', type: 'date', required: true, span: 12 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]

const innerFormData = ref({})

watch(() => props.modelValue, (val) => {
  if (val) {
    const now = new Date()
    const no = `TRF-${now.getFullYear()}${String(now.getMonth() + 1).padStart(2, '0')}${String(now.getDate()).padStart(2, '0')}-${String(Math.floor(Math.random() * 900) + 100)}`
    innerFormData.value = { transferNo: no, sourceWarehouse: '', targetWarehouse: '', materialCode: '', quantity: 1, transferDate: '', remark: '' }
  }
})

const handleConfirm = () => {
  loading.value = true
  setTimeout(() => {
    loading.value = false
    visible.value = false
    KhMessageFn.success('调拨单已提交，等待审核')
    emit('success')
  }, 500)
}
</script>
