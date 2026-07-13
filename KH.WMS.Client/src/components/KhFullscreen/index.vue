<template>
  <el-tooltip :content="isFullscreen ? '退出全屏' : '全屏'" placement="bottom">
    <div class="kh-fullscreen" @click="toggle">
      <el-icon :size="18">
        <component :is="isFullscreen ? AimIcon : FullScreenIcon" />
      </el-icon>
    </div>
  </el-tooltip>
</template>

<script setup>
// 图标引用（供模板 :is 绑定使用，需在 script 中声明才能暴露到 $setup）
const AimIcon = Aim
const FullScreenIcon = FullScreen

const isFullscreen = ref(false)

const toggle = () => {
  if (!document.fullscreenElement) {
    document.documentElement.requestFullscreen().catch(() => {})
  } else {
    document.exitFullscreen().catch(() => {})
  }
}

const handleFullscreenChange = () => {
  isFullscreen.value = !!document.fullscreenElement
}

onMounted(() => {
  document.addEventListener('fullscreenchange', handleFullscreenChange)
})

onBeforeUnmount(() => {
  document.removeEventListener('fullscreenchange', handleFullscreenChange)
})

defineExpose({ isFullscreen })
</script>

<style scoped>
.kh-fullscreen {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 6px;
  cursor: pointer;
  color: #606266;
  transition: all 0.2s;
}

.kh-fullscreen:hover {
  background-color: #f0f2f5;
  color: #409eff;
}
</style>
