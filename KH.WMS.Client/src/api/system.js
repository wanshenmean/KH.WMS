import request from '@/utils/request'

// ========== 用户管理 ==========

/** 获取用户列表 */
export function getUserList(params) {
  return request.get('/api/user/list', { params })
}

/** 获取用户详情 */
export function getUserDetail(id) {
  return request.get(`/api/user/${id}`)
}

/** 新增用户 */
export function createUser(data) {
  return request.post('/api/user', data)
}

/** 修改用户 */
export function updateUser(data) {
  return request.put('/api/user', data)
}

/** 删除用户 */
export function deleteUser(id) {
  return request.delete(`/api/user/${id}`)
}

/** 重置密码 */
export function resetUserPassword(id) {
  return request.post(`/api/user/reset-password/${id}`)
}

/** 修改用户状态 */
export function changeUserStatus(id, status) {
  return request.put(`/api/user/${id}/status`, { status })
}

// ========== 角色管理 ==========

/** 获取角色列表 */
export function getRoleList(params) {
  return request.get('/api/role/list', { params })
}

/** 获取角色选项（下拉选择用） */
export function getRoleOptions() {
  return request.get('/api/role/options')
}

/** 新增角色 */
export function createRole(data) {
  return request.post('/api/role', data)
}

/** 修改角色 */
export function updateRole(data) {
  return request.put('/api/role', data)
}

/** 删除角色 */
export function deleteRole(id) {
  return request.delete(`/api/role/${id}`)
}

/** 获取角色权限 */
export function getRolePermissions(id) {
  return request.get(`/api/role/${id}/permissions`)
}

/** 保存角色权限 */
export function saveRolePermissions(id, data) {
  return request.put(`/api/role/${id}/permissions`, data)
}

// ========== 字典管理 ==========

/** 获取字典类型列表 */
export function getDictTypeList() {
  return request.get('/api/dict/all')
}

/** 新增字典类型 */
export function createDictType(data) {
  return request.post('/api/dict/create', data)
}

/** 修改字典类型 */
export function updateDictType(data) {
  return request.post('/api/dict/update', data)
}

/** 删除字典类型 */
export function deleteDictType(id) {
  return request.delete(`/api/dict/type/${id}`)
}

/** 获取字典数据列表 */
export function getDictDataList(id, params) {
  return request.post(`/api/dict/items/list/${id}`, params)
}

/** 新增字典数据 */
export function createDictData(data) {
  return request.post('/api/dict/items/save', data)
}

/** 修改字典数据 */
export function updateDictData(data) {
  return request.post('/api/dict/items/save', data)
}

/** 删除字典数据 */
export function deleteDictData(id) {
  return request.delete(`/api/dict/data/${id}`)
}

/** 获取字典数据（按字典类型） */
export function getDictDataByType(dictType) {
  return request.get(`/api/dict/items/${dictType}`, { showLoading: false })
}

// ========== 菜单管理 ==========

/** 获取菜单树（全部） */
export function getAllMenuTree() {
  return request.get('/api/permission/menu-tree', { showLoading: false })
}

/** 获取菜单树（按角色） */
export function getMenuTree(roleId) {
  return request.get(`/api/permission/menu-tree/role/${roleId}`, { showLoading: false })
}

/** 分配角色权限 */
export function assignPermissions(data) {
  return request.post('/api/permission/assign', data)
}

/** 获取菜单详情 */
export function getMenuDetail(id) {
  return request.get(`/api/permission/${id}`, { showLoading: false })
}

/** 新增/编辑菜单（含按钮，统一走 save 接口） */
export function saveMenu(data) {
  return request.post('/api/permission/save', data)
}

/** 新增菜单（兼容旧调用，内部委托 saveMenu） */
export function createMenu(data) {
  return saveMenu(data)
}

/** 修改菜单（兼容旧调用，内部委托 saveMenu） */
export function updateMenu(data) {
  return saveMenu(data)
}

/** 删除菜单 */
export function deleteMenu(id) {
  return request.delete(`/api/permission/delete/${id}`)
}

// ========== 附件管理 ==========

/** 删除附件 */
export function deleteAttachment(id) {
    return request.delete(`/api/attachment/delete/${id}`)
}

/** 下载附件（返回 blob） */
export function downloadFile(id) {
    return request.get(`/api/file/download/${id}`, { responseType: 'blob', showLoading: false })
}

// ========== 系统文本日志 ==========

/** 查询文本日志（分页 + 条件筛选：日期范围/级别/LogType/关键字/RequestId） */
export function getSystemLogs(data) {
  return request.post('/api/system-log/query', data)
}

/** 获取可用日志日期列表（降序） */
export function getLogDates() {
  return request.get('/api/system-log/dates', { showLoading: false })
}

/** 获取所有日志文件列表（分类+文件名+日期+大小，供文件树） */
export function getLogFiles() {
  return request.get('/api/system-log/files', { showLoading: false })
}

/** 读取原始日志内容（多文件拼接 + 分页 + 关键字/级别过滤） */
export function getLogContent(data) {
  return request.post('/api/system-log/content', data)
}
