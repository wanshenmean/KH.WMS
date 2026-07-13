<script setup>
// 命令式调用封装
</script>
<script>
import { ElNotification } from 'element-plus'

export default {
  name: 'KhNotify',
}

const defaultOptions = {
  duration: 4500,
  position: 'top-right',
}

export const KhNotifyFn = {
  success: (options = {}) => {
    const { title, message, ...rest } = typeof options === 'string' ? { message: options } : options
    return ElNotification({ title: title || '成功', message, type: 'success', ...defaultOptions, ...rest })
  },
  warning: (options = {}) => {
    const { title, message, ...rest } = typeof options === 'string' ? { message: options } : options
    return ElNotification({ title: title || '警告', message, type: 'warning', ...defaultOptions, ...rest })
  },
  info: (options = {}) => {
    const { title, message, ...rest } = typeof options === 'string' ? { message: options } : options
    return ElNotification({ title: title || '通知', message, type: 'info', ...defaultOptions, ...rest })
  },
  error: (options = {}) => {
    const { title, message, ...rest } = typeof options === 'string' ? { message: options } : options
    return ElNotification({ title: title || '错误', message, type: 'error', ...defaultOptions, ...rest })
  },
  closeAll: () => ElNotification.closeAll(),
}
</script>
