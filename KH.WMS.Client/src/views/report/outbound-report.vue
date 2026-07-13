<template>
  <div class="outbound-report">
    <h2 class="page-title">出库报表</h2>

    <!-- 统计卡片 -->
    <el-row :gutter="16" class="report-stats">
      <el-col :span="6">
        <KhStatCard :value="statToday" label="今日出库量" :icon="markRaw(Box)" theme="primary" />
      </el-col>
      <el-col :span="6">
        <KhStatCard :value="statWeek" label="本周出库量" :icon="markRaw(TrendCharts)" theme="success" />
      </el-col>
      <el-col :span="6">
        <KhStatCard :value="statMonth" label="本月出库量" :icon="markRaw(DataAnalysis)" theme="warning" />
      </el-col>
      <el-col :span="6">
        <KhStatCard :value="statOrderCount" label="出库单数" :icon="markRaw(Document)" theme="info" />
      </el-col>
    </el-row>

    <!-- 查询表单 -->
    <div class="report-search">
      <el-form :model="searchModel" inline>
        <el-form-item label="时间范围">
          <el-date-picker
            v-model="searchModel.dateRange"
            type="daterange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            value-format="YYYY-MM-DD"
            clearable
          />
        </el-form-item>
        <el-form-item label="仓库">
          <el-select v-model="searchModel.warehouse" placeholder="请选择仓库" clearable>
            <el-option v-for="w in warehouseOptions" :key="w" :label="w" :value="w" />
          </el-select>
        </el-form-item>
        <el-form-item label="客户">
          <el-select v-model="searchModel.customer" placeholder="请选择客户" clearable>
            <el-option v-for="c in customerOptions" :key="c" :label="c" :value="c" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :icon="markRaw(Search)" @click="handleSearch">查询</el-button>
          <el-button :icon="markRaw(Refresh)" @click="handleReset">重置</el-button>
          <el-button :icon="markRaw(Download)" @click="handleExport">导出</el-button>
        </el-form-item>
      </el-form>
    </div>

    <!-- 简易趋势图（纯CSS柱状图） -->
    <div class="report-chart-section">
      <div class="report-chart-title">近30天出库趋势</div>
      <div class="report-chart-bars">
        <div
          v-for="(item, idx) in chartData"
          :key="idx"
          class="report-chart-bar-item"
        >
          <div class="report-chart-bar-wrapper">
            <div
              class="report-chart-bar"
              :style="{ height: `${(item.totalQty / maxQty) * 100}%` }"
            >
              <span class="report-chart-bar-value">{{ item.totalQty }}</span>
            </div>
          </div>
          <div class="report-chart-bar-label">{{ item.date.slice(5) }}</div>
        </div>
      </div>
    </div>

    <!-- 明细表格 -->
    <div class="report-table">
      <el-table :data="filteredTableData" border stripe style="width: 100%">
        <el-table-column type="index" label="序号" width="60" align="center" />
        <el-table-column prop="date" label="日期" width="120" align="center" />
        <el-table-column prop="orderCount" label="出库单数" width="100" align="right" />
        <el-table-column prop="materialCount" label="出库物料数" width="120" align="right" />
        <el-table-column prop="totalQty" label="出库总量" width="120" align="right" />
        <el-table-column prop="customerCount" label="客户数" width="100" align="right" />
      </el-table>
    </div>
  </div>
</template>

<script setup>
import KhStatCard from '@/components/KhStatCard/index.vue'

// ============================================================
//  统计数据
// ============================================================
const statToday = ref(1058)
const statWeek = ref(7321)
const statMonth = ref(29840)
const statOrderCount = ref(398)

// ============================================================
//  查询条件
// ============================================================
const warehouseOptions = ['A区-原材料仓', 'B区-成品仓', 'C区-半成品仓', 'D区-备件仓']
const customerOptions = ['华为技术', '比亚迪汽车', '富士康科技', '中兴通讯', '美的集团', '格力电器', '海尔智家', '三一重工']

const searchModel = reactive({
  dateRange: null,
  warehouse: '',
  customer: '',
})

// ============================================================
//  模拟30天数据
// ============================================================
function generateDailyData() {
  const data = []
  const today = new Date('2025-04-09')
  for (let i = 29; i >= 0; i--) {
    const d = new Date(today)
    d.setDate(d.getDate() - i)
    const y = d.getFullYear()
    const m = String(d.getMonth() + 1).padStart(2, '0')
    const day = String(d.getDate()).padStart(2, '0')
    const dateStr = `${y}-${m}-${day}`
    const isWeekend = d.getDay() === 0 || d.getDay() === 6
    const base = isWeekend ? 150 : 700
    const orderCount = Math.floor(Math.random() * 10) + (isWeekend ? 1 : 6)
    const materialCount = Math.floor(Math.random() * 25) + (isWeekend ? 3 : 15)
    const totalQty = Math.floor(Math.random() * base) + (isWeekend ? 80 : 500)
    const customerCount = Math.floor(Math.random() * 5) + (isWeekend ? 1 : 3)
    data.push({ date: dateStr, orderCount, materialCount, totalQty, customerCount })
  }
  return data
}

const allData = ref(generateDailyData())
const chartData = computed(() => allData.value.slice(-30))
const maxQty = computed(() => Math.max(...chartData.value.map(d => d.totalQty), 1))

// ============================================================
//  筛选逻辑
// ============================================================
const filteredTableData = computed(() => {
  let data = allData.value
  if (searchModel.dateRange && searchModel.dateRange.length === 2) {
    const [start, end] = searchModel.dateRange
    data = data.filter(d => d.date >= start && d.date <= end)
  }
  return data
})

// ============================================================
//  操作方法
// ============================================================
const handleSearch = () => {
  KhMessageFn.success('查询完成')
}

const handleReset = () => {
  searchModel.dateRange = null
  searchModel.warehouse = ''
  searchModel.customer = ''
  KhMessageFn.success('已重置')
}

const handleExport = () => {
  KhMessageFn.success('导出数据（功能待接入）')
}
</script>

<style scoped>
.outbound-report {
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
  gap: 4px;
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

.report-chart-bar-wrapper {
  flex: 1;
  width: 100%;
  display: flex;
  align-items: flex-end;
  justify-content: center;
}

.report-chart-bar {
  width: 80%;
  max-width: 20px;
  min-height: 2px;
  background: linear-gradient(180deg, #5ad8a6 0%, #36b37e 100%);
  border-radius: 2px 2px 0 0;
  position: relative;
  transition: height 0.3s ease;
}

.report-chart-bar:hover {
  background: linear-gradient(180deg, #7ce8c0 0%, #5ad8a6 100%);
}

.report-chart-bar-value {
  position: absolute;
  top: -18px;
  left: 50%;
  transform: translateX(-50%);
  font-size: 10px;
  color: #86909c;
  white-space: nowrap;
}

.report-chart-bar-label {
  font-size: 10px;
  color: #86909c;
  margin-top: 4px;
  white-space: nowrap;
}

.report-table {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}
</style>
