<template>
  <div class="kh-page-header">
    <div class="kh-page-header__left">
      <div v-if="showBack" class="kh-page-header__back" @click="handleBack">
        <el-icon :size="18">
          <ArrowLeft />
        </el-icon>
      </div>
      <div class="kh-page-header__info">
        <div class="kh-page-header__title-row">
          <h2 class="kh-page-header__title">{{ title }}</h2>
          <slot name="extra" />
        </div>
        <p v-if="subtitle" class="kh-page-header__subtitle">{{ subtitle }}</p>
        <el-breadcrumb v-if="breadcrumb ? breadcrumb.length : false" separator="/" class="kh-page-header__breadcrumb">
          <el-breadcrumb-item v-for="(crumb, i) in breadcrumb" :key="i" :to="crumb.to">
            {{ crumb.title }}
          </el-breadcrumb-item>
        </el-breadcrumb>
      </div>
    </div>
    <div v-if="$slots.content" class="kh-page-header__content">
      <slot name="content" />
    </div>
  </div>
</template>
<script setup>
const props = defineProps({
  title: { type: String, default: '' },
  subtitle: { type: String, default: '' },
  breadcrumb: { type: Array, default: () => [] },
  showBack: { type: Boolean, default: true },
})
const emit = defineEmits(['back'])
const handleBack = () => emit('back')
</script>
<style scoped>
.kh-page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
  margin-bottom: 12px;
}

.kh-page-header__left {
  display: flex;
  align-items: center;
  gap: 12px;
  min-width: 0;
}

.kh-page-header__back {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: 6px;
  cursor: pointer;
  color: #606266;
  transition: all 0.2s;
  flex-shrink: 0;
}

.kh-page-header__back:hover {
  background: #f0f2f5;
  color: #409eff;
}

.kh-page-header__title {
  font-size: 16px;
  font-weight: 600;
  color: #1d2129;
  margin: 0;
  line-height: 1.4;
}

.kh-page-header__subtitle {
  font-size: 13px;
  color: #86909c;
  margin: 4px 0 0;
}

.kh-page-header__breadcrumb {
  margin-top: 6px;
}

.kh-page-header__content {
  flex-shrink: 0;
  margin-left: 16px;
}
</style>
