<template>
  <KhDashboard
    ref="dashboardRef"
    title="仓储运营数据大屏"
    :stats="dashboardStats"
    :charts="dashboardCharts"
    :stat-span="6"
  />
</template>

<script setup>
import KhDashboard from '@/components/KhDashboard/index.vue'

// ============================================================
//  统计卡片
// ============================================================
const dashboardStats = ref([
  { label: '今日入库', value: '1,286', icon: markRaw(Upload), color: '#5ad8a6' },
  { label: '今日出库', value: '1,058', icon: markRaw(Box), color: '#5b8ff9' },
  { label: '库存总量', value: '286,350', icon: markRaw(Goods), color: '#f6bd16' },
  { label: '待处理任务', value: '53', icon: markRaw(List), color: '#e86452' },
])

// ============================================================
//  图表配置
// ============================================================
const dashboardCharts = ref([
  // 折线图 - 近7天出入库趋势
  {
    type: 'line',
    title: '近7天出入库趋势',
    span: 12,
    option: {
      grid: { left: '3%', right: '4%', bottom: '3%', top: '12%', containLabel: true },
      xAxis: {
        type: 'category',
        data: ['04-03', '04-04', '04-05', '04-06', '04-07', '04-08', '04-09'],
        axisLine: { lineStyle: { color: '#30363d' } },
        axisLabel: { color: '#8b949e' },
      },
      yAxis: {
        type: 'value',
        axisLine: { show: false },
        axisTick: { show: false },
        splitLine: { lineStyle: { color: 'rgba(48, 54, 61, 0.6)' } },
        axisLabel: { color: '#8b949e' },
      },
      tooltip: {
        trigger: 'axis',
        axisPointer: { type: 'cross' },
      },
      legend: {
        data: ['入库', '出库'],
        top: 0,
      },
      series: [
        {
          name: '入库',
          type: 'line',
          smooth: true,
          data: [1120, 980, 1350, 1050, 860, 1180, 1286],
          areaStyle: {
            color: {
              type: 'linear',
              x: 0, y: 0, x2: 0, y2: 1,
              colorStops: [
                { offset: 0, color: 'rgba(90, 216, 166, 0.3)' },
                { offset: 1, color: 'rgba(90, 216, 166, 0.02)' },
              ],
            },
          },
          lineStyle: { width: 2 },
          symbol: 'circle',
          symbolSize: 6,
        },
        {
          name: '出库',
          type: 'line',
          smooth: true,
          data: [960, 870, 1200, 1100, 780, 1040, 1058],
          areaStyle: {
            color: {
              type: 'linear',
              x: 0, y: 0, x2: 0, y2: 1,
              colorStops: [
                { offset: 0, color: 'rgba(91, 143, 249, 0.3)' },
                { offset: 1, color: 'rgba(91, 143, 249, 0.02)' },
              ],
            },
          },
          lineStyle: { width: 2 },
          symbol: 'circle',
          symbolSize: 6,
        },
      ],
    },
  },
  // 柱状图 - 各仓库库存分布
  {
    type: 'bar',
    title: '各仓库库存分布',
    span: 12,
    option: {
      grid: { left: '3%', right: '4%', bottom: '3%', top: '12%', containLabel: true },
      xAxis: {
        type: 'category',
        data: ['A区-原材料仓', 'B区-成品仓', 'C区-半成品仓', 'D区-备件仓'],
        axisLine: { lineStyle: { color: '#30363d' } },
        axisLabel: { color: '#8b949e', fontSize: 11 },
      },
      yAxis: {
        type: 'value',
        axisLine: { show: false },
        axisTick: { show: false },
        splitLine: { lineStyle: { color: 'rgba(48, 54, 61, 0.6)' } },
        axisLabel: { color: '#8b949e' },
      },
      tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
      series: [
        {
          type: 'bar',
          data: [98500, 82300, 65400, 40150],
          barWidth: 36,
          itemStyle: {
            borderRadius: [4, 4, 0, 0],
            color: (params) => {
              const colors = [
                ['#5b8ff9', '#6dc8ec'],
                ['#5ad8a6', '#36b37e'],
                ['#f6bd16', '#f09000'],
                ['#e86452', '#d9363e'],
              ]
              const [c1, c2] = colors[params.dataIndex % colors.length]
              return {
                type: 'linear', x: 0, y: 0, x2: 0, y2: 1,
                colorStops: [{ offset: 0, color: c1 }, { offset: 1, color: c2 }],
              }
            },
          },
          label: {
            show: true,
            position: 'top',
            color: '#c9d1d9',
            fontSize: 11,
            formatter: '{c}',
          },
        },
      ],
    },
  },
  // 饼图 - 物料分类占比
  {
    type: 'pie',
    title: '物料分类占比',
    span: 8,
    option: {
      tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
      legend: {
        orient: 'vertical',
        right: '5%',
        top: 'center',
        textStyle: { color: '#8b949e', fontSize: 12 },
      },
      series: [
        {
          type: 'pie',
          radius: ['40%', '65%'],
          center: ['35%', '50%'],
          avoidLabelOverlap: false,
          label: { show: false },
          emphasis: {
            label: { show: true, fontSize: 14, fontWeight: 'bold' },
          },
          data: [
            { value: 42800, name: '电子元器件' },
            { value: 35600, name: '机械零部件' },
            { value: 28400, name: '五金工具' },
            { value: 21500, name: '包装材料' },
            { value: 18200, name: '化工原料' },
          ],
        },
      ],
    },
  },
  // 仪表盘 - 库位利用率
  {
    type: 'gauge',
    title: '库位利用率',
    span: 8,
    option: {
      series: [
        {
          type: 'gauge',
          startAngle: 200,
          endAngle: -20,
          min: 0,
          max: 100,
          splitNumber: 10,
          itemStyle: { color: '#5b8ff9' },
          progress: {
            show: true,
            width: 14,
            itemStyle: {
              color: {
                type: 'linear',
                x: 0, y: 0, x2: 1, y2: 0,
                colorStops: [
                  { offset: 0, color: '#5ad8a6' },
                  { offset: 0.5, color: '#f6bd16' },
                  { offset: 1, color: '#e86452' },
                ],
              },
            },
          },
          pointer: {
            itemStyle: { color: 'auto' },
            width: 4,
            length: '60%',
          },
          axisLine: {
            lineStyle: {
              width: 14,
              color: [[1, 'rgba(48, 54, 61, 0.5)']],
            },
          },
          axisTick: { show: false },
          splitLine: { show: false },
          axisLabel: { show: false },
          title: {
            show: true,
            offsetCenter: [0, '70%'],
            color: '#8b949e',
            fontSize: 13,
          },
          detail: {
            valueAnimation: true,
            fontSize: 26,
            offsetCenter: [0, '40%'],
            color: '#e6edf3',
            formatter: '{value}%',
          },
          data: [{ value: 78.6, name: '综合利用率' }],
        },
      ],
    },
  },
  // 排名 - 库区周转TOP
  {
    type: 'rank',
    title: '库区周转率TOP排名',
    span: 8,
    data: [
      { name: 'A1区-原材料', value: 5.8 },
      { name: 'B2区-成品', value: 5.2 },
      { name: 'C1区-半成品', value: 4.6 },
      { name: 'A2区-原材料', value: 4.1 },
      { name: 'B1区-成品', value: 3.9 },
      { name: 'D1区-备件', value: 2.8 },
      { name: 'C2区-半成品', value: 2.3 },
      { name: 'D2区-备件', value: 1.6 },
    ],
  },
])

// ============================================================
//  引用
// ============================================================
const dashboardRef = ref(null)
</script>

<style scoped>
/* KhDashboard 已有暗色主题样式，此处无需额外样式 */
</style>
