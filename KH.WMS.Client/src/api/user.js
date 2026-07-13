import request from '@/utils/request'

/** 获取当前用户信息 */
export function getUserInfo() {
  return request.get('/api/user/info')
}

/** 获取用户权限 */
export function getUserPermissions(roleId) {
  return request.get(`/api/permission/menu-tree/role/${roleId}`, { showLoading: false })
}
