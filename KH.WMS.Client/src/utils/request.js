import axios from 'axios'
import { ElLoading } from 'element-plus'

// baseURL 来自运行时配置（public/config.js → 打包后 dist/config.js），可部署后修改无需重新构建
const request = axios.create({
  baseURL: window.__APP_CONFIG__?.API_BASE_URL ?? '',
  timeout: 15000,
})

// --- 全局 Loading 遮罩 ---
let loadingCount = 0
let loadingInstance = null

function showLoading() {
  if (loadingCount === 0) {
    loadingInstance = ElLoading.service({
      lock: true,
      text: '',
      background: 'rgba(0, 0, 0, 0.15)',
    })
  }
  loadingCount++
}

function hideLoading() {
  loadingCount--
  if (loadingCount <= 0) {
    loadingCount = 0
    loadingInstance?.close()
    loadingInstance = null
  }
}

// --- Token 刷新状态 ---
let isRefreshing = false
let pendingRequests = []

// --- License 失效(402) 跳转并发守卫：并发请求只跳一次到 /license ---
let isRedirectingToLicense = false

function processQueue(error, newToken = null) {
  pendingRequests.forEach(({ resolve, reject }) => {
    if (error) {
      reject(error)
    } else {
      resolve(newToken)
    }
  })
  pendingRequests = []
}

async function handleForceLogout() {
  loadingCount = 0
  loadingInstance?.close()
  loadingInstance = null
  try {
    const { useUserStore } = await import('@/stores/user')
    const { usePermissionStore } = await import('@/stores/permission')
    const { useDictStore } = await import('@/stores/dict')
    const { useWebSocketStore } = await import('@/stores/websocket')
    useWebSocketStore().closeAll()
    usePermissionStore().clearPermissions()
    useDictStore().clearDict()
    useUserStore().clearAuth()
  } catch (_) {
    // Pinia 可能尚未就绪，兜底清除 localStorage
    localStorage.removeItem('token')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('userInfo')
  }
  KhMessageFn.error('登录已过期，请重新登录')
  // SPA 跳转优先（无整页刷新、提示可保留）：router.replace 到登录页；
  // 若失败（如拦截器与路由守卫并发的 NavigationFailure）则兜底整页硬跳转，确保一定能到登录页。
  // 用动态 import 取 router，避免与 @/router 形成静态循环依赖。
  // 注意：vue-router 4 导航失败时是 resolve 出 NavigationFailure 对象而非 reject，故同时判断返回值与 catch。
  try {
    const { default: router } = await import('@/router')
    const failure = await router.replace('/login')
    if (failure) window.location.href = '/login'
  } catch {
    window.location.href = '/login'
  }
}

// --- 静默续期 Token（从响应头读取后端下发的新 token） ---
const TOKEN_HEADER_ACCESS = 'x-access-token'
const TOKEN_HEADER_REFRESH = 'x-refresh-token'

function silentRefreshToken(headers) {
  const newAccessToken = headers[TOKEN_HEADER_ACCESS]
  const newRefreshToken = headers[TOKEN_HEADER_REFRESH]
  if (!newAccessToken) return

  localStorage.setItem('token', newAccessToken)
  if (newRefreshToken) {
    localStorage.setItem('refreshToken', newRefreshToken)
  }
  try {
    // 需要同步更新 Pinia store（动态导入避免循环依赖）
    import('@/stores/user').then(({ useUserStore }) => {
      const userStore = useUserStore()
      userStore.setToken(newAccessToken)
      if (newRefreshToken) userStore.setRefreshToken(newRefreshToken)
    }).catch(() => {})
  } catch (_) {
    // Pinia 可能尚未就绪
  }
}

// 请求拦截器
request.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    // showLoading: false 的请求不显示全局遮罩（如字典查询、权限加载等）
    if (config.showLoading !== false) {
      showLoading()
    }
    return config
  },
  (error) => {
    hideLoading()
    return Promise.reject(error)
  }
)

// 响应拦截器
request.interceptors.response.use(
  (response) => {
    if (response.config.showLoading !== false) hideLoading()

    // 后端通过响应头下发新 token 时，静默替换（token 快过期自动续期）
    silentRefreshToken(response.headers)

    // axios 成功回调仅在 HTTP 2xx 时触发，直接返回响应体
    return response.data
  },
  async (error) => {
    const originalRequest = error.config

    if (error.response) {
      const { status, data } = error.response

      // License 授权失效（HTTP 402，与 RBAC 的 403 区分）：跳授权恢复页 /license。
      // 并发只跳一次；不清 token（授权恢复后免重登）；已在 /license 页则不再跳避免循环。
      if (status === 402) {
        if (originalRequest.showLoading !== false) hideLoading()
        if (location.pathname === '/license' || isRedirectingToLicense) {
          return Promise.reject(error)
        }
        isRedirectingToLicense = true
        KhMessageFn.error(data?.message || '系统授权校验未通过，请导入授权文件')
        try {
          const { default: router } = await import('@/router')
          const failure = await router.replace({ path: '/license', query: { redirect: location.pathname + location.search } })
          if (failure) location.href = '/license'
        } catch {
          location.href = '/license'
        } finally {
          setTimeout(() => { isRedirectingToLicense = false }, 800)
        }
        return Promise.reject(error)
      }

      if (status === 401 && !originalRequest._retry) {
        const refreshTokenValue = localStorage.getItem('refreshToken')

        if (!refreshTokenValue) {
          handleForceLogout()
          return Promise.reject(error)
        }

        if (isRefreshing) {
          return new Promise((resolve, reject) => {
            pendingRequests.push({ resolve, reject })
          }).then((newToken) => {
            originalRequest.headers.Authorization = `Bearer ${newToken}`
            return request(originalRequest)
          })
        }

        originalRequest._retry = true
        isRefreshing = true

        try {
          // 动态导入避免循环依赖
          const { refreshToken } = await import('@/api/auth')
          const res = await refreshToken(refreshTokenValue)

          if (res.code === 200 || res.code === 0) {
            const { token, refreshToken: newRefreshToken } = res.data

            localStorage.setItem('token', token)
            localStorage.setItem('refreshToken', newRefreshToken)

            try {
              const { useUserStore } = await import('@/stores/user')
              const userStore = useUserStore()
              userStore.setToken(token)
              userStore.setRefreshToken(newRefreshToken)
            } catch (_) {
              // Pinia 可能尚未就绪
            }

            processQueue(null, token)
            originalRequest.headers.Authorization = `Bearer ${token}`
            return request(originalRequest)
          } else {
            processQueue(new Error('Token refresh failed'))
            handleForceLogout()
            return Promise.reject(error)
          }
        } catch (refreshError) {
          processQueue(refreshError)
          handleForceLogout()
          return Promise.reject(refreshError)
        } finally {
          isRefreshing = false
        }
      }

      switch (status) {
        case 403:
          KhMessageFn.error('没有权限访问')
          break
        case 404:
          KhMessageFn.error('请求的资源不存在')
          break
        case 500:
          KhMessageFn.error('服务器内部错误')
          break
        default:
          KhMessageFn.error(data?.message || '请求失败')
      }
    } else {
      KhMessageFn.error('网络连接异常，请检查网络')
    }
    if (originalRequest.showLoading !== false) hideLoading()
    return Promise.reject(error)
  }
)

export default request
