<template>
  <el-timeline class="kh-timeline">
    <el-timeline-item
      v-for="(item, index) in items"
      :key="index"
      :timestamp="item.time"
      :type="item.type || 'primary'"
      :hollow="item.hollow || false"
      :icon="item.icon"
      placement="top"
    >
      <div class="kh-timeline__title">{{ item.title }}</div>
      <div v-if="item.content" class="kh-timeline__content">{{ item.content }}</div>
      <slot :name="`item-${index}`" :item="item" :index="index" />
    </el-timeline-item>
  </el-timeline>
</template>
<script setup>
defineProps({
  items: { type: Array, default: () => [] },
})
</script>
<style scoped>
.kh-timeline :deep(.el-timeline-item__timestamp) {
  font-size: 12px;
  color: #86909c;
}
.kh-timeline :deep(.el-timeline-item__node) {
  width: 12px;
  height: 12px;
  left: -1px;
}
.kh-timeline :deep(.el-timeline-item__tail) {
  left: 4px;
}
.kh-timeline__title {
  font-size: 14px;
  font-weight: 500;
  color: #1d2129;
  margin-bottom: 4px;
}
.kh-timeline__content {
  font-size: 13px;
  color: #4e5969;
  line-height: 1.6;
}
</style>
