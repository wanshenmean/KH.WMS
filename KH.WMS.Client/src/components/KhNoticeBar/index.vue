<template>
  <div v-if="visible" class="kh-notice-bar" :class="[`is-${type}`, { 'is-scrollable': scrollable }]">
    <div class="kh-notice-bar__icon">
      <el-icon v-if="type === 'info'"><InfoFilled /></el-icon>
      <el-icon v-else-if="type === 'success'"><SuccessFilled /></el-icon>
      <el-icon v-else-if="type === 'warning'"><WarningFilled /></el-icon>
      <el-icon v-else-if="type === 'error'"><CircleCloseFilled /></el-icon>
    </div>
    <div class="kh-notice-bar__content" ref="contentRef">
      <div v-if="scrollable" class="kh-notice-bar__scroll-wrap">
        <div class="kh-notice-bar__scroll-text" :class="{ 'is-animate': scrollStarted }">
          <slot>{{ text }}</slot>
        </div>
      </div>
      <slot v-else>{{ text }}</slot>
    </div>
    <div v-if="closable" class="kh-notice-bar__close" @click="handleClose">
      <el-icon><Close /></el-icon>
    </div>
  </div>
</template>

<script setup>

const props = defineProps({
  /** 提醒文字 */
  text: { type: String, default: '' },
  /** 类型 */
  type: { type: String, default: 'info', validator: (v) => ['info', 'success', 'warning', 'error'].includes(v) },
  /** 是否可关闭 */
  closable: { type: Boolean, default: true },
  /** 是否滚动 */
  scrollable: { type: Boolean, default: false },
  /** 滚动速度（px/s） */
  speed: { type: Number, default: 50 },
})

const emit = defineEmits(['close'])

const visible = ref(true)
const contentRef = ref(null)
const scrollStarted = ref(false)
let animationTimer = null

const handleClose = () => {
  visible.value = false
  emit('close')
}

const startScroll = () => {
  if (!props.scrollable || !contentRef.value) return
  const wrap = contentRef.value.querySelector('.kh-notice-bar__scroll-wrap')
  const text = contentRef.value.querySelector('.kh-notice-bar__scroll-text')
  if (!wrap || !text) return

  const textWidth = text.scrollWidth
  const wrapWidth = wrap.offsetWidth

  if (textWidth <= wrapWidth) return

  // 计算动画时长
  const duration = textWidth / props.speed

  text.style.animationDuration = `${duration}s`
  nextTick(() => {
    scrollStarted.value = true
  })
}

watch(() => props.text, () => {
  if (props.scrollable) {
    scrollStarted.value = false
    nextTick(() => startScroll())
  }
})

onMounted(() => {
  if (props.scrollable) {
    nextTick(() => startScroll())
  }
})

onBeforeUnmount(() => {
  if (animationTimer) clearTimeout(animationTimer)
})
</script>

<style scoped>
.kh-notice-bar {
  display: flex;
  align-items: center;
  padding: 10px 16px;
  border-radius: 6px;
  font-size: 13px;
  line-height: 1.5;
  gap: 8px;
  transition: all 0.3s;
}

.kh-notice-bar.is-info {
  background-color: #e8f3ff;
  color: #337ecc;
  border: 1px solid #b3d8ff;
}

.kh-notice-bar.is-success {
  background-color: #e8f9ef;
  color: #2d9a56;
  border: 1px solid #a4e0b5;
}

.kh-notice-bar.is-warning {
  background-color: #fdf6ec;
  color: #b88230;
  border: 1px solid #f3d19e;
}

.kh-notice-bar.is-error {
  background-color: #fef0f0;
  color: #c45656;
  border: 1px solid #fab6b6;
}

.kh-notice-bar__icon {
  flex-shrink: 0;
  display: flex;
  align-items: center;
  font-size: 16px;
}

.kh-notice-bar__content {
  flex: 1;
  min-width: 0;
  overflow: hidden;
}

.kh-notice-bar__scroll-wrap {
  overflow: hidden;
  white-space: nowrap;
}

.kh-notice-bar__scroll-text {
  display: inline-block;
  white-space: nowrap;
}

.kh-notice-bar__scroll-text.is-animate {
  animation: kh-notice-scroll linear infinite;
}

@keyframes kh-notice-scroll {
  0% { transform: translateX(100%); }
  100% { transform: translateX(-100%); }
}

.kh-notice-bar__close {
  flex-shrink: 0;
  display: flex;
  align-items: center;
  cursor: pointer;
  opacity: 0.6;
  transition: opacity 0.2s;
  border-radius: 50%;
  padding: 2px;
}

.kh-notice-bar__close:hover {
  opacity: 1;
}
</style>
