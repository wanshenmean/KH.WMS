<template>
  <el-dialog v-model="visible" title="调拨详情" width="600px" append-to-body>
    <el-descriptions :column="2" border>
      <el-descriptions-item label="调拨单号">{{ data.transferNo }}</el-descriptions-item>
      <el-descriptions-item label="状态">
        <el-tag :type="statusMap[data.status] || 'info'" size="small">{{ data.status }}</el-tag>
      </el-descriptions-item>
      <el-descriptions-item label="源仓库">{{ data.sourceWarehouse }}</el-descriptions-item>
      <el-descriptions-item label="目标仓库">{{ data.targetWarehouse }}</el-descriptions-item>
      <el-descriptions-item label="物料数">{{ data.totalItems }}</el-descriptions-item>
      <el-descriptions-item label="审核人">{{ data.approver || '-' }}</el-descriptions-item>
      <el-descriptions-item label="调拨日期">{{ data.transferDate }}</el-descriptions-item>
      <el-descriptions-item label="备注">{{ data.remark || '-' }}</el-descriptions-item>
    </el-descriptions>
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

const statusMap = { '待审核': 'warning', '待执行': 'info', '执行中': '', '已完成': 'success', '已取消': 'danger' }
</script>
