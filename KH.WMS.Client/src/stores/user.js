import { defineStore } from 'pinia'
import { login as loginApi, logout as logoutApi } from '@/api/auth'
import { getUserInfo as getUserInfoApi } from '@/api/user'

export const useUserStore = defineStore('user', {
  state: () => ({
    token: localStorage.getItem('token') || '',
    refreshToken: localStorage.getItem('refreshToken') || '',
    userInfo: JSON.parse(localStorage.getItem('userInfo') || '{}'),
  }),

  getters: {
    isLoggedIn: (state) => !!state.token,
    username: (state) => state.userInfo.username || '',
    userDisplayName: (state) => state.userInfo.name || state.userInfo.username || '用户',
  },

  actions: {
    /** 登录 */
    async login(credentials) {
      const res = await loginApi(credentials)
      if (res.code === 200 || res.code === 0) {
        this.setToken(res.data.token)
        this.setRefreshToken(res.data.refreshToken)
        // await this.fetchUserInfo()
      }
      return res
    },

    /** 获取当前用户信息 */
    async fetchUserInfo() {
      const res = await getUserInfoApi()
      if (res.code === 200 || res.code === 0) {
        this.setUserInfo(res.data)
      }
      return res
    },

    /** 登出 */
    async logout() {
      try {
        await logoutApi()
      } catch (_) {
        // 登出 API 失败也要清除本地状态
      } finally {
        this.clearAuth()
      }
    },

    setToken(token) {
      this.token = token
      localStorage.setItem('token', token)
    },

    setRefreshToken(refreshToken) {
      this.refreshToken = refreshToken
      localStorage.setItem('refreshToken', refreshToken)
    },

    setUserInfo(userInfo) {
      this.userInfo = userInfo
      localStorage.setItem('userInfo', JSON.stringify(userInfo))
    },

    clearAuth() {
      this.token = ''
      this.refreshToken = ''
      this.userInfo = {}
      localStorage.removeItem('token')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('userInfo')
    },
  },
})
