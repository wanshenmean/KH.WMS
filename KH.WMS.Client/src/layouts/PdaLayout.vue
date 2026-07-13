<template>
  <div class="pda-layout">
    <!-- 顶部固定栏 -->
    <header class="pda-layout__header">
      <h1 class="pda-layout__title">WMS PDA</h1>
      <el-button
        class="pda-layout__back-btn"
        :icon="ArrowLeftIcon"
        circle
        size="small"
        @click="goBack"
      />
    </header>

    <!-- 滚动内容区域 -->
    <main class="pda-layout__content">
      <router-view />
    </main>

    <!-- 底部固定导航栏 -->
    <nav class="pda-layout__tabbar">
      <div
        v-for="tab in tabBarList"
        :key="tab.path"
        class="pda-layout__tab-item"
        :class="{ 'is-active': isActive(tab.path) }"
        @click="navigateTo(tab.path)"
      >
        <el-icon :size="22">
          <component :is="tab.icon" />
        </el-icon>
        <span class="pda-layout__tab-label">{{ tab.label }}</span>
      </div>
    </nav>
  </div>
</template>

<script setup>

/** markRaw 包裹图标组件，避免被 Vue 响应式代理产生警告 */
const ArrowLeftIcon = markRaw(ArrowLeft)
const IconBox = markRaw(Box)
const IconTopRight = markRaw(TopRight)
const IconList = markRaw(List)
const IconSort = markRaw(Sort)
const IconEditPen = markRaw(EditPen)

const router = useRouter()
const route = useRoute()

/** 底部标签页配置 */
const tabBarList = [
  { label: '收货', path: '/pda/receiving', icon: IconBox },
  { label: '上架', path: '/pda/putaway', icon: IconTopRight },
  { label: '拣货', path: '/pda/picking', icon: IconList },
  { label: '分拣', path: '/pda/sorting', icon: IconSort },
  { label: '盘点', path: '/pda/count', icon: IconEditPen },
]

/** 判断当前标签页是否激活 */
const isActive = (path) => {
  return route.path.startsWith(path)
}

/** 导航到指定路径 */
const navigateTo = (path) => {
  if (route.path !== path) {
    router.push(path)
  }
}

/** 返回上一页 */
const goBack = () => {
  if (window.history.length > 1) {
    router.back()
  } else {
    router.push('/pda/receiving')
  }
}
</script>

<style scoped>
.pda-layout {
  display: flex;
  flex-direction: column;
  height: 100vh;
  max-width: 480px;
  margin: 0 auto;
  background-color: #f5f7fa;
  position: relative;
  overflow: hidden;
}

/* ---- 顶部栏 ---- */
.pda-layout__header {
  position: fixed;
  top: 0;
  left: 50%;
  transform: translateX(-50%);
  width: 100%;
  max-width: 480px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 12px;
  background-color: #409eff;
  color: #fff;
  z-index: 100;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.pda-layout__title {
  font-size: 18px;
  font-weight: 600;
  margin: 0;
  letter-spacing: 1px;
}

.pda-layout__back-btn {
  --el-button-bg-color: rgba(255, 255, 255, 0.2);
  --el-button-border-color: rgba(255, 255, 255, 0.3);
  --el-button-text-color: #fff;
  --el-button-hover-bg-color: rgba(255, 255, 255, 0.3);
  --el-button-hover-border-color: rgba(255, 255, 255, 0.4);
  --el-button-hover-text-color: #fff;
  min-width: 44px;
  min-height: 44px;
}

/* ---- 内容区 ---- */
.pda-layout__content {
  flex: 1;
  margin-top: 48px;
  margin-bottom: 50px;
  overflow-y: auto;
  -webkit-overflow-scrolling: touch;
  padding: 12px;
}

/* ---- 底部导航栏 ---- */
.pda-layout__tabbar {
  position: fixed;
  bottom: 0;
  left: 50%;
  transform: translateX(-50%);
  width: 100%;
  max-width: 480px;
  height: 50px;
  display: flex;
  align-items: center;
  justify-content: space-around;
  background-color: #fff;
  border-top: 1px solid #e5e6eb;
  z-index: 100;
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.06);
}

.pda-layout__tab-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
  min-height: 44px;
  cursor: pointer;
  color: #909399;
  transition: color 0.2s;
  user-select: none;
  -webkit-tap-highlight-color: transparent;
}

.pda-layout__tab-item.is-active {
  color: #409eff;
}

.pda-layout__tab-label {
  font-size: 11px;
  margin-top: 2px;
  line-height: 1;
}

.pda-layout__tab-item.is-active .pda-layout__tab-label {
  font-weight: 600;
}
</style>
