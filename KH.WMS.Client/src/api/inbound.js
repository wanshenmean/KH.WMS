import request from '@/utils/request'

/** 创建入库单（含明细行） */
export function createInboundOrder(data) {
  return request.post('/api/inbound-order', data)
}

/** 更新入库单（含明细行） */
export function updateInboundOrder(id, data) {
  return request.put(`/api/inbound-order/${id}`, data)
}

/** 获取入库单详情（含明细行） */
export function getInboundOrderDetail(id) {
  return request.get(`/api/inbound-order/${id}/detail`)
}

/** 收货 */
export function receiveInboundOrder(orderId, data) {
  return request.post(`/api/inbound-container-bind/receive/${orderId}`, data)
}

/** 组盘 */
export function bindInboundOrder(data) {
  return request.post('/api/inbound-container-bind/bind', data)
}

/** 收货并组盘 */
export function receiveAndBindInboundOrder(data) {
  return request.post('/api/inbound-container-bind/receive-and-bind', data)
}

/** 查询入库单的组盘记录 */
export function getContainerBinds(orderId) {
  return request.get(`/api/inbound-container-bind/by-order/${orderId}`)
}

/** 按容器编号查询组盘记录 */
export function getContainerBindsByContainer(containerCode) {
    return request.get(`/api/inbound-container-bind/by-container/${containerCode}`)
}

/** 取消组盘（解除容器绑定，回滚容器状态与入库单组盘状态） */
export function cancelContainerBind(id) {
    return request.post(`/api/inbound-container-bind/cancel/${id}`)
}

/** 请求上架（分配库位 + 创建上架任务） */
export function putawayContainerBind(headerIds) {
    return request.post('/api/inbound-container-bind/putaway', headerIds)
}

/** 删除组盘记录 */
export function deleteContainerBind(id) {
    return request.delete(`/api/inbound-container-bind/delete/${id}`)
}
