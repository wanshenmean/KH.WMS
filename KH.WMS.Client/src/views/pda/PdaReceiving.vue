<template>
  <div class="pda-receiving">
    <!-- 步骤条 -->
    <el-steps :active="currentStep" finish-status="success" simple class="pda-receiving__steps">
      <el-step title="扫码" />
      <el-step title="核对" />
      <el-step title="确认" />
    </el-steps>

    <!-- Step 1: 扫码 -->
    <div v-show="currentStep === 0" class="pda-receiving__section">
      <el-card shadow="never" class="pda-receiving__card">
        <template #header>
          <span class="card-title">扫描ASN条码</span>
        </template>
        <el-form label-position="top" class="pda-receiving__form">
          <el-form-item label="ASN单号">
            <el-input
              v-model="asnBarcode"
              ref="asnInputRef"
              placeholder="请扫描或输入ASN条码"
              size="large"
              clearable
              clearable-icon="CircleClose"
              @keyup.enter="handleScanAsn"
            />
          </el-form-item>
          <el-button
            type="primary"
            size="large"
            class="pda-receiving__btn-full"
            @click="handleScanAsn"
          >
            <el-icon class="btn-icon"><component :is="ScanIcon" /></el-icon>
            扫码查询
          </el-button>
        </el-form>
      </el-card>

      <!-- 收货信息卡片 -->
      <el-card v-if="receivingInfo" shadow="never" class="pda-receiving__card">
        <template #header>
          <span class="card-title">收货信息</span>
        </template>
        <div class="info-grid">
          <div class="info-row">
            <span class="info-label">ASN单号:</span>
            <span class="info-value">{{ receivingInfo.asnNo }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">供应商:</span>
            <span class="info-value">{{ receivingInfo.supplier }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">物料行数:</span>
            <span class="info-value">{{ receivingInfo.materials.length }} 项</span>
          </div>
          <div class="info-row">
            <span class="info-label">创建日期:</span>
            <span class="info-value">{{ receivingInfo.createDate }}</span>
          </div>
        </div>
        <el-button
          type="primary"
          size="large"
          class="pda-receiving__btn-full"
          @click="goToStep(1)"
        >
          下一步：核对物料
        </el-button>
      </el-card>
    </div>

    <!-- Step 2: 核对 -->
    <div v-show="currentStep === 1" class="pda-receiving__section">
      <el-card shadow="never" class="pda-receiving__card">
        <template #header>
          <span class="card-title">物料核对</span>
        </template>
        <div class="supplier-banner">
          <span class="supplier-label">供应商</span>
          <span class="supplier-name">{{ receivingInfo.supplier }}</span>
        </div>

        <div
          v-for="(item, index) in receivingInfo.materials"
          :key="index"
          class="material-item"
          :class="{ 'is-selected': item.checked }"
        >
          <div class="material-header">
            <el-checkbox
              v-model="item.checked"
              size="large"
              class="material-checkbox"
            >
              <span class="material-code">{{ item.materialCode }}</span>
            </el-checkbox>
          </div>
          <div class="material-detail">
            <div class="detail-row">
              <span class="detail-label">物料名称</span>
              <span class="detail-value">{{ item.materialName }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">应收数量</span>
              <span class="detail-value highlight">{{ item.expectedQty }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">实收数量</span>
              <el-input-number
                v-model="item.actualQty"
                :min="0"
                :max="item.expectedQty * 2"
                size="large"
                controls-position="right"
                class="qty-input"
              />
            </div>
          </div>
        </div>

        <el-button
          type="primary"
          size="large"
          class="pda-receiving__btn-full"
          :disabled="confirmedItems.length === 0"
          @click="goToStep(2)"
        >
          确认（已选 {{ confirmedItems.length }} 项）
        </el-button>
        <el-button
          size="large"
          class="pda-receiving__btn-full"
          @click="goToStep(0)"
        >
          返回扫码
        </el-button>
      </el-card>
    </div>

    <!-- Step 3: 确认提交 -->
    <div v-show="currentStep === 2" class="pda-receiving__section">
      <el-card shadow="never" class="pda-receiving__card">
        <template #header>
          <span class="card-title">收货确认</span>
        </template>

        <div class="summary-header">
          <span class="summary-label">ASN单号</span>
          <span class="summary-value">{{ receivingInfo.asnNo }}</span>
        </div>
        <div class="summary-header">
          <span class="summary-label">供应商</span>
          <span class="summary-value">{{ receivingInfo.supplier }}</span>
        </div>

        <div class="divider"></div>

        <div
          v-for="(item, index) in confirmedItems"
          :key="index"
          class="summary-item"
        >
          <div class="summary-item-header">
            <span class="summary-item-code">{{ item.materialCode }}</span>
            <el-tag type="success" size="large">已确认</el-tag>
          </div>
          <div class="summary-item-detail">
            {{ item.materialName }} | 实收: {{ item.actualQty }} {{ item.unit }}
          </div>
        </div>

        <div class="divider"></div>

        <el-button
          type="success"
          size="large"
          class="pda-receiving__btn-full"
          :loading="submitting"
          @click="handleSubmit"
        >
          <el-icon class="btn-icon"><component :is="CheckIcon" /></el-icon>
          提交收货
        </el-button>
        <el-button
          size="large"
          class="pda-receiving__btn-full"
          @click="goToStep(1)"
        >
          返回修改
        </el-button>
      </el-card>

      <!-- 成功提示 -->
      <el-result
        v-if="submitted"
        icon="success"
        title="收货完成"
        :sub-title="`成功接收 ${confirmedItems.length} 项物料`"
      >
        <template #extra>
          <el-button
            type="primary"
            size="large"
            class="pda-receiving__btn-full"
            @click="handleReset"
          >
            继续收货
          </el-button>
        </template>
      </el-result>
    </div>
  </div>
</template>

<script setup>

const ScanIcon = markRaw(Camera)
const CheckIcon = markRaw(Check)

/** 当前步骤 */
const currentStep = ref(0)

/** ASN条码输入 */
const asnBarcode = ref('')
const asnInputRef = ref(null)

/** 提交状态 */
const submitting = ref(false)
const submitted = ref(false)

/** 收货信息（模拟数据） */
const receivingInfo = ref(null)

const mockReceivingData = {
  asnNo: 'ASN-2026-04-0901',
  supplier: '深圳市鑫达电子有限公司',
  createDate: '2026-04-09',
  materials: [
    {
      materialCode: 'MAT-1001',
      materialName: '贴片电阻 0603 10KΩ',
      expectedQty: 5000,
      actualQty: 5000,
      unit: 'PCS',
      checked: true,
    },
    {
      materialCode: 'MAT-1002',
      materialName: '陶瓷电容 0805 100nF',
      expectedQty: 3000,
      actualQty: 3000,
      unit: 'PCS',
      checked: true,
    },
    {
      materialCode: 'MAT-1003',
      materialName: 'LED灯珠 白光 3528',
      expectedQty: 2000,
      actualQty: 2000,
      unit: 'PCS',
      checked: true,
    },
  ],
}

/** 已确认的物料行 */
const confirmedItems = computed(() => {
  if (!receivingInfo.value) return []
  return receivingInfo.value.materials.filter((m) => m.checked)
})

/** 扫描ASN条码 */
const handleScanAsn = () => {
  if (!asnBarcode.value.trim()) {
    KhMessageFn.warning('请输入或扫描ASN条码')
    return
  }

  // 模拟查询延迟
  setTimeout(() => {
    receivingInfo.value = reactive(JSON.parse(JSON.stringify(mockReceivingData)))
    receivingInfo.value.asnNo = asnBarcode.value.trim().toUpperCase()
    KhMessageFn.success('查询成功，请核对物料信息')
  }, 300)
}

/** 切换步骤 */
const goToStep = (step) => {
  if (step === 1 && confirmedItems.value.length === 0) {
    KhMessageFn.warning('请至少勾选一项物料')
    return
  }
  currentStep.value = step
}

/** 提交收货 */
const handleSubmit = () => {
  submitting.value = true
  setTimeout(() => {
    submitting.value = false
    submitted.value = true
    KhMessageFn.success('收货提交成功！')
  }, 1200)
}

/** 重置状态，继续下一单收货 */
const handleReset = () => {
  currentStep.value = 0
  asnBarcode.value = ''
  receivingInfo.value = null
  submitted.value = false
  nextTick(() => {
    asnInputRef.value?.focus()
  })
}

onMounted(() => {
  nextTick(() => {
    asnInputRef.value?.focus()
  })
})
</script>

<style scoped>
.pda-receiving {
  padding-bottom: 16px;
}

/* 步骤条 */
.pda-receiving__steps {
  margin-bottom: 12px;
  border-radius: 8px;
  overflow: hidden;
}

.pda-receiving__steps :deep(.el-step__title) {
  font-size: 15px;
  font-weight: 600;
}

/* 卡片 */
.pda-receiving__card {
  margin-bottom: 12px;
  border-radius: 10px;
}

.pda-receiving__card :deep(.el-card__header) {
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
.pda-receiving__form {
  margin-top: 4px;
}

.pda-receiving__form :deep(.el-form-item__label) {
  font-size: 15px;
  font-weight: 600;
  color: #606266;
  padding-bottom: 6px;
}

.pda-receiving__form :deep(.el-input__wrapper) {
  min-height: 48px;
  font-size: 16px;
}

/* 按钮 */
.pda-receiving__btn-full {
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

/* 信息网格 */
.info-grid {
  display: flex;
  flex-direction: column;
  gap: 10px;
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

/* 供应商横幅 */
.supplier-banner {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 12px;
  background: linear-gradient(135deg, #409eff 0%, #66b1ff 100%);
  border-radius: 8px;
  margin-bottom: 16px;
}

.supplier-label {
  font-size: 13px;
  color: rgba(255, 255, 255, 0.85);
}

.supplier-name {
  font-size: 18px;
  font-weight: 700;
  color: #fff;
  margin-top: 4px;
}

/* 物料行 */
.material-item {
  border: 2px solid #e5e6eb;
  border-radius: 10px;
  margin-bottom: 12px;
  overflow: hidden;
  transition: border-color 0.2s;
}

.material-item.is-selected {
  border-color: #409eff;
  background-color: #f0f7ff;
}

.material-header {
  padding: 12px 12px 4px;
}

.material-checkbox {
  display: flex;
  align-items: center;
}

.material-checkbox :deep(.el-checkbox__label) {
  font-size: 16px;
  font-weight: 700;
  color: #303133;
}

.material-code {
  font-size: 16px;
  font-weight: 700;
  color: #409eff;
}

.material-detail {
  padding: 4px 12px 12px;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 6px 0;
}

.detail-label {
  font-size: 14px;
  color: #909399;
}

.detail-value {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}

.detail-value.highlight {
  color: #e6a23c;
}

.qty-input {
  width: 140px;
}

.qty-input :deep(.el-input__inner) {
  font-size: 18px;
  font-weight: 700;
}

/* 确认摘要 */
.summary-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
}

.summary-label {
  font-size: 14px;
  color: #909399;
}

.summary-value {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}

.divider {
  height: 1px;
  background-color: #e5e6eb;
  margin: 12px 0;
}

.summary-item {
  padding: 10px 0;
  border-bottom: 1px solid #f0f0f0;
}

.summary-item:last-of-type {
  border-bottom: none;
}

.summary-item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.summary-item-code {
  font-size: 16px;
  font-weight: 700;
  color: #303133;
}

.summary-item-detail {
  font-size: 14px;
  color: #606266;
}

/* 结果 */
.pda-receiving :deep(.el-result) {
  padding: 24px 0;
}

.pda-receiving :deep(.el-result__title p) {
  font-size: 22px;
  font-weight: 700;
}

.pda-receiving :deep(.el-result__subtitle p) {
  font-size: 15px;
}
</style>
