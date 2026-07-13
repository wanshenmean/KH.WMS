<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage ref="pageRef" title="作业执行日志" :search-columns="searchColumns" :search-model="searchModel" :columns="tableColumns"
      :load="loadFn" :show-stat-cards="false" :show-toolbar="true" :show-index="true" :search-col-count="3">
      <template #toolbar-left>
        <el-popconfirm title="确定清理所有历史日志?" @confirm="handleCleanLog">
          <template #reference>
            <el-button type="danger">清理日志</el-button>
          </template>
        </el-popconfirm>
      </template>
      <template #action="{ row }">
        <el-button type="primary" link size="small" @click="handleViewDetail(row)">查看详情</el-button>
      </template>
    </KhPage>

    <!-- 详情弹窗 -->
    <KhDialog v-model="detailVisible" title="执行日志详情" width="650px" :show-footer="false">
      <div class="job-log-detail">
        <div class="job-log-detail__item">
          <span class="job-log-detail__label">作业名称：</span>
          <span>{{ detailData.jobName }}</span>
        </div>
        <div class="job-log-detail__item">
          <span class="job-log-detail__label">执行方式：</span>
          <span>{{ detailData.execMethod }}</span>
        </div>
        <div class="job-log-detail__item">
          <span class="job-log-detail__label">开始时间：</span>
          <span>{{ detailData.startTime }}</span>
        </div>
        <div class="job-log-detail__item">
          <span class="job-log-detail__label">结束时间：</span>
          <span>{{ detailData.endTime }}</span>
        </div>
        <div class="job-log-detail__item">
          <span class="job-log-detail__label">耗时：</span>
          <span>{{ detailData.duration }}</span>
        </div>
        <div class="job-log-detail__item">
          <span class="job-log-detail__label">执行状态：</span>
          <el-tag :type="statusTagType(detailData.execStatus)">{{ statusLabel(detailData.execStatus) }}</el-tag>
        </div>
        <div class="job-log-detail__item job-log-detail__item--block">
          <span class="job-log-detail__label">执行结果：</span>
          <el-input type="textarea" :model-value="detailData.result" :rows="6" readonly />
        </div>
      </div>
    </KhDialog>
  </div>
</template>

<script setup>
import KhPage from '@/components/KhPage/index.vue'
import KhDialog from '@/components/KhDialog/index.vue'

const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'jobName', label: '作业名称', type: 'input', clearable: true },
  {
    prop: 'execStatus', label: '执行状态', type: 'select', clearable: true,
    options: [
      { label: '成功', value: 'success' },
      { label: '失败', value: 'failed' },
      { label: '执行中', value: 'running' },
    ],
  },
  { prop: 'dateRange', label: '时间范围', type: 'daterange', clearable: true },
]

const searchModel = reactive({ jobName: '', execStatus: '', dateRange: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'jobName', label: '作业名称', width: 180 },
  { prop: 'execMethod', label: '执行方式', width: 100, align: 'center' },
  { prop: 'startTime', label: '开始时间', width: 170 },
  { prop: 'endTime', label: '结束时间', width: 170 },
  { prop: 'duration', label: '耗时', width: 100, align: 'center' },
  {
    prop: 'execStatus', label: '执行状态', width: 100, align: 'center',
    type: 'tag', tagMap: { success: '成功', failed: '失败', running: '执行中' },
    tagTypeMap: { success: 'success', failed: 'danger', running: 'warning' },
  },
  { prop: 'result', label: '执行结果', minWidth: 180, showOverflowTooltip: true },
]

// ==================== Mock 数据 ====================
const mockData = ref([
  {
    id: 1, jobName: '库存同步作业', execMethod: '定时触发',
    startTime: '2026-04-16 02:00:00', endTime: '2026-04-16 02:03:25', duration: '3分25秒',
    execStatus: 'success',
    result: '库存同步完成，共同步物料数据 1,256 条，库位数据 3,480 条，库存记录 15,320 条。',
  },
  {
    id: 2, jobName: '数据清理作业', execMethod: '定时触发',
    startTime: '2026-04-16 03:00:00', endTime: '2026-04-16 03:01:12', duration: '1分12秒',
    execStatus: 'success',
    result: '数据清理完成，共清理90天前的操作日志 2,340 条，临时数据 856 条。',
  },
  {
    id: 3, jobName: '报表生成作业', execMethod: '手动触发',
    startTime: '2026-04-15 18:30:00', endTime: '2026-04-15 18:32:45', duration: '2分45秒',
    execStatus: 'failed',
    result: '报表生成失败：连接报表服务器超时，错误码 ETIMEDOUT，请检查网络连接后重试。详细堆栈: java.net.SocketTimeoutException: connect timed out at com.report.client.ReportService.connect(ReportService.java:128)',
  },
  {
    id: 4, jobName: '库存预警检查', execMethod: '定时触发',
    startTime: '2026-04-16 01:00:00', endTime: '-', duration: '-',
    execStatus: 'running',
    result: '正在执行库存预警检查，已检查仓库 WH01...',
  },
])

// ==================== 数据加载 ====================
const loadFn = async (params) => {
  await new Promise(r => setTimeout(r, 400))
  let filtered = [...mockData.value]
  if (params.jobName) filtered = filtered.filter(d => d.jobName.includes(params.jobName))
  if (params.execStatus) filtered = filtered.filter(d => d.execStatus === params.execStatus)
  const start = (params.pageNum - 1) * params.pageSize
  return {
    data: filtered.slice(start, start + params.pageSize),
    total: filtered.length,
  }
}

// ==================== 详情弹窗 ====================
const detailVisible = ref(false)
const detailData = ref({})

const handleViewDetail = (row) => {
  detailData.value = { ...row }
  detailVisible.value = true
}

const statusTagType = (status) => {
  const map = { success: 'success', failed: 'danger', running: 'warning' }
  return map[status] || 'info'
}

const statusLabel = (status) => {
  const map = { success: '成功', failed: '失败', running: '执行中' }
  return map[status] || status
}

// ==================== 清理日志 ====================
const handleCleanLog = () => {
  mockData.value = []
  KhMessageFn.success('日志清理成功')
  pageRef.value?.reload()
}
</script>

<style scoped>
.job-log-detail {
  padding: 8px 0;
}

.job-log-detail__item {
  display: flex;
  align-items: flex-start;
  padding: 10px 0;
  border-bottom: 1px solid #f0f0f0;
}

.job-log-detail__item:last-child {
  border-bottom: none;
}

.job-log-detail__item--block {
  flex-direction: column;
  gap: 8px;
}

.job-log-detail__label {
  width: 100px;
  flex-shrink: 0;
  color: #909399;
  font-size: 14px;
}
</style>
