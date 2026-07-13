// 全局 API composable
// 配合 main.js 中 app.config.globalProperties.$api = api 使用
// 在 <script setup> 中：const { getMenuTree, createInboundOrder } = useApi()
import { getCurrentInstance } from 'vue'

export function useApi() {
  const { proxy } = getCurrentInstance()
  return proxy.$api
}
