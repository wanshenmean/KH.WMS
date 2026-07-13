<template>
  <el-transfer
    v-model="innerValue"
    :data="data"
    v-bind="$attrs"
    class="kh-transfer"
    filterable
    :filter-placeholder="filterPlaceholder"
  />
</template>
<script setup>
const props = defineProps({
  modelValue: { type: Array, default: () => [] },
  data: { type: Array, default: () => [] },
  filterPlaceholder: { type: String, default: '搜索' },
})
const emit = defineEmits(['update:modelValue', 'change'])
const innerValue = computed({
  get: () => props.modelValue,
  set: (val) => { emit('update:modelValue', val); emit('change', val) },
})
defineExpose({ innerValue })
</script>
<style scoped>
.kh-transfer :deep(.el-transfer-panel) {
  border-radius: 8px;
  border-color: #e5e6eb;
  width: 280px;
}
.kh-transfer :deep(.el-transfer-panel__header) {
  background: #fafbfc;
  border-radius: 8px 8px 0 0;
  border-bottom: 1px solid #e5e6eb;
  padding: 12px 16px;
}
.kh-transfer :deep(.el-transfer-panel__body) {
  min-height: 260px;
}
.kh-transfer :deep(.el-transfer-panel__item) {
  padding: 8px 16px;
  font-size: 13px;
}
.kh-transfer :deep(.el-transfer-panel__item:hover) {
  background: #e8f3ff;
}
.kh-transfer :deep(.el-transfer__button) {
  border-radius: 6px;
  padding: 8px 12px;
}
</style>
