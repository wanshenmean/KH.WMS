<template>
  <div class="kh-dashboard">
    <!-- Title Bar -->
    <div class="kh-dashboard__header">
      <div class="kh-dashboard__header-border kh-dashboard__header-border--left"></div>
      <h1 class="kh-dashboard__title">{{ title }}</h1>
      <div class="kh-dashboard__header-border kh-dashboard__header-border--right"></div>
    </div>

    <!-- Stats Row -->
    <div v-if="stats && stats.length" class="kh-dashboard__stats">
      <el-row :gutter="16">
        <el-col
          v-for="(stat, index) in stats"
          :key="index"
          :span="statSpan"
        >
          <div class="kh-dashboard__stat-card" :style="{ '--accent': stat.color || '#5b8ff9' }">
            <div class="kh-dashboard__stat-icon" v-if="stat.icon">
              <el-icon :size="28">
                <component :is="stat.icon" />
              </el-icon>
            </div>
            <div class="kh-dashboard__stat-info">
              <div class="kh-dashboard__stat-value">{{ stat.value }}</div>
              <div class="kh-dashboard__stat-label">{{ stat.label }}</div>
            </div>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- Charts Grid -->
    <div v-if="charts && charts.length" class="kh-dashboard__charts">
      <el-row :gutter="16">
        <el-col
          v-for="(chart, index) in charts"
          :key="index"
          :span="chart.span || 12"
        >
          <div class="kh-dashboard__chart-card">
            <div v-if="chart.title" class="kh-dashboard__chart-title">
              <span class="kh-dashboard__chart-title-dot"></span>
              {{ chart.title }}
            </div>
            <div
              :ref="(el) => setChartRef(el, index)"
              class="kh-dashboard__chart-body"
            ></div>
          </div>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script setup>

// ECharts tree-shaking imports
import * as echarts from 'echarts/core'
import { LineChart, BarChart, PieChart, GaugeChart, ScatterChart } from 'echarts/charts'
import {
  TitleComponent,
  TooltipComponent,
  LegendComponent,
  GridComponent,
  DatasetComponent,
  TransformComponent,
  ToolboxComponent,
  DataZoomComponent,
  VisualMapComponent,
  GraphicComponent,
} from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { LabelLayout } from 'echarts/features'

echarts.use([
  LineChart,
  BarChart,
  PieChart,
  GaugeChart,
  ScatterChart,
  TitleComponent,
  TooltipComponent,
  LegendComponent,
  GridComponent,
  DatasetComponent,
  TransformComponent,
  ToolboxComponent,
  DataZoomComponent,
  VisualMapComponent,
  GraphicComponent,
  CanvasRenderer,
  LabelLayout,
])

const props = defineProps({
  /** Dashboard title */
  title: { type: String, default: 'WMS 数据大屏' },
  /** Top stat cards config: [{ label, value, icon, color }] */
  stats: { type: Array, default: () => [] },
  /** Chart configs: [{ type, title, span, option, data }] */
  charts: { type: Array, default: () => [] },
  /** Stat card col span, default 6 (4 per row) */
  statSpan: { type: Number, default: 6 },
})

/** Chart DOM element refs keyed by index */
const chartRefs = {}
const chartInstances = []

/** Store chart DOM element refs */
function setChartRef(el, index) {
  if (el) {
    chartRefs[index] = el
  }
}

/** Default dark-theme color palette */
const darkColors = ['#5b8ff9', '#5ad8a6', '#f6bd16', '#e86452', '#6dc8ec', '#945fb9', '#ff9845', '#1e9493']

/** Default dark-theme base styles applied to every chart option */
function applyDarkTheme(option) {
  return {
    color: darkColors,
    backgroundColor: 'transparent',
    textStyle: { color: '#c9d1d9' },
    title: {
      ...(option.title || {}),
      textStyle: { color: '#e6edf3', ...(option.title?.textStyle || {}) },
    },
    tooltip: {
      backgroundColor: 'rgba(22, 27, 34, 0.95)',
      borderColor: '#30363d',
      textStyle: { color: '#e6edf3' },
      ...(option.tooltip || {}),
    },
    legend: {
      textStyle: { color: '#8b949e' },
      ...(option.legend || {}),
    },
    ...option,
  }
}

/**
 * Build a rank chart (horizontal bar) option from data array.
 * @param {Array<{ name: string, value: number }>} data
 * @returns {Object} echarts option
 */
function buildRankOption(data) {
  if (!data || !data.length) return {}

  // Sort descending and reverse so the largest appears at the top visually
  const sorted = [...data].sort((a, b) => a.value - b.value)

  return {
    grid: {
      left: '3%',
      right: '12%',
      top: '4%',
      bottom: '4%',
      containLabel: true,
    },
    xAxis: {
      type: 'value',
      axisLine: { show: false },
      axisTick: { show: false },
      splitLine: { lineStyle: { color: 'rgba(48, 54, 61, 0.6)' } },
      axisLabel: { color: '#8b949e' },
    },
    yAxis: {
      type: 'category',
      data: sorted.map(item => item.name),
      axisLine: { show: false },
      axisTick: { show: false },
      axisLabel: { color: '#c9d1d9', fontSize: 13 },
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: { type: 'shadow' },
    },
    series: [
      {
        type: 'bar',
        data: sorted.map(item => item.value),
        barWidth: 18,
        showBackground: true,
        backgroundStyle: {
          color: 'rgba(48, 54, 61, 0.5)',
          borderRadius: [0, 4, 4, 0],
        },
        itemStyle: {
          borderRadius: [0, 4, 4, 0],
          color: new echarts.graphic.LinearGradient(0, 0, 1, 0, [
            { offset: 0, color: '#5b8ff9' },
            { offset: 1, color: '#6dc8ec' },
          ]),
        },
        label: {
          show: true,
          position: 'right',
          color: '#c9d1d9',
          fontSize: 12,
          formatter: '{c}',
        },
      },
    ],
  }
}

/** Initialize all chart instances */
function initCharts() {
  disposeCharts()

  if (!props.charts || !props.charts.length) return

  props.charts.forEach((chart, index) => {
    const dom = chartRefs[index]
    if (!dom) return

    let option

    if (chart.type === 'rank') {
      option = applyDarkTheme(buildRankOption(chart.data))
    } else {
      option = applyDarkTheme(chart.option || {})
    }

    const instance = echarts.init(dom)
    instance.setOption(option)
    chartInstances.push(instance)
  })
}

/** Dispose all chart instances */
function disposeCharts() {
  chartInstances.forEach(instance => {
    if (instance && !instance.isDisposed()) {
      instance.dispose()
    }
  })
  chartInstances.length = 0
}

/** Resize all chart instances */
function resizeCharts() {
  chartInstances.forEach(instance => {
    if (instance && !instance.isDisposed()) {
      instance.resize()
    }
  })
}

// ---------- ResizeObserver ----------
let resizeObserver = null

onMounted(() => {
  nextTick(() => {
    initCharts()

    // Use ResizeObserver for container-level resize detection
    const dashboardEl = document.querySelector('.kh-dashboard')
    if (dashboardEl && typeof ResizeObserver !== 'undefined') {
      resizeObserver = new ResizeObserver(() => {
        resizeCharts()
      })
      resizeObserver.observe(dashboardEl)
    }

    // Also listen to window resize as a fallback
    window.addEventListener('resize', resizeCharts)
  })
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', resizeCharts)

  if (resizeObserver) {
    resizeObserver.disconnect()
    resizeObserver = null
  }

  disposeCharts()
})

defineExpose({
  /** Re-initialize all charts (useful after data changes) */
  refresh: initCharts,
  /** Resize all chart instances */
  resize: resizeCharts,
  /** Get all echarts instances */
  getChartInstances: () => [...chartInstances],
})
</script>

<style scoped>
.kh-dashboard {
  width: 100%;
  min-height: 100vh;
  padding: 0 20px 20px;
  background: linear-gradient(145deg, #0d1117 0%, #161b22 50%, #0d1117 100%);
  box-sizing: border-box;
  overflow: hidden;
}

/* ==================== Header ==================== */
.kh-dashboard__header {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px 40px;
  position: relative;
}

.kh-dashboard__title {
  margin: 0;
  font-size: 30px;
  font-weight: 700;
  color: #e6edf3;
  letter-spacing: 6px;
  text-shadow:
    0 0 10px rgba(91, 143, 249, 0.4),
    0 0 30px rgba(91, 143, 249, 0.15);
  white-space: nowrap;
  position: relative;
  z-index: 1;
}

.kh-dashboard__header-border {
  flex: 1;
  height: 2px;
  position: relative;
}

.kh-dashboard__header-border::after {
  content: '';
  position: absolute;
  top: 0;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(91, 143, 249, 0.6), transparent);
}

.kh-dashboard__header-border--left {
  margin-right: 30px;
  background: linear-gradient(90deg, transparent, rgba(91, 143, 249, 0.3));
}

.kh-dashboard__header-border--right {
  margin-left: 30px;
  background: linear-gradient(90deg, rgba(91, 143, 249, 0.3), transparent);
}

/* ==================== Stats ==================== */
.kh-dashboard__stats {
  margin-bottom: 16px;
}

.kh-dashboard__stat-card {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 18px 20px;
  background: rgba(22, 27, 34, 0.75);
  border: 1px solid rgba(48, 54, 61, 0.7);
  border-radius: 8px;
  backdrop-filter: blur(8px);
  transition: border-color 0.3s ease, box-shadow 0.3s ease;
}

.kh-dashboard__stat-card:hover {
  border-color: var(--accent, rgba(91, 143, 249, 0.5));
  box-shadow: 0 0 16px rgba(91, 143, 249, 0.1);
}

.kh-dashboard__stat-icon {
  width: 46px;
  height: 46px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  flex-shrink: 0;
  background: linear-gradient(135deg, var(--accent, #5b8ff9), var(--accent, #5b8ff9));
  opacity: 0.9;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
}

.kh-dashboard__stat-info {
  flex: 1;
  min-width: 0;
}

.kh-dashboard__stat-value {
  font-size: 26px;
  font-weight: 700;
  color: var(--accent, #5b8ff9);
  line-height: 1.3;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  letter-spacing: -0.5px;
}

.kh-dashboard__stat-label {
  font-size: 13px;
  color: #8b949e;
  margin-top: 2px;
}

/* ==================== Charts ==================== */
.kh-dashboard__charts {
  margin-top: 4px;
}

.kh-dashboard__chart-card {
  background: rgba(22, 27, 34, 0.75);
  border: 1px solid rgba(48, 54, 61, 0.7);
  border-radius: 8px;
  padding: 16px;
  backdrop-filter: blur(8px);
  transition: border-color 0.3s ease;
}

.kh-dashboard__chart-card:hover {
  border-color: rgba(91, 143, 249, 0.35);
}

.kh-dashboard__chart-title {
  font-size: 15px;
  font-weight: 600;
  color: #e6edf3;
  margin-bottom: 12px;
  padding-bottom: 10px;
  border-bottom: 1px solid rgba(48, 54, 61, 0.7);
  display: flex;
  align-items: center;
  gap: 8px;
}

.kh-dashboard__chart-title-dot {
  display: inline-block;
  width: 4px;
  height: 16px;
  border-radius: 2px;
  background: linear-gradient(180deg, #5b8ff9, #6dc8ec);
  flex-shrink: 0;
}

.kh-dashboard__chart-body {
  width: 100%;
  height: 300px;
}

/* ==================== Responsive ==================== */
@media (max-width: 1200px) {
  .kh-dashboard__title {
    font-size: 24px;
    letter-spacing: 4px;
  }

  .kh-dashboard__stat-value {
    font-size: 22px;
  }

  .kh-dashboard__chart-body {
    height: 260px;
  }
}

@media (max-width: 768px) {
  .kh-dashboard {
    padding: 0 10px 10px;
  }

  .kh-dashboard__header {
    padding: 14px 16px;
  }

  .kh-dashboard__title {
    font-size: 20px;
    letter-spacing: 2px;
  }

  .kh-dashboard__header-border--left,
  .kh-dashboard__header-border--right {
    display: none;
  }

  .kh-dashboard__chart-body {
    height: 220px;
  }
}
</style>
