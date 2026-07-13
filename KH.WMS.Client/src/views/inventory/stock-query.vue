<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage ref="pageRef" module="inventory-header" title="库存查询" :stat-cards="statCards" :search-columns="searchColumns"
      :search-model="searchModel" :columns="tableColumns" :show-stat-cards="true" :show-toolbar="true"
      :show-index="true" :show-header-filter="true" :search-col-count="3" :permission-prefix="'inv:query'"
      :crud-operations="crudOperations" :detail-lines="detailLineConfigs" :detail-width="'1000px'"
      :action-buttons="actionButtons" :action-width="'150'" />

    <!-- 冻结原因弹窗 -->
    <KhDialog ref="freezeDialogRef" title="冻结库存" width="420px"
      :description="freezeDescription"
      confirm-text="确认冻结"
      :form-columns="freezeFormColumns"
      :form-model="freezeFormModel"
      @confirm="handleFreezeConfirm" />
  </div>
</template>

<script setup>
import { getInventoryStatData, freezeInventory, unfreezeInventory } from '@/api/inventory'
import KhDialog from '@/components/KhDialog/index.vue'

const pageRef = ref(null)

const Icons = markRaw({ Box, Check, Lock, IceCreamRound })

// ==================== 统计卡片 ====================
const statData = reactive({
  totalCount: 0,
  availableCount: 0,
  lockedCount: 0,
  frozenCount: 0,
})

const statCards = computed(() => [
  { label: '总托盘数', value: statData.totalCount, icon: Icons.Box, theme: 'primary' },
  { label: '可用', value: statData.availableCount, icon: Icons.Check, theme: 'success' },
  { label: '锁定', value: statData.lockedCount, icon: Icons.Lock, theme: 'warning' },
  { label: '冻结', value: statData.frozenCount, icon: Icons.IceCreamRound, theme: 'danger' },
])

const loadStatData = async () => {
  try {
    const res = await getInventoryStatData()
    if (res.code == 200) {
      statData.totalCount = res.data.totalCount
      statData.availableCount = res.data.availableCount
      statData.lockedCount = res.data.lockedCount
      statData.frozenCount = res.data.frozenCount
    }
  } catch (error) {
    console.error('加载库存统计数据失败:', error)
  }
}

onMounted(async () => {
  await loadStatData()
})

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'containerCode', label: '容器编号', type: 'input', clearable: true },
  { prop: 'locationCode', label: '库位编码', type: 'input', clearable: true },
  {
    prop: 'inventoryStatus', label: '库存状态', type: 'select', clearable: true,
    options: 'dict:inventory_status',
  },
]

const searchModel = reactive({
  containerCode: '',
  locationCode: '',
  inventoryStatus: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'containerCode', label: '容器编号', width: 140, fixed: 'left' },
  { prop: 'locationCode', label: '库位编码', width: 160 },
  {
    prop: 'inventoryStatus', label: '库存状态', width: 100, align: 'center',
    type: 'tag', tagMap: 'dict:inventory_status',
  },
  { prop: 'detailCount', label: '物料种数', width: 90, align: 'center' },
  { prop: 'inboundTime', label: '入库时间', width: 170 },
  { prop: 'lastInboundTime', label: '最后入库时间', width: 170 },
  { prop: 'lastStocktakeTime', label: '最后盘点时间', width: 170 },
  { prop: 'remark', label: '备注', minWidth: 120, showOverflowTooltip: true },
]

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: false,
  update: false,
  delete: false,
  view: true,
  export: true,
}

// ==================== 冻结/解冻操作 ====================
const freezeDialogRef = ref(null)
const freezeTargetRow = ref(null)

const freezeDescription = computed(() => {
  if (!freezeTargetRow.value) return ''
  return `冻结托盘：${freezeTargetRow.value.containerCode}（库位：${freezeTargetRow.value.locationCode || '-'}）`
})

const freezeFormColumns = [
  { prop: 'reason', label: '冻结原因', type: 'textarea', required: true, maxlength: 300, rows: 3 },
]

const freezeFormModel = reactive({ reason: '' })

const actionButtons = [
  {
    label: '冻结',
    type: 'danger',
    permission: 'inv:query:freeze',
    show: (row) => row.inventoryStatus !== 'FROZEN' && row.inventoryStatus !== 'LOCKED',
    onClick: (row) => {
      freezeTargetRow.value = row
      freezeFormModel.reason = ''
      freezeDialogRef.value?.open()
    },
  },
  {
    label: '解冻',
    type: 'success',
    permission: 'inv:query:unfreeze',
    show: (row) => row.inventoryStatus === 'FROZEN',
    confirm: '确认解冻该托盘？解冻后库存恢复正常可用。',
    onClick: async (row) => {
      try {
        const res = await unfreezeInventory(row.id)
        if (res?.code === 200) {
          KhMessageFn.success(res?.message || '解冻成功')
          await loadStatData()
          pageRef.value?.reload()
        } else {
          KhMessageFn.error(res?.message || '解冻失败')
        }
      } catch (error) {
        KhMessageFn.error('解冻失败: ' + error.message)
      }
    },
  },
]

const handleFreezeConfirm = async (formData) => {
  try {
    const res = await freezeInventory(freezeTargetRow.value.id, formData.reason)
    if (res.code === 200) {
      KhMessageFn.success(res.message || '冻结成功')
      freezeDialogRef.value?.close()
      await loadStatData()
      pageRef.value?.reload()
    } else {
      KhMessageFn.error(res.message || '冻结失败')
    }
  } catch (error) {
    KhMessageFn.error('冻结失败: ' + error.message)
  }
}

// ==================== 详情行配置（托盘下的物料明细） ====================
const detailLineConfigs = [
  {
    prop: 'details',
    title: '托盘物料明细',
    columns: [
      { prop: 'materialCode', label: '物料编码', width: 130 },
      { prop: 'batchNo', label: '批次号', width: 120 },
      { prop: 'qty', label: '数量', width: 100, align: 'right' },
      { prop: 'lockedQty', label: '锁定数量', width: 100, align: 'right' },
      { prop: 'unit', label: '单位', width: 70, align: 'center' },
      { prop: 'productionDate', label: '生产日期', width: 120 },
      { prop: 'expiryDate', label: '过期日期', width: 120 },
      { prop: 'inboundDocNo', label: '入库单号', width: 160 },
      { prop: 'remark', label: '备注', minWidth: 120 },
    ],
  },
]
</script>
