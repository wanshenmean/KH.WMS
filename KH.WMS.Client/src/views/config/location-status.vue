<template>
    <div style="height: 100%; display: flex; flex-direction: column;">
        <KhPage ref="pageRef" module="location-status" title="库位状态配置" :search-columns="searchColumns"
            :search-model="searchModel" :columns="tableColumns" :show-stat-cards="false" :show-toolbar="true"
            :show-index="true" :show-selection="true" :show-header-filter="true" :search-col-count="3"
            :crud-operations="crudOperations" :permission-prefix="'cfg:location_status'"
            :action-buttons="extraActionButtons" :toolbar-buttons="extraToolbarButtons" :form-columns="formColumns"
            :custom-form-data="formDialogData" :action-width="'120'" />

    </div>
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('location-status')

// 添加处理函数
const handleDialogSuccess = () => {
    pageRef.value?.reload()
}

// ==================== 搜索 ====================
const searchColumns = [
    { prop: 'statusCode', label: '状态编码', type: 'input', clearable: true },
    { prop: 'statusName', label: '状态名称', type: 'input', clearable: true },
]

const searchModel = reactive({
    statusCode: '',
    statusName: '',
})

// ==================== 表格列 ====================
const tableColumns = [
    { prop: 'id', label: '主键ID', visible: false },
    { prop: 'statusCode', label: '状态编码', width: 130 },
    { prop: 'statusName', label: '状态名称', width: 120 },
    { prop: 'statusCategory', label: '状态分类', minWidth: 120, type: 'tag', tagMap: 'dict:status_category' },
    { prop: 'allowPutaway', label: '是否允许上架', minWidth: 120, type: 'tag', tagMap: 'dict:yes_no' },
    { prop: 'allowPicking', label: '是否允许下架', minWidth: 120, type: 'tag', tagMap: 'dict:yes_no' },
    { prop: 'allowTransfer', label: '是否允许移库', minWidth: 120, type: 'tag', tagMap: 'dict:yes_no' },
    { prop: 'color', label: '颜色标识', minWidth: 90, type: 'tag', tagMap: 'dict:color_flag', },
    { prop: 'description', label: '描述', width: 140 },
    { prop: 'sortNo', label: '排序号', width: 90 },
    { prop: 'status', label: '状态', minWidth: 90, type: 'tag', tagMap: 'dict:status_flag' },
]

// ==================== CRUD 配置 ====================
const crudOperations = {
    create: true,
    update: true,
    delete: true,
    view: true,
    export: true,
}

// ==================== 表单配置（新增/编辑弹窗） ====================
const formColumns = [
    { prop: 'statusCode', label: '状态编码', type: 'input', required: true },
    { prop: 'statusName', label: '状态名称', type: 'input', required: true },
    { prop: 'statusCategory', label: '状态分类', type: 'select', required: true, options: 'dict:status_category' },
    { prop: 'allowPutaway', label: '是否允许上架', type: 'switch', required: true, labelWidth: 120, activeValue: 1, inactiveValue: 0 },
    { prop: 'allowPicking', label: '是否允许下架', type: 'switch', required: true, labelWidth: 120, activeValue: 1, inactiveValue: 0 },
    { prop: 'allowTransfer', label: '是否允许移库', type: 'switch', required: true, labelWidth: 120, activeValue: 1, inactiveValue: 0 },
    {
        prop: 'color', label: '颜色标识', type: 'select', required: true, options: 'dict:color_flag', tagTypeMap: {
            'success': 'success',
            'primary': 'primary',
            'info': 'info',
            'warning': 'warning',
            'danger': 'danger'
        }
    },
    { prop: 'sortNo', label: '排序号', type: 'number', required: true },
    { prop: 'status', label: '状态', type: 'select', required: true, options: 'dict:status_flag' },
    { prop: 'description', label: '描述', type: 'textarea' },
]

const formDialogData = reactive({
    statusCode: '',
    statusName: '',
    statusCategory: '',
    allowPutaway: 0,
    allowPicking: 0,
    allowTransfer: 0,
    color: '',
    description: '',
    sortNo: '',
    status: '1',
})

// ==================== 额外操作按钮 ====================
const extraActionButtons = [
  {
    label: (row) => row.status === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'cfg:location_status:toggle',
    confirm: (row) => `确定要${row.status === 1 ? '禁用' : '启用'}吗？`,
    onClick: async (row) => {
      const currentStatus = row.status
      const newStatus = currentStatus === 1 ? 0 : 1
      const res = await crudApi.setStatus(row.id, newStatus)
      if (res.code === 200) {
        KhMessageFn.success(res.message)
        pageRef.value?.reload()
      }
    },
  },
]

// ==================== 额外工具栏按钮 ====================
const extraToolbarButtons = [

]

</script>
