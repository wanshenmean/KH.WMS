<template>
  <div class="category-list-page">
    <!-- 搜索区域 -->
    <div class="category-list-page__search">
      <KhForm :columns="searchColumns" v-model="searchModel" :inline="true" :col-count="4" label-width="80px"
        @search="handleSearch" @reset="handleReset">
        <template #extra-buttons>
          <el-button type="primary" :icon="Icons.Plus" @click="handleAdd(null)">新增分类</el-button>
        </template>
      </KhForm>
    </div>

    <!-- 树形表格 -->
    <div class="category-list-page__table">
      <KhTable ref="tableRef" title="物料分类" :columns="tableColumns" :data="filteredTreeData" :default-expand-all="true"
        :tree-props="{ children: 'children' }" :row-key="'id'" :show-toolbar="true" :show-pagination="false">
        <template #toolbar-left>
          <el-tag type="info">共 {{ flatCount }} 个分类</el-tag>
          <el-button style="margin-left: 8px" size="small" @click="expandAll">全部展开</el-button>
          <el-button size="small" @click="collapseAll">全部折叠</el-button>
        </template>
        <template #action="{ row }">
          <el-button type="primary" link size="small" @click="handleAdd(row)">新增子级</el-button>
          <el-divider direction="vertical" />
          <el-button type="primary" link size="small" @click="handleEdit(row)">编辑</el-button>
          <el-divider direction="vertical" />
          <el-popconfirm :title="`确定要${row.status === 1 ? '禁用' : '启用'}该分类吗？`" @confirm="handleToggleStatus(row)">
            <template #reference>
              <el-button type="warning" link size="small">{{ row.status === 1 ? '禁用' : '启用' }}</el-button>
            </template>
          </el-popconfirm>
          <el-divider direction="vertical" />
          <el-popconfirm title="确定删除该分类? 子分类也将一并删除。" @confirm="handleDelete(row)">
            <template #reference>
              <el-button type="danger" link size="small">删除</el-button>
            </template>
          </el-popconfirm>
        </template>
      </KhTable>
    </div>

    <CategoryFormDialog v-model="dialogVisible" :mode="formDialogMode" :data="formDialogData"
      @success="handleSuccess" />
  </div>
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'
import KhTable from '@/components/KhTable/index.vue'
import CategoryFormDialog from './components/CategoryFormDialog.vue'
import { getCategoryTree, deleteCategory } from '@/api/basedata'

const Icons = markRaw({ Plus })

const tableRef = ref(null)
const crudApi = useCrudApi('material-category')

const handleToggleStatus = async (row) => {
  const newStatus = row.status === 1 ? 0 : 1
  const res = await crudApi.setStatus(row.id, newStatus)
  if (res.code === 200) {
    KhMessageFn.success(res.message)
    loadTree()
  }
}

// ---- 搜索 ----
const searchColumns = [
  { prop: 'categoryCode', label: '分类编码', type: 'input', clearable: true },
  { prop: 'categoryName', label: '分类名称', type: 'input', clearable: true },
]

const searchModel = reactive({
  categoryCode: '',
  categoryName: '',
})

// ---- 树形数据 ----
const treeData = ref([])

const loadTree = async () => {
  try {
    const res = await getCategoryTree()
    treeData.value = res.code == 200 ? res.data : []
  } catch {
    treeData.value = []
  }
}

onMounted(() => {
  loadTree()
})

// ---- 过滤 ----
const flatCount = computed(() => {
  const count = (nodes) => {
    let c = 0
    nodes.forEach(n => { c += 1; if (n.children) c += count(n.children) })
    return c
  }
  return count(filteredTreeData.value)
})

const filterTree = (nodes, model) => {
  return nodes.map(node => {
    const children = node.children ? filterTree(node.children, model) : []
    const matchCode = !model.categoryCode || node.categoryCode.toLowerCase().includes(model.categoryCode.toLowerCase())
    const matchName = !model.categoryName || node.categoryName.includes(model.categoryName)
    const childMatch = children.length > 0
    if (matchCode && matchName) return { ...node, children }
    if (childMatch) return { ...node, children }
    return null
  }).filter(Boolean)
}

const filteredTreeData = computed(() => {
  return filterTree(treeData.value, searchModel)
})

const handleSearch = () => {
  loadTree()
}
const handleReset = () => {
  Object.assign(searchModel, { categoryCode: '', categoryName: '' })
  loadTree()
}

// ---- 展开/折叠 ----
const expandAll = () => {
  toggleExpand(treeData.value, true)
}

const collapseAll = () => {
  toggleExpand(treeData.value, false)
}

const toggleExpand = (data, expand) => {
  data.forEach(item => {
    const elTable = tableRef.value?.tableRef
    if (elTable) {
      elTable.toggleRowExpansion(item, expand)
    }
    if (item.children) toggleExpand(item.children, expand)
  })
}

// ---- 表格列 ----
const tableColumns = [
  { prop: 'categoryCode', label: '分类编码', width: 150 },
  { prop: 'categoryName', label: '分类名称', minWidth: 160 },
  { prop: 'level', label: '层级', width: 80, align: 'center' },
  { prop: 'sortNo', label: '排序', width: 80, align: 'center' },
  {
    prop: 'status', label: '状态', width: 90, align: 'center',
    type: 'tag', tagMap: 'dict:status_flag',
  },
]

// ---- 弹窗 ----
const dialogVisible = ref(false)
const formDialogMode = ref('create')
const formDialogData = ref({})

const handleAdd = (row) => {
  formDialogMode.value = 'create'
  formDialogData.value = {
    status: 1,
    parentId: row ? row.id : null,
  }
  dialogVisible.value = true
}

const handleEdit = (row) => {
  formDialogMode.value = 'edit'
  formDialogData.value = { ...row }
  dialogVisible.value = true
}

const handleDelete = async (row) => {
  try {
    await deleteCategory(row.id)
    KhMessageFn.success('删除成功')
    loadTree()
  } catch {
    // 错误由拦截器处理
  }
}

const handleSuccess = () => {
  loadTree()
}
</script>

<style scoped>
.category-list-page {
  display: flex;
  flex-direction: column;
  gap: 12px;
  height: 100%;
  overflow: hidden;
}

.category-list-page__search {
  flex-shrink: 0;
  padding: 16px 20px 0;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}

.category-list-page__table {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
  overflow: hidden;
}

.category-list-page__table :deep(.kh-table) {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
}

.category-list-page__table :deep(.kh-table .el-table) {
  flex: 1;
  min-height: 0;
}

.category-list-page__table :deep(.kh-table .el-table__body-wrapper) {
  overflow-y: auto;
}
</style>
