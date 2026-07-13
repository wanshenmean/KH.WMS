<script setup>
// 命令式调用封装
</script>
<script>
import { ElMessageBox } from 'element-plus'

export default {
  name: 'KhMsgBox',
}

const defaultOptions = {
  confirmButtonText: '确定',
  cancelButtonText: '取消',
  closeOnClickModal: false,
}

export const KhMsgBoxFn = {
  alert: (message, title = '提示', options = {}) => {
    return ElMessageBox.alert(message, title, { ...defaultOptions, ...options })
  },
  confirm: (message, title = '确认', options = {}) => {
    return ElMessageBox.confirm(message, title, { ...defaultOptions, type: 'warning', ...options })
  },
  prompt: (message, title = '请输入', options = {}) => {
    return ElMessageBox.prompt(message, title, { ...defaultOptions, ...options })
  },
}
</script>
