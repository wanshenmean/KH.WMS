<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage
      ref="pageRef"
      module="strategy-config"
      title="策略配置"
      :search-columns="searchColumns"
      :search-model="searchModel"
      :columns="tableColumns"
      :show-stat-cards="false"
      :show-toolbar="true"
      :show-index="true"
      :show-header-filter="true"
      :search-col-count="3"
      :crud-operations="{ create: false, update: false, delete: true, view: true, export: false }"
      :permission-prefix="'strategy:config'"
      :before-delete="beforeDelete"
      :action-buttons="actionButtons"
    >
      <template #toolbar-left>
        <el-button type="primary" @click="handleCreate">
          <el-icon><Plus /></el-icon> 新增
        </el-button>
      </template>
    </KhPage>

    <!-- 新增/编辑弹窗 -->
    <KhDialog
      v-model="dialogVisible"
      :title="dialogMode === 'create' ? '新增策略配置' : '编辑策略配置'"
      width="780px"
      destroy-on-close
      :confirm-loading="submitLoading"
      @confirm="handleSubmit"
      @close="resetForm"
    >
      <template #default>
        <KhForm
          ref="formRef"
          :columns="formColumns"
          v-model="formData"
          :label-width="'100px'"
          :col-count="2"
          @change="handleFormChange"
        >
          <template #strategyParamsSlot="{ model }">
            <el-divider content-position="left">
              <span style="font-size: 13px; color: #606266;">策略参数</span>
            </el-divider>
            <StrategyParamForm ref="paramFormRef" :rule-code="model.ruleCode" :params-json="model.strategyParams" :schema="paramSchemaMap[model.ruleCode] || []" />
          </template>
        </KhForm>
      </template>
    </KhDialog>
  </div>
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'
import { getZonesByWarehouse } from '@/api/warehouse'
import { getStrategyOptions } from '@/api/strategy'
import StrategyParamForm from './components/StrategyParamForm.vue'

const pageRef = ref(null)
const crudApi = useCrudApi('strategy-config')

// ==================== 策略类型与规则编码映射（从后端动态加载） ====================
const strategyTypeOptions = ref([])
const ruleCodeMap = ref({})
const paramSchemaMap = ref({})

const strategyTypeTagMap = computed(() => {
  const map = {}
  strategyTypeOptions.value.forEach(opt => { map[opt.value] = opt.label })
  return map
})

const ruleCodeTagMap = computed(() => {
  const map = {}
  Object.values(ruleCodeMap.value).flat().forEach(opt => { map[opt.value] = opt.label })
  return map
})

// 加载策略选项
async function loadStrategyOptions() {
  try {
    const res = await getStrategyOptions()
    const data = res.data || res
    strategyTypeOptions.value = data.types || []
    ruleCodeMap.value = data.ruleCodeMap || {}
    paramSchemaMap.value = data.paramSchemaMap || {}
    // 更新表单中策略类型下拉选项
    const col = formColumns.value.find(c => c.prop === 'strategyType')
    if (col) col.options = strategyTypeOptions.value
    formColumns.value = [...formColumns.value]
  } catch {
    // 加载失败时保持空列表
  }
}

onMounted(() => {
  loadStrategyOptions()
})

// ==================== 搜索 ====================
const searchColumns = computed(() => [
  { prop: 'strategyName', label: '策略名称', type: 'input', clearable: true },
  {
    prop: 'strategyType', label: '策略类型', type: 'select', clearable: true,
    options: strategyTypeOptions,
  },
  {
    prop: 'status', label: '状态', type: 'select', clearable: true,
    options: [
      { label: '启用', value: 1 },
      { label: '禁用', value: 0 },
    ],
  },
])

const searchModel = reactive({
  strategyName: '',
  strategyType: '',
  status: '',
})

// ==================== 表格列 ====================
const tableColumns = computed(() => [
  { prop: 'strategyCode', label: '策略编码', width: 140 },
  { prop: 'strategyName', label: '策略名称', width: 160 },
  {
    prop: 'strategyType', label: '策略类型', width: 120, align: 'center',
    type: 'tag', tagMap: strategyTypeTagMap.value,
    tagTypeMap: {
      PUTAWAY: '', LOCATION_ALLOCATION: 'success', PICKING: 'warning',
      INVENTORY_ALLOCATION: 'danger', REPLENISHMENT: 'info',
      OUTBOUND_ALLOCATION: 'primary',
    },
  },
  {
    prop: 'ruleCode', label: '策略规则', width: 150, align: 'center',
    type: 'tag', tagMap: ruleCodeTagMap.value,
  },
  { prop: 'priority', label: '优先级', width: 80, align: 'center' },
  {
    prop: 'isDefault', label: '默认', width: 70, align: 'center',
    type: 'tag',
    tagMap: { 0: '否', 1: '是' },
    tagTypeMap: { 1: 'success', 0: 'info' },
  },
  {
    prop: 'status', label: '状态', width: 70, align: 'center',
    type: 'tag',
    tagMap: { 0: '禁用', 1: '启用' },
    tagTypeMap: { 1: 'success', 0: 'danger' },
  },
  { prop: 'description', label: '描述', minWidth: 140, showOverflowTooltip: true },
  { prop: 'remark', label: '备注', minWidth: 120, showOverflowTooltip: true },
])

// ==================== 操作按钮 ====================
const actionButtons = [
  {
    label: '编辑',
    permission: 'strategy:config:edit',
    onClick: (row) => handleUpdate(row),
  },
  {
    label: '启用/停用',
    type: 'warning',
    permission: 'strategy:config:toggle',
    show: () => true,
    onClick: (row) => {
      KhMessageFn.success(`已${row.status === 1 ? '停用' : '启用'}策略 "${row.strategyName}"`)
    },
  },
]

// ==================== 表单配置 ====================
const formColumns = ref([
  { prop: 'strategyCode', label: '策略编码', type: 'input', required: true, maxlength: 50 },
  { prop: 'strategyName', label: '策略名称', type: 'input', required: true, maxlength: 100 },
  {
    prop: 'strategyType', label: '策略类型', type: 'select', required: true,
    options: [],
  },
  {
    prop: 'ruleCode', label: '策略规则', type: 'select', required: true,
    options: [], disabled: true,
  },
  { prop: 'priority', label: '优先级', type: 'number', min: 1, max: 999, defaultValue: 100 },
  { prop: 'sortOrder', label: '执行顺序', type: 'number', min: 1, max: 999, defaultValue: 100 },
  {
    prop: 'executionMode', label: '执行模式', type: 'select', defaultValue: 'CHAIN',
    options: [
      { label: '链式执行', value: 'CHAIN' },
      { label: '并行执行', value: 'PARALLEL' },
    ],
  },
  {
    prop: 'docType', label: '单据类型', type: 'select', clearable: true,
    options: 'dict:order_type',
    placeholder: '不填则不限单据类型',
  },
  { prop: 'isDefault', label: '是否默认', type: 'switch', defaultValue: 0, activeValue: 1, inactiveValue: 0 },
  { prop: 'status', label: '状态', type: 'switch', defaultValue: 1, activeValue: 1, inactiveValue: 0 },
  { prop: 'description', label: '描述', type: 'textarea', span: 24, maxlength: 500 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 500 },
  // 仓库-库区
  {
    prop: 'warehouseId', label: '仓库', type: 'select', clearable: true,
    options: 'dict:warehouse_list', placeholder: '不填则不限仓库',
  },
  {
    prop: 'zoneId', label: '库区', type: 'select', clearable: true,
    options: [], placeholder: '不填则不限库区',
  },
  {
    prop: 'materialCategoryId', label: '物料分类ID', type: 'remote-select', clearable: true,
    placeholder: '不填则不限',
    remoteMethod: async (query) => {
      if (!query) return []
      try {
        const res = await fetch('/api/material-category/tree')
        const json = await res.json()
        const flat = []
        const walk = (nodes) => {
          if (!nodes) return
          nodes.forEach(n => {
            flat.push({ label: n.name || n.categoryName, value: n.id })
            if (n.children) walk(n.children)
          })
        }
        walk(json.data || json)
        return flat.filter(i => i.label.includes(query))
      } catch { return [] }
    },
  },
  // 策略参数 slot
  { prop: 'strategyParamsSlot', label: '', type: 'slot', span: 24 },
])

// ==================== 弹窗逻辑 ====================
const dialogVisible = ref(false)
const dialogMode = ref('create')
const submitLoading = ref(false)
const formRef = ref(null)
const paramFormRef = ref(null)

const createFormData = () => ({
  id: null,
  strategyCode: '',
  strategyName: '',
  strategyType: '',
  ruleCode: '',
  warehouseId: null,
  zoneId: null,
  materialCategoryId: null,
  docType: '',
  priority: 100,
  isDefault: 0,
  status: 1,
  sortOrder: 100,
  executionMode: 'CHAIN',
  strategyParams: '',
  description: '',
  remark: '',
})

const formData = reactive(createFormData())

// 表单字段变更处理
function handleFormChange(prop, val) {
  if (prop === 'strategyType') {
    formRef.value.formData.ruleCode = ''
    nextTick(() => {
      const col = formColumns.value.find(c => c.prop === 'ruleCode')
      if (col) {
        col.disabled = !val
        col.options = ruleCodeMap.value[val] || []
        formColumns.value = [...formColumns.value]
      }
    })
  }
  if (prop === 'warehouseId') {
    formRef.value.formData.zoneId = ''
    nextTick(() => {
      const col = formColumns.value.find(c => c.prop === 'zoneId')
      if (col) col.options = []
      formColumns.value = [...formColumns.value]
    })
    if (val) {
      getZonesByWarehouse(val).then(res => {
        const col = formColumns.value.find(c => c.prop === 'zoneId')
        if (col) {
          col.options = (res.data || []).map(z => ({ label: z.zoneName || z.zoneCode, value: z.id }))
          formColumns.value = [...formColumns.value]
        }
      }).catch(() => {})
    }
  }
}

const beforeDelete = (row) => {
  if (row.status === 1) {
    KhMessageFn.warning('启用状态的策略不允许删除，请先停用')
    return false
  }
}

const resetForm = () => {
  Object.assign(formData, createFormData())
  const ruleCol = formColumns.value.find(c => c.prop === 'ruleCode')
  if (ruleCol) { ruleCol.disabled = true; ruleCol.options = [] }
  const zoneCol = formColumns.value.find(c => c.prop === 'zoneId')
  if (zoneCol) zoneCol.options = []
  formColumns.value = [...formColumns.value]
}

const handleCreate = () => {
  dialogMode.value = 'create'
  resetForm()
  dialogVisible.value = true
}

const handleUpdate = async (row) => {
  dialogMode.value = 'update'
  try {
    const res = await crudApi.detail(row.id)
    const data = res.data || row
    Object.assign(formData, {
      id: data.id,
      strategyCode: data.strategyCode || '',
      strategyName: data.strategyName || '',
      strategyType: data.strategyType || '',
      ruleCode: data.ruleCode || '',
      warehouseId: data.warehouseId || null,
      zoneId: data.zoneId || null,
      materialCategoryId: data.materialCategoryId || null,
      docType: data.docType || '',
      priority: data.priority ?? 100,
      isDefault: data.isDefault ?? 0,
      status: data.status ?? 1,
      sortOrder: data.sortOrder ?? 100,
      executionMode: data.executionMode || 'CHAIN',
      strategyParams: data.strategyParams || '',
      description: data.description || '',
      remark: data.remark || '',
    })
    // 编辑时加载库区选项和规则选项
    if (formData.warehouseId) {
      try {
        const res = await getZonesByWarehouse(formData.warehouseId)
        const zoneCol = formColumns.value.find(c => c.prop === 'zoneId')
        if (zoneCol) {
          zoneCol.options = (res.data || []).map(z => ({ label: z.zoneName || z.zoneCode, value: z.id }))
          formColumns.value = [...formColumns.value]
        }
      } catch {}
    }
    if (formData.strategyType) {
      const ruleCol = formColumns.value.find(c => c.prop === 'ruleCode')
      if (ruleCol) {
        ruleCol.disabled = false
        ruleCol.options = ruleCodeMap.value[formData.strategyType] || []
        formColumns.value = [...formColumns.value]
      }
    }
    dialogVisible.value = true
  } catch {
    Object.assign(formData, {
      id: row.id,
      strategyCode: row.strategyCode || '',
      strategyName: row.strategyName || '',
      strategyType: row.strategyType || '',
      ruleCode: row.ruleCode || '',
      warehouseId: row.warehouseId || null,
      zoneId: row.zoneId || null,
      materialCategoryId: row.materialCategoryId || null,
      docType: row.docType || '',
      priority: row.priority ?? 100,
      isDefault: row.isDefault ?? 0,
      status: row.status ?? 1,
      sortOrder: row.sortOrder ?? 100,
      executionMode: row.executionMode || 'CHAIN',
      strategyParams: row.strategyParams || '',
      description: row.description || '',
      remark: row.remark || '',
    })
    dialogVisible.value = true
  }
}

const handleSubmit = async () => {
  // KhDialog 使用自定义插槽，@confirm 不传参，需从 KhForm 内部 formData 获取
  const submitData = { ...formRef.value.formData }
  // 收集策略参数
  submitData.strategyParams = paramFormRef.value?.getParamJson() || ''
  // 移除辅助字段
  delete submitData.strategyParamsSlot

  submitLoading.value = true
  try {
    if (dialogMode.value === 'create') {
      delete submitData.id
      await crudApi.create(submitData)
      KhMessageFn.success('新增成功')
    } else {
      await crudApi.update(submitData)
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
