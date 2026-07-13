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
import { getRoleOptions } from '@/api/system'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  mode: { type: String, default: 'create' },
  data: { type: Object, default: () => ({}) },
  pageTitle: { type: String, default: '角色管理' },
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

const roleOptions = ref([])

const formColumns = computed(() => [
  { prop: 'roleName', label: '角色名称', type: 'input', required: true, maxlength: 30 },
  { prop: 'roleCode', label: '角色编码', type: 'input', required: true, maxlength: 50 },
  {
    prop: 'parentId',
    label: '上级角色',
    type: 'cascader',
    options: roleOptions.value,
    cascaderProps: { checkStrictly: true, value: 'value', label: 'label', children: 'children', emitPath: false },
    clearable: true,
    filterable: true,
  },
  { prop: 'sortNo', label: '排序', type: 'number', min: 1 },
  { prop: 'status', label: '状态', type: 'radio', required: true, options: [{ label: '启用', value: 1 }, { label: '停用', value: 0 }] },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
])

const loadRoleTree = async () => {
  try {
    const res = await getRoleOptions()
    if (res.code === 200 && res.data) {
      roleOptions.value = buildTree(res.data)
    }
  } catch (e) {
    console.error('加载角色列表失败:', e)
  }
}

/** 将扁平角色列表构建为树形结构 */
const buildTree = (roles) => {
  const map = {}
  const roots = []
  roles.forEach(r => { map[r.id] = { ...r, value: r.id, label: r.roleName, children: [] } })
  Object.values(map).forEach(node => {
    if (node.parentId && map[node.parentId]) {
      map[node.parentId].children.push(node)
    } else {
      roots.push(node)
    }
  })
  return roots
}

const innerFormData = ref({})

watch(() => props.modelValue, (val) => {
  if (val) {
    loadRoleTree()
    if (props.mode === 'create') {
      innerFormData.value = { status: 1, sortNo: 1 }
    } else {
      innerFormData.value = { ...props.data }
    }
  }
})

const handleConfirm = (data) => {
  loading.value = true
  console.log('角色提交:', data)
  setTimeout(() => {
    loading.value = false
    visible.value = false
    KhMessageFn.success('操作成功')
    emit('success')
  }, 500)
}
</script>
