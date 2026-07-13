<template>
  <el-container class="kh-layout">
    <!-- 侧边栏 -->
    <el-aside :width="isCollapse ? '64px' : asideWidth" class="kh-layout__aside">
      <!-- Logo -->
      <div class="kh-layout__logo">
        <el-icon v-if="rawLogoIcon" :size="28"><component :is="rawLogoIcon" /></el-icon>
        <span v-show="!isCollapse" class="kh-layout__logo-text">{{ title }}</span>
      </div>

      <!-- 菜单 -->
      <KhMenu
        :menu-list="menuList"
        :collapse="isCollapse"
        :menu-router="menuRouter"
        :active-index="activeIndex"
        :unique-opened="uniqueOpened"
        :menu-bg-color="menuBgColor"
        :menu-text-color="menuTextColor"
        :menu-active-color="menuActiveColor"
        @select="handleSelect"
      />
    </el-aside>

    <!-- 右侧主体 -->
    <el-container class="kh-layout__main-container">
      <!-- 顶部栏 -->
      <el-header class="kh-layout__header" :height="headerHeight">
        <div class="kh-layout__header-left">
          <el-icon
            class="kh-layout__collapse-btn"
            @click="toggleCollapse"
          >
            <component :is="isCollapse ? IconExpand : IconFold" />
          </el-icon>
          <!-- 面包屑 -->
          <el-breadcrumb v-if="showBreadcrumb" separator="/">
            <el-breadcrumb-item
              v-for="crumb in breadcrumbs"
              :key="crumb.path"
              :to="crumb.path"
            >
              {{ crumb.title }}
            </el-breadcrumb-item>
          </el-breadcrumb>
        </div>
        <div class="kh-layout__header-right">
          <slot name="header-right" />
        </div>
      </el-header>

      <!-- Tab 标签栏 + 内容区（有 tabs 时包裹为卡片式） -->
      <template v-if="showTabs && tabs.length">
        <div class="kh-layout__tabs">
          <div ref="tabsScrollRef" class="kh-layout__tabs-scroll">
            <div
              v-for="tab in tabs"
              :key="tab.key"
              :data-tab-key="tab.key"
              class="kh-layout__tab-item"
              :class="{ 'is-active': activeTabKey === tab.key }"
              @click="handleTabClick(tab)"
              @contextmenu.prevent="(e) => handleTabContextMenu(e, tab)"
            >
              <span class="kh-layout__tab-title">{{ tab.title }}</span>
              <el-icon
                v-if="tab.closable"
                class="kh-layout__tab-close"
                @click.stop="handleTabClose(tab)"
              >
                <IconClose />
              </el-icon>
            </div>
          </div>
          <!-- 右键菜单 -->
          <Teleport to="body">
            <div
              v-if="contextMenuVisible"
              class="kh-layout__context-menu"
              :style="{ left: contextMenuPos.x + 'px', top: contextMenuPos.y + 'px' }"
            >
              <div class="kh-layout__context-item" @click="handleContextCloseCurrent">关闭当前</div>
              <div class="kh-layout__context-item" @click="handleContextCloseOthers">关闭其他</div>
              <div class="kh-layout__context-item" @click="handleContextCloseAll">关闭所有</div>
            </div>
          </Teleport>
        </div>
        <el-main class="kh-layout__content kh-layout__content--with-tabs">
          <slot />
        </el-main>
      </template>

      <!-- 无 tabs 时直接显示内容区 -->
      <el-main v-else class="kh-layout__content">
        <slot />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup>
/**
 * @component KhLayout
 * @description 布局组件，包含侧边栏菜单、顶部栏（面包屑）、内容区域。
 *               支持路由模式和非路由模式，支持折叠菜单。
 *
 * @example
 * <!-- 路由模式 -->
 * <KhLayout :menu-list="menus" />
 *
 * @example
 * <!-- 非路由模式 -->
 * <KhLayout
 *   :menu-list="menus"
 *   :menu-router="false"
 *   active-index="/home"
 *   @select="handleMenuSelect"
 * />
 *
 * ──────────────────────────────────────────────────────────────
 * Props 属性说明
 * ──────────────────────────────────────────────────────────────
 * @prop {Array<MenuItem>}      menuList         - 菜单数据列表，默认值：空数组 []
 * @prop {string}               asideWidth       - 侧边栏展开时的宽度，默认值：'220px'
 * @prop {string}               headerHeight     - 顶部栏高度，默认值：'56px'
 * @prop {string}               title            - 系统标题，显示在侧边栏 Logo 旁，默认值：'管理系统'
 * @prop {string|Object}        logoIcon         - Logo 图标组件名或组件对象，默认值：'Box'
 * @prop {boolean}              defaultCollapse  - 侧边栏初始折叠状态，默认值：false（展开）
 * @prop {boolean}              uniqueOpened     - 是否只展开一个子菜单，默认值：true
 * @prop {boolean}              menuRouter       - 是否使用 vue-router 路由模式，默认值：true
 * @prop {string}               activeIndex      - 非 router 模式下手动控制当前激活菜单项的 index，默认值：''
 * @prop {boolean}              showBreadcrumb   - 是否显示顶部面包屑导航，默认值：true
 * @prop {string}               menuBgColor      - 菜单背景颜色，默认值：'#304156'
 * @prop {string}               menuTextColor    - 菜单文字颜色，默认值：'#bfcbd9'
 * @prop {string}               menuActiveColor  - 菜单激活项文字颜色，默认值：'#409eff'
 *
 * ──────────────────────────────────────────────────────────────
 * Events 事件说明
 * ──────────────────────────────────────────────────────────────
 * @event {Function} select          - 菜单项被选中时触发
 *   @param {string} index           - 被选中菜单项的 index 值（即 path 或 index）
 *
 * @event {Function} collapse-change - 侧边栏折叠/展开状态变化时触发
 *   @param {boolean} isCollapse     - 当前折叠状态，true 表示已折叠，false 表示已展开
 *
 * ──────────────────────────────────────────────────────────────
 * Expose 暴露的方法和属性
 * ──────────────────────────────────────────────────────────────
 * @expose {Ref<boolean>} isCollapse    - 响应式折叠状态引用，可通过 ref 直接读写
 * @expose {Function}      toggleCollapse - 手动切换侧边栏折叠/展开状态的方法
 *
 * ──────────────────────────────────────────────────────────────
 * Slots 插槽说明
 * ──────────────────────────────────────────────────────────────
 * @slot default       - 主内容区域插槽，用于放置页面主体内容
 * @slot header-right  - 顶部栏右侧插槽，用于放置用户头像、通知等自定义内容
 *
 * ──────────────────────────────────────────────────────────────
 * MenuItem 菜单项数据结构
 * ──────────────────────────────────────────────────────────────
 * @typedef {Object} MenuItem
 * @property {string}            title    - 菜单项显示标题文本
 * @property {string|Object}     [icon]   - 菜单项图标，支持 Element Plus 图标组件名或组件对象，可选
 * @property {string}            [path]   - 菜单项路由路径，在 router 模式下作为路由跳转地址，可选
 * @property {string}            [index]  - 菜单项唯一标识，在非 router 模式下作为激活匹配依据，可选
 * @property {Array<MenuItem>}   [children] - 子菜单列表，存在时渲染为可展开的子菜单组，可选
 *
 * @example
 * const menuList = [
 *   {
 *     title: '系统管理',
 *     icon: 'Setting',
 *     path: '/system',
 *     children: [
 *       { title: '用户管理', icon: 'User', path: '/system/user' },
 *       { title: '角色管理', icon: 'Lock', path: '/system/role' },
 *     ],
 *   },
 *   { title: '首页', icon: 'HomeFilled', path: '/home' },
 * ]
 */

/** markRaw 包裹图标组件，避免被响应式代理 */
const IconFold = markRaw(Fold)
const IconExpand = markRaw(Expand)
const IconClose = markRaw(Close)

// ──────────────────────────────────────────────────────────────
// Props 定义
// ──────────────────────────────────────────────────────────────
const props = defineProps({
  /**
   * 菜单数据列表
   * @type {Array<MenuItem>}
   * @default () => []
   */
  menuList: {
    type: Array,
    default: () => [],
  },

  /**
   * 侧边栏展开时的宽度
   * 折叠时固定为 64px，展开时使用此值
   * @type {string}
   * @default '220px'
   */
  asideWidth: {
    type: String,
    default: '220px',
  },

  /**
   * 顶部栏（Header）的高度
   * @type {string}
   * @default '48px'
   */
  headerHeight: {
    type: String,
    default: '48px',
  },

  /**
   * 系统标题文本，显示在侧边栏 Logo 区域
   * 折叠状态下标题自动隐藏
   * @type {string}
   * @default '管理系统'
   */
  title: {
    type: String,
    default: '管理系统',
  },

  /**
   * Logo 区域的图标
   * 传入 Element Plus 图标组件名称（字符串）或图标组件对象
   * @type {string|Object}
   * @default 'Box'
   */
  logoIcon: {
    type: [String, Object],
    default: 'Box',
  },

  /**
   * 侧边栏初始折叠状态
   * 为 true 时组件加载后侧边栏即处于折叠状态（宽度 64px）
   * @type {boolean}
   * @default false
   */
  defaultCollapse: {
    type: Boolean,
    default: false,
  },

  /**
   * 是否只展开一个子菜单
   * 为 true 时，展开一个子菜单会自动收起其他已展开的子菜单
   * @type {boolean}
   * @default true
   */
  uniqueOpened: {
    type: Boolean,
    default: true,
  },

  /**
   * 是否使用 vue-router 路由模式
   * 为 true 时，点击菜单项将通过 vue-router 进行页面跳转，并根据当前路由自动高亮菜单
   * 为 false 时，菜单项不触发路由跳转，需通过 activeIndex 手动控制激活项，并通过 @select 事件监听点击
   * @type {boolean}
   * @default true
   */
  menuRouter: {
    type: Boolean,
    default: true,
  },

  /**
   * 非 router 模式下手动控制当前激活菜单项
   * 仅在 menuRouter 为 false 时生效，值为菜单项的 path 或 index
   * @type {string}
   * @default ''
   */
  activeIndex: {
    type: String,
    default: '',
  },

  /**
   * 是否显示顶部面包屑导航
   * 路由模式下从路由 matched 信息自动生成面包屑
   * 非路由模式下从菜单树中查找当前激活项生成面包屑
   * @type {boolean}
   * @default true
   */
  showBreadcrumb: {
    type: Boolean,
    default: true,
  },

  /**
   * 菜单的背景颜色
   * @type {string}
   * @default '#f5f7fa'
   */
  menuBgColor: {
    type: String,
    default: '#f5f7fa',
  },

  /**
   * 菜单项的文字颜色
   * @type {string}
   * @default '#646a73'
   */
  menuTextColor: {
    type: String,
    default: '#646a73',
  },

  /**
   * 菜单项被激活（选中）时的文字颜色
   * @type {string}
   * @default '#2d8cf0'
   */
  menuActiveColor: {
    type: String,
    default: '#2d8cf0',
  },

  /**
   * 是否显示 Tab 标签栏
   * @type {boolean}
   * @default false
   */
  showTabs: {
    type: Boolean,
    default: false,
  },

  /**
   * 首页 Tab 的 key（不可关闭），路由模式默认 '/home'，非路由模式取第一个菜单项
   * @type {string}
   * @default '/home'
   */
  homeKey: {
    type: String,
    default: '/home',
  },
})

// ──────────────────────────────────────────────────────────────
// Events 定义
// ──────────────────────────────────────────────────────────────
const emit = defineEmits([
  /**
   * 菜单项被选中时触发
   * 在 router 模式和非 router 模式下均会触发
   * @event select
   * @param {string} index - 被选中菜单项的 index 值（对应菜单项的 path 或 index 字段）
   */
  'select',

  /**
   * 侧边栏折叠/展开状态变化时触发
   * @event collapse-change
   * @param {boolean} isCollapse - 变化后的折叠状态
   */
  'collapse-change',

  /**
   * Tab 切换时触发（非路由模式下使用）
   * @event tab-change
   * @param {string} key - Tab 的 key 值
   */
  'tab-change',

  /**
   * Tab 关闭时触发
   * @event tab-close
   * @param {string} key - 被关闭 Tab 的 key 值
   */
  'tab-close',
])

// ──────────────────────────────────────────────────────────────
// 路由实例与响应式状态
// ──────────────────────────────────────────────────────────────

/** @type {import('vue-router').RouteLocationNormalizedLoaded} 当前路由信息 */
const route = useRoute()
const router = useRouter()

/**
 * 侧边栏折叠状态
 * 根据 defaultCollapse prop 初始化，可通过 toggleCollapse 方法或直接修改此 ref 来切换
 * @type {import('vue').Ref<boolean>}
 */
const isCollapse = ref(props.defaultCollapse)

// ──────────────────────────────────────────────────────────────
// 计算属性
// ──────────────────────────────────────────────────────────────

/** 解包 logoIcon，避免组件对象被响应式代理 */
const rawLogoIcon = computed(() => {
  if (!props.logoIcon) return props.logoIcon
  return typeof props.logoIcon === 'string' ? props.logoIcon : toRaw(props.logoIcon)
})

/**
 * 面包屑导航数据
 * 根据当前模式生成面包屑数组：
 * - router 模式：从 route.matched 中提取带有 meta.title 的路由记录，前缀固定添加"首页"
 * - 非 router 模式：在 menuList 中查找 activeIndex 对应的菜单项，返回其父级和自身的标题
 *
 * 每个面包屑项格式：{ title: string, path: string }
 * @type {import('vue').ComputedRef<Array<{title: string, path: string}>>}
 * @returns {Array<{title: string, path: string}>} 面包屑导航数据数组
 */
const breadcrumbs = computed(() => {
  if (props.menuRouter) {
    // 路由模式：从当前路由的 matched 记录中生成面包屑
    const matched = route.matched.filter((item) => item.meta?.title)
    const crumbs = [{ title: '首页', path: props.homeKey }]
    matched.forEach((item) => {
      crumbs.push({ title: item.meta.title, path: item.path })
    })
    return crumbs
  }
  // 非 router 模式：从菜单树中查找当前激活项，生成面包屑
  const crumbs = []
  for (const group of props.menuList) {
    if (group.children?.length) {
      // 在子菜单中查找匹配项
      for (const child of group.children) {
        const key = child.path || child.index
        if (key === props.activeIndex) {
          crumbs.push({ title: group.title, path: '' })
          crumbs.push({ title: child.title, path: '' })
          return crumbs
        }
      }
    }
    // 在顶级菜单中查找匹配项
    const key = group.path || group.index
    if (key === props.activeIndex) {
      crumbs.push({ title: group.title, path: '' })
      return crumbs
    }
  }
  return crumbs
})

// ──────────────────────────────────────────────────────────────
// 方法
// ──────────────────────────────────────────────────────────────

// ---- Tab 标签页逻辑 ----

/** 已打开的 Tab 列表 */
const tabs = ref([])

/** 当前激活的 Tab key */
const activeTabKey = ref('')

/** Tab 标签栏滚动容器引用 */
const tabsScrollRef = ref(null)

/** 右键菜单状态 */
const contextMenuVisible = ref(false)
const contextMenuPos = ref({ x: 0, y: 0 })
const contextMenuTab = ref(null)

/** 从菜单树中查找菜单项 */
const findMenuItem = (key) => {
  for (const group of props.menuList) {
    if (group.children?.length) {
      for (const child of group.children) {
        if (child.path === key || child.index === key) return child
      }
    }
    if (group.path === key || group.index === key) return group
  }
  return null
}

/** 滚动 Tab 到可视区域 */
const scrollToTab = (key) => {
  nextTick(() => {
    const container = tabsScrollRef.value
    if (!container) return
    const tabEl = container.querySelector(`[data-tab-key="${key}"]`)
    if (!tabEl) return
    const containerWidth = container.clientWidth
    const tabLeft = tabEl.offsetLeft
    const tabWidth = tabEl.offsetWidth
    const targetScroll = tabLeft - containerWidth / 2 + tabWidth / 2
    container.scrollTo({ left: Math.max(0, targetScroll), behavior: 'smooth' })
  })
}

/** 打开 Tab */
const openTab = (key) => {
  const existing = tabs.value.find(t => t.key === key)
  if (existing) {
    activeTabKey.value = key
    scrollToTab(key)
    return
  }
  const menuItem = findMenuItem(key)
  const resolved = router.resolve(key)
  const title = menuItem?.title || resolved.meta?.title || key
  const closable = key !== props.homeKey
  tabs.value.push({ key, title, closable })
  activeTabKey.value = key
  scrollToTab(key)
}

/** 关闭 Tab */
const closeTab = (key) => {
  const index = tabs.value.findIndex(t => t.key === key)
  if (index === -1) return
  const tab = tabs.value[index]
  if (!tab.closable) return

  tabs.value.splice(index, 1)
  emit('tab-close', key)

  // 如果关闭的是当前激活的 Tab，切换到相邻 Tab
  if (activeTabKey.value === key) {
    const nextTab = tabs.value[index] || tabs.value[index - 1]
    if (nextTab) {
      activeTabKey.value = nextTab.key
      if (props.menuRouter) {
        router.push(nextTab.key)
      } else {
        emit('tab-change', nextTab.key)
      }
    }
  }
}

/** 关闭其他 Tab */
const closeOtherTabs = (key) => {
  tabs.value = tabs.value.filter(t => t.key === key || t.key === props.homeKey)
  activeTabKey.value = key
}

/** 关闭所有可关闭的 Tab */
const closeAllTabs = () => {
  tabs.value = tabs.value.filter(t => !t.closable)
  const homeTab = tabs.value.find(t => t.key === props.homeKey)
  activeTabKey.value = homeTab ? homeTab.key : ''
  if (props.menuRouter && homeTab) {
    router.push(homeTab.key)
  }
}

/** 点击 Tab */
const handleTabClick = (tab) => {
  activeTabKey.value = tab.key
  scrollToTab(tab.key)
  if (props.menuRouter) {
    router.push(tab.key)
  } else {
    emit('tab-change', tab.key)
  }
}

/** 关闭按钮点击 */
const handleTabClose = (tab) => {
  closeTab(tab.key)
}

/** 右键菜单 */
const handleTabContextMenu = (e, tab) => {
  contextMenuTab.value = tab
  contextMenuPos.value = { x: e.clientX, y: e.clientY }
  contextMenuVisible.value = true
}

const handleContextCloseCurrent = () => {
  if (contextMenuTab.value) closeTab(contextMenuTab.value.key)
  contextMenuVisible.value = false
}

const handleContextCloseOthers = () => {
  if (contextMenuTab.value) closeOtherTabs(contextMenuTab.value.key)
  contextMenuVisible.value = false
}

const handleContextCloseAll = () => {
  closeAllTabs()
  contextMenuVisible.value = false
}

/** 点击页面其他区域关闭右键菜单 */
const handleDocClick = () => { contextMenuVisible.value = false }
onMounted(() => { if (props.showTabs) document.addEventListener('click', handleDocClick) })
onBeforeUnmount(() => { document.removeEventListener('click', handleDocClick) })

/** 路由模式：监听路由变化自动打开 Tab */
watch(() => route.path, (path) => {
  if (props.showTabs && props.menuRouter && path) {
    openTab(props.homeKey)
    if (path !== props.homeKey) {
      openTab(path)
    }
  }
}, { immediate: true })

// ---- 折叠 & 菜单 ----

/**
 * 切换侧边栏的折叠/展开状态
 * 点击顶部栏的折叠按钮时调用，切换 isCollapse 值并触发 collapse-change 事件
 * @fires collapse-change
 */
function toggleCollapse() {
  isCollapse.value = !isCollapse.value
  emit('collapse-change', isCollapse.value)
}

/**
 * 处理菜单选中事件
 * 当用户点击菜单项时由 el-menu 的 @select 事件触发，向父组件转发选中的 index
 * @param {string} index - 被选中菜单项的 index 值（对应菜单项的 path 或 index 字段）
 * @fires select
 */
function handleSelect(index) {
  if (props.showTabs) openTab(index)
  emit('select', index)
}

// ──────────────────────────────────────────────────────────────
// 暴露给父组件的属性和方法
// ──────────────────────────────────────────────────────────────
defineExpose({
  isCollapse,
  toggleCollapse,
  /** Tab 列表引用 */
  tabs,
  /** 当前激活 Tab key */
  activeTabKey,
  /** 关闭指定 Tab */
  closeTab,
  /** 关闭除指定 Tab 外的其他 Tab */
  closeOtherTabs,
  /** 关闭所有可关闭 Tab */
  closeAllTabs,
})
</script>

<style scoped>
.kh-layout {
  height: 100%;
  width: 100%;
  overflow: hidden;
}

/* ── 侧边栏 ── */
.kh-layout__aside {
  background-color: #f5f7fa;
  transition: width 0.3s;
  overflow: hidden;
  border-right: 1px solid #e8e8e8;
}
.kh-layout__logo {
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  color: #2d8cf0;
  font-size: 16px;
  font-weight: 600;
  white-space: nowrap;
  overflow: hidden;
  border-bottom: 1px solid #e8e8e8;
}
.kh-layout__logo-text {
  overflow: hidden;
  text-overflow: ellipsis;
}

/* ── 主容器 ── */
.kh-layout__main-container {
  display: flex;
  flex-direction: column;
  overflow: hidden;
  background: #f0f2f5;
  min-width: 0;
}

/* ── 顶部栏 ── */
.kh-layout__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  background: #fff;
  border-bottom: 1px solid #e8e8e8;
  z-index: 1;
  flex-shrink: 0;
}
.kh-layout__header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}
.kh-layout__collapse-btn {
  font-size: 18px;
  cursor: pointer;
  color: #646a73;
  transition: color 0.3s;
  padding: 4px;
  border-radius: 4px;
}
.kh-layout__collapse-btn:hover {
  color: #2d8cf0;
  background: rgba(45, 140, 240, 0.06);
}
.kh-layout__header-right {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* ── 内容区 ── */
.kh-layout__content {
  background: #f0f2f5;
  padding: 16px !important;
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.kh-layout__content--with-tabs {
  background: #f0f2f5;
  padding: 12px 16px !important;
}

/* ── Tab 标签栏 ── */
.kh-layout__tabs {
  background: #fff;
  border-bottom: 1px solid #f0f0f0;
  padding: 0 16px;
  user-select: none;
  flex-shrink: 0;
}
.kh-layout__tabs-scroll {
  display: flex;
  gap: 0;
  overflow-x: auto;
  scrollbar-width: none;
  align-items: center;
}
.kh-layout__tabs-scroll::-webkit-scrollbar {
  display: none;
}
.kh-layout__tab-item {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 0 16px;
  font-size: 13px;
  color: #8c8c8c;
  cursor: pointer;
  border: none;
  border-radius: 0;
  white-space: nowrap;
  flex-shrink: 0;
  transition: all 0.2s ease;
  position: relative;
  background: transparent;
  line-height: 36px;
  height: 36px;
}
.kh-layout__tab-item::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 50%;
  width: 0;
  height: 2px;
  background: #2d8cf0;
  transition: all 0.25s ease;
  transform: translateX(-50%);
  border-radius: 1px;
}
.kh-layout__tab-item:hover {
  color: #2d8cf0;
}
.kh-layout__tab-item:hover::after {
  width: 60%;
}
.kh-layout__tab-item.is-active {
  color: #2d8cf0;
  font-weight: 500;
  background: #e8f3ff;
}
.kh-layout__tab-item.is-active::after {
  width: 100%;
}
.kh-layout__tab-item.is-active::before {
  display: none;
}
.kh-layout__tab-title {
  max-width: 120px;
  overflow: hidden;
  text-overflow: ellipsis;
}
.kh-layout__tab-close {
  font-size: 12px;
  border-radius: 50%;
  width: 16px;
  height: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
  color: #c0c4cc;
}
.kh-layout__tab-item:hover .kh-layout__tab-close {
  color: #8c8c8c;
}
.kh-layout__tab-close:hover {
  background: #ff4d4f;
  color: #fff !important;
  transform: scale(1.1);
}

/* ── 右键菜单 ── */
.kh-layout__context-menu {
  position: fixed;
  z-index: 9999;
  background: #fff;
  border: 1px solid #e5e6eb;
  border-radius: 8px;
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.1);
  padding: 6px 0;
  min-width: 140px;
  animation: kh-context-fade-in 0.15s ease;
}
@keyframes kh-context-fade-in {
  from { opacity: 0; transform: scale(0.92); }
  to { opacity: 1; transform: scale(1); }
}
.kh-layout__context-item {
  padding: 8px 20px;
  font-size: 13px;
  color: #4e5969;
  cursor: pointer;
  transition: all 0.15s;
}
.kh-layout__context-item:hover {
  background: #e8f3ff;
  color: #2d8cf0;
}

/* KhMenu 占满侧边栏剩余空间 */
.kh-layout__aside > :deep(.kh-menu__scrollbar) {
  height: calc(100% - 48px);
}
</style>
