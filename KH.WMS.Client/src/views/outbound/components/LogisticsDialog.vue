<template>
  <el-dialog v-model="visible" title="物流跟踪信息" width="600px" append-to-body>
    <el-descriptions :column="2" border style="margin-bottom: 16px">
      <el-descriptions-item label="出库单号">{{ data.orderNo || '-' }}</el-descriptions-item>
      <el-descriptions-item label="客户名称">{{ data.customerName || '-' }}</el-descriptions-item>
      <el-descriptions-item label="物流公司">{{ data.logisticsCompany || '-' }}</el-descriptions-item>
      <el-descriptions-item label="物流单号">{{ data.logisticsNo || '-' }}</el-descriptions-item>
      <el-descriptions-item label="发货时间">{{ data.shipDate || '-' }}</el-descriptions-item>
      <el-descriptions-item label="签收状态">
        <el-tag v-if="data.status === '已签收'" type="success" size="small">已签收</el-tag>
        <el-tag v-else-if="data.status === '运输中'" type="warning" size="small">运输中</el-tag>
        <el-tag v-else type="info" size="small">{{ data.status || '待发货' }}</el-tag>
      </el-descriptions-item>
    </el-descriptions>
    <h4 style="margin: 0 0 12px; font-size: 14px; color: #303133;">物流轨迹</h4>
    <el-timeline>
      <el-timeline-item v-for="(item, i) in timeline" :key="i" :timestamp="item.time" placement="top" :type="i === 0 ? 'primary' : 'info'">
        {{ item.content }}
      </el-timeline-item>
    </el-timeline>
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

const timeline = ref([])

watch(() => props.modelValue, (val) => {
  if (val) {
    timeline.value = [
      { time: '2025-04-09 16:30', content: '包裹已签收，签收人：前台王小姐' },
      { time: '2025-04-09 08:00', content: '快递员派送中，联系电话：138****1234' },
      { time: '2025-04-08 22:15', content: '已到达目的城市，准备派送' },
      { time: '2025-04-08 10:30', content: '已从分拨中心发出' },
      { time: '2025-04-07 18:00', content: '已到达中转站' },
      { time: '2025-04-07 09:00', content: '快件已揽收' },
    ]
  }
})
</script>
