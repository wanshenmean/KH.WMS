<template>
  <div class="location-map-page">
    <h2 class="page-title">库位图</h2>

    <!-- 筛选工具栏 -->
    <div class="location-map-page__toolbar">
      <div class="toolbar-left">
        <el-select v-model="selectedWarehouse" placeholder="选择仓库" style="width: 170px" @change="handleWarehouseChange">
          <el-option label="华东立体仓库" value="WH-A" />
          <el-option label="华南平面仓库" value="WH-B" />
          <el-option label="华北冷库" value="WH-C" />
        </el-select>
        <el-select v-model="selectedZone" placeholder="选择库区" style="width: 180px" @change="generateGrid">
          <el-option v-for="z in zoneOptions" :key="z.value" :label="z.label" :value="z.value" />
        </el-select>
        <el-select v-model="selectedLayer" placeholder="选择层" style="width: 110px" @change="generateGrid">
          <el-option v-for="l in layerOptions" :key="l" :label="`第${l}层`" :value="l" />
        </el-select>
      </div>
      <div class="toolbar-right">
        <div class="legend">
          <span class="legend-item"><span class="legend-color" style="background: #67c23a" />空闲 ({{ statusCount.free }})</span>
          <span class="legend-item"><span class="legend-color" style="background: #409eff" />占用 ({{ statusCount.occupied }})</span>
          <span class="legend-item"><span class="legend-color" style="background: #e6a23c" />锁定 ({{ statusCount.locked }})</span>
          <span class="legend-item"><span class="legend-color" style="background: #f56c6c" />维护 ({{ statusCount.maintenance }})</span>
        </div>
      </div>
    </div>

    <!-- 库位图主体 -->
    <div class="location-map-page__body">
      <div class="bin-grid-wrapper">
        <div class="row-labels">
          <span v-for="r in rowCount" :key="r" class="row-label">{{ String(r).padStart(2, '0') }}排</span>
        </div>
        <div class="bin-grid">
          <div class="col-labels">
            <span v-for="c in colCount" :key="c" class="col-label">{{ String(c).padStart(2, '0') }}列</span>
          </div>
          <div class="grid-container" :style="{ gridTemplateColumns: `repeat(${colCount}, 1fr)` }">
            <el-popover
              v-for="(bin, idx) in gridData"
              :key="idx"
              placement="top"
              :width="280"
              trigger="click"
              :offset="8"
              :hide-after="0"
            >
              <template #reference>
                <div class="bin-cell" :class="`bin-cell--${bin.status}`" @click.stop>
                  <div class="bin-cell__code">{{ bin.code }}</div>
                  <div class="bin-cell__status">{{ bin.statusText }}</div>
                </div>
              </template>
              <div class="bin-popover">
                <div class="bin-popover__title">库位详情</div>
                <el-descriptions :column="1" size="small" border>
                  <el-descriptions-item label="库位编码">{{ bin.code }}</el-descriptions-item>
                  <el-descriptions-item label="位置">{{ bin.row }}排-{{ bin.col }}列-{{ bin.layer }}层</el-descriptions-item>
                  <el-descriptions-item label="库位类型">{{ bin.binType }}</el-descriptions-item>
                  <el-descriptions-item label="状态">
                    <el-tag :type="statusTagType[bin.status]" size="small">{{ bin.statusText }}</el-tag>
                  </el-descriptions-item>
                  <el-descriptions-item v-if="bin.materialCode" label="物料编码">{{ bin.materialCode }}</el-descriptions-item>
                  <el-descriptions-item v-if="bin.materialName" label="物料名称">{{ bin.materialName }}</el-descriptions-item>
                  <el-descriptions-item v-if="bin.quantity > 0" label="存放数量">{{ bin.quantity }}</el-descriptions-item>
                </el-descriptions>
                <div class="bin-popover__actions">
                  <el-button v-if="bin.status === 'free'" type="warning" size="small" @click="handleLock">锁定</el-button>
                  <el-button v-if="bin.status === 'locked'" type="success" size="small" @click="handleUnlock">解锁</el-button>
                </div>
              </div>
            </el-popover>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const statusTagType = { free: 'success', occupied: '', locked: 'warning', maintenance: 'danger' }

const selectedWarehouse = ref('WH-A')
const selectedZone = ref('A-01')
const selectedLayer = ref(1)

const rowCount = 10
const colCount = 12

// 库区选项
const zoneOptionsMap = {
  'WH-A': [
    { label: 'A-01 原材料存储区', value: 'A-01' },
    { label: 'A-02 成品拣选区', value: 'A-02' },
    { label: 'A-03 暂存区', value: 'A-03' },
    { label: 'A-04 退货区', value: 'A-04' },
  ],
  'WH-B': [
    { label: 'B-01 半成品存储区', value: 'B-01' },
    { label: 'B-02 出库拣选区', value: 'B-02' },
    { label: 'B-03 退货暂存区', value: 'B-03' },
  ],
  'WH-C': [
    { label: 'C-01 冷藏存储区', value: 'C-01' },
    { label: 'C-02 冷冻存储区', value: 'C-02' },
  ],
}

// 库区对应的层数配置
const zoneLayerMap = {
  'A-01': 5, 'A-02': 3, 'A-03': 2, 'A-04': 2,
  'B-01': 3, 'B-02': 2, 'B-03': 2,
  'C-01': 4, 'C-02': 4,
}

const zoneOptions = computed(() => zoneOptionsMap[selectedWarehouse.value] || [])
const layerOptions = computed(() => {
  const total = zoneLayerMap[selectedZone.value] || 1
  return Array.from({ length: total }, (_, i) => i + 1)
})

const handleWarehouseChange = () => {
  const opts = zoneOptionsMap[selectedWarehouse.value]
  if (opts?.length) selectedZone.value = opts[0].value
  selectedLayer.value = 1
  generateGrid()
}

// 生成网格
const statusTextMap = { free: '空闲', occupied: '占用', locked: '锁定', maintenance: '维护' }
const binTypeList = ['大型', '中型', '小型']
const materialList = [
  { code: 'MAT-0001', name: '电阻器 10K' },
  { code: 'MAT-0005', name: 'STM32F103 芯片' },
  { code: 'MAT-0015', name: 'PCB主板 A型' },
  { code: 'MAT-0018', name: '不锈钢螺栓 M6' },
  { code: 'MAT-0002', name: '电容器 100uF' },
  { code: 'MAT-0008', name: 'LED灯珠 白光' },
]

const gridData = ref([])

const generateGrid = () => {
  const prefix = selectedZone.value.split('-')[0]
  const layer = selectedLayer.value
  const data = []
  for (let r = 1; r <= rowCount; r++) {
    for (let c = 1; c <= colCount; c++) {
      const seed = (r * 13 + c * 7 + prefix.charCodeAt(0) + layer * 3) % 20
      let status
      if (seed < 8) status = 'free'
      else if (seed < 14) status = 'occupied'
      else if (seed < 17) status = 'locked'
      else status = 'maintenance'

      const mat = status === 'occupied' ? materialList[(r + c + layer) % materialList.length] : null
      data.push({
        code: `${prefix}-${String(r).padStart(2, '0')}-${String(c).padStart(2, '0')}-${String(layer).padStart(2, '0')}`,
        row: r, col: c, layer,
        binType: binTypeList[(r + c) % 3],
        status,
        statusText: statusTextMap[status],
        materialCode: mat?.code || null,
        materialName: mat?.name || null,
        quantity: status === 'occupied' ? (r * 37 + c * 23 + layer) % 500 + 10 : 0,
      })
    }
  }
  gridData.value = data
}

const statusCount = computed(() => {
  const bins = gridData.value
  return {
    free: bins.filter(b => b.status === 'free').length,
    occupied: bins.filter(b => b.status === 'occupied').length,
    locked: bins.filter(b => b.status === 'locked').length,
    maintenance: bins.filter(b => b.status === 'maintenance').length,
  }
})

const handleLock = () => KhMessageFn.success('已锁定')
const handleUnlock = () => KhMessageFn.success('已解锁')

onMounted(() => generateGrid())
</script>

<style scoped>
.location-map-page {
  display: flex;
  flex-direction: column;
  gap: 12px;
  flex: 1;
  min-height: 0;
  overflow: hidden;
}

.location-map-page__toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #fff;
  border-radius: 8px;
  padding: 12px 20px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
  flex-shrink: 0;
}
.toolbar-left {
  display: flex;
  align-items: center;
  gap: 12px;
}
.toolbar-right {
  display: flex;
  align-items: center;
}
.legend {
  display: flex;
  gap: 16px;
  align-items: center;
}
.legend-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: #606266;
}
.legend-color {
  display: inline-block;
  width: 14px;
  height: 14px;
  border-radius: 3px;
}

.location-map-page__body {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
  flex: 1;
  min-height: 0;
  overflow: hidden;
}

.bin-grid-wrapper {
  display: flex;
  gap: 8px;
  height: 100%;
}
.row-labels {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding-top: 28px;
  flex-shrink: 0;
}
.row-label {
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  font-size: 12px;
  color: #909399;
  padding-right: 4px;
  min-width: 36px;
}
.bin-grid {
  flex: 1;
  overflow: auto;
  min-width: 0;
}
.col-labels {
  display: flex;
  gap: 6px;
  margin-bottom: 4px;
}
.col-label {
  flex: 1;
  min-width: 56px;
  text-align: center;
  font-size: 12px;
  color: #909399;
  padding-bottom: 4px;
}
.grid-container {
  display: grid;
  gap: 6px;
}

.bin-cell {
  min-width: 56px;
  height: 48px;
  border-radius: 6px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s ease;
  border: 2px solid transparent;
  user-select: none;
}
.bin-cell:hover {
  transform: scale(1.08);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  z-index: 1;
}
.bin-cell__code {
  font-size: 11px;
  font-weight: 600;
  color: #fff;
  line-height: 1.2;
}
.bin-cell__status {
  font-size: 10px;
  color: rgba(255, 255, 255, 0.85);
  margin-top: 2px;
}

.bin-cell--free { background: #67c23a; border-color: #5daf34; }
.bin-cell--occupied { background: #409eff; border-color: #337ecc; }
.bin-cell--locked { background: #e6a23c; border-color: #cf9236; }
.bin-cell--maintenance { background: #f56c6c; border-color: #dd6161; }
.bin-cell--free:hover { border-color: #85ce61; }
.bin-cell--occupied:hover { border-color: #66b1ff; }
.bin-cell--locked:hover { border-color: #ebb563; }
.bin-cell--maintenance:hover { border-color: #f78989; }

/* 气泡内容样式 */
.bin-popover__title {
  font-size: 14px;
  font-weight: 600;
  margin-bottom: 10px;
  color: #303133;
}
.bin-popover__actions {
  margin-top: 10px;
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}
</style>
