<template>
  <div class="kh-drag-list">
    <div v-if="innerList.length > 0" class="kh-drag-list__list">
      <VueDraggable
        v-model="innerList"
        :item-key="rowKey"
        handle=".kh-drag-list__handle"
        :animation="animation"
        :ghost-class="ghostClass"
        :chosen-class="chosenClass"
        :disabled="disabled"
        @start="handleDragStart"
        @end="handleDragEnd"
      >
        <template #item="{ element, index }">
          <div class="kh-drag-list__item" :class="{ 'is-disabled': disabled }">
            <el-icon class="kh-drag-list__handle"><Rank /></el-icon>
            <div class="kh-drag-list__body">
              <slot name="item" :element="element" :index="index">
                <!-- 默认内容：序号 + label -->
                <div class="kh-drag-list__default-item">
                  <el-tag size="small" type="primary">{{ index + 1 }}</el-tag>
                  <span class="kh-drag-list__text">{{ getLabel(element) }}</span>
                </div>
              </slot>
            </div>
            <div class="kh-drag-list__actions">
              <slot name="actions" :element="element" :index="index" :remove="() => handleRemove(index)" :list="innerList">
                <el-button
                  v-if="showRemove"
                  type="danger"
                  link
                  size="small"
                  :icon="DeleteIcon"
                  @click="handleRemove(index)"
                />
              </slot>
            </div>
          </div>
        </template>
      </VueDraggable>
    </div>
    <el-empty v-else :description="emptyText" :image-size="emptyImageSize" />
  </div>
</template>

<script setup>
import { VueDraggable } from 'vue-draggable-plus'

// 图标引用（供模板 :icon 绑定使用）
const DeleteIcon = Delete

const props = defineProps({
  /** 列表数据（v-model 双向绑定） */
  modelValue: { type: Array, required: true, default: () => [] },
  /** 行数据的唯一标识字段名，默认 'id'。如果数据没有唯一字段，设为 'index' 使用数组索引 */
  rowKey: { type: String, default: 'id' },
  /** 拖拽动画时长（ms），默认 200 */
  animation: { type: Number, default: 200 },
  /** 拖拽时的占位样式类名 */
  ghostClass: { type: String, default: 'kh-drag-list__ghost' },
  /** 选中时的样式类名 */
  chosenClass: { type: String, default: 'kh-drag-list__chosen' },
  /** 是否禁用拖拽 */
  disabled: { type: Boolean, default: false },
  /** 是否显示默认删除按钮 */
  showRemove: { type: Boolean, default: true },
  /** 空数据时的提示文字 */
  emptyText: { type: String, default: '暂无数据' },
  /** 空数据图标大小 */
  emptyImageSize: { type: Number, default: 80 },
  /** 元素的标签字段名，用于默认插槽显示文本。默认依次尝试 label、name、title 字段 */
  labelKey: { type: String, default: '' },
})

const emit = defineEmits([
  'update:modelValue',
  'change',
  'remove',
  'drag-start',
  'drag-end',
])

const innerList = ref([...props.modelValue])
const isInternalChange = ref(false)

watch(() => props.modelValue, (val) => {
  if (isInternalChange.value) return
  innerList.value = [...val]
}, { deep: true })

watch(innerList, (val) => {
  isInternalChange.value = true
  emit('update:modelValue', val)
  emit('change', val)
  nextTick(() => { isInternalChange.value = false })
}, { deep: true })

const getLabel = (element) => {
  if (props.labelKey) return element[props.labelKey]
  return element.label || element.name || element.title || element.filterName || JSON.stringify(element)
}

const handleRemove = (index) => {
  const removed = innerList.value.splice(index, 1)
  emit('remove', removed[0], index, innerList.value)
}

const handleDragStart = (e) => {
  emit('drag-start', e)
}

const handleDragEnd = (e) => {
  emit('drag-end', e, innerList.value)
}

defineExpose({
  /** 内部列表数据 */
  innerList,
})
</script>

<style scoped>
.kh-drag-list__list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.kh-drag-list__item {
  display: flex;
  align-items: center;
  padding: 12px 14px;
  background: #fafbfc;
  border-radius: 8px;
  border: 1px solid #e5e6eb;
  transition: all 0.25s ease;
}

.kh-drag-list__item:hover {
  background: #f2f3f5;
  border-color: #c9cdd4;
}

.kh-drag-list__item.is-disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.kh-drag-list__handle {
  margin-right: 14px;
  cursor: move;
  font-size: 16px;
  color: #c9cdd4;
  flex-shrink: 0;
  transition: color 0.2s;
}

.kh-drag-list__handle:hover {
  color: #409eff;
}

.kh-drag-list__body {
  flex: 1;
  min-width: 0;
}

.kh-drag-list__default-item {
  display: flex;
  align-items: center;
  gap: 10px;
}

.kh-drag-list__text {
  font-size: 14px;
  color: #1d2129;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.kh-drag-list__actions {
  flex-shrink: 0;
  margin-left: 12px;
  display: flex;
  align-items: center;
  gap: 4px;
}
</style>

<style>
/* 拖拽占位样式（不能 scoped，因为 draggable 渲染在 body） */
.kh-drag-list__ghost {
  opacity: 0.5;
  background: #e8f3ff !important;
  border: 2px dashed #409eff !important;
  border-radius: 8px;
}

.kh-drag-list__chosen {
  box-shadow: 0 4px 16px rgba(64, 158, 255, 0.2);
  border-radius: 8px;
}
</style>
