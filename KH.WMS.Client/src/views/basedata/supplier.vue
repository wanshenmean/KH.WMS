<template>
  <KhPage
    ref="pageRef"
    module="supplier"
    title="供应商管理"
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
    :permission-prefix="'bd:supplier'"
    :before-submit="beforeSubmit"
    :load="load"
    :action-buttons="extraActionButtons"
  />
</template>

<script setup>
import { useExtFields } from '@/utils/useExtFields'
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('supplier')

const extraActionButtons = [
  {
    label: (row) => row.status === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'bd:supplier:toggle',
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
const { loadExtConfig, mergedColumns, mergedTableColumns, extractAndCleanExtData, withFlatExtLoad } = useExtFields('/api/supplier/form-config')

const searchColumns = [
  { prop: 'supplierCode', label: '供应商编码', type: 'input', clearable: true, placeholder: '请输入供应商编码' },
  { prop: 'supplierName', label: '供应商名称', type: 'input', clearable: true, placeholder: '请输入供应商名称' },
  {
    prop: 'status', label: '状态', type: 'select', clearable: true,
    options: 'dict:status_flag',
  },
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

const searchModel = reactive({ supplierCode: '', supplierName: '', status: '' })

// ==================== 表格列 ====================
const baseTableColumns = [
  { prop: 'supplierCode', label: '供应商编码', width: 140 },
  { prop: 'supplierName', label: '供应商名称', minWidth: 180 },
  { prop: 'contact', label: '联系人', width: 100 },
  { prop: 'contactPhone', label: '联系电话', width: 140 },
  { prop: 'address', label: '地址', minWidth: 200, showOverflowTooltip: true },
  {
    prop: 'status', label: '状态', width: 90, align: 'center',
    type: 'tag', tagMap: 'dict:status_flag',
  },
]

const tableColumns = computed(() => mergedTableColumns(baseTableColumns))

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: false,
  export: true,
}

// ==================== 表单配置（新增/编辑弹窗） ====================
const baseFormColumns = [
  { prop: 'supplierCode', label: '供应商编码', type: 'input', required: true, placeholder: '请输入供应商编码' },
  { prop: 'supplierName', label: '供应商名称', type: 'input', required: true, placeholder: '请输入供应商名称' },
  { prop: 'contact', label: '联系人', type: 'input', placeholder: '请输入联系人' },
  { prop: 'contactPhone', label: '联系电话', type: 'input', placeholder: '请输入联系电话' },
  { prop: 'address', label: '地址', type: 'textarea', maxlength: 300, rows: 2 },
  { prop: 'status', label: '状态', type: 'switch', activeValue: 1, inactiveValue: 0 },
  { prop: 'remark', label: '备注', type: 'textarea', maxlength: 200, rows: 3 },
]

const formColumns = computed(() => mergedColumns(baseFormColumns))
</script>
