<template>
  <el-alert
    :type="type"
    :title="title"
    :description="description"
    :closable="closable"
    :center="center"
    :show-icon="showIcon"
    :effect="effect"
    v-bind="$attrs"
    class="kh-alert"
    @close="emit('close')"
  />
</template>
<script setup>
defineProps({
  type: { type: String, default: 'info' },
  title: { type: String, default: '' },
  description: { type: String, default: '' },
  closable: { type: Boolean, default: true },
  center: { type: Boolean, default: false },
  showIcon: { type: Boolean, default: false },
  effect: { type: String, default: 'light' },
})
const emit = defineEmits(['close'])
</script>
<style scoped>
.kh-alert {
  border-radius: 8px;
  border: none;
  padding: 12px 16px;
}
.kh-alert :deep(.el-alert__title) {
  font-size: 14px;
  font-weight: 500;
}
.kh-alert :deep(.el-alert__description) {
  font-size: 13px;
  margin-top: 4px;
  line-height: 1.5;
}
.kh-alert :deep(.el-alert__closebtn) {
  font-size: 14px;
}
</style>
