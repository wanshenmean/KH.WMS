<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage
      ref="pageRef"
      title="无单据出库"
      module="inventory-header"
      :search-columns="searchColumns"
      :columns="tableColumns"
      :show-stat-cards="true"
      :show-search="true"
      :show-toolbar="true"
      :show-index="true"
      :show-header-filter="true"
      :search-col-count="4"
      :stat-span="6"
      :crud-operations="crudOperations"
      :action-buttons="[]"
    >
      <template #toolbar-left>
        <el-button type="primary" @click="handleOpenOutbound">
          <el-icon><Upload /></el-icon> 无单据出库
        </el-button>
      </template>
    </KhPage>

    <!-- 无单据出库弹窗 -->
    <KhDialog
      v-model="dialogVisible"
      title="无单据出库"
      width="1100px"
      destroy-on-close
      :confirm-loading="submitLoading"
      @confirm="handleConfirmOutbound"
      @close="handleDialogClose"
    >
      <template #default>
        <!-- 出库方式 -->
        <div style="margin-bottom: 16px;">
          <span style="font-weight: 600; margin-right: 16px;">出库方式：</span>
          <el-radio-group v-model="outboundMode" @change="handleModeChange">
            <el-radio-button
              v-for="item in outboundModeOptions"
              :key="item.value"
              :value="item.value"
            >
              {{ item.label }}
            </el-radio-button>
          </el-radio-group>
        </div>

        <!-- 指定出库口 -->
        <div v-if="outboundMode === 'port'" style="margin-bottom: 16px; display: flex; align-items: center;">
          <span style="width: 80px; flex-shrink: 0;">出库口：</span>
          <el-select
            v-model="portId"
            filterable
            placeholder="请选择出库口"
            style="width: 300px;"
          >
            <el-option
              v-for="item in portOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </div>

        <!-- 指定出库区域 -->
        <div v-if="outboundMode === 'outbound_zone'" style="margin-bottom: 16px; display: flex; align-items: center;">
          <span style="width: 80px; flex-shrink: 0;">出库区域：</span>
          <el-select
            v-model="zoneId"
            filterable
            placeholder="请选择出库区域"
            style="width: 300px;"
          >
            <el-option
              v-for="item in zoneOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </div>

        <!-- 已选库存表格 -->
        <div v-if="dialogRows.length > 0" style="margin-top: 8px;">
          <span style="font-weight: 600; margin-bottom: 8px; display: block;">
            已选库存（填写出库数量）
          </span>
          <el-table :data="dialogRows" border max-height="360" style="width: 100%;">
            <el-table-column prop="containerCode" label="容器编号" width="130" />
            <el-table-column prop="locationCode" label="货位编码" width="130" />
            <el-table-column prop="materialCode" label="物料编码" width="130" />
            <el-table-column prop="materialName" label="物料名称" min-width="150" show-overflow-tooltip />
            <el-table-column prop="batchNo" label="批次号" width="120" />
            <el-table-column prop="qty" label="库存数量" width="80" align="right" />
            <el-table-column prop="lockedQty" label="锁定数量" width="80" align="right" />
            <el-table-column label="出库数量" width="130" align="center">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.outboundQty"
                  :min="0"
                  :max="row.availableQty"
                  :precision="0"
                  :step="1"
                  size="small"
                  controls-position="right"
                  style="width: 100%"
                />
              </template>
            </el-table-column>
          </el-table>
        </div>
      </template>
    </KhDialog>
  </div>
</template>

<script setup>
import { getZonesByWarehouse, getPortPageList } from '@/api/warehouse'
import {
  adhocOutbound,
  adhocOutboundByContainer,
  adhocOutboundByLocation,
  adhocOutboundByZone,
  adhocOutboundByAisle,
  adhocOutboundByPort,
} from '@/api/adhoc'

const pageRef = ref(null)

// ==================== 搜索配置 ====================
const searchColumns = [
  { prop: 'containerCode', label: '容器编号', type: 'input', clearable: true },
  { prop: 'locationCode', label: '货位编码', type: 'input', clearable: true },
  { prop: 'materialCode', label: '物料编码', type: 'input', clearable: true },
  {
    prop: 'inventoryStatus', label: '库存状态', type: 'select', clearable: true,
    options: 'dict:inventory_status',
  },
]

// ==================== 表格列配置 ====================
const tableColumns = [
  { prop: 'containerCode', label: '容器编号', width: 130 },
  { prop: 'locationCode', label: '货位编码', width: 130 },
  { prop: 'materialCode', label: '物料编码', width: 130 },
  { prop: 'materialName', label: '物料名称', minWidth: 150 },
  { prop: 'batchNo', label: '批次号', width: 120 },
  { prop: 'qty', label: '库存数量', width: 80, align: 'right' },
  { prop: 'lockedQty', label: '锁定数量', width: 80, align: 'right' },
  {
    prop: 'inventoryStatus', label: '库存状态', width: 100,
    type: 'tag', tagMap: 'dict:inventory_status',
  },
  { prop: 'inboundTime', label: '入库时间', width: 170 },
]

// ==================== CRUD 配置 ====================
const crudOperations = { create: false, update: false, delete: false, view: false, export: false }

// ==================== 出库方式选项 ====================
const outboundModeOptions = [
  { label: '指定容器出库', value: 'container' },
  { label: '指定货位出库', value: 'location' },
  { label: '指定区域+物料', value: 'zone_material' },
  { label: '指定出库口', value: 'port' },
  { label: '指定出库区域', value: 'outbound_zone' },
  { label: '指定巷道', value: 'aisle' },
]

// ==================== 弹窗状态 ====================
const dialogVisible = ref(false)
const submitLoading = ref(false)

// ==================== 弹窗表单数据 ====================
const outboundMode = ref('container')
const portId = ref(null)
const zoneId = ref(null)
const dialogRows = ref([])

// ==================== 远程下拉选项 ====================
const portOptions = ref([])
const zoneOptions = ref([])

// ==================== 远程加载方法 ====================
const loadPorts = async (warehouseId) => {
  if (!warehouseId) return
  try {
    const res = await getPortPageList({
      pageIndex: 1,
      pageSize: 100,
      filters: [{ field: 'warehouseId', value: warehouseId, operator: 'eq' }],
    })
    const list = res.data?.rows || res.data || []
    portOptions.value = list.map(p => ({ label: p.portName || p.portCode, value: p.portCode }))
  } catch {
    portOptions.value = []
  }
}

const loadZones = async (warehouseId) => {
  if (!warehouseId) return
  try {
    const res = await getZonesByWarehouse(warehouseId)
    const list = res.data || []
    // 后端需要 zoneCode 而非 id
    zoneOptions.value = list.map(z => ({ label: z.zoneName, value: z.zoneCode }))
  } catch {
    zoneOptions.value = []
  }
}

// ==================== 出库方式变更 ====================
const handleModeChange = () => {
  portId.value = null
  zoneId.value = null
}

// ==================== 打开出库弹窗 ====================
const handleOpenOutbound = () => {
  const rows = pageRef.value?.getSelectionRows() || []
  if (rows.length === 0) {
    KhMessageFn.warning('请先在列表中选择需要出库的库存')
    return
  }

  // 从选中行提取仓库ID（取第一行的仓库，要求所有行同仓库）
  const warehouseIds = [...new Set(rows.map(r => r.warehouseId).filter(Boolean))]
  if (warehouseIds.length > 1) {
    KhMessageFn.warning('所选库存属于不同仓库，请统一筛选后再选择')
    return
  }
  if (warehouseIds.length === 0 || !warehouseIds[0]) {
    KhMessageFn.warning('所选库存缺少仓库信息')
    return
  }

  const warehouseId = warehouseIds[0]

  // 准备弹窗数据：计算可用数量，默认出库数量=可用数量
  dialogRows.value = rows.map(item => {
    const availableQty = (item.qty || 0) - (item.lockedQty || 0)
    return {
      ...item,
      availableQty: Math.max(0, availableQty),
      outboundQty: Math.max(0, availableQty),
    }
  })

  // 重置弹窗表单
  outboundMode.value = 'container'
  portId.value = null
  zoneId.value = null
  portOptions.value = []
  zoneOptions.value = []

  dialogVisible.value = true

  // 加载出库口和出库区域选项（按需）
  loadPorts(warehouseId)
  loadZones(warehouseId)
}

// ==================== 确认出库 ====================
const handleConfirmOutbound = async () => {
  const validRows = dialogRows.value.filter(row => row.outboundQty > 0)
  if (validRows.length === 0) {
    KhMessageFn.warning('请填写出库数量')
    return
  }

  const exceeded = validRows.find(row => row.outboundQty > row.availableQty)
  if (exceeded) {
    KhMessageFn.warning('出库数量不能超过可用数量')
    return
  }

  // 指定出库口必须选择
  if (outboundMode.value === 'port' && !portId.value) {
    KhMessageFn.warning('请选择出库口')
    return
  }
  // 指定出库区域必须选择
  if (outboundMode.value === 'outbound_zone' && !zoneId.value) {
    KhMessageFn.warning('请选择出库区域')
    return
  }

  const mode = outboundMode.value
  const warehouseId = dialogRows.value[0].warehouseId

  // 构建选择行
  const selectedLines = validRows.map(row => ({
    inventoryDetailId: row.detailId || row.id,
    qty: row.outboundQty,
  }))

  let apiFn = adhocOutbound
  let payload = { warehouseId, lines: selectedLines }

  if (mode === 'container') {
    apiFn = adhocOutboundByContainer
    payload = { warehouseId, containerCode: dialogRows.value[0].containerCode, allOutbound: true }
  } else if (mode === 'location') {
    apiFn = adhocOutboundByLocation
    payload = { warehouseId, locationCode: dialogRows.value[0].locationCode, allOutbound: true }
  } else if (mode === 'zone_material') {
    apiFn = adhocOutboundByZone
    payload = { warehouseId, zoneCode: dialogRows.value[0].zoneCode, allOutbound: true }
  } else if (mode === 'outbound_zone') {
    apiFn = adhocOutboundByZone
    payload = { warehouseId, zoneCode: zoneId.value, allOutbound: true }
  } else if (mode === 'aisle') {
    apiFn = adhocOutboundByAisle
    payload = { warehouseId, aisleCode: dialogRows.value[0].aisleCode, allOutbound: true }
  } else if (mode === 'port') {
    apiFn = adhocOutboundByPort
    payload = { warehouseId, portCode: portId.value, allOutbound: true }
  }

  submitLoading.value = true
  try {
    await apiFn(payload)
    KhMessageFn.success('出库任务创建成功')
    dialogVisible.value = false
    pageRef.value?.clearSelection()
    pageRef.value?.reload()
  } catch {
    // request.js 已处理错误提示
  } finally {
    submitLoading.value = false
  }
}

// ==================== 弹窗关闭 ====================
const handleDialogClose = () => {
  dialogRows.value = []
  portOptions.value = []
  zoneOptions.value = []
}
</script>
