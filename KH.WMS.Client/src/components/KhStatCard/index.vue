<template>
  <el-card class="kh-stat-card" :class="{ 'is-clickable': clickable }" :shadow="shadow" @click="handleClick">
    <div class="kh-stat-card__content">
      <div class="kh-stat-card__icon" :class="[`is-${theme}`]">
        <el-icon :size="iconSize">
          <component :is="rawIcon" />
        </el-icon>
      </div>
      <div class="kh-stat-card__info">
        <div class="kh-stat-card__value">{{ displayValue }}</div>
        <div class="kh-stat-card__label">{{ label }}</div>
      </div>
    </div>
    <div v-if="$slots.extra" class="kh-stat-card__extra">
      <slot name="extra" />
    </div>
  </el-card>
</template>

<script setup>

const props = defineProps({
  /** 统计数值 */
  value: { type: [Number, String], default: 0 },
  /** 标签文字 */
  label: { type: String, default: '' },
  /** 图标组件（Element Plus 图标组件引用） */
  icon: { type: [Object, String], default: undefined },
  /** 图标大小（px），默认 28 */
  iconSize: { type: Number, default: 28 },
  /** 主题色，控制图标渐变背景色。可选: primary | success | warning | danger | info */
  theme: { type: String, default: 'primary', validator: (v) => ['primary', 'success', 'warning', 'danger', 'info'].includes(v) },
  /** 卡片阴影，默认 hover */
  shadow: { type: String, default: 'hover' },
  /** 是否可点击 */
  clickable: { type: Boolean, default: false },
  /** 数值格式化函数，接收原始 value，返回显示字符串 */
  formatter: { type: Function, default: null },
})

const emit = defineEmits(['click'])

/** 使用 toRaw 解包 icon，避免组件对象被响应式代理导致性能警告 */
const rawIcon = computed(() => {
  if (!props.icon) return props.icon
  return typeof props.icon === 'string' ? props.icon : toRaw(props.icon)
})

const displayValue = computed(() => {
  if (props.formatter) return props.formatter(props.value)
  if (typeof props.value === 'number') {
    return props.value.toLocaleString()
  }
  return props.value
})

const handleClick = (e) => {
  if (props.clickable) emit('click', props.value, e)
}
</script>

<style scoped>
.kh-stat-card {
  transition: all 0.3s ease;
  cursor: default;
  border: none;
  border-radius: 8px;
  overflow: hidden;
  position: relative;
}

.kh-stat-card :deep(.el-card__body) {
  padding: 20px;
}

.kh-stat-card.is-clickable {
  cursor: pointer;
}

.kh-stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.1);
}

.kh-stat-card__content {
  display: flex;
  align-items: center;
  gap: 16px;
}

.kh-stat-card__icon {
  width: 48px;
  height: 48px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  flex-shrink: 0;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.kh-stat-card__icon.is-primary {
  background: linear-gradient(135deg, #5b8ff9 0%, #3d76e0 100%);
}

.kh-stat-card__icon.is-success {
  background: linear-gradient(135deg, #5ad8a6 0%, #36b37e 100%);
}

.kh-stat-card__icon.is-warning {
  background: linear-gradient(135deg, #f7ba1e 0%, #f09000 100%);
}

.kh-stat-card__icon.is-danger {
  background: linear-gradient(135deg, #f4664a 0%, #d9363e 100%);
}

.kh-stat-card__icon.is-info {
  background: linear-gradient(135deg, #6dc8ec 0%, #3ba1d1 100%);
}

.kh-stat-card__info {
  flex: 1;
  min-width: 0;
}

.kh-stat-card__value {
  font-size: 26px;
  font-weight: 600;
  color: #1d2129;
  line-height: 1.3;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  letter-spacing: -0.5px;
}

.kh-stat-card__label {
  font-size: 13px;
  color: #86909c;
  margin-top: 2px;
}

.kh-stat-card__extra {
  margin-top: 8px;
  padding-top: 8px;
  border-top: 1px solid #f2f3f5;
}
</style>
