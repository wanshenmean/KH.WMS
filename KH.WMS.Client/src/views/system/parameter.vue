<template>
  <KhPage
    ref="pageRef"
    module="parameter"
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
    :permission-prefix="'sys:param'"
    :before-delete="beforeDelete"
  />
</template>

<script setup>
const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'paramName', label: '参数名称', type: 'input', clearable: true },
  { prop: 'paramCode', label: '参数键名', type: 'input', clearable: true },
  {
    prop: 'systemFlag', label: '系统内置', type: 'select', clearable: true,
    options: [{ label: '是', value: 1 }, { label: '否', value: 0 }],
  },
]

const searchModel = reactive({ paramName: '', paramCode: '', systemFlag: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'paramName', label: '参数名称', minWidth: 180 },
  { prop: 'paramCode', label: '参数键名', width: 200 },
  { prop: 'paramValue', label: '参数键值', width: 150, showOverflowTooltip: true },
  {
    prop: 'systemFlag', label: '系统内置', width: 90, align: 'center',
    type: 'tag', tagMap: { 1: '是', 0: '否' }, tagTypeMap: { 1: 'warning', 0: 'info' },
  },
  {
    prop: 'status', label: '状态', width: 80, align: 'center',
    type: 'tag', tagMap: 'dict:status_flag',
  },
  { prop: 'remark', label: '备注', minWidth: 200, showOverflowTooltip: true },
  { prop: 'createTime', label: '创建时间', width: 170 },
]

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: false,
  update: true,
  delete: true,
  view: false,
  export: false,
}

/** 删除前校验：系统内置参数不允许删除 */
const beforeDelete = (row) => {
  if (row.systemFlag === 1) {
    KhMessageFn.warning('系统内置参数不允许删除')
    return false
  }
}

// ==================== 表单配置（编辑弹窗：只允许修改参数键值） ====================
const formColumns = [
  { prop: 'paramName', label: '参数名称', type: 'input', disabled: true },
  { prop: 'paramCode', label: '参数键名', type: 'input', disabled: true },
  { prop: 'paramValue', label: '参数键值', type: 'input', required: true, maxlength: 500 },
  { prop: 'systemFlag', label: '系统内置', type: 'radio', disabled: true, options: [{ label: '是', value: 1 }, { label: '否', value: 0 }] },
  { prop: 'status', label: '状态', type: 'radio', disabled: true, options: 'dict:status_flag' },
  { prop: 'remark', label: '备注', type: 'textarea', disabled: true, maxlength: 500 },
]
</script>
