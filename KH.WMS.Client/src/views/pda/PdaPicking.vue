<template>
  <div class="pda-picking">
    <!-- 任务列表视图 -->
    <div v-if="!selectedTask" class="pda-picking__section">
      <div class="page-header">
        <span class="page-title">拣货任务列表</span>
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
            <span class="task-card__label">物料行数</span>
            <span class="task-card__value">{{ task.lines.length }} 行</span>
          </div>
          <div class="task-card__row">
            <span class="task-card__label">优先级</span>
            <el-rate
              v-model="task.priority"
              disabled
              :colors="priorityColors"
              size="small"
            />
          </div>
        </div>
        <div class="task-card__footer">
          <el-button type="primary" size="large" class="task-card__btn">
            开始拣货
          </el-button>
        </div>
      </div>
    </div>

    <!-- 拣货详情视图 -->
    <div v-else class="pda-picking__section">
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

      <!-- 进度条 -->
      <div class="progress-section">
        <el-progress
          :percentage="pickingProgress"
          :stroke-width="18"
          :text-inside="true"
          status=""
          :color="progressColor"
        />
        <div class="progress-text">
          已拣 {{ pickedCount }}/{{ selectedTask.lines.length }} 行
        </div>
      </div>

      <!-- 拣货行列表 -->
      <div
        v-for="(line, index) in selectedTask.lines"
        :key="index"
        class="pick-line"
        :class="{ 'is-done': line.picked, 'is-current': !line.picked && !previousLinesDone(index) }"
      >
        <div class="pick-line__header">
          <span class="pick-line__index">第 {{ index + 1 }} 行</span>
          <el-tag
            v-if="line.picked"
            type="success"
            size="large"
            effect="dark"
          >
            已拣
          </el-tag>
          <el-tag
            v-else-if="previousLinesDone(index)"
            type="warning"
            size="large"
            effect="dark"
          >
            待拣
          </el-tag>
          <el-tag
            v-else
            type="info"
            size="large"
          >
            排队中
          </el-tag>
        </div>

        <div class="pick-line__info">
          <div class="pick-line__row">
            <span class="pick-line__label">物料编码</span>
            <span class="pick-line__value code">{{ line.materialCode }}</span>
          </div>
          <div class="pick-line__row">
            <span class="pick-line__label">物料名称</span>
            <span class="pick-line__value">{{ line.materialName }}</span>
          </div>
          <div class="pick-line__row">
            <span class="pick-line__label">库位</span>
            <span class="pick-line__value bin">{{ line.binLocation }}</span>
          </div>
          <div class="pick-line__row">
            <span class="pick-line__label">拣货数量</span>
            <span class="pick-line__value qty">{{ line.quantity }} {{ line.unit }}</span>
          </div>
        </div>

        <!-- 拣货操作区（当前行可操作） -->
        <div v-if="previousLinesDone(index) && !line.picked" class="pick-line__action">
          <el-form label-position="top" class="pick-form">
            <el-form-item label="扫描库位条码">
              <el-input
                v-model="line.scanBin"
                placeholder="请扫描库位条码"
                size="large"
                clearable
                @keyup.enter="focusQtyInput(index)"
              />
            </el-form-item>
            <el-form-item label="实拣数量">
              <el-input-number
                v-model="line.actualQty"
                :min="0"
                :max="line.quantity * 2"
                size="large"
                controls-position="right"
                class="qty-input"
              />
            </el-form-item>
            <el-button
              type="primary"
              size="large"
              class="pick-line__confirm-btn"
              @click="confirmPickLine(index)"
            >
              <el-icon class="btn-icon"><component :is="CheckIcon" /></el-icon>
              确认拣货
            </el-button>
          </el-form>
        </div>

        <!-- 已拣完成显示 -->
        <div v-if="line.picked" class="pick-line__done-info">
          <el-icon class="done-icon"><component :is="CircleCheckIcon" /></el-icon>
          <span>实拣 {{ line.actualQty }} {{ line.unit }}</span>
        </div>
      </div>

      <!-- 完成按钮 -->
      <el-button
        v-if="pickedCount === selectedTask.lines.length"
        type="success"
        size="large"
        class="pda-picking__btn-full"
        :loading="completing"
        @click="completePicking"
      >
        <el-icon class="btn-icon"><component :is="FinishedIcon" /></el-icon>
        完成拣货任务
      </el-button>
    </div>

    <!-- 完成结果 -->
    <el-result
      v-if="showCompleteResult"
      icon="success"
      title="拣货完成"
      :sub-title="`${selectedTask.taskNo} 已全部拣货完成`"
    >
      <template #extra>
        <el-button
          type="primary"
          size="large"
          class="pda-picking__btn-full"
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
const CircleCheckIcon = markRaw(CircleCheck)
const FinishedIcon = markRaw(Finished)

const priorityColors = ['#F7BA2A', '#E6A23C', '#F56C6C']

/** 当前选中的任务 */
const selectedTask = ref(null)

/** 完成状态 */
const completing = ref(false)
const showCompleteResult = ref(false)

/** 模拟拣货任务列表 */
const taskList = reactive([
  {
    taskNo: 'PK-2026-04-09-001',
    orderNo: 'SO-20260409-003',
    status: 'pending',
    statusText: '待拣货',
    priority: 3,
    lines: [
      {
        materialCode: 'MAT-2001',
        materialName: 'MCU STM32F103C8T6',
        binLocation: 'A-01-02-03',
        quantity: 200,
        actualQty: 200,
        unit: 'PCS',
        picked: false,
        scanBin: '',
      },
      {
        materialCode: 'MAT-2002',
        materialName: '晶振 8MHz HC49S',
        binLocation: 'B-03-01-02',
        quantity: 500,
        actualQty: 500,
        unit: 'PCS',
        picked: false,
        scanBin: '',
      },
      {
        materialCode: 'MAT-2003',
        materialName: '电解电容 470uF 25V',
        binLocation: 'A-02-04-01',
        quantity: 300,
        actualQty: 300,
        unit: 'PCS',
        picked: false,
        scanBin: '',
      },
      {
        materialCode: 'MAT-2004',
        materialName: '排针 2.54mm 双排',
        binLocation: 'C-01-03-05',
        quantity: 150,
        actualQty: 150,
        unit: 'PCS',
        picked: false,
        scanBin: '',
      },
    ],
  },
  {
    taskNo: 'PK-2026-04-09-002',
    orderNo: 'SO-20260409-005',
    status: 'pending',
    statusText: '待拣货',
    priority: 2,
    lines: [
      {
        materialCode: 'MAT-3001',
        materialName: '电阻 1/4W 4.7KΩ',
        binLocation: 'A-01-01-01',
        quantity: 1000,
        actualQty: 1000,
        unit: 'PCS',
        picked: false,
        scanBin: '',
      },
    ],
  },
  {
    taskNo: 'PK-2026-04-09-003',
    orderNo: 'SO-20260409-007',
    status: 'pending',
    statusText: '待拣货',
    priority: 1,
    lines: [
      {
        materialCode: 'MAT-4001',
        materialName: 'PCB空白板 FR4 双面',
        binLocation: 'D-01-01-01',
        quantity: 50,
        actualQty: 50,
        unit: 'PCS',
        picked: false,
        scanBin: '',
      },
      {
        materialCode: 'MAT-4002',
        materialName: '锡膏 SAC305 500g',
        binLocation: 'E-02-01-01',
        quantity: 10,
        actualQty: 10,
        unit: '罐',
        picked: false,
        scanBin: '',
      },
    ],
  },
])

/** 已拣数量 */
const pickedCount = computed(() => {
  if (!selectedTask.value) return 0
  return selectedTask.value.lines.filter((l) => l.picked).length
})

/** 拣货进度百分比 */
const pickingProgress = computed(() => {
  if (!selectedTask.value) return 0
  return Math.round((pickedCount.value / selectedTask.value.lines.length) * 100)
})

/** 进度条颜色 */
const progressColor = computed(() => {
  const p = pickingProgress.value
  if (p === 100) return '#67c23a'
  if (p >= 50) return '#409eff'
  return '#e6a23c'
})

/** 判断前面的行是否都已拣完 */
const previousLinesDone = (index) => {
  if (!selectedTask.value) return false
  return selectedTask.value.lines.slice(0, index).every((l) => l.picked)
}

/** 选择任务 */
const selectTask = (task) => {
  selectedTask.value = task
  showCompleteResult.value = false
}

/** 返回列表 */
const backToList = () => {
  selectedTask.value = null
  showCompleteResult.value = false
}

/** 聚焦数量输入 */
const focusQtyInput = (index) => {
  // 验证库位扫码
  const line = selectedTask.value.lines[index]
  if (line.scanBin && line.scanBin !== line.binLocation) {
    KhMessageFn.warning('库位条码不匹配，请重新扫描')
    line.scanBin = ''
    return
  }
  KhMessageFn.success('库位验证通过')
}

/** 确认拣货行 */
const confirmPickLine = (index) => {
  const line = selectedTask.value.lines[index]

  if (!line.scanBin) {
    KhMessageFn.warning('请先扫描库位条码')
    return
  }

  if (line.scanBin !== line.binLocation) {
    KhMessageFn.error('库位条码不匹配')
    return
  }

  if (!line.actualQty || line.actualQty <= 0) {
    KhMessageFn.warning('请输入实拣数量')
    return
  }

  line.picked = true
  KhMessageFn.success(`第 ${index + 1} 行拣货成功`)
}

/** 完成拣货 */
const completePicking = () => {
  completing.value = true
  setTimeout(() => {
    completing.value = false
    showCompleteResult.value = true
    if (selectedTask.value) {
      selectedTask.value.status = 'done'
      selectedTask.value.statusText = '已完成'
    }
    KhMessageFn.success('拣货任务已完成！')
  }, 1200)
}
</script>

<style scoped>
.pda-picking {
  padding-bottom: 16px;
}

.pda-picking__section {
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
  padding: 12px 0;
}

.progress-text {
  text-align: center;
  font-size: 15px;
  font-weight: 600;
  color: #606266;
  margin-top: 8px;
}

/* 拣货行 */
.pick-line {
  background: #fff;
  border-radius: 10px;
  border: 2px solid #e5e6eb;
  padding: 14px;
  margin-bottom: 10px;
  transition: all 0.2s;
}

.pick-line.is-done {
  border-color: #67c23a;
  background-color: #f0f9eb;
}

.pick-line.is-current {
  border-color: #409eff;
  box-shadow: 0 0 0 1px rgba(64, 158, 255, 0.3);
}

.pick-line__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.pick-line__index {
  font-size: 15px;
  font-weight: 700;
  color: #303133;
}

.pick-line__info {
  margin-bottom: 4px;
}

.pick-line__row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 4px 0;
}

.pick-line__label {
  font-size: 13px;
  color: #909399;
}

.pick-line__value {
  font-size: 15px;
  font-weight: 600;
  color: #303133;
}

.pick-line__value.code {
  color: #409eff;
}

.pick-line__value.bin {
  color: #e6a23c;
  font-weight: 700;
}

.pick-line__value.qty {
  color: #f56c6c;
  font-size: 17px;
}

/* 拣货操作区 */
.pick-line__action {
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px dashed #dcdfe6;
}

.pick-form :deep(.el-form-item__label) {
  font-size: 14px;
  font-weight: 600;
  color: #606266;
  padding-bottom: 4px;
}

.pick-form :deep(.el-input__wrapper) {
  min-height: 46px;
  font-size: 16px;
}

.qty-input {
  width: 100%;
}

.qty-input :deep(.el-input__inner) {
  font-size: 18px;
  font-weight: 700;
}

.pick-line__confirm-btn {
  width: 100%;
  min-height: 48px;
  font-size: 17px;
  font-weight: 600;
  margin-top: 8px;
  border-radius: 10px;
}

.btn-icon {
  margin-right: 6px;
  font-size: 20px;
}

/* 已拣完成 */
.pick-line__done-info {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  margin-top: 8px;
  padding: 8px 0;
  font-size: 16px;
  font-weight: 600;
  color: #67c23a;
}

.done-icon {
  font-size: 22px;
}

/* 全宽按钮 */
.pda-picking__btn-full {
  width: 100%;
  min-height: 48px;
  font-size: 17px;
  font-weight: 600;
  margin-top: 12px;
  border-radius: 10px;
}

/* 结果 */
.pda-picking :deep(.el-result) {
  padding: 24px 0;
}

.pda-picking :deep(.el-result__title p) {
  font-size: 22px;
  font-weight: 700;
}

.pda-picking :deep(.el-result__subtitle p) {
  font-size: 15px;
}
</style>
