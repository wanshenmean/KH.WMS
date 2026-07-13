import request from '@/utils/request'

/** 无单据组盘再入库 */
export function adhocInbound(data) {
  return request.post('/api/adhoc/inbound', data)
}

/** 无单据出库（按库存明细ID列表） */
export function adhocOutbound(data) {
  return request.post('/api/adhoc/outbound', data)
}

/** 指定托盘号出库 */
export function adhocOutboundByContainer(data) {
  return request.post('/api/adhoc/outbound-by-container', data)
}

/** 指定货位出库 */
export function adhocOutboundByLocation(data) {
  return request.post('/api/adhoc/outbound-by-location', data)
}

/** 按库区出库 */
export function adhocOutboundByZone(data) {
  return request.post('/api/adhoc/outbound-by-zone', data)
}

/** 按巷道出库 */
export function adhocOutboundByAisle(data) {
  return request.post('/api/adhoc/outbound-by-aisle', data)
}

/** 按出库口出库 */
export function adhocOutboundByPort(data) {
  return request.post('/api/adhoc/outbound-by-port', data)
}

/** 起始地址→目的地址上架 */
export function adhocPutawayFromTo(data) {
  return request.post('/api/adhoc/putaway-from-to', data)
}

/** 直接上架到指定地址 */
export function adhocPutawayTo(data) {
  return request.post('/api/adhoc/putaway-to', data)
}

/** 按多种条件查询库存（用于无单据出库页筛选） */
export function adhocQueryInventory(data) {
  return request.post('/api/adhoc/query-inventory', data)
}

/** 路线校验（起点地址能否到达目标地址） */
export function adhocCheckRoute(data) {
  return request.post('/api/adhoc/check-route', data)
}
