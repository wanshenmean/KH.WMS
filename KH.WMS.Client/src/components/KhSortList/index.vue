<template>
  <div class="kh-sort-list">
    <div v-if="list.length > 0" class="kh-sort-list__container">
      <div v-if="showHint" class="kh-sort-list__hint">{{ hint }}</div>
      <div class="kh-sort-list__list">
        <div v-for="(item, index) in list" :key="rowKey === 'index' ? index : item[rowKey]" class="kh-sort-list__item">
          <div class="kh-sort-list__body">
            <slot name="item" :element="item" :index="index">
              <!-- 默认内容：序号 + 标签 -->
              <el-tag type="primary" class="kh-sort-list__tag">
                {{ index + 1 }}. {{ getLabel(item) }}
              </el-tag>
            </slot>
          </div>
          <div class="kh-sort-list__actions">
            <slot name="actions" :element="item" :index="index" :move-up="() => handleMove(index, -1)" :move-down="() => handleMove(index, 1)" :remove="() => handleRemove(index)">
              <el-button
                size="small"
                :icon="ArrowUpIcon"
                circle
                :disabled="index === 0 || disabled"
                @click="handleMove(index, -1)"
              />
              <el-button
                size="small"
                :icon="ArrowDownIcon"
                circle
                :disabled="index === list.length - 1 || disabled"
                @click="handleMove(index, 1)"
              />
              <el-button
                v-if="showRemove"
                size="small"
                :icon="CloseIcon"
                circle
                type="danger"
                :disabled="disabled"
                @click="handleRemove(index)"
              />
            </slot>
          </div>
        </div>
      </div>
    </div>
    <el-empty v-else :description="emptyText" :image-size="emptyImageSize" />
  </div>
</template>

<script setup>
// 图标引用（供模板 :icon 绑定使用）
const ArrowUpIcon = ArrowUp
const ArrowDownIcon = ArrowDown
const CloseIcon = Close

const props = defineProps({
  /** 列表数据（v-model 双向绑定） */
  modelValue: { type: Array, required: true, default: () => [] },
  /** 行数据的唯一标识字段名，默认 'id'。设为 'index' 则使用数组索引作为 key */
  rowKey: { type: String, default: 'id' },
  /** 是否禁用操作 */
  disabled: { type: Boolean, default: false },
  /** 是否显示默认删除按钮 */
  showRemove: { type: Boolean, default: true },
  /** 是否显示提示文字 */
  showHint: { type: Boolean, default: true },
  /** 提示文字 */
  hint: { type: String, default: '点击上下箭头调整顺序，点击 x 删除' },
  /** 空数据时的提示文字 */
  emptyText: { type: String, default: '暂无数据' },
  /** 空数据图标大小 */
  emptyImageSize: { type: Number, default: 80 },
  /** 元素的标签字段名，用于默认插槽显示文本 */
  labelKey: { type: String, default: '' },
})

const emit = defineEmits([
  'update:modelValue',
  'change',
  'remove',
  'sort',
])

const list = ref([...props.modelValue])
const isInternalChange = ref(false)

watch(() => props.modelValue, (val) => {
  if (isInternalChange.value) return
  list.value = [...val]
}, { deep: true })

watch(list, (val) => {
  isInternalChange.value = true
  emit('update:modelValue', val)
  nextTick(() => { isInternalChange.value = false })
}, { deep: true })

const getLabel = (element) => {
  if (props.labelKey) return element[props.labelKey]
  return element.label || element.name || element.title || String(element)
}

const handleMove = (index, direction) => {
  const newIndex = index + direction
  if (newIndex < 0 || newIndex >= list.value.length) return
  const temp = list.value[index]
  list.value[index] = list.value[newIndex]
  list.value[newIndex] = temp
  // 触发响应式更新
  list.value = [...list.value]
  emit('change', list.value)
  emit('sort', { from: index, to: newIndex, list: list.value })
}

const handleRemove = (index) => {
  const removed = list.value.splice(index, 1)
  list.value = [...list.value]
  emit('change', list.value)
  emit('remove', removed[0], index, list.value)
}

defineExpose({
  /** 内部列表数据 */
  list,
  /** 手动上移指定索引的项 */
  moveUp: (index) => handleMove(index, -1),
  /** 手动下移指定索引的项 */
  moveDown: (index) => handleMove(index, 1),
  /** 手动移除指定索引的项 */
  remove: (index) => handleRemove(index),
})
</script>

<style scoped>
.kh-sort-list__container {
  padding: 12px;
  background-color: #fafbfc;
  border-radius: 8px;
  border: 1px solid #e5e6eb;
}

.kh-sort-list__hint {
  font-size: 12px;
  color: #86909c;
  margin-bottom: 10px;
}

.kh-sort-list__list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.kh-sort-list__item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 14px;
  background-color: #fff;
  border-radius: 6px;
  border: 1px solid #e5e6eb;
  transition: all 0.2s ease;
}

.kh-sort-list__item:hover {
  border-color: #c9cdd4;
  background-color: #f7f8fa;
}

.kh-sort-list__body {
  flex: 1;
  min-width: 0;
}

.kh-sort-list__tag {
  font-size: 14px;
}

.kh-sort-list__actions {
  display: flex;
  gap: 6px;
  flex-shrink: 0;
  margin-left: 12px;
}
</style>
