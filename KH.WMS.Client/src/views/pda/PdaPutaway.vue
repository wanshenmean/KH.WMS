<template>
  <div class="pda-putaway">
    <!-- 步骤条 -->
    <el-steps :active="currentStep" finish-status="success" simple class="pda-putaway__steps">
      <el-step title="扫码" />
      <el-step title="分配库位" />
      <el-step title="确认" />
    </el-steps>

    <!-- Step 1: 扫码 -->
    <div v-show="currentStep === 0" class="pda-putaway__section">
      <el-card shadow="never" class="pda-putaway__card">
        <template #header>
          <span class="card-title">扫描物料条码</span>
        </template>
        <el-form label-position="top" class="pda-putaway__form">
          <el-form-item label="物料条码">
            <el-input
              v-model="materialBarcode"
              ref="barcodeInputRef"
              placeholder="请扫描或输入物料条码"
              size="large"
              clearable
              @keyup.enter="handleScanMaterial"
            />
          </el-form-item>
          <el-button
            type="primary"
            size="large"
            class="pda-putaway__btn-full"
            @click="handleScanMaterial"
          >
            <el-icon class="btn-icon"><component :is="ScanIcon" /></el-icon>
            扫码查询
          </el-button>
        </el-form>
      </el-card>

      <!-- 物料信息卡片 -->
      <el-card v-if="materialInfo" shadow="never" class="pda-putaway__card">
        <template #header>
          <span class="card-title">物料信息</span>
        </template>
        <div class="material-banner">
          <div class="material-banner-code">{{ materialInfo.materialCode }}</div>
          <div class="material-banner-name">{{ materialInfo.materialName }}</div>
        </div>
        <div class="info-grid">
          <div class="info-row">
            <span class="info-label">批次号</span>
            <span class="info-value">{{ materialInfo.batchNo }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">数量</span>
            <span class="info-value highlight">{{ materialInfo.quantity }} {{ materialInfo.unit }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">来源单号</span>
            <span class="info-value">{{ materialInfo.sourceNo }}</span>
          </div>
        </div>
        <el-button
          type="primary"
          size="large"
          class="pda-putaway__btn-full"
          @click="goToStep(1)"
        >
          下一步：分配库位
        </el-button>
      </el-card>
    </div>

    <!-- Step 2: 分配库位 -->
    <div v-show="currentStep === 1" class="pda-putaway__section">
      <el-card shadow="never" class="pda-putaway__card">
        <template #header>
          <span class="card-title">库位分配</span>
        </template>

        <!-- 推荐库位 -->
        <div class="recommend-section">
          <div class="recommend-label">
            <el-icon class="recommend-icon"><component :is="LocationIcon" /></el-icon>
            系统推荐库位
          </div>
          <div class="recommend-bin">{{ recommendedBin }}</div>
          <div class="recommend-hint">
            库区: A区 | 货架: 02 | 层: 03 | 位: 04
          </div>
        </div>

        <div class="divider"></div>

        <!-- 库位选择 -->
        <el-form label-position="top" class="pda-putaway__form">
          <el-form-item label="是否使用推荐库位">
            <div class="bin-choice-group">
              <el-button
                type="primary"
                size="large"
                class="bin-choice-btn"
                :class="{ 'is-active': useRecommended }"
                @click="useRecommended = true"
              >
                使用推荐库位
              </el-button>
              <el-button
                size="large"
                class="bin-choice-btn"
                :class="{ 'is-active': !useRecommended }"
                @click="useRecommended = false"
              >
                手动输入库位
              </el-button>
            </div>
          </el-form-item>

          <el-form-item v-if="!useRecommended" label="目标库位">
            <el-input
              v-model="manualBin"
              ref="manualBinRef"
              placeholder="请扫描或输入库位编码"
              size="large"
              clearable
            />
          </el-form-item>

          <div v-if="!useRecommended && manualBin" class="bin-preview">
            <span class="bin-preview-label">目标库位:</span>
            <span class="bin-preview-value">{{ manualBin }}</span>
          </div>

          <el-button
            type="primary"
            size="large"
            class="pda-putaway__btn-full"
            :disabled="!useRecommended && !manualBin"
            @click="goToStep(2)"
          >
            确认库位
          </el-button>
          <el-button
            size="large"
            class="pda-putaway__btn-full"
            @click="goToStep(0)"
          >
            返回扫码
          </el-button>
        </el-form>
      </el-card>
    </div>

    <!-- Step 3: 确认上架 -->
    <div v-show="currentStep === 2" class="pda-putaway__section">
      <el-card shadow="never" class="pda-putaway__card">
        <template #header>
          <span class="card-title">上架确认</span>
        </template>

        <div class="confirm-list">
          <div class="confirm-item">
            <span class="confirm-label">物料编码</span>
            <span class="confirm-value">{{ materialInfo.materialCode }}</span>
          </div>
          <div class="confirm-item">
            <span class="confirm-label">物料名称</span>
            <span class="confirm-value">{{ materialInfo.materialName }}</span>
          </div>
          <div class="confirm-item">
            <span class="confirm-label">批次号</span>
            <span class="confirm-value">{{ materialInfo.batchNo }}</span>
          </div>
          <div class="confirm-item">
            <span class="confirm-label">上架数量</span>
            <span class="confirm-value highlight">{{ materialInfo.quantity }} {{ materialInfo.unit }}</span>
          </div>
          <div class="confirm-item">
            <span class="confirm-label">目标库位</span>
            <span class="confirm-value bin-highlight">{{ finalBin }}</span>
          </div>
        </div>

        <el-button
          type="success"
          size="large"
          class="pda-putaway__btn-full"
          :loading="submitting"
          @click="handleSubmit"
        >
          <el-icon class="btn-icon"><component :is="CheckIcon" /></el-icon>
          确认上架
        </el-button>
        <el-button
          size="large"
          class="pda-putaway__btn-full"
          @click="goToStep(1)"
        >
          返回修改库位
        </el-button>
      </el-card>

      <!-- 成功结果 -->
      <el-result
        v-if="submitted"
        icon="success"
        title="上架成功"
        :sub-title="`${materialInfo.materialName} 已上架至 ${finalBin}`"
      >
        <template #extra>
          <el-button
            type="primary"
            size="large"
            class="pda-putaway__btn-full"
            @click="handleReset"
          >
            继续上架
          </el-button>
        </template>
      </el-result>
    </div>
  </div>
</template>

<script setup>

const ScanIcon = markRaw(Camera)
const CheckIcon = markRaw(Check)
const LocationIcon = markRaw(Location)

/** 当前步骤 */
const currentStep = ref(0)

/** 物料条码 */
const materialBarcode = ref('')
const barcodeInputRef = ref(null)

/** 库位选择 */
const useRecommended = ref(true)
const manualBin = ref('')
const manualBinRef = ref(null)

/** 推荐库位 */
const recommendedBin = ref('A-02-03-04')

/** 提交状态 */
const submitting = ref(false)
const submitted = ref(false)

/** 物料信息（模拟数据） */
const materialInfo = ref(null)

const mockMaterialData = {
  materialCode: 'MAT-0015',
  materialName: '贴片电容 0402 22uF',
  batchNo: 'B20260409001',
  quantity: 10000,
  unit: 'PCS',
  sourceNo: 'ASN-2026-04-0901',
}

/** 最终库位 */
const finalBin = computed(() => {
  return useRecommended.value ? recommendedBin.value : manualBin.value
})

/** 扫描物料条码 */
const handleScanMaterial = () => {
  if (!materialBarcode.value.trim()) {
    KhMessageFn.warning('请输入或扫描物料条码')
    return
  }

  setTimeout(() => {
    materialInfo.value = { ...mockMaterialData }
    KhMessageFn.success('物料查询成功')
  }, 300)
}

/** 切换步骤 */
const goToStep = (step) => {
  if (step === 2 && !useRecommended.value && !manualBin.value) {
    KhMessageFn.warning('请输入目标库位')
    return
  }
  currentStep.value = step
}

/** 提交上架 */
const handleSubmit = () => {
  submitting.value = true
  setTimeout(() => {
    submitting.value = false
    submitted.value = true
    KhMessageFn.success('上架完成！')
  }, 1200)
}

/** 重置 */
const handleReset = () => {
  currentStep.value = 0
  materialBarcode.value = ''
  materialInfo.value = null
  useRecommended.value = true
  manualBin.value = ''
  submitted.value = false
  nextTick(() => {
    barcodeInputRef.value?.focus()
  })
}

/** 切换到手动输入时自动聚焦 */
watch(useRecommended, (val) => {
  if (!val) {
    nextTick(() => {
      manualBinRef.value?.focus()
    })
  }
})

onMounted(() => {
  nextTick(() => {
    barcodeInputRef.value?.focus()
  })
})
</script>

<style scoped>
.pda-putaway {
  padding-bottom: 16px;
}

/* 步骤条 */
.pda-putaway__steps {
  margin-bottom: 12px;
  border-radius: 8px;
  overflow: hidden;
}

.pda-putaway__steps :deep(.el-step__title) {
  font-size: 15px;
  font-weight: 600;
}

/* 卡片 */
.pda-putaway__card {
  margin-bottom: 12px;
  border-radius: 10px;
}

.pda-putaway__card :deep(.el-card__header) {
  padding: 12px 16px;
  background-color: #f0f7ff;
  border-bottom: 1px solid #d9ecff;
}

.card-title {
  font-size: 17px;
  font-weight: 700;
  color: #303133;
}

/* 表单 */
.pda-putaway__form {
  margin-top: 4px;
}

.pda-putaway__form :deep(.el-form-item__label) {
  font-size: 15px;
  font-weight: 600;
  color: #606266;
  padding-bottom: 6px;
}

.pda-putaway__form :deep(.el-input__wrapper) {
  min-height: 48px;
  font-size: 16px;
}

/* 按钮 */
.pda-putaway__btn-full {
  width: 100%;
  min-height: 48px;
  font-size: 17px;
  font-weight: 600;
  margin-top: 12px;
  border-radius: 10px;
}

.btn-icon {
  margin-right: 6px;
  font-size: 20px;
}

/* 物料横幅 */
.material-banner {
  text-align: center;
  padding: 16px 12px;
  background: linear-gradient(135deg, #409eff 0%, #66b1ff 100%);
  border-radius: 8px;
  margin-bottom: 16px;
}

.material-banner-code {
  font-size: 20px;
  font-weight: 800;
  color: #fff;
  letter-spacing: 1px;
}

.material-banner-name {
  font-size: 15px;
  color: rgba(255, 255, 255, 0.9);
  margin-top: 4px;
}

/* 信息网格 */
.info-grid {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px solid #f0f0f0;
}

.info-label {
  font-size: 14px;
  color: #909399;
}

.info-value {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}

.info-value.highlight {
  color: #e6a23c;
  font-size: 18px;
}

/* 推荐库位 */
.recommend-section {
  text-align: center;
  padding: 16px 0;
}

.recommend-label {
  font-size: 14px;
  color: #909399;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  margin-bottom: 8px;
}

.recommend-icon {
  font-size: 16px;
  color: #409eff;
}

.recommend-bin {
  font-size: 32px;
  font-weight: 800;
  color: #409eff;
  letter-spacing: 3px;
  padding: 8px 0;
}

.recommend-hint {
  font-size: 13px;
  color: #b0b3b8;
}

/* 分割线 */
.divider {
  height: 1px;
  background-color: #e5e6eb;
  margin: 12px 0;
}

/* 库位选择按钮组 */
.bin-choice-group {
  display: flex;
  gap: 10px;
  width: 100%;
}

.bin-choice-btn {
  flex: 1;
  min-height: 48px;
  font-size: 15px;
  font-weight: 600;
  border-radius: 10px;
}

.bin-choice-btn.is-active {
  border-color: #409eff;
  color: #409eff;
  font-weight: 700;
}

/* 库位预览 */
.bin-preview {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px;
  background-color: #f5f7fa;
  border-radius: 8px;
  margin-bottom: 8px;
}

.bin-preview-label {
  font-size: 14px;
  color: #909399;
}

.bin-preview-value {
  font-size: 18px;
  font-weight: 700;
  color: #409eff;
}

/* 确认列表 */
.confirm-list {
  display: flex;
  flex-direction: column;
}

.confirm-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid #f0f0f0;
}

.confirm-item:last-child {
  border-bottom: none;
}

.confirm-label {
  font-size: 15px;
  color: #909399;
}

.confirm-value {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}

.confirm-value.highlight {
  color: #e6a23c;
  font-size: 18px;
}

.confirm-value.bin-highlight {
  color: #409eff;
  font-size: 20px;
  font-weight: 800;
}

/* 结果 */
.pda-putaway :deep(.el-result) {
  padding: 24px 0;
}

.pda-putaway :deep(.el-result__title p) {
  font-size: 22px;
  font-weight: 700;
}

.pda-putaway :deep(.el-result__subtitle p) {
  font-size: 15px;
}
</style>
