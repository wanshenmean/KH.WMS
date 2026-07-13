<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <!-- Kh 组件无需 import，模板中直接使用 -->
    <KhPage
      ref="pageRef"
      title="示例页面"
      module="inbound-order"
      :search-columns="searchColumns"
      :search-model="searchModel"
      :columns="tableColumns"
      :show-stat-cards="false"
      :show-toolbar="true"
      :show-index="true"
      :crud-operations="crudOperations"
      :permission-prefix="'in:order'"
      :detail-lines="detailLineConfigs"
      :detail-width="'800px'"
    />

    <!-- KhDialog、KhForm 也无需 import -->
    <KhDialog
      v-model="dialogVisible"
      title="示例弹窗"
      width="600px"
      :confirm-loading="submitLoading"
      @confirm="handleSubmit"
    >
      <template #default>
        <KhForm ref="formRef" :columns="formColumns" v-model="formData" :col-count="2" />
      </template>
    </KhDialog>
  </div>
</template>

<script setup>
// ============================================================
//  Vue API（ref / reactive / computed / onMounted 等）无需 import
//  —— 由 unplugin-auto-import 自动注入
// ============================================================
// import { ref, reactive, computed, onMounted } from 'vue'  ← 不需要了

// ============================================================
//  Kh 系列组件（KhPage / KhDialog / KhForm 等）无需 import
//  —— 由 unplugin-vue-components 自动按需导入（扫描 src/components）
// ============================================================
// import KhPage from '@/components/KhPage/index.vue'         ← 不需要了
// import KhDialog from '@/components/KhDialog/index.vue'     ← 不需要了
// import { KhEditableTable } from '@/components'             ← 不需要了

// ============================================================
//  API 无需逐个 import，通过 useApi() 获取全局挂载的 API
// ============================================================
import { useApi } from '@/composables/useApi'
import { useCrudApi } from '@/utils/crud'
import { useExtFields } from '@/utils/useExtFields'

// 仍然需要显式 import 的：
// 1. 本目录下的子组件（非 src/components 的组件）
// import ReceiveDialog from './components/ReceiveDialog.vue'
// 2. 工具函数（utils 下的 composable）
// import { useCrudApi } from '@/utils/crud'
// 3. Element Plus 图标（Plus / Download 等）、KhMessageFn 等均已全局注入，无需 import
// 2. 本目录下的子组件（非 src/components 的组件）
// import ReceiveDialog from './components/ReceiveDialog.vue'
// 3. 工具函数（utils 下的 composable）
// import { useCrudApi } from '@/utils/crud'
// 4. KhMessageFn / KhMsgBoxFn / KhNotifyFn 也已全局注入，无需 import

// ─── API 获取 ──────────────────────────────────────────────
const $api = useApi()
// 解构出本页面用到的 API（来自 api/inbound.js）
const { createInboundOrder, updateInboundOrder, getInboundOrderDetail } = $api

// ─── 响应式数据（无需 import ref/reactive）─────────────────
const pageRef = ref(null)
const formRef = ref(null)
const dialogVisible = ref(false)
const submitLoading = ref(false)
const formData = reactive({})

const searchModel = reactive({
  orderNo: '',
  orderStatus: '',
})

// ─── 搜索列配置 ────────────────────────────────────────────
const searchColumns = [
  { prop: 'orderNo', label: '入库单号', type: 'input', clearable: true },
  {
    prop: 'orderStatus',
    label: '单据状态',
    type: 'select',
    clearable: true,
    options: 'dict:inbound_order_status',
  },
]

// ─── 表格列 ────────────────────────────────────────────────
const tableColumns = [
  { prop: 'orderNo', label: '入库单号', width: 160, fixed: 'left' },
  { prop: 'orderType', label: '入库类型', width: 100 },
  { prop: 'sourceDocNo', label: '来源单号', width: 150, showOverflowTooltip: true },
  {
    prop: 'orderStatus', label: '状态', width: 100, align: 'center',
    type: 'tag', tagMap: 'dict:inbound_order_status',
  },
  { prop: 'totalLines', label: '行数', width: 80, align: 'center' },
  { prop: 'createdTime', label: '创建时间', width: 170 },
]

// ─── CRUD 配置 ─────────────────────────────────────────────
const crudOperations = {
  create: false,
  update: false,
  delete: true,
  view: true,
  export: false,
}

// ─── 详情行 ────────────────────────────────────────────────
const detailLineConfigs = [
  {
    prop: 'items',
    title: '入库明细',
    columns: [
      { prop: 'materialCode', label: '物料编码', width: 130 },
      { prop: 'materialName', label: '物料名称', minWidth: 160 },
      { prop: 'orderedQty', label: '计划数量', width: 100, align: 'right' },
      { prop: 'receivedQty', label: '已收数量', width: 100, align: 'right' },
      { prop: 'unit', label: '单位', width: 70, align: 'center' },
    ],
  },
]

// ─── 表单列 ────────────────────────────────────────────────
const formColumns = [
  { prop: 'orderNo', label: '入库单号', type: 'input', disabled: true, span: 12 },
  { prop: 'orderType', label: '入库类型', type: 'select', required: true, options: 'dict:inbound_type', span: 12 },
  { prop: 'sourceDocNo', label: '来源单号', type: 'input', span: 12 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]

// ─── 提交 ──────────────────────────────────────────────────
const handleSubmit = async () => {
  submitLoading.value = true
  try {
    await createInboundOrder(formData)
    // KhMessageFn 已全局注入，直接使用（无需 import）
    KhMessageFn.success('操作成功')
    dialogVisible.value = false
    pageRef.value?.reload()
  } catch {
    // 错误已在拦截器中处理
  } finally {
    submitLoading.value = false
  }
}

// ─── 生命周期（无需 import onMounted）──────────────────────
onMounted(() => {
  // 页面初始化逻辑
})
</script>
