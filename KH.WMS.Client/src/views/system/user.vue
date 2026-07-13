<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage ref="pageRef" module="user" title="用户管理" :search-columns="searchColumns" :search-model="searchModel"
      :columns="tableColumns" :show-stat-cards="false" :show-toolbar="true" :show-index="true" :show-selection="true"
      :show-header-filter="true" :search-col-count="3" :crud-operations="crudOperations" :permission-prefix="'sys:user'"
      :before-delete="beforeDelete" :action-buttons="extraActionButtons" :toolbar-buttons="extraToolbarButtons"
      :form-columns="formColumns" />

    <UserFormDialog v-model="dialogVisible" :mode="formDialogMode" :data="formDialogData"
      @success="handleDialogSuccess" />
  </div>
</template>

<script setup>
import UserFormDialog from './components/UserFormDialog.vue'
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('user')

// 添加处理函数
const handleDialogSuccess = () => {
  pageRef.value?.reload()
}

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'userName', label: '用户名', type: 'input', clearable: true },
  { prop: 'realName', label: '姓名', type: 'input', clearable: true },
  {
    prop: 'status', label: '状态', type: 'select', clearable: true,
    options: 'dict:status_flag',
  },
]

const searchModel = reactive({
  userName: '',
  realName: '',
  status: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'userName', label: '用户名', width: 130 },
  { prop: 'realName', label: '姓名', width: 120 },
  {
    prop: 'roleNames', label: '角色', minWidth: 140,
    formatter: (row) => (row.roleNames || []).join('、'),
  },
  {
    prop: 'isSystem', label: '系统用户', width: 90, align: 'center',
    type: 'tag', tagMap: { 1: '是', 0: '否' }, tagTypeMap: { 1: 'warning', 0: 'info' },
  },
  {
    prop: 'status', label: '状态', width: 80, align: 'center',
    type: 'tag', tagMap: 'dict:status_flag',
  },
  { prop: 'loginIp', label: '登录IP', width: 140 },
  { prop: 'loginTime', label: '最后登录', width: 170 },
  { prop: 'remark', label: '备注', minWidth: 180, showOverflowTooltip: true },
]

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: true,
  export: true,
}

// ==================== 表单配置（新增/编辑弹窗） ====================
const formColumns = [
  { prop: 'userName', label: '用户名', type: 'input', required: true },
  { prop: 'realName', label: '姓名', type: 'input', required: true },
  { prop: 'roleIds', label: '角色', type: 'select', required: true, options: 'dict:roleOptions' },
  { prop: 'status', label: '状态', type: 'radio', required: true, options: 'dict:status_flag' },
  { prop: 'remark', label: '备注', type: 'textarea' }
]

const beforeDelete = (row) => {
  if (row.isSystem === 1) {
    KhMessageFn.warning('系统内置用户不允许删除')
    return false
  }
}

// ==================== 额外操作按钮 ====================
const extraActionButtons = [
  {
    label: '重置密码',
    type: 'warning',
    permission: 'sys:user:reset_pwd',
    confirm: '确定重置该用户密码?',
    onClick: async (row) => {
      const { resetUserPassword } = await import('@/api/system')
      const res = await resetUserPassword(row.id)
      if (res.code === 200) {
        KhMessageFn.success(`已重置用户 ${row.realName} 的密码`)
      } else {
        KhMessageFn.error(res.message || '重置密码失败')
      }
    },
  },
  {
    label: (row) => row.status === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'sys:user:toggle',
    show: (row) => true,
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

// ==================== 额外工具栏按钮 ====================
const extraToolbarButtons = [

]

// ==================== UserFormDialog（保留独立组件） ====================
const dialogVisible = ref(false)
const formDialogMode = ref('create')
const formDialogData = ref({})
</script>
