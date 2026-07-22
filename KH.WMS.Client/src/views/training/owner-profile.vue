<template>
  <KhPage ref="pageRef" title="货主 ExtData 实战" module="training-owner-profile"
    :search-columns="searchColumns" :search-model="searchModel" :columns="tableColumns"
    :form-columns="formColumns" :load="load" :before-submit="beforeSubmit"
    :show-stat-cards="false" :show-toolbar="true" :show-index="true" :show-header-filter="true"
    :crud-operations="{ create: true, update: true, delete: true, view: true, export: true }"
    permission-prefix="training:owner" />
</template>

<script setup>
import { useCrudApi } from '@/utils/crud'
import { useExtFields } from '@/utils/useExtFields'

const pageRef = ref(null)
const crudApi = useCrudApi('training-owner-profile')
const { loadExtConfig, mergedColumns, mergedTableColumns, extractAndCleanExtData, withFlatExtLoad } =
  useExtFields('/api/training-owner-profile/form-config')

const searchModel = reactive({ ownerCode: '', ownerName: '' })
const searchColumns = [
  { prop: 'ownerCode', label: '货主编码', type: 'input', clearable: true },
  { prop: 'ownerName', label: '货主名称', type: 'input', clearable: true },
]
const baseTableColumns = [
  { prop: 'ownerCode', label: '货主编码', width: 140 },
  { prop: 'ownerName', label: '货主名称', minWidth: 160 },
  { prop: 'contactName', label: '联系人', width: 100 },
  { prop: 'contactPhone', label: '联系电话', width: 130 },
  { prop: 'address', label: '地址', minWidth: 180, showOverflowTooltip: true },
  { prop: 'remark', label: '备注', minWidth: 150, showOverflowTooltip: true },
]
const baseFormColumns = [
  { prop: 'ownerCode', label: '货主编码', type: 'input', required: true, maxlength: 30 },
  { prop: 'ownerName', label: '货主名称', type: 'input', required: true, maxlength: 100 },
  { prop: 'contactName', label: '联系人', type: 'input', maxlength: 50 },
  { prop: 'contactPhone', label: '联系电话', type: 'input', maxlength: 20 },
  { prop: 'address', label: '地址', type: 'textarea', span: 24, maxlength: 500 },
  { prop: 'remark', label: '备注', type: 'textarea', span: 24, maxlength: 500 },
]
const tableColumns = computed(() => mergedTableColumns(baseTableColumns))
const formColumns = computed(() => mergedColumns(baseFormColumns))
const load = withFlatExtLoad(crudApi, searchColumns)
const beforeSubmit = (data) => {
  data.extDataRaw = extractAndCleanExtData(data) || '{}'
}

onMounted(loadExtConfig)
</script>
