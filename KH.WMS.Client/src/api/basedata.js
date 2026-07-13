import request from '@/utils/request'

// ========== 物料分类 ==========

/** 获取物料分类树 */
export function getCategoryTree() {
  return request.get('/api/material-category/tree')
}

/** 获取物料分类详情 */
export function getCategoryDetail(id) {
  return request.get(`/api/material-category/${id}`)
}

/** 新增物料分类 */
export function createCategory(data) {
  return request.post('/api/material-category', data)
}

/** 修改物料分类 */
export function updateCategory(data) {
  return request.put('/api/material-category', data)
}

/** 删除物料分类 */
export function deleteCategory(id) {
  return request.delete(`/api/material-category/${id}`)
}

// ========== 周转分类配置 ==========

/** 获取周转分类配置列表 */
export function getTurnoverClassList(params) {
  return request.get('/api/cfg-turnover-class/list', { params })
}

/** 新增周转分类配置 */
export function createTurnoverClass(data) {
  return request.post('/api/cfg-turnover-class', data)
}

/** 修改周转分类配置 */
export function updateTurnoverClass(data) {
  return request.put('/api/cfg-turnover-class', data)
}

/** 删除周转分类配置 */
export function deleteTurnoverClass(id) {
  return request.delete(`/api/cfg-turnover-class/${id}`)
}

// ========== 物料周转分析 ==========

/** 获取物料周转分析列表 */
export function getMaterialTurnoverList(params) {
  return request.get('/api/material-turnover/list', { params })
}

/** 触发周转分析计算 */
export function calculateMaterialTurnover(params) {
  return request.post('/api/material-turnover/calculate', params)
}
