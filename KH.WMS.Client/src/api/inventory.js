import request from '@/utils/request'

/** 库存统计数据 */
export function getInventoryStatData() {
  return request.get('/api/inventory-header/stat')
}

/** 冻结库存（按托盘） */
export function freezeInventory(id, reason) {
  return request.post(`/api/inventory-header/freeze/${id}`, { reason })
}

/** 解冻库存（按托盘） */
export function unfreezeInventory(id) {
    return request.post(`/api/inventory-header/unfreeze/${id}`)
}

/** 创建库存快照 */
export function createSnapshot(data) {
    return request.post('/api/inventory-snapshot/create-snapshot', data)
}
