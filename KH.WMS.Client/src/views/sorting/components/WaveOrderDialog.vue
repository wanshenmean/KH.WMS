<template>
  <el-dialog v-model="visible" title="波次订单" width="750px" append-to-body>
    <el-table :data="orderList" border size="small" max-height="400">
      <el-table-column type="index" label="序号" width="60" align="center" />
      <el-table-column prop="orderNo" label="订单号" width="160" />
      <el-table-column prop="customerName" label="客户名称" min-width="140" />
      <el-table-column prop="totalItems" label="物料数" width="80" align="center" />
      <el-table-column prop="totalQty" label="总数量" width="80" align="center" />
      <el-table-column prop="status" label="状态" width="100" align="center">
        <template #default="{ row }">
          <el-tag :type="row.status === '已完成' ? 'success' : row.status === '分拣中' ? 'warning' : 'info'" size="small">{{ row.status }}</el-tag>
        </template>
      </el-table-column>
    </el-table>
  </el-dialog>
</template>

<script setup>

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  waveNo: { type: String, default: '' },
})

const emit = defineEmits(['update:modelValue'])

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const customers = ['华为终端', '小米科技', '比亚迪电子', '富士康精密', '中兴通讯', '联想集团']

const orderList = ref([])

const generateOrders = () => {
  const count = Math.floor(Math.random() * 5) + 3
  orderList.value = Array.from({ length: count }, (_, i) => ({
    id: i + 1,
    orderNo: `OB-2025${String(Math.floor(Math.random() * 12) + 1).padStart(2, '0')}${String(Math.floor(Math.random() * 28) + 1).padStart(2, '0')}-${String(Math.floor(Math.random() * 900) + 100)}`,
    customerName: customers[i % customers.length],
    totalItems: Math.floor(Math.random() * 8) + 2,
    totalQty: Math.floor(Math.random() * 500) + 50,
    status: ['待分拣', '分拣中', '已完成'][Math.floor(Math.random() * 3)],
  }))
}

watch(() => props.modelValue, (val) => {
  if (val) generateOrders()
})
</script>
