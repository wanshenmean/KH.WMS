<script setup>
// 命令式调用封装
</script>
<script>
import { ElMessage } from 'element-plus'

const createMethod = (type) => (message, options = {}) => {
  return ElMessage({ type, message, grouping: true, offset: 60, duration: 3000, ...options })
}

export default {
  name: 'KhMessage',
}

export const KhMessageFn = {
  success: createMethod('success'),
  warning: createMethod('warning'),
  info: createMethod('info'),
  error: createMethod('error'),
  closeAll: () => ElMessage.closeAll(),
}
</script>
