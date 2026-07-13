<template>
  <KhDialog
    v-model="visible"
    :title="dialogTitle"
    :form-columns="formColumns"
    :form-model="innerFormData"
    :form-col-count="2"
    :confirm-loading="loading"
    width="800px"
    @confirm="handleConfirm"
  />
</template>

<script setup>
import KhDialog from '@/components/KhDialog/index.vue'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  data: { type: Object, default: () => ({}) },
  pageTitle: { type: String, default: '出库发货' },
})

const emit = defineEmits(['update:modelValue', 'success'])

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const loading = ref(false)

const dialogTitle = computed(() => props.pageTitle + ' - 发货信息')

const formColumns = [
  { prop: 'orderNo', label: '出库单号', type: 'input', span: 12, disabled: true },
  { prop: 'customerName', label: '客户名称', type: 'input', span: 12, disabled: true },
  { prop: 'logisticsCompany', label: '物流公司', type: 'select', required: true, span: 12, options: [
    { label: '顺丰速运', value: '顺丰速运' },
    { label: '京东物流', value: '京东物流' },
    { label: '中通快递', value: '中通快递' },
    { label: '德邦物流', value: '德邦物流' },
    { label: 'EMS', value: 'EMS' },
  ]},
  { prop: 'logisticsNo', label: '物流单号', type: 'input', required: true, span: 12 },
  { prop: 'shipDate', label: '发货时间', type: 'date', required: true, span: 12 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]

const innerFormData = ref({})

watch(() => props.modelValue, (val) => {
  if (val) {
    innerFormData.value = {
      orderNo: props.data.orderNo || '',
      customerName: props.data.customerName || '',
      logisticsCompany: '',
      logisticsNo: '',
      shipDate: '',
      remark: '',
    }
  }
})

const handleConfirm = () => {
  loading.value = true
  setTimeout(() => {
    loading.value = false
    visible.value = false
    KhMessageFn.success('发货信息录入成功')
    emit('success')
  }, 500)
}
</script>
