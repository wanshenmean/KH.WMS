<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage ref="pageRef" title="库位管理" module="location" :stat-cards="statCards" :columns="tableColumns"
      :show-stat-cards="true" :show-toolbar="true" :show-index="true" :show-selection="true" :show-header-filter="true"
      :permission-prefix="'wh:location'" :form-columns="formColumns" :custom-form-data="formDialogData"
      :crud-operations="crudOperations" :action-width="'150'" :row-style="(row) => {
        if (row.status === 'OCCUPIED') return 'info'
        if (row.status === 'LOCKED') return 'warning'
        if (row.isDisabled === 1) return 'danger'
        return ''
      }"
      :action-buttons="extraActionButtons">
      <!-- <template #toolbar-left>
        <el-button type="primary" :icon="Icons.Plus" @click="handleAddBin">新增</el-button>
        <el-button :icon="Icons.Upload" @click="handleImport">导入</el-button>
        <el-button :icon="Icons.Download" @click="handleExport">导出</el-button>
      </template> -->
      <!-- <template #binType="{ row }">
        <el-select v-if="row._editing" v-model="row.binType" size="small" style="width: 80px">
          <el-option label="大型" value="大型" />
          <el-option label="中型" value="中型" />
          <el-option label="小型" value="小型" />
        </el-select>
        <span v-else>{{ row.binType }}</span>
      </template> -->
      <!-- <template #action="{ row }">
        <el-button type="primary" link size="small" @click="handleLock(row)"
          v-if="row.binStatus === '空闲'">锁定</el-button>
        <el-button type="warning" link size="small" @click="handleUnlock(row)"
          v-if="row.binStatus === '锁定'">解锁</el-button>
        <el-button type="danger" link size="small" @click="handleMaintain(row)"
          v-if="row.binStatus === '空闲' || row.binStatus === '占用'">维护</el-button>
        <el-button type="success" link size="small" @click="handleRelease(row)"
          v-if="row.binStatus === '维护'">恢复</el-button>
      </template> -->
    </KhPage>
  </div>
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'
import KhPage from '@/components/KhPage/index.vue'
import { getZonesAndAislesByWarehouse, getLocationStatData } from '@/api/warehouse'

const Icons = markRaw({ Plus, Upload, Download })

const pageRef = ref(null)
const crudApi = useCrudApi('location')

const extraActionButtons = [
  {
    label: (row) => row.isDisabled === 1 ? '启用' : '禁用',
    type: 'warning',
    permission: 'wh:location:toggle',
    show: (row) => true,
    confirm: (row) => `确定要${row.isDisabled === 1 ? '启用' : '禁用'}吗？`,
    onClick: async (row) => {
      const newStatus = row.isDisabled === 0 ? 1 : 0
      const res = await crudApi.setStatus(row.id, newStatus)
      if (res.code === 200) {
        KhMessageFn.success(res.message)
        pageRef.value?.reload()
      }
    },
  },
]
const zoneOptions = ref([])
const aislesOptions = ref([])

// 添加处理函数
const handleDialogSuccess = () => {
  pageRef.value?.reload()
}

const locationStatData = reactive({
  totalCount: 0,
  emptyCount: 0,
  occupiedCount: 0,
})

// ---- 统计卡片 ----
const statCards = computed(() => [
  { label: '总库位', value: locationStatData.totalCount, icon: Icons.Plus, theme: 'primary', clickable: true },
  { label: '已占用', value: locationStatData.occupiedCount, icon: Icons.Upload, theme: 'info', clickable: true },
  { label: '空闲', value: locationStatData.emptyCount, icon: Icons.Download, theme: 'success', clickable: true },
])

const loadLocationStatData = async () => {
  try {
    const res = await getLocationStatData()
    if (res.code == 200) {
      locationStatData.totalCount = res.data.totalCount
      locationStatData.emptyCount = res.data.emptyCount
      locationStatData.occupiedCount = res.data.occupiedCount
    }
  } catch (error) {
    console.error('加载库位统计数据失败:', error)
  }
}

onMounted(async () => {
  await loadLocationStatData()
})

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: true,
  update: true,
  delete: true,
  view: true,
  export: true,
}

// ---- 表格列 ----
const tableColumns = [
  { prop: 'locationCode', label: '库位编码', width: 180, searchable: true },
  { prop: 'warehouseId', label: '所属仓库', width: 140, searchable: true, type: 'tag', tagMap: 'dict:warehouse_list' },
  { prop: 'zoneId', label: '所属库区', width: 140, type: 'tag', tagMap: 'dict:zone_list' },
  { prop: 'aisleNo', label: '巷道', width: 100, align: 'center' },
  { prop: 'rowNo', label: '行', width: 100, align: 'center', searchable: true, filterType: 'number-range' },
  { prop: 'colNo', label: '列', width: 130 },
  { prop: 'layerNo', label: '层', width: 100, align: 'center' },
  { prop: 'side', label: '边', width: 80, align: 'center' },
  { prop: 'depth', label: '深度', width: 80, align: 'center' },
  { prop: 'locationType', label: '库位类型', width: 140, type: 'tag', tagMap: 'dict:location_type', searchable: true, filterType: 'multiple-select' },
  {
    prop: 'status', label: '库位状态', width: 140, align: 'center', type: 'tag', tagMap: 'dict:location_status', searchable: true, filterType: 'multiple-select'
  },
  { prop: 'isLocked', label: '是否锁定', width: 140, align: 'center', type: 'tag', tagMap: 'dict:yes_no', searchable: true, filterType: 'select' },
  { prop: 'isDisabled', label: '是否禁用', width: 140, align: 'center', type: 'tag', tagMap: 'dict:yes_no', searchable: true, filterType: 'select' },
  { prop: 'remark', label: '备注', minWidth: 180, showOverflowTooltip: true },
]

// ==================== 表单配置（新增/编辑弹窗） ====================
const formColumns = [
  {
    prop: 'warehouseId', label: '所属仓库', type: 'select', required: true,
    options: 'dict:warehouse_list', onChange: async (value, formData) => {
      formData.zoneId = ''
      formData.aisleNo = ''
      if (value) {
        const res = await getZonesAndAislesByWarehouse(value)
        zoneOptions.value = res.data.zones.map(item => ({
          label: item.zoneName,
          value: item.id,
        }))
        aislesOptions.value = res.data.aisles.map(item => ({
          label: item.aisleName,
          value: item.id,
        }))
      } else {
        zoneOptions.value = []
        aislesOptions.value = []
      }
    }
  },
  {
    prop: 'zoneId', label: '所属库区', type: 'select', required: true, options: zoneOptions,
  },
  { prop: 'aisleNo', label: '巷道号', type: 'select', required: true, options: aislesOptions },
  { prop: 'side', label: '边', type: 'select', required: true, options: 'dict:location_side' },
  { prop: 'rowNo', label: '排', type: 'number', required: true, maxlength: 20 },
  { prop: 'colNo', label: '列', type: 'number', required: true, maxlength: 20 },
  { prop: 'layerNo', label: '层', type: 'number', required: true, maxlength: 20 },
  { prop: 'depthNo', label: '深度', type: 'number', required: true, maxlength: 20 },
  {
    prop: 'locationType', label: '库位类型', type: 'select', required: true,
    options: 'dict:location_type',
  },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]

const formDialogData = reactive({
  warehouseId: '',
  zoneId: '',
  aisleNo: '',
  rowNo: null,
  colNo: null,
  layerNo: null,
  depthNo: null,
  locationType: '',
  remark: ''
})

// ---- 操作 ----
const handleLock = (row) => {
  row.binStatus = '锁定'
  KhMessageFn.success(`库位 ${row.binCode} 已锁定`)
}

const handleUnlock = (row) => {
  row.binStatus = '空闲'
  KhMessageFn.success(`库位 ${row.binCode} 已解锁`)
}

const handleMaintain = (row) => {
  row.binStatus = '维护'
  KhMessageFn.warning(`库位 ${row.binCode} 已进入维护状态`)
}

const handleRelease = (row) => {
  row.binStatus = '空闲'
  KhMessageFn.success(`库位 ${row.binCode} 已恢复正常`)
}
</script>
