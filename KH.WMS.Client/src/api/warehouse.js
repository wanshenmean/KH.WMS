import request from '@/utils/request'

/** 根据仓库获取库区 */
export function getZonesByWarehouse(warehouseId) {
    return request.get(`/api/warehouse-zone/by-warehouse/${warehouseId}`)
}

/** 根据仓库获取库区和巷道 */
export function getZonesAndAislesByWarehouse(warehouseId) {
    return request.get(`/api/warehouse/zone-aisle/${warehouseId}`)
}

export function getLocationStatData(){
    return request.get('/api/location/stat')
}

/** 根据仓库获取存储库区（用于上架/入库选库区） */
export function getStorageZones(warehouseId) {
    return request.get(`/api/warehouse-zone/storage-zones/${warehouseId}`)
}

/** 根据仓库+库区获取可用库位 */
export function getAvailableLocationsByZone(warehouseId, zoneCode) {
    return request.get('/api/location/available-by-zone', { params: { warehouseId, zoneCode } })
}

/** 出库口分页查询（用于无单据出库选出库口） */
export function getPortPageList(params) {
    return request.post('/api/port/pagelist', params)
}