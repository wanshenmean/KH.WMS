import request from '@/utils/request'

/** 登录 */
export function login(data) {
  return request.post('/api/user/login', data)
}

/** 登出 */
export function logout() {
  return request.post('/api/user/logout')
}

/** 刷新token */
export function refreshToken(refreshToken) {
  return request.post('/api/user/refresh-token', { refreshToken })
}

/** 获取RSA公钥 */
export function getPublicKey() {
  return request.get('/api/user/public-key')
}
