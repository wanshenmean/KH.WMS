<template>
  <el-popover
    placement="bottom-end"
    :width="380"
    trigger="click"
    popper-class="kh-notification__popper"
    @show="handlePopoverShow"
  >
    <template #reference>
      <div class="kh-notification__trigger">
        <el-badge :value="unreadCount" :hidden="unreadCount === 0" :max="99" v-bind="badgeProps">
          <el-icon :size="18"><Bell /></el-icon>
        </el-badge>
      </div>
    </template>

    <div class="kh-notification__panel">
      <!-- 头部 -->
      <div class="kh-notification__header">
        <span class="kh-notification__title">消息通知</span>
        <el-button v-if="unreadCount > 0" link type="primary" size="small" @click="handleReadAll">
          全部已读
        </el-button>
      </div>

      <!-- 消息列表 -->
      <div class="kh-notification__list">
        <template v-if="messages.length > 0">
          <div
            v-for="(msg, index) in messages"
            :key="index"
            class="kh-notification__item"
            :class="{ 'is-unread': !msg.read }"
            @click="handleClick(msg)"
          >
            <div class="kh-notification__dot" :class="`is-${msg.type || 'info'}`" />
            <div class="kh-notification__body">
              <div class="kh-notification__msg-title">{{ msg.title }}</div>
              <div class="kh-notification__msg-content">{{ msg.content }}</div>
              <div class="kh-notification__msg-time">{{ msg.time }}</div>
            </div>
          </div>
        </template>
        <div v-else class="kh-notification__empty">
          <el-icon :size="32" color="#c0c4cc"><BellFilled /></el-icon>
          <span>{{ emptyText }}</span>
        </div>
      </div>

      <!-- 底部 -->
      <div v-if="$slots.extra" class="kh-notification__footer">
        <slot name="extra" />
      </div>
    </div>
  </el-popover>
</template>

<script setup>

const props = defineProps({
  /** 消息列表 */
  messages: { type: Array, default: () => [] },
  /** 空数据文字 */
  emptyText: { type: String, default: '暂无消息' },
  /** el-badge 额外属性 */
  badgeProps: { type: Object, default: () => ({}) },
})

const emit = defineEmits(['read', 'read-all', 'click'])

const unreadCount = computed(() => {
  return props.messages.filter(m => !m.read).length
})

const handleClick = (msg) => {
  if (!msg.read) {
    msg.read = true
    emit('read', msg)
  }
  emit('click', msg)
}

const handleReadAll = () => {
  props.messages.forEach(m => { m.read = true })
  emit('read-all')
}

const handlePopoverShow = () => {
  // 打开面板时不自动标记已读，由用户点击或点击"全部已读"
}

defineExpose({ unreadCount })
</script>

<style scoped>
.kh-notification__trigger {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 6px;
  cursor: pointer;
  color: #606266;
  transition: all 0.2s;
}

.kh-notification__trigger:hover {
  background-color: #f0f2f5;
  color: #409eff;
}

.kh-notification__panel {
  margin: -12px;
}

.kh-notification__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 16px 10px;
  border-bottom: 1px solid #f0f2f5;
}

.kh-notification__title {
  font-size: 15px;
  font-weight: 600;
  color: #1d2129;
}

.kh-notification__list {
  max-height: 340px;
  overflow-y: auto;
}

.kh-notification__item {
  display: flex;
  gap: 10px;
  padding: 12px 16px;
  cursor: pointer;
  transition: background-color 0.15s;
}

.kh-notification__item:hover {
  background-color: #f7f8fa;
}

.kh-notification__item.is-unread {
  background-color: #f0f7ff;
}

.kh-notification__item.is-unread:hover {
  background-color: #e8f3ff;
}

.kh-notification__dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
  margin-top: 6px;
}

.kh-notification__dot.is-info { background-color: #409eff; }
.kh-notification__dot.is-success { background-color: #67c23a; }
.kh-notification__dot.is-warning { background-color: #e6a23c; }
.kh-notification__dot.is-error { background-color: #f56c6c; }

.kh-notification__body {
  flex: 1;
  min-width: 0;
}

.kh-notification__msg-title {
  font-size: 14px;
  font-weight: 500;
  color: #1d2129;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.kh-notification__msg-content {
  font-size: 12px;
  color: #86909c;
  margin-top: 3px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.kh-notification__msg-time {
  font-size: 11px;
  color: #c0c4cc;
  margin-top: 4px;
}

.kh-notification__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 40px 0;
  color: #c0c4cc;
  font-size: 13px;
}

.kh-notification__footer {
  padding: 10px 16px;
  border-top: 1px solid #f0f2f5;
  text-align: center;
}
</style>
