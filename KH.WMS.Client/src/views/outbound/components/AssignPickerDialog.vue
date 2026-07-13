<template>
  <KhDialog
    v-model="visible"
    :title="dialogTitle"
    :form-columns="formColumns"
    :form-model="innerFormData"
    :form-col-count="1"
    :confirm-loading="loading"
    width="800px"
    @confirm="handleConfirm"
  />
</template>

<script setup>
import KhDialog from '@/components/KhDialog/index.vue'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  batch: { type: Boolean, default: false },
  selectedRows: { type: Array, default: () => [] },
  pageTitle: { type: String, default: '出库管理' },
})

const emit = defineEmits(['update:modelValue', 'success'])

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const loading = ref(false)

const dialogTitle = computed(() => props.batch ? props.pageTitle + ' - 批量分配拣货员' : props.pageTitle + ' - 分配拣货员')

const formColumns = [
  { prop: 'picker', label: '拣货员', type: 'select', required: true, options: [
    { label: '张三', value: '张三' },
    { label: '李四', value: '李四' },
    { label: '王五', value: '王五' },
    { label: '赵六', value: '赵六' },
    { label: '钱七', value: '钱七' },
  ]},
  { prop: 'priority', label: '优先级', type: 'select', options: [
    { label: '普通', value: '普通' },
    { label: '紧急', value: '紧急' },
    { label: '特急', value: '特急' },
  ]},
]

const innerFormData = ref({})

watch(() => props.modelValue, (val) => {
  if (val) {
    innerFormData.value = { picker: '', priority: '普通' }
  }
})

const handleConfirm = () => {
  loading.value = true
  setTimeout(() => {
    loading.value = false
    visible.value = false
    KhMessageFn.success(`已分配给 ${innerFormData.value.picker}`)
    emit('success')
  }, 500)
}
</script>
