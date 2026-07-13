<template>
  <KhPage
    ref="pageRef"
    module="attachment"
    :search-columns="searchColumns"
    :search-model="searchModel"
    :columns="tableColumns"
    :show-stat-cards="false"
    :show-toolbar="true"
    :show-index="true"
    :show-selection="true"
    :search-col-count="3"
    :crud-operations="crudOperations"
    :permission-prefix="'sys:attachment'"
    :toolbar-buttons="extraToolbarButtons"
  >
    <template #action="{ row }">
      <el-button type="success" link size="small" :icon="Download" @click="handleDownload(row)">下载</el-button>
    </template>
  </KhPage>

  <!-- 上传弹窗 -->
  <el-dialog v-model="uploadVisible" title="上传附件" width="500px" append-to-body>
    <KhUpload
      ref="uploadRef"
      action="/api/file/upload"
      :multiple="true"
      :max-size="50"
      :show-file-list="true"
      tip="支持上传任意文件，单个文件不超过 50MB"
      @success="handleUploadSuccess"
    />
  </el-dialog>
</template>

<script setup>
import KhUpload from '@/components/KhUpload/index.vue'
import { downloadFile } from '@/api/system'

const pageRef = ref(null)
const uploadVisible = ref(false)
const uploadRef = ref(null)

// ==================== 搜索 ====================
const searchColumns = [
  { prop: 'fileName', label: '文件名称', type: 'input', clearable: true },
  {
    prop: 'fileType', label: '文件类型', type: 'select', clearable: true,
    options: [
      { label: '图片', value: '图片' },
      { label: '文档', value: '文档' },
      { label: '视频', value: '视频' },
      { label: '其他', value: '其他' },
    ],
  },
  { prop: 'uploadTime', label: '上传时间', type: 'daterange', clearable: true },
]

const searchModel = reactive({ fileName: '', fileType: '', uploadTime: '' })

// ==================== 表格列 ====================
const fileTypeTagMap = {
  '图片': '',
  '文档': 'success',
  '视频': 'warning',
  '其他': 'info',
}

const tableColumns = [
  { prop: 'fileName', label: '文件名称', minWidth: 200, showOverflowTooltip: true },
  {
    prop: 'fileType', label: '文件类型', width: 100, align: 'center',
    type: 'tag', tagMap: fileTypeTagMap,
  },
  {
    prop: 'fileSize', label: '文件大小', width: 120, align: 'right',
    formatter: (row) => formatFileSize(row.fileSize),
  },
  { prop: 'uploadBy', label: '上传人ID', width: 100 },
  { prop: 'uploadTime', label: '上传时间', width: 170 },
]

// ==================== CRUD 配置 ====================
const crudOperations = {
  create: false,
  update: false,
  delete: true,
  view: true,
  export: false,
}

// ==================== 工具栏按钮 ====================
const extraToolbarButtons = [
  {
    label: '上传附件',
    type: 'primary',
    icon: 'Upload',
    permission: 'sys:attachment:upload',
    onClick: () => { uploadVisible.value = true },
  },
]

// ==================== 上传 ====================
const handleUploadSuccess = (response) => {
  if (response.code === 200) {
    uploadRef.value?.clearFiles()
    pageRef.value?.reload()
  } else {
    KhMessageFn.error(response.message || '上传失败')
  }
}

// ==================== 下载 ====================
const handleDownload = async (row) => {
  try {
    const res = await downloadFile(row.id)
    // 处理 blob 下载
    const blob = res instanceof Blob ? res : new Blob([res])
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = row.fileName || 'download'
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
  } catch {
    KhMessageFn.error('下载失败')
  }
}

// ==================== 文件大小格式化 ====================
function formatFileSize(size) {
  if (size == null) return ''
  if (size < 1024) return size + ' B'
  if (size < 1024 * 1024) return (size / 1024).toFixed(1) + ' KB'
  if (size < 1024 * 1024 * 1024) return (size / (1024 * 1024)).toFixed(1) + ' MB'
  return (size / (1024 * 1024 * 1024)).toFixed(2) + ' GB'
}
</script>
