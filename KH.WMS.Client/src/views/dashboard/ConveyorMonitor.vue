<template>
  <div class="conveyor-monitor">
    <!-- 页面标题 -->
    <div class="monitor-header">
      <div class="monitor-header-left">
        <span class="monitor-header-dot" :class="{ 'is-online': allRunning, 'is-warning': !allRunning }"></span>
        <h2 class="monitor-header-title">输送线实时监控</h2>
      </div>
      <div class="monitor-header-right">
        <span class="monitor-header-time">更新时间: {{ lastUpdateTime }}</span>
        <el-tag :type="allRunning ? 'success' : 'warning'" effect="dark" size="small">
          {{ allRunning ? '全部运行' : '存在异常' }}
        </el-tag>
      </div>
    </div>

    <!-- 汇总统计 -->
    <el-row :gutter="16" class="conveyor-summary">
      <el-col :span="6">
        <div class="summary-item">
          <div class="summary-item__value" style="color: #5b8ff9">{{ totalSegments }}</div>
          <div class="summary-item__label">输送段总数</div>
        </div>
      </el-col>
      <el-col :span="6">
        <div class="summary-item">
          <div class="summary-item__value" style="color: #5ad8a6">{{ runningCount }}</div>
          <div class="summary-item__label">运行中</div>
        </div>
      </el-col>
      <el-col :span="6">
        <div class="summary-item">
          <div class="summary-item__value" style="color: #e86452">{{ faultCount }}</div>
          <div class="summary-item__label">故障</div>
        </div>
      </el-col>
      <el-col :span="6">
        <div class="summary-item">
          <div class="summary-item__value" style="color: #f6bd16">{{ totalThroughput.toLocaleString() }}</div>
          <div class="summary-item__label">今日吞吐量(件)</div>
        </div>
      </el-col>
    </el-row>

    <!-- 输送线段可视化 -->
    <div class="conveyor-lines">
      <div class="conveyor-lines__title">输送线段状态一览</div>
      <div class="conveyor-line-group" v-for="(line, lineIdx) in conveyorLines" :key="lineIdx">
        <div class="conveyor-line__label">{{ line.name }}</div>
        <div class="conveyor-line__segments">
          <div
            v-for="(seg, segIdx) in line.segments"
            :key="segIdx"
            class="conveyor-segment"
            :class="[`conveyor-segment--${seg.statusType}`]"
            @click="handleSegmentClick(seg)"
          >
            <!-- 流动动画 -->
            <div v-if="seg.status === '运行'" class="conveyor-segment__flow">
              <span class="conveyor-segment__arrow">&rsaquo;</span>
              <span class="conveyor-segment__arrow">&rsaquo;</span>
              <span class="conveyor-segment__arrow">&rsaquo;</span>
            </div>
            <div class="conveyor-segment__info">
              <span class="conveyor-segment__id">{{ seg.id }}</span>
              <el-tag :type="seg.statusType" effect="dark" size="small">{{ seg.status }}</el-tag>
            </div>
            <div class="conveyor-segment__details">
              <span>货物: {{ seg.currentGoods }}</span>
              <span>速度: {{ seg.speed }} m/s</span>
              <span>吞吐: {{ seg.throughput }} 件/h</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 选中段详情 -->
    <div v-if="selectedSegment" class="conveyor-detail">
      <div class="conveyor-detail__header">
        <h3 class="conveyor-detail__title">段详情 - {{ selectedSegment.id }}</h3>
        <el-button type="info" link @click="selectedSegment = null">关闭</el-button>
      </div>
      <el-descriptions :column="3" border size="small">
        <el-descriptions-item label="段编号">{{ selectedSegment.id }}</el-descriptions-item>
        <el-descriptions-item label="所属线路">{{ selectedSegment.lineName }}</el-descriptions-item>
        <el-descriptions-item label="状态">
          <el-tag :type="selectedSegment.statusType" effect="light">{{ selectedSegment.status }}</el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="当前货物">{{ selectedSegment.currentGoods }}</el-descriptions-item>
        <el-descriptions-item label="运行速度">{{ selectedSegment.speed }} m/s</el-descriptions-item>
        <el-descriptions-item label="吞吐量">{{ selectedSegment.throughput }} 件/h</el-descriptions-item>
        <el-descriptions-item label="累计运行">{{ selectedSegment.totalRuntime }} 小时</el-descriptions-item>
        <el-descriptions-item label="最近维护">{{ selectedSegment.lastMaintenance }}</el-descriptions-item>
        <el-descriptions-item label="温度">{{ selectedSegment.temperature }}°C</el-descriptions-item>
      </el-descriptions>
    </div>
  </div>
</template>

<script setup>
// ============================================================
//  输送线模拟数据
// ============================================================
const conveyorLines = ref([
  {
    name: '1号线（入库线）',
    segments: [
      { id: 'L1-S01', status: '运行', statusType: 'success', currentGoods: '托盘#A1023', speed: 1.2, throughput: 180, totalRuntime: 2860, lastMaintenance: '2025-03-15', temperature: 38.5, lineName: '1号线（入库线）' },
      { id: 'L1-S02', status: '运行', statusType: 'success', currentGoods: '料箱#B0561', speed: 1.0, throughput: 165, totalRuntime: 2860, lastMaintenance: '2025-03-15', temperature: 37.2, lineName: '1号线（入库线）' },
      { id: 'L1-S03', status: '停止', statusType: 'info', currentGoods: '无', speed: 0, throughput: 0, totalRuntime: 2860, lastMaintenance: '2025-03-15', temperature: 25.1, lineName: '1号线（入库线）' },
      { id: 'L1-S04', status: '运行', statusType: 'success', currentGoods: '托盘#A1024', speed: 1.2, throughput: 175, totalRuntime: 2860, lastMaintenance: '2025-03-15', temperature: 39.0, lineName: '1号线（入库线）' },
    ],
  },
  {
    name: '2号线（出库线）',
    segments: [
      { id: 'L2-S01', status: '运行', statusType: 'success', currentGoods: '托盘#C2045', speed: 1.5, throughput: 210, totalRuntime: 3120, lastMaintenance: '2025-03-20', temperature: 40.1, lineName: '2号线（出库线）' },
      { id: 'L2-S02', status: '故障', statusType: 'danger', currentGoods: '托盘#C2046(卡滞)', speed: 0, throughput: 0, totalRuntime: 3120, lastMaintenance: '2025-03-20', temperature: 52.3, lineName: '2号线（出库线）' },
      { id: 'L2-S03', status: '运行', statusType: 'success', currentGoods: '料箱#D0782', speed: 1.3, throughput: 195, totalRuntime: 3120, lastMaintenance: '2025-03-20', temperature: 38.8, lineName: '2号线（出库线）' },
    ],
  },
  {
    name: '3号线（分拣线）',
    segments: [
      { id: 'L3-S01', status: '运行', statusType: 'success', currentGoods: '料箱#E0123', speed: 0.8, throughput: 240, totalRuntime: 1980, lastMaintenance: '2025-04-01', temperature: 36.5, lineName: '3号线（分拣线）' },
      { id: 'L3-S02', status: '运行', statusType: 'success', currentGoods: '料箱#E0124', speed: 0.8, throughput: 235, totalRuntime: 1980, lastMaintenance: '2025-04-01', temperature: 35.9, lineName: '3号线（分拣线）' },
      { id: 'L3-S03', status: '运行', statusType: 'success', currentGoods: '料箱#E0125', speed: 0.8, throughput: 230, totalRuntime: 1980, lastMaintenance: '2025-04-01', temperature: 36.2, lineName: '3号线（分拣线）' },
      { id: 'L3-S04', status: '运行', statusType: 'success', currentGoods: '料箱#E0126', speed: 0.8, throughput: 225, totalRuntime: 1980, lastMaintenance: '2025-04-01', temperature: 37.0, lineName: '3号线（分拣线）' },
      { id: 'L3-S05', status: '停止', statusType: 'info', currentGoods: '无', speed: 0, throughput: 0, totalRuntime: 1980, lastMaintenance: '2025-04-01', temperature: 24.8, lineName: '3号线（分拣线）' },
    ],
  },
])

// ============================================================
//  统计数据
// ============================================================
const allSegments = computed(() => {
  return conveyorLines.value.flatMap(line => line.segments)
})

const totalSegments = computed(() => allSegments.value.length)

const runningCount = computed(() => {
  return allSegments.value.filter(s => s.status === '运行').length
})

const faultCount = computed(() => {
  return allSegments.value.filter(s => s.status === '故障').length
})

const totalThroughput = computed(() => {
  return allSegments.value.reduce((sum, s) => sum + s.throughput, 0)
})

const allRunning = computed(() => faultCount.value === 0)

// ============================================================
//  交互
// ============================================================
const selectedSegment = ref(null)

const handleSegmentClick = (seg) => {
  selectedSegment.value = seg
  KhMessageFn.info(`查看段详情: ${seg.id}`)
}

// ============================================================
//  实时更新
// ============================================================
const lastUpdateTime = ref('')
let timer = null

const goodsPool = [
  '托盘#A1023', '托盘#A1024', '托盘#B0561', '托盘#C2045', '托盘#C2046',
  '料箱#D0782', '料箱#E0123', '料箱#E0124', '料箱#E0125', '料箱#F0331',
  '料箱#G0456', '托盘#H0289', '料箱#J0192', '托盘#K0456',
]

function updateSimulatedData() {
  const now = new Date()
  lastUpdateTime.value = `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}:${String(now.getSeconds()).padStart(2, '0')}`

  conveyorLines.value.forEach(line => {
    line.segments.forEach(seg => {
      if (seg.status === '运行') {
        // 随机更新货物
        if (Math.random() > 0.7) {
          seg.currentGoods = goodsPool[Math.floor(Math.random() * goodsPool.length)]
        }
        // 微调速度
        seg.speed = Math.max(0.5, seg.speed + (Math.random() - 0.5) * 0.2)
        seg.speed = Math.round(seg.speed * 10) / 10
        // 微调吞吐量
        seg.throughput = Math.max(100, seg.throughput + Math.floor((Math.random() - 0.5) * 20))
        // 微调温度
        seg.temperature = Math.round((seg.temperature + (Math.random() - 0.5) * 1) * 10) / 10
        seg.temperature = Math.max(30, Math.min(55, seg.temperature))
      }
      if (seg.status === '故障') {
        // 故障段温度升高
        seg.temperature = Math.min(65, seg.temperature + Math.random() * 0.5)
        seg.temperature = Math.round(seg.temperature * 10) / 10
      }
    })
  })

  // 如果有选中的段，更新详情
  if (selectedSegment.value) {
    const seg = allSegments.value.find(s => s.id === selectedSegment.value.id)
    if (seg) {
      selectedSegment.value = seg
    }
  }
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
.conveyor-monitor {
  padding: 16px;
  background: #f5f7fa;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* ==================== 页面头部 ==================== */
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

.monitor-header-dot.is-warning {
  background: #e86452;
  box-shadow: 0 0 6px rgba(232, 100, 82, 0.5);
  animation: blink 1.5s ease-in-out infinite;
}

@keyframes blink {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.4; }
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

/* ==================== 汇总统计 ==================== */
.conveyor-summary {
  margin-bottom: 0;
}

.summary-item {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  text-align: center;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}

.summary-item__value {
  font-size: 28px;
  font-weight: 700;
  line-height: 1.3;
}

.summary-item__label {
  font-size: 13px;
  color: #86909c;
  margin-top: 4px;
}

/* ==================== 输送线段可视化 ==================== */
.conveyor-lines {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}

.conveyor-lines__title {
  font-size: 15px;
  font-weight: 600;
  color: #1d2129;
  margin-bottom: 16px;
}

.conveyor-line-group {
  margin-bottom: 16px;
}

.conveyor-line-group:last-child {
  margin-bottom: 0;
}

.conveyor-line__label {
  font-size: 13px;
  font-weight: 600;
  color: #4e5969;
  margin-bottom: 8px;
}

.conveyor-line__segments {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.conveyor-segment {
  flex: 1;
  min-width: 200px;
  background: #f7f8fa;
  border-radius: 8px;
  padding: 12px 16px;
  cursor: pointer;
  transition: all 0.3s ease;
  border-left: 4px solid #c9d1d9;
  position: relative;
  overflow: hidden;
}

.conveyor-segment:hover {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
}

.conveyor-segment--success {
  border-left-color: #5ad8a6;
  background: linear-gradient(135deg, #f0faf5 0%, #f7f8fa 100%);
}

.conveyor-segment--info {
  border-left-color: #5b8ff9;
  background: linear-gradient(135deg, #f0f5ff 0%, #f7f8fa 100%);
}

.conveyor-segment--danger {
  border-left-color: #e86452;
  background: linear-gradient(135deg, #fff0ee 0%, #f7f8fa 100%);
}

.conveyor-segment__flow {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: none;
}

.conveyor-segment__arrow {
  font-size: 28px;
  color: rgba(90, 216, 166, 0.15);
  animation: flowRight 2s linear infinite;
}

.conveyor-segment__arrow:nth-child(2) {
  animation-delay: 0.4s;
}

.conveyor-segment__arrow:nth-child(3) {
  animation-delay: 0.8s;
}

@keyframes flowRight {
  0% {
    transform: translateX(-30px);
    opacity: 0;
  }
  50% {
    opacity: 1;
  }
  100% {
    transform: translateX(30px);
    opacity: 0;
  }
}

.conveyor-segment__info {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
  position: relative;
  z-index: 1;
}

.conveyor-segment__id {
  font-size: 14px;
  font-weight: 600;
  color: #1d2129;
}

.conveyor-segment__details {
  display: flex;
  gap: 12px;
  font-size: 12px;
  color: #86909c;
  position: relative;
  z-index: 1;
}

/* ==================== 段详情 ==================== */
.conveyor-detail {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}

.conveyor-detail__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}

.conveyor-detail__title {
  margin: 0;
  font-size: 15px;
  font-weight: 600;
  color: #1d2129;
}
</style>
