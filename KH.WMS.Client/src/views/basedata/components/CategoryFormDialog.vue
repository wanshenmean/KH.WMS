<template>
  <KhDialog
    v-model="visible"
    :title="dialogTitle"
    width="800px"
    :form-columns="formColumns"
    :form-model="innerFormData"
    :form-col-count="1"
    :confirm-loading="loading"
    @confirm="handleConfirm"
  />
</template>

<script setup>
import KhDialog from '@/components/KhDialog/index.vue'
import { createCategory, updateCategory } from '@/api/basedata'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  mode: { type: String, default: 'create' },
  data: { type: Object, default: () => ({}) },
  pageTitle: { type: String, default: '物料分类' },
})

const emit = defineEmits(['update:modelValue', 'success'])

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const loading = ref(false)

const dialogTitle = computed(() => {
  if (props.mode === 'edit') return `${props.pageTitle} - 编辑`
  return `${props.pageTitle} - 新增`
})

const formColumns = [
  { prop: 'categoryCode', label: '分类编码', type: 'input', required: true, maxlength: 20, placeholder: '请输入分类编码' },
  { prop: 'categoryName', label: '分类名称', type: 'input', required: true, maxlength: 50, placeholder: '请输入分类名称' },
  { prop: 'parentId', label: '上级分类ID', type: 'input', disabled: true },
  { prop: 'sortNo', label: '排序号', type: 'number', min: 0, precision: 0, placeholder: '请输入排序号' },
  { prop: 'status', label: '状态', type: 'switch', activeValue: 1, inactiveValue: 0 },
]

const innerFormData = ref({})

watch(() => props.modelValue, (val) => {
  if (val) {
    if (props.mode === 'create') {
      innerFormData.value = {
        status: 1,
        parentId: props.data?.id || null,
      }
    } else {
      innerFormData.value = { ...props.data }
    }
  }
})

const handleConfirm = async (data) => {
  loading.value = true
  try {
    if (props.mode === 'create') {
      await createCategory(data)
    } else {
      await updateCategory(data)
    }
    visible.value = false
    KhMessageFn.success('操作成功')
    emit('success')
  } catch {
    // 错误由拦截器处理
  } finally {
    loading.value = false
  }
}
</script>
