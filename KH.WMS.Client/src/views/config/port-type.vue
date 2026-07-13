<template>
    <div style="height: 100%; display: flex; flex-direction: column;">
        <KhPage ref="pageRef" module="port-type" title="站台类型配置" :search-columns="searchColumns"
            :search-model="searchModel" :columns="tableColumns" :show-stat-cards="false" :show-toolbar="true"
            :show-index="true" :show-selection="true" :show-header-filter="true" :search-col-count="3"
            :crud-operations="crudOperations" :permission-prefix="'cfg:port_type'" :action-buttons="extraActionButtons"
            :toolbar-buttons="extraToolbarButtons" :form-columns="formColumns" :custom-form-data="formDialogData"
            :action-width="'120'" />

    </div>
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'

const pageRef = ref(null)
const crudApi = useCrudApi('port-type')

// 添加处理函数
const handleDialogSuccess = () => {
    pageRef.value?.reload()
}

// ==================== 搜索 ====================
const searchColumns = [
    { prop: 'typeCode', label: '类型编码', type: 'input', clearable: true },
    { prop: 'typeName', label: '类型名称', type: 'input', clearable: true },
]

const searchModel = reactive({
    typeCode: '',
    typeName: '',
})

// ==================== 表格列 ====================
const tableColumns = [
    { prop: 'id', label: '主键ID', visible: false },
    { prop: 'typeCode', label: '类型编码', width: 130 },
    { prop: 'typeName', label: '类型名称', width: 120 },
    { prop: 'allowInbound', label: '是否允许入库', minWidth: 140, },
    { prop: 'allowOutbound', label: '是否允许出库', minWidth: 140, },
    { prop: 'allowPicking', label: '是否允许拣货', minWidth: 140, },
    { prop: 'color', label: '颜色标识', minWidth: 140, type: 'tag', tagMap: 'dict:color_flag', },
    { prop: 'description', label: '描述', minWidth: 140, },
    { prop: 'sortNo', label: '排序号', minWidth: 140, },
    { prop: 'status', label: '状态', minWidth: 180, type: 'tag', tagMap: 'dict:status_flag' },
    { prop: 'createdBy', label: '创建人', width: 140 },
    { prop: 'createdByName', label: '创建人名称', width: 170 },
    { prop: 'createdTime', label: '创建时间', minWidth: 180 },
    { prop: 'lastModifiedBy', label: '最后修改人', minWidth: 180 },
    { prop: 'lastModifiedByName', label: '最后修改人名称', minWidth: 180 },
    { prop: 'lastModifiedTime', label: '最后修改时间', minWidth: 180 },
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
    { prop: 'typeCode', label: '类型编码', type: 'input', required: true },
    { prop: 'typeName', label: '类型名称', type: 'input', required: true },
    { prop: 'allowInbound', label: '是否允许入库', type: 'switch', required: true, labelWidth: 120, activeValue: 1, inactiveValue: 0 },
    { prop: 'allowOutbound', label: '是否允许出库', type: 'switch', required: true, labelWidth: 120, activeValue: 1, inactiveValue: 0 },
    { prop: 'allowPicking', label: '是否允许拣货', type: 'switch', required: true, labelWidth: 120, activeValue: 1, inactiveValue: 0 },
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
    typeCode: '',
    typeName: '',
    allowInbound: 0,
    allowOutbound: 0,
    allowPicking: 0,
    color: '',
    description: '',
    sortNo: 0,
    status: '1',
})

// ==================== 额外操作按钮 ====================
const extraActionButtons = [
  {
    label: (row) => row.status === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'cfg:port_type:toggle',
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
