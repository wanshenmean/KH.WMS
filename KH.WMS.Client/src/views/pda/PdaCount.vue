<template>
  <div class="pda-count">
    <!-- 步骤条 -->
    <el-steps :active="currentStep" finish-status="success" simple class="pda-count__steps">
      <el-step title="扫描库位" />
      <el-step title="盘点" />
      <el-step title="盘点汇总" />
    </el-steps>

    <!-- Step 1: 扫描库位 -->
    <div v-show="currentStep === 0" class="pda-count__section">
      <el-card shadow="never" class="pda-count__card">
        <template #header>
          <span class="card-title">扫描库位条码</span>
        </template>
        <el-form label-position="top" class="pda-count__form">
          <el-form-item label="库位条码">
            <el-input
              v-model="binBarcode"
              ref="binInputRef"
              placeholder="请扫描或输入库位条码"
              size="large"
              clearable
              @keyup.enter="handleScanBin"
            />
          </el-form-item>
          <el-button
            type="primary"
            size="large"
            class="pda-count__btn-full"
            @click="handleScanBin"
          >
            <el-icon class="btn-icon"><component :is="ScanIcon" /></el-icon>
            扫码查询
          </el-button>
        </el-form>
      </el-card>

      <!-- 库位信息 -->
      <el-card v-if="currentBinInfo" shadow="never" class="pda-count__card">
        <template #header>
          <span class="card-title">库位信息</span>
        </template>
        <div class="bin-banner">
          <div class="bin-banner__code">{{ currentBinInfo.binCode }}</div>
          <div class="bin-banner__zone">{{ currentBinInfo.zoneName }}</div>
        </div>
        <div class="info-grid">
          <div class="info-row">
            <span class="info-label">库区</span>
            <span class="info-value">{{ currentBinInfo.zoneName }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">货架</span>
            <span class="info-value">{{ currentBinInfo.shelfNo }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">物料编码</span>
            <span class="info-value code">{{ currentBinInfo.materialCode }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">物料名称</span>
            <span class="info-value">{{ currentBinInfo.materialName }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">系统数量</span>
            <span class="info-value highlight">{{ currentBinInfo.systemQty }} {{ currentBinInfo.unit }}</span>
          </div>
        </div>
        <el-button
          type="primary"
          size="large"
          class="pda-count__btn-full"
          @click="goToStep(1)"
        >
          开始盘点
        </el-button>
      </el-card>

      <!-- 盘点进度 -->
      <el-card v-if="countResults.length > 0" shadow="never" class="pda-count__card">
        <template #header>
          <span class="card-title">盘点进度</span>
        </template>
        <el-progress
          :percentage="countProgress"
          :stroke-width="18"
          :text-inside="true"
          :color="countProgress === 100 ? '#67c23a' : '#409eff'"
        />
        <div class="progress-text">
          已盘点 {{ countResults.length }}/{{ totalBins }} 个库位
        </div>
      </el-card>
    </div>

    <!-- Step 2: 盘点录入 -->
    <div v-show="currentStep === 1" class="pda-count__section">
      <el-card shadow="never" class="pda-count__card">
        <template #header>
          <span class="card-title">库存盘点</span>
        </template>

        <!-- 库位与物料信息 -->
        <div class="count-info-banner">
          <div class="count-bin-code">{{ currentBinInfo.binCode }}</div>
          <div class="count-material-info">
            {{ currentBinInfo.materialCode }} - {{ currentBinInfo.materialName }}
          </div>
        </div>

        <div class="count-compare">
          <div class="compare-item">
            <span class="compare-label">系统数量</span>
            <span class="compare-value">{{ currentBinInfo.systemQty }} {{ currentBinInfo.unit }}</span>
          </div>
          <div class="compare-divider">VS</div>
          <div class="compare-item">
            <span class="compare-label">实盘数量</span>
            <span class="compare-value" :class="{ 'is-diff': actualQty !== null && actualQty !== currentBinInfo.systemQty }">
              {{ actualQty !== null ? actualQty + ' ' + currentBinInfo.unit : '待录入' }}
            </span>
          </div>
        </div>

        <!-- 差异显示 -->
        <div v-if="actualQty !== null && difference !== 0" class="diff-alert">
          <el-icon class="diff-icon"><component :is="WarningIcon" /></el-icon>
          <span class="diff-text">
            {{ difference > 0 ? '盘盈' : '盘亏' }} {{ Math.abs(difference) }} {{ currentBinInfo.unit }}
          </span>
        </div>

        <div v-if="actualQty !== null && difference === 0" class="diff-ok">
          <el-icon class="diff-icon ok"><component :is="CircleCheckIcon" /></el-icon>
          <span class="diff-text ok">数量一致</span>
        </div>

        <el-form label-position="top" class="pda-count__form">
          <el-form-item label="实盘数量">
            <el-input-number
              v-model="actualQty"
              :min="0"
              :max="99999"
              size="large"
              controls-position="right"
              class="qty-input"
            />
          </el-form-item>

          <el-form-item label="差异原因（如有差异请填写）">
            <el-input
              v-model="diffRemark"
              type="textarea"
              placeholder="请输入差异原因说明"
              size="large"
              :rows="3"
              maxlength="200"
              show-word-limit
            />
          </el-form-item>

          <el-button
            type="primary"
            size="large"
            class="pda-count__btn-full"
            :disabled="actualQty === null"
            @click="recordAndNext"
          >
            <el-icon class="btn-icon"><component :is="RightIcon" /></el-icon>
            记录并{{ hasNextBin ? '下一个' : '完成' }}
          </el-button>
          <el-button
            size="large"
            class="pda-count__btn-full"
            @click="goToStep(0)"
          >
            返回扫描
          </el-button>
        </el-form>
      </el-card>
    </div>

    <!-- Step 3: 盘点汇总 -->
    <div v-show="currentStep === 2 && !showCompleteResult" class="pda-count__section">
      <el-card shadow="never" class="pda-count__card">
        <template #header>
          <span class="card-title">盘点汇总</span>
        </template>

        <div class="summary-stats">
          <div class="stat-card">
            <div class="stat-value">{{ countResults.length }}</div>
            <div class="stat-label">已盘库位</div>
          </div>
          <div class="stat-card ok">
            <div class="stat-value">{{ consistentCount }}</div>
            <div class="stat-label">数量一致</div>
          </div>
          <div class="stat-card error">
            <div class="stat-value">{{ diffCount }}</div>
            <div class="stat-label">存在差异</div>
          </div>
        </div>

        <!-- 差异明细 -->
        <div v-if="diffResults.length > 0" class="diff-section">
          <div class="diff-section__title">差异明细</div>
          <div
            v-for="(result, index) in diffResults"
            :key="index"
            class="diff-detail"
          >
            <div class="diff-detail__header">
              <span class="diff-detail__bin">{{ result.binCode }}</span>
              <el-tag :type="result.difference > 0 ? 'danger' : 'warning'" size="large" effect="dark">
                {{ result.difference > 0 ? '盘盈' : '盘亏' }}
              </el-tag>
            </div>
            <div class="diff-detail__body">
              <div class="diff-detail__row">
                <span class="diff-detail__label">物料</span>
                <span class="diff-detail__value">{{ result.materialCode }}</span>
              </div>
              <div class="diff-detail__row">
                <span class="diff-detail__label">系统数量</span>
                <span class="diff-detail__value">{{ result.systemQty }} {{ result.unit }}</span>
              </div>
              <div class="diff-detail__row">
                <span class="diff-detail__label">实盘数量</span>
                <span class="diff-detail__value">{{ result.actualQty }} {{ result.unit }}</span>
              </div>
              <div class="diff-detail__row">
                <span class="diff-detail__label">差异</span>
                <span class="diff-detail__value diff-val">
                  {{ result.difference > 0 ? '+' : '' }}{{ result.difference }} {{ result.unit }}
                </span>
              </div>
              <div v-if="result.remark" class="diff-detail__row">
                <span class="diff-detail__label">原因</span>
                <span class="diff-detail__value remark">{{ result.remark }}</span>
              </div>
            </div>
          </div>
        </div>

        <div v-else class="no-diff">
          <el-icon class="no-diff-icon"><component :is="CircleCheckIcon" /></el-icon>
          <span>所有库位数量一致，无差异</span>
        </div>

        <el-button
          type="success"
          size="large"
          class="pda-count__btn-full"
          :loading="submitting"
          @click="handleSubmit"
        >
          <el-icon class="btn-icon"><component :is="CheckIcon" /></el-icon>
          提交盘点结果
        </el-button>
      </el-card>
    </div>

    <!-- 完成结果 -->
    <el-result
      v-if="showCompleteResult"
      icon="success"
      title="盘点完成"
      :sub-title="`共盘点 ${countResults.length} 个库位，发现 ${diffCount} 处差异`"
    >
      <template #extra>
        <el-button
          type="primary"
          size="large"
          class="pda-count__btn-full"
          @click="handleReset"
        >
          开始新盘点
        </el-button>
      </template>
    </el-result>
  </div>
</template>

<script setup>

const ScanIcon = markRaw(Camera)
const CheckIcon = markRaw(Check)
const WarningIcon = markRaw(Warning)
const CircleCheckIcon = markRaw(CircleCheck)
const RightIcon = markRaw(Right)

/** 当前步骤 */
const currentStep = ref(0)

/** 库位条码 */
const binBarcode = ref('')
const binInputRef = ref(null)

/** 实盘数量 */
const actualQty = ref(null)

/** 差异原因 */
const diffRemark = ref('')

/** 提交状态 */
const submitting = ref(false)
const showCompleteResult = ref(false)

/** 当前库位信息 */
const currentBinInfo = ref(null)

/** 盘点结果列表 */
const countResults = reactive([])

/** 模拟库位数据（5个库位） */
const mockBins = [
  { binCode: 'A-01-01-01', zoneName: 'A区-电子料', shelfNo: '01号架', materialCode: 'MAT-1001', materialName: '贴片电阻 0603 10KΩ', systemQty: 5000, unit: 'PCS' },
  { binCode: 'A-01-02-03', zoneName: 'A区-电子料', shelfNo: '01号架', materialCode: 'MAT-2001', materialName: 'MCU STM32F103C8T6', systemQty: 200, unit: 'PCS' },
  { binCode: 'B-03-01-02', zoneName: 'B区-元器件', shelfNo: '03号架', materialCode: 'MAT-2002', materialName: '晶振 8MHz HC49S', systemQty: 500, unit: 'PCS' },
  { binCode: 'A-02-04-01', zoneName: 'A区-电子料', shelfNo: '02号架', materialCode: 'MAT-2003', materialName: '电解电容 470uF 25V', systemQty: 300, unit: 'PCS' },
  { binCode: 'C-01-03-05', zoneName: 'C区-连接器', shelfNo: '01号架', materialCode: 'MAT-2004', materialName: '排针 2.54mm 双排', systemQty: 150, unit: 'PCS' },
]

/** 总库位数 */
const totalBins = mockBins.length

/** 盘点进度 */
const countProgress = computed(() => {
  return Math.round((countResults.length / totalBins) * 100)
})

/** 数量一致的库位数 */
const consistentCount = computed(() => {
  return countResults.filter((r) => r.difference === 0).length
})

/** 有差异的库位数 */
const diffCount = computed(() => {
  return countResults.filter((r) => r.difference !== 0).length
})

/** 差异结果列表 */
const diffResults = computed(() => {
  return countResults.filter((r) => r.difference !== 0)
})

/** 差异值 */
const difference = computed(() => {
  if (actualQty.value === null || !currentBinInfo.value) return 0
  return actualQty.value - currentBinInfo.value.systemQty
})

/** 是否还有下一个库位 */
const hasNextBin = computed(() => {
  if (!currentBinInfo.value) return true
  const currentIndex = mockBins.findIndex((b) => b.binCode === currentBinInfo.value.binCode)
  return currentIndex < mockBins.length - 1
})

/** 扫描库位 */
const handleScanBin = () => {
  if (!binBarcode.value.trim()) {
    KhMessageFn.warning('请输入或扫描库位条码')
    return
  }

  const code = binBarcode.value.trim().toUpperCase()
  const binData = mockBins.find((b) => b.binCode === code)

  if (!binData) {
    KhMessageFn.error('未找到该库位信息，请检查条码')
    return
  }

  // 检查是否已盘点
  if (countResults.some((r) => r.binCode === code)) {
    KhMessageFn.warning('该库位已盘点，请扫描其他库位')
    return
  }

  currentBinInfo.value = { ...binData }
  actualQty.value = null
  diffRemark.value = ''
  KhMessageFn.success('库位查询成功')
}

/** 切换步骤 */
const goToStep = (step) => {
  currentStep.value = step
}

/** 记录并下一个 */
const recordAndNext = () => {
  if (actualQty.value === null) {
    KhMessageFn.warning('请输入实盘数量')
    return
  }

  // 记录盘点结果
  countResults.push({
    binCode: currentBinInfo.value.binCode,
    materialCode: currentBinInfo.value.materialCode,
    materialName: currentBinInfo.value.materialName,
    systemQty: currentBinInfo.value.systemQty,
    actualQty: actualQty.value,
    unit: currentBinInfo.value.unit,
    difference: actualQty.value - currentBinInfo.value.systemQty,
    remark: diffRemark.value || '',
  })

  KhMessageFn.success(`${currentBinInfo.value.binCode} 盘点记录成功`)

  // 判断是否还有下一个库位
  if (hasNextBin.value) {
    // 自动加载下一个库位
    const currentIndex = mockBins.findIndex((b) => b.binCode === currentBinInfo.value.binCode)
    const nextBin = mockBins[currentIndex + 1]

    // 检查下一个是否已盘点
    if (countResults.some((r) => r.binCode === nextBin.binCode)) {
      // 查找未盘点的下一个
      const unscanned = mockBins.find((b) => !countResults.some((r) => r.binCode === b.binCode))
      if (unscanned) {
        currentBinInfo.value = { ...unscanned }
        actualQty.value = null
        diffRemark.value = ''
        binBarcode.value = unscanned.binCode
        KhMessageFn.info(`已切换至下一个库位: ${unscanned.binCode}`)
      } else {
        // 所有库位已盘点完
        goToStep(2)
        return
      }
    } else {
      currentBinInfo.value = { ...nextBin }
      actualQty.value = null
      diffRemark.value = ''
      binBarcode.value = nextBin.binCode
      KhMessageFn.info(`已切换至下一个库位: ${nextBin.binCode}`)
    }

    // 保持当前在步骤1
    currentStep.value = 1
  } else {
    // 所有库位已盘点完
    goToStep(2)
  }
}

/** 提交盘点结果 */
const handleSubmit = () => {
  submitting.value = true
  setTimeout(() => {
    submitting.value = false
    showCompleteResult.value = true
    KhMessageFn.success('盘点结果提交成功！')
  }, 1200)
}

/** 重置 */
const handleReset = () => {
  currentStep.value = 0
  binBarcode.value = ''
  currentBinInfo.value = null
  actualQty.value = null
  diffRemark.value = ''
  countResults.splice(0, countResults.length)
  showCompleteResult.value = false
  nextTick(() => {
    binInputRef.value?.focus()
  })
}

onMounted(() => {
  nextTick(() => {
    binInputRef.value?.focus()
  })
})
</script>

<style scoped>
.pda-count {
  padding-bottom: 16px;
}

/* 步骤条 */
.pda-count__steps {
  margin-bottom: 12px;
  border-radius: 8px;
  overflow: hidden;
}

.pda-count__steps :deep(.el-step__title) {
  font-size: 15px;
  font-weight: 600;
}

.pda-count__section {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

/* 卡片 */
.pda-count__card {
  border-radius: 10px;
}

.pda-count__card :deep(.el-card__header) {
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
.pda-count__form {
  margin-top: 4px;
}

.pda-count__form :deep(.el-form-item__label) {
  font-size: 15px;
  font-weight: 600;
  color: #606266;
  padding-bottom: 6px;
}

.pda-count__form :deep(.el-input__wrapper) {
  min-height: 48px;
  font-size: 16px;
}

.pda-count__form :deep(.el-textarea__inner) {
  font-size: 16px;
  min-height: 80px;
}

/* 按钮 */
.pda-count__btn-full {
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

/* 库位横幅 */
.bin-banner {
  text-align: center;
  padding: 16px 12px;
  background: linear-gradient(135deg, #409eff 0%, #66b1ff 100%);
  border-radius: 8px;
  margin-bottom: 14px;
}

.bin-banner__code {
  font-size: 28px;
  font-weight: 800;
  color: #fff;
  letter-spacing: 2px;
}

.bin-banner__zone {
  font-size: 14px;
  color: rgba(255, 255, 255, 0.85);
  margin-top: 4px;
}

/* 信息网格 */
.info-grid {
  display: flex;
  flex-direction: column;
}

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px solid #f0f0f0;
}

.info-row:last-child {
  border-bottom: none;
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

.info-value.code {
  color: #409eff;
}

/* 进度 */
.progress-text {
  text-align: center;
  font-size: 15px;
  font-weight: 600;
  color: #606266;
  margin-top: 6px;
}

/* 盘点信息横幅 */
.count-info-banner {
  text-align: center;
  padding: 12px;
  background-color: #f5f7fa;
  border-radius: 8px;
  margin-bottom: 14px;
}

.count-bin-code {
  font-size: 24px;
  font-weight: 800;
  color: #303133;
  letter-spacing: 2px;
}

.count-material-info {
  font-size: 14px;
  color: #909399;
  margin-top: 4px;
}

/* 数量对比 */
.count-compare {
  display: flex;
  align-items: center;
  justify-content: space-around;
  padding: 14px 0;
  background-color: #fafafa;
  border-radius: 8px;
  margin-bottom: 12px;
}

.compare-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.compare-label {
  font-size: 13px;
  color: #909399;
}

.compare-value {
  font-size: 20px;
  font-weight: 700;
  color: #303133;
}

.compare-value.is-diff {
  color: #f56c6c;
}

.compare-divider {
  font-size: 16px;
  font-weight: 700;
  color: #c0c4cc;
}

/* 差异提示 */
.diff-alert {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px;
  background-color: #fef0f0;
  border: 1px solid #fde2e2;
  border-radius: 8px;
  margin-bottom: 12px;
}

.diff-alert .diff-icon {
  font-size: 22px;
  color: #f56c6c;
}

.diff-alert .diff-text {
  font-size: 17px;
  font-weight: 700;
  color: #f56c6c;
}

.diff-ok {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px;
  background-color: #f0f9eb;
  border: 1px solid #e1f3d8;
  border-radius: 8px;
  margin-bottom: 12px;
}

.diff-ok .diff-icon.ok {
  font-size: 22px;
  color: #67c23a;
}

.diff-ok .diff-text.ok {
  font-size: 17px;
  font-weight: 700;
  color: #67c23a;
}

/* 数量输入 */
.qty-input {
  width: 100%;
}

.qty-input :deep(.el-input__inner) {
  font-size: 20px;
  font-weight: 700;
}

/* 汇总统计 */
.summary-stats {
  display: flex;
  gap: 10px;
  margin-bottom: 16px;
}

.stat-card {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 14px 8px;
  background-color: #f5f7fa;
  border-radius: 10px;
  border: 2px solid #e5e6eb;
}

.stat-card.ok {
  background-color: #f0f9eb;
  border-color: #e1f3d8;
}

.stat-card.error {
  background-color: #fef0f0;
  border-color: #fde2e2;
}

.stat-value {
  font-size: 28px;
  font-weight: 800;
  color: #303133;
}

.stat-card.ok .stat-value {
  color: #67c23a;
}

.stat-card.error .stat-value {
  color: #f56c6c;
}

.stat-label {
  font-size: 13px;
  color: #909399;
  margin-top: 4px;
}

/* 差异明细 */
.diff-section {
  margin-bottom: 16px;
}

.diff-section__title {
  font-size: 16px;
  font-weight: 700;
  color: #303133;
  margin-bottom: 10px;
  padding-bottom: 6px;
  border-bottom: 2px solid #f56c6c;
}

.diff-detail {
  border: 1px solid #fde2e2;
  border-radius: 8px;
  padding: 12px;
  margin-bottom: 10px;
  background-color: #fffbfb;
}

.diff-detail__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.diff-detail__bin {
  font-size: 16px;
  font-weight: 700;
  color: #303133;
}

.diff-detail__body {
  display: flex;
  flex-direction: column;
}

.diff-detail__row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 4px 0;
}

.diff-detail__label {
  font-size: 13px;
  color: #909399;
}

.diff-detail__value {
  font-size: 15px;
  font-weight: 600;
  color: #303133;
}

.diff-detail__value.diff-val {
  color: #f56c6c;
  font-size: 17px;
  font-weight: 700;
}

.diff-detail__value.remark {
  color: #e6a23c;
  font-style: italic;
}

/* 无差异 */
.no-diff {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 20px;
  color: #67c23a;
  font-size: 16px;
  font-weight: 600;
}

.no-diff-icon {
  font-size: 24px;
}

/* 结果 */
.pda-count :deep(.el-result) {
  padding: 24px 0;
}

.pda-count :deep(.el-result__title p) {
  font-size: 22px;
  font-weight: 700;
}

.pda-count :deep(.el-result__subtitle p) {
  font-size: 15px;
}
</style>
