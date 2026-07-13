<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
  <KhPage
    ref="pageRef"
    title="收货登记"
    module="inbound-order"
    :stat-cards="statCards"
    :search-columns="searchColumns"
    :search-model="searchModel"
    :columns="tableColumns"
    :show-stat-cards="true"
    :show-search="true"
    :show-toolbar="true"
    :show-index="true"
    :stat-span="6"
    :crud-operations="{ create: false, update: false, delete: false, view: false, export: false }"
    @search="handleSearch"
  >
    <template #status="{ row }">
      <el-tag :type="statusTagType[row.orderStatus] || 'info'" effect="light" size="small">
        {{ statusLabel[row.orderStatus] || row.orderStatus }}
      </el-tag>
    </template>
    <template #action="{ row }">
      <el-button type="primary" link size="small" @click="handleView(row)">查看</el-button>
      <el-divider direction="vertical" />
      <el-button
        type="warning"
        link
        size="small"
        :disabled="row.orderStatus === 'RECEIVED'"
        @click="handleReceive(row)"
      >
        收货
      </el-button>
      <el-divider direction="vertical" v-if="row.orderStatus === 'RECEIVING' || row.orderStatus === 'RECEIVED'" />
      <el-button
        type="success"
        link
        size="small"
        v-if="row.orderStatus === 'RECEIVING' || row.orderStatus === 'RECEIVED'"
        @click="handleBind(row)"
      >
        组盘
      </el-button>
    </template>
  </KhPage>

  <!-- 收货弹窗 -->
  <ReceiveDialog
    v-model="receiveDialogVisible"
    :order-id="currentOrder.id"
    :order-no="currentOrder.orderNo"
    :order-status="currentOrder.orderStatus"
    @success="handleDialogSuccess"
  />

  <!-- 组盘弹窗 -->
  <ContainerBindDialog
    v-model="bindDialogVisible"
    :order-id="currentOrder.id"
    :order-no="currentOrder.orderNo"
    @success="handleDialogSuccess"
  />

  <!-- 查看详情弹窗 -->
  <KhDialog
    v-model="viewDialogVisible"
    title="入库单详情"
    width="900px"
    :form-columns="viewFormColumns"
    :form-model="viewFormData"
    :form-disabled="true"
    :form-col-count="3"
    :show-footer="false"
  />
  </div>
</template>

<script setup>
import KhPage from '@/components/KhPage/index.vue'
import KhDialog from '@/components/KhDialog/index.vue'
import ReceiveDialog from './components/ReceiveDialog.vue'
import ContainerBindDialog from './components/ContainerBindDialog.vue'
import { getInboundOrderDetail } from '@/api/inbound'

// ============================================================
//  状态配置
// ============================================================
const statusTagType = {
  DRAFT: 'info',
  RECEIVING: 'warning',
  RECEIVED: 'success',
  COMPLETED: '',
  CANCELLED: 'danger',
}

const statusLabel = {
  DRAFT: '待收货',
  RECEIVING: '收货中',
  RECEIVED: '已收货',
  COMPLETED: '已完成',
  CANCELLED: '已取消',
}

// ============================================================
//  统计卡片
// ============================================================
const statCards = ref([
  { label: '待收货', value: 0, icon: markRaw(Timer), theme: 'warning', clickable: true },
  { label: '收货中', value: 0, icon: markRaw(Warning), theme: 'primary', clickable: true },
  { label: '已收货', value: 0, icon: markRaw(CircleCheck), theme: 'success', clickable: true },
])

// ============================================================
//  查询表单
// ============================================================
const searchColumns = ref([
  { prop: 'orderNo', label: '入库单号', type: 'input', clearable: true },
  {
    prop: 'orderStatus', label: '状态', type: 'select', clearable: true,
    options: [
      { label: '待收货', value: 'DRAFT' },
      { label: '收货中', value: 'RECEIVING' },
      { label: '已收货', value: 'RECEIVED' },
    ],
  },
])

const searchModel = reactive({
  orderNo: '',
  orderStatus: '',
})

const handleSearch = (model) => {
  // KhPage 内部已处理搜索
}

// ============================================================
//  表格列配置
// ============================================================
const tableColumns = ref([
  { prop: 'orderNo', label: '入库单号', width: 180, fixed: 'left' },
  { prop: 'orderType', label: '入库类型', width: 120 },
  { prop: 'orderStatus', label: '状态', width: 100, type: 'slot' },
  { prop: 'totalLines', label: '物料数', width: 90, align: 'center' },
  { prop: 'warehouseId', label: '仓库', width: 120 },
  { prop: 'supplierId', label: '供应商', width: 140 },
  { prop: 'orderDate', label: '单据日期', width: 120, align: 'center' },
  { prop: 'createdTime', label: '创建时间', width: 170 },
])

// ============================================================
//  弹窗逻辑
// ============================================================
const pageRef = ref(null)
const receiveDialogVisible = ref(false)
const bindDialogVisible = ref(false)
const viewDialogVisible = ref(false)
const currentOrder = ref({ id: null, orderNo: '', orderStatus: '' })

const viewFormColumns = [
  { prop: 'orderNo', label: '入库单号' },
  { prop: 'orderType', label: '入库类型' },
  { prop: 'orderStatus', label: '状态' },
  { prop: 'orderDate', label: '单据日期' },
  { prop: 'warehouseId', label: '仓库' },
  { prop: 'supplierId', label: '供应商' },
  { prop: 'totalLines', label: '总行数' },
  { prop: 'sourceDocNo', label: '来源单号' },
  { prop: 'remark', label: '备注', span: 24 },
]
const viewFormData = ref({})

const handleDialogSuccess = () => {
  pageRef.value?.reload()
}

const handleView = async (row) => {
  try {
    const res = await getInboundOrderDetail(row.id)
    const detail = res.data || res
    const order = detail.order || detail
    viewFormData.value = { ...order }
    viewDialogVisible.value = true
  } catch {
    KhMessageFn.error('获取详情失败')
  }
}

const handleReceive = (row) => {
  currentOrder.value = { id: row.id, orderNo: row.orderNo, orderStatus: row.orderStatus }
  receiveDialogVisible.value = true
}

const handleBind = (row) => {
  currentOrder.value = { id: row.id, orderNo: row.orderNo, orderStatus: row.orderStatus }
  bindDialogVisible.value = true
}
</script>
