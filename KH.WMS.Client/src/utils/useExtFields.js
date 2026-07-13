import request from '@/utils/request'

/**
 * 扩展字段 composable
 * 用于页面动态加载和渲染扩展字段配置，支持 KhPage CRUD 模式和自定义表单模式
 *
 * @param {string} apiUrl - form-config API 路径，如 '/api/inbound-order/form-config'
 */
export function useExtFields(apiUrl) {
  const extConfig = ref({ columns: [], lineColumns: [] })
  const extLoaded = ref(false)

  /** 加载扩展字段配置 */
  const loadExtConfig = async () => {
    try {
      const res = await request.get(apiUrl)
      const data = res.data || res
      extConfig.value = {
        columns: data.columns || [],
        lineColumns: data.lineColumns || [],
      }
    } catch {
      extConfig.value = { columns: [], lineColumns: [] }
    } finally {
      extLoaded.value = true
    }
  }

  // ==================== 列合并（表单 + 表格） ====================

  /**
   * 在 remark 之前插入扩展列
   * @param {Array} baseColumns - 基础列
   * @param {Array} extColumns - 扩展列配置
   * @returns {Array} 合并后的列
   */
  const _insertExtColumns = (baseColumns, extColumns) => {
    if (!extColumns.length) return baseColumns
    const decorated = extColumns.map(col => ({ ...col, isExt: true, clearable: true }))
    const remarkIdx = baseColumns.findIndex(c => c.prop === 'remark')
    if (remarkIdx >= 0) {
      return [...baseColumns.slice(0, remarkIdx), ...decorated, ...baseColumns.slice(remarkIdx)]
    }
    return [...baseColumns, ...decorated]
  }

  /** 合并表单列 */
  const mergedColumns = (baseColumns) => _insertExtColumns(baseColumns, extConfig.value.columns)

  /** 合并行级表单列 */
  const mergedLineColumns = (baseLineColumns) => _insertExtColumns(baseLineColumns, extConfig.value.lineColumns)

  /** 合并表格列 */
  const mergedTableColumns = (baseTableColumns) => _insertExtColumns(baseTableColumns, extConfig.value.columns)

  // ==================== 提交：提取并清理扩展字段 ====================

  /**
   * 从表单数据中提取扩展字段，序列化为 JSON 字符串，同时从 formData 中删除这些字段
   * 用于 KhPage 的 beforeSubmit 钩子
   *
   * 用法:
   *   const beforeSubmit = (data) => {
   *     const raw = extractAndCleanExtData(data)
   *     if (raw) data.extDataRaw = raw
   *   }
   *
   * @param {Object} formData - KhDialog 传来的完整表单数据（会被原地修改）
   * @returns {string|null} 扩展字段的 JSON 字符串，无扩展数据时返回 null
   */
  const extractAndCleanExtData = (formData) => {
    const columns = extConfig.value.columns
    if (!columns.length) return null
    const extFields = {}
    for (const col of columns) {
      const val = formData[col.prop]
      if (val !== undefined && val !== null && val !== '') {
        extFields[col.prop] = val
      }
      delete formData[col.prop]
    }
    return Object.keys(extFields).length > 0 ? JSON.stringify(extFields) : null
  }

  /**
   * 从明细行数据中提取行级扩展字段（旧版兼容，入库/出库单页面使用）
   */
  const extractExtData = (formData) => {
    if (!extConfig.value.columns.length) return null
    const extFields = {}
    for (const col of extConfig.value.columns) {
      const val = formData[col.prop]
      if (val !== undefined && val !== null && val !== '') {
        extFields[col.prop] = val
      }
    }
    return Object.keys(extFields).length > 0 ? extFields : null
  }

  /**
   * 从明细行数据中提取行级扩展字段值（旧版兼容）
   */
  const extractLineExtData = (linesData) => {
    if (!extConfig.value.lineColumns.length) return null
    const lineExtData = {}
    for (let i = 0; i < linesData.length; i++) {
      const line = linesData[i]
      const extFields = {}
      for (const col of extConfig.value.lineColumns) {
        const val = line[col.prop]
        if (val !== undefined && val !== null && val !== '') {
          extFields[col.prop] = val
        }
      }
      if (Object.keys(extFields).length > 0) {
        lineExtData[String(i + 1)] = extFields
      }
    }
    return Object.keys(lineExtData).length > 0 ? lineExtData : null
  }

  // ==================== 加载：展开 ExtData 到扁平字段 ====================

  /**
   * 将行数据的 ExtData JSON 字符串展开为扁平字段，合并到行数据中
   * 用于 KhPage 的自定义 load 函数，让表格和编辑表单能显示扩展字段
   *
   * 用法:
   *   const loadFn = async (params) => {
   *     const res = await crudApi.pageList(params)
   *     res.data.items = res.data.items.map(row => flattenExtData(row))
   *     return res
   *   }
   *
   * @param {Object} row - 后端返回的行数据（会被原地修改）
   * @returns {Object} 修改后的行数据
   */
  const flattenExtData = (row) => {
    if (!row || !row.extData) return row
    try {
      const extData = typeof row.extData === 'string' ? JSON.parse(row.extData) : row.extData
      Object.assign(row, extData)
    } catch {
      // ExtData 格式异常时忽略
    }
    return row
  }

  /**
   * 将反序列化的 ExtData 合并到表单数据中（旧版兼容，入库/出库单页面使用）
   */
  const mergeExtDataToForm = (formData, extDataFlattened) => {
    if (!extDataFlattened) return
    for (const [key, value] of Object.entries(extDataFlattened)) {
      formData[key] = value
    }
  }

  /**
   * 将反序列化的行级 ExtData 合并到明细行数据中（旧版兼容）
   */
  const mergeLineExtDataToForm = (lines, lineExtDataFlattened, detailLines) => {
    if (!lineExtDataFlattened || !detailLines) return
    for (let i = 0; i < lines.length; i++) {
      const detailLine = detailLines[i]
      if (detailLine && lineExtDataFlattened[detailLine.id]) {
        Object.assign(lines[i], lineExtDataFlattened[detailLine.id])
      }
    }
  }

  return {
    extConfig,
    extLoaded,
    loadExtConfig,
    mergedColumns,
    mergedLineColumns,
    mergedTableColumns,
    extractAndCleanExtData,
    extractExtData,
    extractLineExtData,
    flattenExtData,
    mergeExtDataToForm,
    mergeLineExtDataToForm,
    /**
     * 创建带 ExtData 展平的分页加载函数，传给 KhPage 的 :load
     * @param {Object} crudApi - useCrudApi 返回的 CRUD API 对象
     * @param {Array} searchCols - 页面的 searchColumns
     * @returns {Function} 符合 KhTable load 签名的函数
     */
    withFlatExtLoad: (crudApi, searchCols) => async (params) => {
      const res = await crudApi.pageList({ ...params, searchColumns: searchCols })
      const items = res.data?.items ?? []
      items.forEach(row => flattenExtData(row))
      return { data: items, total: res.data?.total ?? 0 }
    },
  }
}
