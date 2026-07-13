/**
 * WebSocket 客户端工具类
 * 支持自动重连（指数退避）、心跳保活、token 认证
 */
export class WebSocketClient {
  constructor(options = {}) {
    this.url = options.url
    this.name = options.name || 'default'
    this.heartbeatInterval = options.heartbeatInterval || 30000
    this.reconnectBaseDelay = options.reconnectBaseDelay || 1000
    this.reconnectMaxDelay = options.reconnectMaxDelay || 30000
    this.maxReconnectAttempts = options.maxReconnectAttempts || Infinity

    this.onMessage = options.onMessage || (() => {})
    this.onOpen = options.onOpen || (() => {})
    this.onClose = options.onClose || (() => {})
    this.onError = options.onError || (() => {})

    this.ws = null
    this.heartbeatTimer = null
    this.reconnectTimer = null
    this.reconnectAttempts = 0
    this._isManualClose = false
  }

  get isConnected() {
    return this.ws && this.ws.readyState === WebSocket.OPEN
  }

  get state() {
    if (!this.ws) return 'DISCONNECTED'
    const states = ['CONNECTING', 'OPEN', 'CLOSING', 'CLOSED']
    return states[this.ws.readyState]
  }

  /** 建立连接，token 通过 query 参数传递 */
  connect(token = '') {
    if (this.isConnected) return

    this._isManualClose = false
    const url = token ? `${this.url}?token=${encodeURIComponent(token)}` : this.url

    try {
      this.ws = new WebSocket(url)
    } catch (err) {
      console.error(`[WS:${this.name}] 创建连接失败:`, err)
      this._scheduleReconnect()
      return
    }

    this.ws.onopen = (event) => {
      console.log(`[WS:${this.name}] 已连接`)
      this.reconnectAttempts = 0
      this._startHeartbeat()
      this.onOpen(event)
    }

    this.ws.onmessage = (event) => {
      if (event.data === 'pong' || event.data === '"pong"') {
        return
      }
      try {
        const data = JSON.parse(event.data)
        this.onMessage(data)
      } catch (_) {
        this.onMessage(event.data)
      }
    }

    this.ws.onclose = (event) => {
      console.log(`[WS:${this.name}] 已关闭 (code: ${event.code})`)
      this._stopHeartbeat()
      this.onClose(event)
      if (!this._isManualClose) {
        this._scheduleReconnect()
      }
    }

    this.ws.onerror = (event) => {
      console.error(`[WS:${this.name}] 错误:`, event)
      this.onError(event)
    }
  }

  /** 发送消息，自动 JSON 序列化 */
  send(data) {
    if (!this.isConnected) {
      console.warn(`[WS:${this.name}] 未连接，无法发送`)
      return false
    }
    const payload = typeof data === 'string' ? data : JSON.stringify(data)
    this.ws.send(payload)
    return true
  }

  /** 主动关闭连接，不触发自动重连 */
  close() {
    this._isManualClose = true
    this._stopHeartbeat()
    this._stopReconnect()
    if (this.ws) {
      this.ws.close(1000, 'Manual close')
      this.ws = null
    }
  }

  // --- 内部方法 ---

  _startHeartbeat() {
    this._stopHeartbeat()
    this.heartbeatTimer = setInterval(() => {
      if (this.isConnected) {
        this.send('ping')
      }
    }, this.heartbeatInterval)
  }

  _stopHeartbeat() {
    if (this.heartbeatTimer) {
      clearInterval(this.heartbeatTimer)
      this.heartbeatTimer = null
    }
  }

  _scheduleReconnect() {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.error(`[WS:${this.name}] 已达最大重连次数`)
      return
    }

    const delay = Math.min(
      this.reconnectBaseDelay * Math.pow(2, this.reconnectAttempts),
      this.reconnectMaxDelay
    )
    this.reconnectAttempts++

    console.log(`[WS:${this.name}] ${delay}ms 后重连 (第 ${this.reconnectAttempts} 次)`)
    this.reconnectTimer = setTimeout(() => {
      const token = localStorage.getItem('token') || ''
      this.connect(token)
    }, delay)
  }

  _stopReconnect() {
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer)
      this.reconnectTimer = null
    }
  }
}

export default WebSocketClient
