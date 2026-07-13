<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage
      ref="pageRef"
      title="策略链配置"
      module="strategy-chain"
      :search-columns="searchColumns"
      :search-model="searchModel"
      :columns="tableColumns"
      :show-stat-cards="false"
      :show-toolbar="true"
      :show-index="true"
      :show-header-filter="true"
      :search-col-count="2"
      :crud-operations="{ create: false, update: false, delete: true, view: true, export: false }"
      :permission-prefix="'strategy:chain'"
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
      :title="dialogMode === 'create' ? '新增策略链配置' : '编辑策略链配置'"
      width="860px"
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
          <template #chainStepsSlot="{ model }">
            <el-divider content-position="left">
              <span style="font-size: 13px; color: #606266;">步骤管理</span>
            </el-divider>
            <ChainStepEditor ref="stepEditorRef" v-model="model.steps" :strategy-options="strategyOptions" :param-schema-map="paramSchemaMap" />
          </template>
        </KhForm>
      </template>
    </KhDialog>
  </div>
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'
import { getZonesByWarehouse } from '@/api/warehouse'
import { getStrategyOptions, getChainDetail, createChain, updateChain } from '@/api/strategy'
import ChainStepEditor from './components/ChainStepEditor.vue'

const pageRef = ref(null)
const crudApi = useCrudApi('strategy-chain')
const strategyCrudApi = useCrudApi('strategy-config')

// ==================== 策略链类型（从后端动态加载，排除REPLENISHMENT） ====================
const chainTypeOptions = ref([])

const chainTypeTagMap = computed(() => {
  const map = {}
  chainTypeOptions.value.forEach(opt => { map[opt.value] = opt.label })
  return map
})

// 加载策略链类型选项
async function loadChainTypeOptions() {
  try {
    const res = await getStrategyOptions()
    const data = res.data || res
    const allTypes = data.types || []
    chainTypeOptions.value = allTypes.filter(t => t.value !== 'REPLENISHMENT' && t.value !== 'WAVE')
    paramSchemaMap.value = data.paramSchemaMap || {}
    // 更新表单中策略链类型下拉选项
    const col = formColumns.value.find(c => c.prop === 'chainType')
    if (col) col.options = chainTypeOptions.value
    formColumns.value = [...formColumns.value]
  } catch {
    // 加载失败时保持空列表
  }
}

// ==================== 策略配置列表（供步骤编辑器选择） ====================
const strategyOptions = ref([])
const paramSchemaMap = ref({})

async function loadStrategyList() {
  try {
    const res = await strategyCrudApi.pageList({ pageNum: 1, pageSize: 999, status: 1 })
    const list = res.data?.items || res.data || res.rows || []
    strategyOptions.value = list.filter(s => s.status === 1)
  } catch {
    // 加载失败时保持空列表
  }
}

onMounted(() => {
  loadChainTypeOptions()
  loadStrategyList()
})

// ==================== 搜索 ====================
const searchColumns = computed(() => [
  { prop: 'chainName', label: '策略链名称', type: 'input', clearable: true },
  {
    prop: 'chainType', label: '策略链类型', type: 'select', clearable: true,
    options: chainTypeOptions.value,
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
  chainName: '',
  chainType: '',
  status: '',
})

// ==================== 表格列 ====================
const tableColumns = computed(() => [
  { prop: 'chainCode', label: '策略链编码', width: 160 },
  { prop: 'chainName', label: '策略链名称', width: 160 },
  {
    prop: 'chainType', label: '策略链类型', width: 140, align: 'center',
    type: 'tag', tagMap: chainTypeTagMap.value,
    tagTypeMap: {
      PUTAWAY: '', LOCATION_ALLOCATION: 'success', PICKING: 'warning',
      INVENTORY_ALLOCATION: 'danger', OUTBOUND_ALLOCATION: 'primary',
    },
  },
  { prop: 'stepCount', label: '步骤数量', width: 90, align: 'center' },
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
    permission: 'strategy:chain:edit',
    onClick: (row) => handleUpdate(row),
  },
  {
    label: '启用/停用',
    type: 'warning',
    permission: 'strategy:chain:toggle',
    show: () => true,
    onClick: (row) => {
      KhMessageFn.success(`已${row.status === 1 ? '停用' : '启用'}策略链 "${row.chainName}"`)
    },
  },
]

// ==================== 表单配置 ====================
const formColumns = ref([
  { prop: 'chainCode', label: '策略链编码', type: 'input', required: true, maxlength: 50 },
  { prop: 'chainName', label: '策略链名称', type: 'input', required: true, maxlength: 100 },
  {
    prop: 'chainType', label: '策略链类型', type: 'select', required: true,
    options: [],
  },
  { prop: 'priority', label: '优先级', type: 'number', min: 1, max: 999, defaultValue: 100 },
  {
    prop: 'docType', label: '单据类型', type: 'select', clearable: true,
    options: 'dict:order_type',
    placeholder: '不填则不限',
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
  // 步骤管理 slot
  { prop: 'chainStepsSlot', label: '', type: 'slot', span: 24 },
])

// ==================== 弹窗逻辑 ====================
const dialogVisible = ref(false)
const dialogMode = ref('create')
const submitLoading = ref(false)
const formRef = ref(null)
const stepEditorRef = ref(null)

const createFormData = () => ({
  id: null,
  chainCode: '',
  chainName: '',
  chainType: '',
  warehouseId: null,
  zoneId: null,
  docType: '',
  priority: 100,
  isDefault: 0,
  status: 1,
  stepCount: 0,
  description: '',
  remark: '',
  steps: [],
})

const formData = reactive(createFormData())

const beforeDelete = (row) => {
  if (row.status === 1) {
    KhMessageFn.warning('启用状态的策略链不允许删除，请先停用')
    return false
  }
}

const resetForm = () => {
  Object.assign(formData, createFormData())
  const zoneCol = formColumns.value.find(c => c.prop === 'zoneId')
  if (zoneCol) zoneCol.options = []
  formColumns.value = [...formColumns.value]
}

function handleFormChange(prop, val) {
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

const handleCreate = () => {
  dialogMode.value = 'create'
  resetForm()
  dialogVisible.value = true
}

const handleUpdate = async (row) => {
  dialogMode.value = 'update'
  try {
    const res = await getChainDetail(row.id)
    const detail = res.data || res
    const data = detail.chain || detail
    const steps = detail.steps || []

    Object.assign(formData, {
      id: data.id,
      chainCode: data.chainCode || '',
      chainName: data.chainName || '',
      chainType: data.chainType || '',
      warehouseId: data.warehouseId || null,
      zoneId: data.zoneId || null,
      docType: data.docType || '',
      priority: data.priority ?? 100,
      isDefault: data.isDefault ?? 0,
      status: data.status ?? 1,
      stepCount: data.stepCount ?? 0,
      description: data.description || '',
      remark: data.remark || '',
      steps: steps,
    })
    // 编辑时加载库区选项
    if (formData.warehouseId) {
      try {
        const zoneRes = await getZonesByWarehouse(formData.warehouseId)
        const zoneCol = formColumns.value.find(c => c.prop === 'zoneId')
        if (zoneCol) {
          zoneCol.options = (zoneRes.data || []).map(z => ({ label: z.zoneName || z.zoneCode, value: z.id }))
          formColumns.value = [...formColumns.value]
        }
      } catch {}
    }
    dialogVisible.value = true
    // KhForm 初始化时跳过 slot 类型字段，需手动同步 steps
    nextTick(() => {
      formRef.value.formData.steps = steps
    })
  } catch {
    // 接口失败时用行数据回退（无步骤数据）
    Object.assign(formData, {
      id: row.id,
      chainCode: row.chainCode || '',
      chainName: row.chainName || '',
      chainType: row.chainType || '',
      warehouseId: row.warehouseId || null,
      zoneId: row.zoneId || null,
      docType: row.docType || '',
      priority: row.priority ?? 100,
      isDefault: row.isDefault ?? 0,
      status: row.status ?? 1,
      stepCount: row.stepCount ?? 0,
      description: row.description || '',
      remark: row.remark || '',
      steps: [],
    })
    dialogVisible.value = true
    nextTick(() => {
      formRef.value.formData.steps = []
    })
  }
}

const handleSubmit = async () => {
  // KhDialog 自定义插槽时 @confirm 不传参，需从 KhForm 内部 formData 获取
  const submitData = { ...formRef.value.formData }

  // 从 ChainStepEditor 内部获取步骤数据并转换类型（isEnabled: boolean → byte）
  const rawSteps = stepEditorRef.value?.steps || []
  const steps = rawSteps.map((s, i) => ({
    stepNo: i + 1,
    stepName: s.stepName || '',
    strategyConfigId: s.strategyConfigId || null,
    ruleCode: s.ruleCode || '',
    stepMode: s.stepMode || 'CHAIN',
    isEnabled: s.isEnabled ? 1 : 0,
    stepParams: s.stepParams || '',
    remark: s.remark || '',
  }))

  // 构建后端所需的 DTO 格式：{ chain: {...}, steps: [...] }
  const chainData = { ...submitData }
  chainData.stepCount = steps.length
  delete chainData.steps

  const requestBody = {
    chain: chainData,
    steps: steps,
  }

  submitLoading.value = true
  try {
    if (dialogMode.value === 'create') {
      delete requestBody.chain.id
      await createChain(requestBody)
      KhMessageFn.success('新增成功')
    } else {
      await updateChain(requestBody.chain.id, requestBody)
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
