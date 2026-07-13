<template>
  <el-dialog
    v-model="visible"
    :title="`分配权限 - ${roleData.roleName || ''}`"
    width="960px"
    :close-on-click-modal="false"
    @open="handleOpen"
  >
    <div v-loading="loadingTree">
      <el-row class="perm-header" :gutter="0">
        <el-col :span="8">菜单权限</el-col>
        <el-col :span="16">按钮权限</el-col>
      </el-row>
      <div class="perm-list">
        <el-row v-for="node in visibleNodes" :key="node.id" class="perm-row" :gutter="0">
          <!-- 左侧：菜单 -->
          <el-col :span="8" class="perm-menu" :style="{ paddingLeft: node._level * 20 + 8 + 'px' }">
            <el-checkbox
              :model-value="getMenuChecked(node)"
              :indeterminate="getMenuIndeterminate(node)"
              @change="(val) => toggleMenu(node, val)"
            >
              {{ node.label }}
            </el-checkbox>
          </el-col>
          <!-- 右侧：按钮 -->
          <el-col :span="16" class="perm-btns">
            <template v-if="!node._isParent && node.buttons && node.buttons.length">
              <el-checkbox
                v-for="btn in node.buttons"
                :key="btn.value"
                :model-value="checkedButtons.has(btn.value)"
                @change="(val) => toggleButton(btn.value, val)"
              >
                {{ btn.label }}
              </el-checkbox>
            </template>
          </el-col>
        </el-row>
      </div>
    </div>

    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="saving" @click="handleSave">确定</el-button>
    </template>
  </el-dialog>
</template>

<script setup>
import { getMenuTree, assignPermissions } from '@/api/system'
import { useUserStore } from '@/stores/user'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  roleData: { type: Object, default: () => ({}) },
})

const emit = defineEmits(['update:modelValue', 'success'])

const visible = defineModel('modelValue')

const userStore = useUserStore()
const loadingTree = ref(false)
const saving = ref(false)
const checkedButtons = ref(new Set())
const menuChecked = ref(new Set())

// ---- 菜单树（从后端加载） ----
// 内部格式：{ id, label, buttons: [{ value, label }], children? }
const menuTreeData = ref([])

// ---- 扁平化所有节点（始终全部展开） ----
const visibleNodes = computed(() => {
  const result = []
  function traverse(nodes, level) {
    if (!nodes) return
    nodes.forEach(node => {
      const isParent = !!(node.children && node.children.length)
      result.push({ id: node.id, label: node.label, buttons: node.buttons || [], _level: level, _isParent: isParent })
      if (isParent) {
        traverse(node.children, level + 1)
      }
    })
  }
  traverse(menuTreeData.value, 0)
  return result
})

// ---- 后端数据 → 内部格式 ----
function convertTree(nodes) {
  if (!nodes) return []
  return nodes.map(node => {
    const item = {
      id: node.id,
      label: node.permissionName,
      buttons: (node.buttons || []).map(b => ({ value: b.permKey, label: b.buttonName })),
    }
    if (node.children && node.children.length) {
      item.children = convertTree(node.children)
    }
    return item
  })
}

// ---- 菜单显示状态 ----
function findTreeNode(id, nodes) {
  if (!nodes) return null
  for (const node of nodes) {
    if (node.id === id) return node
    if (node.children) {
      const found = findTreeNode(id, node.children)
      if (found) return found
    }
  }
  return null
}

function isNodeFullyChecked(treeNode) {
  if (!treeNode) return false
  if (!treeNode.children || treeNode.children.length === 0) {
    const btns = treeNode.buttons || []
    if (btns.length === 0) return menuChecked.value.has(treeNode.id)
    return btns.every(btn => checkedButtons.value.has(btn.value))
  }
  return treeNode.children.every(child => isNodeFullyChecked(child))
}

function isNodeIndeterminate(treeNode) {
  if (!treeNode) return false
  if (isNodeFullyChecked(treeNode)) return false
  if (!treeNode.children || treeNode.children.length === 0) {
    return (treeNode.buttons || []).some(btn => checkedButtons.value.has(btn.value))
  }
  return treeNode.children.some(child => isNodeFullyChecked(child) || isNodeIndeterminate(child))
}

function getMenuChecked(node) {
  return isNodeFullyChecked(findTreeNode(node.id, menuTreeData.value))
}

function getMenuIndeterminate(node) {
  return isNodeIndeterminate(findTreeNode(node.id, menuTreeData.value))
}

// ---- 交互 ----
function toggleButton(value, val) {
  const newSet = new Set(checkedButtons.value)
  if (val) newSet.add(value)
  else newSet.delete(value)
  checkedButtons.value = newSet
}

function toggleMenu(node, val) {
  const treeNode = findTreeNode(node.id, menuTreeData.value)
  if (!treeNode) return

  if (node._isParent) {
    const leafIds = []
    const leafButtons = []
    function collectLeaves(n) {
      if (n.children && n.children.length) n.children.forEach(collectLeaves)
      else {
        leafIds.push(n.id)
        ;(n.buttons || []).forEach(b => leafButtons.push(b.value))
      }
    }
    collectLeaves(treeNode)
    const newMenu = new Set(menuChecked.value)
    const newBtns = new Set(checkedButtons.value)
    if (val) {
      leafIds.forEach(id => newMenu.add(id))
      leafButtons.forEach(v => newBtns.add(v))
    } else {
      leafIds.forEach(id => newMenu.delete(id))
      leafButtons.forEach(v => newBtns.delete(v))
    }
    menuChecked.value = newMenu
    checkedButtons.value = newBtns
  } else {
    const newMenu = new Set(menuChecked.value)
    const newBtns = new Set(checkedButtons.value)
    if (val) {
      newMenu.add(node.id)
      ;(node.buttons || []).forEach(b => newBtns.add(b.value))
    } else {
      newMenu.delete(node.id)
      ;(node.buttons || []).forEach(b => newBtns.delete(b.value))
    }
    menuChecked.value = newMenu
    checkedButtons.value = newBtns
  }
}

// ---- 收集所有叶子节点ID和按钮 ----
function getAllLeafIds(nodes) {
  const result = []
  function traverse(list) {
    if (!list) return
    list.forEach(n => {
      if (n.children && n.children.length) traverse(n.children)
      else result.push(n.id)
    })
  }
  traverse(nodes)
  return result
}

function getAllButtonValues(nodes) {
  const result = []
  function traverse(list) {
    if (!list) return
    list.forEach(n => {
      if (n.children && n.children.length) traverse(n.children)
      ;(n.buttons || []).forEach(b => result.push(b.value))
    })
  }
  traverse(nodes)
  return result
}

// ---- 从后端加载当前用户可分配的权限 + 目标角色已有权限 ----
const handleOpen = async () => {
  checkedButtons.value = new Set()
  menuChecked.value = new Set()
  menuTreeData.value = []
  loadingTree.value = true

  try {
    const currentRoleId = userStore.userInfo.roleId

    // 当前用户可分配的权限树（只能分配自己拥有的权限）
    const currentUserRes = await getMenuTree(currentRoleId)
    const currentUserTree = currentUserRes.data || currentUserRes || []
    menuTreeData.value = convertTree(currentUserTree)

    // 目标角色已有权限
    if (props.roleData.id && props.roleData.id !== currentRoleId) {
      const roleRes = await getMenuTree(props.roleData.id)
      const roleTree = roleRes.data || roleRes || []

      // 提取所有被选中的叶子菜单ID和按钮
      const selectedMenuIds = new Set()
      const selectedButtonKeys = new Set()

      function collectRolePermissions(nodes) {
        if (!nodes) return
        nodes.forEach(node => {
          if (node.children && node.children.length) {
            collectRolePermissions(node.children)
          } else {
            selectedMenuIds.add(node.id)
            ;(node.buttons || []).forEach(b => {
              if (b.permKey) selectedButtonKeys.add(b.permKey)
            })
          }
        })
      }
      collectRolePermissions(roleTree)

      menuChecked.value = selectedMenuIds
      checkedButtons.value = selectedButtonKeys
    }
  } catch {
    KhMessageFn.error('加载权限数据失败')
  } finally {
    loadingTree.value = false
  }
}

// ---- 保存 ----
const handleSave = async () => {
  saving.value = true
  try {
    // 构建提交数据：每个选中的菜单 + 该菜单下选中的按钮
    const menus = []
    const allTree = menuTreeData.value

    function collect(nodes) {
      if (!nodes) return
      nodes.forEach(node => {
        const hasChildren = node.children && node.children.length > 0
        if (!hasChildren && menuChecked.value.has(node.id)) {
          // 叶子菜单已选中
          const allBtnValues = (node.buttons || []).map(b => b.value)
          const selectedBtnValues = allBtnValues.filter(v => checkedButtons.value.has(v))

          // 如果选中了所有按钮（或没有按钮），AllowedButtons 传 null（全部授权）
          // 如果只选了部分按钮，传选中的按钮列表
          const allowedButtons = (allBtnValues.length > 0 && selectedBtnValues.length < allBtnValues.length)
            ? selectedBtnValues
            : null

          menus.push({
            permissionId: node.id,
            allowedButtons: allowedButtons,
          })
        }
        if (hasChildren) collect(node.children)
      })
    }
    collect(allTree)

    const res = await assignPermissions({
      roleId: props.roleData.id,
      menus,
    })

    if (res.code === 200) {
      KhMessageFn.success('权限分配成功')
      visible.value = false
      emit('success')
    } else {
      KhMessageFn.error(res.message || '权限分配失败')
    }
  } catch {
    KhMessageFn.error('权限分配失败')
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.perm-header {
  font-size: 13px;
  font-weight: 600;
  color: #606266;
  margin-bottom: 8px;
}

.perm-header .el-col {
  padding: 4px 12px;
}

.perm-list {
  max-height: 480px;
  overflow-y: auto;
}

.perm-row {
  border-bottom: none;
}

.perm-menu {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px 8px !important;
  min-height: 36px;
}

.perm-btns {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 4px 12px;
  padding: 4px 12px !important;
}

.perm-btns :deep(.el-checkbox) {
  margin-right: 0 !important;
}
</style>
