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
  pageTitle: { type: String, default: '仓库管理' },
})

const emit = defineEmits(['update:modelValue', 'success'])

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const disabled = computed(() => props.mode === 'view')
const loading = ref(false)

const dialogTitle = computed(() =>
  props.pageTitle + (props.mode === 'create' ? ' - 新增' : props.mode === 'edit' ? ' - 编辑' : ' - 详情')
)

const formColumns = [
  { prop: 'warehouseCode', label: '仓库编码', type: 'input', required: true, maxlength: 20 },
  { prop: 'warehouseName', label: '仓库名称', type: 'input', required: true, maxlength: 50 },
  {
    prop: 'type', label: '仓库类型', type: 'select', required: true,
    options: [
      { label: '立体库', value: '立体库' },
      { label: '平面库', value: '平面库' },
      { label: '冷库', value: '冷库' },
    ],
  },
  { prop: 'address', label: '仓库地址', type: 'input', required: true, span: 24, maxlength: 200 },
  { prop: 'totalBins', label: '库位总数', type: 'number', required: true, min: 1 },
  { prop: 'asrsEnabled', label: '启用ASRS', type: 'radio', options: [{ label: '是', value: 1 }, { label: '否', value: 0 }] },
  { prop: 'status', label: '状态', type: 'radio', required: true, options: [{ label: '启用', value: 1 }, { label: '停用', value: 0 }] },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]

const innerFormData = ref({})

watch(() => props.modelValue, (val) => {
  if (val) {
    if (props.mode === 'create') {
      innerFormData.value = { status: 1, asrsEnabled: 0, totalBins: 0 }
    } else {
      innerFormData.value = { ...props.data }
    }
  }
})

const handleConfirm = (data) => {
  loading.value = true
  console.log('仓库提交:', data)
  setTimeout(() => {
    loading.value = false
    visible.value = false
    KhMessageFn.success('操作成功')
    emit('success')
  }, 500)
}
</script>
