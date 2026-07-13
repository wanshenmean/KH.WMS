import request from '@/utils/request'

/** 需要从请求参数中排除的保留字段 */
const RESERVED_KEYS = ['pageNum', 'pageSize', 'pageCount', 'total', 'sortConditions', 'filters']

/**
 * 将 KhTable 传来的扁平查询参数转换为后端分页查询结构
 * @param {Object} params - KhTable doLoad 传来的参数
 * @param {Array}  params.sortConditions - 排序条件 [{ field, direction }]
 * @param {number} params.pageNum - 页码（从 1 开始）
 * @param {number} params.pageSize - 每页条数
 * @param {Object} params.searchColumns - 搜索列配置（可选，用于推断 operator）
 * @returns {Object} 后端分页查询结构 { pageIndex, pageSize, sortConditions, filters }
 */
export function buildPageQuery(params) {
  const { pageNum, pageSize, sortConditions, searchColumns, filters: headerFilters, ...rest } = params

  // 构建过滤条件
  const filters = []

  // 如果传了 searchColumns，用它来推断每个字段的 operator
  const operatorMap = {}
  if (Array.isArray(searchColumns)) {
    for (const col of searchColumns) {
      if (!col.prop) continue
      if (col.filterOperator) {
        operatorMap[col.prop] = col.filterOperator
      } else if (col.type === 'select') {
        operatorMap[col.prop] = 'equals'
      } else if (col.type === 'input') {
        operatorMap[col.prop] = 'contains'
      } else if (col.type === 'number') {
        operatorMap[col.prop] = 'equals'
      } else if (col.type === 'date-picker' || col.type === 'date-range') {
        operatorMap[col.prop] = 'equals'
      } else {
        operatorMap[col.prop] = 'contains'
      }
    }
  }

  for (const [key, value] of Object.entries(rest)) {
    // 跳过空值和保留字段
    if (RESERVED_KEYS.includes(key)) continue
    if (value === '' || value === null || value === undefined) continue
    if (Array.isArray(value) && value.length === 0) continue

    // 数组值（如多选下拉）使用 in 操作符
    if (Array.isArray(value)) {
      filters.push({ field: key, operator: 'in', value: value.join(',') })
    } else {
      const operator = operatorMap[key] || 'contains'
      filters.push({ field: key, operator, value: String(value) })
    }
  }

  // 合并 KhTable 表头筛选产生的 filters
  if (Array.isArray(headerFilters) && headerFilters.length > 0) {
    filters.push(...headerFilters)
  }

  return {
    pageIndex: pageNum || 1,
    pageSize: pageSize || 30,
    sortConditions: sortConditions || [],
    filters,
  }
}

/**
 * 创建模块的 CRUD API 方法集
 * @param {string} module - 模块名，如 'user'、'role'
 * @returns {Object} CRUD 方法集合
 *
 * @example
 * const api = useCrudApi('user')
 * api.pageList({ pageNum: 1, pageSize: 10, userName: 'admin' })
 * api.create({ username: 'test' })
 * api.update({ id: 1, username: 'test2' })
 * api.delete(1)
 * api.detail(1)
 * api.formConfig()
 */
export function useCrudApi(module) {
  const prefix = `/api/${module}`
  return {
    /** 分页查询（自动将扁平参数转换为后端分页结构） */
    pageList: (params) => request.post(`${prefix}/pagelist`, buildPageQuery(params)),
    /** 获取详情 */
    detail: (id) => request.get(`${prefix}/${id}`),
    /** 新增 */
    create: (data) => request.post(`${prefix}/create`, data),
    /** 修改 */
    update: (data) => request.post(`${prefix}/update`, data),
    /** 删除 */
    delete: (id) => request.delete(`${prefix}/delete/${id}`),
    /** 设置启用/禁用状态 */
    setStatus: (id, status) => request.put(`${prefix}/status/${id}`, { status }),
    /** 获取表单配置（后端动态返回 formColumns） */
    formConfig: () => request.get(`${prefix}/form-config`),
    /** 导出（query 为 buildPageQuery 返回的结构，exportAll=true 导出全部，false 导出当前页，columns 为列配置） */
    export: (query, exportAll = true, columns = null) => request.post(`${prefix}/export`, {
      ...query,
      exportAll,
      ...(columns ? { columns } : {}),
    }),
  }
}
