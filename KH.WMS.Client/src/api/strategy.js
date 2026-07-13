import request from '@/utils/request'

/** 获取策略选项聚合数据（策略类型 + 规则编码映射） */
export function getStrategyOptions() {
  return request.get('/api/strategy/query/options')
}

/** 获取策略链详情（含步骤列表） */
export function getChainDetail(id) {
  return request.get(`/api/strategy-chain/${id}/detail`)
}

/** 新增策略链（含步骤） */
export function createChain(data) {
  return request.post('/api/strategy-chain', data)
}

/** 更新策略链（含步骤） */
export function updateChain(id, data) {
  return request.put(`/api/strategy-chain/${id}`, data)
}
