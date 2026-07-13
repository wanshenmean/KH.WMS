<template>
  <div class="home-dashboard" v-loading="loading">
    <!-- 第一行：统计卡片（响应式，低分辨率自动换行） -->
    <el-row :gutter="16" class="stat-row row--fixed">
      <el-col :xs="12" :sm="8" :md="6" :lg="4" v-for="stat in statCards" :key="stat.label">
        <KhStatCard
          :value="stat.value"
          :label="stat.label"
          :icon="stat.icon"
          :theme="stat.theme"
          :formatter="stat.formatter"
        />
      </el-col>
    </el-row>

    <!-- 第二行：快捷操作 + 库存概览（等高，低分辨率堆叠） -->
    <el-row :gutter="16" class="row--fixed row--equal">
      <el-col :xs="24" :md="8">
        <el-card shadow="never" class="section-card">
          <template #header>
            <div class="section-header">
              <span class="section-title">快捷操作</span>
            </div>
          </template>
          <div class="quick-action-grid">
            <div
              v-for="action in quickActions"
              :key="action.label"
              class="quick-action-item"
              @click="router.push(action.path)"
            >
              <div class="quick-action-icon" :class="action.color">
                <el-icon :size="20"><component :is="action.icon" /></el-icon>
              </div>
              <span class="quick-action-label">{{ action.label }}</span>
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :xs="24" :md="16">
        <el-card shadow="never" class="section-card">
          <template #header>
            <div class="section-header">
              <span class="section-title">库存概览</span>
              <el-button link type="primary" @click="router.push('/inventory/stock-query')">查看详情</el-button>
            </div>
          </template>
          <el-row :gutter="24">
            <el-col :xs="12" :sm="6">
              <div class="inv-stat-item">
                <div class="inv-stat-value">{{ inventory.containerCount.toLocaleString() }}</div>
                <div class="inv-stat-label">在库容器</div>
              </div>
            </el-col>
            <el-col :xs="12" :sm="6">
              <div class="inv-stat-item">
                <div class="inv-stat-value">{{ inventory.materialTypeCount.toLocaleString() }}</div>
                <div class="inv-stat-label">物料种类</div>
              </div>
            </el-col>
            <el-col :xs="12" :sm="6">
              <div class="inv-stat-item">
                <div class="inv-stat-value">{{ inventory.detailCount.toLocaleString() }}</div>
                <div class="inv-stat-label">库存明细</div>
              </div>
            </el-col>
            <el-col :xs="12" :sm="6">
              <div class="inv-stat-item">
                <div class="inv-stat-value inv-stat--warning">{{ inventory.lockedDetailCount.toLocaleString() }}</div>
                <div class="inv-stat-label">锁定明细</div>
              </div>
            </el-col>
          </el-row>
          <div class="inv-bar-section">
            <div class="inv-bar-header">
              <span>库存状态分布（按容器）</span>
              <span>合计 {{ statusTotal.toLocaleString() }}</span>
            </div>
            <div class="inv-bar-track">
              <div class="inv-bar-fill inv-bar-fill--available" :style="{ width: pct(inventory.statusAvailable) + '%' }"></div>
              <div class="inv-bar-fill inv-bar-fill--locked" :style="{ width: pct(inventory.statusLocked) + '%' }"></div>
              <div class="inv-bar-fill inv-bar-fill--qc" :style="{ width: pct(inventory.statusQC) + '%' }"></div>
              <div class="inv-bar-fill inv-bar-fill--frozen" :style="{ width: pct(inventory.statusFrozen) + '%' }"></div>
            </div>
            <div class="inv-bar-legend">
              <span class="legend-item"><i class="legend-dot legend-dot--available"></i>可用 {{ inventory.statusAvailable.toLocaleString() }}</span>
              <span class="legend-item"><i class="legend-dot legend-dot--locked"></i>锁定 {{ inventory.statusLocked.toLocaleString() }}</span>
              <span class="legend-item"><i class="legend-dot legend-dot--qc"></i>质检 {{ inventory.statusQC.toLocaleString() }}</span>
              <span class="legend-item"><i class="legend-dot legend-dot--frozen"></i>冻结 {{ inventory.statusFrozen.toLocaleString() }}</span>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 第三行：出入库趋势 + 最近完成单据（低分辨率堆叠） -->
    <el-row :gutter="16" class="row--fixed row--equal row--compact">
      <el-col :xs="24" :md="14">
        <el-card shadow="never" class="section-card trend-card card--flex">
          <template #header>
            <div class="section-header">
              <span class="section-title">近7日出入库趋势</span>
            </div>
          </template>
          <div class="card-body--flex">
          <div class="trend-legend">
            <span class="trend-legend-item trend-legend--inbound">
              <i class="legend-dot legend-dot--inbound"></i>入库 ({{ inboundTotal.toLocaleString() }})
            </span>
            <span class="trend-legend-item trend-legend--outbound">
              <i class="legend-dot legend-dot--outbound"></i>出库 ({{ outboundTotal.toLocaleString() }})
            </span>
          </div>
          <div class="trend-chart">
            <div
              v-for="(item, index) in inboundTrend"
              :key="item.date"
              class="trend-col"
            >
              <div class="trend-bars">
                <div class="trend-bar-group">
                  <div
                    class="trend-bar trend-bar--inbound"
                    :style="{ height: inboundMax > 0 ? (item.value / inboundMax * 100) + '%' : '0%' }"
                    :title="'入库: ' + item.value"
                  ></div>
                </div>
                <div class="trend-bar-group">
                  <div
                    class="trend-bar trend-bar--outbound"
                    :style="{ height: outboundMax > 0 ? ((outboundTrend[index]?.value || 0) / outboundMax * 100) + '%' : '0%' }"
                    :title="'出库: ' + (outboundTrend[index]?.value || 0)"
                  ></div>
                </div>
              </div>
              <div class="trend-col-date">{{ item.date }}</div>
              <div class="trend-col-value">{{ item.value }}</div>
            </div>
          </div>
          </div>
        </el-card>
      </el-col>
      <el-col :xs="24" :md="10">
        <el-card shadow="never" class="section-card card--flex">
          <template #header>
            <div class="section-header">
              <span class="section-title">最近完成单据</span>
              <el-tag type="success" size="small" effect="plain">{{ recentDocuments.length }} 条</el-tag>
            </div>
          </template>
          <div class="card-body--flex">
          <div class="task-list">
            <div
              v-for="doc in recentDocuments"
              :key="doc.category + '-' + doc.orderNo"
              class="task-item"
            >
              <div class="task-left">
                <el-tag
                  :type="doc.category === 'INBOUND' ? 'primary' : 'success'"
                  size="small"
                  effect="dark"
                  class="task-priority-tag"
                >
                  {{ doc.category === 'INBOUND' ? '入库' : '出库' }}
                </el-tag>
                <div class="task-info">
                  <div class="task-main">
                    <span class="task-no">{{ doc.orderNo }}</span>
                  </div>
                  <div class="task-sub">{{ orderTypeLabel(doc.orderType) }} · {{ doc.totalLines }} 行</div>
                </div>
              </div>
              <div class="task-time">{{ formatTime(doc.completedTime) }}</div>
            </div>
            <el-empty v-if="recentDocuments.length === 0" description="暂无已完成单据" :image-size="60" />
          </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 第四行：任务类型分布 + 库位状态（低分辨率堆叠） -->
    <el-row :gutter="16" class="row--flex row--equal">
      <el-col :xs="24" :md="12">
        <el-card shadow="never" class="section-card card--flex">
          <template #header>
            <div class="section-header">
              <span class="section-title">任务类型分布（近7日）</span>
              <span class="section-sub">合计 {{ taskTypeTotal.toLocaleString() }}</span>
            </div>
          </template>
          <div class="card-body--flex">
            <div class="dist-list">
              <div
                v-for="item in operations.taskTypeDistribution"
                :key="item.taskType"
                class="dist-item"
              >
                <span class="dist-label">{{ taskTypeLabel(item.taskType) }}</span>
                <div class="dist-bar-track">
                  <div class="dist-bar-fill" :style="{ width: distPct(item.count) + '%' }"></div>
                </div>
                <span class="dist-count">{{ (item.count || 0).toLocaleString() }}</span>
              </div>
              <el-empty v-if="operations.taskTypeDistribution.length === 0" description="暂无任务数据" :image-size="56" />
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :xs="24" :md="12">
        <el-card shadow="never" class="section-card card--flex">
          <template #header>
            <div class="section-header">
              <span class="section-title">库位状态</span>
              <span class="section-sub">总库位 {{ operations.locationTotal.toLocaleString() }}</span>
            </div>
          </template>
          <div class="card-body--flex loc-body">
            <el-row :gutter="16">
              <el-col :xs="12" :sm="6">
                <div class="loc-stat">
                  <div class="loc-value loc--success">{{ operations.locationEmpty.toLocaleString() }}</div>
                  <div class="loc-label">空闲</div>
                </div>
              </el-col>
              <el-col :xs="12" :sm="6">
                <div class="loc-stat">
                  <div class="loc-value loc--primary">{{ operations.locationOccupied.toLocaleString() }}</div>
                  <div class="loc-label">占用</div>
                </div>
              </el-col>
              <el-col :xs="12" :sm="6">
                <div class="loc-stat">
                  <div class="loc-value loc--warning">{{ operations.locationLocked.toLocaleString() }}</div>
                  <div class="loc-label">锁定</div>
                </div>
              </el-col>
              <el-col :xs="12" :sm="6">
                <div class="loc-stat">
                  <div class="loc-value loc--info">{{ operations.locationDisabled.toLocaleString() }}</div>
                  <div class="loc-label">禁用</div>
                </div>
              </el-col>
            </el-row>
            <div class="loc-bar-section">
              <div class="loc-bar-header">
                <span>占用构成</span>
                <span>{{ operations.locationOccupied.toLocaleString() }} / {{ operations.locationTotal.toLocaleString() }}</span>
              </div>
              <div class="loc-bar-track">
                <div class="loc-bar-fill" :style="{ width: locationOccupiedPct + '%' }"></div>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import {
  Download,
  Upload,
  Bell,
  Box,
  Tickets,
  Search,
  CircleCheck,
  Loading,
  Coin,
} from '@element-plus/icons-vue'
import KhStatCard from '@/components/KhStatCard/index.vue'
import { getHomeStat, getHomeTrend, getRecentDocuments, getInventoryOverview, getOperationsOverview } from '@/api/dashboard'

const router = useRouter()
const loading = ref(true)

// ─── Stat cards ────────────────────────────────────────────
const statCards = ref([
  { label: '今日入库单', value: 0, icon: markRaw(Download), theme: 'primary', formatter: undefined },
  { label: '今日出库单', value: 0, icon: markRaw(Upload), theme: 'success', formatter: undefined },
  { label: '今日完成任务', value: 0, icon: markRaw(CircleCheck), theme: 'info', formatter: undefined },
  { label: '执行中任务', value: 0, icon: markRaw(Loading), theme: 'warning', formatter: undefined },
  { label: '库位使用率', value: 0, icon: markRaw(Box), theme: 'info', formatter: (v) => v + '%' },
  { label: '在库容器', value: 0, icon: markRaw(Coin), theme: 'success', formatter: undefined },
])

// ─── Quick actions ─────────────────────────────────────────
const quickActions = ref([
  { label: '入库单管理', icon: markRaw(Download), path: '/inbound/order', color: 'is-primary' },
  { label: '组盘管理', icon: markRaw(Box), path: '/inbound/container-bind-list', color: 'is-info' },
  { label: '出库单管理', icon: markRaw(Upload), path: '/outbound/order', color: 'is-success' },
  { label: '库存查询', icon: markRaw(Search), path: '/inventory/stock-query', color: 'is-info' },
  { label: '任务列表', icon: markRaw(Bell), path: '/task/list', color: 'is-warning' },
  { label: '库存变动记录', icon: markRaw(Tickets), path: '/inventory/movement-list', color: 'is-primary' },
])

// ─── Inventory overview（全部为单位无关的计数指标）─────────
const inventory = ref({
  containerCount: 0,
  materialTypeCount: 0,
  detailCount: 0,
  lockedDetailCount: 0,
  statusAvailable: 0,
  statusLocked: 0,
  statusQC: 0,
  statusFrozen: 0,
})

const statusTotal = computed(() =>
  inventory.value.statusAvailable +
  inventory.value.statusLocked +
  inventory.value.statusQC +
  inventory.value.statusFrozen
)

function pct(n) {
  return statusTotal.value > 0 ? Math.round(n / statusTotal.value * 100) : 0
}

// ─── Trend ──────────────────────────────────────────────────
const inboundTrend = ref([])
const outboundTrend = ref([])
const inboundTotal = computed(() => inboundTrend.value.reduce((s, i) => s + i.value, 0))
const outboundTotal = computed(() => outboundTrend.value.reduce((s, i) => s + i.value, 0))
const inboundMax = computed(() => Math.max(...inboundTrend.value.map(i => i.value), 1))
const outboundMax = computed(() => Math.max(...outboundTrend.value.map(i => i.value), 1))

// ─── Recent completed documents ─────────────────────────────
const recentDocuments = ref([])

const orderTypeMap = {
  PURCHASE: '采购入库',
  RETURN_INBOUND: '退货入库',
  PRODUCTION_INBOUND: '生产入库',
  OTHER_INBOUND: '其他入库',
}

function orderTypeLabel(t) {
  return orderTypeMap[t] || t || '-'
}

// ─── Operations overview（任务类型分布 + 库位状态）──────────
const operations = ref({
  taskTypeDistribution: [],
  locationTotal: 0,
  locationEmpty: 0,
  locationOccupied: 0,
  locationLocked: 0,
  locationDisabled: 0,
})

const taskTypeMap = {
  PUTAWAY: '上架',
  PICKING: '拣货',
  TRANSFER: '移库',
  REPLENISHMENT: '补货',
  STOCKTAKE: '盘点',
}

function taskTypeLabel(t) {
  return taskTypeMap[t] || t || '-'
}

const taskTypeTotal = computed(() =>
  operations.value.taskTypeDistribution.reduce((s, i) => s + (i.count || 0), 0)
)

const taskTypeMax = computed(() =>
  Math.max(...operations.value.taskTypeDistribution.map(i => i.count || 0), 1)
)

function distPct(count) {
  return taskTypeMax.value > 0 ? Math.round(count / taskTypeMax.value * 100) : 0
}

const locationOccupiedPct = computed(() => {
  const total = operations.value.locationTotal
  return total > 0 ? Math.round(operations.value.locationOccupied / total * 100) : 0
})

function formatTime(val) {
  if (!val) return ''
  const d = new Date(val)
  const pad = n => String(n).padStart(2, '0')
  return `${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

// ─── Fetch data ────────────────────────────────────────────
async function fetchData() {
  try {
    const [statRes, trendRes, docsRes, invRes, opsRes] = await Promise.all([
      getHomeStat(),
      getHomeTrend(7),
      getRecentDocuments(10),
      getInventoryOverview(),
      getOperationsOverview(),
    ])

    // Stat cards
    if (statRes?.data) {
      const d = statRes.data
      statCards.value[0].value = d.todayInboundCount
      statCards.value[1].value = d.todayOutboundCount
      statCards.value[2].value = d.todayCompletedTaskCount
      statCards.value[3].value = d.inProgressTaskCount
      statCards.value[4].value = d.locationUsagePercent
      statCards.value[5].value = d.containerCount
    }

    // Trend
    if (trendRes?.data) {
      inboundTrend.value = trendRes.data.inbound || []
      outboundTrend.value = trendRes.data.outbound || []
    }

    // Recent documents
    if (docsRes?.data) {
      recentDocuments.value = docsRes.data || []
    }

    // Inventory overview
    if (invRes?.data) {
      inventory.value = { ...inventory.value, ...invRes.data }
    }

    // Operations overview
    if (opsRes?.data) {
      operations.value = { ...operations.value, ...opsRes.data }
    }
  } catch (e) {
    console.error('首页数据加载失败', e)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
.home-dashboard {
  padding: 4px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-height: 0;
  flex: 1;
  overflow-x: hidden;
  overflow-y: auto;
}

/* ── Flex layout helpers ────────────────────────────── */
.row--fixed {
  flex-shrink: 0;
}

.row--flex {
  flex: 1;
  min-height: 220px;
}

.row--flex > .el-col {
  height: 100%;
}

.card--flex {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.card--flex :deep(.el-card__body) {
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
}

.card-body--flex {
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
}

/* ── 第二行两列等高 ─────────────────────────────────── */
.row--equal {
  align-items: stretch;
}

.row--equal > .el-col {
  display: flex;
}

.row--equal > .el-col > .el-card {
  width: 100%;
}

/* ── Stat row ─────────────────────────────────────────── */
.stat-row {
  margin-bottom: 0;
}

/* ── Section card ─────────────────────────────────────── */
.section-card {
  border-radius: 8px;
  border: 1px solid #ebeef5;
}

.section-card :deep(.el-card__header) {
  padding: 14px 20px;
  border-bottom: 1px solid #f2f3f5;
}

.section-card :deep(.el-card__body) {
  padding: 16px 20px;
}

.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.section-title {
  font-size: 15px;
  font-weight: 600;
  color: #1d2129;
}

/* ── Quick actions ────────────────────────────────────── */
.quick-action-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
}

.quick-action-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 14px 8px;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
}

.quick-action-item:hover {
  background: #f7f8fa;
  border-color: #e5e6eb;
}

.quick-action-icon {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
}

.quick-action-icon.is-primary { background: linear-gradient(135deg, #5b8ff9, #3d76e0); }
.quick-action-icon.is-success { background: linear-gradient(135deg, #5ad8a6, #36b37e); }
.quick-action-icon.is-warning { background: linear-gradient(135deg, #f7ba1e, #f09000); }
.quick-action-icon.is-info { background: linear-gradient(135deg, #6dc8ec, #3ba1d1); }

.quick-action-label {
  font-size: 13px;
  color: #4e5969;
  font-weight: 500;
}

/* ── Inventory overview ─────────────────────────────── */
.inv-stat-item {
  text-align: center;
  padding: 8px 0;
}

.inv-stat-value {
  font-size: 24px;
  font-weight: 600;
  color: #1d2129;
}

.inv-stat--success { color: #36b37e; }
.inv-stat--warning { color: #f09000; }

.inv-stat-label {
  font-size: 13px;
  color: #86909c;
  margin-top: 4px;
}

.inv-bar-section {
  margin-top: 16px;
}

.inv-bar-header {
  display: flex;
  justify-content: space-between;
  font-size: 13px;
  color: #4e5969;
  margin-bottom: 8px;
}

.inv-bar-track {
  height: 12px;
  background: #f2f3f5;
  border-radius: 6px;
  overflow: hidden;
  display: flex;
}

.inv-bar-fill {
  height: 100%;
  transition: width 0.6s ease;
}

.inv-bar-fill--available {
  background: linear-gradient(90deg, #5ad8a6, #36b37e);
}

.inv-bar-fill--locked {
  background: linear-gradient(90deg, #f7ba1e, #f09000);
}

.inv-bar-fill--qc {
  background: linear-gradient(90deg, #6dc8ec, #3ba1d1);
}

.inv-bar-fill--frozen {
  background: linear-gradient(90deg, #b8b8c8, #8a8aa0);
}

.inv-bar-legend {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 16px;
  margin-top: 8px;
  font-size: 12px;
  color: #86909c;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 4px;
}

.legend-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  display: inline-block;
}

.legend-dot--available { background: #36b37e; }
.legend-dot--locked { background: #f09000; }
.legend-dot--qc { background: #3ba1d1; }
.legend-dot--frozen { background: #8a8aa0; }
.legend-dot--inbound { background: #5b8ff9; }
.legend-dot--outbound { background: #5ad8a6; }

/* ── Trend chart ──────────────────────────────────────── */
.trend-legend {
  display: flex;
  gap: 20px;
  margin-bottom: 12px;
  font-size: 13px;
  color: #4e5969;
}

.trend-legend-item {
  display: flex;
  align-items: center;
  gap: 4px;
}

.trend-chart {
  flex: 1;
  display: flex;
  align-items: flex-end;
  gap: 0;
  min-height: 100px;
  padding: 0 4px;
}

.trend-col {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  height: 100%;
}

.trend-bars {
  flex: 1;
  display: flex;
  align-items: flex-end;
  gap: 4px;
  width: 100%;
  justify-content: center;
}

.trend-bar-group {
  flex: 1;
  max-width: 24px;
  min-width: 8px;
  height: 100%;
  display: flex;
  align-items: flex-end;
}

.trend-bar {
  width: 100%;
  border-radius: 3px 3px 0 0;
  min-height: 2px;
  transition: height 0.6s ease;
}

.trend-bar--inbound {
  background: linear-gradient(180deg, #5b8ff9, #3d76e0);
}

.trend-bar--outbound {
  background: linear-gradient(180deg, #5ad8a6, #36b37e);
}

.trend-col-date {
  font-size: 11px;
  color: #c0c4cc;
  margin-top: 6px;
}

.trend-col-value {
  font-size: 11px;
  color: #4e5969;
  font-weight: 500;
}

/* ── Recent documents / generic list ─────────────────── */
.task-list {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-height: 0;
  overflow-y: auto;
}

.task-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 8px;
  border-radius: 6px;
  transition: background 0.15s;
}

.task-item:hover {
  background: #f7f8fa;
}

.task-left {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 0;
}

.task-priority-tag {
  flex-shrink: 0;
}

.task-info {
  min-width: 0;
}

.task-main {
  display: flex;
  align-items: center;
  gap: 8px;
}

.task-no {
  font-size: 13px;
  font-weight: 500;
  color: #1d2129;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.task-sub {
  font-size: 12px;
  color: #c0c4cc;
  margin-top: 2px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.task-time {
  font-size: 12px;
  color: #c0c4cc;
  flex-shrink: 0;
  margin-left: 12px;
}

/* ── 第三行紧凑高度（低分辨率下自适应） ────────────────── */
.row--compact {
  min-height: 300px;
}

/* ── 卡片头部副标题 ──────────────────────────────────── */
.section-sub {
  font-size: 12px;
  color: #86909c;
  font-weight: 400;
}

/* ── 任务类型分布（横向条形） ────────────────────────── */
.dist-list {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 14px;
  min-height: 0;
}

.dist-item {
  display: flex;
  align-items: center;
  gap: 12px;
}

.dist-label {
  width: 56px;
  flex-shrink: 0;
  font-size: 13px;
  color: #4e5969;
  text-align: right;
}

.dist-bar-track {
  flex: 1;
  height: 14px;
  background: #f2f3f5;
  border-radius: 7px;
  overflow: hidden;
}

.dist-bar-fill {
  height: 100%;
  border-radius: 7px;
  background: linear-gradient(90deg, #5b8ff9, #3d76e0);
  transition: width 0.6s ease;
}

.dist-count {
  width: 48px;
  flex-shrink: 0;
  font-size: 13px;
  font-weight: 600;
  color: #1d2129;
  text-align: right;
}

/* ── 库位状态 ─────────────────────────────────────────── */
.loc-body {
  justify-content: center;
  gap: 18px;
}

.loc-stat {
  text-align: center;
  padding: 10px 0;
}

.loc-value {
  font-size: 24px;
  font-weight: 600;
  color: #1d2129;
  line-height: 1.3;
}

.loc--success { color: #36b37e; }
.loc--primary { color: #3d76e0; }
.loc--warning { color: #f09000; }
.loc--info { color: #3ba1d1; }

.loc-label {
  font-size: 13px;
  color: #86909c;
  margin-top: 4px;
}

.loc-bar-section {
  margin-top: 4px;
}

.loc-bar-header {
  display: flex;
  justify-content: space-between;
  font-size: 13px;
  color: #4e5969;
  margin-bottom: 8px;
}

.loc-bar-track {
  height: 12px;
  background: #f2f3f5;
  border-radius: 6px;
  overflow: hidden;
}

.loc-bar-fill {
  height: 100%;
  background: linear-gradient(90deg, #5b8ff9, #3d76e0);
  border-radius: 6px;
  transition: width 0.6s ease;
}

/* ── 响应式：小屏适配 ───────────────────────────────── */
/* 中小屏（< 992px）：卡片堆叠后增加间距，避免内容贴紧 */
@media (max-width: 991px) {
  .home-dashboard {
    gap: 12px;
  }

  .section-card :deep(.el-card__header) {
    padding: 12px 16px;
  }

  .section-card :deep(.el-card__body) {
    padding: 12px 16px;
  }

  .inv-stat-value,
  .loc-value {
    font-size: 20px;
  }

  .quick-action-grid {
    grid-template-columns: repeat(3, 1fr);
    gap: 8px;
  }

  .quick-action-item {
    padding: 10px 6px;
  }
}

/* 超小屏（< 768px）：进一步压缩，趋势图隐藏数值避免溢出 */
@media (max-width: 767px) {
  .home-dashboard {
    padding: 2px;
    gap: 10px;
  }

  .inv-stat-value,
  .loc-value {
    font-size: 18px;
  }

  .inv-bar-legend {
    gap: 8px;
  }

  .trend-col-value {
    display: none;
  }

  .row--compact {
    min-height: 240px;
  }
}

</style>
