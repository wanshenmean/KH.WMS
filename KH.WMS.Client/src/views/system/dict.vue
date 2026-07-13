<template>
  <div class="dict-page">
    <!-- 左侧：字典类型 -->
    <div class="dict-page__type">
      <div class="dict-page__type-header">
        <span class="dict-page__type-title">字典类型</span>
        <el-button type="primary" link :icon="Icons.Plus" @click="handleAddType">新增</el-button>
      </div>
      <el-input v-model="typeSearch" placeholder="搜索字典类型" clearable size="small" class="dict-page__type-search" />
      <div class="dict-page__type-list">
        <div v-for="item in filteredDictTypes" :key="item.id"
          :class="['dict-page__type-item', { 'dict-page__type-item--active': selectedType && selectedType.id === item.id }]"
          @click="handleSelectType(item)">
          <div class="dict-page__type-item-content">
            <div class="dict-page__type-item-name">
              {{ item.dictName }}
              <el-tag v-if="item.dataSourceType == '1'" size="small" type="warning" effect="plain"
                class="dict-page__type-tag">SQL</el-tag>
            </div>
            <div class="dict-page__type-item-code">{{ item.dictCode }}</div>
          </div>
          <el-button class="dict-page__type-item-edit" type="primary" link size="small" :icon="Icons.Edit"
            @click.stop="handleEditType(item)" />
        </div>
      </div>
    </div>

    <!-- 右侧：字典数据 -->
    <div class="dict-page__data">
      <KhPage v-if="selectedType" ref="pageRef" :title="`${selectedType.dictName} - 字典数据`"
        :search-columns="isSqlMode ? sqlSearchColumns : searchColumns"
        :search-model="isSqlMode ? sqlSearchModel : searchModel" :columns="isSqlMode ? sqlTableColumns : tableColumns"
        :load="loadFn" :show-stat-cards="false" :show-toolbar="true" :show-index="true" :search-col-count="4"
        @search="(model) => console.log('字典数据查询:', model)">
        <template #toolbar-left>
          <el-button v-if="!isSqlMode" type="primary" :icon="Icons.Plus" @click="handleAddData">新增字典值</el-button>
          <el-tag v-else type="info" effect="plain" size="small">SQL 数据源（只读）</el-tag>
        </template>
        <template v-if="!isSqlMode" #action="{ row }">
          <el-button type="primary" link size="small" @click="handleEditData(row)">编辑</el-button>
          <el-divider direction="vertical" />
          <el-popconfirm :title="`确定要${row.isActive === 1 ? '停用' : '启用'}吗？`" @confirm="handleToggleDataStatus(row)">
            <template #reference>
              <el-button type="warning" link size="small">{{ row.isActive === 1 ? '停用' : '启用' }}</el-button>
            </template>
          </el-popconfirm>
          <el-divider direction="vertical" />
          <el-popconfirm title="确定删除该字典值?" @confirm="handleDeleteData(row)">
            <template #reference>
              <el-button type="danger" link size="small">删除</el-button>
            </template>
          </el-popconfirm>
        </template>
      </KhPage>
      <div v-else class="dict-page__data-empty">
        <el-empty description="请选择左侧字典类型" />
      </div>
    </div>

    <!-- 字典类型弹窗 -->
    <KhDialog v-model="typeDialogVisible" :title="typeDialogMode === 'create' ? '字典管理 - 新增类型' : '字典管理 - 编辑类型'"
      width="600px" :form-columns="typeFormColumns" :form-model="typeFormData" :form-col-count="1"
      :confirm-loading="typeLoading" @confirm="handleConfirmType" />

    <!-- 字典数据弹窗 -->
    <KhDialog v-model="dataDialogVisible" :title="dataDialogMode === 'create' ? '字典管理 - 新增字典值' : '字典管理 - 编辑字典值'"
      width="800px" :form-columns="dataFormColumns" :form-model="dataFormData" :form-col-count="2"
      :confirm-loading="dataLoading" @confirm="handleConfirmData" />
  </div>
</template>

<script setup>
import KhPage from '@/components/KhPage/index.vue'
import KhDialog from '@/components/KhDialog/index.vue'
import { useDictStore } from '@/stores/dict'
import { getDictTypeList, createDictType, updateDictType, getDictDataList, createDictData, updateDictData, deleteDictData } from '@/api/system'

const dictStore = useDictStore()

const Icons = markRaw({ Plus, Edit })
const pageRef = ref(null)

// ==================== 字典类型 ====================
const typeSearch = ref('')

const dictTypes = ref([])

const filteredDictTypes = computed(() => {
  return dictTypes.value.filter(d =>
    d.dictName.includes(typeSearch.value) || d.dictCode.includes(typeSearch.value)
  )
})

const selectedType = ref(null)
const currentMode = ref(false)

/** 当前选中的字典类型是否为 SQL 数据源（只读） */
const isSqlMode = computed(() => selectedType.value?.dataSourceType == '1')

// 加载字典类型列表
onMounted(async () => {
  try {
    const response = await getDictTypeList()
    if (response.code == 200) {
      dictTypes.value = response.data
    }
  } catch (error) {
    console.error('加载字典类型失败:', error)
  }
})

const handleSelectType = (item) => {
  selectedType.value = item
  pageRef.value?.reload()
}

// 字典类型弹窗
const typeDialogVisible = ref(false)
const typeDialogMode = ref('create')
const typeLoading = ref(false)
const typeDataSourceType = ref('0')
const typeFormData = ref({
  dictName: '',
  dictCode: '',
  dataSourceType: '0',
  sqlQuery: '',
  isActive: 1,
  remark: '',
  labelColumn: '',
  valueColumn: '',
})

const typeFormColumns = computed(() => [
  { prop: 'dictName', label: '字典名称', type: 'input', required: true, maxlength: 50 },
  { prop: 'dictCode', label: '字典编码', type: 'input', required: true, maxlength: 50 },
  {
    prop: 'dataSourceType', label: '数据来源', type: 'radio', required: true, span: 24,
    options: [{ label: '手动管理', value: '0' }, { label: 'SQL 查询', value: '1' }],
    onChange: (val) => { typeDataSourceType.value = val },
  },
  {
    prop: 'sqlQuery', label: 'SQL 语句', type: 'textarea', span: 24, maxlength: 1000, rows: 4,
    placeholder: '请输入查询 SQL，如: SELECT StatusName AS itemLabel, StatusCode AS itemValue, Color AS tagColor, SortNo AS sortOrder FROM cfg_location_status WHERE Status=1',
    hidden: typeDataSourceType.value != '1', required: typeDataSourceType.value === '1',
  },
  { prop: 'labelColumn', label: '标签列名', type: 'input', required: typeDataSourceType.value == '1', maxlength: 50, hidden: typeDataSourceType.value != '1', },
  { prop: 'valueColumn', label: '值列名', type: 'input', required: typeDataSourceType.value == '1', maxlength: 50, hidden: typeDataSourceType.value != '1', },
  { prop: 'isActive', label: '状态', type: 'radio', required: true, options: [{ label: '启用', value: 1 }, { label: '停用', value: 0 }] },
  { prop: 'remark', label: '备注', type: 'textarea', maxlength: 200 },
])

const handleAddType = () => {
  typeDialogMode.value = 'create'
  typeFormData.value = { isActive: 1 }
  typeDataSourceType.value = '0'
  typeDialogVisible.value = true
}

const handleEditType = (item) => {
  typeDialogMode.value = 'edit'
  typeFormData.value = { ...item }
  typeDataSourceType.value = item.dataSourceType ?? '0'
  typeDialogVisible.value = true
}

const handleConfirmType = async (data) => {
  if (data.dataSourceType === 'sql' && !data.dataSourceSql?.trim()) {
    KhMessageFn.warning('SQL 查询模式下，SQL 语句不能为空')
    return
  }
  typeLoading.value = true
  try {
    const res = typeDialogMode.value === 'create'
      ? await createDictType(data)
      : await updateDictType(data)
    if (res.code === 200) {
      typeDialogVisible.value = false
      KhMessageFn.success('操作成功')
      // 刷新左侧类型列表
      const response = await getDictTypeList()
      if (response.code === 200) dictTypes.value = response.data
    }
  } catch (error) {
    console.error('字典类型提交失败:', error)
  } finally {
    typeLoading.value = false
  }
}

// ==================== 字典数据 ====================

// --- 手动管理模式 ---
const searchColumns = [
  { prop: 'dictLabel', label: '字典标签', type: 'input', clearable: true },
  { prop: 'isActive', label: '状态', type: 'select', clearable: true, options: [{ label: '启用', value: 1 }, { label: '停用', value: 0 }] },
]

const searchModel = reactive({ dictLabel: '', isActive: '' })

const tableColumns = [
  { prop: 'itemLabel', label: '字典标签', width: 160 },
  { prop: 'itemValue', label: '字典值', width: 120 },
  { prop: 'sortOrder', label: '排序', width: 80, align: 'center' },
  { prop: 'tagColor', label: '样式属性', width: 100, type: 'tag', tagMap: 'dict:color_flag', },
  {
    prop: 'isActive', label: '状态', width: 80, align: 'center',
    type: 'tag', tagMap: { 1: '启用', 0: '停用' }, tagTypeMap: { 1: 'success', 0: 'danger' },
  },
  { prop: 'remark', label: '备注', minWidth: 160, showOverflowTooltip: true },
  { prop: 'createdTime', label: '创建时间', width: 170 },
]

// --- SQL 查询模式（只读，列由后端 SQL 结果动态决定，此处为通用默认列） ---
const sqlSearchColumns = [
  { prop: 'itemLabel', label: '字典标签', type: 'input', clearable: true },
]

const sqlSearchModel = reactive({ dictLabel: '' })

const sqlTableColumns = [
  { prop: 'itemLabel', label: '字典标签', width: 160 },
  { prop: 'itemValue', label: '字典值', width: 120 },
  { prop: 'tagColor', label: '颜色', width: 80, align: 'center' },
  { prop: 'sortOrder', label: '排序', width: 80, align: 'center' },
  { prop: 'remark', label: '备注', minWidth: 200, showOverflowTooltip: true },
]

const loadFn = async (params) => {
  const res = await getDictDataList(selectedType.value?.id, params)

  return res.code === 200 ? (selectedType.value?.dataSourceType != '1' ? { data: res.data.items, total: res.data.total } :
    { data: res.data, total: res.data.length }) : { data: [], total: 0 }

}

// 字典数据弹窗
const dataDialogVisible = ref(false)
const dataDialogMode = ref('create')
const dataLoading = ref(false)
const dataFormData = ref({

})

const dataFormColumns = [
  { prop: 'itemLabel', label: '字典标签', type: 'input', required: true, maxlength: 50 },
  { prop: 'itemValue', label: '字典值', type: 'input', required: true, maxlength: 50 },
  { prop: 'sortOrder', label: '排序', type: 'number', min: 1 },
  {
    prop: 'tagColor', label: '样式属性', type: 'select', clearable: true,
    options: [
      { label: 'primary(蓝色)', value: 'primary' },
      { label: 'success(绿色)', value: 'success' },
      { label: 'warning(橙色)', value: 'warning' },
      { label: 'danger(红色)', value: 'danger' },
      { label: 'info(灰色)', value: 'info' },
    ],
  },
  { prop: 'isActive', label: '状态', type: 'radio', required: true, options: [{ label: '启用', value: 1 }, { label: '停用', value: 0 }] },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 200 },
]

const handleAddData = () => {
  dataDialogMode.value = 'create'
  dataFormData.value = { isActive: 1, sort: 1 }
  dataDialogVisible.value = true
}

const handleEditData = (row) => {
  dataDialogMode.value = 'edit'
  dataFormData.value = { ...row }
  dataDialogVisible.value = true
}

const handleDeleteData = async (row) => {
  try {
    const res = await deleteDictData(row.id)
    if (res.code === 200) {
      KhMessageFn.success('删除成功')
      dictStore.refreshDict(selectedType.value?.dictCode)
      pageRef.value?.reload()
    }
  } catch (error) {
    console.error('删除字典数据失败:', error)
  }
}

const handleToggleDataStatus = async (row) => {
  try {
    const payload = { ...row, isActive: row.isActive === 1 ? 0 : 1 }
    const res = await updateDictData(payload)
    if (res.code === 200) {
      KhMessageFn.success(`已${row.isActive === 1 ? '停用' : '启用'}该字典值`)
      dictStore.refreshDict(selectedType.value?.dictCode)
      pageRef.value?.reload()
    }
  } catch (error) {
    console.error('切换字典数据状态失败:', error)
  }
}

const handleConfirmData = async (data) => {
  dataLoading.value = true
  try {
    console.log('提交的数据:', dataDialogMode.value)
    const payload = { ...data, dictCode: selectedType.value?.dictCode, dictTypeId: selectedType.value?.id }
    const res = dataDialogMode.value === 'create'
      ? await createDictData(payload)
      : await updateDictData(payload)
    if (res.code === 200) {
      dataDialogVisible.value = false
      KhMessageFn.success('操作成功')
      dictStore.refreshDict(selectedType.value?.dictCode)
      pageRef.value?.reload()
    }
  } catch (error) {
    console.error('字典数据提交失败:', error)
  } finally {
    dataLoading.value = false
  }
}
</script>

<style scoped>
.dict-page {
  display: flex;
  gap: 16px;
  height: 100%;
}

.dict-page__type {
  width: 260px;
  flex-shrink: 0;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.dict-page__type-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 16px 8px;
}

.dict-page__type-title {
  font-size: 15px;
  font-weight: 600;
  color: #303133;
}

.dict-page__type-search {
  margin: 0 12px 8px;
}

.dict-page__type-list {
  flex: 1;
  overflow-y: auto;
  padding: 0 8px 8px;
}

.dict-page__type-item {
  padding: 10px 12px;
  border-radius: 6px;
  cursor: pointer;
  transition: background-color 0.2s;
  margin-bottom: 2px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.dict-page__type-item-content {
  flex: 1;
  min-width: 0;
}

.dict-page__type-item-edit {
  opacity: 0;
  flex-shrink: 0;
}

.dict-page__type-item:hover .dict-page__type-item-edit {
  opacity: 1;
}

.dict-page__type-item:hover {
  background: #f0f2f5;
}

.dict-page__type-item--active {
  background: #e8f3ff;
  border-left: 3px solid #409eff;
}

.dict-page__type-item-name {
  font-size: 14px;
  color: #303133;
  margin-bottom: 2px;
  display: flex;
  align-items: center;
  gap: 6px;
}

.dict-page__type-tag {
  transform: scale(0.8);
  transform-origin: left center;
}

.dict-page__type-item-code {
  font-size: 12px;
  color: #909399;
}

.dict-page__data {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.dict-page__data-empty {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}
</style>
