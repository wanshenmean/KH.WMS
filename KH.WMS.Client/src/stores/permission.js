import { defineStore } from 'pinia'
import { getUserPermissions as getPermissionsApi } from '@/api/user'

/**
 * 权限匹配工具
 * 支持精确匹配、通配符匹配（system:*）、父级匹配（inbound 匹配 in_order）
 */
function hasPermission(permCode, requiredCode) {
  if (!requiredCode) return true
  if (!permCode) return false

  // 精确匹配
  if (permCode === requiredCode) return true

  // 通配符匹配：'system:*' 匹配 'sys_user'
  if (permCode.endsWith(':*')) {
    const prefix = permCode.slice(0, -1) // 'system:*' -> 'system:'
    return requiredCode.startsWith(prefix)
  }

  // 父级匹配：拥有 'inbound' 权限可访问 'in_order'
  const permParts = permCode.split(':')
  const reqParts = requiredCode.split(':')
  if (reqParts.length > permParts.length) {
    return reqParts.slice(0, permParts.length).join(':') === permCode
  }

  return false
}

/**
 * 从树形权限数据中提取所有权限码
 * @param {Array} tree - API返回的权限树
 * @param {'route'|'button'} type - 提取类型
 * @returns {string[]} 权限码列表
 */
function extractPermissions(tree, type) {
  const result = []
  function walk(nodes) {
    if (!nodes) return
    for (const node of nodes) {
      if (type === 'route' && node.permissionCode) {
        result.push(node.permissionCode)
      }
      if (type === 'button' && node.buttons) {
        for (const btn of node.buttons) {
          if (btn.permKey) {
            result.push(btn.permKey)
          }
        }
      }
      if (node.children?.length) {
        walk(node.children)
      }
    }
  }
  walk(tree)
  return result
}

/**
 * 后端 icon 名称到 Element Plus 图标组件名的映射
 */
const iconMap = {
  home: 'HomeFilled',
  setting: 'Setting',
  user: 'User',
  team: 'UserFilled',
  menu: 'Menu',
  dict: 'Collection',
  log: 'Notebook',
  tool: 'Tools',
  attachment: 'Paperclip',
  database: 'Coin',
  goods: 'Goods',
  category: 'Files',
  unit: 'Stamp',
  supplier: 'OfficeBuilding',
  peoples: 'UserFilled',
  container: 'Box',
  box: 'Box',
  warehouse: 'House',
  apartment: 'Grid',
  guidance: 'Sort',
  location: 'Position',
  link: 'Link',
  position: 'MapLocation',
  connection: 'Connection',
  list: 'List',
  edit: 'Edit',
  switch: 'Switch',
  camera: 'Camera',
  clipboard: 'DocumentCopy',
  bell: 'Bell',
  inbox: 'Download',
  outbox: 'Upload',
  document: 'Document',
  wave: 'Timer',
  allocation: 'Share',
  schedule: 'Clock',
  check: 'CircleCheck',
  plan: 'Tickets',
  warning: 'WarningFilled',
  chart: 'TrendCharts',
  'pie-chart': 'PieChart',
  'bar-chart': 'DataLine',
}

/**
 * 将后端菜单树转换为前端 KhMenu 需要的格式
 * @param {Array} tree - 后端权限树
 * @returns {Array} 前端菜单列表
 */
function buildMenuList(tree) {
  if (!tree) return []
  return tree
    .filter(item => item.isVisible === 1 && item.status === 1)
    .map(item => {
      const menu = {
        title: item.permissionName,
        icon: iconMap[item.icon] || item.icon || '',
        permission: item.permissionCode,
      }
      if (item.menuType === 0 && item.children?.length) {
        // 目录：有子菜单
        menu.index = item.permissionCode
        menu.children = buildMenuList(item.children)
      } else if (item.menuType === 1) {
        // 菜单项：可点击的页面
        menu.path = item.path
      }
      return menu
    })
    .filter(item => {
      // 目录必须有子项，菜单项必须有路径
      return item.children?.length || item.path
    })
}

/**
 * 将后端菜单树转换为动态路由配置
 * @param {Array} tree - 后端权限树
 * @returns {Array} 路由配置列表（仅包含 menuType=1 的叶子节点）
 */
function buildRoutes(tree) {
  const routes = []
  if (!tree) return routes
  function walk(nodes) {
    for (const node of nodes) {
      if (node.menuType === 1 && node.path && node.isVisible === 1 && node.status === 1) {
        routes.push({
          path: node.path,
          component: node.component || null,
          permissionCode: node.permissionCode,
          permissionName: node.permissionName,
          isCache: node.isCache === 1,
        })
      }
      if (node.children?.length) {
        walk(node.children)
      }
    }
  }
  walk(tree)
  return routes
}

export const usePermissionStore = defineStore('permission', {
  state: () => ({
    /** 路由权限码（permissionCode） */
    routePermissions: [],
    /** 按钮权限码（permKey） */
    buttonPermissions: [],
    /** 权限是否已加载 */
    permissionsLoaded: false,
    /** 原始权限树数据 */
    permissionTree: [],
    /** 从后端菜单树生成的动态菜单列表 */
    dynamicMenuList: [],
    /** 从后端菜单树生成的动态路由列表 */
    dynamicRoutes: [],
  }),

  getters: {
    hasRoutePermission: (state) => {
      return (routeCode) => {
        if (!state.permissionsLoaded) return true
        if (state.routePermissions.includes('*')) return true
        return state.routePermissions.some(p => hasPermission(p, routeCode))
      }
    },

    hasButtonPermission: (state) => {
      return (btnCode) => {
        if (!state.permissionsLoaded) return true
        if (state.buttonPermissions.includes('*')) return true
        return state.buttonPermissions.some(p => p === btnCode)
      }
    },
  },

  actions: {
    /** 从 API 获取权限（解析树形结构） */
    async fetchPermissions(roleId) {
      const res = await getPermissionsApi(roleId)
      if (res.code === 200 || res.code === 0) {
        const tree = Array.isArray(res.data) ? res.data : []
        this.permissionTree = tree
        this.routePermissions = extractPermissions(tree, 'route')
        this.buttonPermissions = extractPermissions(tree, 'button')
        this.dynamicMenuList = buildMenuList(tree)
        this.dynamicRoutes = buildRoutes(tree)
        this.permissionsLoaded = true
      }
      else {
        // 非 200（如 403/500）必须抛异常，否则路由守卫因 permissionsLoaded 未设 true
        // 会 next({...to, replace}) 重新导航 → 无限循环调用接口
        throw new Error(res.message || '获取权限失败')
      }
      return res
    },

    /** 根据权限过滤菜单列表（兼容旧菜单格式） */
    filterMenuList(menuList) {
      if (!this.permissionsLoaded || this.routePermissions.includes('*')) {
        return menuList
      }

      var filteredMenuList = menuList
        .map(item => {
          if (item.children?.length) {
            const filteredChildren = this.filterMenuList(item.children)
            return filteredChildren.length > 0
              ? { ...item, children: filteredChildren }
              : null
          }
          const permCode = item.permission || item.path
          if (this.hasRoutePermission(permCode)) {
            return item
          }
          return null
        })
        .filter(Boolean)
      return filteredMenuList;
    },

    /** 清除权限（登出时调用） */
    clearPermissions() {
      this.routePermissions = []
      this.buttonPermissions = []
      this.permissionTree = []
      this.dynamicMenuList = []
      this.dynamicRoutes = []
      this.permissionsLoaded = false
    },
  },
})
