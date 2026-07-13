import { useDictStore } from '@/stores/dict'

/**
 * 从 columns 配置中收集所有字典类型
 * @param {Array} columns - 列配置数组
 * @returns {Set<string>} 字典类型集合
 */
export function collectDictTypes(columns) {
  const types = new Set()
  if (!Array.isArray(columns)) return types
  for (const col of columns) {
    if (!col) continue
    for (const key of ['options', 'tagMap', 'tagTypeMap', 'filterOptions']) {
      if (typeof col[key] === 'string' && col[key].startsWith('dict:')) {
        types.add(col[key].slice(5))
      }
    }
    // 收集 cascade-select 各级的字典引用
    if (col.type === 'cascade-select' && Array.isArray(col.cascadeItems)) {
      for (const level of col.cascadeItems) {
        if (typeof level.options === 'string' && level.options.startsWith('dict:')) {
          types.add(level.options.slice(5))
        }
      }
    }
  }
  return types
}

/**
 * 解析单个字典引用为选项数组
 * @param {string} dictRef - 'dict:xxx' 格式
 * @param {Array} cache - 字典缓存列表
 * @returns {Array<{label: string, value: *}>}
 */
export function resolveDictToOptions(dictRef, cache) {
  if (!Array.isArray(cache)) return []
  return cache.map(({ label, value }) => ({ label, value }))
}

/**
 * 解析单个字典引用为 tagMap（value -> label 映射）
 * @param {string} dictRef - 'dict:xxx' 格式
 * @param {Array} cache - 字典缓存列表
 * @returns {Object}
 */
export function resolveDictToTagMap(dictRef, cache) {
  if (!Array.isArray(cache)) return {}
  const map = {}
  for (const item of cache) {
    map[item.value] = item.label
  }
  return map
}

/**
 * 解析单个字典引用为 tagTypeMap（value -> tagType 映射）
 * @param {string} dictRef - 'dict:xxx' 格式
 * @param {Array} cache - 字典缓存列表
 * @returns {Object}
 */
/** 后端未返回 tagColor 时的兜底颜色轮转池 */
const fallbackColors = ['primary', 'success', 'warning', 'danger', 'info']

export function resolveDictToTagTypeMap(dictRef, cache) {
  if (!Array.isArray(cache)) return {}
  const map = {}
  let fallbackIndex = 0
  for (const item of cache) {
    if (item.tagType) {
      map[item.value] = item.tagType
    } else {
      map[item.value] = fallbackColors[fallbackIndex % fallbackColors.length]
      fallbackIndex++
    }
  }
  return map
}

/**
 * 解析列配置中的字典引用
 * @param {Object} col - 列配置
 * @param {Object} dictCache - 字典 store 的 cache
 * @returns {Object} 解析后的列配置副本
 */
export function resolveColumn(col, dictCache) {
  const resolved = { ...col }

  // 解析 options
  if (typeof col.options === 'string' && col.options.startsWith('dict:')) {
    const dictType = col.options.slice(5)
    resolved.options = resolveDictToOptions(col.options, dictCache[dictType])
  }

  // 解析 tagMap，同时自动从同一字典生成 tagTypeMap（API 已返回 tagColor）
  if (typeof col.tagMap === 'string' && col.tagMap.startsWith('dict:')) {
    const dictType = col.tagMap.slice(5)
    const cache = dictCache[dictType]
    resolved.tagMap = resolveDictToTagMap(col.tagMap, cache)
    // 如果列上没有显式指定 tagTypeMap，则自动从同一字典缓存中提取 tagType
    if (col.tagTypeMap === undefined) {
      resolved.tagTypeMap = resolveDictToTagTypeMap(col.tagMap, cache)
    }
  }

  // 解析显式指定的 tagTypeMap
  if (typeof col.tagTypeMap === 'string' && col.tagTypeMap.startsWith('dict:')) {
    const dictType = col.tagTypeMap.slice(5)
    resolved.tagTypeMap = resolveDictToTagTypeMap(col.tagTypeMap, dictCache[dictType])
  }

  // 解析 filterOptions
  if (typeof col.filterOptions === 'string' && col.filterOptions.startsWith('dict:')) {
    const dictType = col.filterOptions.slice(5)
    resolved.filterOptions = resolveDictToOptions(col.filterOptions, dictCache[dictType])
  }

  // 支持 options / filterOptions 传入 ref，自动解包
  if (typeof resolved.options === 'object' && typeof resolved.options?.__v_isRef === 'function') {
    resolved.options = resolved.options.value
  }
  if (typeof resolved.filterOptions === 'object' && typeof resolved.filterOptions?.__v_isRef === 'function') {
    resolved.filterOptions = resolved.filterOptions.value
  }

  // 解析 cascade-select 各级的字典引用
  if (col.type === 'cascade-select' && Array.isArray(col.cascadeItems)) {
    resolved.cascadeItems = col.cascadeItems.map(level => {
      const r = { ...level }
      if (typeof level.options === 'string' && level.options.startsWith('dict:')) {
        const dictType = level.options.slice(5)
        r.options = resolveDictToOptions(level.options, dictCache[dictType])
      }
      return r
    })
  }

  return resolved
}
