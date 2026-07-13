<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage ref="pageRef" title="库区管理" module="warehouse-zone" :search-columns="searchColumns"
      :search-model="searchModel" :columns="tableColumns" :show-stat-cards="false" :show-toolbar="true"
      :show-index="false" :show-selection="false" :show-header-filter="false" :show-pagination="false"
      :search-col-count="2" :crud-operations="crudOperations" :permission-prefix="'wh:zone'" :default-collapsed="true"
      :action-buttons="extraActionButtons" :toolbar-buttons="extraToolbarButtons" :form-columns="formColumns"
      :custom-form-data="formDialogData" :collapsible="true">
    </KhPage>

    <ZoneFormDialog v-model="dialogVisible" :mode="formDialogMode" :data="formDialogData" :parent-name="parentName"
      @success="handleDialogSuccess" />
  </div>
</template>

<script setup>
import KhPage from '@/components/KhPage/index.vue'
import ZoneFormDialog from './components/ZoneFormDialog.vue'
import { useCrudApi } from '@/utils/crud'

const Icons = markRaw({ Plus })
const pageRef = ref(null)
const crudApi = useCrudApi('warehouse-zone')

const extraActionButtons = [
  {
    label: (row) => row.status === 1 ? '禁用' : '启用',
    type: 'warning',
    permission: 'wh:zone:toggle',
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

// 添加处理函数
const handleDialogSuccess = () => {
  pageRef.value?.reload()
}

// ---- 搜索 ----
const searchColumns = [
  { prop: 'zoneCode', label: '库区编码' },
  {
    prop: 'zoneType', label: '库区类型', type: 'select',
    options: 'dict:zone_type',
  },
]

const searchModel = reactive({ zoneCode: '', zoneType: '' })

// ---- 树形数据 ----
const treeData = ref([])

// ---- 过滤逻辑 ----
const filterTree = (nodes, model) => {
  return nodes.map(node => {
    const children = node.children ? filterTree(node.children, model) : []
    const matchCode = !model.zoneCode || node.zoneCode.toLowerCase().includes(model.zoneCode.toLowerCase())
    const matchType = !model.zoneType || node.zoneType === model.zoneType
    const childMatch = children.length > 0
    if (matchCode && matchType) return { ...node, children }
    if (childMatch) return { ...node, children }
    return null
  }).filter(Boolean)
}

const filteredTreeData = computed(() => filterTree(treeData.value, searchModel))

// ---- 表格列 ----
const tableColumns = [
  { prop: 'zoneCode', label: '库区编码', width: 130 },
  { prop: 'zoneName', label: '库区名称', minWidth: 180 },
  { prop: 'warehouseId', label: '所属仓库', width: 160, type: 'tag', tagMap: 'dict:warehouse_list' },
  { prop: 'parentZone', label: '所属上级', width: 160 },
  { prop: 'zoneType', label: '库区类型', width: 100, type: 'tag', tagMap: 'dict:zone_type' },
  {
    prop: 'status', label: '状态', width: 90, align: 'center',
    type: 'tag', tagMap: 'dict:status_flag',
  },
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
  { prop: 'zoneCode', label: '库区编码', type: 'input', required: true },
  { prop: 'zoneName', label: '库区名称', type: 'input', required: true },
  { prop: 'warehouseId', label: '所属仓库', type: 'select', required: true, options: 'dict:warehouse_list' },
  { prop: 'parentZone', label: '所属上级', type: 'input' },
  { prop: 'zoneType', label: '库区类型', type: 'select', required: true, options: 'dict:zone_type' },
  { prop: 'sortNo', label: '排序号', type: 'number', required: true },
  { prop: 'status', label: '状态', type: 'select', required: true, options: 'dict:status_flag' },
]

const formDialogData = reactive({
  zoneCode: '',
  zoneName: '',
  parentZone: '',
  zoneType: '',
  sortNo: '',
  status: '1',
})

</script>
