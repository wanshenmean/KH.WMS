<template>
  <div class="login-page">
    <!-- 左侧 -->
    <div class="login-left">
      <!-- 微妙网格纹理 -->
      <div class="login-left__grid" />
      <!-- 柔光装饰 -->
      <div class="login-left__glow" />

      <div class="login-left__content">
        <div class="login-left__top">
          <svg viewBox="0 0 36 36" fill="none" class="login-left__logo">
            <rect x="1" y="1" width="34" height="34" rx="9" fill="#3B82F6" fill-opacity="0.1" stroke="#3B82F6"
              stroke-width="1.2" stroke-opacity="0.25" />
            <rect x="7" y="11" width="7" height="5.5" rx="1.2" fill="#3B82F6" fill-opacity="0.7" />
            <rect x="16" y="7" width="13" height="5.5" rx="1.2" fill="#3B82F6" fill-opacity="0.45" />
            <rect x="7" y="19.5" width="13" height="5.5" rx="1.2" fill="#3B82F6" fill-opacity="0.55" />
            <rect x="22" y="19.5" width="7" height="5.5" rx="1.2" fill="#3B82F6" fill-opacity="0.35" />
          </svg>
          <div class="login-left__text">
            <h1 class="login-left__title">WMS</h1>
            <p class="login-left__subtitle">智慧仓储管理系统</p>
          </div>
        </div>

        <!-- 仓储场景插画 -->
        <div class="login-left__scene">
          <svg viewBox="0 0 520 280" fill="none" class="login-left__scene-svg">
            <!-- 地面 -->
            <line x1="20" y1="230" x2="500" y2="230" stroke="#3B82F6" stroke-width="0.6" stroke-opacity="0.15" />

            <!-- 左仓库 -->
            <rect x="24" y="110" width="110" height="120" rx="5" fill="#3B82F6" fill-opacity="0.04" stroke="#3B82F6"
              stroke-width="0.8" stroke-opacity="0.12" />
            <rect x="24" y="102" width="110" height="16" rx="5" fill="#3B82F6" fill-opacity="0.07" stroke="#3B82F6"
              stroke-width="0.8" stroke-opacity="0.15" />
            <rect x="38" y="128" width="26" height="86" rx="1.5" fill="#3B82F6" fill-opacity="0.05" stroke="#3B82F6"
              stroke-width="0.6" stroke-opacity="0.18" />
            <rect x="40" y="135" width="22" height="8" rx="1" fill="#3B82F6" fill-opacity="0.22" />
            <rect x="40" y="149" width="22" height="8" rx="1" fill="#3B82F6" fill-opacity="0.35" />
            <rect x="40" y="163" width="22" height="8" rx="1" fill="#3B82F6" fill-opacity="0.18" />
            <rect x="40" y="177" width="22" height="8" rx="1" fill="#3B82F6" fill-opacity="0.3" />
            <rect x="40" y="191" width="22" height="8" rx="1" fill="#3B82F6" fill-opacity="0.15" />

            <rect x="72" y="128" width="26" height="86" rx="1.5" fill="#60A5FA" fill-opacity="0.05" stroke="#60A5FA"
              stroke-width="0.6" stroke-opacity="0.18" />
            <rect x="74" y="135" width="22" height="8" rx="1" fill="#60A5FA" fill-opacity="0.18" />
            <rect x="74" y="149" width="22" height="8" rx="1" fill="#60A5FA" fill-opacity="0.3" />
            <rect x="74" y="163" width="22" height="8" rx="1" fill="#60A5FA" fill-opacity="0.22" />
            <rect x="74" y="177" width="22" height="8" rx="1" fill="#60A5FA" fill-opacity="0.35" />
            <rect x="74" y="191" width="22" height="8" rx="1" fill="#60A5FA" fill-opacity="0.15" />

            <rect x="106" y="128" width="20" height="86" rx="1.5" fill="#3B82F6" fill-opacity="0.03" stroke="#3B82F6"
              stroke-width="0.6" stroke-opacity="0.1" />
            <rect x="90" y="185" width="14" height="45" rx="1.5" fill="#3B82F6" fill-opacity="0.04" stroke="#3B82F6"
              stroke-width="0.6" stroke-opacity="0.1" />

            <!-- 中间立体库 -->
            <rect x="168" y="50" width="160" height="180" rx="6" fill="#3B82F6" fill-opacity="0.04" stroke="#3B82F6"
              stroke-width="1" stroke-opacity="0.12" />
            <rect x="168" y="42" width="160" height="16" rx="6" fill="#3B82F6" fill-opacity="0.07" stroke="#3B82F6"
              stroke-width="1" stroke-opacity="0.15" />
            <rect x="182" y="66" width="20" height="150" rx="1.5" fill="#3B82F6" fill-opacity="0.04" stroke="#3B82F6"
              stroke-width="0.5" stroke-opacity="0.16" />
            <rect x="184" y="73" width="16" height="7" rx="1" fill="#3B82F6" fill-opacity="0.28" />
            <rect x="184" y="85" width="16" height="7" rx="1" fill="#3B82F6" fill-opacity="0.16" />
            <rect x="184" y="97" width="16" height="7" rx="1" fill="#3B82F6" fill-opacity="0.35" />
            <rect x="184" y="109" width="16" height="7" rx="1" fill="#3B82F6" fill-opacity="0.22" />
            <rect x="184" y="121" width="16" height="7" rx="1" fill="#3B82F6" fill-opacity="0.32" />
            <rect x="184" y="133" width="16" height="7" rx="1" fill="#3B82F6" fill-opacity="0.14" />
            <rect x="184" y="145" width="16" height="7" rx="1" fill="#3B82F6" fill-opacity="0.25" />
            <rect x="184" y="157" width="16" height="7" rx="1" fill="#3B82F6" fill-opacity="0.2" />
            <rect x="184" y="169" width="16" height="7" rx="1" fill="#3B82F6" fill-opacity="0.3" />
            <rect x="184" y="181" width="16" height="7" rx="1" fill="#3B82F6" fill-opacity="0.18" />
            <rect x="184" y="193" width="16" height="7" rx="1" fill="#3B82F6" fill-opacity="0.24" />

            <rect x="210" y="66" width="20" height="150" rx="1.5" fill="#F59E0B" fill-opacity="0.04" stroke="#F59E0B"
              stroke-width="0.5" stroke-opacity="0.16" />
            <rect x="212" y="73" width="16" height="7" rx="1" fill="#F59E0B" fill-opacity="0.2" />
            <rect x="212" y="85" width="16" height="7" rx="1" fill="#F59E0B" fill-opacity="0.32" />
            <rect x="212" y="97" width="16" height="7" rx="1" fill="#F59E0B" fill-opacity="0.18" />
            <rect x="212" y="109" width="16" height="7" rx="1" fill="#F59E0B" fill-opacity="0.35" />
            <rect x="212" y="121" width="16" height="7" rx="1" fill="#F59E0B" fill-opacity="0.25" />
            <rect x="212" y="133" width="16" height="7" rx="1" fill="#F59E0B" fill-opacity="0.14" />
            <rect x="212" y="145" width="16" height="7" rx="1" fill="#F59E0B" fill-opacity="0.3" />
            <rect x="212" y="157" width="16" height="7" rx="1" fill="#F59E0B" fill-opacity="0.2" />
            <rect x="212" y="169" width="16" height="7" rx="1" fill="#F59E0B" fill-opacity="0.35" />
            <rect x="212" y="181" width="16" height="7" rx="1" fill="#F59E0B" fill-opacity="0.18" />
            <rect x="212" y="193" width="16" height="7" rx="1" fill="#F59E0B" fill-opacity="0.26" />

            <rect x="238" y="66" width="20" height="150" rx="1.5" fill="#10B981" fill-opacity="0.04" stroke="#10B981"
              stroke-width="0.5" stroke-opacity="0.16" />
            <rect x="240" y="73" width="16" height="7" rx="1" fill="#10B981" fill-opacity="0.28" />
            <rect x="240" y="85" width="16" height="7" rx="1" fill="#10B981" fill-opacity="0.16" />
            <rect x="240" y="97" width="16" height="7" rx="1" fill="#10B981" fill-opacity="0.25" />
            <rect x="240" y="109" width="16" height="7" rx="1" fill="#10B981" fill-opacity="0.35" />
            <rect x="240" y="121" width="16" height="7" rx="1" fill="#10B981" fill-opacity="0.2" />
            <rect x="240" y="133" width="16" height="7" rx="1" fill="#10B981" fill-opacity="0.32" />
            <rect x="240" y="145" width="16" height="7" rx="1" fill="#10B981" fill-opacity="0.14" />
            <rect x="240" y="157" width="16" height="7" rx="1" fill="#10B981" fill-opacity="0.25" />
            <rect x="240" y="169" width="16" height="7" rx="1" fill="#10B981" fill-opacity="0.18" />
            <rect x="240" y="181" width="16" height="7" rx="1" fill="#10B981" fill-opacity="0.28" />
            <rect x="240" y="193" width="16" height="7" rx="1" fill="#10B981" fill-opacity="0.22" />

            <rect x="266" y="66" width="20" height="150" rx="1.5" fill="#EF4444" fill-opacity="0.04" stroke="#EF4444"
              stroke-width="0.5" stroke-opacity="0.16" />
            <rect x="268" y="73" width="16" height="7" rx="1" fill="#EF4444" fill-opacity="0.25" />
            <rect x="268" y="85" width="16" height="7" rx="1" fill="#EF4444" fill-opacity="0.35" />
            <rect x="268" y="97" width="16" height="7" rx="1" fill="#EF4444" fill-opacity="0.14" />
            <rect x="268" y="109" width="16" height="7" rx="1" fill="#EF4444" fill-opacity="0.3" />
            <rect x="268" y="121" width="16" height="7" rx="1" fill="#EF4444" fill-opacity="0.2" />
            <rect x="268" y="133" width="16" height="7" rx="1" fill="#EF4444" fill-opacity="0.32" />
            <rect x="268" y="145" width="16" height="7" rx="1" fill="#EF4444" fill-opacity="0.18" />
            <rect x="268" y="157" width="16" height="7" rx="1" fill="#EF4444" fill-opacity="0.35" />
            <rect x="268" y="169" width="16" height="7" rx="1" fill="#EF4444" fill-opacity="0.22" />
            <rect x="268" y="181" width="16" height="7" rx="1" fill="#EF4444" fill-opacity="0.28" />
            <rect x="268" y="193" width="16" height="7" rx="1" fill="#EF4444" fill-opacity="0.18" />

            <rect x="294" y="66" width="20" height="150" rx="1.5" fill="#60A5FA" fill-opacity="0.04" stroke="#60A5FA"
              stroke-width="0.5" stroke-opacity="0.16" />
            <rect x="296" y="73" width="16" height="7" rx="1" fill="#60A5FA" fill-opacity="0.2" />
            <rect x="296" y="85" width="16" height="7" rx="1" fill="#60A5FA" fill-opacity="0.32" />
            <rect x="296" y="97" width="16" height="7" rx="1" fill="#60A5FA" fill-opacity="0.18" />
            <rect x="296" y="109" width="16" height="7" rx="1" fill="#60A5FA" fill-opacity="0.25" />
            <rect x="296" y="121" width="16" height="7" rx="1" fill="#60A5FA" fill-opacity="0.35" />
            <rect x="296" y="133" width="16" height="7" rx="1" fill="#60A5FA" fill-opacity="0.14" />
            <rect x="296" y="145" width="16" height="7" rx="1" fill="#60A5FA" fill-opacity="0.3" />
            <rect x="296" y="157" width="16" height="7" rx="1" fill="#60A5FA" fill-opacity="0.2" />
            <rect x="296" y="169" width="16" height="7" rx="1" fill="#60A5FA" fill-opacity="0.25" />
            <rect x="296" y="181" width="16" height="7" rx="1" fill="#60A5FA" fill-opacity="0.35" />
            <rect x="296" y="193" width="16" height="7" rx="1" fill="#60A5FA" fill-opacity="0.18" />

            <!-- 堆垛机 -->
            <rect x="202" y="38" width="8" height="16" rx="1.5" fill="#3B82F6" fill-opacity="0.5" />
            <rect x="199" y="37" width="14" height="3" rx="1.5" fill="#3B82F6" fill-opacity="0.4" />
            <rect x="205" y="46" width="3" height="5" rx="1" fill="#3B82F6" fill-opacity="0.3" />

            <!-- 数据面板 -->
            <rect x="360" y="130" width="120" height="100" rx="6" fill="#3B82F6" fill-opacity="0.04" stroke="#3B82F6"
              stroke-width="0.8" stroke-opacity="0.12" />
            <rect x="374" y="145" width="35" height="5" rx="2.5" fill="#3B82F6" fill-opacity="0.1" />
            <rect x="374" y="158" width="92" height="3.5" rx="1.5" fill="#3B82F6" fill-opacity="0.22" />
            <rect x="374" y="167" width="72" height="3.5" rx="1.5" fill="#10B981" fill-opacity="0.22" />
            <rect x="374" y="176" width="56" height="3.5" rx="1.5" fill="#F59E0B" fill-opacity="0.22" />
            <rect x="374" y="185" width="82" height="3.5" rx="1.5" fill="#60A5FA" fill-opacity="0.22" />
            <rect x="374" y="194" width="66" height="3.5" rx="1.5" fill="#EF4444" fill-opacity="0.22" />
            <rect x="360" y="96" width="55" height="26" rx="5" fill="#3B82F6" fill-opacity="0.04" stroke="#3B82F6"
              stroke-width="0.6" stroke-opacity="0.1" />
            <polyline points="369,114 378,105 387,110 401,100" stroke="#10B981" stroke-width="1.2" fill="none"
              stroke-linecap="round" stroke-linejoin="round" stroke-opacity="0.5" />
            <rect x="425" y="96" width="55" height="26" rx="5" fill="#3B82F6" fill-opacity="0.04" stroke="#3B82F6"
              stroke-width="0.6" stroke-opacity="0.1" />
            <polyline points="434,112 443,104 452,108 466,98" stroke="#3B82F6" stroke-width="1.2" fill="none"
              stroke-linecap="round" stroke-linejoin="round" stroke-opacity="0.5" />

            <!-- 叉车 -->
            <g transform="translate(146, 216)">
              <rect x="0" y="3" width="12" height="8" rx="1.5" fill="#F59E0B" fill-opacity="0.4" />
              <rect x="12" y="0" width="7" height="11" rx="1.2" fill="#F59E0B" fill-opacity="0.3" />
              <circle cx="3" cy="13.5" r="2" fill="#3B82F6" fill-opacity="0.2" />
              <circle cx="9" cy="13.5" r="2" fill="#3B82F6" fill-opacity="0.2" />
            </g>
            <path d="M134 222 L164 222" stroke="#3B82F6" stroke-width="0.8" stroke-dasharray="3 2.5" fill="none"
              stroke-opacity="0.15" />
            <path d="M328 170 L356 170" stroke="#3B82F6" stroke-width="0.8" stroke-dasharray="3 2.5" fill="none"
              stroke-opacity="0.15" />
            <rect x="58" y="218" width="16" height="12" rx="1.5" fill="#3B82F6" fill-opacity="0.15" stroke="#3B82F6"
              stroke-width="0.6" stroke-opacity="0.3" />
            <rect x="432" y="214" width="18" height="16" rx="1.5" fill="#10B981" fill-opacity="0.15" stroke="#10B981"
              stroke-width="0.6" stroke-opacity="0.3" />
          </svg>
        </div>

        <!-- 底部特性 -->
        <div class="login-left__features">
          <div class="login-left__feat">
            <svg viewBox="0 0 20 20" fill="none" class="login-left__feat-icon">
              <rect x="3" y="3" width="14" height="14" rx="3" stroke="#3B82F6" stroke-width="1.2"
                stroke-opacity="0.5" />
              <path d="M7 10h6M10 7v6" stroke="#3B82F6" stroke-width="1.2" stroke-linecap="round"
                stroke-opacity="0.5" />
            </svg>
            <span>实时库存监控</span>
          </div>
          <div class="login-left__feat">
            <svg viewBox="0 0 20 20" fill="none" class="login-left__feat-icon">
              <path d="M10 2v16M2 10h16" stroke="#3B82F6" stroke-width="1.2" stroke-linecap="round"
                stroke-opacity="0.5" />
              <circle cx="10" cy="10" r="3" stroke="#3B82F6" stroke-width="1.2" stroke-opacity="0.5" />
            </svg>
            <span>智能调度协同</span>
          </div>
          <div class="login-left__feat">
            <svg viewBox="0 0 20 20" fill="none" class="login-left__feat-icon">
              <path d="M3 10h4l3-5 3 3 4-4 3 6h3" stroke="#3B82F6" stroke-width="1.2" stroke-linecap="round"
                stroke-linejoin="round" fill="none" stroke-opacity="0.5" />
            </svg>
            <span>全链路数据追溯</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 右侧 -->
    <div class="login-right">
      <div class="login-right__inner">
        <h2 class="login-right__title">欢迎回来</h2>
        <p class="login-right__desc">登录您的账户以继续操作</p>

        <el-form ref="loginFormRef" :model="loginForm" :rules="loginRules" label-width="0" size="large"
          class="login-right__form" @keyup.enter="handleLogin">
          <div class="login-right__field">
            <label class="login-right__label">用户名</label>
            <el-input v-model="loginForm.username" placeholder="请输入用户名" :prefix-icon="UserIcon" />
          </div>
          <div class="login-right__field">
            <label class="login-right__label">密码</label>
            <el-input v-model="loginForm.password" type="password" placeholder="请输入密码" :prefix-icon="LockIcon"
              show-password />
          </div>

          <div class="login-right__options">
            <el-checkbox v-model="rememberMe">记住登录</el-checkbox>
          </div>

          <button type="button" class="login-right__btn" :disabled="loading" @click="handleLogin">
            <span v-if="loading" class="login-right__btn-loading">
              <svg class="login-right__btn-spinner" viewBox="0 0 24 24" fill="none">
                <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2.5" stroke-dasharray="31.4 31.4"
                  stroke-linecap="round" />
              </svg>
            </span>
            <span>{{ loading ? '登录中' : '登 录' }}</span>
          </button>
        </el-form>

        <p class="login-right__hint">演示账号 admin，密码任意</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import JSEncrypt from 'jsencrypt'
import { useUserStore } from '@/stores/user'
import { getPublicKey } from '@/api/auth'
const UserIcon = markRaw(User)
const LockIcon = markRaw(Lock)

const router = useRouter()
const userStore = useUserStore()
const loginFormRef = ref(null)
const loading = ref(false)
const rememberMe = ref(false)

// RSA 公钥缓存
let rsaPublicKey = ''

const loadPublicKey = async () => {
  try {
    const res = await getPublicKey()
    if (res.code === 200 && res.data) {
      rsaPublicKey = res.data
    }
  } catch {
    // 获取公钥失败不影响页面渲染，登录时会提示
  }
}

// onMounted(() => {
//   loadPublicKey()
// })

const loginForm = reactive({
  username: '',
  password: '',
})

const loginRules = {
  username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }],
}

const encryptPassword = (password) => {
  if (!rsaPublicKey) return null
  const encrypt = new JSEncrypt()
  encrypt.setPublicKey(rsaPublicKey)
  const encrypted = encrypt.encrypt(password)
  return encrypted || null
}

const handleLogin = async () => {
  const valid = await loginFormRef.value.validate().catch(() => false)
  if (!valid) return

  loading.value = true
  try {
    // 公钥不存在时重新获取
    // if (!rsaPublicKey) {
      await loadPublicKey()
    // }
    const encryptedPassword = encryptPassword(loginForm.password)
    if (!encryptedPassword) {
      KhMessageFn.error('获取加密公钥失败，请稍后重试')
      return
    }

    const res = await userStore.login({
      userName: loginForm.username,
      password: encryptedPassword,
    })

    if (res.code === 200) {
      userStore.setUserInfo({
        id: res.data.userId,
        username: res.data.userName,
        name: res.data.name,
        roleId: res.data.roleId,
        token: res.data.token,
      })

      KhMessageFn.success('登录成功')
      const redirect = router.currentRoute.value.query.redirect || '/home'
      router.push(redirect)
    } else {
      KhMessageFn.error(res.message || '登录失败，请检查用户名和密码')
    }
  } catch (error) {
    KhMessageFn.error(error?.message || '登录失败，请检查用户名和密码')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-page {
  display: flex;
  height: 100%;
  min-height: 600px;
}

/* ========== 左侧 ========== */
.login-left {
  flex: 5;
  background: linear-gradient(135deg, #EFF6FF 0%, #DBEAFE 50%, #BFDBFE 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 60px;
  position: relative;
  overflow: hidden;
}

.login-left__grid {
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(rgba(59, 130, 246, 0.04) 1px, transparent 1px),
    linear-gradient(90deg, rgba(59, 130, 246, 0.04) 1px, transparent 1px);
  background-size: 48px 48px;
}

.login-left__glow {
  position: absolute;
  width: 500px;
  height: 500px;
  border-radius: 50%;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  background: radial-gradient(circle, rgba(59, 130, 246, 0.08) 0%, transparent 70%);
  pointer-events: none;
}

.login-left__content {
  position: relative;
  z-index: 1;
  width: 100%;
  max-width: 540px;
}

.login-left__top {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 48px;
}

.login-left__logo svg {
  width: 40px;
  height: 40px;
  flex-shrink: 0;
}

.login-left__title {
  font-size: 32px;
  font-weight: 800;
  color: #1E40AF;
  margin: 0;
  letter-spacing: 4px;
}

.login-left__subtitle {
  font-size: 13px;
  color: #60A5FA;
  margin: 2px 0 0;
  letter-spacing: 2px;
}

.login-left__scene {
  margin-bottom: 40px;
}

.login-left__scene-svg {
  width: 100%;
  display: block;
}

.login-left__features {
  display: flex;
  gap: 24px;
}

.login-left__feat {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #3B82F6;
  font-size: 13px;
  font-weight: 500;
}

.login-left__feat-icon {
  width: 18px;
  height: 18px;
  flex-shrink: 0;
}

/* ========== 右侧 ========== */
.login-right {
  flex: 4;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #fff;
  padding: 40px;
}

.login-right__inner {
  width: 100%;
  max-width: 380px;
}

.login-right__title {
  font-size: 26px;
  font-weight: 700;
  color: #1E293B;
  margin: 0 0 6px;
}

.login-right__desc {
  font-size: 14px;
  color: #94A3B8;
  margin: 0 0 36px;
}

/* 表单 */
.login-right__form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.login-right__field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.login-right__label {
  font-size: 13px;
  font-weight: 500;
  color: #475569;
}

.login-right__field :deep(.el-input__wrapper) {
  border-radius: 10px;
  padding: 2px 14px;
  height: 44px;
  box-shadow: 0 0 0 1px #E2E8F0;
  background: #F8FAFC;
  transition: all 0.2s;
}

.login-right__field :deep(.el-input__wrapper:hover) {
  box-shadow: 0 0 0 1px #CBD5E1;
  background: #fff;
}

.login-right__field :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.15), 0 0 0 1px #3B82F6;
  background: #fff;
}

.login-right__field :deep(.el-input__inner) {
  font-size: 14px;
  color: #1E293B;
}

.login-right__field :deep(.el-input__prefix .el-icon) {
  color: #93C5FD;
  font-size: 16px;
}

.login-right__field :deep(.el-input__suffix) {
  color: #94A3B8;
}

.login-right__options {
  display: flex;
  align-items: center;
}

.login-right__options :deep(.el-checkbox__label) {
  font-size: 13px;
  color: #64748B;
}

.login-right__options :deep(.el-checkbox__input.is-checked + .el-checkbox__label) {
  color: #3B82F6;
}

/* 登录按钮 */
.login-right__btn {
  width: 100%;
  height: 46px;
  margin-top: 4px;
  border: none;
  border-radius: 10px;
  background: linear-gradient(135deg, #2563EB, #3B82F6);
  color: #fff;
  font-size: 15px;
  font-weight: 600;
  letter-spacing: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: all 0.2s;
  box-shadow: 0 2px 8px rgba(37, 99, 235, 0.2);
  position: relative;
  overflow: hidden;
}

.login-right__btn::before {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.1), transparent);
  opacity: 0;
  transition: opacity 0.2s;
}

.login-right__btn:hover {
  box-shadow: 0 4px 16px rgba(37, 99, 235, 0.3);
  transform: translateY(-1px);
}

.login-right__btn:hover::before {
  opacity: 1;
}

.login-right__btn:active {
  transform: translateY(0);
  box-shadow: 0 2px 6px rgba(37, 99, 235, 0.2);
}

.login-right__btn:disabled {
  cursor: not-allowed;
  opacity: 0.8;
}

.login-right__btn-loading {
  display: flex;
  align-items: center;
}

.login-right__btn-spinner {
  width: 18px;
  height: 18px;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.login-right__hint {
  text-align: center;
  font-size: 12px;
  color: #CBD5E1;
  margin-top: 28px;
}

/* ========== 响应式 ========== */
@media (max-width: 860px) {
  .login-left {
    display: none;
  }

  .login-right {
    background: #fff;
  }
}
</style>
