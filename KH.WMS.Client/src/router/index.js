import { createRouter, createWebHistory } from 'vue-router'
import { usePermissionStore } from '@/stores/permission'
import { useUserStore } from '@/stores/user'

/** 已注册的动态路由集合，用于去重和清理 */
const dynamicRouteNames = new Set()

/**
 * 预加载所有 views 下的页面组件（排除 components 子目录）
 * 使用相对路径避免 Vite 在含非 ASCII 字符的 Windows 路径下生成无法解析的绝对路径
 */
const allViewModules = import.meta.glob('../views/**/*.vue')

/**
 * 过滤后的页面组件映射，排除 components 子目录下的文件
 * key 格式如 "../views/system/user.vue"
 */
const viewModules = Object.fromEntries(
  Object.entries(allViewModules).filter(([key]) => !key.includes('/components/'))
)

/**
 * 根据后端 component 字段精确匹配前端组件
 * 后端格式如 "system/user"，对应前端文件 "../views/system/user.vue"
 */
function resolveComponent(backendComponent) {
  if (!backendComponent) return null
  const componentPath = `../views/${backendComponent}.vue`
  return viewModules[componentPath] || null
}

/**
 * 根据后端菜单数据动态生成路由配置
 * 已有实际组件的页面使用真实组件，没有的则使用占位组件
 */
function generateDynamicRoutes(dynamicRoutes) {
  return dynamicRoutes.map(route => {
    const component = resolveComponent(route.component)
      || (() => import('@/views/PlaceholderView.vue'))
    return {
      path: route.path,
      name: `dynamic-${route.permissionCode}`,
      component,
      meta: {
        title: route.permissionName,
        permission: route.permissionCode,
      },
    }
  })
}

/**
 * 注册动态路由到 router
 */
function addDynamicRoutes(router, routes) {
  // 先清理旧的动态路由
  removeDynamicRoutes(router)
  // 注册新的动态路由
  for (const route of routes) {
    router.addRoute('Layout', route)
    dynamicRouteNames.add(route.name)
  }
}

/**
 * 移除所有已注册的动态路由
 */
function removeDynamicRoutes(router) {
  for (const name of dynamicRouteNames) {
    if (router.hasRoute(name)) {
      router.removeRoute(name)
    }
  }
  dynamicRouteNames.clear()
}

const routes = [
  { path: '/login', component: () => import('@/views/LoginView.vue'), meta: { title: '登录', public: true } },
  // 授权管理页（公开：license 失效时无需登录即可访问，用于查看机器码 / 导入 .lic 恢复）
  { path: '/license', name: 'license', component: () => import('@/views/LicenseView.vue'), meta: { title: '系统授权', public: true } },
  {
    path: '/',
    name: 'Layout',
    component: () => import('@/layouts/PcLayout.vue'),
    redirect: '/home',
    meta: { requiresAuth: true },
    children: [
      { path: 'home', name: 'Home', component: () => import('@/views/HomeView.vue'), meta: { title: '首页' } },
    ]
  },
  {
    path: '/pda',
    component: () => import('@/layouts/PdaLayout.vue'),
    redirect: '/pda/receiving',
    meta: { requiresAuth: true },
    children: [
      { path: 'receiving', component: () => import('@/views/pda/PdaReceiving.vue'), meta: { title: 'PDA收货', permission: 'pda:receiving' } },
      { path: 'putaway', component: () => import('@/views/pda/PdaPutaway.vue'), meta: { title: 'PDA上架', permission: 'pda:putaway' } },
      { path: 'picking', component: () => import('@/views/pda/PdaPicking.vue'), meta: { title: 'PDA拣货', permission: 'pda:picking' } },
      { path: 'sorting', component: () => import('@/views/pda/PdaSorting.vue'), meta: { title: 'PDA分拣', permission: 'pda:sorting' } },
      { path: 'count', component: () => import('@/views/pda/PdaCount.vue'), meta: { title: 'PDA盘点', permission: 'pda:count' } },
    ]
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

const publicPaths = ['/login', '/license']

router.beforeEach(async (to, from, next) => {
  // 1. 设置页面标题
  document.title = to.meta.title ? `${to.meta.title} - WMS` : 'WMS'

  // 2. 判断是否为公开页面
  const isPublic = to.meta.public || publicPaths.includes(to.path)

  // 3. 检查 token
  const token = localStorage.getItem('token')

  if (!token && !isPublic) {
    return next({ path: '/login', query: { redirect: to.fullPath } })
  }

  if (token && to.path === '/login') {
    return next({ path: '/' })
  }

  // 4. 已登录的受保护路由：确保用户信息和权限已加载
  if (token && !isPublic) {
    const userStore = useUserStore()
    const permissionStore = usePermissionStore()

    // 页面刷新场景：重新获取用户信息
    if (!userStore.userInfo.id) {
      try {
        await userStore.fetchUserInfo()
      } catch (err) {
        userStore.clearAuth()
        return next({ path: '/login', query: { redirect: to.fullPath } })
      }
    }

    // 首次加载权限，并注册动态路由
    if (!permissionStore.permissionsLoaded) {
      try {
        await permissionStore.fetchPermissions(userStore.userInfo.roleId)
        // 根据后端菜单数据注册动态路由
        const dynamicRoutes = generateDynamicRoutes(permissionStore.dynamicRoutes)
        addDynamicRoutes(router, dynamicRoutes)
        // 动态路由注册后需要重新导航
        return next({ ...to, replace: true })
      } catch (err) {
        console.error('加载权限失败:', err)
        userStore.clearAuth()
        return next({ path: '/login', query: { redirect: to.fullPath } })
      }
    }

    // 5. 路由级权限校验
    if (to.meta.permission && !permissionStore.hasRoutePermission(to.meta.permission)) {
      console.warn(`[Router] 无权限访问: ${to.path} (需要: ${to.meta.permission})`)
      return next({ path: '/home' })
    }
  }

  next()
})

export { removeDynamicRoutes }
export default router
