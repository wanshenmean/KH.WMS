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

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  batch: { type: Boolean, default: false },
  selectedCount: { type: Number, default: 0 },
  pageTitle: { type: String, default: '任务管理' },
})

const emit = defineEmits(['update:modelValue', 'success'])

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const loading = ref(false)

const dialogTitle = computed(() => props.batch ? props.pageTitle + ' - 批量分配任务' : props.pageTitle + ' - 分配任务')

const formColumns = [
  { prop: 'assignee', label: '执行人', type: 'select', required: true, options: [
    { label: '张三', value: '张三' },
    { label: '李四', value: '李四' },
    { label: '王五', value: '王五' },
    { label: '赵六', value: '赵六' },
  ]},
  { prop: 'priority', label: '优先级', type: 'select', options: [
    { label: '普通', value: '普通' },
    { label: '紧急', value: '紧急' },
    { label: '特急', value: '特急' },
  ]},
  { prop: 'remark', label: '备注', type: 'textarea', maxlength: 200 },
]

const innerFormData = ref({})

watch(() => props.modelValue, (val) => {
  if (val) {
    innerFormData.value = { assignee: '', priority: '普通', remark: '' }
  }
})

const handleConfirm = () => {
  loading.value = true
  setTimeout(() => {
    loading.value = false
    visible.value = false
    KhMessageFn.success(`任务已分配给 ${innerFormData.value.assignee}`)
    emit('success')
  }, 500)
}
</script>
