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
  pageTitle: { type: String, default: '物料管理' },
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
  { prop: 'materialCode', label: '物料编码', type: 'input', required: true, maxlength: 20 },
  { prop: 'materialName', label: '物料名称', type: 'input', required: true, maxlength: 100 },
  { prop: 'spec', label: '规格型号', type: 'input', required: true, maxlength: 100 },
  { prop: 'unit', label: '计量单位', type: 'select', required: true, options: [
    { label: '个', value: '个' },
    { label: '只', value: '只' },
    { label: '片', value: '片' },
    { label: '箱', value: '箱' },
    { label: 'kg', value: 'kg' },
    { label: '卷', value: '卷' },
    { label: '包', value: '包' },
    { label: '桶', value: '桶' },
    { label: '瓶', value: '瓶' },
  ]},
  {
    prop: 'category', label: '物料分类', type: 'select', required: true,
    options: [
      { label: '电子元器件', value: '电子元器件' },
      { label: '芯片', value: '芯片' },
      { label: '电阻电容', value: '电阻电容' },
      { label: '机械五金', value: '机械五金' },
      { label: '紧固件', value: '紧固件' },
      { label: '包装材料', value: '包装材料' },
      { label: '化工材料', value: '化工材料' },
    ],
  },
  {
    prop: 'abcClass', label: 'ABC分类', type: 'select', required: true,
    options: [
      { label: 'A类（重点管理）', value: 'A' },
      { label: 'B类（一般管理）', value: 'B' },
      { label: 'C类（简单管理）', value: 'C' },
    ],
  },
  {
    prop: 'storageType', label: '存储类型', type: 'select', required: true,
    options: [
      { label: '常温', value: '常温' },
      { label: '冷藏', value: '冷藏' },
      { label: '冷冻', value: '冷冻' },
      { label: '防潮', value: '防潮' },
      { label: '防静电', value: '防静电' },
    ],
  },
  { prop: 'weight', label: '重量(kg)', type: 'number', min: 0, precision: 4 },
  { prop: 'safetyStock', label: '安全库存', type: 'number', required: true, min: 0 },
  { prop: 'currentStock', label: '当前库存', type: 'number', min: 0 },
  { prop: 'status', label: '状态', type: 'radio', required: true, options: [{ label: '启用', value: 1 }, { label: '停用', value: 0 }], defaultValue: 1 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]

const innerFormData = ref({})

watch(() => props.modelValue, (val) => {
  if (val) {
    if (props.mode === 'create') {
      innerFormData.value = { status: 1, weight: 0, safetyStock: 0, currentStock: 0 }
    } else {
      innerFormData.value = { ...props.data }
    }
  }
})

const handleConfirm = (data) => {
  loading.value = true
  console.log('物料提交:', data)
  setTimeout(() => {
    loading.value = false
    visible.value = false
    KhMessageFn.success('操作成功')
    emit('success')
  }, 500)
}
</script>
