<template>
  <KhDialog
    v-model="visible"
    :title="title"
    :width="width"
    :height="height"
    :description="description"
    :show-footer="false"
    :show-close="showClose"
    :close-on-click-modal="closeOnClickModal"
    :destroy-on-close="destroyOnClose"
    @close="handleClose"
  >
    <div class="kh-detail">
      <!-- 主表信息 -->
      <el-descriptions :column="column" border :size="size" class="kh-detail__desc">
        <el-descriptions-item
          v-for="item in visibleItems"
          :key="item.prop"
          :label="item.label"
          :span="item.span || 1"
          :label-class-name="'kh-detail__label'"
          :content-class-name="'kh-detail__content'"
        >
          <!-- tag 类型 -->
          <template v-if="item.type === 'tag'">
            <el-tag :type="getTagType(item, data)" :size="tagSize" effect="light">
              {{ getDisplayValue(item, data) }}
            </el-tag>
          </template>
          <!-- slot 类型 -->
          <template v-else-if="item.type === 'slot'">
            <slot :name="item.prop" :row="data" :item="item" />
          </template>
          <!-- 默认文本 -->
          <template v-else>
            {{ getDisplayValue(item, data) }}
          </template>
        </el-descriptions-item>
      </el-descriptions>

      <!-- 从表信息（自动检测数组数据并渲染为表格） -->
      <template v-for="line in visibleLineConfigs" :key="line.prop">
        <div class="kh-detail__lines">
          <div class="kh-detail__lines-header">
            <span class="kh-detail__lines-title">{{ line.title || '明细行' }}</span>
            <span class="kh-detail__lines-count">共 {{ data[line.prop].length }} 条</span>
          </div>
          <div class="kh-detail__lines-table">
            <el-table
              :data="data[line.prop]"
              size="small"
              :max-height="line.maxHeight || 360"
              :header-cell-style="{ background: '#f5f7fa', color: '#606266', fontWeight: '500' }"
              :cell-style="{ padding: '8px 0' }"
            >
              <el-table-column type="index" label="#" width="50" align="center" />
              <el-table-column
                v-for="col in line.columns"
                :key="col.prop"
                :prop="col.prop"
                :label="col.label"
                :width="col.width"
                :min-width="col.minWidth"
                :align="col.align || 'center'"
                :show-overflow-tooltip="col.showOverflowTooltip !== false"
              >
                <template #default="{ row: lineRow }">
                  <template v-if="col.type === 'tag'">
                    <el-tag
                      :type="getLineTagType(col, lineRow)"
                      size="small"
                      effect="light"
                    >
                      {{ getLineDisplayValue(col, lineRow) }}
                    </el-tag>
                  </template>
                  <template v-else-if="col.type === 'slot'">
                    <slot :name="`line-${line.prop}-${col.prop}`" :row="lineRow" :line="line" />
                  </template>
                  <template v-else>
                    {{ getLineDisplayValue(col, lineRow) }}
                  </template>
                </template>
              </el-table-column>
            </el-table>
          </div>
        </div>
      </template>
    </div>
  </KhDialog>
</template>

<script setup>
import KhDialog from '@/components/KhDialog/index.vue'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  title: { type: String, default: '详情' },
  description: { type: String, default: '' },
  width: { type: [String, Number], default: '900px' },
  height: { type: [String, Number], default: '' },
  /**
   * 详情项配置数组
   * { prop, label, span?, type?, tagMap?, tagTypeMap?, formatter?, visible? }
   * type: 'tag' | 'slot' | 不填(纯文本)
   */
  items: { type: Array, default: () => [] },
  /**
   * 从表配置数组，用于展示主从关系中的明细行
   * { prop, title?, columns, maxHeight? }
   * - prop: data 中对应的数组字段名（如 'lines'）
   * - title: 从表区域标题，默认 '明细行'
   * - columns: el-table 列配置 [{ prop, label, width?, minWidth?, align?, type?, tagMap?, tagTypeMap?, formatter?, showOverflowTooltip? }]
   * - maxHeight: 表格最大高度，默认 360
   */
  lineConfigs: { type: Array, default: () => [] },
  /** el-descriptions 的 column 数 */
  column: { type: Number, default: 2 },
  /** el-descriptions 的 size */
  size: { type: String, default: 'large' },
  /** tag 模式下的 tag 尺寸 */
  tagSize: { type: String, default: 'default' },
  /** 行数据对象 */
  data: { type: Object, default: () => ({}) },
  showClose: { type: Boolean, default: true },
  closeOnClickModal: { type: Boolean, default: false },
  destroyOnClose: { type: Boolean, default: false },
})

const emit = defineEmits(['update:modelValue', 'close'])

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const visibleItems = computed(() => props.items.filter(item => {
  if (typeof item.visible === 'function') return item.visible(props.data)
  if (item.visible === false) return false
  return true
}))

/** 过滤出数据中实际存在且为非空数组的从表配置 */
const visibleLineConfigs = computed(() =>
  props.lineConfigs.filter(line => {
    const val = props.data[line.prop]
    return Array.isArray(val) && val.length > 0
  })
)

function getDisplayValue(item, row) {
  if (item.formatter) return item.formatter(row[item.prop], row)
  const val = row[item.prop]
  if (val === null || val === undefined || val === '') return '-'
  if (item.tagMap) {
    const map = typeof item.tagMap === 'string' ? item.tagMap : item.tagMap
    return map[val] ?? val
  }
  return val
}

function getTagType(item, row) {
  if (!item.tagTypeMap) return 'info'
  const val = row[item.prop]
  return item.tagTypeMap[val] || 'info'
}

function getLineDisplayValue(col, row) {
  if (col.formatter) return col.formatter(row[col.prop], row)
  const val = row[col.prop]
  if (val === null || val === undefined || val === '') return '-'
  if (col.tagMap) return col.tagMap[val] ?? val
  return val
}

function getLineTagType(col, row) {
  if (!col.tagTypeMap) return 'info'
  const val = row[col.prop]
  return col.tagTypeMap[val] || 'info'
}

function handleClose() {
  emit('close')
}
</script>

<style scoped>
.kh-detail {
  padding: 4px 0;
}

.kh-detail__desc :deep(.el-descriptions__label) {
  font-weight: 500;
  color: #909399;
}

.kh-detail__desc :deep(.el-descriptions__content) {
  color: #303133;
}

.kh-detail__desc :deep(.el-descriptions__cell) {
  padding: 14px 20px;
  min-height: 44px;
  font-size: 14px;
}

/* 从表区域 */
.kh-detail__lines {
  margin-top: 16px;
}

.kh-detail__lines-header {
  display: flex;
  align-items: center;
  gap: 8px;
  padding-bottom: 10px;
  border-bottom: 1px solid #ebeef5;
  margin-bottom: 12px;
}

.kh-detail__lines-title {
  font-size: 14px;
  font-weight: 600;
  color: #303133;
}

.kh-detail__lines-count {
  font-size: 12px;
  color: #909399;
}

.kh-detail__lines-table {
  border-radius: 6px;
  overflow: hidden;
}

/* 去掉 el-table 外边框，让它看起来更内嵌 */
.kh-detail__lines-table :deep(.el-table) {
  --el-table-border-color: #ebeef5;
  --el-table-header-bg-color: #f5f7fa;
}

.kh-detail__lines-table :deep(.el-table__inner-wrapper::before) {
  display: none;
}

.kh-detail__lines-table :deep(.el-table th.el-table__cell) {
  font-size: 13px;
  padding: 10px 0;
}

.kh-detail__lines-table :deep(.el-table td.el-table__cell) {
  font-size: 13px;
}

.kh-detail__lines-table :deep(.el-table__row:hover > td.el-table__cell) {
  background-color: #f5f7fa;
}
</style>
