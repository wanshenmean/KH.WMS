<template>
  <div ref="triggerRef" class="kh-color-picker">
    <!-- 触发器 -->
    <div
      class="kh-color-picker__input"
      :class="{ 'is-focus': visible, 'is-disabled': disabled }"
      @click.stop="toggle"
    >
      <span v-if="modelValue" class="kh-color-picker__swatch" :style="{ backgroundColor: modelValue }" />
      <span class="kh-color-picker__text" :class="{ 'is-placeholder': !modelValue }">
        {{ modelValue || placeholder || '请选择颜色' }}
      </span>
      <span v-if="modelValue && !disabled" class="kh-color-picker__clear" @click.stop="handleClear">
        <el-icon :size="14"><CircleClose /></el-icon>
      </span>
      <span class="kh-color-picker__arrow">
        <el-icon :size="14"><ArrowDown /></el-icon>
      </span>
    </div>

    <!-- 下拉面板（Teleport 到 body 避免 overflow 裁剪） -->
    <Teleport to="body">
      <Transition name="kh-fade">
        <div
          v-if="visible"
          ref="panelRef"
          class="kh-color-picker__panel"
          :style="panelStyle"
          @mousedown.stop
        >
          <!-- 预设颜色 -->
          <div class="kh-color-picker__section">
            <span class="kh-color-picker__label">预设颜色</span>
            <div class="kh-color-picker__presets">
              <span
                v-for="color in predefine"
                :key="color"
                class="kh-color-picker__preset"
                :class="{ 'is-active': modelValue && modelValue.toLowerCase() === color.toLowerCase() }"
                :style="{ backgroundColor: color }"
                :title="color"
                @click="handleSelect(color)"
              />
            </div>
          </div>
          <!-- 自定义输入 -->
          <div class="kh-color-picker__section">
            <span class="kh-color-picker__label">自定义颜色值</span>
            <div class="kh-color-picker__custom-row">
              <span
                class="kh-color-picker__preview-swatch"
                :style="{ backgroundColor: normalizedInput || undefined }"
              />
              <el-input
                ref="inputRef"
                v-model="inputHex"
                size="small"
                placeholder="#409EFF"
                maxlength="7"
                @keyup.enter="handleInputConfirm"
                @blur="handleInputConfirm"
              />
              <el-button size="small" type="primary" @click="handleInputConfirm">确定</el-button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup>

const props = defineProps({
  modelValue: { type: String, default: '' },
  placeholder: { type: String, default: '' },
  disabled: { type: Boolean, default: false },
  showAlpha: { type: Boolean, default: false },
  predefine: { type: Array, default: () => [
    '#409EFF', '#67C23A', '#E6A23C', '#F56C6C', '#909399',
    '#00bcd4', '#ff6b6b', '#ffd93d', '#6bcb77', '#4d96ff',
    '#9c27b0', '#ff5722', '#795548', '#607d8b',
  ]},
})

const emit = defineEmits(['update:modelValue', 'change'])

const triggerRef = ref(null)
const panelRef = ref(null)
const inputRef = ref(null)
const visible = ref(false)
const inputHex = ref('')

// 校验 hex 格式，自动补 #
const normalizedInput = computed(() => {
  let val = inputHex.value.trim()
  if (!val) return ''
  if (!val.startsWith('#')) val = '#' + val
  return val
})

watch(() => props.modelValue, (val) => {
  inputHex.value = val || ''
}, { immediate: true })

const panelStyle = computed(() => {
  if (!triggerRef.value) return { display: 'none' }
  const rect = triggerRef.value.getBoundingClientRect()
  return {
    position: 'fixed',
    top: `${rect.bottom + 6}px`,
    left: `${rect.left}px`,
    zIndex: 9999,
    minWidth: `${Math.max(rect.width, 280)}px`,
  }
})

function toggle() {
  if (props.disabled) return
  inputHex.value = props.modelValue || ''
  visible.value = !visible.value
  if (visible.value) {
    nextTick(() => inputRef.value?.focus())
  }
}

function close() {
  visible.value = false
}

function handleSelect(color) {
  inputHex.value = color
  emit('update:modelValue', color)
  emit('change', color)
  close()
}

function handleInputConfirm() {
  let val = inputHex.value.trim()
  if (!val) {
    emit('update:modelValue', '')
    emit('change', '')
    close()
    return
  }
  if (!val.startsWith('#')) val = '#' + val
  // 校验 hex 格式
  if (/^#[0-9a-fA-F]{3}$/.test(val) || /^#[0-9a-fA-F]{6}$/.test(val)) {
    inputHex.value = val
    emit('update:modelValue', val)
    emit('change', val)
    close()
  }
}

function handleClear() {
  inputHex.value = ''
  emit('update:modelValue', '')
  emit('change', '')
}

// 点击外部关闭
function handleClickOutside(e) {
  if (!visible.value) return
  const trigger = triggerRef.value
  const panel = panelRef.value
  if (trigger?.contains(e.target) || panel?.contains(e.target)) return
  close()
}

// 滚动时关闭（避免面板位置偏移）
function handleScroll() {
  if (visible.value) close()
}

onMounted(() => {
  document.addEventListener('mousedown', handleClickOutside)
  window.addEventListener('scroll', handleScroll, true)
})

onBeforeUnmount(() => {
  document.removeEventListener('mousedown', handleClickOutside)
  window.removeEventListener('scroll', handleScroll, true)
})
</script>

<style scoped>
.kh-color-picker {
  width: 100%;
}

.kh-color-picker__input {
  display: flex;
  align-items: center;
  height: 32px;
  padding: 0 30px 0 11px;
  border: 1px solid var(--el-border-color);
  border-radius: var(--el-border-radius-base);
  background-color: var(--el-fill-color-blank);
  transition: border-color 0.2s;
  position: relative;
  box-sizing: border-box;
  cursor: pointer;
}

.kh-color-picker__input:hover {
  border-color: var(--el-border-color-hover);
}

.kh-color-picker__input.is-focus {
  border-color: var(--el-color-primary);
}

.kh-color-picker__input.is-disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

.kh-color-picker__swatch {
  display: inline-block;
  width: 16px;
  height: 16px;
  border-radius: 3px;
  border: 1px solid var(--el-border-color-lighter);
  margin-right: 8px;
  flex-shrink: 0;
}

.kh-color-picker__text {
  flex: 1;
  font-size: 14px;
  color: var(--el-text-color-regular);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  line-height: 32px;
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
}

.kh-color-picker__text.is-placeholder {
  color: var(--el-text-color-placeholder);
  font-family: inherit;
}

.kh-color-picker__clear {
  position: absolute;
  right: 25px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--el-text-color-placeholder);
  cursor: pointer;
  display: flex;
  align-items: center;
}

.kh-color-picker__clear:hover {
  color: var(--el-text-color-secondary);
}

.kh-color-picker__arrow {
  position: absolute;
  right: 8px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--el-text-color-placeholder);
  display: flex;
  align-items: center;
}

.kh-color-picker__panel {
  background: var(--el-bg-color-overlay);
  border: 1px solid var(--el-border-color-lighter);
  border-radius: var(--el-border-radius-base);
  box-shadow: var(--el-box-shadow-light);
  padding: 14px;
}

.kh-color-picker__section {
  margin-bottom: 12px;
}

.kh-color-picker__section:last-child {
  margin-bottom: 0;
}

.kh-color-picker__label {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  display: block;
  margin-bottom: 8px;
}

.kh-color-picker__presets {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.kh-color-picker__preset {
  width: 26px;
  height: 26px;
  border-radius: 4px;
  cursor: pointer;
  border: 2px solid transparent;
  transition: all 0.15s;
}

.kh-color-picker__preset:hover {
  transform: scale(1.15);
  border-color: var(--el-border-color);
}

.kh-color-picker__preset.is-active {
  border-color: var(--el-color-primary);
  box-shadow: 0 0 0 2px var(--el-color-primary-light-7);
}

.kh-color-picker__custom-row {
  display: flex;
  align-items: center;
  gap: 8px;
}

.kh-color-picker__preview-swatch {
  display: inline-block;
  width: 32px;
  height: 32px;
  border-radius: 4px;
  border: 1px solid var(--el-border-color-lighter);
  flex-shrink: 0;
  background-color: var(--el-fill-color);
}
</style>

<style>
/* 全局过渡动画（Teleport 到 body，scoped 不生效） */
.kh-fade-enter-active {
  transition: opacity 0.2s ease;
}

.kh-fade-leave-active {
  transition: opacity 0.15s ease;
}

.kh-fade-enter-from,
.kh-fade-leave-to {
  opacity: 0;
}
</style>
