import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import zhCn from 'element-plus/dist/locale/zh-cn.mjs'
import 'element-plus/dist/index.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'

import App from './App.vue'
import router from './router'
import permissionDirective from './directives/permission'

// 命令式函数（通过 this.$khMessage.success() 等调用）
import { KhMessageFn } from './components/KhMessage/index.vue'
import { KhMsgBoxFn } from './components/KhMsgBox/index.vue'
import { KhNotifyFn } from './components/KhNotify/index.vue'

// 全局 API（通过 this.$api.xxx() 或 useApi() 调用）
import * as api from './api'

const app = createApp(App)

// 注册所有 Element Plus 图标
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

// 全局挂载命令式函数
app.config.globalProperties.$khMessage = KhMessageFn
app.config.globalProperties.$khMsgBox = KhMsgBoxFn
app.config.globalProperties.$khNotify = KhNotifyFn

// 全局挂载 API
app.config.globalProperties.$api = api

app.use(createPinia())
app.use(router)
app.use(ElementPlus, { locale: zhCn })

// 注册全局指令
app.directive('permission', permissionDirective)

app.mount('#app')
