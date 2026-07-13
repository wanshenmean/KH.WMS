<template>
  <div style="height: 100%; display: flex; flex-direction: column;">
    <KhPage
      ref="pageRef"
      title="全局配置管理"
      :search-columns="searchColumns"
      :search-model="searchModel"
      :columns="tableColumns"
      :load="loadData"
      :show-stat-cards="false"
      :show-toolbar="true"
      :show-index="true"
      :show-pagination="false"
      :show-selection="false"
      :show-header-filter="true"
      :search-col-count="3"
      :permission-prefix="'cfg:global_config'"
    >
      <template #toolbar-left>
        <el-button type="primary" :loading="saveLoading" @click="handleBatchSave">
          <el-icon><Check /></el-icon> 批量保存
        </el-button>
        <el-popconfirm title="确定将该分组所有配置恢复为默认值?" @confirm="handleResetClick" :disabled="!searchModel.configGroup">
          <template #reference>
            <el-button type="warning" :loading="resetLoading" :disabled="!searchModel.configGroup">
              <el-icon><RefreshLeft /></el-icon> 重置默认值
            </el-button>
          </template>
        </el-popconfirm>
      </template>

      <!-- 配置值列：内联编辑 -->
      <template #configValue="{ row }">
        <el-select
          v-if="row.valueType === 'ENUM' && row.optionList && row.optionList.length > 0"
          v-model="editMap[row.id]"
          placeholder="请选择"
          clearable
          style="width: 100%"
        >
          <el-option v-for="opt in row.optionList" :key="opt" :label="opt" :value="opt" />
        </el-select>
        <el-switch
          v-else-if="row.valueType === 'BOOLEAN'"
          v-model="editMap[row.id]"
          active-value="true"
          inactive-value="false"
        />
        <el-input v-else v-model="editMap[row.id]" :placeholder="row.defaultValue || ''" />
      </template>

      <!-- 操作列：修改状态 + 恢复默认值 -->
      <template #action="{ row }">
        <el-button
          v-if="editMap[row.id] !== row.configValue"
          type="warning"
          link
          @click="editMap[row.id] = row.configValue"
        >
          撤销
        </el-button>
        <el-button
          v-if="row.defaultValue && row.defaultValue !== row.configValue"
          type="info"
          link
          @click="editMap[row.id] = row.defaultValue"
        >
          恢复默认
        </el-button>
        <el-divider direction="vertical" />
        <el-popconfirm :title="`确定要${row.status === 1 ? '禁用' : '启用'}吗？`" @confirm="handleToggleStatus(row)">
          <template #reference>
            <el-button type="warning" link size="small">{{ row.status === 1 ? '禁用' : '启用' }}</el-button>
          </template>
        </el-popconfirm>
      </template>
    </KhPage>
  </div>
</template>

<script setup>
import { getGlobalConfigGroups, batchUpdateGlobalConfig, setGlobalConfigStatus, resetGlobalConfig } from '@/api/config'

const pageRef = ref(null)
const saveLoading = ref(false)
const resetLoading = ref(false)
const allData = ref([])
const editMap = reactive({})

// ==================== 搜索 ====================
const searchColumns = [
  {
    prop: 'configGroup', label: '配置分组', type: 'select', clearable: true, options: 'dict:config_group',
  },
  { prop: 'configKey', label: '配置键', type: 'input', clearable: true },
  { prop: 'configName', label: '配置名称', type: 'input', clearable: true },
]

const searchModel = reactive({
  configGroup: '',
  configKey: '',
  configName: '',
})

// ==================== 表格列 ====================
const tableColumns = [
  {
    prop: 'configGroup', label: '分组', width: 120, type: 'tag', tagMap: 'dict:config_group',
  },
  { prop: 'configKey', label: '配置键', width: 220 },
  { prop: 'configName', label: '配置名称', width: 180 },
  { prop: 'configValue', label: '配置值', minWidth: 200, type: 'slot' },
  { prop: 'defaultValue', label: '默认值', width: 140, showOverflowTooltip: true },
  {
    prop: 'valueType', label: '值类型', width: 90, type: 'tag', tagMap: 'dict:config_value_type',
  },
  {
    prop: 'scopeLevel', label: '作用域', width: 90, type: 'tag', tagMap: 'dict:config_scope_level',
  },
  { prop: 'description', label: '描述', minWidth: 160, showOverflowTooltip: true },
  { prop: 'sortNo', label: '排序', width: 70, align: 'center' },
  { prop: 'status', label: '状态', width: 70, align: 'center', type: 'tag', tagMap: 'dict:status_flag' },
]

// ==================== 数据加载 ====================
const loadData = async (params) => {
  try {
    const res = await getGlobalConfigGroups()
    const groups = res || []
    // 扁平化所有配置项
    const flatItems = []
    groups.forEach(group => {
      group.items.forEach(item => {
        flatItems.push(item)
      })
    })
    allData.value = flatItems
    // 初始化编辑映射
    flatItems.forEach(item => {
      if (editMap[item.id] === undefined) {
        editMap[item.id] = item.configValue ?? ''
      }
    })
  } catch {
    // request.js 已处理错误
  }

  // 客户端过滤
  let filtered = [...allData.value]
  if (params.configGroup) {
    filtered = filtered.filter(d => d.configGroup === params.configGroup)
  }
  if (params.configKey) {
    filtered = filtered.filter(d => d.configKey && d.configKey.includes(params.configKey))
  }
  if (params.configName) {
    filtered = filtered.filter(d => d.configName && d.configName.includes(params.configName))
  }

  return { data: filtered, total: filtered.length }
}

// ==================== 批量保存 ====================
const handleBatchSave = async () => {
  const changedItems = allData.value.filter(item => item.configValue !== editMap[item.id])
  if (changedItems.length === 0) {
    KhMessageFn.info('没有需要保存的修改')
    return
  }

  saveLoading.value = true
  try {
    await batchUpdateGlobalConfig(changedItems.map(item => ({
      id: item.id,
      configValue: editMap[item.id],
    })))
    KhMessageFn.success('保存成功')
    pageRef.value?.reload()
  } catch {
    // request.js 已处理错误
  } finally {
    saveLoading.value = false
  }
}

// ==================== 启用/禁用 ====================
const handleToggleStatus = async (row) => {
  const newStatus = row.status === 1 ? 0 : 1
  try {
    const res = await setGlobalConfigStatus(row.id, newStatus)
    if (res?.code === 200) {
      KhMessageFn.success(res.message || '操作成功')
      pageRef.value?.reload()
    }
  } catch {
    // request.js 已处理错误
  }
}

// ==================== 重置默认值 ====================
const handleResetClick = async () => {
  if (!searchModel.configGroup) {
    KhMessageFn.warning('请先选择一个配置分组')
    return
  }

  resetLoading.value = true
  try {
    await resetGlobalConfig(searchModel.configGroup)
    KhMessageFn.success('已重置分组所有配置为默认值')
    pageRef.value?.reload()
  } catch {
    // request.js 已处理错误
  } finally {
    resetLoading.value = false
  }
}
</script>
