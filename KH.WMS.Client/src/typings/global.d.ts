import { KhMessageFn } from './components/KhMessage/index.vue'
import { KhMsgBoxFn } from './components/KhMsgBox/index.vue'
import { KhNotifyFn } from './components/KhNotify/index.vue'

declare module 'vue' {
  interface ComponentCustomProperties {
    $khMessage: typeof KhMessageFn
    $khMsgBox: typeof KhMsgBoxFn
    $khNotify: typeof KhNotifyFn
  }
}

export {}
