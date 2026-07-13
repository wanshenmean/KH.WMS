<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage title="拣货管理" :stat-cards="statCards" :search-columns="searchColumns" :search-model="searchModel"
      :columns="tableColumns" :load="loadFn" :show-stat-cards="true" :show-search="true" :show-toolbar="true"
      :show-index="true">
      <template #toolbar-left>
        <el-button type="primary" @click="handleAssign">分配拣货任务</el-button>
        <el-button type="success" @click="handleBatchComplete">批量完成</el-button>
        <el-button @click="handlePrint">打印拣货单</el-button>
      </template>
      <template #action="{ row }">
        <el-button type="primary" link size="small" @click="handleStartPick(row)">开始拣货</el-button>
        <el-button type="success" link size="small" @click="handleComplete(row)">确认完成</el-button>
      </template>
    </KhPage>

    <AssignPickerDialog :visible="assignDialogVisible" :data="assignDialogData" @success="handleDialogSuccess" />
  </div>
</template>

<script setup>
import KhPage from '@/components/KhPage/index.vue'
import AssignPickerDialog from './components/AssignPickerDialog.vue'
import {
  Clock,
  List,
  CircleCheck
} from '@element-plus/icons-vue'

const statCards = [
  { label: '待拣货', value: 18, icon: markRaw(Clock), theme: 'warning' },
  { label: '拣货中', value: 7, icon: markRaw(List), theme: 'primary' },
  { label: '已完成', value: 126, icon: markRaw(CircleCheck), theme: 'success' }
]

const searchColumns = [
  { prop: 'pickNo', label: '拣货单号', type: 'input', placeholder: '请输入拣货单号' },
  { prop: 'orderNo', label: '出库单号', type: 'input', placeholder: '请输入出库单号' },
  { prop: 'materialCode', label: '物料编码', type: 'input', placeholder: '请输入物料编码' },
  {
    prop: 'picker', label: '拣货员', type: 'select', placeholder: '请选择拣货员',
    options: [
      { label: '李明', value: '李明' },
      { label: '张伟', value: '张伟' },
      { label: '王芳', value: '王芳' },
      { label: '赵刚', value: '赵刚' },
      { label: '孙丽', value: '孙丽' }
    ]
  },
  {
    prop: 'status', label: '状态', type: 'select', placeholder: '请选择状态',
    options: [
      { label: '待拣货', value: '待拣货' },
      { label: '拣货中', value: '拣货中' },
      { label: '已完成', value: '已完成' }
    ]
  }
]

const searchModel = reactive({
  pickNo: '',
  orderNo: '',
  materialCode: '',
  picker: '',
  status: ''
})

const tagMap = { '待拣货': '待拣货', '拣货中': '拣货中', '已完成': '已完成' }
const tagTypeMap = { '待拣货': 'warning', '拣货中': 'primary', '已完成': 'success' }

const tableColumns = [
  { prop: 'pickNo', label: '拣货单号', width: 150 },
  { prop: 'orderNo', label: '出库单号', width: 160 },
  { prop: 'materialCode', label: '物料编码', width: 140 },
  { prop: 'materialName', label: '物料名称', minWidth: 160 },
  { prop: 'reqQty', label: '需求数量', width: 100, align: 'right' },
  { prop: 'pickedQty', label: '已拣数量', width: 100, align: 'right' },
  { prop: 'binCode', label: '源库位', width: 130 },
  { prop: 'picker', label: '拣货员', width: 100 },
  { prop: 'status', label: '状态', width: 100, tagMap, tagTypeMap }
]

const pickers = ['李明', '张伟', '王芳', '赵刚', '孙丽']
const statuses = ['待拣货', '拣货中', '已完成']

const materialList = [
  { code: 'MAT-0001', name: '贴片电阻0402-10K' },
  { code: 'MAT-0002', name: '贴片电容0603-100nF' },
  { code: 'MAT-0003', name: 'STM32F103C8T6芯片' },
  { code: 'MAT-0004', name: 'USB Type-C连接器' },
  { code: 'MAT-0005', name: '4层PCB主板 1.6mm' },
  { code: 'MAT-0006', name: '0.96寸OLED显示屏' },
  { code: 'MAT-0007', name: 'LM7805稳压芯片' },
  { code: 'MAT-0008', name: '继电器SRD-05VDC-SL-C' },
  { code: 'MAT-0009', name: '光电传感器E3F-DS30' },
  { code: 'MAT-0010', name: '步进电机42BYGHW609' },
  { code: 'MAT-0011', name: '锂电池组48V20Ah' },
  { code: 'MAT-0012', name: '铝合金外壳 200x150x80' },
  { code: 'MAT-0013', name: '散热风扇6025 12V' },
  { code: 'MAT-0014', name: '硅胶密封圈 Φ25x3' },
  { code: 'MAT-0015', name: 'M3x8不锈钢螺丝' },
  { code: 'MAT-0016', name: 'ESP32-WROOM-32D模组' },
  { code: 'MAT-0017', name: 'RS485转接模块' },
  { code: 'MAT-0018', name: '温度传感器PT100' },
  { code: 'MAT-0019', name: '防水连接器M12-4Pin' },
  { code: 'MAT-0020', name: '工业级SD卡 32GB' }
]

function padZero(n, len = 2) {
  return String(n).padStart(len, '0')
}

function generateMockData() {
  const data = []
  for (let i = 1; i <= 55; i++) {
    const material = materialList[(i - 1) % materialList.length]
    const reqQty = Math.floor(Math.random() * 500) + 10
    const statusIdx = i % 3
    const status = statuses[statusIdx]
    const pickedQty = statusIdx === 0 ? 0 : (statusIdx === 1 ? Math.floor(reqQty * (0.3 + Math.random() * 0.5)) : reqQty)

    data.push({
      id: i,
      pickNo: `PICK-${String(202500001 + i)}`,
      orderNo: `SO-${String(202400100 + Math.ceil(i / 2))}`,
      materialCode: material.code,
      materialName: material.name,
      reqQty,
      pickedQty,
      binCode: `${String.fromCharCode(65 + (i % 6))}-${padZero((i % 10) + 1)}-${padZero((i % 5) + 1)}-${(i % 8) + 1}`,
      picker: statusIdx === 0 ? '' : pickers[i % pickers.length],
      status
    })
  }
  return data
}

const allData = generateMockData()

const loadFn = async (params) => {
  await new Promise(r => setTimeout(r, 300))
  const { page = 1, size = 20, ...filters } = params || {}
  let filtered = [...allData]

  if (filters.pickNo) {
    filtered = filtered.filter(r => r.pickNo.includes(filters.pickNo))
  }
  if (filters.orderNo) {
    filtered = filtered.filter(r => r.orderNo.includes(filters.orderNo))
  }
  if (filters.materialCode) {
    filtered = filtered.filter(r => r.materialCode.includes(filters.materialCode))
  }
  if (filters.picker) {
    filtered = filtered.filter(r => r.picker === filters.picker)
  }
  if (filters.status) {
    filtered = filtered.filter(r => r.status === filters.status)
  }

  const total = filtered.length
  const start = (page - 1) * size
  const end = start + size

  return { data: filtered.slice(start, end), total }
}

// --- Dialog state ---
const pageRef = ref(null)
const assignDialogVisible = ref(false)
const assignDialogData = ref({})

// --- Toolbar handlers ---
const handleAssign = () => {
  assignDialogData.value = {}
  assignDialogVisible.value = true
}

const handleBatchComplete = () => {
  KhMsgBoxFn.confirm('确认批量完成选中的拣货任务？', '批量完成', {
    confirmButtonText: '确认',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(() => {
    KhMessageFn.success('批量完成成功')
  }).catch(() => { })
}

const handlePrint = () => {
  KhMessageFn.info('拣货单打印功能开发中')
}

// --- Action handlers ---
const handleStartPick = (row) => {
  KhMsgBoxFn.confirm(`确认开始拣货【${row.pickNo}】？`, '开始拣货', {
    confirmButtonText: '确认',
    cancelButtonText: '取消',
    type: 'info'
  }).then(() => {
    KhMessageFn.success('已开始拣货')
  }).catch(() => { })
}

const handleComplete = (row) => {
  KhMsgBoxFn.confirm(`确认完成拣货【${row.pickNo}】？`, '确认完成', {
    confirmButtonText: '确认',
    cancelButtonText: '取消',
    type: 'success'
  }).then(() => {
    KhMessageFn.success('拣货完成')
  }).catch(() => { })
}

const handleDialogSuccess = () => {
  assignDialogVisible.value = false
  if (pageRef.value) {
    pageRef.value.reload()
  }
}
</script>
