/**
 * 行样式预设颜色方案
 * 每种预设包含协调的 backgroundColor 和 color，可直接用于 KhTable 的 rowStyle。
 *
 * @example
 * // 在 rowStyle 中返回预设名称字符串
 * :row-style="(row) => row.status === 'LOCKED' ? 'danger' : ''"
 *
 * // 也可以自定义颜色或混合使用
 * :row-style="(row) => row.status === 'LOCKED' ? 'danger' : { backgroundColor: '#f0f0f0' }"
 */

export const ROW_STYLE_PRESETS = {
  primary: { backgroundColor: '#e8f4fd', color: '#1a6fb5' },
  success: { backgroundColor: '#e8f8ef', color: '#1a8c52' },
  warning: { backgroundColor: '#fdf6ec', color: '#b8860b' },
  danger:  { backgroundColor: '#fef0f0', color: '#c45656' },
  info:    { backgroundColor: '#f0f2f5', color: '#646a73' },
  cyan:    { backgroundColor: '#e8fafb', color: '#0d8a8f' },
  purple:  { backgroundColor: '#f3ecfd', color: '#7c3aed' },
  orange:  { backgroundColor: '#fef3e8', color: '#c05621' },
  pink:    { backgroundColor: '#fde8ef', color: '#c2185b' },
  dark:    { backgroundColor: '#e8e8e8', color: '#374151' },
  gold:    { backgroundColor: '#fdf9e8', color: '#a16207' },
  mint:    { backgroundColor: '#e6f9f0', color: '#0f766e' },
}

/**
 * 解析行样式：支持预设名称字符串或自定义样式对象。
 * @param {string|Object|undefined} style - 预设名称（如 'danger'）、样式对象或空值
 * @returns {Object} CSS 样式对象
 */
export function resolveRowStyle(style) {
  if (!style) return {}
  if (typeof style === 'string') {
    return ROW_STYLE_PRESETS[style] || {}
  }
  return style
}
