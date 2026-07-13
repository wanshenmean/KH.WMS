<template>
  <KhPage
    ref="pageRef"
    module="container-type"
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
    :permission-prefix="'bd:container_type'"
    title="容器类型"
    :action-buttons="extraActionButtons"
  />
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('container-type')

const extraActionButtons = [
  {
    label: (row) => row.status === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'bd:container_type:toggle',
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

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'typeCode', label: '类型编码', type: 'input', clearable: true, placeholder: '请输入类型编码' },
  { prop: 'typeName', label: '类型名称', type: 'input', clearable: true, placeholder: '请输入类型名称' },
  {
    prop: 'status', label: '状态', type: 'select', clearable: true,
    options: 'dict:status_flag',
  },
]

const searchModel = reactive({ typeCode: '', typeName: '', status: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'typeCode', label: '类型编码', width: 140 },
  { prop: 'typeName', label: '类型名称', minWidth: 150 },
  {
    prop: 'status', label: '状态', width: 90, align: 'center',
    type: 'tag', tagMap: 'dict:status_flag',
  },
]

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: false,
  export: true,
}

// ==================== 表单配置（新增/编辑弹窗） ====================
const formColumns = [
  { prop: 'typeCode', label: '类型编码', type: 'input', required: true, placeholder: '请输入类型编码' },
  { prop: 'typeName', label: '类型名称', type: 'input', required: true, placeholder: '请输入类型名称' },
  { prop: 'status', label: '状态', type: 'switch', activeValue: 1, inactiveValue: 0 },
  { prop: 'remark', label: '备注', type: 'textarea', maxlength: 200, rows: 3 },
]
</script>
