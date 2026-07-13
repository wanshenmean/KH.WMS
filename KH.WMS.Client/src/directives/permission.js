import { usePermissionStore } from '@/stores/permission'

/**
 * v-permission 按钮级权限指令
 *
 * 用法：
 *   <el-button v-permission="'sys:user:add'">新增</el-button>
 *   <el-button v-permission="['sys:user:add', 'sys:user:edit']">操作</el-button>
 */
function checkPermission(el, binding) {
  const { value } = binding
  if (!value) return

  const permissionStore = usePermissionStore()
  const requiredPermissions = Array.isArray(value) ? value : [value]

  const hasAccess = requiredPermissions.some(perm =>
    permissionStore.hasButtonPermission(perm)
  )

  if (!hasAccess) {
    el.parentNode && el.parentNode.removeChild(el)
  }
}

export default {
  mounted(el, binding) {
    checkPermission(el, binding)
  },
  updated(el, binding) {
    checkPermission(el, binding)
  },
}
