<template>
  <KhPage
    ref="pageRef"
    module="turnover-class"
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
    :permission-prefix="'bd:turnover_class'"
    :action-buttons="extraActionButtons"
  />
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('turnover-class')

const extraActionButtons = [
  {
    label: (row) => row.status === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'bd:turnover_class:toggle',
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
  { prop: 'classCode', label: '分类编码', type: 'input', clearable: true, placeholder: '请输入分类编码' },
  { prop: 'className', label: '分类名称', type: 'input', clearable: true, placeholder: '请输入分类名称' },
  { prop: 'status', label: '状态', type: 'select', clearable: true, options: 'dict:status_flag' },
]

const searchModel = reactive({ classCode: '', className: '', status: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'classCode', label: '分类编码', width: 100 },
  { prop: 'className', label: '分类名称', minWidth: 140 },
  { prop: 'cumulativeRatioMin', label: '占比下限(%)', width: 120, align: 'right' },
  { prop: 'cumulativeRatioMax', label: '占比上限(%)', width: 120, align: 'right' },
  {
    prop: 'analysisDimension', label: '分析维度', width: 130, align: 'center',
    type: 'tag', tagMap: { OUTBOUND_QTY: '出库数量', OUTBOUND_FREQ: '出库频次' },
  },
  { prop: 'color', label: '颜色标识', width: 100, align: 'center' },
  { prop: 'sortNo', label: '排序', width: 80, align: 'center' },
  { prop: 'status', label: '状态', width: 90, align: 'center', type: 'tag', tagMap: 'dict:status_flag' },
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
  { prop: 'classCode', label: '分类编码', type: 'input', required: true, maxlength: 10, placeholder: '如 A、B、C' },
  { prop: 'className', label: '分类名称', type: 'input', required: true, maxlength: 50, placeholder: '如 高频物料' },
  { prop: 'cumulativeRatioMin', label: '占比下限(%)', type: 'number', required: true, min: 0, max: 100, precision: 2, placeholder: '累计占比下限', labelWidth: 150 },
  { prop: 'cumulativeRatioMax', label: '占比上限(%)', type: 'number', required: true, min: 0, max: 100, precision: 2, placeholder: '累计占比上限', labelWidth: 150 },
  {
    prop: 'analysisDimension', label: '分析维度', type: 'select', required: true,
    options: [{ label: '出库数量', value: 'OUTBOUND_QTY' }, { label: '出库频次', value: 'OUTBOUND_FREQ' }],
    placeholder: '请选择分析维度',
  },
  { prop: 'color', label: '颜色标识', type: 'input', maxlength: 20, placeholder: '前端展示颜色，如 #FF4D4F' },
  { prop: 'sortNo', label: '排序号', type: 'number', min: 0, precision: 0, placeholder: '请输入排序号' },
  { prop: 'status', label: '状态', type: 'switch', activeValue: 1, inactiveValue: 0 },
]
</script>
