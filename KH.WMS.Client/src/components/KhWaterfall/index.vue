<template>
  <div>
    <div class="kh-waterfall" :style="{ columnCount: columns, columnGap: gap + 'px' }">
      <div v-for="(item, index) in items" :key="index" class="kh-waterfall__item" :style="{ marginBottom: gap + 'px' }">
        <slot :item="item" :index="index" />
      </div>
    </div>
    <div v-if="loading" class="kh-waterfall__loading">
      <el-icon class="is-loading" :size="20">
        <Loading />
      </el-icon>
      <span>加载中...</span>
    </div>
    <div v-if="finished && items.length > 0" class="kh-waterfall__finished">
      没有更多了
    </div>
  </div>
</template>
<script setup>

defineProps({
  items: { type: Array, default: () => [] },
  columns: { type: Number, default: 3 },
  gap: { type: Number, default: 12 },
  loading: { type: Boolean, default: false },
  finished: { type: Boolean, default: false },
})

defineEmits(['load-more'])
</script>
<style scoped>
.kh-waterfall {
  width: 100%;
}

.kh-waterfall__item {
  break-inside: avoid;
}

.kh-waterfall__loading,
.kh-waterfall__finished {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 20px 0;
  font-size: 13px;
  color: #86909c;
}
</style>
