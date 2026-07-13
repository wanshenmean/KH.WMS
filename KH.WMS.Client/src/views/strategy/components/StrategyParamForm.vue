<template>
  <el-form label-width="120px" size="default">
    <!-- 无参数时的提示 -->
    <el-form-item v-if="!currentSchema || currentSchema.length === 0" label="策略参数">
      <el-text type="info">该策略无需额外参数配置</el-text>
    </el-form-item>

    <template v-for="field in currentSchema" :key="field.prop">
      <!-- switch 开关 -->
      <el-form-item v-if="field.type === 'switch'" :label="field.label">
        <el-switch v-model="paramData[field.prop]" />
      </el-form-item>

      <!-- number 数字输入 -->
      <el-form-item v-else-if="field.type === 'number'" :label="field.label" :required="field.required">
        <el-input-number v-model="paramData[field.prop]" :min="field.min ?? 0" :max="field.max ?? 9999"
          :step="field.step ?? 1" :precision="field.precision ?? 0" controls-position="right"
          style="width: 220px" />
      </el-form-item>

      <!-- select 下拉选择 -->
      <el-form-item v-else-if="field.type === 'select'" :label="field.label" :required="field.required">
        <el-select v-model="paramData[field.prop]" :placeholder="`请选择${field.label}`" clearable style="width: 220px">
          <el-option v-for="opt in field.options" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
      </el-form-item>

      <!-- sort-rules 排序规则编辑器 -->
      <el-form-item v-else-if="field.type === 'sort-rules'" :label="field.label">
        <div class="dynamic-table-wrapper">
          <div class="dynamic-table-header">
            <span class="dynamic-table-col" style="width: 50px; flex-shrink: 0;">序号</span>
            <span class="dynamic-table-col">排序字段</span>
            <span class="dynamic-table-col" style="width: 140px;">排序方式</span>
            <span class="dynamic-table-col" style="width: 60px; flex-shrink: 0;">操作</span>
          </div>
          <div v-for="(rule, idx) in paramData[field.prop]" :key="idx" class="dynamic-table-row">
            <span class="dynamic-table-col" style="width: 80px; flex-shrink: 0; text-align: center; color: #909399;">{{ idx + 1 }}</span>
            <el-select v-model="rule.field" placeholder="选择字段" size="small" class="dynamic-table-col">
              <el-option v-for="f in sortFieldOptions" :key="f.value" :label="f.label" :value="f.value"
                :disabled="isSortFieldUsed(field.prop, f.value, idx)" />
            </el-select>
            <el-select v-model="rule.direction" size="small" class="dynamic-table-col" style="width: 140px;">
              <el-option label="升序 (ASC)" value="ASC" />
              <el-option label="降序 (DESC)" value="DESC" />
            </el-select>
            <div class="dynamic-table-col" style="width: 80px; flex-shrink: 0; text-align: center;">
              <el-button type="danger" link size="small" @click="removeSortRule(field.prop, idx)">删除</el-button>
            </div>
          </div>
          <el-button type="primary" link size="small" @click="addSortRule(field.prop)">
            + 添加排序规则
          </el-button>
          <div v-if="field.hint" class="param-hint">
            <el-text type="info" size="small">{{ field.hint }}</el-text>
          </div>
        </div>
      </el-form-item>

      <!-- category-mapping 品类-库区映射编辑器 -->
      <el-form-item v-else-if="field.type === 'category-mapping'" :label="field.label">
        <div class="dynamic-table-wrapper">
          <div class="dynamic-table-header">
            <span class="dynamic-table-col" style="width: 50px; flex-shrink: 0;">序号</span>
            <span class="dynamic-table-col">物料分类ID</span>
            <span class="dynamic-table-col">库区编码</span>
            <span class="dynamic-table-col" style="width: 60px; flex-shrink: 0;">操作</span>
          </div>
          <div v-for="(item, idx) in paramData[field.prop]" :key="idx" class="dynamic-table-row">
            <span class="dynamic-table-col" style="width: 50px; flex-shrink: 0; text-align: center; color: #909399;">{{ idx + 1 }}</span>
            <el-input v-model="item.key" placeholder="输入分类ID" size="small" class="dynamic-table-col" />
            <el-input v-model="item.value" placeholder="输入库区编码" size="small" class="dynamic-table-col" />
            <div class="dynamic-table-col" style="width: 60px; flex-shrink: 0; text-align: center;">
              <el-button type="danger" link size="small" @click="removeMappingItem(field.prop, idx)">删除</el-button>
            </div>
          </div>
          <el-button type="primary" link size="small" @click="addMappingItem(field.prop)">
            + 添加映射
          </el-button>
          <div v-if="field.hint" class="param-hint">
            <el-text type="info" size="small">{{ field.hint }}</el-text>
          </div>
        </div>
      </el-form-item>

      <!-- textarea 带模板提示 -->
      <el-form-item v-else-if="field.type === 'textarea'" :label="field.label">
        <el-input v-model="paramData[field.prop]" type="textarea" :rows="field.rows ?? 4"
          :placeholder="field.placeholder ?? ''" />
        <div v-if="field.hint" class="param-hint">
          <el-text type="info" size="small">{{ field.hint }}</el-text>
        </div>
      </el-form-item>

      <!-- input 文本输入 -->
      <el-form-item v-else :label="field.label" :required="field.required">
        <el-input v-model="paramData[field.prop]" :placeholder="field.placeholder ?? `请输入${field.label}`"
          :maxlength="field.maxlength" style="width: 220px" />
      </el-form-item>
    </template>
  </el-form>
</template>

<script setup>

const props = defineProps({
  /** 策略规则编码 */
  ruleCode: { type: String, default: '' },
  /** 参数JSON字符串（编辑时回显） */
  paramsJson: { type: String, default: '' },
  /** 策略参数表单Schema（从后端API动态获取） */
  schema: { type: Array, default: () => [] },
})

const emit = defineEmits(['update:paramsJson'])

// ==================== 通用排序字段选项 ====================
const sortFieldOptions = [
  { label: '层 (LayerNo)', value: 'LayerNo' },
  { label: '行/排 (RowNo)', value: 'RowNo' },
  { label: '列 (ColNo)', value: 'ColNo' },
  { label: '深 (Depth)', value: 'Depth' },
  { label: '巷道 (AisleNo)', value: 'AisleNo' },
  { label: '评分 (Score)', value: 'Score' },
  { label: '库区 (ZoneCode)', value: 'ZoneCode' },
]

// 当前参数表单配置
const currentSchema = ref([])

// 参数数据
const paramData = reactive({})

/** 初始化默认值 */
function initDefaults(schema) {
  schema.forEach(field => {
    if (field.defaultValue != null) {
      paramData[field.prop] = Array.isArray(field.defaultValue) ? [...field.defaultValue] : field.defaultValue
    }
  })
}

/** 从JSON字符串解析参数 */
function parseFromJson(json) {
  if (!json) return
  try {
    const parsed = JSON.parse(json)
    // 构建 prop → type 映射，用于类型感知解析
    const fieldTypeMap = {}
    currentSchema.value.forEach(f => { if (f.type) fieldTypeMap[f.prop] = f.type })

    Object.keys(parsed).forEach(key => {
      const val = parsed[key]
      if (key === 'SortRules' && Array.isArray(val)) {
        // 后端格式 [{Field, Direction}] → 前端格式 [{field, direction}]
        paramData[key] = val.map(v => ({
          field: v.Field || v.field || '',
          direction: v.Direction || v.direction || 'ASC',
        }))
      } else if (fieldTypeMap[key] === 'category-mapping' && val != null && typeof val === 'object' && !Array.isArray(val)) {
        // 后端存储为 {key: value} 对象 → 转为前端 [{key, value}] 数组
        paramData[key] = Object.entries(val).map(([k, v]) => ({ key: String(k), value: String(v) }))
      } else if (Array.isArray(val)) {
        paramData[key] = val.map(v => ({ ...v }))
      } else {
        paramData[key] = val
      }
    })
  } catch {
    // JSON解析失败时忽略
  }
}

/** 导出为JSON字符串 */
function getParamJson() {
  const result = {}
  currentSchema.value.forEach(field => {
    const val = paramData[field.prop]
    if (val === undefined || val === null) return

    if (field.type === 'sort-rules') {
      if (!Array.isArray(val)) return
      // 前端格式 [{field, direction}] → 后端格式 [{Field, Direction}]
      const rules = val
        .filter(r => r.field)
        .map(r => ({ Field: r.field, Direction: r.direction || 'ASC' }))
      if (rules.length > 0) result[field.prop] = rules
    } else if (field.type === 'category-mapping') {
      if (!Array.isArray(val)) return
      // 将 [{key, value}] 转为 {key: value}
      const obj = {}
      val.forEach(item => {
        if (item.key) obj[item.key] = item.value || ''
      })
      if (Object.keys(obj).length > 0) result[field.prop] = obj
    } else {
      result[field.prop] = val
    }
  })
  return Object.keys(result).length > 0 ? JSON.stringify(result) : ''
}

/** 添加排序规则 */
function addSortRule(prop) {
  if (!Array.isArray(paramData[prop])) paramData[prop] = []
  paramData[prop].push({ field: '', direction: 'ASC' })
}

/** 删除排序规则 */
function removeSortRule(prop, idx) {
  paramData[prop].splice(idx, 1)
}

/** 判断排序字段是否已被其他行选中 */
function isSortFieldUsed(prop, fieldValue, currentIndex) {
  if (!fieldValue) return false
  return (paramData[prop] || []).some((r, idx) => idx !== currentIndex && r.field === fieldValue)
}

/** 添加品类映射 */
function addMappingItem(prop) {
  if (!Array.isArray(paramData[prop])) paramData[prop] = []
  paramData[prop].push({ key: '', value: '' })
}

/** 删除品类映射 */
function removeMappingItem(prop, idx) {
  paramData[prop].splice(idx, 1)
}

/** 监听 ruleCode/schema 变化，切换参数表单 */
watch([() => props.ruleCode, () => props.schema], () => {
  // 清空旧数据
  Object.keys(paramData).forEach(key => delete paramData[key])

  currentSchema.value = props.schema || []
  initDefaults(currentSchema.value)

  // 尝试从paramsJson恢复
  parseFromJson(props.paramsJson)
}, { immediate: true })

// 暴露方法给父组件
defineExpose({
  getParamJson,
  paramData,
})
</script>

<style scoped>
.param-hint {
  margin-top: 4px;
  line-height: 1.4;
}

.dynamic-table-wrapper {
  width: 100%;
  max-width: 520px;
}

.dynamic-table-header {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 0;
  border-bottom: 1px solid #ebeef5;
  font-size: 12px;
  color: #909399;
  font-weight: 500;
}

.dynamic-table-row {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 0;
}

.dynamic-table-col {
  flex: 1;
  min-width: 0;
}
</style>
