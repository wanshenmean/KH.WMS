<template>
  <el-steps
    :active="active"
    :direction="direction"
    :process-status="processStatus"
    :finish-status="finishStatus"
    :simple="simple"
    v-bind="$attrs"
    class="kh-steps"
  >
    <el-step
      v-for="(step, index) in steps"
      :key="index"
      :title="step.title"
      :description="step.description"
      :icon="step.icon"
      :status="step.status"
    >
      <template v-if="$slots[`icon-${index}`]" #icon>
        <slot :name="`icon-${index}`" :step="step" :index="index" />
      </template>
    </el-step>
  </el-steps>
</template>
<script setup>
defineProps({
  active: { type: Number, default: 0 },
  steps: { type: Array, default: () => [] },
  direction: { type: String, default: 'horizontal' },
  processStatus: { type: String, default: 'process' },
  finishStatus: { type: String, default: 'finish' },
  simple: { type: Boolean, default: false },
})
</script>
<style scoped>
.kh-steps :deep(.el-step__title) {
  font-size: 14px;
  font-weight: 500;
}
.kh-steps :deep(.el-step__description) {
  font-size: 12px;
  color: #86909c;
}
.kh-steps :deep(.el-step__head.is-finish) {
  color: #67c23a;
  border-color: #67c23a;
}
</style>
