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
  pageTitle: { type: String, default: '波次管理' },
})

const emit = defineEmits(['update:modelValue', 'success'])

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const loading = ref(false)

const dialogTitle = computed(() => pageTitle + ' - 新建')

const formColumns = [
  { prop: 'waveName', label: '波次名称', type: 'input', required: true, span: 12 },
  { prop: 'mergeRule', label: '合单规则', type: 'select', required: true, span: 12, options: [
    { label: '按客户合并', value: '按客户合并' },
    { label: '按区域合并', value: '按区域合并' },
    { label: '按仓库合并', value: '按仓库合并' },
    { label: '按时间合并', value: '按时间合并' },
  ]},
  { prop: 'warehouse', label: '出库仓库', type: 'select', required: true, span: 12, options: [
    { label: 'A区-原材料仓', value: 'A区-原材料仓' },
    { label: 'B区-成品仓', value: 'B区-成品仓' },
    { label: 'C区-半成品仓', value: 'C区-半成品仓' },
  ]},
  { prop: 'maxOrders', label: '最大订单数', type: 'number', span: 12, min: 1 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]

const innerFormData = ref({})

watch(() => props.modelValue, (val) => {
  if (val) {
    innerFormData.value = { waveName: '', mergeRule: '按客户合并', warehouse: '', maxOrders: 50, remark: '' }
  }
})

const handleConfirm = () => {
  loading.value = true
  setTimeout(() => {
    loading.value = false
    visible.value = false
    KhMessageFn.success('波次创建成功')
    emit('success')
  }, 500)
}
</script>
