<template>
  <KhDialog
    v-model="visible"
    :title="dialogTitle"
    :form-columns="formColumns"
    :form-model="innerFormData"
    :form-col-count="2"
    :form-disabled="disabled"
    :confirm-loading="loading"
    width="800px"
    @confirm="handleConfirm"
  />
</template>

<script setup>
import KhDialog from '@/components/KhDialog/index.vue'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  mode: { type: String, default: 'create' },
  data: { type: Object, default: () => ({}) },
  pageTitle: { type: String, default: '出库管理' },
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
  { prop: 'orderNo', label: '出库单号', type: 'input', required: true, span: 12, disabled: true },
  { prop: 'customerName', label: '客户名称', type: 'select', required: true, span: 12, options: [
    { label: '华为终端', value: '华为终端' },
    { label: '小米科技', value: '小米科技' },
    { label: '比亚迪电子', value: '比亚迪电子' },
    { label: '富士康精密', value: '富士康精密' },
    { label: '中兴通讯', value: '中兴通讯' },
  ]},
  { prop: 'warehouse', label: '出库仓库', type: 'select', required: true, span: 12, options: [
    { label: 'A区-原材料仓', value: 'A区-原材料仓' },
    { label: 'B区-成品仓', value: 'B区-成品仓' },
    { label: 'C区-半成品仓', value: 'C区-半成品仓' },
  ]},
  { prop: 'orderType', label: '出库类型', type: 'select', required: true, span: 12, options: [
    { label: '销售出库', value: '销售出库' },
    { label: '生产领料', value: '生产领料' },
    { label: '调拨出库', value: '调拨出库' },
    { label: '样品出库', value: '样品出库' },
  ]},
  { prop: 'orderDate', label: '订单日期', type: 'date', required: true, span: 12 },
  { prop: 'shipDate', label: '要求发货日期', type: 'date', span: 12 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]

const innerFormData = ref({})

watch(() => props.modelValue, (val) => {
  if (val) {
    if (props.mode === 'create') {
      const now = new Date()
      const no = `OB-2025${String(now.getMonth() + 1).padStart(2, '0')}${String(now.getDate()).padStart(2, '0')}-${String(Math.floor(Math.random() * 900) + 100)}`
      innerFormData.value = { orderNo: no, customerName: '', warehouse: '', orderType: '销售出库', orderDate: '', shipDate: '', remark: '' }
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
    KhMessageFn.success(props.mode === 'create' ? '新建出库单成功' : '保存成功')
    emit('success')
  }, 500)
}
</script>
