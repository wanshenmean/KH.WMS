import { defineStore } from 'pinia'
import { WebSocketClient } from '@/utils/websocket'

/** WebSocket 事件总线 */
class WsEventBus {
  constructor() {
    this._listeners = new Map()
  }

  on(event, callback) {
    if (!this._listeners.has(event)) {
      this._listeners.set(event, [])
    }
    this._listeners.get(event).push(callback)
  }

  off(event, callback) {
    const listeners = this._listeners.get(event)
    if (listeners) {
      const index = listeners.indexOf(callback)
      if (index > -1) listeners.splice(index, 1)
    }
  }

  emit(event, data) {
    const listeners = this._listeners.get(event)
    if (listeners) {
      listeners.forEach(cb => {
        try { cb(data) } catch (err) {
          console.error(`[WsEventBus] "${event}" 事件处理错误:`, err)
        }
      })
    }
  }

  clear() {
    this._listeners.clear()
  }
}

function getWsBaseUrl() {
  const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:'
  return `${protocol}//${window.location.host}`
}

export const useWebSocketStore = defineStore('websocket', {
  state: () => ({
    /** 是否启用 WebSocket（默认关闭，后端就绪后设为 true） */
    enabled: false,
    notificationWs: null,
    deviceWs: null,
    eventBus: new WsEventBus(),
    notificationConnected: false,
    deviceConnected: false,
    notifications: [],
    deviceData: {
      crane: [],
      conveyor: [],
    },
  }),

  getters: {
    unreadNotificationCount: (state) => {
      return state.notifications.filter(n => !n.read).length
    },
  },

  actions: {
    /** 初始化 WebSocket 连接（登录后调用） */
    initConnections() {
      if (!this.enabled) return
      this._initNotificationWs()
      this._initDeviceWs()
    },

    /** 启用 WebSocket 并建立连接 */
    enable() {
      this.enabled = true
      this.initConnections()
    },

    /** 禁用 WebSocket 并关闭所有连接 */
    disable() {
      this.enabled = false
      this._closeConnections()
    },

    /** 关闭所有连接（登出时调用） */
    closeAll() {
      this._closeConnections()
      this.notifications = []
      this.deviceData = { crane: [], conveyor: [] }
      this.eventBus.clear()
    },

    /** 订阅事件，返回取消订阅函数 */
    subscribe(event, callback) {
      this.eventBus.on(event, callback)
      return () => this.eventBus.off(event, callback)
    },

    // --- 内部方法 ---

    _closeConnections() {
      if (this.notificationWs) {
        this.notificationWs.close()
        this.notificationWs = null
      }
      if (this.deviceWs) {
        this.deviceWs.close()
        this.deviceWs = null
      }
      this.notificationConnected = false
      this.deviceConnected = false
    },

    _initNotificationWs() {
      const wsUrl = `${getWsBaseUrl()}/ws/notification`

      this.notificationWs = new WebSocketClient({
        url: wsUrl,
        name: 'notification',
        heartbeatInterval: 30000,
        onOpen: () => { this.notificationConnected = true },
        onClose: () => { this.notificationConnected = false },
        onMessage: (data) => { this._handleNotificationMessage(data) },
      })

      const token = localStorage.getItem('token') || ''
      this.notificationWs.connect(token)
    },

    _initDeviceWs() {
      const wsUrl = `${getWsBaseUrl()}/ws/device`

      this.deviceWs = new WebSocketClient({
        url: wsUrl,
        name: 'device',
        heartbeatInterval: 15000,
        reconnectBaseDelay: 500,
        onOpen: () => { this.deviceConnected = true },
        onClose: () => { this.deviceConnected = false },
        onMessage: (data) => { this._handleDeviceMessage(data) },
      })

      const token = localStorage.getItem('token') || ''
      this.deviceWs.connect(token)
    },

    _handleNotificationMessage(data) {
      // 预期格式: { type: 'notification', data: { id, title, content, type, time } }
      if (data.type === 'notification') {
        this.notifications.unshift({ ...data.data, read: false })
        if (this.notifications.length > 50) {
          this.notifications = this.notifications.slice(0, 50)
        }
      }
      this.eventBus.emit('notification', data.data)
      if (data.type) this.eventBus.emit(data.type, data.data)
    },

    _handleDeviceMessage(data) {
      // 预期格式: { type: 'crane'|'conveyor', data: {...} }
      if (data.type === 'crane') {
        this.deviceData.crane = data.data
      } else if (data.type === 'conveyor') {
        this.deviceData.conveyor = data.data
      }
      if (data.type) this.eventBus.emit(data.type, data.data)
      this.eventBus.emit('device', data)
    },

    /** 标记通知已读 */
    markNotificationRead(notificationId) {
      const notification = this.notifications.find(n => n.id === notificationId)
      if (notification) notification.read = true
    },

    /** 全部标记已读 */
    markAllNotificationsRead() {
      this.notifications.forEach(n => { n.read = true })
    },
  },
})
