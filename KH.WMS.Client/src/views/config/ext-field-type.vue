<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage
      ref="pageRef"
      module="ext-field-type"
      title="扩展字段实体类型"
      :search-columns="searchColumns"
      :search-model="searchModel"
      :columns="tableColumns"
      :show-stat-cards="false"
      :show-toolbar="true"
      :show-index="true"
      :show-header-filter="true"
      :search-col-count="3"
      :crud-operations="crudOperations"
      :permission-prefix="'cfg:ext_field_type'"
      :form-columns="formColumns"
      :custom-form-data="formData"
      :action-buttons="extraActionButtons"
    />
  </div>
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('ext-field-type')

// ==================== 搜索 ====================
const categoryOptions = [
  { label: '库存操作', value: 'INVENTORY' },
  { label: '基础数据', value: 'BASEDATA' },
]

const searchColumns = [
  { prop: 'entityCode', label: '实体编码', type: 'input', clearable: true },
  { prop: 'entityName', label: '实体名称', type: 'input', clearable: true },
  { prop: 'entityCategory', label: '实体分类', type: 'select', clearable: true, options: categoryOptions },
]

const searchModel = reactive({ entityCode: '', entityName: '', entityCategory: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'entityCode', label: '实体编码', width: 180 },
  { prop: 'entityName', label: '实体名称', width: 150 },
  {
    prop: 'entityCategory', label: '实体分类', width: 110, align: 'center',
    type: 'tag',
    tagMap: { INVENTORY: '库存操作', BASEDATA: '基础数据' },
    tagTypeMap: { INVENTORY: 'warning', BASEDATA: 'primary' },
  },
  {
    prop: 'hasLineLevel', label: '行级扩展', width: 90, align: 'center',
    type: 'tag', tagMap: { 1: '是', 0: '否' }, tagTypeMap: { 1: 'success', 0: 'info' },
  },
  {
    prop: 'isActive', label: '启用', width: 70, align: 'center',
    type: 'tag', tagMap: { 1: '是', 0: '否' }, tagTypeMap: { 1: 'success', 0: 'info' },
  },
  { prop: 'sortOrder', label: '排序', width: 70, align: 'center' },
  { prop: 'remark', label: '备注', minWidth: 120, showOverflowTooltip: true },
]

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: true,
  export: false,
}

// ==================== 表单配置 ====================
const formColumns = [
  { prop: 'entityCode', label: '实体编码', type: 'input', required: true, maxlength: 50, placeholder: '如 INV_ADJUST、MD_MATERIAL' },
  { prop: 'entityName', label: '实体名称', type: 'input', required: true, maxlength: 100, placeholder: '如 库存调整、物料' },
  { prop: 'entityCategory', label: '实体分类', type: 'select', required: true, options: categoryOptions, placeholder: '请选择实体分类' },
  { prop: 'hasLineLevel', label: '行级扩展字段', type: 'switch', defaultValue: 0, activeValue: 1, inactiveValue: 0 },
  { prop: 'isActive', label: '是否启用', type: 'switch', defaultValue: 1, activeValue: 1, inactiveValue: 0 },
  { prop: 'sortOrder', label: '排序号', type: 'number', min: 0, placeholder: '值越小越靠前' },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 500 },
]

const formData = reactive({
  entityCode: '',
  entityName: '',
  entityCategory: '',
  hasLineLevel: 0,
  isActive: 1,
  sortOrder: 0,
  remark: '',
})

// ==================== 额外操作按钮 ====================
const extraActionButtons = [
  {
    label: (row) => row.isActive === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'cfg:ext_field_type:toggle',
    confirm: (row) => `确定要${row.isActive === 1 ? '禁用' : '启用'}吗？`,
    onClick: async (row) => {
      const currentStatus = row.isActive
      const newStatus = currentStatus === 1 ? 0 : 1
      const res = await crudApi.setStatus(row.id, newStatus)
      if (res.code === 200) {
        KhMessageFn.success(res.message)
        pageRef.value?.reload()
      }
    },
  },
]
</script>
