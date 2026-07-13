<template>
  <KhLayout
    :menu-list="menuList"
    :menu-router="true"
    :show-tabs="true"
    home-key="/home"
    title="WMS"
  >
    <!-- 顶部栏右侧：全屏、通知、用户下拉 -->
    <template #header-right>
      <KhFullscreen />

      <KhNotification :messages="wsStore.notifications" />

      <el-dropdown trigger="click" @command="handleUserCommand">
        <div class="pc-layout__user">
          <el-avatar :size="30" :icon="UserFilledIcon" />
          <span class="pc-layout__username">{{ userStore.userDisplayName }}</span>
          <el-icon><ArrowDown /></el-icon>
        </div>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item command="profile">
              <el-icon><User /></el-icon>个人中心
            </el-dropdown-item>
            <el-dropdown-item divided command="logout">
              <el-icon><SwitchButton /></el-icon>退出登录
            </el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
    </template>

    <!-- 主内容区域 -->
    <router-view />
  </KhLayout>
</template>

<script setup>
import KhLayout from '@/components/KhLayout/index.vue'
import KhFullscreen from '@/components/KhFullscreen/index.vue'
import KhNotification from '@/components/KhNotification/index.vue'
import { useUserStore } from '@/stores/user.js'
import { usePermissionStore } from '@/stores/permission.js'
import { useWebSocketStore } from '@/stores/websocket.js'
import { useDictStore } from '@/stores/dict.js'

const UserFilledIcon = markRaw(UserFilled)

const router = useRouter()
const userStore = useUserStore()
const permissionStore = usePermissionStore()
const wsStore = useWebSocketStore()
const dictStore = useDictStore()

/** 使用后端动态菜单数据 */
const menuList = computed(() => {
  return permissionStore.dynamicMenuList
})

/** 初始化 WebSocket 连接 */
onMounted(() => {
  wsStore.initConnections()
})

/**
 * 处理用户下拉菜单命令
 * @param {'profile'|'logout'} command - 菜单命令
 */
const handleUserCommand = async (command) => {
  if (command === 'profile') {
    router.push('/profile')
  } else if (command === 'logout') {
    wsStore.closeAll()
    permissionStore.clearPermissions()
    dictStore.clearDict()
    await userStore.logout()
    router.push('/login')
  }
}
</script>

<style scoped>
.pc-layout__user {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 6px;
  transition: background-color 0.2s;
}

.pc-layout__user:hover {
  background-color: #f0f2f5;
}

.pc-layout__username {
  font-size: 14px;
  color: #333;
  max-width: 100px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
