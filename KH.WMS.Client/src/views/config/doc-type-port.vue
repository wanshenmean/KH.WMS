<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage ref="pageRef" module="document-type-port" title="单据类型站台映射" :search-columns="searchColumns"
      :search-model="searchModel" :columns="tableColumns" :show-stat-cards="false" :show-toolbar="true"
      :show-index="true" :show-header-filter="true" :search-col-count="3" :crud-operations="crudOperations"
      :permission-prefix="'cfg:doc_type_port'" :form-columns="formColumns" :custom-form-data="formData"
      :action-width="'120'" :action-buttons="extraActionButtons">
      <!-- <template #toolbar-left>
        <el-button type="primary" @click="handleCreate">
          <el-icon>
            <Plus />
          </el-icon> 新增
        </el-button>
      </template> -->
    </KhPage>

    <!-- 新增/编辑弹窗 -->
    <!-- <KhDialog v-model="dialogVisible" :title="dialogMode === 'create' ? '新增站台映射' : '编辑站台映射'" width="680px"
      destroy-on-close :confirm-loading="submitLoading" @confirm="handleSubmit" @close="resetForm">
      <template #default>
        <KhForm ref="formRef" :columns="formColumns" v-model="formData" :label-width="'100px'" :col-count="2"
          @change="handleFormChange" />
      </template>
    </KhDialog> -->
  </div>
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('document-type-port')
const docTypeCrudApi = useCrudApi('document-type')

// ==================== 搜索 ====================
const searchColumns = [
  {
    prop: 'direction', label: '方向', type: 'select', clearable: true,
    options: [
      { label: '入库', value: 'INBOUND' },
      { label: '出库', value: 'OUTBOUND' },
    ],
  },
  { prop: 'portId', label: '站台ID', type: 'input', clearable: true },
  { prop: 'zoneId', label: '库区ID', type: 'input', clearable: true },
]

const searchModel = reactive({ direction: '', portId: '', zoneId: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'docTypeId', label: '单据类型ID', width: 120 },
  {
    prop: 'direction', label: '方向', width: 100, align: 'center',
    type: 'tag',
    tagMap: { INBOUND: '入库', OUTBOUND: '出库' },
    tagTypeMap: { INBOUND: 'success', OUTBOUND: 'warning' },
  },
  { prop: 'portId', label: '站台ID', width: 100 },
  { prop: 'zoneId', label: '库区ID', width: 100 },
  { prop: 'priority', label: '优先级', width: 80, align: 'center' },
  {
    prop: 'isActive', label: '状态', width: 80, align: 'center',
    type: 'tag',
    tagMap: { 0: '禁用', 1: '启用' },
    tagTypeMap: { 1: 'success', 0: 'danger' },
  },
  { prop: 'remark', label: '备注', minWidth: 140, showOverflowTooltip: true },
]

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: true,
  export: true,
}

// ==================== 表单配置 ====================
const formColumns = [
  {
    prop: 'docTypeId', label: '单据类型', type: 'select', required: true,
    options: 'dict:order_type_id', // 使用字典数据源
  },
  {
    prop: 'direction', label: '方向', type: 'select', required: true,
    options: [
      { label: '入库', value: 'INBOUND' },
      { label: '出库', value: 'OUTBOUND' },
    ],
  },
  {
    prop: 'portId', label: '站台ID', type: 'select', clearable: true,
    placeholder: '与库区ID二选一', options: 'dict:port',
  },
  {
    prop: 'zoneId', label: '库区ID', type: 'select', clearable: true,
    placeholder: '与站台ID二选一', options: 'dict:zone_list',
  },
  { prop: 'priority', label: '优先级', type: 'number', min: 1, max: 9999, defaultValue: 100 },
  { prop: 'isActive', label: '是否启用', type: 'switch', defaultValue: 1, activeValue: 1, inactiveValue: 0 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]

// ==================== 弹窗逻辑 ====================
const dialogVisible = ref(false)
const dialogMode = ref('create')
const submitLoading = ref(false)
const formRef = ref(null)

// ==================== 额外操作按钮 ====================
const extraActionButtons = [
  {
    label: (row) => row.isActive === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'cfg:doc_type_port:toggle',
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

const createFormData = () => ({
  id: null,
  docTypeId: null,
  direction: '',
  portId: null,
  zoneId: null,
  priority: 100,
  isActive: 1,
  remark: '',
})

const formData = reactive(createFormData())

const resetForm = () => {
  Object.assign(formData, createFormData())
}

function handleFormChange(prop) {
  // 站台ID和库区ID二选一互斥
  if (prop === 'portId') {
    formRef.value.formData.zoneId = null
  } else if (prop === 'zoneId') {
    formRef.value.formData.portId = null
  }
}

const handleCreate = () => {
  dialogMode.value = 'create'
  resetForm()
  dialogVisible.value = true
}

const handleSubmit = async () => {
  const submitData = { ...formRef.value.formData }

  // 校验二选一
  if (!submitData.portId && !submitData.zoneId) {
    KhMessageFn.warning('站台ID和库区ID至少填写一个')
    return
  }

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
</script>
