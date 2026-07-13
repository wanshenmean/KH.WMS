<template>
  <KhDialog v-model="visible" :title="`组盘绑定 - ${orderNo}`" width="1500px" height="80vh" :description="`${orderInfo}`"
    :confirm-loading="submitLoading" confirm-text="确认组盘" destroy-on-close @confirm="handleSubmit" @close="handleClose">
    <div class="bind-layout">
      <!-- ===== 左侧：可组盘明细 ===== -->
      <div class="bind-left">
        <div class="panel-header">
          <span class="panel-title">可组盘明细</span>
          <el-tag v-if="currentTarget" type="success" size="small">
            正在添加到容器 {{ currentTarget.index + 1 }}
          </el-tag>
        </div>
        <div class="panel-content">
          <el-table :data="availableDetails" border size="small" style="width: 100%"
            :header-cell-style="{ background: '#f5f7fa', color: '#606266', fontWeight: '600' }">
            <el-table-column prop="materialCode" label="物料编码" width="110" />
            <el-table-column prop="materialName" label="物料名称" min-width="120" show-overflow-tooltip />
            <el-table-column prop="batchNo" label="批次号" width="100">
              <template #default="{ row }">{{ row.batchNo || '-' }}</template>
            </el-table-column>
            <el-table-column :label="needReceive ? '已收数量' : '单据数量'" width="90" align="right">
              <template #default="{ row }">{{ needReceive ? row.receivedQty : row.orderedQty }}</template>
            </el-table-column>
            <el-table-column label="已组盘" width="70" align="right">
              <template #default="{ row }">
                <el-tag size="small" :type="getBoundQty(row.id) >= row.receivedQty ? 'success' : 'warning'">
                  {{ getBoundQty(row.id) }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="可组盘" width="80" align="right">
              <template #default="{ row }">
                <span :style="{ color: getRemainQty(row) > 0 ? '#f56c6c' : '#67c23a', fontWeight: 600 }">
                  {{ getRemainQty(row) }}
                </span>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="100" align="center">
              <template #default="{ row }">
                <el-button type="primary" link size="small" :disabled="pallets.length === 0 || getRemainQty(row) <= 0"
                  @click="addDetailToPallet(row)">
                  添加到容器
                </el-button>
              </template>
            </el-table-column>
          </el-table>
        </div>
      </div>

      <!-- ===== 右侧：容器列表 ===== -->
      <div class="bind-right">
        <div class="panel-header">
          <span class="panel-title">容器列表</span>
          <el-button type="primary" size="small" @click="addPallet">
            + 添加容器
          </el-button>
        </div>
        <div class="panel-content">
          <el-collapse v-model="activePalletNames" accordion>
            <el-collapse-item v-for="(pallet, index) in pallets" :key="index" :name="index">
              <template #title>
                <div class="pallet-title">
                  <span>
                    容器 {{ index + 1 }}：<b>{{ pallet.containerCode || '未选择' }}</b>
                    <span v-if="pallet.totalQty > 0" class="pallet-qty">({{ pallet.totalQty }})</span>
                  </span>
                  <el-button type="danger" link size="small" @click.stop="removePallet(index)">
                    删除
                  </el-button>
                </div>
              </template>

              <div class="pallet-body">
                <!-- 容器编号选择 -->
                <div class="pallet-field">
                  <label>容器编号</label>
                  <el-select v-model="pallet.containerCode" placeholder="选择或输入容器编号" filterable allow-create
                    reserve-keyword size="small" style="width: 220px">
                    <el-option v-for="c in availableContainers" :key="c.containerCode" :label="c.containerCode"
                      :value="c.containerCode" />
                  </el-select>
                  <span class="field-tip">可从下拉选择已有容器，也可直接输入新编号</span>
                </div>

                <!-- 托盘物料列表 -->
                <div class="pallet-materials-header">
                  <span>容器物料</span>
                  <el-button type="primary" link size="small" @click="setAddTarget(pallet, index)">
                    + 从左侧选择物料
                  </el-button>
                </div>

                <el-table v-if="pallet.materials.length > 0" :data="pallet.materials" border size="small"
                  style="width: 100%">
                  <el-table-column label="物料" min-width="140">
                    <template #default="{ row }">
                      {{ row.materialCode }}
                      <span style="color: #999" v-if="row.materialName"> ({{ row.materialName }})</span>
                    </template>
                  </el-table-column>
                  <el-table-column label="批次号" width="110">
                    <template #default="{ row }">
                      {{ row.batchNo || '-' }}
                    </template>
                  </el-table-column>
                  <el-table-column label="组盘数量" width="120">
                    <template #default="{ row }">
                      <el-input-number v-model="row.qty" :min="0.01" :max="row.maxQty" :precision="2" :controls="false"
                        size="small" style="width: 100%" />
                    </template>
                  </el-table-column>
                  <el-table-column label="操作" width="60" align="center">
                    <template #default="{ $index }">
                      <el-button type="danger" link size="small" @click="removeMaterial(pallet, $index)">
                        删除
                      </el-button>
                    </template>
                  </el-table-column>
                </el-table>
                <el-empty v-else description="暂无物料，请从左侧添加" :image-size="40" />
              </div>
            </el-collapse-item>
          </el-collapse>

          <!-- 已有组盘记录 -->
          <div v-if="existingBinds.length > 0" class="existing-section">
            <el-divider content-position="left">已有组盘记录</el-divider>
            <el-table :data="existingBinds" border size="small" style="width: 100%">
              <el-table-column prop="containerCode" label="容器编号" width="130" />
              <el-table-column prop="materialCode" label="物料编码" width="110" />
              <el-table-column prop="materialName" label="物料名称" min-width="110" show-overflow-tooltip />
              <el-table-column prop="qty" label="数量" width="80" align="right" />
              <el-table-column prop="batchNo" label="批次号" width="100" />
              <el-table-column prop="bindStatus" label="状态" width="80" align="center">
                <template #default="{ row }">
                  <el-tag size="small" :type="bindStatusTagMap[row.bindStatus] || 'info'">
                    {{ bindStatusLabel[row.bindStatus] || row.bindStatus }}
                  </el-tag>
                </template>
              </el-table-column>
            </el-table>
          </div>
        </div>
      </div>
    </div>
  </KhDialog>
</template>

<script setup>
import KhDialog from '@/components/KhDialog/index.vue'
import { getInboundOrderDetail, bindInboundOrder, getContainerBinds } from '@/api/inbound'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  orderId: { type: [Number, String], default: null },
  orderNo: { type: String, default: '' },
})

const emit = defineEmits(['update:modelValue', 'success'])

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const bindStatusTagMap = { BOUND: 'warning', PUT_AWAY: 'success', CANCELLED: 'info' }
const bindStatusLabel = { BOUND: '已绑定', PUT_AWAY: '已上架', CANCELLED: '已取消' }

// ==================== 数据 ====================
const availableDetails = ref([])
const existingBinds = ref([])
const orderType = ref('')
const needReceive = computed(() => orderType.value === 'PURCHASE')
const availableContainers = ref([])
const submitLoading = ref(false)
const activePalletNames = ref([])
const currentTarget = ref(null)

const pallets = reactive([])

const orderInfo = computed(() => {
  const totalBase = availableDetails.value.reduce(
    (s, d) => s + (needReceive.value ? (d.receivedQty || 0) : (d.orderedQty || 0)), 0)
  const totalBound = existingBinds.value
    .filter(b => b.bindStatus === 'BOUND')
    .reduce((s, b) => s + (b.qty || 0), 0)
  const newBound = pallets.reduce((s, p) => s + (p.totalQty || 0), 0)
  return `${needReceive.value ? '已收' : '单据'} ${totalBase} | 已组盘 ${totalBound} | 本次 ${newBound}`
})

const handleClose = () => {
  pallets.length = 0
  availableDetails.value = []
  existingBinds.value = []
  availableContainers.value = []
  currentTarget.value = null
  activePalletNames.value = []
  orderType.value = ''
}

// ==================== 已组盘/未组盘计算 ====================
const getBoundQty = (detailId) => {
  let existingBound = 0
  for (const bind of existingBinds.value) {
    if (bind.bindStatus === 'BOUND' && bind.inboundOrderLineId === detailId) {
      existingBound += (bind.qty || 0)
    }
  }
  // 新增的组盘中也要计算
  for (const pallet of pallets) {
    for (const m of pallet.materials) {
      if (m.detailId === detailId) {
        existingBound += (m.qty || 0)
      }
    }
  }
  return existingBound
}

const getRemainQty = (detail) => {
  const baseQty = needReceive.value ? (detail.receivedQty || 0) : (detail.orderedQty || 0)
  return baseQty - getBoundQty(detail.id)
}

// ==================== 容器操作 ====================
const addPallet = () => {
  const idx = pallets.length
  pallets.push({
    containerCode: '',
    materials: [],
    get totalQty() {
      return this.materials.reduce((s, m) => s + (m.qty || 0), 0)
    },
  })
  activePalletNames.value = [idx]
  currentTarget.value = null
}

const removePallet = (index) => {
  pallets.splice(index, 1)
  if (currentTarget.value && currentTarget.value.index === index) {
    currentTarget.value = null
  }
}

const setAddTarget = (pallet, index) => {
  currentTarget.value = { pallet, index }
  if (!activePalletNames.value.includes(index)) {
    activePalletNames.value = [index]
  }
  KhMessageFn.info('请从左侧"可组盘明细"中选择物料添加')
}

// ==================== 添加物料到容器 ====================
const addDetailToPallet = (detail) => {
  const remain = getRemainQty(detail)
  if (remain <= 0) {
    KhMessageFn.warning('该物料无可组盘数量')
    return
  }

  if (pallets.length === 0) {
    KhMessageFn.warning('请先在右侧添加容器')
    return
  }

  // 确定目标容器
  let target
  if (currentTarget.value) {
    target = currentTarget.value.pallet
  } else if (activePalletNames.value.length > 0) {
    target = pallets[activePalletNames.value[activePalletNames.value.length - 1]]
  } else {
    target = pallets[pallets.length - 1]
  }

  target.materials.push({
    detailId: detail.id,
    materialCode: detail.materialCode,
    materialName: detail.materialName || '',
    batchNo: detail.batchNo || '',
    qty: remain,
    maxQty: remain,
    productionDate: detail.productionDate || undefined,
    expiryDate: detail.expiryDate || undefined,
  })

  currentTarget.value = null
}

const removeMaterial = (pallet, index) => {
  pallet.materials.splice(index, 1)
}

// ==================== 加载数据 ====================
const loadData = async () => {
  if (!props.orderId) return
  try {
    const [detailRes, bindsRes] = await Promise.all([
      getInboundOrderDetail(props.orderId),
      getContainerBinds(props.orderId),
    ])

    const detail = detailRes.data || detailRes
    const detailLines = detail.lines || []
    orderType.value = detail.order?.orderType || ''
    const bindHeaders = (bindsRes.data || bindsRes || [])
    // 将头+明细展开成扁平列表，兼容现有逻辑
    existingBinds.value = bindHeaders.flatMap(h =>
      (h.details || []).map(d => ({
        ...d,
        containerCode: h.containerCode,
        bindNo: h.bindNo,
        bindStatus: h.bindStatus,
        warehouseId: h.warehouseId,
      }))
    )

    // 可组盘明细：基准量(采购用已收货、其他用单据数量)大于已组盘
    const needRecv = needReceive.value
    availableDetails.value = detailLines.filter(l => {
      const base = needRecv ? (l.receivedQty || 0) : (l.orderedQty || 0)
      const existingBound = existingBinds.value
        .filter(b => b.bindStatus === 'BOUND' && b.inboundOrderLineId === l.id)
        .reduce((s, b) => s + (b.qty || 0), 0)
      return base > existingBound
    })

    if (availableDetails.value.length === 0) {
      KhMessageFn.warning('没有需要组盘的明细')
      visible.value = false
    }
  } catch {
    KhMessageFn.error('加载数据失败')
  }
}

watch(() => props.modelValue, (val) => {
  if (val) loadData()
})

// ==================== 提交 ====================
const handleSubmit = async () => {
  if (pallets.length === 0) {
    KhMessageFn.warning('请至少添加一个容器')
    return
  }

  // 校验
  for (let i = 0; i < pallets.length; i++) {
    const pallet = pallets[i]
    if (!pallet.containerCode) {
      KhMessageFn.warning(`第 ${i + 1} 个容器请输入编号`)
      return
    }
    if (pallet.materials.length === 0) {
      KhMessageFn.warning(`第 ${i + 1} 个容器请至少添加一个物料`)
      return
    }
    for (let j = 0; j < pallet.materials.length; j++) {
      const m = pallet.materials[j]
      if (!m.qty || m.qty <= 0) {
        KhMessageFn.warning(`第 ${i + 1} 个容器中物料 ${m.materialCode} 的数量必须大于 0`)
        return
      }
      if (m.qty > m.maxQty) {
        KhMessageFn.warning(`第 ${i + 1} 个容器中物料 ${m.materialCode} 的数量(${m.qty})超过可组盘数量(${m.maxQty})`)
        return
      }
    }
  }

  // 组装数据：按容器分组
  const containerBinds = []
  for (const pallet of pallets) {
    for (const m of pallet.materials) {
      if (!m.qty || m.qty <= 0) continue
      containerBinds.push({
        containerCode: pallet.containerCode,
        inboundOrderLineId: m.detailId,
        qty: m.qty,
        batchNo: m.batchNo || undefined,
        productionDate: m.productionDate || undefined,
        expiryDate: m.expiryDate || undefined,
      })
    }
  }

  submitLoading.value = true
  try {
    const res = await bindInboundOrder(containerBinds)
    if (res.code === 200) {
      KhMessageFn.success(res.message || '组盘绑定成功')
      visible.value = false
      emit('success')
    } else {
      KhMessageFn.error(res.message || '组盘失败')
    }
  } catch {
    // request.js 已处理 HTTP 错误提示
  } finally {
    submitLoading.value = false
  }
}
</script>

<style scoped>
.bind-layout {
  display: flex;
  gap: 16px;
  flex: 1;
  min-height: 0;
  overflow: hidden;
}

.bind-left {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.bind-right {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.panel-content {
  flex: 1;
  min-height: 0;
  overflow-y: auto;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.panel-title {
  font-size: 14px;
  font-weight: 600;
  color: #303133;
}

.pallet-title {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
  font-size: 13px;
}

.pallet-qty {
  color: #909399;
  font-size: 12px;
  margin-left: 4px;
}

.pallet-body {
  padding: 0 4px;
}

.pallet-field {
  margin-bottom: 12px;
}

.pallet-field label {
  display: block;
  font-size: 12px;
  color: #909399;
  margin-bottom: 4px;
}

.field-tip {
  display: block;
  font-size: 11px;
  color: #c0c4cc;
  margin-top: 4px;
}

.pallet-materials-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
  font-size: 13px;
  font-weight: 500;
  color: #606266;
}

.existing-section {
  margin-top: 16px;
}
</style>
