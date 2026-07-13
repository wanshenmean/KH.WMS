<template>
  <KhDialog
    v-model="visible"
    :title="`收货确认 - ${orderNo}`"
    width="950px"
    :description="`状态：${orderStatusLabel[orderStatus] || orderStatus}`"
    :confirm-loading="submitLoading"
    confirm-text="确认收货"
    destroy-on-close
    @confirm="handleSubmit"
    @close="handleClose"
  >
    <KhEditableTable
      v-model="lines"
      :columns="lineColumns"
      :max-height="420"
      :show-add="false"
      :show-action="false"
      :show-index="false"
      size="default"
    >
      <!-- 剩余应收 -->
      <template #remainQty="{ row }">
        <span>{{ ((row.orderedQty || 0) - (row.receivedQty || 0)).toFixed(2) }}</span>
      </template>
      <!-- 本次收货数量 -->
      <template #_receiveQty="{ row }">
        <el-input-number
          v-if="row.lineStatus !== 'RECEIVED'"
          v-model="row._receiveQty"
          :min="0"
          :max="((row.orderedQty || 0) - (row.receivedQty || 0))"
          :precision="2"
          :controls="false"
          size="small"
          style="width: 100%"
          placeholder="0"
        />
        <span v-else style="color: #67c23a;">已收完</span>
      </template>
      <!-- 批次号 -->
      <template #_batchNo="{ row }">
        <el-input
          v-if="row.lineStatus !== 'RECEIVED'"
          v-model="row._batchNo"
          size="small"
          placeholder="批次号"
        />
        <span v-else>{{ row.batchNo || '-' }}</span>
      </template>
    </KhEditableTable>
  </KhDialog>
</template>

<script setup>
import KhDialog from '@/components/KhDialog/index.vue'
import KhEditableTable from '@/components/KhEditableTable/index.vue'
import { getInboundOrderDetail, receiveInboundOrder } from '@/api/inbound'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  orderId: { type: [Number, String], default: null },
  orderNo: { type: String, default: '' },
  orderStatus: { type: String, default: '' },
})

const emit = defineEmits(['update:modelValue', 'success'])

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const orderStatusLabel = {
  DRAFT: '草稿',
  RECEIVING: '收货中',
  RECEIVED: '已收货',
  COMPLETED: '已完成',
  CANCELLED: '已取消',
}

const lines = ref([])
const submitLoading = ref(false)

const lineColumns = [
  { prop: 'lineNo', label: '行号', width: 60, align: 'center' },
  { prop: 'materialCode', label: '物料编码', minWidth: 120 },
  { prop: 'materialName', label: '物料名称', minWidth: 140, showOverflowTooltip: true },
  { prop: 'orderedQty', label: '订单数量', width: 100, align: 'right' },
  { prop: 'receivedQty', label: '已收数量', width: 100, align: 'right' },
  { prop: 'remainQty', label: '剩余应收', width: 100, align: 'right', type: 'slot' },
  { prop: '_receiveQty', label: '本次收货数量', width: 140, align: 'center', type: 'slot' },
  { prop: '_batchNo', label: '批次号', width: 140, type: 'slot' },
]

const handleClose = () => {
  lines.value = []
}

const loadLines = async () => {
  if (!props.orderId) return
  try {
    const res = await getInboundOrderDetail(props.orderId)
    const detail = res.data || res
    const detailLines = detail.lines || []
    lines.value = detailLines.map(l => ({
      ...l,
      _receiveQty: l.lineStatus === 'RECEIVED' ? 0 : ((l.orderedQty || 0) - (l.receivedQty || 0)),
      _batchNo: l.batchNo || '',
    }))
  } catch {
    KhMessageFn.error('加载明细失败')
  }
}

watch(() => props.modelValue, (val) => {
  if (val) loadLines()
})

const handleSubmit = async () => {
  const receiveLines = lines.value
    .filter(l => l.lineStatus !== 'RECEIVED' && l._receiveQty > 0)
    .map(l => ({
      lineId: l.id,
      receiveQty: l._receiveQty,
      batchNo: l._batchNo || undefined,
      manufactureDate: l.manufactureDate || undefined,
      expiryDate: l.expiryDate || undefined,
    }))

  if (receiveLines.length === 0) {
    KhMessageFn.warning('请至少填写一行的收货数量')
    return
  }

  submitLoading.value = true
  try {
    await receiveInboundOrder(props.orderId, receiveLines)
    KhMessageFn.success('收货成功')
    visible.value = false
    emit('success')
  } catch {
    // request.js 已处理错误提示
  } finally {
    submitLoading.value = false
  }
}
</script>
