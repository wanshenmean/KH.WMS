<template>
  <el-dialog v-model="visible" title="分拣明细" width="700px" append-to-body>
    <el-descriptions :column="2" border style="margin-bottom: 16px">
      <el-descriptions-item label="出库单号">{{ data.orderNo || '-' }}</el-descriptions-item>
      <el-descriptions-item label="物料编码">{{ data.materialCode || '-' }}</el-descriptions-item>
      <el-descriptions-item label="物料名称">{{ data.materialName || '-' }}</el-descriptions-item>
      <el-descriptions-item label="需求数量">{{ data.quantity || '-' }}</el-descriptions-item>
      <el-descriptions-item label="复核人">{{ data.verifier || '-' }}</el-descriptions-item>
      <el-descriptions-item label="复核时间">{{ data.verifyTime || '-' }}</el-descriptions-item>
    </el-descriptions>
    <el-table :data="detailList" border size="small" max-height="300">
      <el-table-column type="index" label="序号" width="60" align="center" />
      <el-table-column prop="bin" label="库位" width="120" />
      <el-table-column prop="batchNo" label="批次号" width="160" />
      <el-table-column prop="pickQty" label="拣货数量" width="100" align="center" />
      <el-table-column prop="verifyQty" label="复核数量" width="100" align="center" />
      <el-table-column prop="status" label="状态" width="100" align="center">
        <template #default="{ row }">
          <el-tag :type="row.status === '一致' ? 'success' : 'danger'" size="small">{{ row.status }}</el-tag>
        </template>
      </el-table-column>
    </el-table>
  </el-dialog>
</template>

<script setup>

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  data: { type: Object, default: () => ({}) },
})

const emit = defineEmits(['update:modelValue'])

const visible = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v),
})

const detailList = ref([])

watch(() => props.modelValue, (val) => {
  if (val) {
    detailList.value = Array.from({ length: Math.floor(Math.random() * 3) + 2 }, (_, i) => ({
      id: i + 1,
      bin: `A${(i % 3) + 1}-${String(i + 1).padStart(3, '0')}-A`,
      batchNo: `BATCH-2025${String((i % 12) + 1).padStart(2, '0')}${String((i % 28) + 1).padStart(2, '0')}-${String.fromCharCode(65 + i)}`,
      pickQty: Math.floor(Math.random() * 100) + 10,
      verifyQty: Math.floor(Math.random() * 100) + 10,
      status: Math.random() > 0.2 ? '一致' : '不一致',
    }))
  }
})
</script>
