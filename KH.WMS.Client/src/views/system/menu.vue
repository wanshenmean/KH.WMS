<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage ref="pageRef" title="菜单管理" :search-columns="searchColumns" :search-model="searchModel"
      :columns="tableColumns" :data="filteredTreeData" :show-stat-cards="false" :show-pagination="false"
      :default-collapsed="true" :collapsible="true">
      <template #toolbar-left>
        <el-button type="primary" :icon="Icons.Plus" @click="handleAdd(null)">新增菜单</el-button>
        <el-button :icon="Icons.Sort" @click="handleExpandAll">展开/折叠</el-button>
      </template>
      <template #action="{ row }">
        <el-button type="primary" link size="small" @click="handleEdit(row)">编辑</el-button>
        <el-divider direction="vertical" />
        <el-button v-if="row.menuType === 0" type="success" link size="small" @click="handleAdd(row)">新增子菜单</el-button>
        <el-divider v-if="row.menuType === 0" direction="vertical" />
        <el-popconfirm :title="`确定要${row.status === 1 ? '禁用' : '启用'}该菜单吗？`" @confirm="handleToggleStatus(row)">
          <template #reference>
            <el-button type="warning" link size="small">{{ row.status === 1 ? '禁用' : '启用' }}</el-button>
          </template>
        </el-popconfirm>
        <el-divider direction="vertical" />
        <el-popconfirm title="确定删除该菜单?" @confirm="handleDelete(row)">
          <template #reference>
            <el-button type="danger" link size="small">删除</el-button>
          </template>
        </el-popconfirm>
      </template>
    </KhPage>

    <KhDialog v-model="dialogVisible" :title="dialogMode === 'create' ? '菜单管理 - 新增' : '菜单管理 - 编辑'" width="800px"
      :form-columns="formColumns" :form-model="formData" :form-col-count="2" :confirm-loading="confirmLoading"
      destroy-on-close @confirm="handleConfirm">
      <template #_buttonActions>
        <div class="button-manage">
          <div class="button-manage__header">
            <span class="button-manage__title">按钮列表</span>
            <el-button size="small" type="primary" :icon="Icons.Plus" @click="handleAddButton">添加按钮</el-button>
          </div>
          <el-table v-if="buttonList.length > 0" :data="buttonList" size="small" border max-height="200" class="button-manage__table">
            <el-table-column prop="buttonName" label="名称" min-width="90" />
            <el-table-column prop="buttonCode" label="编码" min-width="90" />
            <el-table-column prop="permKey" label="权限标识" min-width="120" show-overflow-tooltip />
            <el-table-column prop="sortNo" label="排序" width="60" align="center" />
            <el-table-column label="操作" width="110" align="center" fixed="right">
              <template #default="{ row, $index }">
                <el-button type="primary" link size="small" @click="handleEditButton(row, $index)">编辑</el-button>
                <el-button type="danger" link size="small" @click="handleDeleteButton($index)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
          <el-empty v-else description="暂无按钮，点击右上角添加" :image-size="40" />
        </div>
      </template>
    </KhDialog>

    <KhDialog v-model="buttonDialogVisible" :title="buttonEditingIndex >= 0 ? '菜单管理 - 编辑按钮' : '菜单管理 - 添加按钮'" width="600px" :form-columns="buttonFormColumns"
      :form-model="buttonFormData" :form-col-count="1" :confirm-loading="buttonConfirmLoading"
      @confirm="handleAddButtonConfirm" />
  </div>
</template>

<script setup>
import KhPage from '@/components/KhPage/index.vue'
import KhDialog from '@/components/KhDialog/index.vue'
import { useUserStore } from '@/stores/user'
import { getMenuTree, saveMenu, deleteMenu } from '@/api/system'
import { useCrudApi } from '@/utils/crud'

const Icons = markRaw({ Plus, Sort })
const pageRef = ref(null)
const userStore = useUserStore()
const crudApi = useCrudApi('permission')

const handleToggleStatus = async (row) => {
  const newStatus = row.status === 1 ? 0 : 1
  const res = await crudApi.setStatus(row.id, newStatus)
  if (res.code === 200) {
    KhMessageFn.success(res.message)
    fetchMenuTree()
  }
}

// ---- 搜索 ----
const searchColumns = [
  { prop: 'permissionName', label: '菜单名称', type: 'input', clearable: true },
  {
    prop: 'status', label: '状态', type: 'select', clearable: true,
    options: 'dict:status_flag',
  },
]

const searchModel = reactive({ permissionName: '', status: '' })

// ---- 树形数据 ----
const treeData = ref([])

const fetchMenuTree = async () => {
  try {
    const res = await getMenuTree(userStore.userInfo.roleId)
    if (res.code === 200) {
      treeData.value = res.data || []
    }
  } catch {
    treeData.value = []
  }
}

onMounted(() => fetchMenuTree())

// ---- 过滤 ----
const filterTree = (nodes, model) => {
  return nodes.map(node => {
    const children = node.children ? filterTree(node.children, model) : []
    const matchName = !model.permissionName || node.permissionName.includes(model.permissionName)
    const matchStatus = model.status === '' || model.status === undefined || node.status === model.status
    const childMatch = children.length > 0
    if (matchName && matchStatus) return { ...node, children }
    if (childMatch) return { ...node, children }
    return null
  }).filter(Boolean)
}

const filteredTreeData = computed(() => filterTree(treeData.value, searchModel))

// ---- 表格列 ----
const tableColumns = [
  { prop: 'permissionName', label: '菜单名称', minWidth: 180 },
  { prop: 'permissionCode', label: '权限标识', width: 150 },
  {
    prop: 'menuType', label: '类型', width: 80, align: 'center',
    type: 'select', tagMap: 'dict:menu_type',
  },
  { prop: 'path', label: '路由路径', width: 180 },
  { prop: 'icon', label: '图标', width: 100, align: 'center' },
  { prop: 'sortNo', label: '排序', width: 70, align: 'center' },
  {
    prop: 'status', label: '状态', width: 80, align: 'center',
    type: 'select', tagMap: 'dict:status_flag',
  },
  { prop: 'remark', label: '备注', minWidth: 140, showOverflowTooltip: true },
]

// ---- 添加/编辑按钮弹窗 ----
const buttonDialogVisible = ref(false)
const buttonConfirmLoading = ref(false)
const buttonFormData = ref({})
// 当前编辑的按钮索引，-1 表示新增
const buttonEditingIndex = ref(-1)
const buttonFormColumns = [
  { prop: 'buttonCode', label: '按钮编码', type: 'input', required: true, maxlength: 50 },
  { prop: 'buttonName', label: '按钮名称', type: 'input', required: true, maxlength: 50 },
  { prop: 'permKey', label: '权限标识', type: 'input', required: true, maxlength: 50 },
  { prop: 'icon', label: '图标', type: 'icon-picker', maxlength: 50 },
  { prop: 'sortNo', label: '排序号', type: 'number', min: 0, defaultValue: 1 },
  { prop: 'status', label: '状态', type: 'select', required: true, options: 'dict:status_flag', defaultValue: '1' },
  { prop: 'remark', label: '备注', type: 'textarea', maxlength: 100 },
]

// ---- 弹窗 ----
const dialogVisible = ref(false)
const dialogMode = ref('create')
const confirmLoading = ref(false)
const formData = ref({})

// 完整按钮对象列表（用于提交后端 MenuButtonDto[]），checkbox 仅做勾选
const buttonList = ref([])
const buttonOptions = computed(() => buttonList.value.map(btn => ({
  label: btn.buttonName,
  value: btn.buttonCode,
})))

const formColumns = computed(() => [
  { prop: 'parentId', label: '上级菜单Id', type: 'input', span: 24, disabled: true },
  { prop: 'parentName', label: '上级菜单', type: 'input', span: 24, disabled: true },
  { prop: 'menuType', label: '菜单类型', type: 'select', required: true, options: 'dict:menu_type' },
  { prop: 'permissionName', label: '菜单名称', type: 'input', required: true, maxlength: 50 },
  { prop: 'permissionCode', label: '权限标识', type: 'input', maxlength: 50 },
  { prop: 'path', label: '路由路径', type: 'input', maxlength: 200 },
  { prop: 'icon', label: '图标', type: 'icon-picker', maxlength: 50 },
  { prop: 'sortNo', label: '排序', type: 'number', min: 0 },
  { prop: 'status', label: '状态', type: 'select', required: true, options: 'dict:status_flag' },
  { prop: 'buttons', label: '按钮权限', type: 'checkbox', span: 24, options: buttonOptions.value, hidden: Number(formData.value.menuType) !== 1 },
  { prop: '_buttonActions', label: ' ', type: 'slot', span: 24, hidden: Number(formData.value.menuType) !== 1 },  // 插槽位
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
])

/** 查找父节点名称 */
const findParentName = (parentId) => {
  const find = (nodes) => {
    for (const node of nodes) {
      if (node.id === parentId) return node.permissionName
      if (node.children) {
        const found = find(node.children)
        if (found) return found
      }
    }
    return ''
  }
  return find(treeData.value)
}

const handleAdd = (row) => {
  buttonList.value = []
  if (row) {
    formData.value = { parentId: row.id, parentName: row.permissionName, menuType: 1, status: 1, buttons: [] }
  } else {
    formData.value = { parentId: 0, parentName: '', menuType: 0, status: 1, buttons: [] }
  }
  dialogMode.value = 'create'
  dialogVisible.value = true
}

const handleEdit = (row) => {
  // 完整按钮对象列表
  buttonList.value = (row.buttons || []).map(btn => ({ ...btn }))
  // formData.buttons 存已选中的 buttonCode 数组（checkbox 绑定）
  const selectedButtons = (row.buttons || []).map(btn => btn.buttonCode)
  formData.value = {
    ...row,
    menuType: Number(row.menuType),
    parentName: findParentName(row.parentId),
    buttons: selectedButtons,
    status: Number(row.status),
  }
  dialogMode.value = 'edit'
  dialogVisible.value = true
}

const handleDelete = async (row) => {
  try {
    await deleteMenu(row.id)
    KhMessageFn.success('删除成功')
    fetchMenuTree()
  } catch {
    // 错误已在拦截器中处理
  }
}

const handleConfirm = async (data) => {
  confirmLoading.value = true
  try {
    // 根据勾选的 buttonCode 过滤出完整按钮对象，提交后端
    const selectedCodes = data.buttons || []
    const submitData = {
      ...data,
      menuType: Number(data.menuType),
      status: Number(data.status),
      buttons: buttonList.value.filter(btn => selectedCodes.includes(btn.buttonCode)),
    }
    await saveMenu(submitData)
    KhMessageFn.success('操作成功')
    dialogVisible.value = false
    fetchMenuTree()
  } catch {
    // 错误已在拦截器中处理
  } finally {
    confirmLoading.value = false
  }
}

const isExpanded = ref(true)

const toggleExpandAll = (data, expand) => {
  data.forEach(item => {
    const elTable = pageRef.value?.tableRef?.tableRef
    if (elTable) {
      elTable.toggleRowExpansion(item, expand)
    }
    if (item.children) toggleExpandAll(item.children, expand)
  })
}

const handleExpandAll = () => {
  isExpanded.value = !isExpanded.value
  toggleExpandAll(filteredTreeData.value, isExpanded.value)
}

const handleAddButton = () => {
  buttonEditingIndex.value = -1
  buttonFormData.value = {}
  buttonDialogVisible.value = true
}

const handleEditButton = (row, index) => {
  buttonEditingIndex.value = index
  buttonFormData.value = { ...row, status: String(row.status) }
  buttonDialogVisible.value = true
}

const handleDeleteButton = (index) => {
  const removed = buttonList.value[index]
  buttonList.value = buttonList.value.filter((_, i) => i !== index)
  // 同步移除 checkbox 选中状态
  const buttons = (formData.value.buttons || []).filter(code => code !== removed.buttonCode)
  formData.value.buttons = buttons
}

const handleAddButtonConfirm = async (data) => {
  buttonConfirmLoading.value = true
  try {
    const btn = {
      buttonCode: data.buttonCode,
      buttonName: data.buttonName,
      permKey: data.permKey,
      icon: data.icon || '',
      sortNo: Number(data.sortNo) || 0,
      status: Number(data.status) || 1,
      remark: data.remark || '',
    }
    if (buttonEditingIndex.value >= 0) {
      // 编辑模式：替换原按钮
      buttonList.value[buttonEditingIndex.value] = btn
      buttonList.value = [...buttonList.value]
    } else {
      // 新增模式
      buttonList.value = [...buttonList.value, btn]
      const buttons = formData.value.buttons || []
      buttons.push(btn.buttonCode)
      formData.value.buttons = buttons
    }
    buttonDialogVisible.value = false
    KhMessageFn.success(buttonEditingIndex.value >= 0 ? '按钮修改成功' : '按钮添加成功')
  } catch {
    // 错误已在拦截器中处理
  } finally {
    buttonConfirmLoading.value = false
  }
}
</script>

<style scoped>
.button-manage {
  width: 100%;
}

.button-manage__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}

.button-manage__title {
  font-size: 14px;
  font-weight: 600;
  color: #303133;
}

.button-manage__table {
  width: 100%;
}
</style>
