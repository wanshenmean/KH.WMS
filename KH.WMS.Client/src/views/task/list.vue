<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage ref="pageRef" title="任务管理" module="task-header" :search-columns="searchColumns"
      :search-model="searchModel" :columns="tableColumns" :show-stat-cards="false" :show-toolbar="true"
      :show-index="true" :show-header-filter="true" :search-col-count="3"
      :crud-operations="{ create: false, update: false, delete: false, view: true, export: true }"
      :permission-prefix="'task:header'" :action-buttons="actionButtons"
      :detail-lines="detailLineConfigs" :detail-width="'1200px'" :action-width="'200'">
    </KhPage>
  </div>
</template>

<script setup>
import { completeTaskByWcs, cancelTask, allocatePutawayLocation } from '@/api/task'

const pageRef = ref(null)

// ==================== 状态/优先级映射（统一使用字典） ====================

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'taskNo', label: '任务编号', type: 'input', clearable: true },
  {
    prop: 'taskType', label: '任务类型', type: 'select', clearable: true,
    options: 'dict:task_type',
  },
  {
    prop: 'taskStatus', label: '状态', type: 'select', clearable: true,
    options: 'dict:task_status',
  },
  {
    prop: 'taskPriority', label: '优先级', type: 'select', clearable: true,
    options: 'dict:task_priority',
  },
]

const searchModel = reactive({
  taskNo: '',
  taskType: '',
  taskStatus: '',
  taskPriority: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'taskNo', label: '任务编号', width: 160, fixed: 'left' },
  {
    prop: 'taskType', label: '任务类型', width: 100, align: 'center',
    type: 'tag', tagMap: 'dict:task_type',
  },
  {
    prop: 'taskPriority', label: '优先级', width: 90, align: 'center',
    type: 'tag', tagMap: 'dict:task_priority',
  },
  {
    prop: 'taskStatus', label: '状态', width: 100, align: 'center',
    type: 'tag', tagMap: 'dict:task_status',
  },
  { prop: 'containerNo', label: '容器编号', width: 120 },
  { prop: 'fromLocationCode', label: '起始库位', width: 120 },
  { prop: 'toLocationCode', label: '目标库位', width: 120 },
  { prop: 'docNo', label: '来源单号', width: 160 },
  { prop: 'equipmentCode', label: '执行设备', width: 120 },
  { prop: 'totalLines', label: '物料行数', width: 90, align: 'center' },
  { prop: 'startTime', label: '开始时间', width: 170 },
  { prop: 'createdTime', label: '创建时间', width: 170 },
  { prop: 'remark', label: '备注', minWidth: 120, showOverflowTooltip: true },
]

// ==================== 操作按钮 ====================
const actionButtons = [
  {
    label: '分配货位',
    permission: 'task:header:allocate',
    type: 'primary',
    onClick: (row) => handleAllocate(row),
    show: (row) => row.taskType === 'PUTAWAY' && row.locationAllocated === 0
      && (row.taskStatus === 'PENDING' || row.taskStatus === 'IN_PROGRESS'),
  },
  {
    label: '完成',
    permission: 'task:header:complete',
    type: 'success',
    onClick: (row) => handleComplete(row),
    show: (row) => row.taskStatus === 'PENDING' || row.taskStatus === 'IN_PROGRESS',
  },
  {
    label: '取消',
    permission: 'task:header:cancel',
    onClick: (row) => handleCancel(row),
    show: (row) => row.taskStatus === 'PENDING',
  },
]

// ==================== 详情行配置 ====================
const detailLineConfigs = [
  {
    prop: 'lines',
    title: '任务行（托盘物料信息）',
    columns: [
      { prop: 'lineNo', label: '行号', width: 70, align: 'center' },
      { prop: 'materialCode', label: '物料编码', width: 130 },
      { prop: 'materialName', label: '物料名称', minWidth: 160 },
      { prop: 'batchNo', label: '批次号', width: 110 },
      { prop: 'remark', label: '备注', minWidth: 120 },
    ],
  },
]

// ==================== 操作方法 ====================
const handleRefresh = () => {
  pageRef.value?.reload()
}

const handleComplete = (row) => {
  KhMsgBoxFn.confirm(`确认完成任务 ${row.taskNo} 吗？`, '完成确认', {
    confirmButtonText: '确认',
    cancelButtonText: '取消',
    type: 'warning',
  }).then(async () => {
    const res = await completeTaskByWcs({ taskNo: row.taskNo })
    if (res.code === 200) {
      KhMessageFn.success(res.message || '任务完成成功')
      pageRef.value?.reload()
    } else {
      KhMessageFn.error(res.message || '任务完成失败')
    }
  }).catch(() => {})
}

const handleCancel = (row) => {
  KhMsgBoxFn.confirm(`确认取消任务 ${row.taskNo} 吗？取消后关联的组盘、库位、容器状态将被回滚。`, '取消确认', {
    confirmButtonText: '确认',
    cancelButtonText: '取消',
    type: 'warning',
  }).then(async () => {
    const res = await cancelTask(row.id)
    if (res.code === 200) {
      KhMessageFn.success(res.message || '任务取消成功')
      pageRef.value?.reload()
    } else {
      KhMessageFn.error(res.message || '任务取消失败')
    }
  }).catch(() => {})
}

const handleAllocate = (row) => {
  KhMsgBoxFn.confirm(`确认为上架任务 ${row.taskNo} 分配目标货位吗？`, '货位分配', {
    confirmButtonText: '确认',
    cancelButtonText: '取消',
    type: 'info',
  }).then(async () => {
    const res = await allocatePutawayLocation(row.id)
    if (res.code === 200) {
      KhMessageFn.success(res.message || '货位分配成功')
      pageRef.value?.reload()
    } else {
      KhMessageFn.error(res.message || '货位分配失败')
    }
  }).catch(() => {})
}
</script>

<style scoped></style>
