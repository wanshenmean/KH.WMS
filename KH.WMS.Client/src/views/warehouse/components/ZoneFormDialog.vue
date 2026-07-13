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
  mode: { type: String, default: 'create' },
  data: { type: Object, default: () => ({}) },
  parentName: { type: String, default: '' },
  pageTitle: { type: String, default: '库区管理' },
})

const emit = defineEmits(['update:modelValue', 'success'])

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const loading = ref(false)

const dialogTitle = computed(() => {
  if (props.mode === 'edit') return `${props.pageTitle} - 编辑`
  if (props.parentName) return `${props.pageTitle} - 新增`
  return `${props.pageTitle} - 新增`
})

const formColumns = [
  { prop: 'zoneCode', label: '库区编码', type: 'input', required: true, maxlength: 20 },
  { prop: 'zoneName', label: '库区名称', type: 'input', required: true, maxlength: 50 },
  {
    prop: 'zoneType', label: '库区类型', type: 'select', required: true,
    options: [
      { label: '存储区', value: '存储区' },
      { label: '拣选区', value: '拣选区' },
      { label: '暂存区', value: '暂存区' },
      { label: '退货区', value: '退货区' },
    ],
  },
  { prop: 'rows', label: '排数', type: 'number', required: true, min: 1 },
  { prop: 'cols', label: '列数', type: 'number', required: true, min: 1 },
  { prop: 'layers', label: '层数', type: 'number', required: true, min: 1 },
  { prop: 'status', label: '状态', type: 'radio', required: true, options: [{ label: '启用', value: 1 }, { label: '停用', value: 0 }] },
]

const innerFormData = ref({})

watch(() => props.modelValue, (val) => {
  if (val) {
    if (props.mode === 'create') {
      innerFormData.value = { status: 1, rows: 1, cols: 1, layers: 1 }
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
    KhMessageFn.success('操作成功')
    emit('success')
  }, 500)
}
</script>
