<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage ref="pageRef" module="document-type" title="单据类型配置" :search-columns="searchColumns" :search-model="searchModel"
      :columns="tableColumns" :form-columns="formColumns" :show-stat-cards="false" :show-toolbar="true"
      :show-index="true" :show-selection="true" :show-header-filter="true" :search-col-count="3"
      :custom-form-data="formDialogData" :crud-operations="crudOperations" :permission-prefix="'cfg:doc_type'"
      :action-buttons="actionButtons" />

    <!-- 流程配置弹窗 -->
    <KhDialog v-model="processDialogVisible" title="流程控制配置" width="600px" destroy-on-close
      :confirm-loading="processSubmitLoading" @confirm="handleProcessSubmit" @close="resetProcessForm">
      <template #default>
        <KhForm ref="processFormRef" :columns="processFormColumns" v-model="processFormData" :label-width="'110px'"
          :col-count="2" />
      </template>
    </KhDialog>

    <!-- 规则配置弹窗 -->
    <KhDialog v-model="ruleDialogVisible" title="业务规则配置" width="600px" destroy-on-close
      :confirm-loading="ruleSubmitLoading" @confirm="handleRuleSubmit" @close="resetRuleForm">
      <template #default>
        <KhForm ref="ruleFormRef" :columns="ruleFormColumns" v-model="ruleFormData" :label-width="'110px'"
          :col-count="2" />
      </template>
    </KhDialog>

  </div>
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('document-type')

// ==================== 子表 CRUD ====================
const processApi = useCrudApi('cfg-document-type-process')
const ruleApi = useCrudApi('cfg-document-type-rule')

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'typeCode', label: '类型编码', type: 'input', clearable: true },
  { prop: 'typeName', label: '类型名称', type: 'input', clearable: true },
  {
    prop: 'typeCategory', label: '类型分类', type: 'select', clearable: true,
    options: [
      { label: '入库', value: 'INBOUND' },
      { label: '出库', value: 'OUTBOUND' },
      { label: '调拨', value: 'TRANSFER' },
      { label: '盘点', value: 'INVENTORY' },
    ],
  },
]

const searchModel = reactive({ typeCode: '', typeName: '', typeCategory: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'typeCode', label: '类型编码', width: 150 },
  { prop: 'typeName', label: '类型名称', width: 160 },
  {
    prop: 'typeCategory', label: '类型分类', width: 100, align: 'center',
    type: 'tag',
    tagMap: { INBOUND: '入库', OUTBOUND: '出库', TRANSFER: '调拨', INVENTORY: '盘点' },
    tagTypeMap: { INBOUND: 'success', OUTBOUND: 'warning', TRANSFER: 'info', INVENTORY: 'danger' },
  },
  {
    prop: 'isActive', label: '状态', width: 80, align: 'center',
    type: 'tag',
    tagMap: { 0: '禁用', 1: '启用' },
    tagTypeMap: { 1: 'success', 0: 'danger' },
  },
  { prop: 'remark', label: '备注', minWidth: 180, showOverflowTooltip: true },
]

// ==================== 操作按钮 ====================
const actionButtons = [
  {
    label: '流程配置',
    type: 'primary',
    permission: 'cfg:doc_type:process',
    onClick: (row) => handleOpenProcess(row),
  },
  {
    label: '规则配置',
    permission: 'cfg:doc_type:rule',
    onClick: (row) => handleOpenRule(row),
  },
  {
    label: (row) => row.isActive === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'cfg:doc_type:toggle',
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

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: false,
  export: false,
}

// ==================== 表单配置（新增/编辑弹窗） ====================
const formColumns = [
  { prop: 'typeCode', label: '类型编码', type: 'input', required: true, maxlength: 50 },
  { prop: 'typeName', label: '类型名称', type: 'input', required: true, maxlength: 100 },
  {
    prop: 'typeCategory', label: '类型分类', type: 'select', required: true,
    options: [
      { label: '入库', value: 'INBOUND' },
      { label: '出库', value: 'OUTBOUND' },
      { label: '调拨', value: 'TRANSFER' },
      { label: '盘点', value: 'INVENTORY' },
    ],
  },
  { prop: 'isActive', label: '是否启用', type: 'switch', defaultValue: 1, activeValue: 1, inactiveValue: 0 },
  { prop: 'sortOrder', label: '排序', type: 'number', min: 0, max: 9999 },
  { prop: 'description', label: '描述', type: 'textarea', span: 24, maxlength: 500 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 500 },
]

const formDialogData = reactive({
  typeCode: '',
  typeName: '',
  typeCategory: '',
  isActive: 1,
  sortOrder: 0,
  description: '',
  remark: '',
})

// ==================== 流程配置弹窗 ====================
const processDialogVisible = ref(false)
const processSubmitLoading = ref(false)
const processFormRef = ref(null)
const currentDocTypeId = ref(null)

const processFormColumns = [
  { prop: 'initialStatus', label: '初始状态', type: 'input', maxlength: 30, placeholder: '如 DRAFT' },
  { prop: 'requireApproval', label: '需要审批', type: 'switch', defaultValue: 0, activeValue: 1, inactiveValue: 0 },
  { prop: 'requireReceiving', label: '需要收货确认', type: 'switch', defaultValue: 1, activeValue: 1, inactiveValue: 0 },
  { prop: 'autoPutaway', label: '自动上架', type: 'switch', defaultValue: 1, activeValue: 1, inactiveValue: 0 },
  { prop: 'autoPick', label: '自动分配拣货', type: 'switch', defaultValue: 1, activeValue: 1, inactiveValue: 0 },
  { prop: 'autoAllocate', label: '自动分配库位', type: 'switch', defaultValue: 0, activeValue: 1, inactiveValue: 0 },
  { prop: 'autoPrintLabel', label: '自动打印标签', type: 'switch', defaultValue: 0, activeValue: 1, inactiveValue: 0 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 500 },
]

const createProcessFormData = () => ({
  id: null,
  docTypeId: null,
  initialStatus: '',
  requireApproval: 0,
  requireReceiving: 1,
  autoPutaway: 1,
  autoPick: 1,
  autoAllocate: 0,
  autoPrintLabel: 0,
  remark: '',
})

const processFormData = reactive(createProcessFormData())

const resetProcessForm = () => Object.assign(processFormData, createProcessFormData())

async function handleOpenProcess(row) {
  currentDocTypeId.value = row.id
  resetProcessForm()
  processFormData.docTypeId = row.id
  try {
    const res = await processApi.pageList({ docTypeId: row.id, pageSize: 1 })
    const list = res?.data?.rows || res?.rows || []
    if (list.length > 0) {
      const data = list[0]
      Object.assign(processFormData, {
        id: data.id,
        docTypeId: data.docTypeId,
        initialStatus: data.initialStatus || '',
        requireApproval: data.requireApproval ?? 0,
        requireReceiving: data.requireReceiving ?? 1,
        autoPutaway: data.autoPutaway ?? 1,
        autoPick: data.autoPick ?? 1,
        autoAllocate: data.autoAllocate ?? 0,
        autoPrintLabel: data.autoPrintLabel ?? 0,
        remark: data.remark || '',
      })
    }
  } catch {
    // 没有配置则保持默认值
  }
  processDialogVisible.value = true
}

async function handleProcessSubmit() {
  const submitData = { ...processFormRef.value.formData }
  processSubmitLoading.value = true
  try {
    if (submitData.id) {
      await processApi.update(submitData)
      KhMessageFn.success('流程配置已更新')
    } else {
      delete submitData.id
      await processApi.create(submitData)
      KhMessageFn.success('流程配置已创建')
    }
    processDialogVisible.value = false
  } catch {
    // request.js 已处理错误提示
  } finally {
    processSubmitLoading.value = false
  }
}

// ==================== 规则配置弹窗 ====================
const ruleDialogVisible = ref(false)
const ruleSubmitLoading = ref(false)
const ruleFormRef = ref(null)

const ruleFormColumns = [
  { prop: 'allowModify', label: '允许修改', type: 'switch', defaultValue: 1, activeValue: 1, inactiveValue: 0 },
  { prop: 'allowCancel', label: '允许取消', type: 'switch', defaultValue: 1, activeValue: 1, inactiveValue: 0 },
  { prop: 'allowPartialExecute', label: '允许部分执行', type: 'switch', defaultValue: 0, activeValue: 1, inactiveValue: 0 },
  { prop: 'allowMultipleWarehouse', label: '允许多仓', type: 'switch', defaultValue: 0, activeValue: 1, inactiveValue: 0 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 500 },
]

const createRuleFormData = () => ({
  id: null,
  docTypeId: null,
  allowModify: 1,
  allowCancel: 1,
  allowPartialExecute: 0,
  allowMultipleWarehouse: 0,
  remark: '',
})

const ruleFormData = reactive(createRuleFormData())

const resetRuleForm = () => Object.assign(ruleFormData, createRuleFormData())

async function handleOpenRule(row) {
  currentDocTypeId.value = row.id
  resetRuleForm()
  ruleFormData.docTypeId = row.id
  try {
    const res = await ruleApi.pageList({ docTypeId: row.id, pageSize: 1 })
    const list = res?.data?.rows || res?.rows || []
    if (list.length > 0) {
      const data = list[0]
      Object.assign(ruleFormData, {
        id: data.id,
        docTypeId: data.docTypeId,
        allowModify: data.allowModify ?? 1,
        allowCancel: data.allowCancel ?? 1,
        allowPartialExecute: data.allowPartialExecute ?? 0,
        allowMultipleWarehouse: data.allowMultipleWarehouse ?? 0,
        remark: data.remark || '',
      })
    }
  } catch {
    // 没有配置则保持默认值
  }
  ruleDialogVisible.value = true
}

async function handleRuleSubmit() {
  const submitData = { ...ruleFormRef.value.formData }
  ruleSubmitLoading.value = true
  try {
    if (submitData.id) {
      await ruleApi.update(submitData)
      KhMessageFn.success('规则配置已更新')
    } else {
      delete submitData.id
      await ruleApi.create(submitData)
      KhMessageFn.success('规则配置已创建')
    }
    ruleDialogVisible.value = false
  } catch {
    // request.js 已处理错误提示
  } finally {
    ruleSubmitLoading.value = false
  }
}
</script>
