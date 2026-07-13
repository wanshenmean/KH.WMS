<template>
  <div class="pda-sorting">
    <!-- 任务列表视图 -->
    <div v-if="!selectedTask" class="pda-sorting__section">
      <div class="page-header">
        <span class="page-title">分拣任务列表</span>
        <el-tag type="info" size="large" round>{{ taskList.length }} 个任务</el-tag>
      </div>

      <div
        v-for="task in taskList"
        :key="task.taskNo"
        class="task-card"
        @click="selectTask(task)"
      >
        <div class="task-card__header">
          <span class="task-card__no">{{ task.taskNo }}</span>
          <el-tag
            :type="task.status === 'pending' ? 'warning' : 'success'"
            size="large"
            effect="dark"
          >
            {{ task.statusText }}
          </el-tag>
        </div>
        <div class="task-card__body">
          <div class="task-card__row">
            <span class="task-card__label">出库单号</span>
            <span class="task-card__value">{{ task.orderNo }}</span>
          </div>
          <div class="task-card__row">
            <span class="task-card__label">物料总数</span>
            <span class="task-card__value">{{ task.totalItems }} 件</span>
          </div>
          <div class="task-card__row">
            <span class="task-card__label">通道数</span>
            <span class="task-card__value">{{ task.channels.length }} 个通道</span>
          </div>
          <div class="task-card__row">
            <span class="task-card__label">已分拣</span>
            <span class="task-card__value highlight">{{ task.sortedCount }}/{{ task.totalItems }}</span>
          </div>
        </div>
        <div class="task-card__footer">
          <el-button type="primary" size="large" class="task-card__btn">
            开始分拣
          </el-button>
        </div>
      </div>
    </div>

    <!-- 分拣操作视图 -->
    <div v-else class="pda-sorting__section">
      <!-- 头部 -->
      <div class="detail-header">
        <el-button
          size="large"
          class="back-btn"
          @click="backToList"
        >
          <el-icon><component :is="ArrowLeftIcon" /></el-icon>
          返回
        </el-button>
        <span class="detail-title">{{ selectedTask.taskNo }}</span>
      </div>

      <!-- 进度 -->
      <div class="progress-section">
        <el-progress
          :percentage="sortingProgress"
          :stroke-width="18"
          :text-inside="true"
          status=""
          :color="progressColor"
        />
        <div class="progress-text">
          已分拣 {{ selectedTask.sortedCount }}/{{ selectedTask.totalItems }} 件
        </div>
      </div>

      <!-- 扫描物料 -->
      <el-card shadow="never" class="pda-sorting__card">
        <template #header>
          <span class="card-title">扫描物料条码</span>
        </template>
        <el-form label-position="top" class="pda-sorting__form">
          <el-form-item label="物料条码">
            <el-input
              v-model="scanBarcode"
              ref="barcodeInputRef"
              placeholder="请扫描物料条码"
              size="large"
              clearable
              @keyup.enter="handleScanMaterial"
            />
          </el-form-item>
        </el-form>

        <!-- 物料信息展示 -->
        <div v-if="scannedMaterial" class="scanned-material">
          <div class="scanned-material__code">{{ scannedMaterial.materialCode }}</div>
          <div class="scanned-material__name">{{ scannedMaterial.materialName }}</div>
          <div class="scanned-material__qty">待分拣: {{ scannedMaterial.quantity }} {{ scannedMaterial.unit }}</div>
        </div>
      </el-card>

      <!-- 通道选择 -->
      <el-card v-if="scannedMaterial" shadow="never" class="pda-sorting__card">
        <template #header>
          <span class="card-title">选择分拣通道</span>
        </template>
        <div class="channel-grid">
          <div
            v-for="channel in selectedTask.channels"
            :key="channel.id"
            class="channel-btn"
            :class="{ 'is-selected': selectedChannel === channel.id }"
            @click="selectedChannel = channel.id"
          >
            <div class="channel-btn__id">{{ channel.id }}</div>
            <div class="channel-btn__count">{{ getChannelCount(channel.id) }} 件</div>
          </div>
        </div>
        <el-button
          type="primary"
          size="large"
          class="pda-sorting__btn-full"
          :disabled="!selectedChannel"
          @click="confirmSort"
        >
          <el-icon class="btn-icon"><component :is="CheckIcon" /></el-icon>
          确认分拣至 {{ selectedChannel || '--' }}
        </el-button>
      </el-card>

      <!-- 通道分拣统计 -->
      <el-card shadow="never" class="pda-sorting__card">
        <template #header>
          <span class="card-title">通道统计</span>
        </template>
        <div class="channel-stats">
          <div
            v-for="channel in selectedTask.channels"
            :key="channel.id"
            class="channel-stat"
          >
            <span class="channel-stat__name">通道 {{ channel.id }}</span>
            <div class="channel-stat__bar-wrap">
              <div
                class="channel-stat__bar"
                :style="{ width: getChannelPercent(channel.id) + '%' }"
              ></div>
            </div>
            <span class="channel-stat__count">{{ getChannelCount(channel.id) }} 件</span>
          </div>
        </div>
      </el-card>

      <!-- 完成按钮 -->
      <el-button
        v-if="selectedTask.sortedCount >= selectedTask.totalItems"
        type="success"
        size="large"
        class="pda-sorting__btn-full"
        :loading="completing"
        @click="completeSorting"
      >
        <el-icon class="btn-icon"><component :is="FinishedIcon" /></el-icon>
        完成分拣任务
      </el-button>
    </div>

    <!-- 完成结果 -->
    <el-result
      v-if="showCompleteResult"
      icon="success"
      title="分拣完成"
      :sub-title="`${selectedTask.taskNo} 分拣任务已完成`"
    >
      <template #extra>
        <div class="result-summary">
          <div
            v-for="channel in selectedTask.channels"
            :key="channel.id"
            class="result-channel"
          >
            <span class="result-channel__name">通道 {{ channel.id }}</span>
            <span class="result-channel__count">{{ getChannelCount(channel.id) }} 件</span>
          </div>
        </div>
        <el-button
          type="primary"
          size="large"
          class="pda-sorting__btn-full"
          style="margin-top: 16px"
          @click="backToList"
        >
          返回任务列表
        </el-button>
      </template>
    </el-result>
  </div>
</template>

<script setup>

const CheckIcon = markRaw(Check)
const ArrowLeftIcon = markRaw(ArrowLeft)
const FinishedIcon = markRaw(Finished)

/** 当前选中的任务 */
const selectedTask = ref(null)

/** 扫码输入 */
const scanBarcode = ref('')
const barcodeInputRef = ref(null)

/** 选中的通道 */
const selectedChannel = ref('')

/** 扫描到的物料 */
const scannedMaterial = ref(null)

/** 完成状态 */
const completing = ref(false)
const showCompleteResult = ref(false)

/** 模拟分拣任务 */
const taskList = reactive([
  {
    taskNo: 'SJ-2026-04-09-001',
    orderNo: 'SO-20260409-003',
    status: 'pending',
    statusText: '待分拣',
    totalItems: 12,
    sortedCount: 0,
    channels: [
      { id: 'A1', count: 0 },
      { id: 'A2', count: 0 },
      { id: 'A3', count: 0 },
      { id: 'A4', count: 0 },
      { id: 'A5', count: 0 },
      { id: 'A6', count: 0 },
    ],
  },
  {
    taskNo: 'SJ-2026-04-09-002',
    orderNo: 'SO-20260409-005',
    status: 'pending',
    statusText: '待分拣',
    totalItems: 8,
    sortedCount: 0,
    channels: [
      { id: 'A1', count: 0 },
      { id: 'A2', count: 0 },
      { id: 'A3', count: 0 },
      { id: 'A4', count: 0 },
      { id: 'A5', count: 0 },
      { id: 'A6', count: 0 },
    ],
  },
])

/** 模拟物料数据映射 */
const materialMap = {
  'MAT-2001': { materialCode: 'MAT-2001', materialName: 'MCU STM32F103C8T6', quantity: 3, unit: 'PCS' },
  'MAT-2002': { materialCode: 'MAT-2002', materialName: '晶振 8MHz HC49S', quantity: 5, unit: 'PCS' },
  'MAT-2003': { materialCode: 'MAT-2003', materialName: '电解电容 470uF 25V', quantity: 4, unit: 'PCS' },
  'MAT-2004': { materialCode: 'MAT-2004', materialName: '排针 2.54mm 双排', quantity: 2, unit: 'PCS' },
  'MAT-3001': { materialCode: 'MAT-3001', materialName: '电阻 1/4W 4.7KΩ', quantity: 6, unit: 'PCS' },
}

/** 分拣进度 */
const sortingProgress = computed(() => {
  if (!selectedTask.value) return 0
  return Math.round((selectedTask.value.sortedCount / selectedTask.value.totalItems) * 100)
})

const progressColor = computed(() => {
  const p = sortingProgress.value
  if (p === 100) return '#67c23a'
  if (p >= 50) return '#409eff'
  return '#e6a23c'
})

/** 获取通道已分拣数量 */
const getChannelCount = (channelId) => {
  if (!selectedTask.value) return 0
  const channel = selectedTask.value.channels.find((c) => c.id === channelId)
  return channel ? channel.count : 0
}

/** 获取通道分拣百分比 */
const getChannelPercent = (channelId) => {
  if (!selectedTask.value || selectedTask.value.totalItems === 0) return 0
  return Math.round((getChannelCount(channelId) / selectedTask.value.totalItems) * 100)
}

/** 选择任务 */
const selectTask = (task) => {
  selectedTask.value = task
  showCompleteResult.value = false
  scanBarcode.value = ''
  scannedMaterial.value = null
  selectedChannel.value = ''
  nextTick(() => {
    barcodeInputRef.value?.focus()
  })
}

/** 返回列表 */
const backToList = () => {
  selectedTask.value = null
  showCompleteResult.value = false
}

/** 扫描物料 */
const handleScanMaterial = () => {
  if (!scanBarcode.value.trim()) {
    KhMessageFn.warning('请扫描物料条码')
    return
  }

  const code = scanBarcode.value.trim().toUpperCase()
  const material = materialMap[code]

  if (!material) {
    // 未知物料，模拟返回一个通用物料
    scannedMaterial.value = {
      materialCode: code,
      materialName: '未知物料',
      quantity: 1,
      unit: 'PCS',
    }
    KhMessageFn.warning('物料未在清单中找到，请确认条码')
    return
  }

  scannedMaterial.value = { ...material }
  selectedChannel.value = ''
  KhMessageFn.success('物料识别成功，请选择分拣通道')
}

/** 确认分拣 */
const confirmSort = () => {
  if (!selectedChannel.value) {
    KhMessageFn.warning('请选择分拣通道')
    return
  }

  const channel = selectedTask.value.channels.find((c) => c.id === selectedChannel.value)
  if (channel) {
    channel.count += scannedMaterial.value.quantity
    selectedTask.value.sortedCount += scannedMaterial.value.quantity
  }

  KhMessageFn.success(`已分拣 ${scannedMaterial.value.quantity} 件至通道 ${selectedChannel.value}`)

  // 重置扫码区域
  scanBarcode.value = ''
  scannedMaterial.value = null
  selectedChannel.value = ''
  nextTick(() => {
    barcodeInputRef.value?.focus()
  })
}

/** 完成分拣 */
const completeSorting = () => {
  completing.value = true
  setTimeout(() => {
    completing.value = false
    showCompleteResult.value = true
    if (selectedTask.value) {
      selectedTask.value.status = 'done'
      selectedTask.value.statusText = '已完成'
    }
    KhMessageFn.success('分拣任务已完成！')
  }, 1200)
}

onMounted(() => {
  nextTick(() => {
    barcodeInputRef.value?.focus()
  })
})
</script>

<style scoped>
.pda-sorting {
  padding-bottom: 16px;
}

.pda-sorting__section {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

/* 页头 */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 4px 0;
}

.page-title {
  font-size: 19px;
  font-weight: 700;
  color: #303133;
}

/* 任务卡片 */
.task-card {
  background: #fff;
  border-radius: 10px;
  padding: 14px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
  border: 1px solid #e5e6eb;
  cursor: pointer;
  transition: all 0.2s;
  -webkit-tap-highlight-color: transparent;
}

.task-card:active {
  transform: scale(0.98);
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.1);
}

.task-card__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.task-card__no {
  font-size: 17px;
  font-weight: 700;
  color: #303133;
}

.task-card__body {
  margin-bottom: 10px;
}

.task-card__row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 5px 0;
}

.task-card__label {
  font-size: 14px;
  color: #909399;
}

.task-card__value {
  font-size: 15px;
  font-weight: 600;
  color: #303133;
}

.task-card__value.highlight {
  color: #409eff;
  font-size: 17px;
}

.task-card__footer {
  padding-top: 8px;
  border-top: 1px solid #f0f0f0;
}

.task-card__btn {
  width: 100%;
  min-height: 44px;
  font-size: 16px;
  font-weight: 600;
  border-radius: 10px;
}

/* 详情头部 */
.detail-header {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 4px 0;
}

.back-btn {
  min-height: 44px;
  font-size: 15px;
  font-weight: 600;
  border-radius: 10px;
}

.detail-title {
  font-size: 17px;
  font-weight: 700;
  color: #303133;
}

/* 进度 */
.progress-section {
  padding: 8px 0;
}

.progress-text {
  text-align: center;
  font-size: 15px;
  font-weight: 600;
  color: #606266;
  margin-top: 6px;
}

/* 卡片 */
.pda-sorting__card {
  border-radius: 10px;
}

.pda-sorting__card :deep(.el-card__header) {
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
.pda-sorting__form {
  margin-top: 4px;
}

.pda-sorting__form :deep(.el-form-item__label) {
  font-size: 15px;
  font-weight: 600;
  color: #606266;
  padding-bottom: 6px;
}

.pda-sorting__form :deep(.el-input__wrapper) {
  min-height: 48px;
  font-size: 16px;
}

/* 扫描到的物料信息 */
.scanned-material {
  text-align: center;
  padding: 14px;
  background: linear-gradient(135deg, #e6f7ff 0%, #bae7ff 100%);
  border-radius: 8px;
  margin-top: 8px;
}

.scanned-material__code {
  font-size: 20px;
  font-weight: 800;
  color: #1890ff;
}

.scanned-material__name {
  font-size: 15px;
  color: #595959;
  margin-top: 4px;
}

.scanned-material__qty {
  font-size: 14px;
  color: #8c8c8c;
  margin-top: 2px;
}

/* 通道选择网格 */
.channel-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
  margin-bottom: 12px;
}

.channel-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 14px 8px;
  border: 2px solid #e5e6eb;
  border-radius: 10px;
  background: #fff;
  cursor: pointer;
  transition: all 0.2s;
  -webkit-tap-highlight-color: transparent;
  user-select: none;
}

.channel-btn:active {
  transform: scale(0.96);
}

.channel-btn.is-selected {
  border-color: #409eff;
  background-color: #ecf5ff;
  box-shadow: 0 0 0 2px rgba(64, 158, 255, 0.25);
}

.channel-btn__id {
  font-size: 22px;
  font-weight: 800;
  color: #303133;
}

.channel-btn.is-selected .channel-btn__id {
  color: #409eff;
}

.channel-btn__count {
  font-size: 13px;
  color: #909399;
  margin-top: 4px;
}

/* 全宽按钮 */
.pda-sorting__btn-full {
  width: 100%;
  min-height: 48px;
  font-size: 17px;
  font-weight: 600;
  border-radius: 10px;
}

.btn-icon {
  margin-right: 6px;
  font-size: 20px;
}

/* 通道统计 */
.channel-stats {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.channel-stat {
  display: flex;
  align-items: center;
  gap: 10px;
}

.channel-stat__name {
  font-size: 14px;
  font-weight: 600;
  color: #606266;
  min-width: 70px;
}

.channel-stat__bar-wrap {
  flex: 1;
  height: 16px;
  background-color: #f0f0f0;
  border-radius: 8px;
  overflow: hidden;
}

.channel-stat__bar {
  height: 100%;
  background: linear-gradient(90deg, #409eff 0%, #66b1ff 100%);
  border-radius: 8px;
  transition: width 0.4s ease;
  min-width: 2px;
}

.channel-stat__count {
  font-size: 15px;
  font-weight: 700;
  color: #303133;
  min-width: 40px;
  text-align: right;
}

/* 结果摘要 */
.result-summary {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
  padding: 0 16px;
}

.result-channel {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 12px 8px;
  background: #f0f9eb;
  border-radius: 8px;
}

.result-channel__name {
  font-size: 14px;
  color: #67c23a;
  font-weight: 600;
}

.result-channel__count {
  font-size: 20px;
  font-weight: 800;
  color: #303133;
  margin-top: 4px;
}

/* 结果 */
.pda-sorting :deep(.el-result) {
  padding: 24px 0;
}

.pda-sorting :deep(.el-result__title p) {
  font-size: 22px;
  font-weight: 700;
}

.pda-sorting :deep(.el-result__subtitle p) {
  font-size: 15px;
}
</style>
