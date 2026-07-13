import request from '@/utils/request'

/** 创建出库单（含明细行） */
export function createOutboundOrder(data) {
  return request.post('/api/outbound-order', data)
}

/** 更新出库单（含明细行） */
export function updateOutboundOrder(id, data) {
  return request.put(`/api/outbound-order/${id}`, data)
}

/** 获取出库单详情（含明细行） */
export function getOutboundOrderDetail(id) {
  return request.get(`/api/outbound-order/${id}/detail`)
}

/** 确认出库单（DRAFT → CONFIRMED） */
export function confirmOutboundOrder(id) {
  return request.post(`/api/outbound-order/confirm/${id}`)
}

/** 执行库存分配 */
export function allocateOutboundOrder(orderId) {
  return request.post(`/api/outbound-allocation/allocate/${orderId}`)
}

/** 按出库单ID查询分配记录 */
export function getAllocationByOrder(orderId) {
  return request.get(`/api/outbound-allocation/by-order/${orderId}`)
}

/** 生成分配单的出库任务 */
export function generateAllocationTasks(allocationHeaderId) {
  return request.post(`/api/outbound-allocation/generate-tasks/${allocationHeaderId}`)
}
