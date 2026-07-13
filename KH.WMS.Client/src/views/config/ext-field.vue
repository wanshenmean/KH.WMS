<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage
      ref="pageRef"
      module="ext-field"
      title="通用扩展字段配置"
      :search-columns="searchColumns"
      :search-model="searchModel"
      :columns="tableColumns"
      :show-stat-cards="false"
      :show-toolbar="true"
      :show-index="true"
      :show-header-filter="true"
      :search-col-count="3"
      :crud-operations="crudOperations"
      :permission-prefix="'cfg:ext_field'"
      :form-columns="formColumns"
      :custom-form-data="formData"
    />
  </div>
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('ext-field')

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'entityTypeId', label: '实体类型', type: 'select', clearable: true, options: 'dict:ext_field_type' },
  { prop: 'fieldKey', label: '字段标识', type: 'input', clearable: true },
  { prop: 'fieldLevel', label: '作用层级', type: 'select', clearable: true, options: 'dict:doc_field_level' },
]

const searchModel = reactive({ entityTypeId: '', fieldKey: '', fieldLevel: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'entityTypeId', label: '实体类型', width: 150, type: 'tag', tagMap: 'dict:ext_field_type' },
  { prop: 'fieldKey', label: '字段标识', width: 150 },
  { prop: 'fieldName', label: '显示名称', width: 120 },
  {
    prop: 'fieldType', label: '字段类型', width: 100, align: 'center',
    type: 'tag', tagMap: 'dict:doc_field_type',
  },
  {
    prop: 'fieldLevel', label: '作用层级', width: 90, align: 'center',
    type: 'tag',
    tagMap: { HEADER: '主表', LINE: '子表' },
    tagTypeMap: { HEADER: 'primary', LINE: 'success' },
  },
  {
    prop: 'isRequired', label: '必填', width: 70, align: 'center',
    type: 'tag', tagMap: { 1: '是', 0: '否' }, tagTypeMap: { 1: 'danger', 0: 'info' },
  },
  {
    prop: 'isProcessable', label: '参与业务', width: 90, align: 'center',
    type: 'tag', tagMap: { 1: '是', 0: '否' }, tagTypeMap: { 1: 'warning', 0: 'info' },
  },
  { prop: 'defaultValue', label: '默认值', width: 120, showOverflowTooltip: true },
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
  {
    prop: 'entityTypeId', label: '实体类型', type: 'select', required: true,
    options: 'dict:ext_field_type', placeholder: '请选择实体类型',
  },
  { prop: 'fieldKey', label: '字段标识', type: 'input', required: true, maxlength: 50, placeholder: '如 specification、taxId' },
  { prop: 'fieldName', label: '显示名称', type: 'input', required: true, maxlength: 100, placeholder: '如 规格型号、税号' },
  { prop: 'fieldType', label: '字段类型', type: 'select', required: true, options: 'dict:doc_field_type' },
  { prop: 'fieldLevel', label: '作用层级', type: 'select', required: true, options: 'dict:doc_field_level' },
  { prop: 'isRequired', label: '是否必填', type: 'switch', defaultValue: 0, activeValue: 1, inactiveValue: 0 },
  { prop: 'isProcessable', label: '参与业务处理', type: 'switch', defaultValue: 0, activeValue: 1, inactiveValue: 0, span: 24 },
  { prop: 'defaultValue', label: '默认值', type: 'input', maxlength: 200, placeholder: '选填，新建时的默认值' },
  { prop: 'sortOrder', label: '排序号', type: 'number', min: 0, placeholder: '值越小越靠前' },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 500 },
]

const formData = reactive({
  entityTypeId: null,
  fieldKey: '',
  fieldName: '',
  fieldType: 'STRING',
  fieldLevel: 'HEADER',
  isRequired: 0,
  isProcessable: 0,
  defaultValue: '',
  sortOrder: 0,
  remark: '',
})
</script>
