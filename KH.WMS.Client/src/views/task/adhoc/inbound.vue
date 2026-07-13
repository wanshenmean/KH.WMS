<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage
      ref="pageRef"
      title="无单据入库"
      module="inbound-container-bind"
      :search-columns="searchColumns"
      :search-model="searchModel"
      :columns="tableColumns"
      :show-stat-cards="true"
      :show-search="true"
      :show-toolbar="true"
      :show-index="true"
      :show-header-filter="true"
      :search-col-count="3"
      :stat-span="6"
      :crud-operations="crudOperations"
      :action-buttons="actionButtons"
      :toolbar-buttons="toolbarButtons"
      :detail-lines="detailLineConfigs"
      :detail-width="'900px'"
      :permission-prefix="'task:adhoc_inbound'"
      :search-params-transform="searchParamsTransform"
    >
    </KhPage>

    <!-- 无单据组盘入库弹窗 -->
    <KhDialog
      v-model="inboundDialogVisible"
      title="无单据组盘入库"
      width="900px"
      destroy-on-close
      :confirm-loading="submitLoading"
      @confirm="handleInboundSubmit"
      @close="resetInboundForm"
    >
      <template #default>
        <KhForm
          ref="inboundFormRef"
          :columns="inboundFormColumns"
          v-model="inboundForm"
          :label-width="'90px'"
          :col-count="2"
        />

        <el-divider content-position="left">
          <span style="font-size: 13px; color: #606266;">物料明细</span>
        </el-divider>

        <KhEditableTable
          v-model="inboundLines"
          :columns="inboundLineColumns"
          :default-row="createEmptyInboundLine"
          :max-height="360"
          add-text="添加行"
          :action-width="70"
        />
      </template>
    </KhDialog>

    <!-- 指定地址上架弹窗 -->
    <KhDialog
      v-model="putawayDialogVisible"
      title="指定地址上架"
      width="900px"
      destroy-on-close
      :confirm-loading="submitLoading"
      @confirm="handlePutawaySubmit"
      @close="handlePutawayDialogClose"
    >
      <template #default>
        <!-- 已选组盘记录 -->
        <div style="margin-bottom: 16px;">
          <div style="font-weight: 600; margin-bottom: 8px; font-size: 14px; color: #303133;">
            已选组盘记录（{{ putawayRows.length }} 条）
          </div>
          <el-table :data="putawayRows" border size="small" max-height="200" style="width: 100%;">
            <el-table-column type="index" label="#" width="50" align="center" />
            <el-table-column prop="containerCode" label="容器编号" min-width="150" show-overflow-tooltip />
            <el-table-column prop="detailCount" label="明细数" width="80" align="center" />
            <el-table-column prop="bindTime" label="组盘时间" width="160" align="center" />
          </el-table>
        </div>

        <!-- 目标位置表单 -->
        <KhForm
          ref="putawayFormRef"
          :columns="putawayFormColumns"
          v-model="putawayForm"
          :label-width="'90px'"
          :col-count="2"
        />
      </template>
    </KhDialog>
  </div>
</template>

<script setup>
import {
  adhocInbound,
  adhocPutawayTo,
} from '@/api/adhoc'
import { getStorageZones, getAvailableLocationsByZone } from '@/api/warehouse'
import { deleteContainerBind } from '@/api/inbound'

const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'containerCode', label: '容器编号', type: 'input', clearable: true },
  {
    prop: 'bindStatus',
    label: '组盘状态',
    type: 'select',
    clearable: true,
    options: 'dict:bind_status',
  },
]

const searchModel = reactive({
  containerCode: '',
  bindStatus: '',
})

const searchParamsTransform = (params) => {
  return { ...params, sourceType: 'ADHOC' }
}

// ==================== CRUD 配置 ====================
const crudOperations = { create: false, update: false, delete: false, view: true, export: false }

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'containerCode', label: '容器编号', width: 160 },
  {
    prop: 'bindStatus',
    label: '组盘状态',
    width: 120,
    align: 'center',
    type: 'tag',
    tagMap: 'dict:bind_status',
  },
  { prop: 'detailCount', label: '明细数', width: 100, align: 'center' },
  { prop: 'bindTime', label: '组盘时间', width: 170, align: 'center' },
  {
    prop: 'warehouseCode',
    label: '仓库',
    width: 120,
    align: 'center',
  },
]

// ==================== 详情弹窗从表配置 ====================
const detailLineConfigs = [
  {
    prop: 'details',
    title: '组盘明细',
    columns: [
      { prop: 'materialCode', label: '物料编码', minWidth: 130 },
      { prop: 'materialName', label: '物料名称', minWidth: 160, showOverflowTooltip: true },
      { prop: 'qty', label: '数量', width: 100, align: 'right' },
      { prop: 'batchNo', label: '批次号', width: 120 },
      { prop: 'remark', label: '备注', minWidth: 120 },
    ],
  },
]

// ==================== 取消组盘 ====================
const handleCancel = async (row) => {
  try {
    await deleteContainerBind(row.id)
    KhMessageFn.success('取消组盘成功')
    pageRef.value?.reload()
  } catch {
    // request.js 已处理错误提示
  }
}

// ==================== 操作列按钮 ====================
const actionButtons = [
  // {
  //   label: '查看',
  //   icon: markRaw(View),
  //   permission: 'task:adhoc_inbound:view',
  //   onClick: (row) => pageRef.value?.showDetail(row),
  // },
  {
    label: '取消组盘',
    type: 'danger',
    icon: markRaw(Delete),
    permission: 'task:adhoc_inbound:delete',
    onClick: (row) => handleCancel(row),
    show: (row) => row.bindStatus === 'BOUND',
  },
]

// ==================== 弹窗通用状态 ====================
const inboundDialogVisible = ref(false)
const putawayDialogVisible = ref(false)
const inboundFormRef = ref(null)
const putawayFormRef = ref(null)
const submitLoading = ref(false)

// ==================== 组盘弹窗 ====================
const inboundForm = reactive({
  warehouseId: '',
  containerCode: '',
})

const inboundLines = ref([])

const inboundFormColumns = [
  {
    prop: 'warehouseId',
    label: '仓库',
    type: 'select',
    required: true,
    options: 'dict:warehouse_list',
  },
  {
    prop: 'containerCode',
    label: '容器编号',
    type: 'input',
    required: true,
  },
]

const inboundLineColumns = [
  {
    prop: 'materialCode',
    label: '物料编码',
    type: 'select',
    minWidth: 130,
    options: 'dict:material_list',
    placeholder: '选择物料',
  },
  { prop: 'materialName', label: '物料名称', type: 'input', minWidth: 130, disabled: true },
  {
    prop: 'qty',
    label: '数量',
    type: 'number',
    width: 100,
    options: { min: 1, precision: 0 },
  },
  { prop: 'batchNo', label: '批次号', type: 'input', width: 130 },
]

const createEmptyInboundLine = () => ({
  materialId: 0,
  materialCode: '',
  materialName: '',
  qty: 1,
  batchNo: '',
})

const handleOpenInbound = () => {
  inboundForm.warehouseId = ''
  inboundForm.containerCode = ''
  inboundLines.value = []
  inboundDialogVisible.value = true
}

const handleInboundSubmit = async () => {
  // 校验表单
  if (!inboundFormRef.value) return
  const valid = await inboundFormRef.value.validate().catch(() => false)
  if (!valid) return

  // 校验物料明细
  if (inboundLines.value.length === 0) {
    KhMessageFn.warning('请添加至少一条物料明细')
    return
  }
  const invalidLine = inboundLines.value.find((l) => !l.materialCode || !l.qty || l.qty <= 0)
  if (invalidLine) {
    KhMessageFn.warning('物料明细中存在编码为空或数量不大于0的行')
    return
  }

  submitLoading.value = true
  try {
    await adhocInbound({
      warehouseId: inboundForm.warehouseId,
      containerCode: inboundForm.containerCode,
      lines: inboundLines.value,
    })
    KhMessageFn.success('组盘成功')
    inboundDialogVisible.value = false
    pageRef.value?.reload()
  } catch {
    // request.js 已处理错误提示
  } finally {
    submitLoading.value = false
  }
}

const resetInboundForm = () => {
  inboundForm.warehouseId = ''
  inboundForm.containerCode = ''
  inboundLines.value = []
}

// ==================== 指定地址上架弹窗 ====================
const putawayForm = reactive({
  toZoneCode: '',
  toLocationCode: '',
})

const putawayRows = ref([])
const putawayZoneOptions = ref([])
const putawayLocationOptions = ref([])

const putawayFormColumns = computed(() => [
  {
    prop: 'toZoneCode',
    label: '目标区域',
    type: 'select',
    required: true,
    options: putawayZoneOptions.value,
    placeholder: '请选择区域',
    filterable: true,
    onChange: async (value) => {
      putawayForm.toLocationCode = ''
      putawayLocationOptions.value = []
      if (value) {
        const warehouseId = putawayRows.value.length > 0 ? putawayRows.value[0].warehouseId : null
        if (!warehouseId) return
        try {
          const res = await getAvailableLocationsByZone(warehouseId, value)
          const list = res.data || []
          putawayLocationOptions.value = list.map(l => ({ label: l.locationCode, value: l.locationCode }))
        } catch {
          putawayLocationOptions.value = []
        }
      }
    },
  },
  {
    prop: 'toLocationCode',
    label: '目标货位',
    type: 'select',
    required: true,
    options: putawayLocationOptions.value,
    placeholder: '请先选择区域',
    filterable: true,
  },
])

// ==================== 上架弹窗操作 ====================
const handleOpenPutaway = () => {
  const rows = pageRef.value?.getSelectionRows() || []
  if (rows.length === 0) {
    KhMessageFn.warning('请先在列表中选择需要上架的组盘记录')
    return
  }

  const invalidRows = rows.filter(r => r.bindStatus !== 'BOUND')
  if (invalidRows.length > 0) {
    KhMessageFn.warning('仅"已绑定"状态的记录可以指定地址上架')
    return
  }

  // 校验所有选中行同仓库
  const warehouseIds = [...new Set(rows.map(r => r.warehouseId).filter(Boolean))]
  if (warehouseIds.length > 1) {
    KhMessageFn.warning('所选记录属于不同仓库，请统一筛选后再选择')
    return
  }

  putawayRows.value = rows
  putawayForm.toZoneCode = ''
  putawayForm.toLocationCode = ''
  putawayLocationOptions.value = []
  putawayDialogVisible.value = true

  // 加载区域选项
  const warehouseId = rows[0].warehouseId
  if (warehouseId) {
    loadPutawayZones(warehouseId)
  }
}

// 加载存储区域下拉（仅存储类库区）
const loadPutawayZones = async (warehouseId) => {
  try {
    const res = await getStorageZones(warehouseId)
    const list = res.data || []
    putawayZoneOptions.value = list.map(z => ({ label: z.zoneName || z.zoneCode, value: z.zoneCode }))
  } catch {
    putawayZoneOptions.value = []
  }
}

const handlePutawaySubmit = async () => {
  if (!putawayFormRef.value) return
  const valid = await putawayFormRef.value.validate().catch(() => false)
  if (!valid) return

  if (!putawayForm.toLocationCode) {
    KhMessageFn.warning('请选择目标货位')
    return
  }

  submitLoading.value = true
  try {
    const warehouseId = putawayRows.value[0].warehouseId

    // 为每个选中的容器创建上架任务
    const containerCodes = putawayRows.value.map(r => r.containerCode)
    for (const containerCode of containerCodes) {
      await adhocPutawayTo({
        warehouseId,
        containerCode,
        toLocationCode: putawayForm.toLocationCode,
      })
    }

    KhMessageFn.success('上架任务创建成功')
    putawayDialogVisible.value = false
    pageRef.value?.clearSelection()
    pageRef.value?.reload()
  } catch {
    // request.js 已处理错误提示
  } finally {
    submitLoading.value = false
  }
}

const handlePutawayDialogClose = () => {
  putawayRows.value = []
  putawayForm.toZoneCode = ''
  putawayForm.toLocationCode = ''
  putawayZoneOptions.value = []
  putawayLocationOptions.value = []
}

// ==================== toolbar 按钮 ====================
const toolbarButtons = [
  {
    label: '无单据组盘',
    type: 'primary',
    icon: markRaw(Plus),
    permission: 'task:adhoc_inbound:create',
    onClick: handleOpenInbound,
  },
  {
    label: '指定地址上架',
    type: 'success',
    icon: markRaw(TopRight),
    permission: 'task:adhoc_inbound:putaway',
    onClick: handleOpenPutaway,
  },
]
</script>
