<template>
  <el-drawer
    :model-value="modelValue"
    :title="title"
    :size="width"
    :direction="placementMap[placement]"
    :show-close="showClose"
    :with-header="!!title"
    v-bind="$attrs"
    @open="emit('open')"
    @close="handleClose"
  >
    <template v-if="$slots.header" #header>
      <slot name="header" />
    </template>
    <slot />
    <template v-if="$slots.footer" #footer>
      <slot name="footer" />
    </template>
  </el-drawer>
</template>

<script setup>
const props = defineProps({
  /** 是否显示 */
  modelValue: { type: Boolean, default: false },
  /** 标题 */
  title: { type: String, default: '' },
  /** 宽度 */
  width: { type: String, default: '380px' },
  /** 方向: left / right / top / bottom */
  placement: { type: String, default: 'right' },
  /** 是否显示关闭按钮 */
  showClose: { type: Boolean, default: true },
})

const emit = defineEmits(['update:modelValue', 'open', 'close'])

/** placement → el-drawer direction 映射 */
const placementMap = { left: 'ltr', right: 'rtl', top: 'ttb', bottom: 'btt' }

const handleClose = () => {
  emit('update:modelValue', false)
  emit('close')
}
</script>
