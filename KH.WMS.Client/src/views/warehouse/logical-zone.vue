<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage ref="pageRef" module="logical-zone" title="逻辑分区管理" :search-columns="searchColumns"
      :search-model="searchModel" :columns="tableColumns" :show-stat-cards="false" :show-toolbar="true"
      :show-index="true" :show-selection="true" :show-header-filter="true" :search-col-count="3"
      :crud-operations="{ create: false, update: false, delete: true, view: true, export: true }"
      :permission-prefix="'wh:logical_zone'" :action-width="'120'" :action-buttons="actionButtons"
      :detail-lines="detailLineConfigs" :detail-width="'900px'" :toolbar-buttons="extraToolbarButtons">
    </KhPage>

    <!-- 新增/编辑弹窗 -->
    <KhDialog v-model="dialogVisible" :title="dialogMode === 'create' ? '新增逻辑分区' : '编辑逻辑分区'" width="900px"
      destroy-on-close :confirm-loading="submitLoading" @confirm="handleSubmit" @close="resetForm">
      <template #default>
        <KhForm ref="formRef" :columns="formColumns" v-model="formData" :label-width="'90px'" :col-count="3" />

        <el-divider content-position="left">
          <span style="font-size: 13px; color: #606266;">物理库区映射</span>
        </el-divider>

        <KhEditableTable v-model="mappings" :columns="mappingColumns" :default-row="createEmptyMapping"
          :max-height="300" add-text="添加映射" :action-width="70" />
      </template>
    </KhDialog>
  </div>
</template>

<script setup>
import { KhEditableTable } from '@/components'
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('logical-zone')

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'zoneCode', label: '分区编码', type: 'input', clearable: true },
  { prop: 'zoneName', label: '分区名称', type: 'input', clearable: true },
  {
    prop: 'zoneType', label: '分区类型', type: 'select', clearable: true,
    options: [
      { label: '拣选区', value: 'PICKING' },
      { label: '存储区', value: 'STORAGE' },
      { label: '暂存区', value: 'STAGING' },
      { label: '质检区', value: 'QC' },
    ],
  },
]

const searchModel = reactive({
  zoneCode: '',
  zoneName: '',
  zoneType: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'id', label: '主键ID', visible: false },
  { prop: 'zoneCode', label: '分区编码', width: 140 },
  { prop: 'zoneName', label: '分区名称', width: 150 },
  {
    prop: 'zoneType', label: '分区类型', width: 110, align: 'center',
    type: 'tag',
    tagMap: { PICKING: '拣选区', STORAGE: '存储区', STAGING: '暂存区', QC: '质检区' },
    tagTypeMap: { PICKING: 'warning', STORAGE: 'primary', STAGING: 'info', QC: 'danger' },
  },
  { prop: 'warehouseId', label: '所属仓库', width: 130, type: 'tag', tagMap: 'dict:warehouse_list' },
  { prop: 'sortNo', label: '排序号', width: 90, align: 'right' },
  {
    prop: 'status', label: '状态', width: 90, align: 'center',
    type: 'tag', tagMap: 'dict:status_flag',
  },
  { prop: 'remark', label: '备注', minWidth: 160, showOverflowTooltip: true },
]

const extraToolbarButtons = [
  // {
  //   label: '新增',
  //   icon: Plus,
  //   type: 'primary',
  //   permission: 'wh:logical_zone:add',
  //   onClick: () => handleCreate(),
  // },
]

// ==================== 操作按钮 ====================
const actionButtons = [
  // {
  //   label: '编辑',
  //   permission: 'wh:logical_zone:edit',
  //   onClick: (row) => handleUpdate(row),
  // },
  {
    label: (row) => row.status === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'wh:logical_zone:toggle',
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

// ==================== 表单配置 ====================
const formColumns = [
  { prop: 'zoneCode', label: '分区编码', type: 'input', required: true, maxlength: 30 },
  { prop: 'zoneName', label: '分区名称', type: 'input', required: true, maxlength: 100 },
  {
    prop: 'zoneType', label: '分区类型', type: 'select', required: true,
    options: [
      { label: '拣选区', value: 'PICKING' },
      { label: '存储区', value: 'STORAGE' },
      { label: '暂存区', value: 'STAGING' },
      { label: '质检区', value: 'QC' },
    ],
  },
  { prop: 'warehouseId', label: '所属仓库', type: 'select', required: true, options: 'dict:warehouse_list' },
  { prop: 'sortNo', label: '排序号', type: 'number', required: true },
  { prop: 'status', label: '状态', type: 'select', required: true, options: 'dict:status_flag' },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 500 },
]

// ==================== 映射子表列配置 ====================
const mappingColumns = [
  {
    prop: 'physicalZoneId', label: '物理库区', minWidth: 200, type: 'select',
    options: 'dict:zone_list', placeholder: '请选择物理库区', filterable: true,
  },
  { prop: 'priority', label: '优先级', width: 120, type: 'number', min: 0, step: 1, controls: false, placeholder: '0' },
  { prop: 'status', label: '状态', width: 80, type: 'switch' },
]

const createEmptyMapping = () => ({
  physicalZoneId: null,
  priority: 0,
  status: 1,
})

// ==================== 详情弹窗子表配置 ====================
const detailLineConfigs = [
  {
    prop: 'mappings',
    title: '物理库区映射',
    columns: [
      { prop: 'physicalZoneId', label: '物理库区ID', width: 120, align: 'center' },
      { prop: 'priority', label: '优先级', width: 80, align: 'center' },
      { prop: 'status', label: '状态', width: 80, align: 'center' },
    ],
  },
]

// ==================== 弹窗逻辑 ====================
const dialogVisible = ref(false)
const dialogMode = ref('create')
const submitLoading = ref(false)
const formRef = ref(null)
const mappings = ref([])

const createFormData = () => ({
  id: null,
  zoneCode: '',
  zoneName: '',
  zoneType: '',
  warehouseId: null,
  sortNo: 0,
  status: 1,
  remark: '',
})

const formData = reactive(createFormData())

const resetForm = () => {
  Object.assign(formData, createFormData())
  mappings.value = []
}

const handleCreate = () => {
  dialogMode.value = 'create'
  resetForm()
  dialogVisible.value = true
}

const handleUpdate = async (row) => {
  dialogMode.value = 'update'
  resetForm()
  try {
    const res = await crudApi.detail(row.id)
    const detail = res.data || res
    Object.assign(formData, {
      id: detail.id,
      zoneCode: detail.zoneCode || '',
      zoneName: detail.zoneName || '',
      zoneType: detail.zoneType || '',
      warehouseId: detail.warehouseId || null,
      sortNo: detail.sortNo ?? 0,
      status: detail.status ?? 1,
      remark: detail.remark || '',
    })
    mappings.value = (detail.mappings || []).map(m => ({
      physicalZoneId: m.physicalZoneId || null,
      priority: m.priority ?? 0,
      status: m.status ?? 1,
    }))
    dialogVisible.value = true
  } catch {
    Object.assign(formData, {
      id: row.id,
      zoneCode: row.zoneCode || '',
      zoneName: row.zoneName || '',
      zoneType: row.zoneType || '',
      warehouseId: row.warehouseId || null,
      sortNo: row.sortNo ?? 0,
      status: row.status ?? 1,
      remark: row.remark || '',
    })
    dialogVisible.value = true
  }
}

const handleSubmit = async () => {
  const submitData = { ...formData }
  submitData.mappings = mappings.value.map(m => ({
    physicalZoneId: m.physicalZoneId,
    priority: m.priority ?? 0,
    status: m.status ?? 1,
  }))

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
