import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import AutoImport from 'unplugin-auto-import/vite'
import Components from 'unplugin-vue-components/vite'
import { ElementPlusResolver } from 'unplugin-vue-components/resolvers'
import path from 'path'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'

// Element Plus 图标列表（供 AutoImport 批量注入 script 中的 markRaw(Plus) 等）
const iconImports = Object.keys(ElementPlusIconsVue).map(name => [name, name])

export default defineConfig({
  plugins: [
    vue({
      template: {
        compilerOptions: {
          // 将 ElPopper 等虚拟组件标记为自定义元素，避免指令挂载警告
          isCustomElement: (tag) => tag === 'ElPopper' || tag === 'ElPopperContent',
        },
      },
    }),
    AutoImport({
      imports: [
        'vue',
        'vue-router',
        'pinia',
        // Kh 命令式组件全局注入（KhMessageFn / KhMsgBoxFn / KhNotifyFn）
        {
          '@/components/KhMessage/index.vue': [['KhMessageFn', 'KhMessageFn']],
          '@/components/KhMsgBox/index.vue': [['KhMsgBoxFn', 'KhMsgBoxFn']],
          '@/components/KhNotify/index.vue': [['KhNotifyFn', 'KhNotifyFn']],
          // Element Plus 图标全局注入（script 中直接用 markRaw(Plus) 等）
          '@element-plus/icons-vue': iconImports,
        },
      ],
      resolvers: [ElementPlusResolver()],
      dts: false,
    }),
    Components({
      dirs: ['src/components'],
      directoryAsNamespace: true,
      resolvers: [ElementPlusResolver()],
    }),
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, 'src'),
    },
  },
  server: {
    host: '0.0.0.0',
    port: 3000,
    open: true,
    proxy: {
      '/api': {
        target: 'http://localhost:9291',
        changeOrigin: true,
      },
      '/ws': {
        target: 'ws://localhost:9291',
        changeOrigin: true,
        ws: true,
      },
    },
  },
})
