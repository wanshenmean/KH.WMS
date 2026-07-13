<template>
  <div class="kh-editable-table">
    <div v-if="showAdd" class="kh-editable-table__toolbar">
      <el-button type="primary" size="small" @click="handleAdd">
        <el-icon><Plus /></el-icon> {{ addText }}
      </el-button>
    </div>

    <el-table
      ref="tableRef"
      :data="modelValue"
      v-bind="$attrs"
      :border="border"
      :stripe="stripe"
      :size="size"
      :max-height="maxHeight"
      :empty-text="emptyText"
      :row-key="rowKey"
      style="width: 100%;"
    >
      <!-- 序号列 -->
      <el-table-column v-if="showIndex" type="index" :label="indexLabel" :width="indexWidth" align="center" header-align="center" />

      <!-- 数据列 -->
      <el-table-column
        v-for="col in visibleColumns"
        :key="col.prop"
        :prop="col.prop"
        :label="col.label"
        :width="col.width"
        :min-width="col.minWidth"
        :fixed="col.fixed"
        :align="col.align || 'center'"
        :header-align="col.headerAlign || 'center'"
        :show-overflow-tooltip="col.showOverflowTooltip"
      >
        <template #default="{ row, $index }">
          <!-- slot 类型 -->
          <slot v-if="col.type === 'slot'" :name="col.prop" :row="row" :column="col" :index="$index" />

          <!-- input -->
          <el-input v-else-if="col.type === 'input'"
            v-model="row[col.prop]"
            :size="inputSize"
            :placeholder="col.placeholder || ''"
            :maxlength="col.maxlength"
            :disabled="col.disabled"
            :clearable="col.clearable !== false"
            @change="handleCellChange(col, row, $index)"
          />

          <!-- number -->
          <el-input-number v-else-if="col.type === 'number'"
            v-model="row[col.prop]"
            :size="inputSize"
            :min="col.min"
            :max="col.max"
            :precision="col.precision"
            :step="col.step || 1"
            :controls="col.controls !== false"
            :placeholder="col.placeholder || ''"
            :disabled="col.disabled"
            controls-position="right"
            @change="handleCellChange(col, row, $index)"
          />

          <!-- select -->
          <el-select v-else-if="col.type === 'select'"
            v-model="row[col.prop]"
            :size="inputSize"
            :placeholder="col.placeholder || '请选择'"
            :clearable="col.clearable !== false"
            :filterable="col.filterable"
            :disabled="col.disabled"
            :multiple="col.multiple"
            style="width: 100%;"
            @change="handleCellChange(col, row, $index)"
          >
            <el-option
              v-for="opt in getOptions(col)"
              :key="opt.value"
              :label="opt.label"
              :value="opt.value"
            />
          </el-select>

          <!-- date -->
          <el-date-picker v-else-if="col.type === 'date'"
            v-model="row[col.prop]"
            :type="col.dateType || 'date'"
            :size="inputSize"
            :placeholder="col.placeholder || '选择日期'"
            :value-format="col.valueFormat || 'YYYY-MM-DD'"
            :disabled="col.disabled"
            :clearable="col.clearable !== false"
            style="width: 100%;"
            @change="handleCellChange(col, row, $index)"
          />

          <!-- switch -->
          <el-switch v-else-if="col.type === 'switch'"
            v-model="row[col.prop]"
            :active-value="col.activeValue ?? 1"
            :inactive-value="col.inactiveValue ?? 0"
            :disabled="col.disabled"
            @change="handleCellChange(col, row, $index)"
          />

          <!-- 默认文本 -->
          <span v-else>{{ row[col.prop] ?? '' }}</span>
        </template>
      </el-table-column>

      <!-- 操作列 -->
      <el-table-column v-if="showAction" :label="actionLabel" :width="actionWidth" :fixed="actionFixed" align="center" header-align="center">
        <template #default="{ row, $index }">
          <slot name="action" :row="row" :index="$index">
            <el-button v-if="!hideDelete" type="danger" link :size="inputSize" @click="handleDelete($index)">
              删除
            </el-button>
          </slot>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>

<script setup>
import { useDictStore } from '@/stores/dict'
import { collectDictTypes, resolveColumn } from '@/utils/dict-resolve'

const dictStore = useDictStore()

const props = defineProps({
  /** 表格数据（v-model） */
  modelValue: { type: Array, default: () => [] },
  /** 列配置 */
  columns: { type: Array, default: () => [] },
  /** 行数据唯一标识 */
  rowKey: { type: [String, Function], default: '' },
  /** 边框 */
  border: { type: Boolean, default: true },
  /** 斑马纹 */
  stripe: { type: Boolean, default: false },
  /** 尺寸 */
  size: { type: String, default: 'default' },
  /** 最大高度 */
  maxHeight: { type: [String, Number], default: 360 },
  /** 空数据文案 */
  emptyText: { type: String, default: '暂无数据' },
  /** 显示序号列 */
  showIndex: { type: Boolean, default: true },
  /** 序号列宽度 */
  indexWidth: { type: Number, default: 55 },
  /** 序号列标题 */
  indexLabel: { type: String, default: '序号' },
  /** 显示操作列 */
  showAction: { type: Boolean, default: true },
  /** 操作列宽度 */
  actionWidth: { type: [String, Number], default: 70 },
  /** 操作列固定 */
  actionFixed: { type: String, default: 'right' },
  /** 操作列标题 */
  actionLabel: { type: String, default: '操作' },
  /** 隐藏删除按钮 */
  hideDelete: { type: Boolean, default: false },
  /** 显示新增按钮 */
  showAdd: { type: Boolean, default: true },
  /** 新增按钮文案 */
  addText: { type: String, default: '添加行' },
  /** 新增行的默认数据工厂 */
  defaultRow: { type: Function, default: null },
  /** 编辑控件尺寸 */
  inputSize: { type: String, default: 'small' },
})

const emit = defineEmits(['update:modelValue', 'cell-change', 'add', 'delete'])

const tableRef = ref(null)

/** 解析后的列配置（字典引用已展开） */
const resolvedColumns = ref([])

/** 预加载字典 + 解析列配置 */
async function initColumns() {
  console.log('初始化列配置', props.columns)
  const dictTypes = collectDictTypes(props.columns)
  await Promise.all([...dictTypes].map(type => dictStore.getDict(type)))

  resolvedColumns.value = props.columns.map(col => resolveColumn(col, dictStore.cache))
}

onMounted(initColumns)
watch(() => props.columns, initColumns, { deep: true })

const visibleColumns = computed(() => resolvedColumns.value.filter(col => col.visible !== false))

/** 获取列的选项列表（支持数组、函数、ref） */
function getOptions(col) {
  const options = col.options
  if (!options) return []
  if (Array.isArray(options)) return options
  if (typeof options === 'function') return options()
  return []
}

/** 新增行 */
function handleAdd() {
  const newRow = props.defaultRow ? props.defaultRow() : {}
  const list = [...props.modelValue, newRow]
  emit('update:modelValue', list)
  emit('add', newRow)
}

/** 删除行 */
function handleDelete(index) {
  const list = [...props.modelValue]
  const removed = list.splice(index, 1)[0]
  emit('update:modelValue', list)
  emit('delete', removed, index)
}

/** 单元格值变化 */
function handleCellChange(col, row, index) {
  emit('cell-change', col.prop, row, index)
}
</script>

<style scoped>
.kh-editable-table__toolbar {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 8px;
}

/* 编辑控件撑满单元格 */
:deep(.el-input-number) {
  width: 100%;
}

:deep(.el-select) {
  width: 100%;
}

:deep(.el-date-editor) {
  width: 100%;
}
</style>
