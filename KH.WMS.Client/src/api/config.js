import request from '@/utils/request'

// ========== 全局配置 ==========

/** 获取配置分组列表 */
export function getGlobalConfigGroups() {
    return request.get('/api/global-config/groups')
}

/** 批量更新配置项 */
export function batchUpdateGlobalConfig(data) {
    return request.put('/api/global-config/batch', { items: data })
}

/** 设置配置项状态（启用/禁用） */
export function setGlobalConfigStatus(id, status) {
    return request.put(`/api/global-config/set-status/${id}`, { status })
}

/** 重置分组所有配置为默认值 */
export function resetGlobalConfig(configGroup) {
    return request.post(`/api/global-config/reset/${configGroup}`)
}
