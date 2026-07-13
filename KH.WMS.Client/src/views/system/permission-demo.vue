<template>
  <KhPage
    ref="pageRef"
    module="user"
    :search-columns="searchColumns"
    :search-model="searchModel"
    :columns="tableColumns"
    :form-columns="formColumns"
    :show-stat-cards="false"
    :show-toolbar="true"
    :show-index="true"
    :show-selection="true"
    :show-header-filter="true"
    :search-col-count="3"
    :crud-operations="crudOperations"
    :permission-prefix="'system:user'"
    :before-delete="beforeDelete"
    :action-buttons="extraActionButtons"
  />
</template>

<script setup>
const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'username', label: '用户名', type: 'input', clearable: true },
  { prop: 'phone', label: '手机号', type: 'input', clearable: true },
  {
    prop: 'status', label: '状态', type: 'select', clearable: true,
    options: [
      { label: '正常', value: 1 },
      { label: '停用', value: 0 },
    ],
  },
]

const searchModel = reactive({ username: '', phone: '', status: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'username', label: '用户名', width: 120, searchable: true },
  { prop: 'realName', label: '姓名', width: 100, searchable: true },
  { prop: 'phone', label: '手机号', width: 140, searchable: true },
  { prop: 'roleName', label: '角色', width: 120,
    searchable: true, filterType: 'select',
    filterOptions: [
      { label: '管理员', value: '管理员' },
      { label: '仓库主管', value: '仓库主管' },
      { label: '库管员', value: '库管员' },
      { label: '调度员', value: '调度员' },
    ],
  },
  { prop: 'warehouse', label: '所属仓库', minWidth: 130,
    searchable: true, filterType: 'multiple-select',
    filterOptions: [
      { label: '华东仓', value: '华东仓' },
      { label: '华南仓', value: '华南仓' },
      { label: '西南仓', value: '西南仓' },
      { label: '华北仓', value: '华北仓' },
    ],
  },
  { prop: 'loginCount', label: '登录次数', width: 100, align: 'center',
    searchable: true, filterType: 'number-range',
  },
  { prop: 'lastLoginTime', label: '最后登录', width: 170,
    searchable: true, filterType: 'date-range',
  },
  {
    prop: 'status', label: '状态', width: 80, align: 'center',
    type: 'tag', tagMap: { 1: '正常', 0: '停用' }, tagTypeMap: { 1: 'success', 0: 'danger' },
    searchable: true, filterType: 'select',
    filterOptions: [
      { label: '正常', value: 1 },
      { label: '停用', value: 0 },
    ],
  },
]

// ==================== CRUD 配置 ====================

/** 开启查看详情 + 默认的增删改 */
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: true,
  export: false,
}

/** 删除前校验 */
const beforeDelete = (row) => {
  if (row.username === 'admin') {
    KhMessageFn.warning('管理员账号不允许删除')
    return false
  }
}

// ==================== 表单配置（新增/编辑弹窗） ====================
const formColumns = [
  { prop: 'username', label: '用户名', type: 'input', required: true, maxlength: 30 },
  { prop: 'realName', label: '姓名', type: 'input', required: true, maxlength: 20 },
  { prop: 'phone', label: '手机号', type: 'input', required: true, maxlength: 11 },
  {
    prop: 'roleName', label: '角色', type: 'select', required: true,
    options: [
      { label: '管理员', value: '管理员' },
      { label: '仓库主管', value: '仓库主管' },
      { label: '库管员', value: '库管员' },
      { label: '调度员', value: '调度员' },
    ],
  },
  { prop: 'status', label: '状态', type: 'switch' },
]

// ==================== 额外操作按钮（与内置按钮合并） ====================
const extraActionButtons = [
  {
    label: '重置密码',
    type: 'warning',
    permission: 'system:user:reset',
    show: (row) => row.status === 1,
    confirm: '确定重置该用户的密码?',
    onClick: (row) => KhMessageFn.success(`已重置用户 ${row.username} 的密码`),
  },
]
</script>
