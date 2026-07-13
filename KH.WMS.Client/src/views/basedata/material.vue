<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage ref="pageRef" module="material" title="物料管理" :search-columns="searchColumns" :search-model="searchModel"
      :columns="tableColumns" :show-stat-cards="false" :show-toolbar="true" :show-index="true" :show-selection="true"
      :show-header-filter="true" :search-col-count="3" :crud-operations="crudOperations"
      :permission-prefix="'bd:material'"
      :form-columns="formColumns" :custom-form-data="formDialogData" :action-width="'120'"
      :before-submit="beforeSubmit" :load="load" :action-buttons="extraActionButtons" />

  </div>
</template>

<script setup>
import { useExtFields } from '@/utils/useExtFields'
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('material')

const extraActionButtons = [
  {
    label: (row) => row.status === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'bd:material:toggle',
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

// ==================== 扩展字段 ====================
const { loadExtConfig, mergedColumns, mergedTableColumns, extractAndCleanExtData, withFlatExtLoad } = useExtFields('/api/material/form-config')

const searchColumns = [
  { prop: 'materialCode', label: '物料编码', type: 'input', clearable: true, placeholder: '请输入物料编码' },
  { prop: 'materialName', label: '物料名称', type: 'input', clearable: true, placeholder: '请输入物料名称' },
  { prop: 'categoryId', label: '物料分类', type: 'select', clearable: true, options: 'dict:material_category' },
  { prop: 'status', label: '状态', type: 'select', clearable: true, options: 'dict:status_flag' },
]

const load = withFlatExtLoad(crudApi, searchColumns)

onMounted(() => {
  loadExtConfig()
})

// ==================== 扩展字段提交处理 ====================
const beforeSubmit = (data) => {
  const raw = extractAndCleanExtData(data)
  if (raw) data.extDataRaw = raw
}

const searchModel = reactive({
  materialCode: '',
  materialName: '',
  categoryId: '',
  status: '',
})

// ==================== 表格列 ====================
const baseTableColumns = [
  { prop: 'materialCode', label: '物料编码', width: 130 },
  { prop: 'materialName', label: '物料名称', minWidth: 150 },
  { prop: 'categoryId', label: '物料分类', width: 120, type: 'tag', tagMap: 'dict:material_category' },
  { prop: 'baseUnitId', label: '基本单位', width: 100, type: 'tag', tagMap: 'dict:material_unit' },
  { prop: 'isBatchManaged', label: '批管理', width: 90, align: 'center', type: 'tag', tagMap: 'dict:status_flag' },
  { prop: 'isSerialManaged', label: '序列号管理', width: 100, align: 'center', type: 'tag', tagMap: 'dict:status_flag' },
  { prop: 'hasExpiry', label: '有效期', width: 80, align: 'center', type: 'tag', tagMap: 'dict:status_flag' },
  { prop: 'shelfLifeDays', label: '保质期(天)', width: 100, align: 'right' },
  { prop: 'turnoverClass', label: '周转分类', width: 100, type: 'tag', tagMap: 'dict:turnover_classification' },
  { prop: 'status', label: '状态', width: 90, align: 'center', type: 'tag', tagMap: 'dict:status_flag' },
  { prop: 'remark', label: '备注', minWidth: 150, showOverflowTooltip: true },
]

const tableColumns = computed(() => mergedTableColumns(baseTableColumns))

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: true,
  export: true,
}

// ==================== 表单配置（新增/编辑弹窗） ====================
const baseFormColumns = [
  { prop: 'materialCode', label: '物料编码', type: 'input', required: true, maxlength: 20, placeholder: '请输入物料编码' },
  { prop: 'materialName', label: '物料名称', type: 'input', required: true, maxlength: 100, placeholder: '请输入物料名称' },
  { prop: 'categoryId', label: '物料分类', type: 'select', required: true, options: 'dict:material_category', placeholder: '请选择物料分类' },
  { prop: 'baseUnitId', label: '基本单位', type: 'select', required: true, options: 'dict:material_unit', placeholder: '请选择基本单位' },
  { prop: 'isBatchManaged', label: '批管理', type: 'switch', activeValue: 1, inactiveValue: 0 },
  { prop: 'isSerialManaged', label: '序列号管理', type: 'switch', activeValue: 1, inactiveValue: 0 },
  { prop: 'hasExpiry', label: '有效期管理', type: 'switch', activeValue: 1, inactiveValue: 0 },
  { prop: 'shelfLifeDays', label: '保质期(天)', type: 'number', min: 0, precision: 0, placeholder: '有效天数' },
  { prop: 'turnoverClass', label: '周转分类', type: 'select', options: 'dict:turnover_classification', placeholder: '请选择周转分类' },
  { prop: 'status', label: '状态', type: 'switch', activeValue: 1, inactiveValue: 0 },
  { prop: 'remark', label: '备注', type: 'textarea', maxlength: 200, rows: 3 },
]

const formColumns = computed(() => mergedColumns(baseFormColumns))

const formDialogData = reactive({
  materialCode: '',
  materialName: '',
  categoryId: null,
  baseUnitId: null,
  isBatchManaged: null,
  isSerialManaged: null,
  hasExpiry: null,
  shelfLifeDays: null,
  turnoverClass: '',
  status: '1',
  remark: '',
})
</script>
