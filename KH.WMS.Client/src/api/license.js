import request from '@/utils/request'

/**
 * 获取当前 License 状态（白名单：license 失效时也能返回状态 + 机器码，用于引导恢复）。
 * 返回 ApiResponse{ data: { isValid, machineCode, expiresAt, remainingDays, reason, reasonCode, ... } }
 */
export function getLicenseInfo() {
  return request.get('/api/license/info', { showLoading: false })
}

/** 获取当前服务器机器码（白名单，无需登录） */
export function getMachineCode() {
  return request.get('/api/license/machine-code', { showLoading: false })
}

/**
 * 上传 .lic 授权文件（白名单 + 免登录，作为 license 失效时的恢复入口）。
 * 注意：传 FormData 时不要手动设置 Content-Type，axios 会自动带 multipart boundary。
 */
export function importLicense(formData) {
  return request.post('/api/license/upload', formData)
}
