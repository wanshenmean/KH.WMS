import request from '@/utils/request'

/** 首页统计卡片 */
export function getHomeStat() {
  return request.get('/api/home/stat', { showLoading: false })
}

/** 近N日出入库趋势 */
export function getHomeTrend(days = 7) {
  return request.get('/api/home/trend', { params: { days }, showLoading: false })
}

/** 最近完成的出入库单据 */
export function getRecentDocuments(top = 10) {
  return request.get('/api/home/recent-documents', { params: { top }, showLoading: false })
}

/** 库存概览数据 */
export function getInventoryOverview() {
  return request.get('/api/home/inventory-overview', { showLoading: false })
}

/** 作业概览（任务类型分布 + 库位状态） */
export function getOperationsOverview() {
  return request.get('/api/home/operations', { showLoading: false })
}
