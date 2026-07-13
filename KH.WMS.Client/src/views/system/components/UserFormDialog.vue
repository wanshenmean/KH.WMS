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
import { createUser, updateUser } from '@/api/system'
import KhDialog from '@/components/KhDialog/index.vue'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  mode: { type: String, default: 'create' },
  data: { type: Object, default: () => ({}) },
  pageTitle: { type: String, default: '用户管理' },
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

const formColumns = computed(() => {
  const cols = [
    { prop: 'userName', label: '用户名', type: 'input', required: true, maxlength: 30 },
    { prop: 'realName', label: '姓名', type: 'input', required: true, maxlength: 30 },
    { prop: 'roleIds', label: '角色', type: 'select', required: true, multiple: true, options: 'roleOptions' },
  ]
  if (props.mode === 'create') {
    cols.push(
      { prop: 'password', label: '密码', type: 'input', required: true, maxlength: 20, span: 12 },
      { prop: 'confirmPassword', label: '确认密码', type: 'input', required: true, maxlength: 20, span: 12 },
    )
  }
  cols.push(
    { prop: 'status', label: '状态', type: 'radio', required: true, options: [{ label: '启用', value: 1 }, { label: '停用', value: 0 }] },
    { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
  )
  return cols
})

const innerFormData = ref({})

watch(() => props.modelValue, (val) => {
  if (val) {
    if (props.mode === 'create') {
      innerFormData.value = { status: 1, roleIds: [] }
    } else {
      innerFormData.value = { ...props.data }
    }
  }
})

const handleConfirm = async (data) => {
  if (props.mode === 'create' && data.password !== data.confirmPassword) {
    KhMessageFn.error('两次输入的密码不一致')
    return
  }
  loading.value = true
  try {
    let res
    if (props.mode === 'create') {
      res = await createUser(data)
    } else {
      res = await updateUser(data)
    }
    if (res.code === 200) {
      visible.value = false
      KhMessageFn.success('操作成功')
      emit('success')
    }
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}
</script>
