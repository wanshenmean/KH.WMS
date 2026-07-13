import request from '@/utils/request'

/** WCS任务完成回调 */
export function completeTaskByWcs(data) {
  return request.post('/api/task-header/complete-by-wcs', data)
}

/** 取消任务 */
export function cancelTask(id) {
  return request.post(`/api/task-header/cancel/${id}`)
}

/** 上架任务货位分配（WCS在接驳口调用，分配实际目标库位） */
export function allocatePutawayLocation(id) {
  return request.post(`/api/task-header/allocate-putaway-location/${id}`)
}
