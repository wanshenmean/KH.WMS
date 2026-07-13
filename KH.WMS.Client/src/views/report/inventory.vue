<template>
  <div class="inventory-report">
    <h2 class="page-title">库存报表</h2>

    <!-- 统计卡片 -->
    <el-row :gutter="16" class="report-stats">
      <el-col :span="6">
        <KhStatCard :value="statTotalQty" label="库存总量" :icon="markRaw(Box)" theme="primary" :formatter="v => v.toLocaleString() + ' 件'" />
      </el-col>
      <el-col :span="6">
        <KhStatCard :value="statSkuCount" label="SKU数" :icon="markRaw(Goods)" theme="success" />
      </el-col>
      <el-col :span="6">
        <KhStatCard :value="statAmount" label="库存金额" :icon="markRaw(Money)" theme="warning" :formatter="v => '¥' + (v / 10000).toFixed(1) + '万'" />
      </el-col>
      <el-col :span="6">
        <KhStatCard :value="statTurnover" label="周转率" :icon="markRaw(TrendCharts)" theme="info" :formatter="v => v.toFixed(1) + '次/月'" />
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
        <el-form-item label="物料分类">
          <el-select v-model="searchModel.category" placeholder="请选择分类" clearable>
            <el-option v-for="c in categoryOptions" :key="c" :label="c" :value="c" />
          </el-select>
        </el-form-item>
        <el-form-item label="ABC分类">
          <el-select v-model="searchModel.abcClass" placeholder="请选择ABC分类" clearable>
            <el-option label="A类（高价值）" value="A" />
            <el-option label="B类（中价值）" value="B" />
            <el-option label="C类（低价值）" value="C" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :icon="markRaw(Search)" @click="handleSearch">查询</el-button>
          <el-button :icon="markRaw(Refresh)" @click="handleReset">重置</el-button>
          <el-button :icon="markRaw(Download)" @click="handleExport">导出</el-button>
        </el-form-item>
      </el-form>
    </div>

    <!-- 分类占比柱状图 -->
    <div class="report-chart-section">
      <div class="report-chart-title">各仓库分类库存金额分布</div>
      <div class="report-chart-bars">
        <div
          v-for="(item, idx) in chartData"
          :key="idx"
          class="report-chart-bar-item"
        >
          <div class="report-chart-bar-wrapper">
            <div
              class="report-chart-bar"
              :style="{ height: `${(item.amount / maxAmount) * 100}%` }"
            >
              <span class="report-chart-bar-value">{{ (item.amount / 10000).toFixed(0) }}万</span>
            </div>
          </div>
          <div class="report-chart-bar-label">{{ item.label }}</div>
        </div>
      </div>
    </div>

    <!-- 明细表格 -->
    <div class="report-table">
      <el-table :data="filteredTableData" border stripe style="width: 100%" show-summary :summary-method="getSummary">
        <el-table-column type="index" label="序号" width="60" align="center" />
        <el-table-column prop="warehouse" label="仓库" width="140" align="center" />
        <el-table-column prop="category" label="物料分类" width="120" align="center" />
        <el-table-column prop="skuCount" label="SKU数" width="90" align="right" />
        <el-table-column prop="qty" label="库存数量" width="100" align="right" />
        <el-table-column prop="amount" label="库存金额" width="120" align="right">
          <template #default="{ row }">
            {{ '¥' + row.amount.toLocaleString() }}
          </template>
        </el-table-column>
        <el-table-column prop="ratio" label="占比" width="100" align="center">
          <template #default="{ row }">
            <el-progress :percentage="row.ratio" :stroke-width="10" :color="getProgressColor(row.ratio)" />
          </template>
        </el-table-column>
        <el-table-column prop="turnoverDays" label="周转天数" width="100" align="right" />
        <el-table-column prop="abcClass" label="ABC分类" width="110" align="center">
          <template #default="{ row }">
            <el-tag :type="row.abcClass === 'A' ? 'danger' : row.abcClass === 'B' ? 'warning' : 'info'" effect="light" size="small">
              {{ row.abcClass }}类
            </el-tag>
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
const statTotalQty = ref(286350)
const statSkuCount = ref(1286)
const statAmount = ref(45800000)
const statTurnover = ref(3.8)

// ============================================================
//  查询条件
// ============================================================
const warehouseOptions = ['A区-原材料仓', 'B区-成品仓', 'C区-半成品仓', 'D区-备件仓']
const categoryOptions = ['电子元器件', '机械零部件', '五金工具', '包装材料', '化工原料']

const searchModel = reactive({
  warehouse: '',
  category: '',
  abcClass: '',
})

// ============================================================
//  模拟数据
// ============================================================
function generateData() {
  const warehouses = ['A区-原材料仓', 'B区-成品仓', 'C区-半成品仓', 'D区-备件仓']
  const categories = ['电子元器件', '机械零部件', '五金工具', '包装材料', '化工原料']
  const abcClasses = ['A', 'B', 'C']
  const data = []
  let id = 0

  warehouses.forEach(w => {
    categories.forEach(c => {
      id++
      const abcClass = abcClasses[id % 3]
      const skuCount = Math.floor(Math.random() * 80) + 20
      const qty = Math.floor(Math.random() * 20000) + 1000
      const amount = Math.floor(Math.random() * 5000000) + 500000
      const turnoverDays = Math.floor(Math.random() * 60) + 10
      data.push({
        id,
        warehouse: w,
        category: c,
        skuCount,
        qty,
        amount,
        ratio: 0, // will be calculated
        turnoverDays,
        abcClass,
      })
    })
  })

  const totalAmount = data.reduce((s, d) => s + d.amount, 0)
  data.forEach(d => {
    d.ratio = Math.round((d.amount / totalAmount) * 100)
  })

  return data
}

const allData = ref(generateData())

// ============================================================
//  图表数据
// ============================================================
const chartData = computed(() => {
  return allData.value.map(d => ({
    label: d.category.length > 4 ? d.category.slice(0, 4) : d.category,
    amount: d.amount,
  }))
})
const maxAmount = computed(() => Math.max(...chartData.value.map(d => d.amount), 1))

// ============================================================
//  筛选逻辑
// ============================================================
const filteredTableData = computed(() => {
  let data = allData.value
  if (searchModel.warehouse) {
    data = data.filter(d => d.warehouse === searchModel.warehouse)
  }
  if (searchModel.category) {
    data = data.filter(d => d.category === searchModel.category)
  }
  if (searchModel.abcClass) {
    data = data.filter(d => d.abcClass === searchModel.abcClass)
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
    if (index === 1) {
      sums[index] = ''
      return
    }
    const prop = column.property
    if (['skuCount', 'qty', 'amount'].includes(prop)) {
      const val = data.reduce((s, d) => s + (Number(d[prop]) || 0), 0)
      sums[index] = prop === 'amount' ? '¥' + val.toLocaleString() : val.toLocaleString()
    } else {
      sums[index] = ''
    }
  })
  return sums
}

// ============================================================
//  辅助方法
// ============================================================
const getProgressColor = (percentage) => {
  if (percentage >= 10) return '#e86452'
  if (percentage >= 5) return '#f6bd16'
  return '#5b8ff9'
}

const handleSearch = () => {
  KhMessageFn.success('查询完成')
}

const handleReset = () => {
  searchModel.warehouse = ''
  searchModel.category = ''
  searchModel.abcClass = ''
  KhMessageFn.success('已重置')
}

const handleExport = () => {
  KhMessageFn.success('导出数据（功能待接入）')
}
</script>

<style scoped>
.inventory-report {
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
  gap: 8px;
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
  max-width: 40px;
  min-height: 2px;
  background: linear-gradient(180deg, #f6bd16 0%, #f09000 100%);
  border-radius: 2px 2px 0 0;
  position: relative;
  transition: height 0.3s ease;
}

.report-chart-bar:hover {
  background: linear-gradient(180deg, #ffd666 0%, #f6bd16 100%);
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
  font-size: 11px;
  color: #86909c;
  margin-top: 6px;
  white-space: nowrap;
}

.report-table {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}
</style>
