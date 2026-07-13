<template>
  <div class="log-browser">
    <!-- 左侧文件树 -->
    <div class="log-browser__left">
      <div class="log-browser__left-header">
        <span>日志文件</span>
        <el-button link type="primary" @click="loadFiles">刷新</el-button>
      </div>
      <el-tree
        ref="treeRef"
        :data="treeData"
        :props="treeProps"
        show-checkbox
        node-key="fileName"
        default-expand-all
        @check="handleCheck"
      />
      <div class="log-browser__left-tip">勾选文件查看，可多选拼接</div>
    </div>

    <!-- 右侧内容 -->
    <div class="log-browser__right">
      <div class="log-browser__toolbar">
        <el-input v-model="keyword" placeholder="关键字" clearable style="width: 180px"
          @keyup.enter="reloadContent" @clear="reloadContent" />
        <el-select v-model="levels" multiple collapse-tags collapse-tags-tooltip placeholder="级别" clearable
          style="width: 140px" @change="reloadContent">
          <el-option label="信息 INF" value="INF" />
          <el-option label="警告 WRN" value="WRN" />
          <el-option label="错误 ERR" value="ERR" />
          <el-option label="致命 FAT" value="FAT" />
        </el-select>
        <el-select v-model="logType" placeholder="日志类型" clearable filterable style="width: 150px"
          @change="reloadContent">
          <el-option v-for="t in logTypeOptions" :key="t.value" :label="t.label" :value="t.value" />
        </el-select>
        <el-input v-model="requestId" placeholder="RequestId 追踪" clearable style="width: 200px"
          @keyup.enter="reloadContent" @clear="reloadContent" />
        <el-select v-model="moduleCode" placeholder="模块" clearable filterable style="width: 130px"
          @change="reloadContent">
          <el-option v-for="m in moduleOptions" :key="m.value" :label="m.label" :value="m.value" />
        </el-select>
        <el-date-picker v-model="timeRange" type="datetimerange" range-separator="至" start-placeholder="开始时间"
          end-placeholder="结束时间" value-format="YYYY-MM-DD HH:mm:ss" clearable style="width: 360px"
          @change="reloadContent" />
        <el-button type="primary" @click="reloadContent">查询</el-button>
        <el-button @click="loadMore" :disabled="!hasMore || loading">加载更多</el-button>
        <span class="log-browser__info">{{ selectedFiles.length }} 文件 · {{ lines.length }} 行{{ hasMore ? '+' : '' }}</span>
      </div>

      <div v-loading="loading" ref="contentRef" class="log-browser__content">
        <div v-for="line in lines" :key="line.lineNo + '-' + line.sourceFile"
          class="log-line" :class="'log-line--' + (levelOf(line.content) || 'TEXT')">
          <span class="log-line__no">{{ line.lineNo }}</span>
          <span v-if="selectedFiles.length > 1" class="log-line__src">[{{ shortName(line.sourceFile) }}]</span>
          <span class="log-line__text" v-html="highlight(line.content)"></span>
        </div>
        <el-empty v-if="!loading && lines.length === 0" description="暂无日志，请在左侧勾选文件" :image-size="60" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { getLogFiles, getLogContent } from '@/api/system'

const treeRef = ref(null)
const contentRef = ref(null)
const treeData = ref([])
const treeProps = { label: 'label', children: 'children' }

const keyword = ref('')
const levels = ref([])
const logType = ref('')
const requestId = ref('')
const moduleCode = ref('')
const timeRange = ref([])

const moduleOptions = [
  { label: '入库', value: '2001' },
  { label: '出库', value: '2002' },
  { label: '库存', value: '2003' },
  { label: '基础数据', value: '2005' },
  { label: '仓库', value: '2006' },
  { label: '任务/移库', value: '2010' },
  { label: '策略', value: '2017' },
  { label: '配置/单据', value: '2018' },
]

const logTypeOptions = [
  { label: '系统 System', value: 'System' },
  { label: '异常 Exception', value: 'Exception' },
  { label: '操作 Operation', value: 'Operation' },
  { label: '业务 Business', value: 'Business' },
  { label: '性能 Performance', value: 'Performance' },
]
const lines = ref([])
const hasMore = ref(false)
const startLine = ref(0)
const pageSize = 500
const loading = ref(false)

const selectedFiles = computed(() => {
  if (!treeRef.value) return []
  return (treeRef.value.getCheckedNodes(true, false) || [])
    .filter(n => n.fileName)
    .map(n => n.fileName)
})

const levelOf = (content) => {
  const m = /^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2} \[(\w{3})\]/.exec(content || '')
  return m ? m[1] : ''
}

const shortName = (f) => (f || '').replace(/^\//, '')

const escapeHtml = (s) => (s || '')
  .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')

const highlight = (text) => {
  const safe = escapeHtml(text)
  if (!keyword.value) return safe
  const kw = keyword.value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
  return safe.replace(new RegExp(kw, 'gi'), '<mark>$&</mark>')
}

const loadFiles = async () => {
  try {
    const res = await getLogFiles()
    const files = res.data || []
    const groups = {}
    files.forEach(f => {
      (groups[f.category] = groups[f.category] || []).push(f)
    })
    treeData.value = Object.keys(groups).map(cat => ({
      label: `${cat} (${groups[cat].length})`,
      children: groups[cat].map(f => ({
        label: `${f.date || f.fileName} · ${f.sizeKB}KB`,
        fileName: f.fileName,
      })),
    }))
  } catch { /* request.js 已提示 */ }
}

const handleCheck = () => reloadContent()

let debounceTimer = null
const reloadContent = () => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(async () => {
    startLine.value = 0
    lines.value = []
    await fetchContent(false)
    nextTick(() => { if (contentRef.value) contentRef.value.scrollTop = 0 })
  }, 300)
}

const loadMore = async () => {
  if (!hasMore.value || loading.value) return
  await fetchContent(true)
}

const fetchContent = async (append) => {
  if (selectedFiles.value.length === 0) {
    hasMore.value = false
    lines.value = []
    return
  }
  loading.value = true
  try {
    const start = append ? startLine.value : 0
    const res = await getLogContent({
      fileNames: selectedFiles.value,
      startLine: start,
      lineCount: pageSize,
      keyword: keyword.value || undefined,
      levels: levels.value.length ? levels.value : undefined,
      logType: logType.value || undefined,
      requestId: requestId.value || undefined,
      moduleCode: moduleCode.value || undefined,
      startTime: timeRange.value?.[0] || undefined,
      endTime: timeRange.value?.[1] || undefined,
    })
    const data = res.data || {}
    const got = data.lines || []
    if (append) lines.value.push(...got)
    else lines.value = got
    hasMore.value = data.hasMore
    startLine.value = start + got.length
  } catch { /* request.js 已提示 */ } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadFiles()
})
</script>

<style scoped>
.log-browser {
  display: flex;
  gap: 12px;
  height: 100%;
  min-height: 0;
  flex: 1;
}

.log-browser__left {
  width: 260px;
  flex-shrink: 0;
  background: #fff;
  border-radius: 8px;
  padding: 12px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}

.log-browser__left-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-weight: 600;
  font-size: 15px;
  margin-bottom: 8px;
}

.log-browser__left-tip {
  margin-top: auto;
  padding-top: 8px;
  font-size: 13px;
  color: #c0c4cc;
}

.log-browser__right {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  background: #fff;
  border-radius: 8px;
  padding: 12px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
  overflow: hidden;
}

.log-browser__toolbar {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
  flex-shrink: 0;
  flex-wrap: wrap;
}

.log-browser__info {
  margin-left: auto;
  font-size: 13px;
  color: #909399;
}

.log-browser__content {
  flex: 1;
  min-height: 0;
  overflow-y: auto;
  background: #fafafa;
  border: 1px solid #ebeef5;
  border-radius: 6px;
  padding: 8px 0;
  font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
  font-size: 14px;
  line-height: 1.8;
}

.log-line {
  display: flex;
  padding: 0 12px;
  color: #303133;
  white-space: pre-wrap;
  word-break: break-all;
}

.log-line:hover {
  background: #f5f7fa;
}

.log-line__no {
  color: #c0c4cc;
  margin-right: 12px;
  flex-shrink: 0;
  user-select: none;
  min-width: 52px;
  text-align: right;
}

.log-line__src {
  color: #409eff;
  margin-right: 12px;
  flex-shrink: 0;
}

.log-line__text {
  flex: 1;
}

.log-line--ERR,
.log-line--FAT {
  color: #f56c6c;
}

.log-line--WRN {
  color: #e6a23c;
}

.log-line--INF {
  color: #606266;
}

.log-line :deep(mark) {
  background: #fef0f0;
  color: #f56c6c;
  border-radius: 2px;
  padding: 0 2px;
}
</style>
