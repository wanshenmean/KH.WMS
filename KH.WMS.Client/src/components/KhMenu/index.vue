<template>
  <el-scrollbar class="kh-menu__scrollbar">
    <!-- 搜索框（折叠态隐藏） -->
    <div v-show="!collapse" class="kh-menu__search">
      <el-input v-model="searchKeyword" placeholder="搜索菜单" clearable
        :prefix-icon="SearchIcon" size="small" class="kh-menu__search-input" />
    </div>
    <el-menu
      :default-active="activeMenu"
      :default-openeds="defaultOpeneds"
      :collapse="collapse"
      :collapse-transition="false"
      :background-color="menuBgColor"
      :text-color="menuTextColor"
      :active-text-color="menuActiveColor"
      :unique-opened="uniqueOpened"
      :router="menuRouter"
      v-bind="$attrs"
      @select="handleSelect"
    >
      <template v-for="item in filteredMenuList" :key="item.path || item.index">
        <!-- 有子菜单 -->
        <el-sub-menu
          v-if="item.children?.length"
          :index="item.path || item.index"
        >
          <template #title>
            <el-icon v-if="item.icon"><component :is="item.icon" /></el-icon>
            <span>{{ item.title }}</span>
          </template>
          <el-menu-item
            v-for="child in item.children"
            :key="child.path || child.index"
            :index="child.path || child.index"
          >
            <el-icon v-if="child.icon"><component :is="child.icon" /></el-icon>
            <template #title>{{ child.title }}</template>
          </el-menu-item>
        </el-sub-menu>

        <!-- 无子菜单 -->
        <el-menu-item v-else :index="item.path || item.index">
          <el-icon v-if="item.icon"><component :is="item.icon" /></el-icon>
          <template #title>{{ item.title }}</template>
        </el-menu-item>
      </template>
    </el-menu>
  </el-scrollbar>
</template>

<script setup>

const SearchIcon = Search

const props = defineProps({
  /** 菜单数据列表 */
  menuList: { type: Array, default: () => [] },
  /** 是否折叠 */
  collapse: { type: Boolean, default: true },
  /** 是否使用 vue-router 路由模式 */
  menuRouter: { type: Boolean, default: true },
  /** 非 router 模式下手动控制当前激活菜单项 */
  activeIndex: { type: String, default: '' },
  /** 是否只展开一个子菜单 */
  uniqueOpened: { type: Boolean, default: true },
  /** 菜单背景颜色 */
  menuBgColor: { type: String, default: '#f5f7fa' },
  /** 菜单文字颜色 */
  menuTextColor: { type: String, default: '#646a73' },
  /** 菜单激活项文字颜色 */
  menuActiveColor: { type: String, default: '#2d8cf0' },
})

const emit = defineEmits(['select'])

const route = useRoute()

/** 搜索关键字 */
const searchKeyword = ref('')

/** 根据搜索关键字过滤后的菜单列表 */
const filteredMenuList = computed(() => {
  const keyword = searchKeyword.value.trim().toLowerCase()
  if (!keyword) return props.menuList
  return props.menuList.filter((item) => {
    if (item.children?.length) {
      const matchedChildren = item.children.filter(
        (child) => child.title.toLowerCase().includes(keyword)
      )
      // 父级标题也匹配，或子项有匹配的
      if (item.title.toLowerCase().includes(keyword)) return true
      return matchedChildren.length > 0
    }
    return item.title.toLowerCase().includes(keyword)
  }).map((item) => {
    if (!item.children?.length) return item
    // 子项有匹配时，只返回匹配的子项
    const keyword2 = searchKeyword.value.trim().toLowerCase()
    if (item.title.toLowerCase().includes(keyword2)) return item
    return {
      ...item,
      children: item.children.filter(
        (child) => child.title.toLowerCase().includes(keyword2)
      ),
    }
  })
})

const activeMenu = computed(() => {
  if (!props.menuRouter && props.activeIndex) return props.activeIndex
  return route.path
})

const defaultOpeneds = computed(() => {
  const opens = []
  // 搜索时展开所有有子菜单的项
  if (searchKeyword.value.trim()) {
    filteredMenuList.value.forEach((item) => {
      if (item.children?.length) opens.push(item.path || item.index)
    })
    return opens
  }
  // 非搜索时，只展开包含当前激活项的子菜单
  filteredMenuList.value.forEach((item) => {
    if (item.children?.length) {
      const isCurrent = item.children.some(
        (child) => (child.path || child.index) === activeMenu.value
      )
      if (isCurrent) {
        opens.push(item.path || item.index)
      }
    }
  })
  return opens
})

const handleSelect = (index) => {
  emit('select', index)
}

defineExpose({
  /** 当前激活菜单 index */
  activeMenu,
})
</script>

<style scoped>
.kh-menu__scrollbar {
  height: 100%;
}

.kh-menu__search {
  padding: 10px 12px 8px;
  flex-shrink: 0;
}

.kh-menu__search-input :deep(.el-input__wrapper) {
  border-radius: 18px;
  box-shadow: 0 0 0 1px #dcdfe6 inset;
  background-color: #fff;
  padding: 0 14px;
}

.kh-menu__search-input :deep(.el-input__wrapper:hover) {
  box-shadow: 0 0 0 1px #c0c4cc inset;
}

.kh-menu__search-input :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 1px #2d8cf0 inset;
}

.kh-menu__search-input :deep(.el-input__prefix .el-icon) {
  color: #c0c4cc;
  font-size: 14px;
}

.kh-menu__search-input :deep(.el-input__inner) {
  font-size: 13px;
}

.kh-menu__search-input :deep(.el-input__suffix .el-icon) {
  color: #c0c4cc;
}

.kh-menu__scrollbar :deep(.el-menu) {
  border-right: none;
}

/* 菜单项 */
.kh-menu__scrollbar :deep(.el-menu-item) {
  margin: 0;
  border-radius: 0;
  transition: all 0.2s ease;
  height: 44px;
  line-height: 44px;
  border-left: 3px solid transparent;
}

.kh-menu__scrollbar :deep(.el-menu-item:hover) {
  background-color: rgba(0, 0, 0, 0.04) !important;
  color: #2d8cf0 !important;
}

.kh-menu__scrollbar :deep(.el-menu-item.is-active) {
  background-color: #e8f3ff !important;
  color: #2d8cf0 !important;
  border-left-color: #2d8cf0;
  font-weight: 500;
}

.kh-menu__scrollbar :deep(.el-menu-item .el-icon) {
  font-size: 18px;
  vertical-align: middle;
  margin-right: 8px;
  color: #8c8c8c;
}

.kh-menu__scrollbar :deep(.el-menu-item.is-active .el-icon) {
  color: #2d8cf0;
}

.kh-menu__scrollbar :deep(.el-menu-item:hover .el-icon) {
  color: #2d8cf0;
}

/* 子菜单标题 */
.kh-menu__scrollbar :deep(.el-sub-menu__title) {
  margin: 0;
  border-radius: 0;
  transition: all 0.2s ease;
  height: 44px;
  line-height: 44px;
  color: #333 !important;
}

.kh-menu__scrollbar :deep(.el-sub-menu__title:hover) {
  background-color: rgba(0, 0, 0, 0.04) !important;
}

.kh-menu__scrollbar :deep(.el-sub-menu__title .el-icon) {
  font-size: 18px;
  vertical-align: middle;
  margin-right: 8px;
  color: #8c8c8c;
}

.kh-menu__scrollbar :deep(.el-sub-menu .el-menu-item) {
  padding-left: 52px !important;
  min-width: auto;
  height: 40px;
  line-height: 40px;
  background-color: rgba(255, 255, 255, 0.5) !important;
}

.kh-menu__scrollbar :deep(.el-sub-menu .el-menu-item:hover) {
  background-color: #e8f3ff !important;
}

/* 子菜单展开箭头 */
.kh-menu__scrollbar :deep(.el-sub-menu__icon-arrow) {
  font-size: 12px;
  transition: transform 0.25s;
  color: #8c8c8c;
}

/* 折叠态 */
.kh-menu__scrollbar :deep(.el-menu--collapse .el-menu-item),
.kh-menu__scrollbar :deep(.el-menu--collapse .el-sub-menu__title) {
  margin: 0;
  border-radius: 0;
}
</style>
