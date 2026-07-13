<template>
  <el-collapse v-model="activeNames" v-bind="$attrs" class="kh-collapse">
    <slot />
  </el-collapse>
</template>
<script setup>
const props = defineProps({
  modelValue: { type: [Array, String], default: () => [] },
  accordion: { type: Boolean, default: false },
})
const emit = defineEmits(['update:modelValue', 'change'])
const activeNames = computed({
  get: () => props.modelValue,
  set: (val) => { emit('update:modelValue', val); emit('change', val) },
})
defineExpose({ activeNames })
</script>
<style scoped>
.kh-collapse :deep(.el-collapse-item__header) {
  font-size: 14px;
  font-weight: 500;
  color: #1d2129;
  height: 44px;
  line-height: 44px;
  background: #fafbfc;
  border-radius: 6px;
  padding: 0 16px;
  border: 1px solid #e5e6eb;
  transition: all 0.2s;
}
.kh-collapse :deep(.el-collapse-item__header:hover) {
  background: #f2f3f5;
  border-color: #c9cdd4;
}
.kh-collapse :deep(.el-collapse-item__arrow) {
  font-size: 14px;
  color: #86909c;
}
.kh-collapse :deep(.el-collapse-item__wrap) {
  border: none;
  background: transparent;
}
.kh-collapse :deep(.el-collapse-item__content) {
  padding: 12px 16px;
  color: #4e5969;
  font-size: 13px;
  line-height: 1.6;
}
.kh-collapse :deep(.el-collapse-item) {
  margin-bottom: 8px;
  border: none;
  border-radius: 6px;
  overflow: hidden;
}
</style>
