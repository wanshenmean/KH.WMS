<template>
  <div class="license-page">
    <!-- 左侧：品牌 + 授权状态 -->
    <div class="license-left">
      <div class="license-left__grid" />
      <div class="license-left__glow" />

      <div class="license-left__content">
        <div class="license-left__top">
          <svg viewBox="0 0 36 36" fill="none" class="license-left__logo">
            <rect x="1" y="1" width="34" height="34" rx="9" fill="#3B82F6" fill-opacity="0.1" stroke="#3B82F6"
              stroke-width="1.2" stroke-opacity="0.25" />
            <path d="M18 7l9 4v6c0 5.5-3.8 9.7-9 11-5.2-1.3-9-5.5-9-11v-6l9-4z" fill="#3B82F6" fill-opacity="0.5"
              stroke="#3B82F6" stroke-width="1" stroke-opacity="0.6" />
            <path d="M14 18.5l2.8 2.8L21.5 16" stroke="#fff" stroke-width="1.7" stroke-linecap="round"
              stroke-linejoin="round" />
          </svg>
          <div class="license-left__text">
            <h1 class="license-left__title">WMS</h1>
            <p class="license-left__subtitle">系统授权管理</p>
          </div>
        </div>

        <!-- 授权状态卡片 -->
        <div class="license-status">
          <div class="license-status__head">
            <span class="license-status__label">授权状态</span>
            <el-tag :type="statusTagType" effect="light" round>{{ statusText }}</el-tag>
          </div>

          <div class="license-status__row">
            <span class="license-status__k">机器码</span>
            <div class="license-status__code">
              <code>{{ machineCode || '获取中…' }}</code>
              <el-button v-if="machineCode" link size="small" type="primary" @click="copyMachineCode">复制</el-button>
            </div>
          </div>

          <template v-if="licenseInfo && licenseInfo.isValid">
            <div class="license-status__row">
              <span class="license-status__k">到期时间</span>
              <span class="license-status__v">{{ formatDateTime(licenseInfo.expiresAt) }}</span>
            </div>
            <div class="license-status__row">
              <span class="license-status__k">剩余天数</span>
              <span class="license-status__v" :class="{ 'is-warning': Number(licenseInfo.remainingDays) <= 7 }">
                {{ licenseInfo.remainingDays }} 天
              </span>
            </div>
            <div class="license-status__row">
              <span class="license-status__k">授权类型</span>
              <span class="license-status__v">{{ licenseInfo.licenseType || '-' }}</span>
            </div>
          </template>

          <div v-else-if="licenseInfo && licenseInfo.reason" class="license-status__reason">
            <el-alert :title="licenseInfo.reason" type="error" :closable="false" show-icon />
          </div>
        </div>

        <p class="license-left__tip">
          请将上方「机器码」提供给软件厂商，获取 <b>.lic</b> 授权文件后在右侧导入。
        </p>
      </div>
    </div>

    <!-- 右侧：导入授权 -->
    <div class="license-right">
      <div class="license-right__inner">
        <h2 class="license-right__title">导入授权文件</h2>
        <p class="license-right__desc">上传厂商签发的 <b>.lic</b> 文件以激活系统</p>

        <el-upload
          ref="uploadRef"
          class="license-uploader"
          drag
          :show-file-list="false"
          :auto-upload="true"
          :before-upload="beforeUpload"
          :http-request="handleUpload"
          accept=".lic,.txt"
        >
          <div class="license-uploader__inner">
            <svg viewBox="0 0 24 24" fill="none" class="license-uploader__icon">
              <path d="M12 16V4m0 0L8 8m4-4l4 4" stroke="#3B82F6" stroke-width="1.6" stroke-linecap="round"
                stroke-linejoin="round" />
              <path d="M4 16v2a2 2 0 002 2h12a2 2 0 002-2v-2" stroke="#3B82F6" stroke-width="1.6"
                stroke-linecap="round" stroke-opacity="0.6" />
            </svg>
            <p class="license-uploader__text">将 .lic 文件拖到此处，或<em>点击上传</em></p>
            <p class="license-uploader__hint">仅支持厂商签发的 .lic 授权文件</p>
          </div>
        </el-upload>

        <button type="button" class="license-right__btn" :disabled="uploading" @click="triggerPick">
          <span v-if="uploading" class="license-right__btn-loading">
            <svg class="license-right__btn-spinner" viewBox="0 0 24 24" fill="none">
              <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2.5" stroke-dasharray="31.4 31.4"
                stroke-linecap="round" />
            </svg>
          </span>
          <span>{{ uploading ? '导入中' : '选择授权文件' }}</span>
        </button>

        <p v-if="licenseInfo && licenseInfo.isValid" class="license-right__hint">
          系统已授权，<el-button link type="primary" @click="goBack">返回系统</el-button>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { getLicenseInfo, getMachineCode, importLicense } from '@/api/license'

const router = useRouter()

const machineCode = ref('')
const licenseInfo = ref(null)
const uploading = ref(false)
const uploadRef = ref(null)

// 注：后端启用 BoolToIntConverter，bool 字段(isValid/isExpired)序列化为 0/1，按真值判断即可
const statusText = computed(() => {
  if (!licenseInfo.value) return '检测中'
  return licenseInfo.value.isValid ? '已授权' : '未授权'
})
const statusTagType = computed(() => {
  if (!licenseInfo.value) return 'info'
  return licenseInfo.value.isValid ? 'success' : 'danger'
})

function formatDateTime(v) {
  if (!v) return '-'
  const d = new Date(v)
  if (isNaN(d.getTime())) return String(v)
  const pad = (n) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

async function loadStatus() {
  try {
    const [infoRes, codeRes] = await Promise.all([getLicenseInfo(), getMachineCode()])
    if (infoRes && infoRes.code === 200) {
      licenseInfo.value = infoRes.data
    } else {
      licenseInfo.value = { isValid: false, reason: infoRes?.message || '授权信息获取失败' }
    }
    if (codeRes && codeRes.code === 200) {
      machineCode.value = codeRes.data?.machineCode || ''
    }
  } catch {
    licenseInfo.value = { isValid: false, reason: '授权信息获取失败，请检查后端服务是否正常' }
  }
}

async function copyMachineCode() {
  if (!machineCode.value) return
  try {
    await navigator.clipboard.writeText(machineCode.value)
    KhMessageFn.success('机器码已复制')
  } catch {
    KhMessageFn.error('复制失败，请手动选择复制')
  }
}

function beforeUpload(file) {
  if (!/\.(lic|txt)$/i.test(file.name)) {
    KhMessageFn.error('请上传 .lic 授权文件')
    return false
  }
  if (file.size > 20 * 1024) {
    KhMessageFn.error('授权文件过大，请确认文件正确')
    return false
  }
  return true
}

async function handleUpload(options) {
  const { file } = options
  uploading.value = true
  try {
    const formData = new FormData()
    formData.append('file', file)
    const res = await importLicense(formData)
    if (res && res.code === 200) {
      KhMessageFn.success(res.message || '授权导入成功')
      await loadStatus()
      if (licenseInfo.value && licenseInfo.value.isValid) {
        const redirect = router.currentRoute.value.query.redirect
        router.replace(typeof redirect === 'string' && redirect ? redirect : '/')
      }
    } else {
      KhMessageFn.error(res?.message || '授权导入失败')
    }
  } catch {
    KhMessageFn.error('授权导入失败，请稍后重试')
  } finally {
    uploading.value = false
  }
}

// 「选择授权文件」按钮触发 el-upload 的文件选择
function triggerPick() {
  uploadRef.value?.$el?.click()
}

function goBack() {
  const redirect = router.currentRoute.value.query.redirect
  router.replace(typeof redirect === 'string' && redirect ? redirect : '/')
}

onMounted(() => {
  loadStatus()
})
</script>

<style scoped>
.license-page {
  display: flex;
  height: 100%;
  min-height: 600px;
}

/* ========== 左侧 ========== */
.license-left {
  flex: 5;
  background: linear-gradient(135deg, #EFF6FF 0%, #DBEAFE 50%, #BFDBFE 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 60px;
  position: relative;
  overflow: hidden;
}

.license-left__grid {
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(rgba(59, 130, 246, 0.04) 1px, transparent 1px),
    linear-gradient(90deg, rgba(59, 130, 246, 0.04) 1px, transparent 1px);
  background-size: 48px 48px;
}

.license-left__glow {
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

.license-left__content {
  position: relative;
  z-index: 1;
  width: 100%;
  max-width: 480px;
}

.license-left__top {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 36px;
}

.license-left__logo {
  width: 40px;
  height: 40px;
  flex-shrink: 0;
}

.license-left__title {
  font-size: 32px;
  font-weight: 800;
  color: #1E40AF;
  margin: 0;
  letter-spacing: 4px;
}

.license-left__subtitle {
  font-size: 13px;
  color: #60A5FA;
  margin: 2px 0 0;
  letter-spacing: 2px;
}

/* 状态卡片 */
.license-status {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(59, 130, 246, 0.12);
  border-radius: 14px;
  padding: 22px 24px;
  box-shadow: 0 4px 18px rgba(30, 64, 175, 0.06);
}

.license-status__head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 18px;
}

.license-status__label {
  font-size: 14px;
  font-weight: 600;
  color: #1E40AF;
}

.license-status__row {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 0;
  border-top: 1px dashed rgba(59, 130, 246, 0.12);
}

.license-status__row:first-of-type {
  border-top: none;
}

.license-status__k {
  width: 72px;
  flex-shrink: 0;
  font-size: 13px;
  color: #64748B;
}

.license-status__v {
  font-size: 13px;
  color: #1E293B;
  font-weight: 500;
}

.license-status__v.is-warning {
  color: #F59E0B;
  font-weight: 700;
}

.license-status__code {
  display: flex;
  align-items: center;
  gap: 8px;
  flex: 1;
  min-width: 0;
}

.license-status__code code {
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
  font-size: 12px;
  color: #1E40AF;
  background: rgba(59, 130, 246, 0.08);
  padding: 4px 8px;
  border-radius: 6px;
  word-break: break-all;
  flex: 1;
  min-width: 0;
}

.license-status__reason {
  margin-top: 14px;
}

.license-left__tip {
  margin: 18px 2px 0;
  font-size: 12px;
  color: #64748B;
  line-height: 1.7;
}

.license-left__tip b {
  color: #1E40AF;
}

/* ========== 右侧 ========== */
.license-right {
  flex: 4;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #fff;
  padding: 40px;
}

.license-right__inner {
  width: 100%;
  max-width: 380px;
}

.license-right__title {
  font-size: 26px;
  font-weight: 700;
  color: #1E293B;
  margin: 0 0 6px;
}

.license-right__desc {
  font-size: 14px;
  color: #94A3B8;
  margin: 0 0 32px;
}

.license-right__desc b {
  color: #475569;
}

/* 上传区 */
.license-uploader {
  width: 100%;
}

.license-uploader :deep(.el-upload),
.license-uploader :deep(.el-upload-dragger) {
  width: 100%;
}

.license-uploader :deep(.el-upload-dragger) {
  border-radius: 12px;
  border: 1.5px dashed #CBD5E1;
  background: #F8FAFC;
  padding: 28px 16px;
  transition: all 0.2s;
}

.license-uploader :deep(.el-upload-dragger:hover) {
  border-color: #3B82F6;
  background: #EFF6FF;
}

.license-uploader__inner {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
}

.license-uploader__icon {
  width: 40px;
  height: 40px;
}

.license-uploader__text {
  margin: 6px 0 0;
  font-size: 13px;
  color: #475569;
}

.license-uploader__text em {
  font-style: normal;
  color: #3B82F6;
  font-weight: 600;
}

.license-uploader__hint {
  margin: 0;
  font-size: 12px;
  color: #CBD5E1;
}

/* 按钮 */
.license-right__btn {
  width: 100%;
  height: 46px;
  margin-top: 20px;
  border: none;
  border-radius: 10px;
  background: linear-gradient(135deg, #2563EB, #3B82F6);
  color: #fff;
  font-size: 15px;
  font-weight: 600;
  letter-spacing: 4px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: all 0.2s;
  box-shadow: 0 2px 8px rgba(37, 99, 235, 0.2);
}

.license-right__btn:hover {
  box-shadow: 0 4px 16px rgba(37, 99, 235, 0.3);
  transform: translateY(-1px);
}

.license-right__btn:disabled {
  cursor: not-allowed;
  opacity: 0.85;
}

.license-right__btn-loading {
  display: flex;
  align-items: center;
}

.license-right__btn-spinner {
  width: 18px;
  height: 18px;
  animation: license-spin 0.8s linear infinite;
}

@keyframes license-spin {
  to {
    transform: rotate(360deg);
  }
}

.license-right__hint {
  text-align: center;
  font-size: 12px;
  color: #94A3B8;
  margin-top: 24px;
}

/* ========== 响应式 ========== */
@media (max-width: 860px) {
  .license-left {
    display: none;
  }

  .license-right {
    background: #fff;
  }
}
</style>
