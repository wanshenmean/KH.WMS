<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage ref="pageRef" title="入库单管理" module="inbound-order" :search-columns="searchColumns"
      :search-model="searchModel" :columns="tableColumns" :show-stat-cards="false" :show-toolbar="true"
      :show-index="true" :show-header-filter="true" :search-col-count="3"
      :crud-operations="{ create: false, update: false, delete: true, view: true, export: false }"
      :permission-prefix="'in:order'" :delete-show="(row) => row.allowedActions?.includes('DELETE')"
      :action-buttons="actionButtons" :toolbar-buttons="toolbarButtons" :detail-lines="detailLineConfigs" :detail-width="'1100px'">
      <template #orderStatus="{ row }">
        <el-tag size="small" :type="statusTagMap[row.orderStatus]?.type || 'info'">
          {{ statusTagMap[row.orderStatus]?.label || row.orderStatus }}
        </el-tag>
      </template>
    </KhPage>

    <!-- 新增/编辑弹窗 -->
    <KhDialog v-model="dialogVisible" :title="dialogMode === 'create' ? '新增入库单' : '编辑入库单'" width="1100px"
      destroy-on-close :confirm-loading="submitLoading" @confirm="handleSubmit" @close="resetForm">
      <template #default>
        <KhForm ref="formRef" :columns="formColumns" v-model="formData" :label-width="'90px'" :col-count="4" />

        <el-divider content-position="left">
          <span style="font-size: 13px; color: #606266;">明细行</span>
        </el-divider>

        <KhEditableTable v-model="lines" :columns="lineColumns" :default-row="createEmptyLine" :max-height="360"
          add-text="添加行" :action-width="70" />
      </template>
    </KhDialog>

    <!-- 收货弹窗 -->
    <ReceiveDialog v-model="receiveDialogVisible" :order-id="currentOrder.id" :order-no="currentOrder.orderNo"
      :order-status="currentOrder.orderStatus" @success="pageRef?.reload()" />

    <!-- 组盘弹窗 -->
    <ContainerBindDialog v-model="bindDialogVisible" :order-id="currentOrder.id" :order-no="currentOrder.orderNo"
      @success="pageRef?.reload()" />
  </div>
</template>

<script setup>
import { KhEditableTable } from '@/components'
import { useCrudApi } from '@/utils/crud'
import { useExtFields } from '@/utils/useExtFields'
import { createInboundOrder, updateInboundOrder, getInboundOrderDetail } from '@/api/inbound'
import ReceiveDialog from './components/ReceiveDialog.vue'
import ContainerBindDialog from './components/ContainerBindDialog.vue'

const pageRef = ref(null)
const crudApi = useCrudApi('inbound-order')

// ==================== 扩展字段 ====================
const {
  loadExtConfig,
  mergedColumns,
  mergedTableColumns,
  mergedLineColumns,
  extractExtData,
  extractLineExtData,
  mergeExtDataToForm,
  mergeLineExtDataToForm,
} = useExtFields('/api/inbound-order/form-config')

onMounted(() => {
  loadExtConfig()
})

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'orderNo', label: '入库单号', type: 'input', clearable: true },
  {
    prop: 'orderType', label: '入库类型', type: 'select', clearable: true,
    options: 'dict:inbound_order_type',
  },
  {
    prop: 'orderStatus', label: '状态', type: 'select', clearable: true,
    options: 'dict:inbound_order_status',
  },
]

const searchModel = reactive({
  orderNo: '',
  orderType: '',
  orderStatus: '',
})

// ==================== 表格列 ====================
const statusTagMap = {
  DRAFT: { label: '草稿', type: 'info' },
  RECEIVING: { label: '收货中', type: 'warning' },
  RECEIVED: { label: '已收货', type: 'success' },
  BOUND: { label: '已组盘', type: '' },
  COMPLETED: { label: '已完成', type: '' },
  CANCELLED: { label: '已取消', type: 'danger' },
}

const baseTableColumns = [
  { prop: 'orderNo', label: '入库单号', width: 160 },
  {
    prop: 'orderType', label: '单据类型', width: 120, align: 'center',
    type: 'tag', tagMap: 'dict:inbound_order_type',
  },
  {
    prop: 'orderStatus', label: '状态', width: 100, align: 'center',
    type: 'slot',
  },
  { prop: 'warehouseId', label: '仓库', width: 100, align: 'center' },
  { prop: 'supplierId', label: '供应商', width: 100, align: 'center' },
  { prop: 'orderDate', label: '单据日期', width: 110, align: 'center' },
  { prop: 'totalLines', label: '总行数', width: 80, align: 'center' },
  { prop: 'remark', label: '备注', minWidth: 120, showOverflowTooltip: true },
]

const tableColumns = computed(() => mergedTableColumns(baseTableColumns))

const handleCreate = () => {
  dialogMode.value = 'create'
  resetForm()
  dialogVisible.value = true
}

// ==================== 操作按钮 ====================
const actionButtons = [
  {
    label: '编辑',
    permission: 'in:order:edit',
    onClick: (row) => handleUpdate(row),
    show: (row) => row.allowedActions?.includes('EDIT'),
  },
  {
    label: '收货',
    permission: 'in:order:receive',
    onClick: (row) => handleReceive(row),
    show: (row) => row.allowedActions?.includes('RECEIVE'),
  },
  {
    label: '组盘',
    permission: 'in:order:bind',
    onClick: (row) => handleContainerBind(row),
    show: (row) => row.allowedActions?.includes('BIND'),
  },
]

const toolbarButtons = [
  {
    label: '新增',
    type: 'primary',
    icon: markRaw(Plus),
    permission: 'in:order:add',
    onClick: handleCreate,
  }
]

// ==================== 收货/组盘弹窗 ====================
const receiveDialogVisible = ref(false)
const bindDialogVisible = ref(false)
const currentOrder = ref({ id: null, orderNo: '', orderStatus: '' })

const handleReceive = (row) => {
  currentOrder.value = { id: row.id, orderNo: row.orderNo, orderStatus: row.orderStatus }
  receiveDialogVisible.value = true
}

const handleContainerBind = (row) => {
  currentOrder.value = { id: row.id, orderNo: row.orderNo, orderStatus: row.orderStatus }
  bindDialogVisible.value = true
}

// ==================== 表单配置 ====================
const baseFormColumns = [
  { prop: 'orderNo', label: '入库单号', type: 'input', disabled: true, colSpan: 2, placeholder: '自动生成' },
  {
    prop: 'orderType', label: '单据类型', type: 'select', required: true, colSpan: 2,
    options: 'dict:inbound_order_type',
  },
  {
    prop: 'warehouseId', label: '仓库', type: 'select', clearable: true,
    options: 'dict:warehouse_list', placeholder: '请选择仓库',
  },
  {
    prop: 'supplierId', label: '供应商', type: 'select', clearable: true,
    options: 'dict:supplier_list', placeholder: '请选择供应商',
  },
  {
    prop: 'orderDate', label: '单据日期', type: 'date', dateType: 'date',
    valueFormat: 'YYYY-MM-DD',
  },
  {
    prop: 'sourceDocNo', label: '来源单号', type: 'input', clearable: true, placeholder: '来源单据编号'
  },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 500 },
]

const formColumns = computed(() => mergedColumns(baseFormColumns))

// ==================== 弹窗逻辑 ====================
const dialogVisible = ref(false)
const dialogMode = ref('create')
const submitLoading = ref(false)
const formRef = ref(null)
const lines = ref([])

const createFormData = () => ({
  id: null,
  orderNo: '',
  orderType: '',
  warehouseId: null,
  supplierId: null,
  orderDate: null,
  sourceDocNo: '',
  totalLines: 0,
  remark: '',
})

const formData = reactive(createFormData())

const createEmptyLine = () => ({
  materialCode: '',
  materialName: '',
  orderedQty: null,
  batchNo: '',
  manufactureDate: '',
  expiryDate: '',
  remark: '',
})

// ==================== 明细行列配置 ====================
const baseLineColumns = [
  { prop: 'materialCode', label: '物料编码', type: 'select', minWidth: 130, placeholder: '物料编码', options: 'dict:material_list' },
  { prop: 'orderedQty', label: '订单数量', type: 'number', width: 120, min: 0, precision: 2, controls: false, placeholder: '数量', align: 'right' },
  { prop: 'batchNo', label: '批次号', type: 'input', width: 120, placeholder: '批次号' },
  { prop: 'manufactureDate', label: '生产日期', type: 'date', width: 155, placeholder: '选择日期' },
  { prop: 'expiryDate', label: '有效期', type: 'date', width: 155, placeholder: '选择日期' },
  { prop: 'remark', label: '备注', type: 'input', width: 130, placeholder: '备注' },
]

const lineColumns = computed(() => mergedLineColumns(baseLineColumns))

// ==================== 详情弹窗从表配置 ====================
const detailLineConfigs = [
  {
    prop: 'items',
    title: '入库明细行',
    columns: [
      { prop: 'lineNo', label: '行号', width: 70, align: 'center' },
      { prop: 'materialCode', label: '物料编码', minWidth: 130 },
      { prop: 'materialName', label: '物料名称', minWidth: 160 },
      { prop: 'orderedQty', label: '订单数量', width: 120, align: 'right' },
      { prop: 'receivedQty', label: '已收数量', width: 120, align: 'right' },
      { prop: 'batchNo', label: '批次号', width: 120 },
      { prop: 'manufactureDate', label: '生产日期', width: 110, align: 'center' },
      { prop: 'expiryDate', label: '有效期', width: 110, align: 'center' },
      { prop: 'remark', label: '备注', minWidth: 120 },
    ],
  },
]

const resetForm = () => {
  Object.assign(formData, createFormData())
  lines.value = []
}

const handleUpdate = async (row) => {
  dialogMode.value = 'update'
  resetForm()
  try {
    const res = await getInboundOrderDetail(row.id)
    const detail = res.data || res
    const order = detail.order || detail
    const detailLines = detail.lines || []

    Object.assign(formData, {
      id: order.id,
      orderNo: order.orderNo || '',
      orderType: order.orderType || '',
      warehouseId: order.warehouseId || null,
      supplierId: order.supplierId || null,
      orderDate: order.orderDate || '',
      sourceDocNo: order.sourceDocNo || '',
      totalLines: order.totalLines || 0,
      remark: order.remark || '',
    })
    // 回显扩展字段
    mergeExtDataToForm(formData, detail.extDataFlattened)

    lines.value = detailLines.map(l => ({
      materialCode: l.materialCode || '',
      materialName: l.materialName || '',
      orderedQty: l.orderedQty ?? null,
      batchNo: l.batchNo || '',
      manufactureDate: l.manufactureDate || '',
      expiryDate: l.expiryDate || '',
      remark: l.remark || '',
    }))
    // 回显行级扩展字段
    mergeLineExtDataToForm(lines.value, detail.lineExtDataFlattened, detailLines)
    dialogVisible.value = true
  } catch {
    Object.assign(formData, {
      id: row.id,
      orderNo: row.orderNo || '',
      orderType: row.orderType || '',
      warehouseId: row.warehouseId || null,
      supplierId: row.supplierId || null,
      orderDate: row.orderDate || '',
      sourceDocNo: row.sourceDocNo || '',
      totalLines: row.totalLines || 0,
      remark: row.remark || '',
    })
    dialogVisible.value = true
  }
}

const handleSubmit = async () => {
  const submitData = { ...formRef.value.formData }

  const submitLines = lines.value.map((l, i) => ({
    lineNo: i + 1,
    materialCode: l.materialCode || '',
    materialName: l.materialName || '',
    orderedQty: l.orderedQty ?? 0,
    batchNo: l.batchNo || '',
    manufactureDate: l.manufactureDate || null,
    expiryDate: l.expiryDate || null,
    remark: l.remark || '',
  }))

  const orderData = { ...submitData }
  orderData.totalLines = submitLines.length
  // 创建时不传 orderStatus，由后端从 CfgDocumentStatus 配置表自动获取初始状态
  if (dialogMode.value === 'create') {
    delete orderData.orderStatus
  }

  const requestBody = {
    order: orderData,
    lines: submitLines,
    // 提交扩展字段原始数据
    extDataRaw: extractExtData(submitData),
    lineExtDataRaw: extractLineExtData(submitLines),
  }

  submitLoading.value = true
  try {
    if (dialogMode.value === 'create') {
      delete requestBody.order.id
      await createInboundOrder(requestBody)
      KhMessageFn.success('新增成功')
    } else {
      await updateInboundOrder(requestBody.order.id, requestBody)
      KhMessageFn.success('修改成功')
    }
    dialogVisible.value = false
    pageRef.value?.reload()
  } catch {
    // request.js 已处理错误提示
  } finally {
    submitLoading.value = false
  }
}
</script>
