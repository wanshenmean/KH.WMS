<template>
  <KhPage ref="pageRef" title="出库分配" module="outbound-allocation" :search-columns="searchColumns"
    :search-model="searchModel" :columns="tableColumns" :show-stat-cards="false" :show-toolbar="true" :show-index="true"
    :show-header-filter="true" :search-col-count="3"
    :crud-operations="{ create: false, update: false, delete: false, view: true, export: false }"
    :permission-prefix="'out:allocation'" :action-buttons="actionButtons" :detail-lines="detailLineConfigs" />
</template>

<script setup>
import { generateAllocationTasks } from '@/api/outbound'

const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'outboundOrderNo', label: '出库单号', type: 'input', clearable: true },
  {
    prop: 'allocStatus', label: '分配状态', type: 'select', clearable: true,
    options: 'dict:allocation_status',
  },
  {
    prop: 'strategyType', label: '策略类型', type: 'select', clearable: true,
    options: 'dict:strategy_type',
  },
]

const searchModel = reactive({
  outboundOrderNo: '',
  allocStatus: '',
  strategyType: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'outboundOrderNo', label: '出库单号', width: 160 },
  {
    prop: 'allocStatus', label: '分配状态', width: 100, align: 'center',
    type: 'tag', tagMap: 'dict:allocation_status',
  },
  {
    prop: 'strategyType', label: '策略类型', width: 120, align: 'center',
    type: 'tag', tagMap: 'dict:strategy_type',
  },
  { prop: 'warehouseCode', label: '仓库编码', width: 100, align: 'center' },
  { prop: 'totalLines', label: '分配行数', width: 90, align: 'center' },
  { prop: 'allocTime', label: '分配时间', width: 170, align: 'center' },
  { prop: 'completeTime', label: '完成时间', width: 170, align: 'center' },
  { prop: 'remark', label: '备注', minWidth: 120, showOverflowTooltip: true },
]

// ==================== 详情从表配置 ====================
const detailLineConfigs = [
  {
    prop: 'details',
    title: '出库明细行',
    columns: [
      { prop: 'materialCode', label: '物料编码', minWidth: 130 },
      { prop: 'materialName', label: '物料名称', minWidth: 160 },
      { prop: 'containerCode', label: '容器编号', width: 120, align: 'right' },
      { prop: 'locationCode', label: '库位编码', width: 120, align: 'right' },
      { prop: 'allocQty', label: '分配数量', width: 120, align: 'right' },
      { prop: 'pickedQty', label: '已拣数量', width: 120, align: 'right' },
      { prop: 'batchNo', label: '批次号', width: 120 },
      { prop: 'remark', label: '备注', minWidth: 120 },
    ],
  },
]

// ==================== 操作按钮 ====================
const actionButtons = [
  {
    label: '生成出库任务',
    type: 'warning',
    permission: 'out:allocation:generate_task',
    show: (row) => row.allocStatus === 'ALLOCATED',
    confirm: '确认为该分配单生成出库搬运任务？',
    onClick: async (row) => {
      try {
        const res = await generateAllocationTasks(row.id)
        KhMessageFn.success(res.message || '出库任务生成成功')
        pageRef.value?.reload()
      } catch {
        // request.js 已处理错误提示
      }
    },
  },
]
</script>
