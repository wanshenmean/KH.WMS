<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage ref="pageRef" module="role" title="角色管理" :search-columns="searchColumns" :search-model="searchModel"
      :columns="tableColumns" :form-columns="formColumns" :show-stat-cards="false" :show-toolbar="true"
      :show-index="true" :crud-operations="crudOperations" :permission-prefix="'sys:role'"
      :toolbar-buttons="extraToolbarButtons" :action-buttons="extraActionButtons" :before-submit="beforeSubmit">
    </KhPage>

    <PermissionAssignDialog v-model="permDialogVisible" :role-data="permRoleData" @success="handleDialogSuccess" />
  </div>
</template>

<script setup>
import KhPage from '@/components/KhPage/index.vue'
import PermissionAssignDialog from './components/PermissionAssignDialog.vue'
import { getRoleOptions } from '@/api/system'
import { useCrudApi } from '@/utils/crud'

const Icons = markRaw({ Plus })
const pageRef = ref(null)
const crudApi = useCrudApi('role')

const handleDialogSuccess = () => {
  pageRef.value?.reload()
}

// ---- 角色树选项（级联选择器） ----
const roleOptions = ref([])

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

/** 扁平角色映射（id → 角色名），用于表格 formatter */
const roleMap = computed(() => {
  const map = {}
  const flatten = (nodes) => {
    nodes.forEach(n => {
      map[n.value] = n.label
      if (n.children?.length) flatten(n.children)
    })
  }
  flatten(roleOptions.value)
  return map
})

const getParentRoleName = (parentId) => {
  if (!parentId) return '-'
  return roleMap.value[parentId] || parentId
}

onMounted(() => {
  loadRoleTree()
})

// ---- 搜索 ----
const searchColumns = [
  { prop: 'roleName', label: '角色名称', type: 'input', clearable: true },
  { prop: 'roleCode', label: '角色编码', type: 'input', clearable: true },
  {
    prop: 'status', label: '状态', type: 'select', clearable: true,
    options: 'dict:status_flag',
  },
]

const searchModel = reactive({ roleName: '', roleCode: '', status: '' })

// ---- 表格列 ----
const tableColumns = [
  { prop: 'roleName', label: '角色名称', width: 140 },
  { prop: 'roleCode', label: '角色编码', width: 140 },
  { prop: 'parentId', label: '上级角色', width: 140, formatter: (row) => getParentRoleName(row.parentId) },
  { prop: 'sortNo', label: '排序', width: 80, align: 'center' },
  { prop: 'dataScope', label: '数据范围', width: 100, align: 'center' },
  {
    prop: 'status', label: '状态', width: 80, align: 'center',
    type: 'tag', tagMap: 'dict:status_flag',
  },
  { prop: 'remark', label: '备注', minWidth: 200, showOverflowTooltip: true },
]

// ---- 表单列 ----
const formColumns = computed(() => [
  { prop: 'roleName', label: '角色名称', type: 'input', required: true, maxlength: 30, span: 12 },
  { prop: 'roleCode', label: '角色编码', type: 'input', required: true, maxlength: 50, span: 12 },
  {
    prop: 'parentId',
    label: '上级角色',
    type: 'cascader',
    options: roleOptions.value,
    cascaderProps: { checkStrictly: true, value: 'value', label: 'label', children: 'children', emitPath: false },
    clearable: true,
    filterable: true,
    span: 12,
  },
  { prop: 'sortNo', label: '排序', type: 'number', min: 1, span: 12 },
  { prop: 'dataScope', label: '数据范围', type: 'number', span: 12 },
  {
    prop: 'status', label: '状态', type: 'radio', required: true,
    options: [{ label: '启用', value: 1 }, { label: '停用', value: 0 }], span: 12,
  },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
])

// ---- CRUD 配置 ----
const crudOperations = {
  create: false,
  update: true,
  delete: true,
  view: true,
  export: false,
}

const beforeSubmit = (data) => {
  if (!data.parentId) data.parentId = 0
}

// ---- 额外工具栏按钮 ----
const extraToolbarButtons = [
  {
    label: '新增',
    icon: Icons.Plus,
    type: 'primary',
    onClick: () => pageRef.value?.openCreateDialog(),
  },
]

// ---- 额外操作按钮 ----
const extraActionButtons = [
  {
    label: '分配权限',
    type: 'success',
    link: true,
    size: 'small',
    onClick: (row) => handleAssignPermission(row),
  },
  {
    label: (row) => row.status === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'sys:role:toggle',
    confirm: (row) => `确定要${row.status === 1 ? '禁用' : '启用'}吗？`,
    onClick: async (row) => {
      const newStatus = row.status === 1 ? 0 : 1
      const res = await crudApi.setStatus(row.id, newStatus)
      if (res.code === 200) {
        KhMessageFn.success(res.message)
        pageRef.value?.reload()
      }
    },
  },
]

// ---- 权限分配弹窗 ----
const permDialogVisible = ref(false)
const permRoleData = ref({})

const handleAssignPermission = (row) => {
  permRoleData.value = { ...row }
  permDialogVisible.value = true
}
</script>
