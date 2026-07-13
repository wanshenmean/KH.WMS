<template>
  <div class="crane-monitor">
    <!-- 页面标题 -->
    <div class="monitor-header">
      <div class="monitor-header-left">
        <span class="monitor-header-dot is-online"></span>
        <h2 class="monitor-header-title">堆垛机实时监控</h2>
      </div>
      <div class="monitor-header-right">
        <span class="monitor-header-time">更新时间: {{ lastUpdateTime }}</span>
        <el-tag :type="allOnline ? 'success' : 'danger'" effect="dark" size="small">
          {{ allOnline ? '全部在线' : '存在故障' }}
        </el-tag>
      </div>
    </div>

    <!-- 堆垛机状态卡片 -->
    <el-row :gutter="16" class="crane-cards">
      <el-col :span="6" v-for="(crane, idx) in craneList" :key="idx">
        <div class="crane-card" :class="[`crane-card--${crane.statusType}`]">
          <div class="crane-card__header">
            <span class="crane-card__id">{{ crane.id }}</span>
            <el-tag :type="crane.statusType" effect="dark" size="small">{{ crane.status }}</el-tag>
          </div>
          <div class="crane-card__body">
            <div class="crane-card__info-row">
              <span class="crane-card__label">当前位置</span>
              <span class="crane-card__value">{{ crane.position }}</span>
            </div>
            <div class="crane-card__info-row">
              <span class="crane-card__label">当前任务</span>
              <span class="crane-card__value">{{ crane.currentTask }}</span>
            </div>
            <div class="crane-card__info-row">
              <span class="crane-card__label">运行速度</span>
              <span class="crane-card__value">{{ crane.speed }} m/min</span>
            </div>
            <div class="crane-card__info-row">
              <span class="crane-card__label">当前载重</span>
              <span class="crane-card__value">{{ crane.load }} kg</span>
            </div>
          </div>
          <!-- 运行动画指示器 -->
          <div v-if="crane.status === '运行中'" class="crane-card__indicator">
            <span class="crane-card__pulse"></span>
            <span class="crane-card__pulse"></span>
            <span class="crane-card__pulse"></span>
          </div>
        </div>
      </el-col>
    </el-row>

    <!-- 任务队列 -->
    <div class="crane-queue">
      <div class="crane-queue__header">
        <h3 class="crane-queue__title">任务队列</h3>
        <el-tag type="info" effect="plain" size="small">共 {{ taskQueue.length }} 条待执行</el-tag>
      </div>
      <el-table :data="taskQueue" border stripe size="small" style="width: 100%" max-height="320">
        <el-table-column type="index" label="序号" width="60" align="center" />
        <el-table-column prop="taskNo" label="任务编号" width="170" />
        <el-table-column prop="craneId" label="分配设备" width="120" align="center" />
        <el-table-column prop="targetLocation" label="目标库位" width="140" />
        <el-table-column prop="materialCode" label="物料编码" width="120" />
        <el-table-column prop="status" label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.statusType" effect="light" size="small">{{ row.status }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="priority" label="优先级" width="80" align="center">
          <template #default="{ row }">
            <el-tag :type="row.priorityType" effect="light" size="small">{{ row.priority }}</el-tag>
          </template>
        </el-table-column>
      </el-table>
    </div>
  </div>
</template>

<script setup>

// ============================================================
//  堆垛机模拟数据
// ============================================================
const craneList = ref([
  {
    id: '堆垛机#1',
    status: '运行中',
    statusType: 'success',
    position: '3排-12列-5层',
    currentTask: 'TASK-IN-001',
    speed: 120,
    load: 850,
  },
  {
    id: '堆垛机#2',
    status: '空闲',
    statusType: 'info',
    position: '1排-8列-3层',
    currentTask: '等待分配',
    speed: 0,
    load: 0,
  },
  {
    id: '堆垛机#3',
    status: '故障',
    statusType: 'danger',
    position: '2排-5列-7层',
    currentTask: 'TASK-OUT-015',
    speed: 0,
    load: 320,
  },
  {
    id: '堆垛机#4',
    status: '维护',
    statusType: 'warning',
    position: '4排-1列-1层',
    currentTask: '计划维护',
    speed: 0,
    load: 0,
  },
])

// ============================================================
//  任务队列
// ============================================================
const taskQueue = ref([
  { taskNo: 'TASK-IN-001', craneId: '堆垛机#1', targetLocation: 'A-03-12-05', materialCode: 'M-001', status: '执行中', statusType: 'warning', priority: '高', priorityType: 'danger' },
  { taskNo: 'TASK-IN-002', craneId: '堆垛机#2', targetLocation: 'B-01-08-03', materialCode: 'M-005', status: '待执行', statusType: 'info', priority: '中', priorityType: 'warning' },
  { taskNo: 'TASK-OUT-015', craneId: '堆垛机#3', targetLocation: 'C-02-05-07', materialCode: 'M-102', status: '异常', statusType: 'danger', priority: '高', priorityType: 'danger' },
  { taskNo: 'TASK-IN-003', craneId: '--', targetLocation: 'A-01-04-02', materialCode: 'M-003', status: '排队中', statusType: 'info', priority: '低', priorityType: 'info' },
  { taskNo: 'TASK-OUT-016', craneId: '--', targetLocation: 'B-03-10-04', materialCode: 'M-108', status: '排队中', statusType: 'info', priority: '中', priorityType: 'warning' },
  { taskNo: 'TASK-IN-004', craneId: '--', targetLocation: 'D-01-02-01', materialCode: 'M-009', status: '排队中', statusType: 'info', priority: '低', priorityType: 'info' },
  { taskNo: 'TASK-OUT-017', craneId: '--', targetLocation: 'A-02-06-03', materialCode: 'M-106', status: '排队中', statusType: 'info', priority: '高', priorityType: 'danger' },
  { taskNo: 'TASK-IN-005', craneId: '--', targetLocation: 'C-01-09-06', materialCode: 'M-007', status: '排队中', statusType: 'info', priority: '中', priorityType: 'warning' },
])

// ============================================================
//  实时更新
// ============================================================
const lastUpdateTime = ref('')
let timer = null

const allOnline = computed(() => {
  return !craneList.value.some(c => c.status === '故障')
})

function updateSimulatedData() {
  const now = new Date()
  lastUpdateTime.value = `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}:${String(now.getSeconds()).padStart(2, '0')}`

  // 更新运行中堆垛机的位置和速度
  craneList.value.forEach(crane => {
    if (crane.status === '运行中') {
      crane.speed = Math.floor(Math.random() * 60) + 80
      crane.load = Math.floor(Math.random() * 500) + 500
      // 模拟位置变化
      const parts = crane.position.match(/(\d+)排-(\d+)列-(\d+)层/)
      if (parts) {
        const row = Math.min(10, Math.max(1, parseInt(parts[1]) + Math.floor(Math.random() * 3) - 1))
        const col = Math.min(20, Math.max(1, parseInt(parts[2]) + Math.floor(Math.random() * 5) - 2))
        const layer = Math.min(8, Math.max(1, parseInt(parts[3]) + Math.floor(Math.random() * 3) - 1))
        crane.position = `${row}排-${col}列-${layer}层`
      }
    }
  })
}

onMounted(() => {
  updateSimulatedData()
  timer = setInterval(updateSimulatedData, 3000)
})

onBeforeUnmount(() => {
  if (timer) {
    clearInterval(timer)
    timer = null
  }
})
</script>

<style scoped>
.crane-monitor {
  padding: 16px;
  background: #f5f7fa;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.monitor-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}

.monitor-header-left {
  display: flex;
  align-items: center;
  gap: 10px;
}

.monitor-header-dot {
  display: inline-block;
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.monitor-header-dot.is-online {
  background: #5ad8a6;
  box-shadow: 0 0 6px rgba(90, 216, 166, 0.5);
}

.monitor-header-title {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #1d2129;
}

.monitor-header-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.monitor-header-time {
  font-size: 13px;
  color: #86909c;
}

/* ==================== 堆垛机卡片 ==================== */
.crane-cards {
  margin-bottom: 0;
}

.crane-card {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
  position: relative;
  overflow: hidden;
  transition: box-shadow 0.3s ease;
  border-top: 3px solid #c9d1d9;
}

.crane-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12);
}

.crane-card--success {
  border-top-color: #5ad8a6;
}

.crane-card--info {
  border-top-color: #5b8ff9;
}

.crane-card--danger {
  border-top-color: #e86452;
}

.crane-card--warning {
  border-top-color: #f6bd16;
}

.crane-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 14px;
}

.crane-card__id {
  font-size: 16px;
  font-weight: 600;
  color: #1d2129;
}

.crane-card__body {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.crane-card__info-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.crane-card__label {
  font-size: 13px;
  color: #86909c;
}

.crane-card__value {
  font-size: 13px;
  color: #1d2129;
  font-weight: 500;
}

/* 运行中脉冲动画 */
.crane-card__indicator {
  position: absolute;
  bottom: 12px;
  right: 16px;
  display: flex;
  align-items: center;
  gap: 4px;
}

.crane-card__pulse {
  display: inline-block;
  width: 4px;
  height: 16px;
  border-radius: 2px;
  background: #5ad8a6;
  animation: pulse 1.2s ease-in-out infinite;
}

.crane-card__pulse:nth-child(2) {
  animation-delay: 0.2s;
}

.crane-card__pulse:nth-child(3) {
  animation-delay: 0.4s;
}

@keyframes pulse {
  0%, 100% {
    opacity: 0.3;
    transform: scaleY(0.6);
  }
  50% {
    opacity: 1;
    transform: scaleY(1);
  }
}

/* ==================== 任务队列 ==================== */
.crane-queue {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}

.crane-queue__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.crane-queue__title {
  margin: 0;
  font-size: 15px;
  font-weight: 600;
  color: #1d2129;
}
</style>
