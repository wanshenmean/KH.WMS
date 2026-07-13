<template>
  <KhDialog
    v-model="visible"
    :title="dialogTitle"
    width="800px"
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
  pageTitle: { type: String, default: '入库管理' },
})

const emit = defineEmits(['update:modelValue', 'success'])

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const disabled = computed(() => props.mode === 'view')
const loading = ref(false)

const dialogTitle = computed(() => {
  return props.pageTitle + (props.mode === 'create' ? ' - 新建' : props.mode === 'edit' ? ' - 编辑' : ' - 详情')
})

const formColumns = [
  { prop: 'receiptNo', label: '收货单号', type: 'input', required: true, span: 12, disabled: true },
  { prop: 'supplierName', label: '供应商', type: 'select', required: true, span: 12, options: [
    { label: '华东精密制造', value: '华东精密制造' },
    { label: '深圳电子科技', value: '深圳电子科技' },
    { label: '北京材料供应', value: '北京材料供应' },
    { label: '广州五金配件', value: '广州五金配件' },
  ]},
  { prop: 'warehouse', label: '收货仓库', type: 'select', required: true, span: 12, options: [
    { label: 'A区-原材料仓', value: 'A区-原材料仓' },
    { label: 'B区-成品仓', value: 'B区-成品仓' },
    { label: 'C区-半成品仓', value: 'C区-半成品仓' },
    { label: 'D区-备件仓', value: 'D区-备件仓' },
  ]},
  { prop: 'receiptType', label: '收货类型', type: 'select', required: true, span: 12, options: [
    { label: '采购收货', value: '采购收货' },
    { label: '退货入库', value: '退货入库' },
    { label: '调拨入库', value: '调拨入库' },
  ]},
  { prop: 'expectedDate', label: '预计到货日期', type: 'date', span: 12 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]

const innerFormData = ref({})

watch(() => props.modelValue, (val) => {
  if (val) {
    if (props.mode === 'create') {
      const now = new Date()
      const no = `RCV-${now.getFullYear()}${String(now.getMonth() + 1).padStart(2, '0')}${String(now.getDate()).padStart(2, '0')}-${String(Math.floor(Math.random() * 900) + 100)}`
      innerFormData.value = { receiptNo: no, supplierName: '', warehouse: '', receiptType: '采购收货', expectedDate: '', remark: '' }
    } else {
      innerFormData.value = { ...props.data }
    }
  }
})

const handleConfirm = (data) => {
  loading.value = true
  setTimeout(() => {
    loading.value = false
    visible.value = false
    KhMessageFn.success(props.mode === 'create' ? '新建收货单成功' : '保存成功')
    emit('success')
  }, 500)
}
</script>
