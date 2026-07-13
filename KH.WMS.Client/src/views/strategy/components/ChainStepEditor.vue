<template>
  <div class="chain-step-editor">
    <div class="step-header">
      <span class="step-title">步骤管理</span>
      <el-button type="primary" size="small" @click="addStep">
        <el-icon>
          <Plus />
        </el-icon> 添加步骤
      </el-button>
    </div>

    <div v-if="steps.length === 0" class="step-empty">
      <el-empty description="暂无步骤，请点击添加" :image-size="60" />
    </div>

    <div class="step-list">
      <div v-for="(step, index) in steps" :key="index" class="step-item" :class="{ 'step-disabled': !step.isEnabled }">
        <div class="step-drag-handle">
          <el-icon>
            <Rank />
          </el-icon>
        </div>

        <div class="step-number">{{ index + 1 }}</div>

        <div class="step-fields">
          <el-form-item label="步骤名称" label-width="80px" class="step-field">
            <el-input v-model="step.stepName" placeholder="请输入步骤名称" size="small" style="width: 160px" />
          </el-form-item>

          <el-form-item label="关联策略" label-width="80px" class="step-field">
            <el-select v-model="step.strategyConfigId" placeholder="选择策略配置" size="small" clearable style="width: 200px"
              filterable @change="onStrategyChange(index, $event)">
              <el-option v-for="s in strategyOptions" :key="s.id" :label="`${s.strategyName}（${s.ruleCode}）`"
                :value="s.id" />
            </el-select>
          </el-form-item>

          <el-form-item label="执行模式" label-width="80px" class="step-field">
            <el-select v-model="step.stepMode" size="small" style="width: 140px">
              <el-option label="链式执行" value="CHAIN" />
              <el-option label="并行执行" value="PARALLEL" />
              <el-option label="成功即停" value="STOP_ON_SUCCESS" />
            </el-select>
          </el-form-item>

          <el-form-item label="启用" label-width="50px" class="step-field step-enable">
            <el-switch v-model="step.isEnabled" />
          </el-form-item>
        </div>

        <div class="step-actions">
          <el-button type="primary" link size="small" @click="openParamDialog(index)">配置参数</el-button>
          <el-button type="danger" link size="small" @click="removeStep(index)">删除</el-button>
        </div>
      </div>
    </div>

    <!-- 步骤参数配置弹窗 -->
    <el-dialog v-model="paramDialogVisible" title="步骤参数配置" width="520px" append-to-body>
      <StrategyParamForm v-if="paramDialogVisible" ref="paramFormRef"
        :rule-code="currentEditStep ? currentEditStep.ruleCode : ''"
        :params-json="currentEditStep ? currentEditStep.stepParams : ''"
        :schema="currentEditStep ? (paramSchemaMap[currentEditStep.ruleCode] || []) : []" />
      <template #footer>
        <el-button @click="paramDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveStepParams">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import StrategyParamForm from './StrategyParamForm.vue'

const props = defineProps({
  /** 步骤数据列表 */
  modelValue: { type: Array, default: () => [] },
  /** 可选策略配置列表 */
  strategyOptions: { type: Array, default: () => [] },
  /** 策略参数Schema映射（从后端API获取） */
  paramSchemaMap: { type: Object, default: () => ({}) },
})

const emit = defineEmits(['update:modelValue'])

const steps = ref([])

// 参数配置弹窗
const paramDialogVisible = ref(false)
const paramFormRef = ref(null)
const currentEditIndex = ref(-1)
const currentEditStep = ref(null)

/** 初始化步骤数据 */
watch(() => props.modelValue, (val) => {
  if (val && val.length > 0) {
    steps.value = val.map(s => ({
      stepName: s.stepName || '',
      strategyConfigId: s.strategyConfigId || null,
      ruleCode: s.ruleCode || '',
      stepMode: s.stepMode || 'CHAIN',
      isEnabled: s.isEnabled !== 0,
      stepParams: s.stepParams || '',
      remark: s.remark || '',
    }))
  }
}, { immediate: true })

/** 同步步骤数据到父组件 */
function syncToParent() {
  emit('update:modelValue', steps.value.map((s, i) => ({
    stepNo: i + 1,
    stepName: s.stepName,
    strategyConfigId: s.strategyConfigId || null,
    ruleCode: s.ruleCode || '',
    stepMode: s.stepMode || 'CHAIN',
    isEnabled: s.isEnabled ? 1 : 0,
    stepParams: s.stepParams || '',
    remark: s.remark || '',
  })))
}

/** 添加步骤 */
function addStep() {
  steps.value.push({
    stepName: '',
    strategyConfigId: null,
    ruleCode: '',
    stepMode: 'CHAIN',
    isEnabled: true,
    stepParams: '',
    remark: '',
  })
  syncToParent()
}

/** 删除步骤 */
function removeStep(index) {
  steps.value.splice(index, 1)
  syncToParent()
}

/** 选择策略配置时同步ruleCode */
function onStrategyChange(index, configId) {
  const found = props.strategyOptions.find(s => s.id === configId)
  steps.value[index].ruleCode = found ? (found.ruleCode || '') : ''
  syncToParent()
}

/** 打开参数配置弹窗 */
function openParamDialog(index) {
  currentEditIndex.value = index
  currentEditStep.value = steps.value[index]
  paramDialogVisible.value = true
}

/** 保存步骤参数 */
function saveStepParams() {
  if (currentEditIndex.value >= 0 && paramFormRef.value) {
    steps.value[currentEditIndex.value].stepParams = paramFormRef.value.getParamJson()
    syncToParent()
  }
  paramDialogVisible.value = false
}

// 暴露方法
defineExpose({
  steps,
  syncToParent,
})
</script>

<style scoped>
.chain-step-editor {
  border: 1px solid #ebeef5;
  border-radius: 6px;
  padding: 12px;
  width: 100%;
}

.step-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.step-title {
  font-weight: 600;
  font-size: 14px;
  color: #303133;
}

.step-empty {
  padding: 16px 0;
}

.step-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.step-item {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  padding: 10px;
  border: 1px solid #ebeef5;
  border-radius: 4px;
  background: #fafafa;
  transition: background 0.2s;
}

.step-item:hover {
  background: #f0f7ff;
}

.step-item.step-disabled {
  opacity: 0.6;
}

.step-drag-handle {
  padding-top: 6px;
  cursor: grab;
  color: #c0c4cc;
  font-size: 16px;
}

.step-drag-handle:active {
  cursor: grabbing;
}

.step-number {
  min-width: 28px;
  height: 28px;
  line-height: 28px;
  text-align: center;
  border-radius: 50%;
  background: #409eff;
  color: #fff;
  font-size: 13px;
  font-weight: 600;
  flex-shrink: 0;
}

.step-fields {
  flex: 1;
  display: flex;
  flex-wrap: wrap;
  gap: 0 16px;
}

.step-field {
  margin-bottom: 0 !important;
}

.step-field :deep(.el-form-item__label) {
  font-size: 12px;
  padding: 0 8px 0 0;
}

.step-field :deep(.el-form-item__content) {
  line-height: 32px;
}

.step-enable {
  margin-left: auto;
  padding-top: 0;
}

.step-actions {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding-top: 4px;
  flex-shrink: 0;
}
</style>
