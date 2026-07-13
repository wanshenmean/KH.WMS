<template>
  <div class="location-report">
    <h2 class="page-title">库位利用率</h2>

    <!-- 统计卡片 -->
    <el-row :gutter="16" class="report-stats">
      <el-col :span="6">
        <KhStatCard :value="statTotal" label="总库位" :icon="markRaw(Grid)" theme="primary" />
      </el-col>
      <el-col :span="6">
        <KhStatCard :value="statUtilization" label="利用率" :icon="markRaw(TrendCharts)" theme="success" :formatter="v => v.toFixed(1) + '%'" />
      </el-col>
      <el-col :span="6">
        <KhStatCard :value="statIdleRate" label="空闲率" :icon="markRaw(CircleCheck)" theme="info" :formatter="v => v.toFixed(1) + '%'" />
      </el-col>
      <el-col :span="6">
        <KhStatCard :value="statMaintaining" label="维护中" :icon="markRaw(Setting)" theme="warning" />
      </el-col>
    </el-row>

    <!-- 查询表单 -->
    <div class="report-search">
      <el-form :model="searchModel" inline>
        <el-form-item label="仓库">
          <el-select v-model="searchModel.warehouse" placeholder="请选择仓库" clearable>
            <el-option v-for="w in warehouseOptions" :key="w" :label="w" :value="w" />
          </el-select>
        </el-form-item>
        <el-form-item label="库区">
          <el-select v-model="searchModel.zone" placeholder="请选择库区" clearable>
            <el-option v-for="z in zoneOptions" :key="z" :label="z" :value="z" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :icon="markRaw(Search)" @click="handleSearch">查询</el-button>
          <el-button :icon="markRaw(Refresh)" @click="handleReset">重置</el-button>
          <el-button :icon="markRaw(Download)" @click="handleExport">导出</el-button>
        </el-form-item>
      </el-form>
    </div>

    <!-- 库位利用率堆叠柱状图 -->
    <div class="report-chart-section">
      <div class="report-chart-title">各库区库位使用情况</div>
      <div class="report-chart-bars">
        <div
          v-for="(item, idx) in chartData"
          :key="idx"
          class="report-chart-bar-item"
        >
          <div class="report-chart-bar-value-top">{{ item.utilization }}%</div>
          <div class="report-chart-bar-wrapper">
            <div class="report-chart-bar-stacked">
              <div
                class="report-chart-bar-segment segment-used"
                :style="{ height: `${(item.used / item.total) * 100}%` }"
                :title="`已用: ${item.used}`"
              ></div>
              <div
                class="report-chart-bar-segment segment-locked"
                :style="{ height: `${(item.locked / item.total) * 100}%` }"
                :title="`锁定: ${item.locked}`"
              ></div>
              <div
                class="report-chart-bar-segment segment-maint"
                :style="{ height: `${(item.maintaining / item.total) * 100}%` }"
                :title="`维护: ${item.maintaining}`"
              ></div>
              <div
                class="report-chart-bar-segment segment-idle"
                :style="{ height: `${(item.idle / item.total) * 100}%` }"
                :title="`空闲: ${item.idle}`"
              ></div>
            </div>
          </div>
          <div class="report-chart-bar-label">{{ item.label }}</div>
        </div>
      </div>
      <!-- 图例 -->
      <div class="report-chart-legend">
        <span class="legend-item"><span class="legend-dot" style="background:#5b8ff9"></span>已用</span>
        <span class="legend-item"><span class="legend-dot" style="background:#f6bd16"></span>锁定</span>
        <span class="legend-item"><span class="legend-dot" style="background:#e86452"></span>维护</span>
        <span class="legend-item"><span class="legend-dot" style="background:#c9d1d9"></span>空闲</span>
      </div>
    </div>

    <!-- 明细表格 -->
    <div class="report-table">
      <el-table :data="filteredTableData" border stripe style="width: 100%" show-summary :summary-method="getSummary">
        <el-table-column type="index" label="序号" width="60" align="center" />
        <el-table-column prop="warehouse" label="仓库" width="140" align="center" />
        <el-table-column prop="zone" label="库区" width="100" align="center" />
        <el-table-column prop="total" label="总库位" width="90" align="right" />
        <el-table-column prop="used" label="已用" width="80" align="right" />
        <el-table-column prop="idle" label="空闲" width="80" align="right" />
        <el-table-column prop="locked" label="锁定" width="80" align="right" />
        <el-table-column prop="maintaining" label="维护" width="80" align="right" />
        <el-table-column prop="utilization" label="利用率(%)" width="120" align="center">
          <template #default="{ row }">
            <el-progress
              :percentage="row.utilization"
              :stroke-width="14"
              :color="getUtilizationColor(row.utilization)"
              :text-inside="true"
            />
          </template>
        </el-table-column>
      </el-table>
    </div>
  </div>
</template>

<script setup>
import KhStatCard from '@/components/KhStatCard/index.vue'

// ============================================================
//  统计数据
// ============================================================
const statTotal = ref(8640)
const statUtilization = ref(78.6)
const statIdleRate = ref(16.2)
const statMaintaining = ref(45)

// ============================================================
//  查询条件
// ============================================================
const warehouseOptions = ['A区-原材料仓', 'B区-成品仓', 'C区-半成品仓', 'D区-备件仓']
const zoneOptions = ['A1区', 'A2区', 'A3区', 'B1区', 'B2区', 'B3区', 'C1区', 'C2区']

const searchModel = reactive({
  warehouse: '',
  zone: '',
})

// ============================================================
//  模拟数据
// ============================================================
function generateData() {
  const warehouses = ['A区-原材料仓', 'B区-成品仓', 'C区-半成品仓', 'D区-备件仓']
  const zones = ['A1区', 'A2区', 'A3区', 'B1区', 'B2区', 'B3区', 'C1区', 'C2区']
  const data = []
  let id = 0

  warehouses.forEach(w => {
    zones.forEach(z => {
      id++
      const total = Math.floor(Math.random() * 500) + 200
      const usedRate = Math.random() * 0.5 + 0.4
      const used = Math.floor(total * usedRate)
      const locked = Math.floor(Math.random() * total * 0.08)
      const maintaining = Math.random() > 0.8 ? Math.floor(Math.random() * 10) + 1 : 0
      const idle = total - used - locked - maintaining
      const utilization = Math.round((used / total) * 1000) / 10
      data.push({
        id,
        warehouse: w,
        zone: z,
        total,
        used,
        idle,
        locked,
        maintaining,
        utilization,
      })
    })
  })

  return data
}

const allData = ref(generateData())

// ============================================================
//  图表数据
// ============================================================
const chartData = computed(() => {
  return allData.value.map(d => ({
    label: d.zone,
    ...d,
  }))
})

// ============================================================
//  筛选逻辑
// ============================================================
const filteredTableData = computed(() => {
  let data = allData.value
  if (searchModel.warehouse) {
    data = data.filter(d => d.warehouse === searchModel.warehouse)
  }
  if (searchModel.zone) {
    data = data.filter(d => d.zone === searchModel.zone)
  }
  return data
})

// ============================================================
//  合计行
// ============================================================
const getSummary = ({ columns, data }) => {
  const sums = []
  columns.forEach((column, index) => {
    if (index === 0) {
      sums[index] = '合计'
      return
    }
    if (index === 1 || index === 2) {
      sums[index] = ''
      return
    }
    const prop = column.property
    if (['total', 'used', 'idle', 'locked', 'maintaining'].includes(prop)) {
      sums[index] = data.reduce((s, d) => s + (Number(d[prop]) || 0), 0).toLocaleString()
    } else if (prop === 'utilization') {
      const totalUsed = data.reduce((s, d) => s + d.used, 0)
      const totalAll = data.reduce((s, d) => s + d.total, 0)
      sums[index] = totalAll ? (totalUsed / totalAll * 100).toFixed(1) + '%' : '0%'
    } else {
      sums[index] = ''
    }
  })
  return sums
}

// ============================================================
//  辅助方法
// ============================================================
const getUtilizationColor = (percentage) => {
  if (percentage >= 90) return '#e86452'
  if (percentage >= 70) return '#5b8ff9'
  if (percentage >= 50) return '#f6bd16'
  return '#86909c'
}

const handleSearch = () => {
  KhMessageFn.success('查询完成')
}

const handleReset = () => {
  searchModel.warehouse = ''
  searchModel.zone = ''
  KhMessageFn.success('已重置')
}

const handleExport = () => {
  KhMessageFn.success('导出数据（功能待接入）')
}
</script>

<style scoped>
.location-report {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.report-stats {
  margin-bottom: 0;
}

.report-stats :deep(.el-card) {
  border: none;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}

.report-search {
  padding: 16px 20px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}

.report-chart-section {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}

.report-chart-title {
  font-size: 15px;
  font-weight: 600;
  color: #1d2129;
  margin-bottom: 16px;
}

.report-chart-bars {
  display: flex;
  align-items: flex-end;
  gap: 6px;
  height: 200px;
  padding: 0 4px;
}

.report-chart-bar-item {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  height: 100%;
  min-width: 0;
}

.report-chart-bar-value-top {
  font-size: 11px;
  color: #5b8ff9;
  font-weight: 600;
  margin-bottom: 4px;
}

.report-chart-bar-wrapper {
  flex: 1;
  width: 100%;
  display: flex;
  align-items: flex-end;
  justify-content: center;
}

.report-chart-bar-stacked {
  width: 80%;
  max-width: 36px;
  min-height: 2px;
  display: flex;
  flex-direction: column;
  border-radius: 2px 2px 0 0;
  overflow: hidden;
}

.report-chart-bar-segment {
  width: 100%;
  min-height: 0;
  transition: height 0.3s ease;
}

.segment-used {
  background: #5b8ff9;
}

.segment-locked {
  background: #f6bd16;
}

.segment-maint {
  background: #e86452;
}

.segment-idle {
  background: #c9d1d9;
}

.report-chart-bar-label {
  font-size: 11px;
  color: #86909c;
  margin-top: 6px;
  white-space: nowrap;
}

.report-chart-legend {
  display: flex;
  justify-content: center;
  gap: 24px;
  margin-top: 12px;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: #86909c;
}

.legend-dot {
  display: inline-block;
  width: 10px;
  height: 10px;
  border-radius: 2px;
}

.report-table {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}
</style>
