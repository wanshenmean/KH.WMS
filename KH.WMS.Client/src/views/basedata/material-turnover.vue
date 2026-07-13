<template>
  <KhPage ref="pageRef" module="material-turnover" :search-columns="searchColumns" :search-model="searchModel"
    :columns="tableColumns" :show-stat-cards="false" :show-toolbar="true" :show-index="true" :show-selection="true"
    :show-header-filter="true" :search-col-count="4" :crud-operations="crudOperations"
    :permission-prefix="'bd:material_turnover'" :toolbar-buttons="extraToolbarButtons" />

  <KhDialog v-model="calcDialogVisible" title="执行周转分析" width="500px" :form-columns="calcFormColumns"
    :form-model="calcFormData" :form-col-count="1" :confirm-loading="calcLoading" @confirm="handleCalcConfirm" />
</template>

<script setup>
import KhDialog from '@/components/KhDialog/index.vue'
import { calculateMaterialTurnover } from '@/api/basedata'

const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'period', label: '分析周期', type: 'input', clearable: true, placeholder: '如 2026-Q1' },
  { prop: 'classCode', label: '分类编码', type: 'select', clearable: true, options: 'dict:turnover_classification' },
  { prop: 'materialCode', label: '物料编码', type: 'input', clearable: true, placeholder: '请输入物料编码' },
  { prop: 'materialName', label: '物料名称', type: 'input', clearable: true, placeholder: '请输入物料名称' },
]

const searchModel = reactive({ period: '', classCode: '', materialCode: '', materialName: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'materialCode', label: '物料编码', width: 130 },
  { prop: 'materialName', label: '物料名称', minWidth: 150 },
  { prop: 'classCode', label: 'ABC分类', width: 100, align: 'center', type: 'tag', tagMap: 'dict:turnover_classification' },
  { prop: 'outboundCount', label: '出库次数', width: 100, align: 'right' },
  { prop: 'outboundQty', label: '出库数量', width: 120, align: 'right', precision: 2 },
  { prop: 'cumulativeRatio', label: '累计占比(%)', width: 130, align: 'right', precision: 2 },
  { prop: 'period', label: '分析周期', width: 110, align: 'center' },
  { prop: 'calculatedAt', label: '计算时间', minWidth: 170 },
]

// ==================== CRUD 配置（只读） ====================
const crudOperations = {
  create: false,
  update: false,
  delete: false,
  view: true,
  export: true,
}

// ==================== 执行分析弹窗 ====================
const calcDialogVisible = ref(false)
const calcLoading = ref(false)
const calcFormData = reactive({
  period: '',
  analysisDimension: '',
  startDate: '',
  endDate: '',
})

const calcFormColumns = [
  { prop: 'period', label: '分析周期', type: 'input', required: true, maxlength: 20, placeholder: '如 2026-Q1、2026-04' },
  {
    prop: 'analysisDimension', label: '分析维度', type: 'select', required: true,
    options: [{ label: '出库数量', value: 'OUTBOUND_QTY' }, { label: '出库频次', value: 'OUTBOUND_FREQ' }],
    placeholder: '请选择分析维度',
  },
  { prop: 'startDate', label: '开始日期', type: 'date', required: true, placeholder: '请选择开始日期' },
  { prop: 'endDate', label: '结束日期', type: 'date', required: true, placeholder: '请选择结束日期' },
]

const handleCalculate = () => {
  Object.assign(calcFormData, { period: '', analysisDimension: '', startDate: '', endDate: '' })
  calcDialogVisible.value = true
}

const handleCalcConfirm = async (data) => {
  calcLoading.value = true
  try {
    const res = await calculateMaterialTurnover(data)
    if (res.code == 200) {
      KhMessageFn.success('周转分析计算完成')
      calcDialogVisible.value = false
      pageRef.value?.reload?.()
    }
  } catch {
    // 错误由拦截器处理
  } finally {
    calcLoading.value = false
  }
}

// ==================== 工具栏按钮 ====================
const extraToolbarButtons = [
  {
    label: '执行计算',
    type: 'warning',
    icon: 'DataAnalysis',
    permission: 'bd:material_turnover:calculate',
    onClick: handleCalculate,
  },
]
</script>
