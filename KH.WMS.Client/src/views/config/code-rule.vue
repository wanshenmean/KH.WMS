<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage
      ref="pageRef"
      module="code-rule"
      title="编码规则配置"
      :search-columns="searchColumns"
      :search-model="searchModel"
      :columns="tableColumns"
      :show-stat-cards="false"
      :show-toolbar="true"
      :show-index="true"
      :show-header-filter="true"
      :search-col-count="3"
      :crud-operations="crudOperations"
      :permission-prefix="'cfg:code_rule'"
      :action-buttons="actionButtons"
    >
      <template #toolbar-left>
        <el-button type="primary" @click="handleCreate">
          <el-icon><Plus /></el-icon> 新增
        </el-button>
      </template>
    </KhPage>

    <!-- 新增/编辑弹窗 -->
    <KhDialog
      v-model="dialogVisible"
      :title="dialogMode === 'create' ? '新增编码规则' : '编辑编码规则'"
      width="780px"
      destroy-on-close
      :confirm-loading="submitLoading"
      @confirm="handleSubmit"
      @close="resetForm"
    >
      <template #default>
        <KhForm
          ref="formRef"
          :columns="formColumns"
          v-model="formData"
          :label-width="'100px'"
          :col-count="2"
        />
      </template>
    </KhDialog>

    <!-- 预览弹窗 -->
    <KhDialog v-model="previewVisible" title="编码规则 - 预览" width="500px" :show-footer="false">
      <div class="code-rule-preview">
        <div class="code-rule-preview__item">
          <span class="code-rule-preview__label">规则编码：</span>
          <span>{{ previewData.ruleCode }}</span>
        </div>
        <div class="code-rule-preview__item">
          <span class="code-rule-preview__label">规则名称：</span>
          <span>{{ previewData.ruleName }}</span>
        </div>
        <div class="code-rule-preview__item">
          <span class="code-rule-preview__label">规则类型：</span>
          <el-tag>{{ ruleTypeTagMap[previewData.ruleType] || previewData.ruleType }}</el-tag>
        </div>
        <div class="code-rule-preview__item">
          <span class="code-rule-preview__label">前缀：</span>
          <span>{{ previewData.prefix || '-' }}</span>
        </div>
        <div class="code-rule-preview__item">
          <span class="code-rule-preview__label">日期格式：</span>
          <span>{{ previewData.dateFormat || '-' }}</span>
        </div>
        <div class="code-rule-preview__item">
          <span class="code-rule-preview__label">序列号长度：</span>
          <span>{{ previewData.sequenceLength }}</span>
        </div>
        <div class="code-rule-preview__item">
          <span class="code-rule-preview__label">序列类型：</span>
          <span>{{ sequenceTypeTagMap[previewData.sequenceType] || previewData.sequenceType }}</span>
        </div>
        <div class="code-rule-preview__item">
          <span class="code-rule-preview__label">分隔符：</span>
          <span>{{ previewData.separator || '-' }}</span>
        </div>
        <div class="code-rule-preview__item">
          <span class="code-rule-preview__label">编码示例：</span>
          <el-tag type="primary">{{ previewData.example || '-' }}</el-tag>
        </div>
      </div>
    </KhDialog>
  </div>
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('code-rule')

// ==================== 标签映射 ====================
const ruleTypeTagMap = {
  MATERIAL: '物料编码', CONTAINER: '容器编码',
  INBOUND_DOC: '入库单号', OUTBOUND_DOC: '出库单号',
  STOCKTAKE_DOC: '盘点单号', ADJUST_DOC: '调整单号', OTHER: '其他',
}

const sequenceTypeTagMap = {
  DAILY: '每日重置', MONTHLY: '每月重置', YEARLY: '每年重置', NONE: '不重置',
}

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'ruleCode', label: '规则编码', type: 'input', clearable: true },
  { prop: 'ruleName', label: '规则名称', type: 'input', clearable: true },
  {
    prop: 'isActive', label: '状态', type: 'select', clearable: true,
    options: [
      { label: '启用', value: 1 },
      { label: '禁用', value: 0 },
    ],
  },
]

const searchModel = reactive({ ruleCode: '', ruleName: '', isActive: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'ruleCode', label: '规则编码', width: 140 },
  { prop: 'ruleName', label: '规则名称', width: 160 },
  {
    prop: 'ruleType', label: '规则类型', width: 110, align: 'center',
    type: 'tag', tagMap: ruleTypeTagMap,
    tagTypeMap: { MATERIAL: '', CONTAINER: 'success', INBOUND_DOC: 'warning', OUTBOUND_DOC: 'danger', STOCKTAKE_DOC: 'info', ADJUST_DOC: '', OTHER: 'info' },
  },
  { prop: 'prefix', label: '前缀', width: 80 },
  { prop: 'dateFormat', label: '日期格式', width: 100 },
  { prop: 'sequenceLength', label: '序列长度', width: 80, align: 'center' },
  { prop: 'example', label: '编码示例', width: 200, showOverflowTooltip: true },
  {
    prop: 'isDefault', label: '默认', width: 70, align: 'center',
    type: 'tag', tagMap: { 0: '否', 1: '是' }, tagTypeMap: { 1: 'success', 0: 'info' },
  },
  {
    prop: 'isActive', label: '状态', width: 70, align: 'center',
    type: 'tag', tagMap: { 0: '禁用', 1: '启用' }, tagTypeMap: { 1: 'success', 0: 'danger' },
  },
]

// ==================== 操作按钮 ====================
const actionButtons = [
  {
    label: '编辑',
    permission: 'cfg:code_rule:edit',
    onClick: (row) => handleUpdate(row),
  },
  {
    label: '预览',
    type: 'primary',
    permission: 'cfg:code_rule:view',
    onClick: (row) => handlePreview(row),
  },
  {
    label: (row) => row.isActive === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'cfg:code_rule:toggle',
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
  create: false,
  update: false,
  delete: true,
  view: false,
  export: false,
}

// ==================== 表单配置 ====================
const formColumns = [
  { prop: 'ruleCode', label: '规则编码', type: 'input', required: true, maxlength: 50 },
  { prop: 'ruleName', label: '规则名称', type: 'input', required: true, maxlength: 100 },
  {
    prop: 'ruleType', label: '规则类型', type: 'select', required: true,
    options: [
      { label: '物料编码', value: 'MATERIAL' },
      { label: '容器编码', value: 'CONTAINER' },
      { label: '入库单号', value: 'INBOUND_DOC' },
      { label: '出库单号', value: 'OUTBOUND_DOC' },
      { label: '盘点单号', value: 'STOCKTAKE_DOC' },
      { label: '调整单号', value: 'ADJUST_DOC' },
      { label: '其他', value: 'OTHER' },
    ],
  },
  { prop: 'prefix', label: '前缀', type: 'input', maxlength: 20, placeholder: '如 RK、CK' },
  {
    prop: 'dateFormat', label: '日期格式', type: 'select', clearable: true,
    options: [
      { label: 'yyyyMMdd', value: 'yyyyMMdd' },
      { label: 'yyMMdd', value: 'yyMMdd' },
      { label: 'yyyyMM', value: 'yyyyMM' },
      { label: 'yyyy', value: 'yyyy' },
      { label: '无', value: '' },
    ],
  },
  { prop: 'sequenceLength', label: '序列号长度', type: 'number', required: true, min: 1, max: 10, defaultValue: 4 },
  {
    prop: 'sequenceType', label: '序列类型', type: 'select', defaultValue: 'DAILY',
    options: [
      { label: '每日重置', value: 'DAILY' },
      { label: '每月重置', value: 'MONTHLY' },
      { label: '每年重置', value: 'YEARLY' },
      { label: '不重置', value: 'NONE' },
    ],
  },
  { prop: 'separator', label: '分隔符', type: 'input', maxlength: 10, placeholder: '如 -、/' },
  { prop: 'example', label: '编码示例', type: 'input', maxlength: 50, placeholder: '如 RK2026042700001' },
  { prop: 'cacheSize', label: '缓存大小', type: 'number', min: 1, max: 1000, defaultValue: 10 },
  { prop: 'isDefault', label: '是否默认', type: 'switch', defaultValue: 0, activeValue: 1, inactiveValue: 0 },
  { prop: 'isActive', label: '是否启用', type: 'switch', defaultValue: 1, activeValue: 1, inactiveValue: 0 },
  { prop: 'effectiveDate', label: '生效日期', type: 'date', clearable: true },
  { prop: 'expiryDate', label: '失效日期', type: 'date', clearable: true },
  { prop: 'description', label: '描述', type: 'textarea', span: 24, maxlength: 500 },
]

// ==================== 弹窗逻辑 ====================
const dialogVisible = ref(false)
const dialogMode = ref('create')
const submitLoading = ref(false)
const formRef = ref(null)

const createFormData = () => ({
  id: null,
  ruleCode: '',
  ruleName: '',
  ruleType: '',
  prefix: '',
  dateFormat: '',
  sequenceLength: 4,
  sequenceType: 'DAILY',
  separator: '',
  example: '',
  cacheSize: 10,
  isDefault: 0,
  isActive: 1,
  effectiveDate: '',
  expiryDate: '',
  description: '',
})

const formData = reactive(createFormData())

const resetForm = () => {
  Object.assign(formData, createFormData())
}

const handleCreate = () => {
  dialogMode.value = 'create'
  resetForm()
  dialogVisible.value = true
}

const handleUpdate = async (row) => {
  dialogMode.value = 'update'
  try {
    const res = await crudApi.detail(row.id)
    const data = res.data || row
    Object.assign(formData, {
      id: data.id,
      ruleCode: data.ruleCode || '',
      ruleName: data.ruleName || '',
      ruleType: data.ruleType || '',
      prefix: data.prefix || '',
      dateFormat: data.dateFormat || '',
      sequenceLength: data.sequenceLength ?? 4,
      sequenceType: data.sequenceType || 'DAILY',
      separator: data.separator || '',
      example: data.example || '',
      cacheSize: data.cacheSize ?? 10,
      isDefault: data.isDefault ?? 0,
      isActive: data.isActive ?? 1,
      effectiveDate: data.effectiveDate || '',
      expiryDate: data.expiryDate || '',
      description: data.description || '',
    })
    dialogVisible.value = true
  } catch {
    Object.assign(formData, {
      id: row.id,
      ruleCode: row.ruleCode || '',
      ruleName: row.ruleName || '',
      ruleType: row.ruleType || '',
      prefix: row.prefix || '',
      dateFormat: row.dateFormat || '',
      sequenceLength: row.sequenceLength ?? 4,
      sequenceType: row.sequenceType || 'DAILY',
      separator: row.separator || '',
      example: row.example || '',
      cacheSize: row.cacheSize ?? 10,
      isDefault: row.isDefault ?? 0,
      isActive: row.isActive ?? 1,
      effectiveDate: row.effectiveDate || '',
      expiryDate: row.expiryDate || '',
      description: row.description || '',
    })
    dialogVisible.value = true
  }
}

const handleSubmit = async () => {
  const submitData = { ...formRef.value.formData }

  submitLoading.value = true
  try {
    if (dialogMode.value === 'create') {
      delete submitData.id
      await crudApi.create(submitData)
      KhMessageFn.success('新增成功')
    } else {
      await crudApi.update(submitData)
      KhMessageFn.success('修改成功')
    }
    dialogVisible.value = false
    pageRef.value?.reload()
  } catch {
    // request.js 已处理错误提示
  } finally {
    submitLoading.value = false
  }
}

// ==================== 预览弹窗 ====================
const previewVisible = ref(false)
const previewData = ref({})

const handlePreview = (row) => {
  previewData.value = { ...row }
  previewVisible.value = true
}
</script>

<style scoped>
.code-rule-preview {
  padding: 8px 0;
}

.code-rule-preview__item {
  display: flex;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid #f0f0f0;
}

.code-rule-preview__item:last-child {
  border-bottom: none;
}

.code-rule-preview__label {
  width: 100px;
  flex-shrink: 0;
  color: #909399;
  font-size: 14px;
}
</style>
