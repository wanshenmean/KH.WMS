<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage ref="pageRef" title="作业定义" :search-columns="searchColumns" :search-model="searchModel" :columns="tableColumns"
      :load="loadFn" :show-stat-cards="false" :show-toolbar="true" :show-index="true" :search-col-count="3">
      <template #toolbar-left>
        <el-button type="primary" @click="handleAdd">新增作业定义</el-button>
      </template>
      <template #action="{ row }">
        <el-button type="primary" link size="small" @click="handleEdit(row)">编辑</el-button>
        <el-button type="warning" link size="small" @click="handleExecute(row)">执行</el-button>
        <el-divider direction="vertical" />
        <el-popconfirm :title="`确定要${row.status === 1 ? '禁用' : '启用'}吗？`" @confirm="handleToggleStatus(row)">
          <template #reference>
            <el-button type="warning" link size="small">{{ row.status === 1 ? '禁用' : '启用' }}</el-button>
          </template>
        </el-popconfirm>
        <el-divider direction="vertical" />
        <el-popconfirm title="确定删除该作业定义?" @confirm="handleDelete(row)">
          <template #reference>
            <el-button type="danger" link size="small">删除</el-button>
          </template>
        </el-popconfirm>
      </template>
    </KhPage>

    <KhDialog v-model="dialogVisible" :title="dialogMode === 'create' ? '作业定义 - 新增' : '作业定义 - 编辑'" width="800px"
      :form-columns="formColumns" :form-model="formData" :form-col-count="2" :confirm-loading="loading"
      @confirm="handleConfirm" />
  </div>
</template>

<script setup>
import KhPage from '@/components/KhPage/index.vue'
import KhDialog from '@/components/KhDialog/index.vue'

const pageRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'code', label: '作业编码', type: 'input', clearable: true },
  { prop: 'name', label: '作业名称', type: 'input', clearable: true },
  {
    prop: 'status', label: '状态', type: 'select', clearable: true,
    options: [
      { label: '启用', value: 1 },
      { label: '停用', value: 0 },
    ],
  },
]

const searchModel = reactive({ code: '', name: '', status: '' })

// ==================== 表格列 ====================
const tableColumns = [
  { prop: 'code', label: '作业编码', width: 140 },
  { prop: 'name', label: '作业名称', width: 180 },
  { prop: 'cron', label: 'Cron表达式', width: 140 },
  {
    prop: 'triggerType', label: '触发方式', width: 100, align: 'center',
    type: 'tag', tagMap: { manual: '手动', scheduled: '定时', event: '事件' },
    tagTypeMap: { manual: 'info', scheduled: 'primary', event: 'warning' },
  },
  { prop: 'lastExecTime', label: '上次执行时间', width: 170 },
  { prop: 'nextExecTime', label: '下次执行时间', width: 170 },
  {
    prop: 'status', label: '状态', width: 80, align: 'center',
    type: 'tag', tagMap: { 1: '启用', 0: '停用' }, tagTypeMap: { 1: 'success', 0: 'danger' },
  },
]

// ==================== Mock 数据 ====================
const mockData = ref([
  {
    id: 1, code: 'JOB_STOCK_SYNC', name: '库存同步作业',
    cron: '0 0 2 * * ?', triggerType: 'scheduled',
    lastExecTime: '2026-04-16 02:00:00', nextExecTime: '2026-04-17 02:00:00',
    status: 1, params: '{"warehouseCode":"WH01"}', remark: '每日凌晨2点同步库存数据',
  },
  {
    id: 2, code: 'JOB_DATA_CLEAN', name: '数据清理作业',
    cron: '0 0 3 * * ?', triggerType: 'scheduled',
    lastExecTime: '2026-04-16 03:00:00', nextExecTime: '2026-04-17 03:00:00',
    status: 1, params: '{"retainDays":90}', remark: '每日凌晨3点清理过期数据',
  },
  {
    id: 3, code: 'JOB_REPORT_GEN', name: '报表生成作业',
    cron: '', triggerType: 'manual',
    lastExecTime: '2026-04-15 18:30:00', nextExecTime: '-',
    status: 1, params: '{"reportType":"daily"}', remark: '手动触发生成日报',
  },
])

// ==================== 数据加载 ====================
const loadFn = async (params) => {
  await new Promise(r => setTimeout(r, 400))
  let filtered = [...mockData.value]
  if (params.code) filtered = filtered.filter(d => d.code.includes(params.code))
  if (params.name) filtered = filtered.filter(d => d.name.includes(params.name))
  if (params.status !== '' && params.status !== undefined) filtered = filtered.filter(d => d.status === params.status)
  const start = (params.pageNum - 1) * params.pageSize
  return {
    data: filtered.slice(start, start + params.pageSize),
    total: filtered.length,
  }
}

// ==================== 弹窗 ====================
const dialogVisible = ref(false)
const dialogMode = ref('create')
const loading = ref(false)
const formData = ref({})

const formColumns = [
  { prop: 'code', label: '作业编码', type: 'input', required: true, maxlength: 30 },
  { prop: 'name', label: '作业名称', type: 'input', required: true, maxlength: 50 },
  { prop: 'cron', label: 'Cron表达式', type: 'input', maxlength: 50, placeholder: '例如: 0 0 2 * * ?' },
  {
    prop: 'triggerType', label: '触发方式', type: 'select', required: true,
    options: [
      { label: '手动', value: 'manual' },
      { label: '定时', value: 'scheduled' },
      { label: '事件', value: 'event' },
    ],
  },
  { prop: 'status', label: '状态', type: 'switch' },
  {
    prop: 'params', label: '作业参数', type: 'textarea', span: 24,
    rows: 3, maxlength: 500,
    placeholder: 'JSON格式参数，例如: {"key":"value"}',
  },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]

const handleAdd = () => {
  dialogMode.value = 'create'
  formData.value = { status: true, triggerType: 'manual' }
  dialogVisible.value = true
}

const handleEdit = (row) => {
  dialogMode.value = 'edit'
  formData.value = { ...row, status: !!row.status }
  dialogVisible.value = true
}

const handleDelete = (row) => {
  const idx = mockData.value.findIndex(d => d.id === row.id)
  if (idx > -1) mockData.value.splice(idx, 1)
  KhMessageFn.success('删除成功')
  pageRef.value?.reload()
}

const handleConfirm = (data) => {
  loading.value = true
  console.log('作业定义提交:', data)
  setTimeout(() => {
    if (dialogMode.value === 'create') {
      mockData.value.push({
        id: Date.now(),
        code: data.code,
        name: data.name,
        cron: data.cron || '',
        triggerType: data.triggerType,
        lastExecTime: '-',
        nextExecTime: '-',
        status: data.status ? 1 : 0,
        params: data.params || '',
        remark: data.remark || '',
      })
    } else {
      const target = mockData.value.find(d => d.id === formData.value.id)
      if (target) {
        Object.assign(target, {
          code: data.code,
          name: data.name,
          cron: data.cron || '',
          triggerType: data.triggerType,
          status: data.status ? 1 : 0,
          params: data.params || '',
          remark: data.remark || '',
        })
      }
    }
    loading.value = false
    dialogVisible.value = false
    KhMessageFn.success('操作成功')
    pageRef.value?.reload()
  }, 500)
}

// ==================== 执行作业 ====================
const handleToggleStatus = (row) => {
  const target = mockData.value.find(d => d.id === row.id)
  if (target) {
    target.status = target.status === 1 ? 0 : 1
    KhMessageFn.success(`已${target.status === 1 ? '启用' : '禁用'}作业【${row.name}】`)
    pageRef.value?.reload()
  }
}

const handleExecute = (row) => {
  KhMsgBoxFn.confirm(`确定要立即执行作业【${row.name}】吗?`, '执行确认', {
    confirmButtonText: '确定执行',
    cancelButtonText: '取消',
    type: 'warning',
  }).then(() => {
    KhMessageFn.success(`作业【${row.name}】已触发执行`)
    const target = mockData.value.find(d => d.id === row.id)
    if (target) {
      target.lastExecTime = new Date().toLocaleString('zh-CN', {
        year: 'numeric', month: '2-digit', day: '2-digit',
        hour: '2-digit', minute: '2-digit', second: '2-digit',
      }).replace(/\//g, '-')
    }
    pageRef.value?.reload()
  }).catch(() => { })
}
</script>
